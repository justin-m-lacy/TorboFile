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

	public class CustomSearchVM : ViewModelBase {

		#region COMMANDS

		public RelayCommand CmdPickDirectory {
			get {
				return this._cmdPickDirectory ?? ( this._cmdPickDirectory = new RelayCommand(
					this.PickDirectory ) );
			}
		}
		private RelayCommand _cmdPickDirectory;

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

		/// <summary>
		/// The private _searchDirectory property is necessary to allow the UI to display
		/// an invalid search path so the user can continue to edit it.
		/// However, invalid paths should not be stored to the application settings or the
		/// CustomSearchData object.
		/// </summary>
		public string SearchDirectory {
			get => this._searchDirectory;
			set {

				if( value != this._searchDirectory ) {

					this.SetProperty( ref this._searchDirectory, value );

					if( !Directory.Exists( value ) ) {
						throw new ValidationException( Resources.DIR_NO_EXIST_ERR );
					} else {
						this._searchData.Options.BaseDirectory = value;
						CustomSearchSettings.Default.LastDirectory = this._searchDirectory;
					}

				}

			} // set()
		}
		private string _searchDirectory;

		public BuildSearchVM BuildSearchVM { get => _buildSearchVM; }
		private BuildSearchVM _buildSearchVM;

		/// <summary>
		/// ViewModel for running the search.
		/// </summary>
		public RunSearchVM RunSearchVM { get => _runSearchVM; }
		private RunSearchVM _runSearchVM;


		public CustomSearchData CustomSearch {
			get => _searchData;
			set {

				if( this.SetProperty( ref this._searchData, value ) ) {

					this.BuildSearchVM.CustomSearch = value;
					this.RunSearchVM.CustomSearch = value;

				}
			}

		}
		private CustomSearchData _searchData;


		#endregion

		/// <summary>
		/// Run the operation.
		/// </summary>
		private void DoSearchMode() {
			Console.WriteLine( "DO SEARCH MODE" );
			this.EditMode = false;
		}

		private void EditSearch() {
			this.EditMode = true;
		}

		/// <summary>
		/// Pick the starting directory for the search.
		/// </summary>
		private void PickDirectory() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string searchDir = dialog.PickFolder( CustomSearchStrings.SEARCH_DIR_PROMPT );
				if( !string.IsNullOrEmpty( searchDir ) ) {
					this.SearchDirectory = searchDir;
				}

			}

		} //

		private void SaveCurrent() {

			//TODO: Cloning not appropriate here.
			//this.CloneSearch();

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string saveFile = dialog.PickSaveFile( CustomSearchStrings.SAVE_SEARCH_MSG, null, "custom_search", Properties.Resources.SEARCH_FILE_EXTENSION );

				if( !string.IsNullOrEmpty( saveFile ) ) {

					/// TODO: Make async?
					FileUtils.WriteBinary( saveFile, this._searchData );

				}

			}

		} //

		public void LoadSearch() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string loadFile = dialog.PickOpenFile( CustomSearchStrings.LOAD_SEARCH_MSG, null, Properties.Resources.SEARCH_FILE_EXTENSION );
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

			if( this._searchData == null ) {
				this._searchData = new CustomSearchData();
			} else {
				this._searchData.Clear();
			}

			this._searchData.Conditions = this.BuildSearchVM.MatchBuilder.CloneCollection();
			this._searchData.Actions = this.BuildSearchVM.ActionBuilder.CloneCollection();

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

			CustomSearchSettings settings = CustomSearchSettings.Default;

			CustomSearchData searchData;

			if( settings.saveLastSearch ) {

				Console.WriteLine( "LOADING PREVIOUS SEARCH" );
				searchData = CustomSearchSettings.LoadLastSearch() ?? NewSearchData();

			} else {

				searchData = NewSearchData();

			}

			this.CustomSearch = searchData;

		} // CustomSearchVM()

		/// <summary>
		/// Create new CustomSearchData with last options from settings.
		/// </summary>
		/// <returns></returns>
		private CustomSearchData NewSearchData() {

			Console.WriteLine( "CREATING NEW SEARCH" );
			CustomSearchData data = new CustomSearchData();

			data.Options.Flags = CustomSearchSettings.Default.searchFlags;
			Console.WriteLine( "LOADED FLAGS: " + data.Options.Flags );
			data.Options.BaseDirectory = CustomSearchSettings.Default.LastDirectory;

			return data;

		}

	} // class

} // namespace
