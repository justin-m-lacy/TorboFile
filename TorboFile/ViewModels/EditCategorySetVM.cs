using TorboFile.Categories;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Creation and editing of a category Set.
	/// </summary>
	public sealed class EditCategorySetVM : ViewModelBase {

		private CategoryManager manager;
		/// <summary>
		/// CatagoryManager.
		/// </summary>
		public CategoryManager CategoryManager {
			get { return this.manager; }
			set { this.manager = value; this.NotifyPropertyChanged(); }
		} //

		/// <summary>
		/// Name of set being edited.
		/// </summary>
		private string editingName;
		public string EditingName {
			get { return this.editingName; }
			set {

				if( this.editingName != value ) {

					this.editingName = value;
					this.NewName = value;
					this.NotifyPropertyChanged();

				}

			} //set()

		}

		/// <summary>
		/// If the existing set is non-null, an existing set is being edited.
		/// </summary>
		private CategorySet editingSet;
		public CategorySet EditingSet {
			set {

				if ( value != this.editingSet ) {
					this.editingSet = value;
					if( value != null ) {
						this._name = value.Name;
					}
					this.NotifyPropertyChanged();
				}

			}
		}


		private RelayCommand _cmdSubmit;
		/// <summary>
		/// Attempt to create the new category, or edit the existing category.
		/// </summary>
		public RelayCommand CmdSubmit {
			get {
				return _cmdSubmit ?? ( this._cmdSubmit = new RelayCommand(

				  () => {
					  if( !string.IsNullOrEmpty(editingName) ) {

						  this.manager.RenameCategorySet( this.editingName, NewName );
						  /*if( !this.manager.Contains( this.NewName ) ) {
							  this.editingSet.Name = this._name;
						  }*/

					  } else {

						  this.CategoryManager.Add( new CategorySet( this._name ) );

					  }

					  this.TryClose();

				  },

				  () => { return ( !string.IsNullOrEmpty( this._name ) && !this.manager.NameTaken( this._name ) ); } )
			);
			}

		} // CmdSubmit

		private string _name;
		/// <summary>
		/// Edited name or name of the new category set.
		/// </summary>
		public string NewName {

			get { return this._name; }
			set {

				if( this._name != value ) {
					this._name = value;
					this.CmdSubmit.RaiseCanExecuteChanged();
					this.NotifyPropertyChanged();
				}
			}

		}

		/// <summary>
		/// Used to set as DataContext in xaml.
		/// CategoryManager must still be initialized.
		/// </summary>
		public EditCategorySetVM() {
		}

		public EditCategorySetVM( CategoryManager manager, CategorySet existing = null ) {

			this.manager = manager;
			this.EditingSet = existing;

		}

    } // class

} // namespace