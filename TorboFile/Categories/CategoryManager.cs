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
using Lemur.Windows.MVVM;

namespace TorboFile.Categories {

	/// <summary>
	/// Stores and Loads CategorySet objects.
	/// 
	/// NOTE: The static functions indicate operations apart from all notifcations, collection functions.
	/// might place these in a different class entirely.
	/// </summary>
	public class CategoryManager : ViewModelLite, ICollection< CategorySetInfo >, INotifyCollectionChanged, INotifyPropertyChanged {

		static private CategoryManager _instance;
		static public CategoryManager Instance {
			get { return CategoryManager._instance ?? ( CategoryManager._instance = new CategoryManager() ); }
		}

		private const string StorageDirectory = "categories";
		private const string DefaultSetName = "New Category Set";

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

		/// <summary>
		/// Property string for watching CurrentSet property updates.
		/// </summary>
		public const string CurrentSetPropName = "Current";

		/// <summary>
		/// Information about the currently active set
		/// </summary>
		private CategorySetInfo ActiveInfo {
			get => this._active;
			set {

				if (this._active != value ) {

					if( this._active != null ) {
						this.Deactivate( this._active );
					}
					this._active = value;

					if( value != null ) {
						this.Activate( this._active );
					}

					this.NotifyPropertyChanged();
					this.NotifyPropertyChanged( "Current" );

				}

			} // set

		}
		private CategorySetInfo _active;

		/// <summary>
		/// Active CategorySet.
		/// </summary>
		public CategorySet Current {
			get {
				if( this._active == null ) {
					return null;
				}
				return this._active.Set;
			}

		}

		public int Count => this.availableSets.Count;

		public bool IsReadOnly => false;

		private CategoryManager() {

			this.AvailableSets = new List<CategorySetInfo >( CategoryManager.GetIsolatedSets() );

			this.RestoreLastSet();

		} //

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

		private void Deactivate( CategorySetInfo info ) {

			if( info.Set != null ) {
				this.SaveSet( info.Set );
			}
			info.IsActive = false;
			info.Set = null;

		}

		private void Activate( CategorySetInfo info ) {

			SortingSettings.Default.lastSortingSet = info.Name;
			if( info.Set == null ) {
				info.Set = ReadIsolated( info.Name );
			}
			info.IsActive = true;

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
		private bool TryActivate( string categoryName ) {

			if( string.IsNullOrEmpty( categoryName ) ) {
				return false;
			}

			CategorySetInfo info = this.GetInfo( categoryName );
			if( info != null ) {
				return TryActivate( info );
			}

			return false;
	
		} // TrySetCurrent()

		public bool TryActivate( CategorySetInfo info ) {

			if( info.Set == null ) {

				info.Set = CategoryManager.ReadIsolated( info.Name );
				if( info.Set == null ) {
					return false;
				}

			}

			this.ActiveInfo = info;

			return true;

		}

		/// <summary>
		/// Restored the last CategorySet used by this user.
		/// </summary>
		private void RestoreLastSet() {

			string last_name = TorboFile.Properties.SortingSettings.Default.lastSortingSet;
			if( TryActivate( last_name ) ) {
				return;
			}

			//Console.WriteLine( "No previous set. Loading default." );

			// no last-set defined. pick first on list. NOTE: maybe don't do this in case of privacy?
			if( this.availableSets != null && availableSets.Count > 0 ) {

				if( this.TryActivate( this.availableSets[0] ) ) {
					Console.WriteLine( "No last Set. Picking first." );
					return;
				}

			}

			this.ActiveInfo = this.CreateDefaultSet();

		}

		public bool RenameCategorySet( string old_name, string new_name ) {

			if( CategoryManager.RenameIsolated( old_name, new_name ) ) {

				CategorySetInfo info = this.GetInfo( old_name );

				if( info != null ) {

					Console.WriteLine( "CategoryManager: Changing Set Name" );
					info.Name = new_name;
					UpdateSetData( info );

				} else {
					Console.WriteLine( "CategoryManager: Cannot find renamed set index" );
				}

				return true;

			}

			return false;

		}

		/// <summary>
		/// Update CategorySet information that was changed in the set info,
		/// such as for a rename.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		private bool UpdateSetData( CategorySetInfo info ) {

			CategorySet set = info.Set;
			if( set == null ) {
				set = CategoryManager.ReadIsolated( info.Name );
				if( set == null ) {
					return false;
				}
			}

			set.Name = info.Name;
			set.SavePath = info.Path;

			this.SaveSet( set );

			return true;

		}

