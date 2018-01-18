using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemur.Utils;
using System.IO;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections;
using TorboFile.Properties;
using System.Runtime.CompilerServices;

namespace TorboFile.Categories {

	/// <summary>
	/// Information about a non-loaded CategorySet?
	/// </summary>
	public class CategorySetInfo : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public CategorySetInfo( string path, string name, CategorySource source ) {

			this.Path = path;
			this.Name = name;
			this.Source = source;

		}

		private bool _loaded;
		public bool IsLoaded {
			get { return this._loaded; }
			internal set {
				if( this._loaded != value ) {
					this._loaded = value;
					this.NotifyPropertyChanged();
				}

			}

		}

		public void NotifyPropertyChanged( [CallerMemberName] string propName=null ) {

			PropertyChangedEventHandler handler = this.PropertyChanged;
			if( handler != null ) {
				handler( this, new PropertyChangedEventArgs( propName ) );
			}

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

	/// <summary>
	/// Stores and Loads CategorySet objects.
	/// 
	/// NOTE: The static functions indicate operations apart from all notifcations, collection functions.
	/// might place these in a different class entirely.
	/// </summary>
	public class CategoryManager : ICollection< CategorySetInfo >, INotifyCollectionChanged, INotifyPropertyChanged {

		static private CategoryManager _instance;
		static public CategoryManager Instance {
			get { return CategoryManager._instance ?? ( CategoryManager._instance = new CategoryManager() ); }
		}

		private const string StorageDirectory = "categories";
		private const string DefaultSetName = "New Category Set";

		public event PropertyChangedEventHandler PropertyChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// All CategorySets available in IsolatedStorage.
		/// </summary>
		private List< CategorySetInfo > availableSets;
		public List<CategorySetInfo> AvailableSets {
			get { return this.availableSets; }
			set {

				this.availableSets = value;
				if( this.CollectionChanged != null ) {
					NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
					handler( this,
						new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, this.availableSets ) );
				}
				
			} // set()

		}


		private CategorySet _current;

		/// <summary>
		/// Property string for watching CurrentSet property updates.
		/// </summary>
		public const string CurrentSetPropName = "Current";

		/// <summary>
		/// Active CategorySet.
		/// </summary>
		public CategorySet Current {
			get { return this._current; }
			private set {

				if( this._current != value ) {

					this._current = value;
					if( value == null ) {
						Console.WriteLine( "CategoryManager.Current: CATEGORY NULL" );
					}

					this.ActivateSet( this._current );

					if( PropertyChanged != null ) {
						PropertyChangedEventHandler handler = this.PropertyChanged;
						handler( this, new PropertyChangedEventArgs( CurrentSetPropName ) );
					}

				}

			} // set()
		}

		public int Count => this.availableSets.Count;

		public bool IsReadOnly => false;

		private CategoryManager() {

			this.AvailableSets = new List<CategorySetInfo >( CategoryManager.GetIsolatedSets() );
			//Console.WriteLine( "CategoryManager.cs : #CATEGORY SETS: " + this.availableSets.Count );

			this.RestoreLastSet();

		} //

		/*private void NotifyCollectionChanged( string changedSetName ) {

			if( this.CollectionChanged == null ) {
				return;
			}
			
			NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
			if( handler != null ) {
				handler( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add ) );
			}

		}*/

		/// <summary>
		/// Return a categorySet name not listed in the available sets.
		/// </summary>
		/// <param name="recommended"></param>
		/// <returns></returns>
		public string GetUniqueName( string recommended = null ) {

			string testName = recommended ?? DefaultSetName;

			if( !this.availableSets.Any( c => c.Name == testName )) {
				return testName;
			}

			int num = 1;
			string curName = testName + num;

			while( this.availableSets.Any( c => c.Name == curName ) ) {
				curName = testName + ++num;
			}

			return curName;

		}

		/// <summary>
		/// Predicate to tell if a given string has been used as a Category name.
		/// </summary>
		/*public Predicate<string> IsNameAvailable {
			get { return new Predicate<string>( NameAvailable ); }
		}*/

		/// <summary>
		/// Returns true if the given category name has already been used.
		/// Can be bound to a ValidationRule condition.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool NameTaken( string name ) {

			return this.availableSets.Any( (info) => { return info.Name == name; });

		}

