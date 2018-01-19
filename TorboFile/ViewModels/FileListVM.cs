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

	public class FileListVM : CheckListVM<FileSystemInfo> {

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
		public RelayCommand CmdShowLocation {

			get {
				return this._cmdShowLocation ?? ( this._cmdShowLocation = new RelayCommand(

			  () => {

				  foreach( var ck in this.CheckedItems ) {
					  AppUtils.ShowExternalAsync( Path.GetDirectoryName( ck.FullName ) );
				  }

			  },

			  this.HasCheckedItems

			  ) );
			}

			set {
				if( this._cmdShowLocation != value ) {
					this._cmdShowLocation = value;
					this.NotifyPropertyChanged();
				}
			} //

		} // CmdShowLocation
		private RelayCommand _cmdShowLocation;

		#endregion

		#region DISPLAY OPTIONS

		public bool ShowPath { get => showPath;
			set => this.SetProperty( ref this.showPath, value );
		}
		public bool ShowName { get => showName; set => this.SetProperty( ref this.showName, value ); }
		public bool ShowSize { get => showSize; set => this.SetProperty( ref this.showSize, value ); }

		/// <summary>
		/// Whether to display a checkbox for selecting files
		/// from the list.
		/// </summary>
		public bool ShowCheckBox { get => showCheckBox; set => this.SetProperty( ref this.showCheckBox, value ); }
		public bool ShowCreateTime { get => showCreateTime; set => this.SetProperty( ref this.showCreateTime, value ); }
		public bool ShowModifyTime { get => showModifyTime; set => this.SetProperty( ref this.showModifyTime, value ); }
		public bool ShowExtension { get => showExtension; set => this.SetProperty( ref this.showExtension, value ); }

		/// <summary>
		/// Whether to display an indication of whether the file
		/// is a File or Directory.
		/// </summary>
		public bool ShowIsFile { get => showIsFile; set => this.SetProperty( ref this.showIsFile, value ); }

		/// <summary>
		/// Display a delete command.
		/// </summary>
		public bool ShowDeleteCmd { get => showDeleteCmd; set => this.SetProperty( ref this.showDeleteCmd, value ); }
		/// <summary>
		/// Display an open command.
		/// </summary>
		public bool ShowOpenCmd { get => showOpenCmd; set => this.SetProperty( ref this.showOpenCmd, value ); }
		/// <summary>
		/// Display an Open Location command.
		/// </summary>
		public bool ShowOpenLocationCmd { get => showLocationCmd; set => this.SetProperty( ref this.showLocationCmd, value ); }

		private bool showPath;
		private bool showName;
		private bool showSize;
		private bool showCheckBox;
		private bool showCreateTime;
		private bool showModifyTime;
		private bool showExtension;
		private bool showIsFile;
		private bool showDeleteCmd;
		private bool showOpenCmd;
		private bool showLocationCmd;

		#endregion

		private Action<IEnumerable<FileSystemInfo>> _deleteAction;

		/// <summary>
		/// Delegate for deleting files.
		/// TODO: move version to base class?
		/// </summary>
		public Action<IEnumerable<FileSystemInfo>> DeleteDelegate {
			get { return this._deleteAction; }
			set {
				if( this._deleteAction != value ) {
					this._deleteAction = value;
					this.NotifyPropertyChanged();
				}
			}
		}

		public FileListVM() : base() {

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
