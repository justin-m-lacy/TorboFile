
using TorboFile.Properties;
using TorboFile.Services;
using Lemur.Types;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using Lemur.Windows.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Lemur.Operations.FileMatching;
using Lemur.Operations;
using System.Diagnostics;
using Lemur.Windows.Services;

namespace TorboFile.ViewModels {

	public class CleanFoldersVM : ViewModelBase {

		#region DEBUG

		~CleanFoldersVM() {
			DebugDestructor();
		}
		[Conditional( "DEBUG" )]
		void DebugDestructor() {
			Console.WriteLine( @"CLEAN FOLDERS MODEL DESTRUCTOR" );
		}

		#endregion
	
		#region COMMANDS

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

		private RelayCommand _cmdPickFolder;
		public RelayCommand CmdPickFolder {
			get {

				return this._cmdPickFolder ?? ( this._cmdPickFolder = new RelayCommand(

					this.PickFolder

				) );
			}

		}

		#endregion

		#region PROPERTIES

		public string SearchDirectory {
			get {
				return Properties.CleanFolderSettings.Default.LastDirectory;
			}
			set {

				if( value != Properties.CleanFolderSettings.Default.LastDirectory ) {

					Properties.CleanFolderSettings.Default.LastDirectory = value;
					this.NotifyPropertyChanged();
					this._cmdBeginSearch.RaiseCanExecuteChanged();

					if( !Directory.Exists( value ) ) {
						throw new ValidationException( @"Error: Target directory not found." );

					}

				}

			}

		} // SearchDirectory

		public bool DeleteEmptyFolders {
			get => Properties.CleanFolderSettings.Default.deleteEmptyFolders;
			set {
				if( value != this.DeleteEmptyFolders ) {
					Properties.CleanFolderSettings.Default.deleteEmptyFolders = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Whether to remove files of size zero.
		/// </summary>
		public bool DeleteEmptyFiles {
			get { return Properties.CleanFolderSettings.Default.deleteEmptyFiles; }
			set {
				if( value != this.DeleteEmptyFiles ) {
					Properties.CleanFolderSettings.Default.deleteEmptyFiles = value;
					this.NotifyPropertyChanged();
				}
			} // set
		}

		/// <summary>
		/// Whether to remove files in a given file size range.
		/// </summary>
		public bool UseSizeRange {
			get { return Properties.CleanFolderSettings.Default.hasDeleteRange; }
			set {
				if( value != this.UseSizeRange ) {
					Properties.CleanFolderSettings.Default.hasDeleteRange = value;
					this.NotifyPropertyChanged();
				}
			} // set
		}

		public string MinSize {

			get {

				DataRange range = CleanFolderSettings.Default.deleteRange;
				if( range != null ) {
					return range.MinSize.ToString();
				}
				return string.Empty;
			}

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
			get { return CleanFolderSettings.Default.deleteRange.MaxSize.ToString(); }
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
			get { return CleanFolderSettings.Default.deleteRange; }
			set { CleanFolderSettings.Default.deleteRange = value; }
		}

		private readonly OutputVM _output = new OutputVM();
		public OutputVM Output {

			get { return this._output; }

		}

		#endregion

		public CleanFoldersVM() {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		private async Task FolderCleanAsync() {

			CleanFolderSettings appSettings = CleanFolderSettings.Default;
			string path = appSettings.LastDirectory;
			FileMatchSettings settings = new FileMatchSettings {
				UseSizeRange = appSettings.hasDeleteRange,
				SizeRange = appSettings.deleteRange, Recursive = appSettings.recursive,
				DeleteEmptyFiles = appSettings.deleteEmptyFiles, MoveToTrash = appSettings.moveToTrash
			};

			this.Output.Clear();

			using( FolderClean clean = new FolderClean( path,settings ) ) {

				clean.DeleteEmptyFolders = this.DeleteEmptyFolders;

				try {

					await Task.Run(
						() => {

							clean.Run( this.FolderDeleted );

						}
					);

					if( clean.DeletedList.Length == 0 && clean.ErrorList.Length == 0 ) {

						this.Output.Add( new TextString( "Nothing found to delete.", TextString.Message ) );
					}

				} catch( Exception e ) {

					this.Output.Add( new TextString( e.Message, TextString.Error ) );
					Console.WriteLine( e.ToString() );

				}

			}

		} //

		private void FolderDeleted( string path, bool success ) {

			Console.WriteLine( "FOLDER DELETED: " + path );
			if( success ) {
				this.AddSuccessLine( path );
			} else {
				this.AddFailLine( path );
			}

		} //
		public void AddSuccessLine( string path ) {
			this.Output.Add( new TextString( "Deleted: " + path + Environment.NewLine ) );
		}

		public void AddFailLine( string path ) {
			this.Output.Add( new TextString( "Could not delete: " + path + Environment.NewLine, TextString.Error ) );
		}

		private void PickFolder() {

			IFileDialogService dialog = (IFileDialogService)this.ServiceProvider.GetService( typeof( IFileDialogService ) );
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
