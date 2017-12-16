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
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl {

		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( object ), typeof( ProgressControl ) );

		/*public static readonly DependencyProperty ProgressProperty =
			DependencyProperty.Register( "Progress", typeof( Progress<float> ), typeof( ProgressControl ) );*/

		public static readonly DependencyProperty CancelProperty =
			DependencyProperty.Register( "Cancel", typeof( ICommand ), typeof( ProgressControl ) );

		/// <summary>
		/// Command to execute in order to cancel the operation.
		/// </summary>
		public ICommand Cancel {
			get { return (ICommand)GetValue( CancelProperty ); }
			set { SetValue( CancelProperty, value ); }
		}

		public object Message {

			get { return GetValue( MessageProperty ); }
			set {

				object current = GetValue( MessageProperty );
				if( value != current ) {
					SetValue( MessageProperty, value );
				}

			}

		}

		/*public Progress<float> Progress {
			get { return (Progress<float>)GetValue( ProgressProperty ); }
			set {

				Progress<float> current = Progress;
				if( current != value ) {

					if( current != null ) {
						current.ProgressChanged -= Value_ProgressChanged;
					}
					value.ProgressChanged += Value_ProgressChanged;
					SetValue( ProgressProperty, value );

				}

			} // set()
	
		}*/

		/// <summary>
		/// Automatically remove the control from the visual tree when progress is complete.
		/// </summary>
		public bool AutoRemove {
			get;
			set;
		}

        public ProgressControl() {
			
			this.InitializeComponent();


        } // ProgressControl()

	} // class

} // namespace
