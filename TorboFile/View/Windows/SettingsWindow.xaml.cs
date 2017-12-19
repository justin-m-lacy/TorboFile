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

namespace TorboFile.View.Windows {

    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {

        public SettingsWindow() {

            this.InitializeComponent();
        }



		#region General Settings
		#endregion

		#region Clean Folders Settings

		#endregion

		#region Sort Files Settings
		#endregion

		private void visit_hyperlink( object sender, System.Windows.Navigation.RequestNavigateEventArgs e ) {
			System.Diagnostics.Process.Start( e.Uri.ToString() );
		}

	} // class

} // namespace
