using TorboFile.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TorboFile.View.Controls {
	/// <summary>
	/// Interaction logic for FileSortControl.xaml
	/// </summary>
	public partial class FileSortControl : UserControl {

		public FileSortControl() {

			InitializeComponent();

		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e ) {
			Focus();
		}

	} // class

} // namespace
