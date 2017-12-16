
using TorboFile.Properties;
using TorboFile.Services;
using LerpingLemur.Types;
using LerpingLemur.Windows;
using LerpingLemur.Windows.MVVM;
using LerpingLemur.Windows.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using LerpingLemur.Operations.FileMatching;

namespace TorboFile.ViewModels {

	public class CleanFoldersModel : ViewModelBase {

		private const string SUCCESS_STRING = "success";
		private const string ERROR_STRING = "error";

		private RelayCommand _cmdBeginSearch;
		public RelayCommand CmdBeginSearch {
			get {

				return this._cmdBeginSearch ?? ( this._cmdBeginSearch = new RelayCommand(

					async ()=> { await this.FolderCleanAsync(); },
					()=> {
						return Directory.Exists( this.SearchDirectory );
					}

				) );
			}

		}

		public string SearchDirectory {
			get {
				return Properties.FolderCleanSettings.Default.LastDirectory;
			}
			set {

				if( value != Properties.FolderCleanSettings.Default.LastDirectory ) {

					Properties.FolderCleanSettings.Default.LastDirectory = value;
					this.NotifyPropertyChanged();
					this._cmdBeginSearch.RaiseCanExecuteChanged();

					if( !Directory.Exists( value ) ) {
						throw new ValidationException( "Error: Target directory not found." );

					}

				}

			}

		} // SearchDirectory

		private RelayCommand _cmdPickFolder;
		public RelayCommand CmdPickFolder {
			get {

				return this._cmdPickFolder ?? ( this._cmdPickFolder = new RelayCommand(

					this.PickFolder

				) );
			}

		}

		/// <summary>
		/// Whether to remove files of size zero.
		/// </summary>
		public bool DeleteEmptyFiles {
			get { return Properties.FolderCleanSettings.Default.deleteEmptyFiles; }
			set {
				if( value != this.DeleteEmptyFiles ) {
					Properties.FolderCleanSettings.Default.deleteEmptyFiles = value;
					this.NotifyPropertyChanged();
				}
			} // set
		}

		/// <summary>
		/// Whether to remove files in a given file size range.
		/// </summary>
		public bool UseSizeRange {
			get { return Properties.FolderCleanSettings.Default.hasDeleteRange; }
			set {
				if( value != this.UseSizeRange ) {
					Properties.FolderCleanSettings.Default.hasDeleteRange = value;
					this.NotifyPropertyChanged();
				}
			} // set
		}

		public string MinSize {

			get { return FolderCleanSettings.Default.deleteRange.MinSize.ToString(); }

			set {

				DataSize newSize;
				if( DataSize.TryParse( value, out newSize ) ) {

					DataSize maxSize = this.CleanSizeRange.MaxSize;
					this.CleanSizeRange = new DataRange( newSize, maxSize );
					NotifyPropertyChanged();

				} else {
					throw new ValidationException( "Invalid data size." );
				}

			}

		}

		public string MaxSize {
			get { return FolderCleanSettings.Default.deleteRange.MaxSize.ToString(); }
			set {

				DataSize newSize;
				if( DataSize.TryParse( value, out newSize ) ) {

					DataSize minSize = this.CleanSizeRange.MinSize;
					this.CleanSizeRange = new DataRange( minSize, newSize );
					NotifyPropertyChanged();

				} else {
					throw new ValidationException( "Invalid data size." );
				}
	
			}
		}

		private DataRange CleanSizeRange {
			get { return FolderCleanSettings.Default.deleteRange; }
			set { FolderCleanSettings.Default.deleteRange = value; }
		}

		private ObservableCollection<TextString> _output;
		public ObservableCollection<TextString> Output {

			get { return this._output; }
			set {
				this._output = value;
				this.NotifyPropertyChanged();
			}

		}

		public CleanFoldersModel() {

			this._output = new ObservableCollection<TextString>();

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		private async Task FolderCleanAsync() {

			this.Output.Clear();

			FolderCleanSettings appSettings = FolderCleanSettings.Default;
			string path = appSettings.LastDirectory;


			FolderClean clean = new FolderClean( path,
				new FileMatchSettings {
					UseSizeRange = appSettings.hasDeleteRange,
					SizeRange = appSettings.deleteRange, Recursive = appSettings.recursive,
					DeleteEmptyFiles = appSettings.deleteEmptyFiles, MoveToTrash = appSettings.moveToTrash
				} );
			
			try {

				await Task.Run(
					()=> {

						clean.Run( this.FolderDeleted );
				
					}
				);

				if( clean.DeletedList.Length == 0 && clean.ErrorList.Length == 0 ) {
					this.Output.Add( new TextString( "Clean complete.", SUCCESS_STRING ) );
					this.Output.Add( new TextString( "Nothing found to delete.", SUCCESS_STRING ) );
				}

			} catch( Exception e ) {

				Console.WriteLine( e.ToString() );

			} finally {

				clean.Dispose();

			}

		} //

		private void FolderDeleted( string path, bool success ) {

			if( success ) {
				this.AddSuccessLine( path );
			} else {
				this.AddFailLine( path );
			}

		} //
		public void AddSuccessLine( string path ) {
			this.Output.Add( new TextString( "Deleted: " + path + Environment.NewLine, SUCCESS_STRING ) );
		}

		public void AddFailLine( string path ) {
			this.Output.Add( new TextString( "Could not delete: " + path + Environment.NewLine, ERROR_STRING ) );
		}

		private void PickFolder() {

			FileDialogService dialog = (FileDialogService)this.ServiceProvider.GetService( typeof( FileDialogService ) );
			if( dialog != null ) {

				string folder = this.SearchDirectory;
				folder = dialog.PickFolder( "Choose a folder to clean...", folder );

				if( !string.IsNullOrEmpty( folder ) ) {
					this.SearchDirectory = folder;
				}
			}

		} // PickFolder()

	} // class

} // namespace
