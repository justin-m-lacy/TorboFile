using LerpingLemur.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static LerpingLemur.Debug.DebugUtils;

namespace TorboFile {

	/// <summary>
	/// Interaction logic for ProgressWindow.xaml
	/// </summary>
	public partial class ProgressWindow : Window {

		private ProgressOperation operation;

		private bool autoClose = true;
		/// <summary>
		/// Whether to automatically close the ProgressWindow on complete.
		/// true by default.
		/// </summary>
		public bool AutoClose {
			get {
				return this.autoClose;
			}
			set {
				this.autoClose = value;
			}
		}

		public ProgressWindow() {
			this.InitializeComponent();
		}

		public void WatchProgress( ProgressOperation op ) {

			this.operation = op;
			if ( op != null ) {

				this.operation.ProgressChanged += this.OnProgress;
				this.UpdateDisplay( op.ProgressInformation );

			}

		}

		private void BtnCancel_Click( object sender, RoutedEventArgs e ) {

			if ( this.operation != null ) {

				this.operation.ProgressChanged -= this.OnProgress;
				this.operation.Cancel();

			}
			this.Close();
					
		} //

		private void OnProgress( object sender, ProgressInformation p ) {

			ProgressControl.Maximum = p.MaxProgress;
			ProgressControl.Value = p.CurProgress;

			if ( p.CurProgress >= p.MaxProgress ) {

				ProgressComplete();

			}

		}

		/// <summary>
		/// Update the progress display.
		/// </summary>
		/// <param name="info"></param>
		public void UpdateDisplay( ProgressInformation info ) {

			if ( info == null ) {

				this.ProgressControl.Value = 0;

			} else {
				this.ProgressControl.Maximum = info.MaxProgress;
				this.ProgressControl.Value = info.CurProgress;
			}

		}

		private void ProgressComplete() {

			if ( this.autoClose ) {

				if ( this.operation != null ) {
					this.operation.ProgressChanged -= this.OnProgress;
					this.operation = null;
				}
				this.Close();
			}

		}

	} // class

} // namespace