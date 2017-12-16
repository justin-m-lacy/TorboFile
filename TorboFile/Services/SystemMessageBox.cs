using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TorboFile.Services {

	public class SystemMessageBox : IMessageBox {

		public MessageResult ShowConfirm( ViewModelBase model, string title=null, string msg=null ) {

			MessageBoxResult result = MessageBox.Show( msg, string.Empty, MessageBoxButton.OKCancel );
			return GetResult( result );

		}

		public MessageResult ShowMessage( ViewModelBase model, string title = null, string msg = null ) {

			MessageBoxResult result = MessageBox.Show( msg );
			return GetResult( result );

		}

		/// <summary>
		/// Converts a Windows MessageBoxResult to the platform independent result.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		private MessageResult GetResult( MessageBoxResult result ) {

			if( result == MessageBoxResult.Cancel ) {
				return MessageResult.Cancel;
			} else if( result == MessageBoxResult.OK || result == MessageBoxResult.Yes ) {
				return MessageResult.Accept;
			}
			return MessageResult.Reject;

		}

	} // class

} // namespace