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

		private void ComboBox_PreviewMouseDown( object sender, MouseButtonEventArgs e ) {

			if( !( (ComboBox)sender ).IsDropDownOpen ) {
				Console.WriteLine( "CHANGING SELECTED INDEX TO -1" );
				( (ComboBox)sender ).SelectedIndex = -1;
			}
		}

	} // class

} // namespace