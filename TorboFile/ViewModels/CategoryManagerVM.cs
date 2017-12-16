using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorboFile.Categories;
using Lemur.Windows;
using TorboFile.Services;
using System.ComponentModel;

namespace TorboFile.ViewModels {

	public class CategoryManagerVM : ViewModelBase {

		private CategoryManager manager;
		/// <summary>
		/// CatagoryManager.
		/// </summary>
		public CategoryManager CategoryManager {
			get { return this.manager; }
			set {
				this.manager = value;
				this.NotifyPropertyChanged();
			}
		}

		private RelayCommand _cmdNewCategory;
		public RelayCommand CmdNewCategory {
			get {
				return _cmdNewCategory ?? ( this._cmdNewCategory =
				  new RelayCommand( () => {
						  App.Instance.EditCategoryDialog( this );
					  }
				) );
			}
		}

		private RelayCommand _cmdNewSet;
		/// <summary>
		/// Create a new Category Set.
		/// </summary>
		public RelayCommand CmdNewSet {
			get {
				return this._cmdNewSet ?? ( this._cmdNewSet = new RelayCommand(
					()=> {
						App.Instance.ShowEditCategorySet( this );
					}
				) );
			}
		}

		private RelayCommand _cmdEditSet;
		/// <summary>
		/// Edit properties of an existing category set.
		/// </summary>
		public RelayCommand CmdEditSet {

			get {
				return this._cmdEditSet ?? ( this._cmdEditSet = new RelayCommand(

					() => {
						if( this.FocusedSet == null ) return;
						string name = this.FocusedSet.Name;
						if( !string.IsNullOrEmpty( name ) ) {

							App.Instance.ShowEditCategorySet( this, name );
						}

					},
					() => { return this.FocusedSet != null; }

				) );
			}

		}

		/// <summary>
		/// TODO: Dispatch PropertyChanged.
		/// </summary>
		/*public CategorySet ActiveSet {
			get { return this.manager.Current; }
			set {
				this.manager.Current = value;
			}
		}

		public string ActiveSetName {
			get {
				CategorySet cur = this.manager.Current;
				if( cur == null ) { return string.Empty; }
				return cur.Name;
			}
		}*/


		private RelayCommand<string> _cmdChangeActiveSet;
		/// <summary>
		/// Change the currently active CategorySet.
		/// </summary>
		public RelayCommand<string> CmdChangeActiveSet {

			get {
				return this._cmdChangeActiveSet ?? ( this._cmdChangeActiveSet = new RelayCommand<string>(

			  ( select_name ) => {
				  //string name = this.FocusedSet;
				  if( !string.IsNullOrEmpty( select_name ) && this.CategoryManager.Contains( select_name ) ) {

					bool success = this.CategoryManager.TrySetCurrent( select_name );
					  if( !success ) {
						  // do something.
					  }

				  }

			  })
			  ); } // get()

		}

		private RelayCommand _cmdRemoveSet;
		public RelayCommand CmdRemoveSet {
			get {
				return this._cmdRemoveSet ?? ( this._cmdRemoveSet = new RelayCommand(
					() => {

						if( this.FocusedSet == null ) return;
						string name = this.FocusedSet.Name;
						IMessageBox msgBox = (IMessageBox)this.ServiceProvider.GetService( typeof( IMessageBox ) );
						MessageResult result =
							msgBox.ShowConfirm( this, "Confirm Delete", "Are you sure you want to delete this set?" );
						if( result == MessageResult.Accept ) {
							// Delete the thingy.
							Console.WriteLine( "Attempting to delete Set: " + name );
							this.manager.Remove( name ); ;
						}

						this.FocusedSet = null;

					},
					// canExecute() => Only run if there is a Set selected.
					() => { return (this.FocusedSet != null ); }

				) );
			}
		}


		private RelayCommand _cmdEditCategory;
		public RelayCommand CmdEditCategory {
			get {
				return this._cmdEditCategory ?? ( this._cmdEditCategory =
					new RelayCommand(
						() => {
		
							Console.WriteLine( "Attempting to edit: " + this.focusCategory.Name );
							App.Instance.EditCategoryDialog( this, this.focusCategory );
						},
						this.IsCategorySelected
						) );
			}
		}

		private RelayCommand _cmdDeleteCategory;
		public RelayCommand CmdDeleteCategory {
			get {
				return this._cmdDeleteCategory ?? ( this._cmdDeleteCategory =
					new RelayCommand(
						() => {

							IMessageBox msgBox = (IMessageBox)this.ServiceProvider.GetService( typeof( IMessageBox ) );

							MessageResult result =
							msgBox.ShowConfirm( this, "Confirm Delete", "Are you sure you want to delete this category?" );

							if( result == MessageResult.Accept ) {
								// Delete the thingy.
								Console.WriteLine( "Attempting to delete: " + this.FocusedCategory.Name );

								this.manager.Current.Remove( this.FocusedCategory );
								this.FocusedCategory = null;

							}

						},
						this.IsCategorySelected
				) );
			}
		}

		/// <summary>
		/// Determines if some category is selected in the view list. Used to enable/disable
		/// certain category Commands.
		/// NOTE: This doesn't work for two ManagerViews simultaneously bound to the same VM...
		/// </summary>
		/// <returns></returns>
		private bool IsCategorySelected() {
			return ( this.FocusedCategory != null );
		}

		/// <summary>
		/// Set currently selected in a SetList, if any.
		/// </summary>
		private CategorySetInfo focusedSet;
		public CategorySetInfo FocusedSet {
			get { return this.focusedSet; }

			set {

				if( this.focusedSet != value ) {

					this.focusedSet = value;
					this.CmdEditSet.RaiseCanExecuteChanged();
					this.CmdRemoveSet.RaiseCanExecuteChanged();

				}

			} // set()

		}

		/// <summary>
		/// Category that has focus in a CategoryList, if any.
		/// </summary>
		private FileCategory focusCategory;
		public FileCategory FocusedCategory {
			get { return this.focusCategory; }
			set {
				if( value != this.focusCategory ) {

					bool wasNull = ( this.focusCategory == null );
					bool isNull = ( value == null );
					this.focusCategory = value;

					if( isNull != wasNull ) {
						this.CmdDeleteCategory.RaiseCanExecuteChanged();
						this.CmdEditCategory.RaiseCanExecuteChanged();
					}

				}
			}

		}

		public CategoryManagerVM( CategoryManager manager ) {

			this.manager = manager;
			this.manager.PropertyChanged += Manager_PropertyChanged;
		}

		private void Manager_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			switch( e.PropertyName ) {

				case CategoryManager.CurrentSetPropName:
					Console.WriteLine( "CategoryManagerVM: ActiveSet Changing" );
					//this.NotifyPropertyChanged( "ActiveSetName" );
					//this.NotifyPropertyChanged( "ActiveSet" );
					break;
			}

		}

	} // class

} // namespace