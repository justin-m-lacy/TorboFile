using TorboFile.Services;
using Lemur.Windows.MVVM;
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
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window {

        public MessageBoxWindow() {

            InitializeComponent();
			ListenClose();

        }

		private void ListenClose() {

			this.MessageBoxView.MessageClosed += MessageBoxView_MessageClosed;
	
		}

		/// <summary>
		/// Called when a Confirm or cancel message has been selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void MessageBoxView_MessageClosed( object sender, Controls.MessageResultArgs args ) {

			Console.WriteLine( "MESSAGE CLOSED EVENT RECEIVED" );
			if( args.result == MessageResult.Accept ) {
				this.DialogResult = true;
			} else {
				this.DialogResult = false;
			}

			this.Close();

		}

		/// <summary>
		/// Unregister events.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnClosed( EventArgs e ) {

			this.MessageBoxView.MessageClosed -= this.MessageBoxView_MessageClosed;
			base.OnClosed( e );

		}

		/// <summary>
		/// Display a confirm dialog box.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public bool? ShowConfirm( string title=null, string msg=null ) {
			
			this.MessageBoxView.ShowConfirm( title, msg );

			return this.ShowDialog();

		}

		/// <summary>
		/// Displays a basic message dialog box.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public bool? ShowMessage( string title=null, string msg = null ) {

			this.MessageBoxView.ShowMessage( title, msg );

			return this.ShowDialog();

		}

	} // MessageBoxWindow()

} // namespace