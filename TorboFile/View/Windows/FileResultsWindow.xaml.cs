using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Lemur.Utils;
using static Lemur.Debug.DebugUtils;

namespace TorboFile {

	/// <summary>
	/// Interaction logic for FileResultsWindow.xaml
	/// </summary>
	public partial class FileResultsWindow : Window {

		/*
private RelayCommand<FileInfo> _toggleFileCommand;
/// <summary>
/// Toggle a file being checked.
/// </summary>
public RelayCommand<FileInfo> ToggleFileCommand {
	get {
		return _toggleFileCommand ?? (
			_toggleFileCommand = new RelayCommand<FileInfo>(
				(FileInfo f) => {
				}
			)
		);
	}
}*/

		//private ObservableCollection<FileData> filePaths;
		/*public void SetResults( List<FileData> files ) {

			this.FileList.SetFiles( files );

		}*/

		public FileResultsWindow() {

			this.InitializeComponent();

		} //

		/// <summary>
		/// Delete selected items clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/*private async void BtnDelete_Click( object sender, RoutedEventArgs args ) {

			FileData[] fileDatas = this.FileList.SelectedItems;
			int len = fileDatas.Length;

			string[] paths = new string[len];

			for ( int i = 0; i < len; i++ ) {

				paths[i] = fileDatas[i].path;
	
			} //

			FileDeleteOp op = new FileDeleteOp( paths );
			await App.RunOperationAsync( op );

			this.FileList.RemoveSelected();

		} //*/

	} // class

} // namespace