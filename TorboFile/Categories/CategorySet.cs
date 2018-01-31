using Lemur.Windows.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TorboFile.Categories {

	/// <summary>
	/// Source where the set came from.
	/// </summary>
	public enum CategorySource {
		Isolated,
		FileSystem
	}

	[Serializable]
	public class CategorySet : ICollection<FileCategory>, INotifyCollectionChanged, INotifyPropertyChanged {

		//private Dictionary<string, FileCategory> categories;
		private List<FileCategory> categories;

		/// <summary>
		/// Dispatches when the collection has changed.
		/// </summary>
		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Dispatched when a property of the collection has changed.
		/// </summary>
		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		#region PROPERTIES

		/// <summary>
		/// Marks whether the Set has unsaved changes.
		/// </summary>
		[field: NonSerialized]  // makes no sense to save this prop to disk.
		private bool _dirty;
		public bool Dirty {
			get { return this._dirty; }
			set {

				if( this._dirty != value ) {
					this._dirty = value;

					// !!! Can't use the DispatchPropChange() function because that sets Dirty=true.
					if( this.PropertyChanged != null ) {
						PropertyChangedEventHandler handler = this.PropertyChanged;
						handler( this, new PropertyChangedEventArgs( "Dirty" ) );
					}
				}
			}

		}

		/// <summary>
		/// Name of the Category Set.
		/// </summary>
		public string Name {
			get { return this._name; }

			set {
				if( value != this._name ) {
					this._name = value;
					this.NotifyPropertyChange();
				}
			}

		}
		private string _name;

		[NonSerialized]
		private string _savePath;
		/// <summary>
		/// File location for storing the category set, if any.
		/// </summary>
		public string SavePath {
			get { return this._savePath; }
			set {
				if( value != this._savePath ) {
					this._savePath = value;
					this.NotifyPropertyChange();
				}
			}
		}

		private CategorySource setSource;
		/// <summary>
		/// Storage source for the category.
		/// </summary>
		public CategorySource Source {

			get { return this.setSource; }
			set { this.setSource = value; }

		}

		private string _passHash;
		/// <summary>
		/// Hash of password for saving the set.
		/// </summary>
		public string PasswordHash {
			get { return this._passHash; }
			set {
				if( this._passHash != value ) {
					this._passHash = value;
					this.NotifyPropertyChange();
				}
			}

		} //

		public int Count => this.categories.Count;

		public bool IsReadOnly => false;

		#endregion

		public CategorySet( string name ) : this() {

			this.Name = name;

		}

		public CategorySet() {

			this.categories = new List<FileCategory>();

		} //

		/// <summary>
		/// Create an input binding that links to the given category.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		public InputBinding MakeCategoryBinding( ICommand command, FileCategory category ) {

			if( category.Gesture == null ) {
				return null;
			}
			InputBinding binding = new InputBinding( command, category.Gesture );
			binding.CommandParameter = category;

			return binding;
		}

		/// <summary>
		/// Create bindings for each category with a bound input.
		/// </summary>
		/// <param name="bindingCommand"></param>
		/// <returns></returns>
		public IEnumerable<InputBinding> MakeCategoryBindings( ICommand bindingCommand ) {

			List<InputBinding> bindings = new List<InputBinding>();

			foreach( FileCategory category in this.categories ) {

				if( category.Gesture != null ) {

					InputBinding binding = new InputBinding( bindingCommand, category.Gesture );
					//Console.WriteLine( "Creating binding for: " + category.Name );
					//Console.WriteLine( "target: " + binding.CommandTarget );
					binding.CommandParameter = category;
					bindings.Add( binding );

				}

			} // foreach

			return bindings;

		} //

		/// <summary>
		/// Add a FileCategory to the set.
		/// </summary>
		/// <param name="category"></param>
		public void Add( FileCategory category ) {

			Console.WriteLine( "ADDING CATEGORY: " + category.Name + " to Set: " + this.Name );
			this.categories.Add( category );

			this.Dirty = true;
			this.CollectionChanged?.Invoke( this,
				new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, category ) );

		} // Add()

		/// <summary>
		/// Remove a FileCategory from the set.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove( FileCategory item ) {

			if( item == null ) { return false; }
			string itemName = item.Name;
			if( string.IsNullOrEmpty( itemName ) ) { return false; }

			/// Must do a manual search since reference equality won't work here.

			FileCategory setItem;
			for( int index = this.categories.Count - 1; index >= 0; index-- ) {

				setItem = this.categories[index];
				if( item.IsEqual( this.categories[index] ) ) {

					this.categories.RemoveAt( index );
					this.DispatchRemove( setItem, index );      // better to remove the actual item, not the matching reference?
					return true;
				}

			}
			Console.WriteLine( "item not found: " + item.Name );
			return false;

		}

		/// <summary>
		/// Clear all categories from the set.
		/// </summary>
		public void Clear() {

			this.categories.Clear();

			this.Dirty = true;
			this.CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );

		}

		public int IndexOf( FileCategory category ) {
			return this.categories.IndexOf( category );
		}

		private void NotifyPropertyChange( [CallerMemberName] string propName = "" ) {

			this.Dirty = true;
			this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );

		}

		/// <summary>
		/// Indexer for CategorySet FileCategories.
		/// Setting an item to itself forces a dispatch of CollectionChanged
		/// so listeners can be notified if an item changes its properties.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public FileCategory this[int index] {
			get {
				return this.categories[index];
			}
			set {
				FileCategory previous = this.categories[index];
				this.categories[index] = value;
				this.DispatchReplace( value, previous, index );
			}
		}

		private void DispatchReplace( FileCategory category, FileCategory previous, int index ) {

			this.Dirty = true;
			if( this.CollectionChanged != null ) {
				Console.WriteLine( "Category Changed: " + category.Name );
				this.CollectionChanged( this,
					new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace, category, previous, index ) );
			}

		} //

		private void DispatchRemove( FileCategory category, int index ) {

			this.Dirty = true;
			this.CollectionChanged?.Invoke( this,
				new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, category, index ) );

		}

		/// <summary>
		/// Checks if a category with the given name already exists.
		/// </summary>
		/// <param name="categoryName"></param>
		/// <returns></returns>
		public bool Contains( string categoryName ) {

			if( string.IsNullOrEmpty( categoryName ) ) {
				return false;
			}
			foreach( FileCategory cat in this.categories ) {
				if( cat.Name == categoryName ) {
					return true;
				}
			}

			return false;

		}

		public bool Contains( FileCategory item ) {

			if( item == null ) {
				return false;
			}
			string itemName = item.Name;
			if( string.IsNullOrEmpty( itemName ) ) {
				return false;
			}

			IEnumerable<FileCategory> matches = this.categories.Where( item.IsEqual );

			return matches.Count() > 0;

		} //

		public void CopyTo( FileCategory[] array, int arrayIndex ) {
			this.categories.CopyTo( array, arrayIndex );
		} // CopyTo()

		public IEnumerator<FileCategory> GetEnumerator() {
			return this.categories.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.categories.GetEnumerator();
		}

	} // class

} // namespace