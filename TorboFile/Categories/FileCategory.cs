using Lemur.Windows.Input;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TorboFile.Categories {

	[Serializable]
	public class FileCategory : PropertyNotifier {

		private string _directory;

		/// <summary>
		/// Directory where the files in this category should be stored.
		/// </summary>
		public string DirectoryPath {
			get { return this._directory; }
			set {
				if( this._directory != value ) {
					this._directory = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Tags added to files moved to this Category.
		/// </summary>
		private List<string> _addTags;
		public List<string> AddTags {
			get { return this._addTags; }
			set { this._addTags = value; }
		}

		private string _lastFileViewed;
		/// <summary>
		/// Last file being previewed when viewing this category.
		/// If the Category is viewed again in sort mode, the viewer
		/// should attempt to open this file first, if it still exists.
		/// </summary>
		public string LastFileViewed {
			get { return this._lastFileViewed; }
			set {
				if( this._lastFileViewed != value ) {
					this._lastFileViewed = value;
				}
			}
		}

		private string _name;
		public string Name {
			get { return this._name; }
			set {
				if( this._name != value ) {
					this._name = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		private bool _relativePath;
		public bool RelativePath {
			get { return this._relativePath; }
			set {
				if( this._relativePath != value ) {
					this._relativePath = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		private FreeKeyGesture _gesture;
		/// <summary>
		/// Key combination to move an item to this category.
		/// </summary>
		public FreeKeyGesture Gesture {
			get { return this._gesture; }
			set {
				if( this._gesture != value ) {
					this._gesture = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		public FileCategory() {
		}

		public FileCategory( string name ) {

			this._addTags = new List<string>();
			this._name = name;
		}

		public FileCategory( string name, string path ) : this( name ) {
			
			this._directory = path;

		}

		public FileCategory( string name, string path, FreeKeyGesture gesture ) : this( name, path ) {

			this.Gesture = gesture;

		}

		/// <summary>
		/// Used to test for FileCategory matching without using class reference equality.
		/// Might chance as new category options are introduced.
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public bool IsEqual( FileCategory f ) {
			return ( f.Name == this.Name && f.DirectoryPath == this.DirectoryPath );
		}

		/// <summary>
		/// Get a path string for moving the given file to this file category.
		/// </summary>
		/// <param name="old_path"></param>
		/// <returns></returns>
		public string GetMovePath( string old_path ) {
			return Path.Combine( this.DirectoryPath, Path.GetFileName( old_path ) );
		}

		/// <summary>
		/// Attempts to move the given file to the Category directory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool MoveFile( string path ) {

			if( string.IsNullOrEmpty( path ) || string.IsNullOrEmpty( this.DirectoryPath ) ) {
				return false;
			}

			if( !Directory.Exists( this.DirectoryPath ) ) {
				Directory.CreateDirectory( this.DirectoryPath );
			}

			File.Move( path, Path.Combine( this.DirectoryPath, Path.GetFileName(path) ) );

			return true;

		}

	} // class

} // namespace