#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2008 NewtonIdeas
 * Distributed under the LGPL licence
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Xml;
using System.Collections;
using System.Reflection;


namespace NI.Winter
{
	/// <summary>
	/// Component Info
	/// </summary>
	public class ComponentInitInfo : IComponentInitInfo
	{
		bool _Singleton = true;
		string _Name = null;
		string _Parent = null;
		Type _Type = null;
		bool _LazyInit = false;
		string _Description = null;
		IValueInitInfo[] _ConstructorArgs = null;
		IPropertyInitInfo[] _Properties = null;
		string _InitMethod = null;
		
		bool ValuesInitialized = false;
		
		/// <summary>
		/// Singleton flag. True by default.
		/// </summary>
		public bool Singleton { get { return _Singleton; } }

		/// <summary>
		/// Lazy init flag. False by default.
		/// </summary>
		public bool LazyInit { get { return _LazyInit; } }
	
		/// <summary>
		/// Component name (alias).
		/// </summary>
		public string Name { get { return _Name; } }

		/// <summary>
		/// Component parent name (alias).
		/// </summary>
		public string Parent { get { return _Parent; } }

		/// <summary>
		/// Initialization method name. Null by default.
		/// </summary>
		public string InitMethod { get { return _InitMethod; } }
		
		/// <summary>
		/// Component System.Type
		/// </summary>
		public Type ComponentType { get { return _Type; } }
		
		/// <summary>
		/// Component description. Null by default.
		/// </summary>
		public string Description { get { return _Description; } }
	
		/// <summary>
		/// Constructor arguments.
		/// </summary>
		public IValueInitInfo[] ConstructorArgs { 
			get {
				if (!ValuesInitialized)
					throw new Exception(
						String.Format("Values for ComponentInitInfo (name={0},type={1}) are not initialized yet.", Name, ComponentType) );
				return _ConstructorArgs; 
			}
		}
		
		/// <summary>
		/// Properies to set
		/// </summary>
		public IPropertyInitInfo[] Properties { 
			get {
				if (!ValuesInitialized)
					throw new Exception(
						String.Format("Values for ComponentInitInfo (name={0},type={1}) are not initialized yet.", Name, ComponentType));
				return _Properties; 
			} 
		}
		
		public ComponentInitInfo(XmlNode componentNode, IComponentsConfig components)
		{
			// extract component name (optional)
			if (componentNode.Attributes["name"]!=null)
				_Name = componentNode.Attributes["name"].Value;

			// extract component parent (optional)
			if (componentNode.Attributes["parent"]!=null)
				_Parent = componentNode.Attributes["parent"].Value;
			
			// extract component type (optional)
			if (componentNode.Attributes["type"]!=null) {
				_Type = ResolveType( componentNode.Attributes["type"].Value );
				if (ComponentType==null)
					throw new System.Configuration.ConfigurationException("Cannot resolve type "+componentNode.Attributes["type"].Value );
			}
			
			// extract component lazy init flag
			_LazyInit = components.DefaultLazyInit;
			if (componentNode.Attributes["lazy-init"]!=null)
				_LazyInit = Convert.ToBoolean( componentNode.Attributes["lazy-init"].Value );
			
			// extract component singleton flag
			if (componentNode.Attributes["singleton"]!=null)
				_Singleton = Convert.ToBoolean( componentNode.Attributes["singleton"].Value );
			
			// extract description value
			XmlNode descriptionNode = componentNode.SelectSingleNode("description");
			if (descriptionNode!=null) _Description = descriptionNode.InnerText;
			
			// extract init-method value
			if (componentNode.Attributes["init-method"]!=null)
				_InitMethod = componentNode.Attributes["init-method"].Value;
			
			
			
		}
		
		/// <summary>
		/// Initialize component values (constructor arguments / properties )
		/// </summary>
		public void InitValues(XmlNode componentNode, IComponentsConfig components) {
			if (ValuesInitialized) return;
			ValuesInitialized = true;
			
			Hashtable constructorArgs = new Hashtable();
			Hashtable properties = new Hashtable();
			
			// parent exists?
			if (Parent!=null) {
				IComponentInitInfo parentComponentInfo = components[Parent];
				if (parentComponentInfo==null)
					throw new Exception("Cannot find parent component with name='"+Parent+"'" );
				
				// copy all property-definitions
				foreach (IPropertyInitInfo propInfo in parentComponentInfo.Properties)
					properties[propInfo.Name] = propInfo;
				// copy all constructor-arg-definitions
				for (int i=0; i<parentComponentInfo.ConstructorArgs.Length; i++)
					constructorArgs[i] = parentComponentInfo.ConstructorArgs[i];
				// copy type attribute (if not set)
				if (_Type==null)
					_Type = parentComponentInfo.ComponentType;
				// copy init method attribute (if not set)
				if (_InitMethod==null)
					_InitMethod = parentComponentInfo.InitMethod;
			}
			
			// extract constructor arguments
			XmlNodeList constructorArgNodes = componentNode.SelectNodes("constructor-arg");
			foreach (XmlNode constructorArgNode in constructorArgNodes) {
				int index = Convert.ToInt32( constructorArgNode.Attributes["index"].Value );
				try {
					constructorArgs[index] = ResolveValueInfo( constructorArgNode, components );
				} catch (Exception ex) {
					throw new Exception(
						String.Format("Cannot resolve value for constructor arg #{0}", index), ex );
				}
			}
			// compose final constructor args list
			// 1) find greatest index
			int maxConstructorArgIndex = -1;
			foreach (object idx in constructorArgs.Keys)
				if ( (int)idx>maxConstructorArgIndex ) 
					maxConstructorArgIndex = (int)idx;
			// 2) create constructor args array
			_ConstructorArgs = new IValueInitInfo[maxConstructorArgIndex+1];
			// 3) initialize constructor args array
			foreach (object idx in constructorArgs.Keys)
				_ConstructorArgs[ (int)idx ] = (IValueInitInfo)constructorArgs[idx];
			

			// extract properies
			XmlNodeList propertyNodes = componentNode.SelectNodes("property");
			foreach (XmlNode propertyNode in propertyNodes) {
				try {
					PropertyInfo pInfo = new PropertyInfo( 
						propertyNode.Attributes["name"].Value,
						ResolveValueInfo( propertyNode, components) );
					properties[ pInfo.Name ] = pInfo;
				} catch (Exception ex) {
					throw new Exception(
						String.Format("Cannot resolve value for property '{0}'", propertyNode.Attributes["name"].Value), ex );
				}
			}
			// compose final properties list
			_Properties = new IPropertyInitInfo[properties.Count];
			int propertyIdx = 0;
			foreach (IPropertyInitInfo propInfo in properties.Values)
				_Properties[propertyIdx++] = propInfo;
				
		}

		

		
		
