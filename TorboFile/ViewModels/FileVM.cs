using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lemur.Windows;

namespace TorboFile.ViewModels {

	/// <summary>
	/// CURRENTLY UNFINISHED.
	/// Wraps a string file path with additional properties.
	/// Supposed to be more versatile than FileSystemInfo.
	/// </summary>
	public class FileVM : DataObjectVM {

		#region COMMANDS

		private RelayCommand _cmdOpenExternal;
		/// <summary>
		/// Open the previewed file in the System explorer.
		/// </summary>
		public RelayCommand CmdOpenExternal {
			get {
				return this._cmdOpenExternal ?? ( this._cmdOpenExternal = new RelayCommand(
			  async () => {

				  await AppUtils.OpenExternalAsync( this.FullPath );

			  } ) );
			}
		} //

		private RelayCommand _CmdShowLocation;
		/// <summary>
		/// Opens the previewed file in Explorer.
		/// </summary>
		public RelayCommand CmdShowLocation {
			get {
				return this._CmdShowLocation ?? ( this._CmdShowLocation = new RelayCommand(
			  () => {

				  AppUtils.ShowExternalAsync( this.FullPath );

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
			get {
				return this._cmdViewAs ?? ( this._cmdViewAs = new RelayCommand<string>(

			  ( string s ) => {
				  this.MimeRoot = s;
			  }

		  ) );
			}
		}

		#endregion

		#region PROPERTIES

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
		private string mimeStem;

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
		private string mimeRoot;

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
		private string mime;

		/// <summary>
		/// Returns the extension portion of the file name.
		/// </summary>
		public string Extension {
			get { return Path.GetExtension( this._fullPath ); }
		}

		/// <summary>
		/// Returns the fileName portion of the file path.
		/// </summary>
		public string FileName {
			get {
				return Path.GetFileName( this._fullPath );
			}
		}

		#endregion

		/// <summary>
		/// Name of the FilePath property.
		/// </summary>
		public const string FilePathPropertyName = "FilePath";
		

		/// <summary>
		/// Complete path to the file or directory.
		/// </summary>
		public string FullPath {
			get {
				return this._fullPath;
			}
			set {

				if( value != this._fullPath ) {

					this._fullPath = value;
					this.FileMime = MimeUtils.GetFileMime( this._fullPath, null );

					this.NotifyPropertyChanged( "FileName" );
					this.NotifyPropertyChanged();

					if( this.MimeRoot == MimeUtils.Text ) {
						this.NotifyPropertyChanged( "Text" );
					}
				}

			} // set()

		}

		private string _fullPath;

		/// <summary>
		/// Underlying file attributes.
		/// </summary>
		private FileAttributes _attrs;
		/// <summary>
		/// Mark that attributes were read.
		/// </summary>
		private bool _readAttrs;

		public long Size {

			get {

				long size;
				if( !this.TryGetCache( "Size", out size ) ) {

					if( this.IsDirectory ) {
						size = 0;
					} else {
						size = new FileInfo( this._fullPath ).Length;
					}
					
					this.CacheProp( "Size", size );

				}
				return size;

			}

		}

		/// <summary>
		/// IsFile Property.
		/// </summary>
		public bool IsFile {

			get {

				if( !this._readAttrs ) {
					this.ReadAttributes();
				}
				return !this._attrs.HasFlag( FileAttributes.Directory );

			}

		}

		/// <summary>
		/// IsDirectory Property.
		/// </summary>
		public bool IsDirectory {

			get {

				if( !this._readAttrs ) {
					this.ReadAttributes();
				}
				return this._attrs.HasFlag( FileAttributes.Directory );

			}

		}

		/// <summary>
		/// Returns the text of the current file.
		/// </summary>
		public string Text {

			get {

				string text;
				if( !this.TryGetCache( "Text", out text ) ) {
					text = this.ReadText();
					this.CacheProp( "Text", text );
				}

				return text;

			}

		}

		private void ReadAttributes() {
			this._attrs = File.GetAttributes( this._fullPath );
			this._readAttrs = true;
		}

		private string ReadText() {

			if( string.IsNullOrEmpty( this._fullPath ) ) {
				return string.Empty;
			}

			try {

				string text = File.ReadAllText( this._fullPath );
				return text;

			} catch( Exception ) {
			}

			return string.Empty;

		}

		public FileVM( string path ) : base( path, true ) {

			this._fullPath = path;

		}

	} // class

} // namespace
