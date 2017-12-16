using LerpingLemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TorboFile.Services {

	public enum MessageBoxMode {
		Message = 0,
		Confirm
	}

	public enum MessageResult {
		Cancel,
		Accept,
		Reject
	}

	public interface IMessageBox {

		//Task<MessageResult> ShowMessageAsync( string msg );
		//Task<MessageResult> ShowConfirmAsync( string msg );

		MessageResult ShowMessage( ViewModelBase model, string title=null, string msg=null );
		MessageResult ShowConfirm( ViewModelBase model, string title=null, string msg=null );

	} //

} // namespace