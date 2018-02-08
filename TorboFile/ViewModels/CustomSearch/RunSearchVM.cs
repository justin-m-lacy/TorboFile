using TorboFile.Operations;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TorboFile.Model;
using Lemur.Windows.Text;
using System.Diagnostics;
using TorboFile.Services;

namespace TorboFile.ViewModels.Main {

	/// <summary>
	/// ViewModel for running a CustomSearch.
	/// </summary>
	public class RunSearchVM : ViewModelBase {

		#region DEBUG

		~RunSearchVM() {
			DebugDestructor();
		}
		[Conditional( "DEBUG" )]
		void DebugDestructor() {
			Console.WriteLine( "RunSearchVM destroyed." );
		}


		#endregion

		#region COMMANDs

		/// <summary>
		/// Command to run the current search, matching any files that match
		/// the search conditions.
		/// </summary>
		public RelayCommand CmdRunSearch {
			get {
				return this._cmdRunSearch ?? ( this._cmdRunSearch = new RelayCommand(
					async () => await this.RunSearchAsync()
				) );
			}
		}
		private RelayCommand _cmdRunSearch;

		/// <summary>
		/// Command to run the associated actions on any files found in the search phase.
		/// </summary>
		public RelayCommand CmdRunActions {
			get {
				return this._cmdRunActions ?? ( this._cmdRunActions = new RelayCommand(
					this.RunActionsAsync
				) );
			}
		}
		private RelayCommand _cmdRunActions;

		#endregion

		#region PROPERTIES

		private TextString _output;
		public TextString Output {
			get => this._output;
			set {
				this._output = value;
				NotifyPropertyChanged();
			}

		}

		/// <summary>
		/// Preview of the currently selected file.
		/// </summary>
		public FilePreviewVM FilePreview {

			get { return this._filePreview ?? ( this.FilePreview = new FilePreviewVM() ); }
			set {
				this._filePreview = value;
				this.NotifyPropertyChanged();
			}

		}
		private FilePreviewVM _filePreview;

		/// <summary>
		/// Models the progression of the current search.
		/// </summary>
		public ProgressVM CurrentProgress {
			get { return this._currentProgress; }
			set {
				this.SetProperty( ref this._currentProgress, value );
			} // set()
		}
		private ProgressVM _currentProgress = new ProgressVM( null );

		/// <summary>
		/// Model of the results from a matching operation.
		/// </summary>
		public FileListVM ResultsList {

			get { return this._resultsList; }

			private set {
				this.SetProperty( ref this._resultsList, value );
			}

		}
		private FileListVM _resultsList;

		public bool HasCheckedItems {
			get {
				return this._resultsList != null && this._resultsList.CheckedItems.Count > 0;
			}
		}

		public CustomSearchData CustomSearch {
			get => _customSearch;
			set {
				this.SetProperty( ref this._customSearch, value );
			}

		}
		private CustomSearchData _customSearch;

		#endregion

		/// <summary>
		/// Only run the actions on the selected files.
		/// </summary>
		/// <returns>A Task, apparently.</returns>
		private async void RunActionsAsync() {

			using ( FileActionOperation operation = new FileActionOperation() ) {
	
				operation.Actions = this.CustomSearch.Actions;
				operation.Targets = this.ResultsList.CheckedItems;

				operation.Options = this._customSearch.Options;

				this.CurrentProgress.Operation = operation;

				try {

					await Task.Run( () => { operation.Run(); }, operation.GetToken() );

				} catch( Exception e ) {

					this.Output = new TextString( e.Message, TextString.Error );
					Console.WriteLine( "ERROR: " + e.ToString() );

				}

				Exception[] exceptions = operation.ErrorList;
				foreach( Exception e in exceptions ) {
					Console.WriteLine( e.ToString() );
					this.Output = new TextString( e.Message, "error" );
				}

				this.CurrentProgress.Operation = null;

			} // using

		} // RunActionsAsync()

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private async Task RunSearchAsync() {

			using( FileMatchOperation operation = this.BuildMatchOperation( this._customSearch ) ) {

				this._currentProgress.Operation = operation;

				// Ensure the ResultsModel exists for displaying results.
				this.CreateResultsList();

				try {

					await Task.Run( () => operation.Run() );

				} catch( Exception e ) {
					this.Output = new TextString( e.Message, TextString.Error );
					Console.WriteLine( "ERROR: " + e.ToString() );
				}

				if( this.ResultsList.Items.Count == 0 ) {
					// report no results found.
					Console.WriteLine( "NO RESULTS FOUND" );
					this.Output = new TextString( "No matches found." );
				}

				this.CurrentProgress.Operation = null;
				SearchDone( operation );

			} //


		} //