		private void ActivateSet( CategorySet set ) {

			if( set == null ) return;
			SortingSettings.Default.lastSortingSet = this._current.Name;
			CategorySetInfo info = this.GetSetInfo( set.Name );
			if ( info != null ) {
				info.IsLoaded = true;
				Console.WriteLine( "CategoryManager : Activating Set: " + set.Name );
			}

		} //

		private void DeactivateSet( CategorySet set ) {

			if( set == null ) {
				return;
			}
			this.SaveSet( set );

			CategorySetInfo info = this.GetSetInfo( set.Name );
			if( info != null ) {
				info.IsLoaded = false;
			}

		}

		public void SaveSet( CategorySet set ) {

			//Console.WriteLine( "will save set" );
			if( set != null && set.Dirty ) {

				Console.WriteLine( "Attempting Save: " + set.Name );
				bool success = CategoryManager.WriteIsolated( set );
				if( success ) {
					set.Dirty = false;
				}

			}

		} // SaveSet()

		/// <summary>
		/// Saves the currently active CategorySet to disk.
		/// </summary>
		public void SaveCurrent() {
			this.SaveSet( this.Current );
		}

		/// <summary>
		/// Attempt to set the Current CategorySet to the given
		/// named set.
		/// </summary>
		/// <param name="setName"></param>
		/// <returns></returns>
		public bool TrySetCurrent( string categoryName ) {

			CategorySet set = CategoryManager.ReadIsolated( categoryName );
			return TrySetCurrent( set );
	
		} // TrySetCurrent()

		/// <summary>
		/// Attempts to set the currently active set to the given category set.
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public bool TrySetCurrent( CategorySet set ) {

			if( set == null ) {
				return false;
			}
			this.DeactivateSet( this._current );

			this.Current = set;

			return true;

		}

		/// <summary>
		/// Restored the last CategorySet used by this user.
		/// </summary>
		private void RestoreLastSet() {

			string last_name = TorboFile.Properties.SortingSettings.Default.lastSortingSet;
			if( TrySetCurrent( last_name ) ) {
				return;
			}

			//Console.WriteLine( "No previous set. Loading default." );

			// no last-set defined. pick first on list. NOTE: maybe don't do this in case of privacy?
			if( this.availableSets != null && availableSets.Count > 0 ) {

				if( this.TrySetCurrent( this.availableSets[0].Name ) ) {
					return;
				}

			}

			this.Current = this.CreateDefaultSet();
	
		}

		public bool RenameCategorySet( string old_name, string new_name ) {

			if( RenameIsolated( old_name, new_name ) ) {

				int changedIndex = this.GetSetIndex( old_name );

				if( changedIndex >= 0 ) {


					this.availableSets[changedIndex].Name = new_name;
					/*if( this.CollectionChanged != null ) {
						NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
						handler(
							this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, new_name, old_name, changedIndex ) );
					}*/

				}

				/// check if loaded set needs to be renamed.
				/// TODO: make this better.
				if( Current != null && Current.Name == old_name ) {
					Current.Name = new_name;
				}

				return true;

			}

			return false;

		}

		private bool RenameCategorySet( CategorySet set, string new_name ) {

			if( this.RenameCategorySet( set.Name, new_name ) ) {
	
				set.Name = new_name;

				return true;

			}

			return false;

		} //


		private CategorySetInfo GetSetInfo( string setName ) {

			for( int i = this.availableSets.Count - 1; i >= 0; i-- ) {
				if( this.availableSets[i].Name == setName ) {
					return this.availableSets[i];
				}
			}
			return null;

		}

