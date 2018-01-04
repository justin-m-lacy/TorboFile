using Lemur.Operations.FileMatching;
using Lemur.Operations.FileMatching.Actions;
using Lemur.Operations.FileMatching.Models;
using Lemur.Utils;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorboFile.Model;
using TorboFile.Services;

namespace TorboFile.ViewModels {

	public class ActionBuilder : CollectionBuilderVM<IFileAction, DataObjectVM> { }
	public class MatchBuilder : CollectionBuilderVM<IMatchCondition, FileTestVM> { }

	public enum CustomSearchMode {

		/// <summary>
		/// Mode that indicates a custom search is being constructed.
		/// </summary>
		Edit,

		/// <summary>
		/// Indicates that a search is being performed to find matching
		/// files.
		/// </summary>
		Search,

		/// <summary>
		/// Indicates that a list of actions are being applied to the
		/// discovered files.
		/// </summary>
		Apply

	} //

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
					this.SaveCurrent ) );
			}
		}

		/// <summary>
		/// Command to save the operation as a permanent object.
		/// </summary>
		private RelayCommand _cmdRun;
		/// <summary>
		/// TODO: Doesn't really belong in this model?
		/// </summary>
		public RelayCommand CmdRunOperation {
			get {
				return this._cmdRun ?? ( this._cmdRun = new RelayCommand(
					this.RunCurrent ) );
			}
		}

		#endregion

		#region PROPERTIES

		/// <summary>
		/// True when a search is being constructed, false when a search is being ran.
		/// </summary>
		public bool EditMode {
			get {
				return this._editMode;
			}
			private set { this.SetProperty( ref this._editMode, value );
			}

		}
		private bool _editMode = true;


		/// <summary>
		/// Model for building a new search.
		/// </summary>
		private readonly MatchBuilder matchBuilder = new MatchBuilder();
		public MatchBuilder MatchBuilder {
			get => matchBuilder;
		}

		public ActionBuilder ActionBuilder {
			get {
				return this.actionBuilder;
			}
		}
		private readonly ActionBuilder actionBuilder = new ActionBuilder();

		public CustomFileSearch CustomSearch {
			get => _customSearch;
			set {

				if( this.SetProperty( ref this._customSearch, value ) ) {

					this.actionBuilder.SetItems( this._customSearch.Actions );
					this.matchBuilder.SetItems( this._customSearch.Conditions );

				}

			}

		}
		private CustomFileSearch _customSearch;

		#endregion

		/// <summary>
		/// Run the operation.
		/// </summary>
		private void RunCurrent() {

			this.CloneSearch();

		}

		private void SaveCurrent() {

			this.CloneSearch();

			/// need a save dialog?
			FileDialogService dialog = this.GetService<FileDialogService>();
			if( dialog != null ) {

				string saveFile = dialog.PickSaveFile( "Save Search..." );
				if( !string.IsNullOrEmpty( saveFile ) ) {

					/// TODO: Make async?
					FileUtils.WriteBinary( saveFile, this._customSearch );

				}

			}

		} //

		public void LoadSearch() {

			/// need a save dialog?
			FileDialogService dialog = this.GetService<FileDialogService>();
			if( dialog != null ) {

				string loadFile = dialog.PickLoadFile( "Load Search..." );
				if( !string.IsNullOrEmpty( loadFile ) ) {

					/// TODO: Make async?
					CustomFileSearch loadedSearch = FileUtils.ReadBinary<CustomFileSearch>( loadFile );
					if( loadedSearch != null ) {
						this.CustomSearch = loadedSearch;
					}

				}

			}

		} // LoadSearch()

		public bool HasActions() {
			return this.actionBuilder.HasItems();
		}

		/// <summary>
		/// Build the current search from the currently displayed items.
		/// </summary>
		private void CloneSearch() {

			if( this._customSearch == null ) {
				this._customSearch = new CustomFileSearch();
			} else {
				this._customSearch.Clear();
			}

			this._customSearch.Conditions = this.matchBuilder.CloneCollection();
			this._customSearch.Actions = this.actionBuilder.CloneCollection();

		}

		/// <summary>
		/// Checks if any conditions have been added to the current match builder.
		/// </summary>
		/// <returns></returns>
		public bool HasConditions() {
			return this.matchBuilder.HasItems();
		}

		public CustomSearchVM() {
		} //

	} // class

} // namespace
