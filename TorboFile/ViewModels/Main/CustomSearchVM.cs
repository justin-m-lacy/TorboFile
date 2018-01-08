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
using TorboFile.Services;

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

	/// <summary>
	/// Class definitions required for xaml links.
	/// </summary>
	public class MatchBuilder : CollectionBuilderVM<IMatchCondition, FileTestVM>  {}
	public class ActionBuilder : CollectionBuilderVM<IFileAction, DataObjectVM> { }

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

		public RelayCommand CmdPickDirectory {
			get {
				return this._cmdPickDirectory ?? ( this._cmdPickDirectory = new RelayCommand(
					this.PickDirectory ) );
			}
		}
		private RelayCommand _cmdPickDirectory;

		/// <summary>
		/// Command to edit/begin a new search. Cancels an operation in progress.
		/// </summary>
		public RelayCommand CmdEditSearch {
			get {
				return this._cmdEdit ?? ( this._cmdEdit = new RelayCommand(
					this.EditSearch ) );
			}
		}
		private RelayCommand _cmdEdit;

		/// <summary>
		/// Command to run the current search and associated actions.
		/// </summary>
		public RelayCommand CmdRunOperation {
			get {
				return this._cmdRun ?? ( this._cmdRun = new RelayCommand(
					this.RunCurrent ) );
			}
		}
		private RelayCommand _cmdRun;

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
		/// Models the progression of the current search.
		/// </summary>
		public ProgressModel SearchProgress {
			get { return this._searchProgress; }
			set {
				this.SetProperty( ref this._searchProgress, value );
			} // set()
		}
		private ProgressModel _searchProgress;

		/// <summary>
		/// Model for building a new search.
		/// </summary>
		private readonly MatchBuilder matchBuilder;
		public MatchBuilder MatchBuilder {
			get => matchBuilder;
		}

		public ActionBuilder ActionBuilder {
			get {
				return this.actionBuilder;
			}
		}
		private readonly ActionBuilder actionBuilder;

		/// <summary>
		/// Used to lock Search results and search errors to enable synchronization
		/// between the search-thread and the ui-thread.
		/// </summary>
		private object resultsLock;

		/// <summary>
		/// Model of the results from a matching operation.
		/// </summary>
		public FileCheckListModel ResultsList {

			get { return this._resultsList; }

			private set {
				this.SetProperty( ref this._resultsList, value );
			}

		}
		private FileCheckListModel _resultsList;

		public CustomFileSearch CustomSearch {
			get => _customSearch;
			set {

				if( this.SetProperty( ref this._customSearch, value ) ) {
					this.actionBuilder.SetItems( this._customSearch.Actions );
					this.matchBuilder.SetItems( this._customSearch.Conditions );
				}
			}

		}

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

		private CustomFileSearch _customSearch;


		#endregion

		/// <summary>
		/// Run the operation.
		/// </summary>
		private void RunCurrent() {

			this.CloneSearch();

			Console.WriteLine( "RUNNING SEARCH" );

			this.EditMode = false;

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private async Task RunSearchAsync( string path ) {

			FileMatchOperation matchFinder = this.BuildMatchOperation();

			List<FileSystemInfo> matches = matchFinder.Matches;

			if( this.resultsLock == null ) {
				this.resultsLock = new object();
			}
			BindingOperations.EnableCollectionSynchronization( matches, resultsLock );

			this.SearchProgress = new ProgressModel( matchFinder );

			// Ensure the ResultsModel exists for displaying results.
			this.CreateResultsList();

			try {
				await
					Task.Run( () => { matchFinder.Run( this.resultsLock ); } );
			} catch( Exception e ) {
				Console.WriteLine( "ERROR: " + e.ToString() );
			}

			if( this.ResultsList.Items.Count == 0 ) {
				// report no results found.
			}

		} //

		private void ResultFound( FileSystemInfo result ) {
		}

		private async Task RunActionsAsync() {

			/*try {
				await
					Task.Run( () => { matchFinder.Run( groupLock ); } );
			} catch( Exception e ) {
				Console.WriteLine( "Actions Error: " + e.ToString() );
			}*/

		} //

		private FileMatchOperation BuildMatchOperation() {

			FileMatchOperation matchOp = new FileMatchOperation( this._customSearch.Conditions );
			return matchOp;
		}

		private FileCheckListModel CreateResultsList() {

			if( this.ResultsList != null ) {
				ResultsList.Clear();
				return this.ResultsList;
			}

			FileCheckListModel checkList = new FileCheckListModel();

			this.ResultsList = checkList;

			return checkList;

		}

		/// <summary>
		/// Assigns a directory to a destination object.
		/// This is used when a descendent object needs to allow the user to pick
		/// a directory from a file dialog.
		/// </summary>
		/// <param name="destObject"></param>
		/// <param name="destProp"></param>
		/*private void AssignFolder( object destObject, string destProp ) {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			string directory = dialog.PickFolder( "Choose a destination folder..." );
			if( !string.IsNullOrEmpty( directory ) ) {

				PropertyInfo pinfo = destObject.GetType().GetProperty( destProp );
				pinfo.SetValue( destObject, directory );

			}

		}*/ //

		private void PickDirectory() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string searchDir = dialog.PickFolder( "Choose search folder." );
				if( !string.IsNullOrEmpty( searchDir ) ) {
					this.SearchDirectory = searchDir;
				}

			}

		} //

		private void EditSearch() {

			Console.WriteLine( "EDITING SEARCH" );
			this.EditMode = true;

		}


		private void SaveCurrent() {

			this.CloneSearch();

			/// need a save dialog?
			IFileDialogService dialog = this.GetService<IFileDialogService>();
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

		public CustomSearchVM( IServiceProvider provider ) : base( provider ) {

			this.matchBuilder = new MatchBuilder();
			this.matchBuilder.ServiceProvider = provider;

			this.actionBuilder = new ActionBuilder();
			this.actionBuilder.ServiceProvider = provider;

			this.InitViewModelCreators();

			this.matchBuilder.Picker.ExcludeTypes = new Type[] { typeof( ContainedIn ), typeof( ConditionList ), typeof( ConditionEnumeration ) };

		}

		/// <summary>
		/// Initializes the view models to use for each displayed data type.
		/// </summary>
		private void InitViewModelCreators() {

			ViewModelBuilder builder = this.GetService<ViewModelBuilder>();
			if( builder != null ) {

				//Console.WriteLine( "SETTING DEFAULT VM BUILDERS" );
				builder.SetCreator<BaseCondition>( this.CreateVM<FileTestVM> );
				builder.SetCreator<MoveFileAction>( this.CreateVM<MoveFileVM> );
				builder.SetCreator<FileActionBase>( this.CreateVM<DataObjectVM> );

			}

		}


		private VM CreateVM<VM>( object data, object view = null ) where VM : DataObjectVM, new() {

			Console.WriteLine( "Creating ViewModel: " + typeof( VM ).Name );
			VM vm = new VM();
			vm.ServiceProvider = this.ServiceProvider;
			vm.Data = data;
			return vm;

		} //

	} // class

} // namespace
