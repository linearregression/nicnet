﻿#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2013 Vitalii Fedorchenko
 * Copyright 2014 NewtonIdeas
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace NI.Data.Storage.Model {
	
	/// <summary>
	/// Generic data class
	/// </summary>
    public class Class {

		public string ID { get; set; }

		public int CompactID { get; set; }

		public string Name { get; set; }

        /// <summary>
        /// Hidden classes are not served by UI for objects manipulation - usually used for system internal data
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Determines indexable objects and their values for global search service
        /// </summary>
        public bool Indexable { get; set; }

        /// <summary>
        /// Predefined classes usually used for system-critical internal data and cannot be modified/removed
        /// </summary>
        public bool Predefined { get; set; }

        /// <summary>
        /// Can act as predicate in the relationship
        /// </summary>
        public bool IsPredicate { get; set; }

		//TODO: logging enabled flag

		public ClassObjectLocationMode ObjectLocation { get; set; }

		public IEnumerable<Property> Properties {
			get {
				return Schema.FindPropertyByClassID(ID);
			}
		}

		public IEnumerable<Relationship> Relationships {
			get {
				return Schema.FindClassRelationships(ID);
			}
		}

		public DataSchema Schema { get; internal set; }

		public Class() {
			ObjectLocation = ClassObjectLocationMode.ObjectTable;
		}

        public Class(string id) {
			ID = id;
		}

		public override bool Equals(object obj) {
			if (obj is Class) {
				var p = (Class)obj;
				if (p.ID != null && this.ID != null)
					return this.ID == p.ID;
			}
			return base.Equals(obj);
		}

		public static bool operator ==(Class v1, Class v2) {
			if ( ((object)v1)==null)
				return ((object)v2)==null;
			return v1.Equals(v2);
		}

		public static bool operator !=(Class a, Class b) {
			return !(a == b);
		}

		public override int GetHashCode() {
			return String.IsNullOrEmpty(ID) ? base.GetHashCode() : ID.GetHashCode();
		}

		public override string ToString() {
			return String.Format("Class(ID={0})", ID);
		}

		public bool HasProperty(Property p) {
			return Properties.Contains(p);
		}

		public Property FindPropertyByID(string id) {
			var p = Schema.FindPropertyByID(id);
			if (p!=null && !HasProperty(p) )
				return null;
			return p;
		}

		public Relationship FindRelationship(Class predicate, Class refClass, bool? reversed = null) {
			var rels = Relationships.Where(r => r.Predicate == predicate && r.Object==refClass);
			if (reversed.HasValue) {
				return rels.Where(r => r.Reversed == reversed.Value).FirstOrDefault();
			} else
				return rels.FirstOrDefault();
		}



		public DataTable CreateDataTable() {
			var t = new DataTable(ID);
			var idCol = t.Columns.Add( "id", typeof(long) );
			idCol.AutoIncrement = true;
			idCol.AutoIncrementSeed = -1;
			idCol.AutoIncrementStep = -1;
			t.PrimaryKey = new[] { idCol };

			foreach (var p in Properties) {
				var col = t.Columns.Add( p.ID, p.DataType.ValueType );
			}
			return t;
		}

	}


	public enum ClassObjectLocationMode {
		ObjectTable, // TBD: feature "SeparateTable"
		NativeTable
	}

}
