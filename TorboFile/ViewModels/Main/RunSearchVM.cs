using Lemur.Operations.FileMatching;
using Lemur.Operations.FileMatching.Actions;
using Lemur.Utils;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using Lemur.Windows.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TorboFile.Model;

namespace TorboFile.ViewModels.Main {

	/// <summary>
	/// ViewModel for running a CustomSearch.
	/// </summary>
	public class RunSearchVM : ViewModelBase {

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
		/// Command to run the current search, matching any files that match
		/// the search conditions.
		/// </summary>
		public RelayCommand CmdRunSearch {
			get {
				return this._cmdRunSearch ?? ( this._cmdRunSearch = new RelayCommand(
					this.RunSearch ) );
			}
		}
		private RelayCommand _cmdRunSearch;

		/// <summary>
		/// Command to run the associated actions on any files found in the search phase.
		/// </summary>
		public RelayCommand CmdRunActions {
			get {
				return this._cmdRunActions ?? ( this._cmdRunActions = new RelayCommand(
					this.RunActions ) );
			}
		}
		private RelayCommand _cmdRunActions;

		#endregion

		#region PROPERTIES

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

		public CustomSearchData CustomSearch {
			get => _customSearch;
			set {
				this.SetProperty( ref this._customSearch, value );
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

		private CustomSearchData _customSearch;


		#endregion

		/// <summary>
		/// Run the operation.
		/// </summary>
		private void RunSearch() {

			Console.WriteLine( "RUNNING SEARCH" );

		}

		private void RunActions() {

			Console.WriteLine( "RUNNING ACTIONS" );
		}

		/// <summary>
		/// Only run the actions on the selected files.
		/// </summary>
		/// <returns>A Task, apparently.</returns>
		private async Task RunActionsAsyc() {

			FileActionOperation operation = new FileActionOperation();

			this.SearchProgress.Operation = operation;


			try {

				await Task.Run( () => { operation.Run(); } );

			} catch( Exception e ) {

				Console.WriteLine( "ERROR: " + e.ToString() );

			}

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

				await Task.Run(
					() => { matchFinder.Run( this.resultsLock ); }
				);

			} catch( Exception e ) {
				Console.WriteLine( "ERROR: " + e.ToString() );
			}

			if( this.ResultsList.Items.Count == 0 ) {
				// report no results found.
			}

		} //

		private void ResultFound( FileSystemInfo result ) {
		}

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

		private void PickDirectory() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string searchDir = dialog.PickFolder( "Choose search folder." );
				if( !string.IsNullOrEmpty( searchDir ) ) {
					this.SearchDirectory = searchDir;
				}

			} else {
				Console.WriteLine( "ERROR: DIALOG NULL" );
			}

		} //

		private void SaveCurrent() {

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
			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string loadFile = dialog.PickLoadFile( "Load Search..." );
				if( !string.IsNullOrEmpty( loadFile ) ) {

					/// TODO: Make async?
					CustomSearchData loadedSearch = FileUtils.ReadBinary<CustomSearchData>( loadFile );
					if( loadedSearch != null ) {
						this.CustomSearch = loadedSearch;
					}

				}

			}

		} // LoadSearch()

		public bool HasActions() {
			//return this.actionBuilder.HasItems();
			return false;
		}

		/// <summary>
		/// Checks if any conditions have been added to the current match builder.
		/// </summary>
		/// <returns></returns>
		public bool HasConditions() {
			//return this.matchBuilder.HasItems();
			return false;
		}

		/// <summary>
		/// Checks that results exist, and that at least some are selected
		/// for running actions.
		/// </summary>
		/// <returns></returns>
		public bool HasCheckedResults() {
			return this._resultsList != null && this._resultsList.CheckedItems.Count > 0;
		}

		public RunSearchVM( IServiceProvider provider ) : base( provider ) {
		}

	} // class

} // namespace
