using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LerpingLemur.Utils;
using System.ComponentModel.Design;
using TorboFile.Windows;
using TorboFile.Services;
using LerpingLemur.Tasks;
using System.IO;

using static LerpingLemur.Debug.DebugUtils;
using LerpingLemur.Windows.MVVM;
using TorboFile.Categories;
using TorboFile.ViewModels;
using TorboFile.Properties;
using LerpingLemur.Windows;

namespace TorboFile {

	/// <summary>
	/// Interaction logic for App.xaml
	/// 
	/// Must be Disposable for the ServiceContainer.
	/// 
	/// </summary>
	public partial class App : Application, IDisposable {

		// <categories:CategoryManager x:Name="categoryManager" x:Key="{x:Static local:Constants.CategoryManager}" />
		private CategoryManager categoryManager;

		static private App _Instance;
		static public App Instance {
			get { return App._Instance; }
		}

		private ServiceContainer services;
		private List<string> tempFiles;

		public CategoryManager CategoryManager {
			get { return this.categoryManager; }
		}

		/*private RelayCommand<string> _cmdOpenExternal;
		/// <summary>
		/// Opens a file externally.
		/// </summary>
		public RelayCommand<string> CmdOpenExternal {
			get {
				return this._cmdOpenExternal ?? ( this._cmdOpenExternal = new RelayCommand<string>(
					this.OpenExternal
				));
			}
		}*/

		/// <summary>
		// Attempts to open a file externally. All Exceptions are caught and ignored.
		/// </summary>
		/// <param name="path"></param>
		public async void OpenExternalAsync( string path ) {

			await Task.Run( () => {
				if( string.IsNullOrEmpty( path ) ) {
					return;
				}
				try {
					System.Diagnostics.Process.Start( path );
				} catch( Exception ) {
				}
			} );

		}

		/// <summary>
		/// Attempt to open a list of files externally.
		/// All exceptions are caught by the function.
		/// </summary>
		/// <param name="paths"></param>
		public async void OpenExternalAsync( string[] paths ) {

			await Task.Run( () => {

				foreach( string path in paths ) {
					if( string.IsNullOrEmpty( path ) ) {
						continue;
					}
					if( !File.Exists( path ) ) {
						continue;
					}
					try {
						System.Diagnostics.Process.Start( path );
					} catch( Exception ) {
					}
				}

			}

			);

		}

		private void Application_Startup( object sender, StartupEventArgs e ) {

			App._Instance = this;
			this.categoryManager = CategoryManager.Instance;
			this.tempFiles = new List<string>();

			ViewModelBase.CloseView = TryCloseView;

			this.InitServices();

			this.CreateMainWindow();

		} //

		/// <summary>
		/// Attempt to close a ViewModel window.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private bool TryCloseView( ViewModelBase model ) {

			FrameworkElement element = model.ViewElement;
			if( element != null ) {

				if( element is Window ) {

					( (Window)element ).Close();
					return true;

				} else {
					Window.GetWindow( element ).Close();
				}


			}
			// who knows?
			return false;

		}

		/// <summary>
		/// Initialize services available to the application models.
		/// </summary>
		private void InitServices() {

			this.services = new ServiceContainer();

			this.services.AddService( typeof( IMessageBox ), new ServiceCreatorCallback(
				( container, type ) => { return new CustomMessageBox(); }
			) );

			this.services.AddService( typeof( FileDialogService ), new ServiceCreatorCallback( ( container, type ) => {
				return new FileDialogService();
			} ) );

			this.services.AddService( typeof( InputBinder ), new ServiceCreatorCallback( ( container, type ) => {
				return new InputBinder();
			}
			) );

			this.services.AddService( typeof( FileDeleteService ), new ServiceCreatorCallback( ( container, type ) => {
				return new FileDeleteService();
			} ) );

		}

		private void CreateMainWindow() {

			MainWindow main = new MainWindow();

			this.InitView( main.FindDuplicatesView, new FindDuplicatesModel() );
			this.InitView( main.SortFilesView, new FileSortModel( this.services, this.CategoryManager ) );
			this.InitView( main.CleanFoldersView, new CleanFoldersModel() );

			this.MainWindow = main;

			main.Show();

		}

		public void InitView( FrameworkElement view ) {

			object context = view.DataContext;
			if( context != null ) {

				ViewModelBase baseContext = view.DataContext as ViewModelBase;
				if( baseContext != null ) {
					InitView( view, baseContext );
				}

			}

		}

		public void InitView( FrameworkElement view, ViewModelBase viewModel, ViewModelBase parentModel=null ) {

			if( viewModel != null ) {

				viewModel.ServiceProvider = this.services;
				viewModel.ViewElement = view;
				Console.WriteLine( "setting context for: " + view.Name );
				view.DataContext = viewModel;

			}

			Window topWindow;
			if( parentModel != null && view is Window ) {
				topWindow = Window.GetWindow( parentModel.ViewElement );
			} else {
				topWindow = this.MainWindow;
			}
			Window myWindow = Window.GetWindow( view );
			if( myWindow != topWindow ) {
				myWindow.Owner = topWindow;
			}

		}

