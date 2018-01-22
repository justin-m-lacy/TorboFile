using Lemur.Operations.FileMatching;
using Lemur.Operations.FileMatching.Actions;
using Lemur.Operations.FileMatching.Models;
using Lemur.Utils;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using Lemur.Windows.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TorboFile.Model;
using TorboFile.Properties;
using TorboFile.Services;
using TorboFile.ViewModels.Main;

namespace TorboFile.ViewModels {

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
		/// Command to edit the current search or build a new search.
		/// Cancels any operation in progress and switches to edit mode.
		/// </summary>
		public RelayCommand CmdEditSearch {
			get {
				return this._cmdEdit ?? ( this._cmdEdit = new RelayCommand(
					this.EditSearch ) );
			}
		}
		private RelayCommand _cmdEdit;

		/// <summary>
		/// Command to switch into run-search mode.
		/// </summary>
		public RelayCommand CmdSearchMode {
			get {
				return this._cmdSearchMode ?? ( this._cmdSearchMode = new RelayCommand(
					this.DoSearchMode ) );
			}
		}
		private RelayCommand _cmdSearchMode;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// True when a search is being constructed, false when a search is being ran.
		/// </summary>
		public bool EditMode {
			get {
				return this._editMode;
			}
			private set {
				this.SetProperty( ref this._editMode, value );
			}

		}
		private bool _editMode = true;

		public string SearchDirectory {
			get => _searchDirectory;
			set {

				if( value != this._searchDirectory ) {

					this.SetProperty( ref this._searchDirectory, value );

					if( !Directory.Exists( value ) ) {
						throw new ValidationException( "Error: Search directory does not exist." );
					}

				}

			} // set()
		}
		private string _searchDirectory;

		public BuildSearchVM BuildSearchVM { get => _buildSearchVM; set => _buildSearchVM = value; }
		private BuildSearchVM _buildSearchVM;

		/// <summary>
		/// ViewModel for running the search.
		/// </summary>
		public RunSearchVM RunSearchVM { get => _runSearchVM; }
		private RunSearchVM _runSearchVM;


		public CustomSearchData CustomSearch {
			get => _customSearch;
			set {

				if( this.SetProperty( ref this._customSearch, value ) ) {
					this.BuildSearchVM.CustomSearch = value;
					this.RunSearchVM.CustomSearch = value;
				}
			}

		}
		private CustomSearchData _customSearch;


		#endregion

		/// <summary>
		/// Run the operation.
		/// </summary>
		private void DoSearchMode() {

			Console.WriteLine( "RUN-SEARCH MODE." );

			this.EditMode = false;

		}

		private void EditSearch() {

			Console.WriteLine( "EDIT-SEARCH MODE." );
			this.EditMode = true;

		}


		private void SaveCurrent() {

			//TODO: Cloning not appropriate here.
			//this.CloneSearch();

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string saveFile = dialog.PickSaveFile( "Save Search...", null, "custom_search", Properties.Resources.SEARCH_FILE_EXTENSION );

				if( !string.IsNullOrEmpty( saveFile ) ) {

					/// TODO: Make async?
					FileUtils.WriteBinary( saveFile, this._customSearch );

				}

			}

		} //

		public void LoadSearch() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string loadFile = dialog.PickOpenFile( "Load Search...", null, Properties.Resources.SEARCH_FILE_EXTENSION );
				if( !string.IsNullOrEmpty( loadFile ) ) {

					/// TODO: Make async?
					CustomSearchData loadedSearch = FileUtils.ReadBinary<CustomSearchData>( loadFile );
					if( loadedSearch != null ) {

						this.CustomSearch = loadedSearch;

					}

				} //

			}

		} // LoadSearch()


		/// <summary>
		/// Build the current search from the currently displayed items.
		/// </summary>
		private void CloneSearch() {

			if( this._customSearch == null ) {
				this._customSearch = new CustomSearchData();
			} else {
				this._customSearch.Clear();
			}

			this._customSearch.Conditions = this.BuildSearchVM.MatchBuilder.CloneCollection();
			this._customSearch.Actions = this.BuildSearchVM.ActionBuilder.CloneCollection();

		}

		/// <summary>
		/// Checks if any conditions have been added to the current match builder.
		/// </summary>
		/// <returns></returns>
		public bool HasConditions() {
			return this.BuildSearchVM.MatchBuilder.HasItems();
		}

		public CustomSearchVM( IServiceProvider provider ) : base( provider ) {

			this._buildSearchVM = new BuildSearchVM( provider );
			this._runSearchVM = new RunSearchVM( provider );

			this.CustomSearch = new CustomSearchData();

			CustomSearchSettings settings = Properties.CustomSearchSettings.Default;
			if( settings.saveLastDirectory ) {
			}

		}

	} // class

} // namespace