		private CategorySetInfo GetSetInfo( string setName ) {

			for( int i = this.availableSets.Count - 1; i >= 0; i-- ) {
				if( this.availableSets[i].Name == setName ) {
					return this.availableSets[i];
				}
			}
			return null;

		}

		private CategorySetInfo CreateDefaultSet() {

			CategorySetInfo info = new CategorySetInfo( new CategorySet( DefaultSetName ), CategorySource.Isolated );
			this.Add( info );
			return info;

		}

		public void Add( CategorySetInfo info ) {

			if( info.Set != null ) {
				CategoryManager.WriteIsolated( info.Set );
			}

			this.availableSets.Add( info );
			this.CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, info ) );

		}

		/// <summary>
		/// </summary>
		/// <param name="set"></param>
		public void Add( CategorySet set ) {
			this.Add( new CategorySetInfo( set, set.Source ) );
		} //


		/// <summary>
		/// Add a new CategorySet with the given name.
		/// </summary>
		/// <param name="set_name">The name of the group to add.</param>
		public void Add( string set_name ) {
			this.Add( new CategorySetInfo( new CategorySet( set_name ), CategorySource.Isolated ) );
		}

		public void Clear() {

			this.availableSets.Clear();
			this.CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );

			this.ActiveInfo = null;

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

						string[] files = storage.GetFileNames( StorageDirectory + Path.DirectorySeparatorChar + "*" );
						CategorySetInfo[] sets = new CategorySetInfo[files.Length];
						for( int i = files.Length - 1; i >= 0; i-- ) {
							sets[i] =
								new CategorySetInfo( StorageDirectory + Path.DirectorySeparatorChar + files[i], files[i], CategorySource.Isolated );
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

			if( !IsolatedStorageFile.IsEnabled ) {
				Console.WriteLine( "CategoryManager: Cannot rename Set: IsolatedStorage not enabled." );
				return false;
			}

			try {

				using( IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForAssembly() ) {

					if( !storage.DirectoryExists( StorageDirectory ) ) {
						// if the directory doesn't exist, the first set can't exist to begin with.
						Console.WriteLine( "CategoryManager: Cannot rename Set: Directory does not exist." );
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
					string[] files = storage.GetFileNames( StorageDirectory + Path.DirectorySeparatorChar + "*" );
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

		static private string GetIsolatedPath( string set_name ) {
			return StorageDirectory + Path.DirectorySeparatorChar + set_name;
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

				int changedIndex = this.GetInfoIndex( setName );

				if( changedIndex >= 0 ) {

					CategorySetInfo removed = this.availableSets[changedIndex];
					this.availableSets.RemoveAt( changedIndex );

					this.CollectionChanged?.Invoke( this,
						new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, removed, changedIndex ) );

					if( this.ActiveInfo.Name == setName ) {
						this.ActiveInfo = null;
					}

					return true;
				} else {
					Console.WriteLine( "CatagoryManager: Set Not Found: " + setName );
				}

			}

			return false;

		}

		/// <summary>
		/// Return a categorySet name not listed in the available sets.
		/// </summary>
		/// <param name="recommended"></param>
		/// <returns></returns>
		public string GetUniqueName( string recommended = null ) {

			string testName = recommended ?? DefaultSetName;

			if( !this.availableSets.Any( c => c.Name == testName ) ) {
				return testName;
			}

			int num = 1;
			string curName = testName + num;

			while( this.availableSets.Any( c => c.Name == curName ) ) {
				curName = testName + ++num;
			}

			return curName;

		}

		private CategorySetInfo GetInfo( string set_name ) {

			CategorySetInfo info;
			for( int i = this.availableSets.Count - 1; i >= 0; i-- ) {
	
				info = this.availableSets[i];

				if( info.Name == set_name ) {
					return info;
				}
			}
			return null;

		}

		private int GetInfoIndex( string name ) {

			for( int i = this.availableSets.Count - 1; i >= 0; i-- ) {
				if( this.availableSets[i].Name == name ) {
					return i;
				}
			}
			return -1;

		}

		public IEnumerator< CategorySetInfo > GetEnumerator() {
			return this.availableSets.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.availableSets.GetEnumerator();
		}

	} // class

} // namespace