		private void MatchFinder_OnMatchFound( FileSystemInfo obj ) {

			//Console.WriteLine( "RESULT FOUND: " + obj.Name );
			if( this.ResultsList != null ) {
				this.ResultsList.Add( obj, true );
			} else {
				Console.WriteLine( "ERROR: RESULT LIST IS NULL" );
			}

		}

		private void SearchDone( FileMatchOperation op ) {
			op.OnMatchFound -= MatchFinder_OnMatchFound;
			op.OnError -= this.MatchOp_OnError;
		}

		private FileMatchOperation BuildMatchOperation( CustomSearchData search ) {

			FileMatchOperation matchOp = new FileMatchOperation( search.Options.BaseDirectory, search.Conditions, search.Options );
			matchOp.OnMatchFound += MatchFinder_OnMatchFound;
			matchOp.OnError += MatchOp_OnError;

			return matchOp;
		}

		private void MatchOp_OnError( Exception obj ) {
			this.Output = new TextString( obj.Message, TextString.Error );
		}

		private FileListVM CreateResultsList() {

			if( this.ResultsList != null ) {
				ResultsList.Clear();
				return this.ResultsList;
			}

			FileListVM checkList = new FileListVM();
			checkList.ShowOpenCmd = true;
			checkList.ShowCheckBox = true;
			checkList.ShowOpenLocationCmd = true;
			checkList.ShowDeleteCmd = true;
			checkList.DeleteAction = this.DeleteCheckedAsync;

			/// NOTE: Apparently a public interface member can be protected unless explicitly cast.
			/// This seems insane to me.
			//( (INotifyPropertyChanged)checkList.CheckedItems ).PropertyChanged += CheckedItems_PropertyChanged;

			checkList.PropertyChanged += ResultsList_PropertyChanged;

			this.ResultsList = checkList;

			return checkList;

		}

		private void ResultsList_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			/// file selected change. show preview.
			if( e.PropertyName == FileListVM.SelectedPropertyName ) {

				ListItemVM<FileSystemInfo> selectedData = this.ResultsList.SelectedItem;
				if( selectedData != null ) {
					FilePreview.FilePath = selectedData.Item.FullName;
				} else {
					FilePreview.FilePath = string.Empty;
				}

			} //

		}

		private async void DeleteCheckedAsync( IEnumerable<FileSystemInfo> checked_files ) {

			FileDeleteService deleteService = this.GetService<FileDeleteService>();
			if( deleteService != null ) {

				await deleteService.DeleteFilesAsync(
					checked_files.Select<FileSystemInfo, string>( ( data ) => { return data.FullName; } ), true
				);

			}

		} //

		/// <summary>
		/// Checks that results exist, and that at least some are selected
		/// for running actions.
		/// </summary>
		/// <returns></returns>
		public bool HasCheckedResults() {
			return this._resultsList != null && this._resultsList.CheckedItems.Count > 0;
		}

		public RunSearchVM( IServiceProvider provider ) : base( provider ) {

			Console.WriteLine( "RunSearchVM created." );

		}

	} // class

} // namespace
