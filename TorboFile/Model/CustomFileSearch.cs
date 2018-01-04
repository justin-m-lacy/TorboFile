using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemur.Operations.FileMatching;
using Lemur.Operations.FileMatching.Actions;

namespace TorboFile.Model {

	/// <summary>
	/// Search criteria and actions to apply during a custom file operation.
	/// </summary>
	[Serializable]
	public class CustomFileSearch {

		public List<IMatchCondition> Conditions { get => _conditions; set => _conditions = value; }
		private List<IMatchCondition> _conditions;

		public List<IFileAction> Actions { get => _actions; set => _actions = value; }
		private List<IFileAction> _actions;

		public CustomFileSearch() {

			this.Conditions = new List<IMatchCondition>();
			this.Actions = new List<IFileAction>();

		}

		public CustomFileSearch( IEnumerable<IMatchCondition> conditions, IEnumerable<IFileAction> actions ) {

			this.Conditions = new List<IMatchCondition>( conditions );
			this.Actions = new List<IFileAction>( actions );

		}

		/// <summary>
		/// Empties all conditions and actions from the search.
		/// </summary>
		public void Clear() {
			this._conditions.Clear();
			this._actions.Clear();
		}


	} // class

} // namespace
