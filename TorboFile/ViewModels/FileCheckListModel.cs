using Lemur.Types;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Model for a file check list.
	/// </summary>
	public class FileCheckListModel : CheckListModel<FileData> {

		#region COMMANDS

		private RelayCommand _cmdShowExternal;
		public RelayCommand CmdShowExternal {

			get {
				return this._cmdShowExternal ?? ( this._cmdShowExternal = new RelayCommand(

			  () => {

				  foreach( var ck in this.CheckedItems ) {
					  AppUtils.ShowExternalAsync( ck.path );
				  }

			  },

			  this.HasCheckedItems

			  ) );
			}

			set {
				if( this._cmdShowExternal != value ) {
					this._cmdShowExternal = value;
					this.NotifyPropertyChanged();
				}
			} //

		} // CmdShowExternal

		#endregion

		private Action<IEnumerable<FileData>> _deleteAction;
		/// <summary>
		/// Delegate for deleting files.
		/// TODO: move version to base class?
		/// </summary>
		public Action<IEnumerable<FileData>> DeleteDelegate {
			get { return this._deleteAction; }
			set {
				if( this._deleteAction != value ) {
					this._deleteAction = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		public FileCheckListModel() : base() {

			this.CmdDelete = new RelayCommand(

					this.DeleteChecked,
					this.HasCheckedItems

				);

			this.CmdOpen = new RelayCommand( () => {

				string[] paths = this.Items.Where( ( item ) => { return item.IsChecked; } ).Select( ( item ) => { return item.Item.Path; } ).ToArray();
				App.Instance.OpenExternalAsync( paths );

			}, this.HasCheckedItems );



			this.CheckedItems.CollectionChanged += this.CheckedItems_CollectionChanged;
		}

		/// <summary>
		/// Set of checked FileData objects has changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckedItems_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) {

			long curSize = this.CheckedSize.Bytes;

			if( e.NewItems != null ) {

				foreach( FileData data in e.NewItems ) {
					curSize += data.size;
				}

			}
			if( e.OldItems != null ) {

				foreach( FileData data in e.OldItems ) {
					curSize -= data.size;
				}

			}

			this.CheckedSize = curSize;

		} //


		/// <summary>
		/// Combined file size of all checked items.
		/// </summary>
		private DataSize _checkedSize = new DataSize( 0 );
		public DataSize CheckedSize {

			get { return this._checkedSize; }
			set {
			
				if( value != this._checkedSize ) {
					this._checkedSize = value;
					this.NotifyPropertyChanged();
				}
			
			}

		} // CheckedSize()
			
		private void Groups_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) {

			if( e.NewItems != null ) {

				foreach( FileMatchGroup group in e.NewItems ) {

					Console.WriteLine( "Adding group: " + group.FileSize );
					long groupFileSize = group.FileSize;

					// Check all but the first element.
					IEnumerator<string> matches = group.GetEnumerator();

					if( matches.MoveNext() ) {
						this.Items.Add( new ListItemModel<FileData>( new FileData( matches.Current, groupFileSize ) ) );
					}

					while( matches.MoveNext() ) {
						this.Items.Add( new ListItemModel<FileData>( new FileData( matches.Current, groupFileSize ), true ) );
					}


				} // foreach()

			}

		} // Groups_CollectionChanged

		/// <summary>
		/// Delete selected files asynchronously.
		/// </summary>
		/// <returns></returns>
		private void DeleteChecked() {

			IEnumerable<ListItemModel<FileData>> items = this.GetCheckedItems();
			IEnumerable<FileData> files = items.Select( ( item ) => { return item.Item; } );

			this._deleteAction?.Invoke( files );

			this.Remove( items );

		} // DeleteCheckedAsync()

	} // class

} // namespace
