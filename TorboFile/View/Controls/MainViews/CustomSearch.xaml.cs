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
	/// Interaction logic for CustomSearch.xaml
	/// </summary>
	public partial class CustomSearch : UserControl {

		public CustomSearch() {

			InitializeComponent();

		}

		/// <summary>
		/// Used to trigger ComboBox SelectedChanged events even if the selection hasn't changed.
		/// This makes it easy to repeatedly add the same search criterion with a single mouse
		/// press/release. (For example, multiple size restrictions, name contains, etc. )
		/// TODO: possibly restore the selected index if no selection made?
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {

			ComboBox box = sender as ComboBox;
			if( !box.IsDropDownOpen ) {
				box.SelectedIndex = -1;
			}

		}

	} // class

} // namespace