using Lemur.Windows;
using Lemur.Windows.Converters;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	/// <summary>
	/// TODO: Apply ViewModel to MainMenu Window.
	/// </summary>
	public class MainVM : ViewModelBase {

		#region MAIN SECTION NAMES

		static public readonly string SortFiles = "SortFilesSection";
		static public readonly string FindCopies = "FindCopiesSection";
		static public readonly string CustomSearch = "CustomSearchSection";
		static public readonly string CleanFolders = "CleanFoldersSection";

		#endregion

		#region COMMANDS

		private RelayCommand _cmdSizeChanged;
		public RelayCommand CmdSizeChanged {
			get {
				return this._cmdSizeChanged ?? ( this._cmdSizeChanged = new RelayCommand(
					this.WinSizeChanged
				) );
			}
		}

		private RelayCommand _cmdExit;
		public RelayCommand CmdExit {
			get {
				return this._cmdExit ?? ( this._cmdExit = new RelayCommand(
					System.Windows.Application.Current.Shutdown
				) );
			}
		}

		private RelayCommand _cmdShowSettings;
		public RelayCommand CmdShowSettings {
			get {
				return this._cmdShowSettings ?? ( this._cmdShowSettings = new RelayCommand( App.Instance.ShowSettings ) );
			}
		}


		private RelayCommand<string> _cmdTabSelected;
		public RelayCommand<string> CmdTabSelected {
			get {
				return this._cmdTabSelected ?? ( this._cmdTabSelected = new RelayCommand<string>( this.SelectTab ) );
			}
		}

		public FileSortModel FileSortVM { get => fileSortVM; set => fileSortVM = value; }
		public CustomSearchVM CustomSearchVM { get => customSearchVM; set => customSearchVM = value; }
		public CleanFoldersModel CleanFoldersVM { get => cleanFoldersVM; set => cleanFoldersVM = value; }
		public FindCopiesVM FindCopiesVM { get => findCopiesVM; set => findCopiesVM = value; }

		#endregion

		#region VIEW MODELS

		private FileSortModel fileSortVM;
		private CustomSearchVM customSearchVM;
		private CleanFoldersModel cleanFoldersVM;
		private FindCopiesVM findCopiesVM;


		#endregion

		#region PROPERTIES

		private double mainViewHeight;
		public double Height {

			get { return this.mainViewHeight; }

			set {

				if( SetProperty( ref mainViewHeight, value ) ) {
				}

			}
	
		}

		private double mainViewWidth;
		public double Width {
			get { return this.mainViewWidth; }
			set {
				SetProperty( ref mainViewWidth, value );
			}

		}


		private string _lastSelectedTab;
		public string LastSelectedTab {

			get {
				return this._lastSelectedTab;
			}
			set {

				if( this.SetProperty( ref this._lastSelectedTab, value ) ) {

					if( Properties.Settings.Default.saveLastView ) {
						Properties.Settings.Default.lastView = this._lastSelectedTab;
						Properties.Settings.Default.Save();
					}
				}

			} //set
		}

		#endregion

		public MainVM() {

			if( Properties.Settings.Default.saveLastView ) {

				this._lastSelectedTab = Properties.Settings.Default.lastView;
				this.RestoreLastSize();

			} else {
				this._lastSelectedTab = string.Empty;
			}

		} //

		/// <summary>
		/// Bindings are called _before_ the event. I have checked this.
		/// </summary>
		private void WinSizeChanged() {

			//Console.WriteLine( "WIN SIZE CHANGED" );
			string winSize = this.Width.ToString() + ',' + this.Height.ToString();
			Properties.Settings.Default.lastViewSize = winSize;
			Properties.Settings.Default.Save();

		} // WinSizeChanged()

		private void RestoreLastSize() {

			string winSize = Properties.Settings.Default.lastViewSize;
			if( !string.IsNullOrEmpty( winSize ) ) {

				string[] coords = winSize.Split( ',' );

				if( coords.Length > 0 ) {

					int width;
					if( int.TryParse( coords[0], out width ) ) {
						this.Width = width;
					}
					int height;
					if( coords.Length > 1 && int.TryParse( coords[1], out height ) ) {
						this.Height = height;
					}

				}

			} //

		} // RestoreWindowSize()

		private void SelectTab( string tabName ) {
		} //


    } // class

} // namespace
