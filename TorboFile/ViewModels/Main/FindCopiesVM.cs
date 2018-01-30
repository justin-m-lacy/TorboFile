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
using System.IO;
using Lemur.Windows.Text;
using System.ComponentModel.DataAnnotations;

namespace TorboFile.ViewModels {

	public class FindCopiesVM : ViewModelBase {

		~FindCopiesVM() {
			DebugDestructor();
		}
		[Conditional( "DEBUG" )]
		void DebugDestructor() {
			Console.WriteLine( "FIND DUPLICATES MODEL DESTRUCTOR" );
		}

		#region COMMANDS

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

		#endregion

		#region PROPERTIES

		private TextString _output;
		public TextString Output {
			get => this._output;
			set => this.SetProperty( ref this._output, value );
		}

		/// <summary>
		/// Current file being previewed.
		/// </summary>
		public FilePreviewVM FilePreview {

			get { return this._filePreview ?? ( this.FilePreview = new FilePreviewVM() ); }
			set {
				this._filePreview = value;
				this.NotifyPropertyChanged();
			}

		}
		private FilePreviewVM _filePreview;

		private ProgressVM _currentSearch;
		/// <summary>
		/// Current duplicates-search in progress.
		/// </summary>
		public ProgressVM CurrentSearch {
			get { return this._currentSearch; }
			set {
				this.SetProperty( ref this._currentSearch, value );
			} // set()
		}

		public bool Recursive {
			get => FindCopiesSettings.Default.recursive;
			set {
				FindCopiesSettings.Default.recursive = value;
				this.NotifyPropertyChanged();
			}
		}


		public bool UseSizeRange {
			get => FindCopiesSettings.Default.useSizeRange;
			set {
				FindCopiesSettings.Default.useSizeRange = value;
				this.NotifyPropertyChanged();
			}
		}

		public DataSize MinSize {

			get { return FindCopiesSettings.Default.minSize; }

			set {
				FindCopiesSettings.Default.minSize = value;
				NotifyPropertyChanged();
			}

		}

		public DataSize MaxSize {
			get { return FindCopiesSettings.Default.maxSize; }
			set {
				FindCopiesSettings.Default.maxSize = value;
				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Model of the results from a matching operation.
		/// </summary>
		public DuplicateFilesVM ResultsList {

			get { return this._resultsList; }

			private set {
				this.SetProperty( ref this._resultsList, value );
			}

		}
		private DuplicateFilesVM _resultsList;

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
				string folder = dialog.PickFolder( Properties.Resources.PICK_SEARCH_FOLDER );

				if( !string.IsNullOrEmpty( folder ) ) {
					Task t = this.FindCopiesAsync( folder );
				}
			}

		} // PickFolder()

		private async void DeleteCheckedAsync( IEnumerable<FileSystemInfo> checked_files ) {

			//Console.WriteLine( "deleting files: " + checked_files.Count() );

			FileDeleteService deleteService = this.GetService<FileDeleteService>();
			if( deleteService != null ) {

				await deleteService.DeleteFilesAsync(
					checked_files.Select<FileSystemInfo, string>( ( data ) => { return data.FullName; } ),
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
			this.CurrentSearch = new ProgressVM( matchFinder );

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
				this.Output = new TextString( Properties.Resources.NO_MATCHES_FOUND );
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

		private DuplicateFilesVM CreateResultsList() {

			if( this.ResultsList != null ) {
				ResultsList.Clear();
				return this.ResultsList;
			}

			DuplicateFilesVM checkList = new DuplicateFilesVM();
			checkList.DeleteAction = this.DeleteCheckedAsync;
			checkList.ShowCheckBox = true;
			checkList.ShowOpenCmd = checkList.ShowOpenLocationCmd = checkList.ShowDeleteCmd = true;

			/// Listen for selected item to set the FilePreviewModel current preview.
			checkList.PropertyChanged += CheckList_PropertyChanged;

			this.ResultsList = checkList;

			return checkList;

		}

		private void CheckList_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {

			/// file selected change. show preview.
			if( e.PropertyName == CheckListVM<FileDuplicateInfo>.SelectedPropertyName ) {

				ListItemVM<FileSystemInfo> selectedData = this.ResultsList.SelectedItem;
				if( selectedData != null ) {
					FilePreview.FilePath = selectedData.Item.FullName;
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

					//Console.WriteLine( "Adding group: " + group.FileSize );
					long groupFileSize = group.FileSize;

					// Check all but the first element.
					IEnumerator<string> matches = group.GetEnumerator();

					if( matches.MoveNext() ) {
						this.ResultsList.AddDuplicateInfo( new FileDuplicateInfo( matches.Current, groupFileSize ) );
					}

					while( matches.MoveNext() ) {

						this.ResultsList.AddDuplicateInfo( new FileDuplicateInfo( matches.Current, groupFileSize ), true );
					}


				} // foreach()

			}

		} // Groups_CollectionChanged

	} // class

} // namespace