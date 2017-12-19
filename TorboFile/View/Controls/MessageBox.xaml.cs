using TorboFile.Services;
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

	public class MessageResultArgs : RoutedEventArgs {

		public MessageResult result;

		public MessageResultArgs( RoutedEvent evt, object sender, MessageResult result ) : base( evt, sender ) {
			this.result = result;
		}

	} // class

	public delegate void MessageResultHandler( object sender, MessageResultArgs e );

    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : UserControl {

		internal enum MessageType {
			Message,
			Confirm
		};

		private const string DefaultTitle = "Message Box";
		private const string DefaultMessage = "Something happened.";

		public static readonly RoutedEvent MessageClosedEvent =
			EventManager.RegisterRoutedEvent( "MessageClosed", RoutingStrategy.Bubble,
				typeof( MessageResultHandler ), typeof( MessageBox ) );

		public event MessageResultHandler MessageClosed {
			add { AddHandler( MessageClosedEvent, value ); }
			remove { RemoveHandler( MessageClosedEvent, value ); }
		}

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register( "Title", typeof( object ), typeof( MessageBox ), new PropertyMetadata( DefaultTitle ) );
		public static readonly DependencyProperty MessageProperty =
			DependencyProperty.Register( "Message", typeof( object ), typeof( MessageBox ), new PropertyMetadata( DefaultMessage ) );
	
		/// <summary>
		/// Title Content.
		/// </summary>
		public object Title {
			get {
				return this.GetValue( TitleProperty );
			}
			set {
				if( value != this.GetValue( TitleProperty ) ) {
					this.SetValue( TitleProperty, value );
				}
			}
		}

		/// <summary>
		/// Message Content.
		/// </summary>
		public object Message {
			get {
				return this.GetValue( MessageProperty );
			}
			set {
				if( value != this.GetValue( MessageProperty ) ) {
					this.SetValue( MessageProperty, value );
				}
			}
		}

		public MessageBox() {
            this.InitializeComponent();
        }

		/*internal MessageType DialogType {
			get;
			set;
		}*/

		public void ShowMessage( string title, string msg ) {

			this.Title = title;
			this.Message = msg;
			this.BtnConfirm.Visibility = Visibility.Hidden;

		} //

		public void ShowConfirm( string title, string msg  ) {

			this.Title = title;
			this.Message = msg;


		} //

		private void BtnConfirm_Click( object sender, RoutedEventArgs e ) {
			Console.WriteLine( "RAISING CONFIRM CLICK EVENT" );
			this.RaiseEvent( new MessageResultArgs( MessageClosedEvent, this, MessageResult.Accept ) );
		}

		private void BtnCancel_Click( object sender, RoutedEventArgs e ) {
			this.RaiseEvent( new MessageResultArgs( MessageClosedEvent, this, MessageResult.Reject ) );
		}

	} // class

} // namespace
