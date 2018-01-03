using Lemur.Operations.FileMatching.Actions;
using Lemur.Operations.FileMatching.Models;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	public class ActionBuilder : CollectionBuilderVM<IFileAction> { }

	public class CustomSearchVM : ViewModelBase {

		#region COMMANDs

		/// <summary>
		/// Command to load a Search Operation saved to disk.
		/// </summary>
		private RelayCommand _cmdLoadOperation;
		/// <summary>
		/// Command to load an existing search from disk.
		/// </summary>
		public RelayCommand CmdLoadOperation {
			get {
				return this._cmdLoadOperation ?? ( this._cmdLoadOperation = new RelayCommand(
					this.LoadSearch ) );
			}
		}

		/// <summary>
		/// Command to save the operation as a permanent object.
		/// </summary>
		private RelayCommand _cmdSaveOperation;
		/// <summary>
		/// TODO: Doesn't really belong in this model?
		/// </summary>
		public RelayCommand CmdSaveOperation {
			get {
				return this._cmdSaveOperation ?? ( this._cmdSaveOperation = new RelayCommand(
					this.DispatchRequestSave, this.HasConditions ) );
			}
		}

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Model for building a new search.
		/// </summary>
		private readonly FileMatchBuilderVM matchBuilder = new FileMatchBuilderVM();
		public FileMatchBuilderVM MatchBuilder {
			get => matchBuilder;
		}

		public ActionBuilder ActionBuilder {
			get {
				return this.actionBuilder;
			}
		}
		private readonly ActionBuilder actionBuilder = new ActionBuilder();

		/// <summary>
		/// Model for picking an Action Type.
		/// </summary>
		/*public ActionPickerVM ActionPicker {
			get { return this.actionPicker; }
		}
		private readonly ActionPickerVM actionPicker = new ActionPickerVM();*/

		#endregion

		/// <summary>
		/// Event triggers when match conditions should be saved for
		/// future use.
		/// </summary>
		public event EventHandler OnRequestSave;
		private void DispatchRequestSave() {
			this.OnRequestSave?.Invoke( this, new EventArgs() );
		}

		public void LoadSearch() {
		}

		public bool HasActions() {
			return this.actionBuilder.HasItems();
		}

		/// <summary>
		/// Checks if any conditions have been added to the current match builder.
		/// </summary>
		/// <returns></returns>
		public bool HasConditions() {
			return this.matchBuilder.HasConditions();
		}

		public CustomSearchVM() {
		} //

	} // class

} // namespace
