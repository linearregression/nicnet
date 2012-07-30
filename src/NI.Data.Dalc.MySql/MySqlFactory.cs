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
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using NI.Common.Providers;

namespace NI.Data.Dalc.MySql
{

	public class MySqlFactory : IDbCommandWrapperFactory, IDbDataAdapterWrapperFactory
	{
		IQueryFieldValueFormatter _QueryFieldValueFormatter = null;
        IObjectProvider _CmdParameterPlaceholderProvider;

        public IObjectProvider CmdParameterPlaceholderProvider
        {
            get { return _CmdParameterPlaceholderProvider; }
            set { _CmdParameterPlaceholderProvider = value; }
        }
		/// <summary>
		/// Get or set default query field value formatter
		/// </summary>
		public IQueryFieldValueFormatter QueryFieldValueFormatter {
			get { return _QueryFieldValueFormatter; }
			set { _QueryFieldValueFormatter = value; }
		}
	
		IDbCommandWrapper IDbCommandWrapperFactory.CreateInstance() {
			MySqlCommandWrapper cmdWrapper = new MySqlCommandWrapper( new MySqlCommand() );
            if (CmdParameterPlaceholderProvider != null)
                cmdWrapper.CmdParameterPlaceholderProvider = CmdParameterPlaceholderProvider;
			cmdWrapper.QueryFieldValueFormatter = QueryFieldValueFormatter;
			return cmdWrapper;
		}

		IDbDataAdapterWrapper IDbDataAdapterWrapperFactory.CreateInstance() {
			return new MySqlAdapterWrapper( new MySqlDataAdapter() );
		}
		
		
		
		


	}
}