		/// <summary>
		/// Display the Settings dialog window.
		/// </summary>
		public void ShowSettings() {

			SettingsWindow settings = GetWindow<SettingsWindow>();
			this.InitView( settings.CategoryManagerView, new CategoryManagerVM( this.categoryManager ) );
			settings.Show();

		}

		/// <summary>
		/// Searches the current windows for a window of type T,
		/// and returns it if found.
		/// If no window of type T exists, a new one is created
		/// and returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetWindow<T>() where T : Window {

			foreach( Window win in App.Current.Windows ) {

				if( win is T ) {
					win.Show();
					return (T)win;
				}
	
			}

			return Activator.CreateInstance<T>();

		} // GetWindow<T>()

		public void ShowCategoryManager( ViewModelBase modelCaller ) {

			CategoryManagerWindow win = this.GetWindow<CategoryManagerWindow>();
			this.InitView( win, new CategoryManagerVM( this.categoryManager ), modelCaller );

			win.Show();

		}

		/// <summary>
		/// Display an EditCategorySet Window.
		/// </summary>
		/// <param name="parentMode"></param>
		/// <param name="editing">The CategorySet to edit, or null if a new set is being created.</param>
		public void ShowEditCategorySet( ViewModelBase parentModel, string editing=null ) {

			EditCategorySetVM editModel;

			/// check for existing window.

			EditCategorySetWindow win = this.GetWindow<EditCategorySetWindow>();

			editModel = win.CategorySetView.DataContext as EditCategorySetVM;
			if( editModel != null ) {
				editModel.CategoryManager = this.categoryManager;
				editModel.EditingName = editing;
			}

			this.InitView( win, editModel, parentModel );

			win.Show();

		}

		/// <summary>
		/// Display a window for creating a new File Sorting Category.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="editing">The FileCategory being edited, or null if creating a new Category.</param>
		public void EditCategoryDialog( ViewModelBase parentModel, FileCategory editing=null ) {

			EditCategoryModel newCategory = new EditCategoryModel( this.CategoryManager.Current, editing );
			EditCategoryWindow win = new EditCategoryWindow();

			this.InitView( win, newCategory, parentModel );

			win.Show();

		} //

		static public ProgressWindow MakeProgressWindow( ProgressOperation operation ) {

			ProgressWindow win = new ProgressWindow();
			win.AutoClose = false;

			win.Show();

			return win;

		}

		/// <summary>
		/// Runs an operation asynchronously, with optional progress bar.
		/// </summary>
		/// <param name="operation"></param>
		/// <param name="showProgressWindow"></param>
		/// <returns></returns>
		static public async Task RunOperationAsync( ProgressOperation operation ) {

			try {

				await Task.Run( (Action)operation.Run );

			} catch( Exception e ) {

				Log( e.ToString() );

			} finally {
				operation.Dispose();
			}

		} // RunOperationAsync()

		static public string MakeTempCopy( string source ) {

			if( string.IsNullOrEmpty( source ) ) {
				return string.Empty;
			}

			string tmp = Path.GetTempFileName();

			byte[] contents = File.ReadAllBytes( source );
			using( FileStream file = File.OpenWrite( tmp ) ) {

				file.Write( contents, 0, contents.Length );

			}

			_Instance.tempFiles.Add( tmp );

			return tmp;

		}

		static public void DeleteTempFiles() {
			App._Instance.DeleteTemps();
		}

		public void DeleteTemps() {

			try {

				foreach( string path in this.tempFiles ) {

					if( File.Exists( path ) ) {
						File.Delete( path );
					}

				}

			} catch( Exception ) {
			}

		}

		public void Dispose() {

			if( this.services != null ) {
				this.services.Dispose();
			}

		}

		/// <summary>
		/// Called when the Application is about to exist.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Application_Exit( object sender, ExitEventArgs e ) {

			/// Attempt to save any changes to the current CategorySet.
			this.SaveCurCategory();

			// save changes to settings.
			Settings.Default.Save();
			FindCopiesSettings.Default.Save();
			FolderCleanSettings.Default.Save();
			SortingSettings.Default.Save();

			this.DeleteTemps();

		}

		/// <summary>
		/// Attempts to save any changes to the current CategorySet.
		/// </summary>
		public void SaveCurCategory() {

			CategoryManager manager = this.categoryManager;// (CategoryManager)this.TryFindResource( Constants.CategoryManager );
			if( manager != null ) {
				manager.SaveCurrent();
			}

		}

		/// <summary>
		/// Catches any unhandled exceptions in the program.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Application_DispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {

			Exception err = e.Exception;
			if( err is UnauthorizedAccessException ) {

				MessageBoxResult result = MessageBox.Show(
					"The program does not have the appropriate permissions." + " Running the program in Administrator mode might solve the problem.",
							"Unauthorized access." );

				if( result == MessageBoxResult.OK ) {
				} else if( result == MessageBoxResult.Cancel ) {
				}

				e.Handled = true;

			} else if( err is null ) {
			} else {
				MessageBox.Show( "An unknown error has occured: " + err.ToString() );
			}

		} // Application_DispatcherUnhandledException()

	} // class

} // namespace
