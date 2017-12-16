using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemur.Windows;
using TorboFile.Windows;
using TorboFile.Controls;
using System.Windows;
using Lemur.Windows.MVVM;

namespace TorboFile.Services {

	public class CustomMessageBox : IMessageBox {

		public CustomMessageBox() {
		}

		public MessageResult ShowConfirm( ViewModelBase model, string title=null, string msg=null ) {

			MessageBoxWindow win = new MessageBoxWindow();

			/// Should not be necessary since ShowDialog() will return a result if window is closed.
			//win.AddHandler( Controls.MessageBox.MessageClosedEvent, new MessageResultHandler( this.OnMessageClosed ) );

			bool? result = win.ShowConfirm( title, msg );
			if( result == null ) {
				return MessageResult.Cancel;
			} else if( result == true ) {
				return MessageResult.Accept;
			}
			return MessageResult.Reject;

		}

		public MessageResult ShowMessage( ViewModelBase model, string title=null, string msg=null ) {

			MessageBoxWindow win = new MessageBoxWindow();

			bool? result = win.ShowMessage( title, msg );
			if( result == null ) {
				return MessageResult.Cancel;
			} else if( result == true ) {
				return MessageResult.Accept;
			}
			return MessageResult.Cancel;
			

		} //

		/*private void OnMessageClosed( object sender, MessageResultArgs args ) {
		} //*/

	} // class

} // namespace