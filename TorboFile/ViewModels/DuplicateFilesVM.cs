using Lemur;
using Lemur.Types;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Model for a file check list.
	/// </summary>
	public class DuplicateFilesVM : FileCheckListVM {

		#region COMMANDS

		#endregion

		/// <summary>
		/// Maps file paths to duplicate and size information about the file.
		/// </summary>
		private Dictionary<string, FileDuplicateInfo> duplicateInfos;

		public DuplicateFilesVM() : base() {

			this.duplicateInfos = new Dictionary<string, FileDuplicateInfo>();

			this.CheckedItems.CollectionChanged += this.CheckedItems_CollectionChanged;
		}

		public void AddDuplicateInfo( FileDuplicateInfo info, bool isChecked=false ) {

			this.duplicateInfos[info.Path] = info;
			this.Items.Add( new ListItemVM<FileSystemInfo>( new FileInfo( info.Path ), isChecked ) );

		}

		/// <summary>
		/// Set of Checked FileData objects has changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckedItems_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e ) {

			long curSize = this.CheckedSize.Bytes;

			if( e.NewItems != null ) {

				foreach( FileSystemInfo data in e.NewItems ) {

					if( data is FileInfo ) {
						curSize += ((FileInfo)data).Length;
					}

				}

			}
			if( e.OldItems != null ) {

				foreach( FileSystemInfo data in e.OldItems ) {

					if( data is FileInfo ) {
						curSize -= ( (FileInfo)data ).Length;
					}

				}

			}

			this.CheckedSize = curSize;

			this.CmdOpenChecked.RaiseCanExecuteChanged();
			this.CmdShowLocation.RaiseCanExecuteChanged();

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

	} // class

} // namespace