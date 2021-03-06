#region License
/*
 * Open NIC.NET library (http://nicnet.googlecode.com/)
 * Copyright 2004-2012 NewtonIdeas
 * Copyright 2008-2013 Vitalii Fedorchenko (changes and v.2)
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
using System.Collections;

namespace NI.Data
{

	/// <summary>
	/// Represents SQL-specified DALC component
	/// </summary>
	public interface ISqlDalc : IDalc {

		/// <summary>
		/// Execute SQL command
		/// </summary>
		/// <param name="sqlText">SQL command text</param>
		/// <returns>number of affected records</returns>
		int ExecuteNonQuery(string sqlText);

		/// <summary>
		/// Execute given raw SQL and return data reader
		/// </summary>
		void ExecuteReader(string sqlText, Action<IDataReader> handler);

		/// <summary>
		/// Execute custom SQL command and store result in specified dataset
		/// </summary>
		void Load(string sqlText, DataSet ds);

	
	}

}
