using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Categories {

	/// <summary>
	/// Lightweight wrapper for CategorySet, to display CategorySet name/location information
	/// without actually loading the set.
	/// </summary>
	public class CategorySetInfo : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public CategorySetInfo( CategorySet set, CategorySource source ) : this( set.SavePath, set.Name, source ) {

			this._set = set;
		}

		public CategorySetInfo( string path, string name, CategorySource source ) {

			this._path = path;
			this._name = name;
			this._source = source;

		}

		private CategorySet _set;
		public CategorySet Set {
			get => _set;
			set {

				if( this._set != value ) {
					this._set = value;
					this.NotifyPropertyChanged( "IsLoaded" );
					this.NotifyPropertyChanged( "Set" );
				}

			}
		}


		public bool IsLoaded {
			get { return (this._set != null); }
		}

		public void NotifyPropertyChanged( [CallerMemberName] string propName = null ) {
			this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
		}

		private string _path;
		public string Path {
			get { return this._path; }
			set {
				if( this._path != value ) {
					this._path = value;
					this.NotifyPropertyChanged();
				}
			}

		}

		private bool _active;
		public bool IsActive {
			get => this._active;
			set {
				if( this._active != value ) {
					this._active = value;
					this.NotifyPropertyChanged();
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

		private CategorySource _source;
		public CategorySource Source {
			get { return this._source; }
			set {
				if( this._source != value ) {
					this._source = value;
					this.NotifyPropertyChanged();
				}
			}
		}

	}

} // namespace