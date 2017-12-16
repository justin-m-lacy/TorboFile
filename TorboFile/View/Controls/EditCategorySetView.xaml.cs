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

namespace TorboFile.Controls {

    /// <summary>
    /// Interaction logic for EditCategorySetView.xaml
    /// </summary>
    public partial class EditCategorySetView : UserControl {

        public EditCategorySetView()
		{
            InitializeComponent();
        }

		private void BtnCancel_Click( object sender, RoutedEventArgs e ) {
			Window.GetWindow( this ).Close();
		}

	}

}
