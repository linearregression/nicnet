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

namespace NI.Data.Dalc.Permissions
{
	/// <summary>
	/// Dalc operation.
	/// </summary>
	public enum DalcOperation
	{
		Retrieve = 1,
		Create = 2,
		Update = 3,
		Delete = 4
	}
}