		/// <summary>
		/// Resolve object instance by its definition in config
		/// </summary>
		protected IValueInitInfo ResolveValueInfo(XmlNode objectDefinition, IComponentsConfig components) {
			
			// component definition ?
			XmlNode componentNode = objectDefinition.SelectSingleNode("component");
			if (componentNode!=null) {
				// build nested component init info
				ComponentInitInfo nestedComponentInfo = new ComponentInitInfo(componentNode, components);
				nestedComponentInfo.InitValues(componentNode, components);
				
				return new RefValueInfo( nestedComponentInfo );
			}
			
			// reference ?
			XmlNode refNode = objectDefinition.SelectSingleNode("ref");
			if (refNode!=null) {
				string refName = refNode.Attributes["name"].Value;
				if (components[refName]==null)
					throw new NullReferenceException("Reference to non-existent component: "+refName);
				return new RefValueInfo( components[ refName ] );
			}
			
			// value ?
			XmlNode valueNode = objectDefinition.SelectSingleNode("value");
			if (valueNode!=null)
				return new ValueInitInfo( valueNode.InnerText );

			// xml ?
			XmlNode xmlNode = objectDefinition.SelectSingleNode("xml");
			if (xmlNode!=null)
				return new ValueInitInfo( xmlNode.InnerXml );

			
			// System.Type reference ?
			XmlNode typeNode = objectDefinition.SelectSingleNode("type");
			if (typeNode!=null)
				return new TypeValueInitInfo( ResolveType(typeNode.InnerXml) );
			
			
			// list ?
			XmlNode listNode = objectDefinition.SelectSingleNode("list");
			if (listNode!=null) {
				XmlNodeList entryNodes = listNode.SelectNodes("entry");
				IValueInitInfo[] entries = new IValueInitInfo[entryNodes.Count];
				for (int i=0; i<entryNodes.Count; i++)
					entries[i] = ResolveValueInfo( entryNodes[i], components );
				return new ListValueInitInfo( entries );
			}
			
			// map ?
			XmlNode mapNode = objectDefinition.SelectSingleNode("map");
			if (mapNode!=null) {
				XmlNodeList entryNodes = mapNode.SelectNodes("entry");
				MapEntryInfo[] entries = new MapEntryInfo[entryNodes.Count];
				for (int i=0; i<entryNodes.Count; i++)
					entries[i] = new MapEntryInfo(
						entryNodes[i].Attributes["key"].Value,
						ResolveValueInfo( entryNodes[i], components ) );
				return new MapValueInitInfo( entries );
			}
			
			// unknown object definition (???)
			return null;
		}
		
		protected Type ResolveType(string type_description) 
        {
			const char Separator = ',' ;

            int aposPos = type_description.IndexOf('`');
            bool isGenericType = aposPos >= 0;
            string genericTypePart = String.Empty;

            if (isGenericType)
            {
                genericTypePart = type_description.Substring(aposPos, type_description.Length - aposPos);
                int genericPartEnd = genericTypePart.LastIndexOf(']');
                genericTypePart = genericTypePart.Substring(0, genericPartEnd + 1);

                type_description = type_description.Replace(genericTypePart, String.Empty);
            }

            string[] parts = type_description.Split(new char[] { Separator }, 2);

			if (parts.Length>1) 
            {
				// assembly name provided
				Assembly assembly;
                try {
                    assembly = Assembly.LoadWithPartialName(parts[1]);
                } catch (Exception ex) {
                    throw new Exception("Cannot load assembly " + parts[1], ex);
                }
				if (assembly==null)
					throw new Exception("Cannot load assembly "+parts[1]);
				
				try {
					return assembly.GetType( String.Format("{0}{1}", parts[0], genericTypePart), true, false );
				} catch (Exception ex) {
					throw new Exception("Cannot resolve type "+type_description, ex);
				}
			} else {
				int lastDotIndex = type_description.LastIndexOf('.');
                if (lastDotIndex >= 0)
                {
                    return ResolveType(String.Format("{0}{1}", type_description, genericTypePart) + "," + type_description.Substring(0, lastDotIndex));
                }
			}
			throw new Exception("Cannot resolve type "+type_description);
		}		
		
	}
}