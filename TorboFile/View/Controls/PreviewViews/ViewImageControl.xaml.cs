using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;
using static LerpingLemur.Debug.DebugUtils;
using TorboFile.Controls;

namespace TorboFile.Controls {

	/// <summary>
	/// Interaction logic for ViewImageControl.xaml
	/// </summary>
	public partial class ViewImageControl : PreviewControl {

		/// <summary>
		/// The File Path being viewed by the control.
		/// </summary>
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register( "Source", typeof( string ), typeof( ViewImageControl ),
				new PropertyMetadata( string.Empty ) );

		public string Source {

			get { return (string)GetValue( SourceProperty ); }
			set { this.SetValue( SourceProperty, value );}

		} // Source


		public ViewImageControl() {
			InitializeComponent();
		}

		/// <summary>
		/// Image failed to load properly.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void viewImage_ImageFailed( object sender, ExceptionRoutedEventArgs e ) {

			Console.WriteLine( "LOAD IMAGE FAILed" );
			this.RaisePreviewFailed();

		} //

	} // class

} // namespace
