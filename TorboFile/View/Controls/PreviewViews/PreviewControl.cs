using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TorboFile.View.Controls {

	public class PreviewControl : UserControl {

	/*	static public readonly DependencyProperty MediaShouldCloseProperty = DependencyProperty.Register(
	"MediaShouldClose", typeof( bool ), typeof( PreviewControl ),
	new FrameworkPropertyMetadata( false, new PropertyChangedCallback( ShouldCloseChanged ) ) );

		/// <summary>
		/// Action for subclasses to take when media closes.
		/// </summary>
		protected Action CloseMediaAction;

		public bool MediaShouldClose {
			get { return (bool)this.GetValue( MediaShouldCloseProperty ); }
			set {
				/// ensure property change.
				bool cur = (bool)this.GetValue( MediaShouldCloseProperty );
				this.SetValue( MediaShouldCloseProperty, !cur );
			}
		}

		private static void ShouldCloseChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {

			if( (bool)e.NewValue == true ) {
				PreviewControl c = (PreviewControl)d;
				Action onClose = c.CloseMediaAction;
				if( onClose != null ) {
					onClose();
				}
			}
		
		}*/

		public static readonly RoutedEvent PreviewFailedEvent = EventManager.RegisterRoutedEvent(
			"PreviewFailed", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( PreviewControl ) );

		/// <summary>
		/// The PreviewFailed event is linked to an interaction that calls a PreviewFailed command in the Model.
		/// This will attempt to switch the view to a more appropriate type.
		/// </summary>
		public event RoutedEventHandler PreviewFailed {
			add { AddHandler( PreviewFailedEvent, value ); }
			remove { RemoveHandler( PreviewFailedEvent, value ); }
		}

		protected void RaisePreviewFailed() {
			RaiseEvent( new RoutedEventArgs( PreviewFailedEvent, this ) );
		} //

	} // class

} // namespace
