using TorboFile.Services;
using Lemur.Types;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorboFile.Properties;
using System.Windows.Data;
using System.Collections.Specialized;
using Lemur;
using System.Diagnostics;
using Lemur.Windows.Services;

namespace TorboFile.ViewModels {

	public class FindCopiesVM : ViewModelBase {

		~FindCopiesVM() {
			DebugDestructor();
		}
		[Conditional( "DEBUG" )]
		void DebugDestructor() {
			Console.WriteLine( "FIND DUPLICATES MODEL DESTRUCTOR" );
		}

		private RelayCommand _cmdBeginSearch;
		/// <summary>
		/// Choose a folder and begins search for duplicates.
		/// </summary>
		public RelayCommand CmdBeginSearch {
			get {

				return this._cmdBeginSearch ?? ( this._cmdBeginSearch = new RelayCommand(

					this.PickFolder

				) );
			}

		}

		#region PROPERTIES

		/// <summary>
		/// Current file being previewed.
		/// </summary>
		public FilePreviewModel FilePreview {

			get { return this._filePreview ?? ( this.FilePreview = new FilePreviewModel() ); }
			set {
				this._filePreview = value;
				this.NotifyPropertyChanged();
			}

		}
		private FilePreviewModel _filePreview;

		private ProgressModel _currentSearch;
		/// <summary>
		/// Current duplicates-search in progress.
		/// </summary>
		public ProgressModel CurrentSearch {
			get { return this._currentSearch; }
			set {
				this.SetProperty( ref this._currentSearch, value );
			} // set()
		}

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

		public string IncludeText {
			get {
				return FindCopiesSettings.Default.IncludeExtensions;
			}
			set {
				FindCopiesSettings.Default.IncludeExtensions = value;
			}
		}

		public string ExcludeText {
			get {
				return FindCopiesSettings.Default.ExcludeExtensions;
			}
			set {
				FindCopiesSettings.Default.ExcludeExtensions = value;
			}
		}

		#endregion

		public FindCopiesVM() {} //

		private void PickFolder() {

			IFileDialogService dialog = (IFileDialogService)this.ServiceProvider.GetService( typeof( IFileDialogService ) );
			if( dialog != null ) {
				string folder = dialog.PickFolder( "Choose a source folder..." );

				if( !string.IsNullOrEmpty( folder ) ) {

					Task t = this.FindCopiesAsync( folder );

				}
			}

		}

		private async void DeleteCheckedAsync( IEnumerable<FileData> checked_files ) {

			//Console.WriteLine( "deleting files: " + checked_files.Count() );

			FileDeleteService deleteService = this.GetService<FileDeleteService>();
			if( deleteService != null ) {

				await deleteService.DeleteFilesAsync(
					checked_files.Select<FileData, string>( ( data ) => { return data.Path; } ),
					FindCopiesSettings.Default.moveToTrash
				);

			}

			//Console.WriteLine( "DELETE COMPLETE" );

		} //

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private async Task FindCopiesAsync( string path ) {

			FileMatchFinder matchFinder = this.BuildMatchOperation( path );

			/// Displays progress.
			this.CurrentSearch = new ProgressModel( matchFinder );

			MatchCollection matchGroups = matchFinder.Matches;


			// NOTE: This isn't needed because the results are Dispatched to the UI Dispatcher.
			// The collection is locked during the CollectionChanged events, and the results are copied into a new list.
			// If the MatchCollection itself was displayed, this line would probably be necessary.
			//BindingOperations.EnableCollectionSynchronization( matchGroups, groupLock );
			
	
			// Receive updates as matches are found.
			matchGroups.CollectionChanged += this.Matches_CollectionChanged;
	
			// Ensure the ResultsModel exists for displaying results.
			this.CreateResultsList();

			try {

				object groupLock = new object();
				await
					Task.Run( () => { matchFinder.Run( groupLock ); } );

			} catch( Exception e ) {
				Console.WriteLine( "ERROR: " + e.ToString() );
			}

			if( this.ResultsList.Items.Count == 0 ) {
				// report no results found.
			}

			matchGroups.CollectionChanged -= this.Matches_CollectionChanged;

			Console.WriteLine( "nulling search" );
			this.CurrentSearch = null;

		} //

		/// <summary>
		/// Uses the current settings to build a file match finding operation.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private FileMatchFinder BuildMatchOperation( string path ) {

			FindCopiesSettings settings = FindCopiesSettings.Default;

			string[] include = settings.ParseExtensions( this.IncludeText );
			string[] exclude = settings.ParseExtensions( this.ExcludeText );

			bool recursive = settings.recursive;

			FileMatchFinder matchFinder = new FileMatchFinder( path, recursive );

			if( settings.useSizeRange ) {

				long minSize = settings.minSize;
				long maxSize = settings.maxSize;

				settings.minSize = minSize;
				settings.maxSize = maxSize;

				matchFinder.SetSizeRange( minSize, maxSize );

			}

			matchFinder.ExcludeTypes = exclude;
			matchFinder.FileTypes = include;

			return matchFinder;

		}

		private FileCheckListModel CreateResultsList() {

			if( this.ResultsList != null ) {
				ResultsList.Clear();
				return this.ResultsList;
			}

			FileCheckListModel checkList = new FileCheckListModel();
			checkList.DeleteDelegate = this.DeleteCheckedAsync;

			/// Listen for selected item to set the FilePreviewModel current preview.
			checkList.PropertyChanged += CheckList_PropertyChanged;

			this.ResultsList = checkList;

			return checkList;

		}

		private void CheckList_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {

			/// file selected change. show preview.
			if( e.PropertyName == CheckListModel<FileData>.SelectedPropertyName ) {

				ListItemModel<FileData> selectedData = this.ResultsList.SelectedItem;
				if( selectedData != null ) {
					FilePreview.FilePath = selectedData.Item.Path;
				} else {
					FilePreview.FilePath = string.Empty;
				}

			} //

		}

		/// <summary>
		/// Called when a Matching operation updates its collection.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Matches_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) {

			if( e.NewItems != null ) {

				foreach( FileMatchGroup group in e.NewItems ) {

					Console.WriteLine( "Adding group: " + group.FileSize );
					long groupFileSize = group.FileSize;

					// Check all but the first element.
					IEnumerator<string> matches = group.GetEnumerator();

					if( matches.MoveNext() ) {
						this.ResultsList.Items.Add( new ListItemModel<FileData>( new FileData( matches.Current, groupFileSize ) ) );
					}

					while( matches.MoveNext() ) {
						this.ResultsList.Items.Add( new ListItemModel<FileData>( new FileData( matches.Current, groupFileSize ), true ) );
					}


				} // foreach()

			}

		} // Groups_CollectionChanged

	} // class

} // namespace