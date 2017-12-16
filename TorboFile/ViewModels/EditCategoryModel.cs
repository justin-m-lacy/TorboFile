using TorboFile.Categories;
using LerpingLemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LerpingLemur.Windows;
using System.ComponentModel.DataAnnotations;
using LerpingLemur.Windows.Input;

namespace TorboFile.ViewModels {

	public class EditCategoryModel : ViewModelBase {

		/// <summary>
		/// Prevent duplicate category names.
		/// </summary>
		private CategorySet categories;

		private FileCategory editCategory;
		/// <summary>
		/// Category being edited (if any) as opposed to creating a new category.
		/// </summary>
		public FileCategory EditCategory {
			get { return this.editCategory; }
			set {

				if( this.editCategory != value ) {

					this.editCategory = value;
					if( value != null ) {
						this.NewName = value.Name;
						this.NewPath = value.DirectoryPath;
						this.KeyGesture = value.Gesture;
					}

					this.NotifyPropertyChanged();

				}

			} //set()

		} // EditCategory


		private RelayCommand _cmdSubmit;
		public RelayCommand CmdSubmit {
			get {
				return _cmdSubmit ?? ( this._cmdSubmit = new RelayCommand(

				  () => {
					  if( this.editCategory != null ) {

						  this.editCategory.DirectoryPath = this.NewPath;
						  this.editCategory.Name = this.NewName;
						  this.editCategory.Gesture = this.KeyGesture;

						  int index = this.categories.IndexOf( this.editCategory );
						  if( index >= 0 ) {
							  this.categories[index] = this.editCategory;		// Force update hack.
						  }

					  } else {
						  this.categories.Add( new FileCategory( this._name, this._path, this.KeyGesture ) );
					  }


					  this.TryClose();

				  },

				  () => { return ( !string.IsNullOrEmpty(this._name) && !string.IsNullOrEmpty(this._path) ); } )
			); }

		} // CmdSubmit

		private string _name;
		public string NewName {

			get { return this._name; }
			set {

				/*if( !VerifyNameChars( value ) ) {
					throw new ValidationException( "Invalid category name." );
				}*/
				this._name = value;

				this.CmdSubmit.RaiseCanExecuteChanged();

				this.NotifyPropertyChanged();
			}

		}

		private string _path;
		public string NewPath {

			get {
				return this._path;
			}
			set {

				/*Console.WriteLine( "validation attempt" );
				if( !VerifyPathChars( value ) ) {
					Console.WriteLine( "Validation fail" );
					throw new ValidationException( "Invalid file path." );
				}*/
				this._path = value;

				this._cmdSubmit.RaiseCanExecuteChanged();

				this.NotifyPropertyChanged();

			}

		}

		/// <summary>
		/// Key combination used to sort files to the category.
		/// </summary>
		private FreeKeyGesture _keyGesture;
		public FreeKeyGesture KeyGesture {
			get { return this._keyGesture; }
			set {

				if( value == null && this._keyGesture == null ) {
					Console.WriteLine( "BOTH KEYS NULL" );
					return;
				}

				if( this._keyGesture == null || !this._keyGesture.Equals( value ) ) {
					Console.WriteLine( "Model key changing" );
					this._keyGesture = value;
					NotifyPropertyChanged();
				}

			}

		}

		public EditCategoryModel( CategorySet baseSet, FileCategory editing=null ) {

			this.EditCategory = editing;
			this.categories = baseSet;

		}

		private bool VerifyNameChars( string name ) {

			int len = name.Length;
			char[] invalid = Path.GetInvalidFileNameChars();

			for( int i = 0; i < len; i++ ) {

				char c = name[i];
				if( invalid.Contains( c ) ) {
					return false;
				}

			}

			return true;

		}

		private bool VerifyPathChars( string path ) {

			int len = path.Length;
			char[] invalid = Path.GetInvalidPathChars();

			for( int i = 0; i < len; i++ ) {

				char c = path[i];
				if( invalid.Contains( c ) ) {
					return false;
				}

			}

			return true;
		}

	} // class

} // namespace
