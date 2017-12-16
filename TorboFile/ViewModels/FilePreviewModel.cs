using LerpingLemur.Windows;
using LerpingLemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Model for previwing a File.
	/// </summary>
	public class FilePreviewModel : ViewModelBase {

		#region COMMANDS

		private RelayCommand _cmdOpenExternal;
		/// <summary>
		/// Open the previewed file in the System explorer.
		/// </summary>
		public RelayCommand CmdOpenExternal {
			get { return this._cmdOpenExternal ?? ( this._cmdOpenExternal = new RelayCommand(
				() => {

					App.Instance.OpenExternalAsync( this.FilePath );

				} ) );
			}
		} //

		private RelayCommand _cmdShowExternal;
		/// <summary>
		/// Opens the previewed file in Explorer.
		/// </summary>
		public RelayCommand CmdShowExternal {
			get {
				return this._cmdShowExternal ?? ( this._cmdShowExternal = new RelayCommand(
			  () => {

				  AppUtils.ShowExternalAsync( this.FilePath );

			  } ) );
			}
		}

		/// <summary>
		/// Command to call when preview of a file failed to load
		/// or decode successfully.
		/// </summary>
		private RelayCommand _cmdPreviewFailed;
		public RelayCommand CmdPreviewFailed {

			get {
				return this._cmdPreviewFailed ?? ( this._cmdPreviewFailed = new RelayCommand(
					() => {
						this.MimeRoot = MimeUtils.Unknown;
					}
				) );
	
			}

		}

		private RelayCommand<string> _cmdViewAs;
		/// <summary>
		/// Changes the MimeRoot to attempt viewing the file as a different type.
		/// </summary>
		public RelayCommand<string> CommandViewAs {
			get { return this._cmdViewAs ?? ( this._cmdViewAs = new RelayCommand<string>(

				( string s ) => {
					this.MimeRoot = s; }

			) ); }
		}

		#endregion

		private string mimeStem;
		/// <summary>
		/// Stem portion of a Mimetype: root/stem
		/// </summary>
		public string MimeStem {
			get { return this.mimeStem; }
			set {
				if( value != this.mimeStem ) {
					this.mimeStem = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		private string mimeRoot;
		/// <summary>
		/// Root portion of a mime-type: root/stem
		/// </summary>
		public string MimeRoot {
			get {
				return this.mimeRoot;
			}
			set {
				if( value != this.mimeRoot ) {

					this.mimeRoot = value;
					//Console.WriteLine( "Mime Change notification." );
					this.NotifyPropertyChanged();
				}
				
			}

		}

		private string mime;
		/// <summary>
		/// The complete mime-type string of the file being viewed.
		/// </summary>
		public string FileMime {
			get {
				return this.mime;
			}
			set {
				if( value != this.mime ) {

					this.mime = value;
					if( string.IsNullOrEmpty( value ) ) {
					
						this.MimeRoot = this.MimeStem = string.Empty;
					} else {

						string[] parts = value.Split( MimeUtils.MIME_SLASH );
						if( parts.Length >= 1 ) {
							this.MimeStem = parts[1];
						}
						this.MimeRoot = parts[0];

					}

				}
				this.NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Returns the extension portion of the file name.
		/// </summary>
		public string Extension {
			get { return Path.GetExtension( this._filePath ); }
		}

		/// <summary>
		/// Returns the fileName portion of the file path.
		/// </summary>
		public string FileName {
			get {
				return Path.GetFileName( this._filePath );
			}
		}

		/*private bool _triggerClose;
		public bool TriggerClose {
			get { return this._triggerClose; }
			set {
				if( value == true ) {
					this._triggerClose = true;
					this.NotifyPropertyChanged();
					this._triggerClose = false;
				}
			}
		}*/

		/// <summary>
		/// Name of the FilePath property.
		/// </summary>
		public const string FilePathPropertyName = "FilePath";

		/// <summary>
		/// Path to the file being previewed.
		/// </summary>
		private string _filePath;

		/// <summary>
		/// File path of the file being previewed.
		/// Setting the path attempts to update the preview with the new file.
		/// </summary>
		public string FilePath {
			get {
				return this._filePath;
			}
			set {

				if( value != this._filePath ) {
	
					this._filePath = value;
					this.FileMime = MimeUtils.GetFileMime( this._filePath, null );

					//Console.WriteLine( "FilePreviewModel: File Path: " + value );

					this.NotifyPropertyChanged( "FileName" );
					this.NotifyPropertyChanged();

					if( this.MimeRoot == MimeUtils.Text ) {
						this.NotifyPropertyChanged( "Text" );
					}
				}

			} // set()
		}

		/// <summary>
		/// Returns the text of the current file.
		/// </summary>
		public string Text {
			get { return this.TryReadFile(); }
		}

		private string TryReadFile() {

			if( string.IsNullOrEmpty( this._filePath ) ) {
				return string.Empty;
			}

			try {

				string text = File.ReadAllText( this._filePath );
				return text;

			} catch( Exception ) {
			}

			return string.Empty;

		}

	} // class

} // namespace