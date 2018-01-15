using Lemur.Types;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	public class FileCheckListVM : CheckListVM<FileSystemInfo> {

		#region COMMANDS

		/// <summary>
		/// Use the System to open selected files ( not checked. )
		/// </summary>
		public RelayCommand CmdOpenSelected {

			get {
				return this._cmdOpenSelected ?? ( this._cmdOpenSelected = new RelayCommand(


				() => {

					App.Instance.OpenExternalAsync( this.SelectedItem.Item.FullName );

				}, this.HasSelectedItems )

			  );
			}

			set {

				this.SetProperty( ref this._cmdOpenSelected, value );
			}

		} // CmdOpenSelected
		private RelayCommand _cmdOpenSelected;

		/// <summary>
		/// Open all checked files.
		/// </summary>
		public RelayCommand CmdOpenChecked {

			get {
				return this._cmdOpenChecked ?? ( this._cmdOpenChecked = new RelayCommand(

				() => {

					string[] paths = this.Items.Where( ( item ) => { return item.IsChecked; } ).Select( ( item ) => { return item.Item.FullName; } ).ToArray();
					App.Instance.OpenExternalAsync( paths );

				}, this.HasCheckedItems

			  ) );
			}

			set {

				this.SetProperty( ref this._cmdOpenChecked, value );

			} //

		} // CmdOpen
		private RelayCommand _cmdOpenChecked;

		/// <summary>
		/// Command to open the directory of the checked files in the system explorer.
		/// </summary>
		public RelayCommand CmdShowExternal {

			get {
				return this._cmdShowExternal ?? ( this._cmdShowExternal = new RelayCommand(

			  () => {

				  foreach( var ck in this.CheckedItems ) {
					  AppUtils.ShowExternalAsync( Path.GetDirectoryName( ck.FullName ) );
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
		private RelayCommand _cmdShowExternal;

		#endregion

		private Action<IEnumerable<FileSystemInfo>> _deleteAction;
		/// <summary>
		/// Delegate for deleting files.
		/// TODO: move version to base class?
		/// </summary>
		public Action<IEnumerable< FileSystemInfo> > DeleteDelegate {
			get { return this._deleteAction; }
			set {
				if( this._deleteAction != value ) {
					this._deleteAction = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		public FileCheckListVM() : base() {

			this.CmdDelete = new RelayCommand(

					this.DeleteChecked,
					this.HasCheckedItems
			);

		}

		/// <summary>
		/// Delete selected files asynchronously.
		/// </summary>
		/// <returns></returns>
		private void DeleteChecked() {

			IEnumerable<FileSystemInfo> files = this.RemoveCheckedItems();
			this._deleteAction?.Invoke( files );

		} // DeleteCheckedAsync()

	} // class

} // namespace