		private int GetSetIndex( string name ) {

			for( int i = this.availableSets.Count - 1; i >= 0; i-- ) {
				if( this.availableSets[i].Name == name ) {
					return i;
				}
			}
			return -1;

		}

		private CategorySet CreateDefaultSet() {

			CategorySet set = new CategorySet( DefaultSetName );
			this.Add( set );

			return set;
		}

		static private string GetIsolatedPath( string set_name ) {
			return StorageDirectory + "/" + set_name;
		}

		/// <summary>
		/// </summary>
		/// <param name="set"></param>
		public void Add( CategorySet set ) {

			if( CategoryManager.WriteIsolated( set ) ) {
				Console.WriteLine( "new set stored." );
			} else {
				Console.WriteLine( "Could not store set." );
			}

			CategorySetInfo info = new CategorySetInfo( set.SavePath, set.Name, CategorySource.Isolated );
			this.availableSets.Add( info );

			if( this.CollectionChanged != null ) {
				this.CollectionChanged( this,
					new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, info ) );
			}

		} //

		public void Add( CategorySetInfo info ) {
			this.Add( new CategorySet( info.Name ) );
		}

		/// <summary>
		/// Add a new CategorySet with the given name.
		/// </summary>
		/// <param name="set_name">The name of the group to add.</param>
		public void Add( string set_name ) {

			/// Add( CategorySet ) notifies collection changed.
			this.Add( new CategorySet( set_name ) );

		}

		public void Clear() {

			this.availableSets.Clear();
			if( this.CollectionChanged != null ) {
				this.CollectionChanged( this,
					new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}

			this.Current = null;

			CategoryManager.DeleteAllIsolated();

		}

		/// <summary>
		/// Returns a list of the CategorySet files stored in Isolated Storage.
		/// </summary>
		/// <returns></returns>
		static public CategorySetInfo[] GetIsolatedSets() {

			if( IsolatedStorageFile.IsEnabled ) {

				using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

					if( storage.DirectoryExists( StorageDirectory ) ) {

						string[] files = storage.GetFileNames( StorageDirectory + "/*" );
						CategorySetInfo[] sets = new CategorySetInfo[files.Length];
						for( int i = files.Length - 1; i >= 0; i-- ) {
							sets[i] =
								new CategorySetInfo( StorageDirectory + "/" + files[i], files[i], CategorySource.Isolated );
						} // for-loop.

						return sets;

					}

				} // using
			}
			return new CategorySetInfo[0];

		} //

		/// <summary>
		/// Remove a CategorySet from Isolated storage.
		/// </summary>
		/// <param name="set_name"></param>
		static private bool DeleteIsolated( string set_name ) {

			if( !IsolatedStorageFile.IsEnabled ) { return false; }

			try {

				using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

					string path = GetIsolatedPath( set_name );
					if( storage.FileExists( path ) ) {
						/// file not existing => as good as deleted.
						storage.DeleteFile( path );
					}
				}

			} catch( Exception ) {
				return false;
			}

			return true;

		}

		/// <summary>
		/// Rename a CategorySet in Isolated Storage.
		/// </summary>
		/// <param name="old_name"></param>
		/// <param name="new_name"></param>
		/// <returns></returns>
		static private bool RenameIsolated( string old_name, string new_name ) {

			if( !IsolatedStorageFile.IsEnabled ) { return false; }

			try {

				using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

					if( !storage.DirectoryExists( StorageDirectory ) ) {
						// if the directory doesn't exist, the first set can't exist to begin with.
						return false;
					}

					string old_path = GetIsolatedPath( old_name );
					string new_path = GetIsolatedPath( new_name );

					storage.MoveFile( old_path, new_path );

					return true;

				}

			} catch( Exception ) {
				return false;
			}

		}

		/// <summary>
		/// Read a CategorySet from isolated storage.
		/// </summary>
		/// <param name="set_name"></param>
		/// <returns>The CategorySet stored at the file location, or null if an error occurs.</returns>
		static private CategorySet ReadIsolated( string set_name ) {

			if( string.IsNullOrEmpty( set_name ) ) { return null; }
			if( !IsolatedStorageFile.IsEnabled ) { return null; }

			using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

				string path = GetIsolatedPath( set_name );
				if( !storage.FileExists( path ) ) {
					return null;
				}


				using( FileStream stream = storage.OpenFile( path, System.IO.FileMode.Open ) ) {

					return FileUtils.ReadBinary<CategorySet>( stream );

				}

			} // using

		} //

		/// <summary>
		/// Write a CategorySet to IsolatedStorage.
		/// </summary>
		/// <param name="set"></param>
		static private bool WriteIsolated( CategorySet set ) {

			if( !IsolatedStorageFile.IsEnabled ) { return false; }

			try {

				using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

					if( !storage.DirectoryExists( StorageDirectory ) ) {
						Console.WriteLine( "Creating CategorySets directory" );
						storage.CreateDirectory( StorageDirectory );
					}

					string savePath = CategoryManager.GetIsolatedPath( set.Name );
					//Console.WriteLine( "Saving current CategorySet to: " + savePath );
					using( FileStream stream = storage.OpenFile( savePath, FileMode.Create ) ) {
						FileUtils.WriteBinary<CategorySet>( stream, set );
					}

					return true;

				}

			} catch( Exception e ) {

				Console.WriteLine( e.ToString() ); 
				return false;

			}

		} // WriteIsolated()

		/// <summary>
		/// Removes all CategorySets from isolated storage.
		/// </summary>
		static public void DeleteAllIsolated() {

			if( !IsolatedStorageFile.IsEnabled ) { return; }

			using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

				if( storage.DirectoryExists( StorageDirectory ) ) {
					string[] files = storage.GetFileNames( StorageDirectory + "/*" );
					foreach( string file in files ) {
						string deletePath = GetIsolatedPath( file );
						Console.WriteLine( "DELETING CategorySet: " + deletePath );
						storage.DeleteFile( deletePath );
					}

					//storage.DeleteDirectory( StorageDirectory );

				} else {
					Console.WriteLine( "Cannot delete categorySets. Directory does not exist." );
				}

			}

		}

		public bool Contains( CategorySetInfo info ) {
			return this.availableSets.Contains( info );
		}
		public bool Contains( string item ) {
			return this.availableSets.Any( c => c.Name == item );
		}

		public void CopyTo( CategorySetInfo[] a, int index ) {
			this.availableSets.CopyTo( a, index );
		}
		public void CopyTo( string[] array, int arrayIndex ) {
			this.availableSets.Select( c => c.Name ).ToArray();
		}

		public bool Remove( CategorySetInfo info ) {
			return this.Remove( info.Name );
		}

		public bool Remove( string setName ) {

			if( CategoryManager.DeleteIsolated( setName ) ) {

				int changedIndex = this.GetSetIndex( setName );

				if( changedIndex >= 0 ) {

					CategorySetInfo removed = this.availableSets[changedIndex];
					this.availableSets.RemoveAt( changedIndex );

					if( this.CollectionChanged != null ) {
						Console.WriteLine( "NOTIFYING COLLECTION CHANGED" );
						NotifyCollectionChangedEventHandler handler = this.CollectionChanged;
						handler( this,
						new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, removed, changedIndex ) );
					}
					if( this.Current.Name == setName ) {
						this.Current = null;
					}

					return true;
				} else {
					Console.WriteLine( "CatagoryManager: Set Not Found: " + setName );
				}

			}

			return false;

		}

		public IEnumerator< CategorySetInfo > GetEnumerator() {
			return this.availableSets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.availableSets.GetEnumerator();
		}

	} // class

} // namespace
