using TorboFile.Categories;
using Lemur.Windows.MVVM;
using Lemur.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using TorboFile.Services;
using static Lemur.Debug.DebugUtils;
using System.Windows.Input;
using System.Collections.Specialized;
using Lemur.Operations;
using System.Diagnostics;
using Lemur.Windows.Services;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Used to control the display of files previewed in the FileSort portion of the app.
	/// 
	/// Made IDisposable for the DirectoryVisitor which listens to changes in the current Directory.
	/// </summary>
	public class FileSortVM : ViewModelBase, IDisposable {

		~FileSortVM() {
			DebugDestructor();
		}
		[Conditional( "DEBUG" )]
		void DebugDestructor() {
			Console.WriteLine( @"FILE SORT MODEL DESTRUCTOR" );
		}

		#region COMMANDS

		private RelayCommand<FileCategory> _cmdCategoryClick;
		/// <summary>
		/// Attempts to move the currently viewed file into the given FileCategory.
		/// </summary>
		public RelayCommand<FileCategory> CmdCategoryClick {
			get {
				return this._cmdCategoryClick ?? ( this._cmdCategoryClick = new RelayCommand<FileCategory>(
					async (c)=> {
						//Console.WriteLine( "Category Click: " + c.Name );
						await this.CategoryClicked(c); } )
					);
			}
		}

		private RelayCommand _cmdDirBrowser;
		/// <summary>
		/// Shows the directory browser.
		/// </summary>
		public RelayCommand CmdPickDirectory {
			get {
				return this._cmdDirBrowser ?? (this._cmdDirBrowser = new RelayCommand( this.ShowDirectoryBrowser ) );
			}
		}

		private RelayCommand<string> _cmdSetDirectory;
		/// <summary>
		/// Sets the directory to the given path.
		/// </summary>
		public RelayCommand<string> CmdSetDirectory {
			get {
				return this._cmdSetDirectory ?? ( this._cmdSetDirectory = new RelayCommand<string>( this.SetDirectory) );
			}
		}

		private RelayCommand _CmdShowLocation;
		public RelayCommand CmdShowLocation {

			get {
				return this._CmdShowLocation ?? ( this._CmdShowLocation = new RelayCommand(
					() => { AppUtils.ShowExternalAsync( this.CurrentPath ); }
				) );
			}

		}


		private RelayCommand _cmdNewCategory;
		/// <summary>
		/// Creates a new File Category.
		/// </summary>
		public RelayCommand CmdNewCategory {
			get {
				return this._cmdNewCategory ?? ( this._cmdNewCategory = new RelayCommand(
					() => { App.Instance.EditCategoryDialog( this ); }
				) );
			}
		}

		private RelayCommand _cmdDeleteFile;
		/// <summary>
		/// Deletes the file being viewed.
		/// </summary>
		public RelayCommand CmdDeleteFile {
			get {
				return this._cmdDeleteFile ?? ( this._cmdDeleteFile = new RelayCommand( this.DeleteCurrentFile ) );
			}
		}

		private RelayCommand _cmdCategoryManager;
		/// <summary>
		/// Displays the CategoryManager.
		/// </summary>
		public RelayCommand CmdShowManager {
			get {
				return this._cmdCategoryManager ?? (
					this._cmdCategoryManager = new RelayCommand( ()=> { App.Instance.ShowCategoryManager( this ); } )
				);
			}
		}

		private RelayCommand _cmdPrevFile;
		/// <summary>
		/// Moves to the previous file in the current directory.
		/// </summary>
		public RelayCommand CmdPrevFile {
			get { return this._cmdPrevFile ?? ( this._cmdPrevFile = new RelayCommand( this.PrevFile ) ); }
		}

		private RelayCommand _cmdNextFile;
		/// <summary>
		/// Moves to the next file in the current directory.
		/// </summary>
		public RelayCommand CmdNextFile {
			get { return this._cmdNextFile ?? ( this._cmdNextFile = new RelayCommand( this.NextFile ) ); }
		}

		#endregion

		#region PROPERTIES

		private FilePreviewVM _filePreview;
		public FilePreviewVM FilePreview {
			get {
				return this._filePreview;
			}
			set {
				this._filePreview = value;
				this.NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Current file being viewed.
		/// </summary>
		public FileInfo CurrentFile {
			get { return this.visitor.Current; }
		} //

		/// <summary>
		/// File path of the current file being viewed.
		/// </summary>
		public string CurrentPath {
			get {
				return ( this.CurrentFile == null ) ? string.Empty : this.CurrentFile.FullName;
			}
		}

		/// <summary>
		/// Visits files within the current directory.
		/// </summary>
		private DirectoryVisitor visitor;
		public DirectoryVisitor FileList {

			get { return this.visitor; }

		} //

		private CategorySet _categorySet;
		public CategorySet CategorySet {

			get { return this._categorySet; }
			set {

				if( this._categorySet == value ) { return; }

				if( this._categorySet != null ) {
					this._categorySet.CollectionChanged -= CategorySetUpdated;
				}

				// clear any existing keybindings.
				this.ClearKeyBindings();
				this._categorySet = value;

				if( value != null ) {
					this._categorySet.CollectionChanged += CategorySetUpdated;
					Console.WriteLine( "Sort: CategorySet Property Set" );
					this.ResetKeyBindings();
				}

				this.NotifyPropertyChanged();

			}

		} //

		#endregion

		private void CategorySetUpdated( object sender, NotifyCollectionChangedEventArgs e ) {

			InputBinder binder = this.GetService<InputBinder>();
			if( binder == null ) {
				Console.WriteLine( @"ERROR: Can't find InputBinder service." );
				return;
			}

			switch( e.Action ) {

				case NotifyCollectionChangedAction.Remove:

					foreach( FileCategory cat in e.OldItems ) {
						binder.RemoveBinding( this, cat.Gesture );
					}
					break;

				case NotifyCollectionChangedAction.Add:

					Console.WriteLine( @"CategorySet ADD ACTION" );
					foreach( FileCategory cat in e.NewItems ) {
						InputBinding b = this.CategorySet.MakeCategoryBinding( this.CmdCategoryClick, cat );
						if( b != null ) {
							binder.AddBinding( this, b );
						}
					}
					break;

				case NotifyCollectionChangedAction.Replace:

					foreach( FileCategory cat in e.OldItems ) {
						binder.RemoveBinding( this, cat.Gesture );
					}
					foreach( FileCategory cat in e.NewItems ) {

						InputBinding b = this.CategorySet.MakeCategoryBinding( this.CmdCategoryClick, cat );
						if( b != null ) {
							binder.AddBinding( this, b );
						}

					}

					break;
				case NotifyCollectionChangedAction.Reset:
					Console.WriteLine( @"RESET COLLECTION ACTION" );
					this.ClearKeyBindings();
					this.ResetKeyBindings();
					break;

			}

		} //

		public FileSortVM( IServiceProvider services, CategoryManager manager ) : base( services ) {

			manager.PropertyChanged += Manager_PropertyChanged;
			this.CategorySet = manager.Current;

			this.FilePreview = new FilePreviewVM();

			this.visitor = new DirectoryVisitor( string.Empty );
			this.visitor.PropertyChanged += this.DirectoryPropertyChanged;
			this.visitor.SetDirectory( Properties.SortingSettings.Default.LastDirectory );

			// Used to listen for changes to the underlying view to reset the bindings.
			// Necessary because the view isn't linked when the constructor is called.
			// TODO: this isn't very good.
			this.PropertyChanged += this.FileSortVM_PropertyChanged;


		}

		/// <summary>
		/// Sets up basic keybindings for the UI.
		/// </summary>
		private void SetUIKeys() {

			InputBinder binder = this.GetService<InputBinder>();
			if( binder != null ) {
				binder.AddBinding( this, new KeyBinding( this.CmdPrevFile, new KeyGesture( Key.Left ) ) );
				binder.AddBinding( this, new KeyBinding( this.CmdNextFile, new KeyGesture( Key.Right ) ) );
			}

		} //

		private void FileSortVM_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			if( e.PropertyName == @"ViewElement" ) {
				Console.WriteLine( "Sort: ViewElement changed" );
				this.ClearKeyBindings();
				this.SetUIKeys();
				this.ResetKeyBindings();
			}

		}

		/// <summary>
		/// CategoryManager Property changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Manager_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			if( e.PropertyName == CategoryManager.CurrentSetPropName ) {
				Console.WriteLine( @"FileSort: CategorySet changed." );
				this.CategorySet = ( (CategoryManager)sender ).Current;
			}

		} //

		private void ClearKeyBindings() {

			InputBinder binder = this.GetService<InputBinder>();
			if( binder != null ) {
				binder.RemoveBindings( this );
			}

		}

		/// <summary>
		/// Initialize bindings of Category keys to file sorting.
		/// </summary>
		private void ResetKeyBindings() {

			Console.WriteLine( "RESET CATEGORY KEY BINDINGS" );
			if( this._categorySet != null ) {

				InputBinder binder = this.GetService<InputBinder>();

				if( binder != null ) {
					IEnumerable<InputBinding> bindings = this._categorySet.MakeCategoryBindings( this.CmdCategoryClick );
					binder.AddBindings( this, bindings );
				} else {
					Console.WriteLine( "FileSortVM.ResetKeyBindings(): Error: No InputBinder" );
				}

			} else {
				Console.WriteLine( "CategorySet NULL" );
			}

		}

		private void DirectoryPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {

			if( e.PropertyName == @"CurrentDirectory" ) {
				//Log( "CHANGING DIRECTORY" );
			} else if( e.PropertyName == "Current" ) {

				Log( "Updating FILE PREVIEW: " + this.visitor.CurrentPath );
				this.FilePreview.FilePath = this.visitor.CurrentPath;
				//this.CurrentMimeType = this._filePreview.MimeRoot;

			}

		} //

		/// <summary>
		/// Delete the currently selected file, if any.
		/// </summary>
		private void DeleteCurrentFile() {

			IMessageBox messageBox = (IMessageBox) this.ServiceProvider.GetService( typeof(IMessageBox) );
			if( messageBox.ShowConfirm( this, @"Confirm Delete", @"Delete current file?" ) == MessageResult.Accept ) {

				FileDeleteService deleter = this.GetService<FileDeleteService>();
				if( deleter != null ) {
					deleter.DeleteFileAsync( this.visitor.Current.FullName,
						Properties.SortingSettings.Default.moveToTrash );
				}

				// should not need:
				//this.visitor.RemoveCurrent();

			}

		}

		/// <summary>
		/// Button clicked to change directory.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDirectoryBrowser() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string new_dir = dialog.PickFolder( @"Select Directory...", this.visitor.CurrentDirectory );
				if( !string.IsNullOrEmpty( new_dir ) ) {
					this.SetDirectory( new_dir );
				}

			}

		} //

		private async Task CategoryClicked( FileCategory category ) {

			FileInfo info = this.CurrentFile;
			if( info == null ) {
				Console.WriteLine( "Move File: No Current File" );
				return;
			}

			try {

				/// close all uses of the file, if possible.
				string old_path = info.FullName;
				if( Path.GetDirectoryName( old_path ) == category.DirectoryPath ) {
					// Source and Dest directories are equal.
					Console.WriteLine( "Source and Dest dirs are equal." );
					return;
				}

				string new_path = category.GetMovePath( old_path );

				this.visitor.Prev();

				FileOperation op = new FileOperation( old_path, category.GetMovePath(old_path), 10*1000 );
				bool success = await op.RunAsync();

				/*if( success ) { /// Should auto-remove.
					this.visitor.Remove( info );
				}*/

			} catch( Exception e ) {
				Log( e.ToString() );
			}

		} //

		/// <summary>
		/// Sets the directory to the given file path.
		/// </summary>
		/// <param name="path"></param>
		private void SetDirectory( string path ) {

			if( string.IsNullOrEmpty( path ) ) {
				return;
			}
			Properties.SortingSettings.Default.LastDirectory = path;
			this.visitor.SetDirectory( path );

		}

		public void NextFile() {

			this.visitor.Next();
			//this.OnFileChanged();

		} // NextFile()

		public void PrevFile() {

			this.visitor.Prev();
			//this.OnFileChanged();

		} // PrevFile()

		public void Dispose() {

			if( this.visitor != null ) {
				this.visitor.Dispose();
			}

		}

	} // class

} // namespace