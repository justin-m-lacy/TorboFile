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
using System.Windows.Threading;
using WPFMediaKit;
using WPFMediaKit.DirectShow.MediaPlayers;

namespace TorboFile.View.Controls {
    /// <summary>
    /// Interaction logic for MediaKitPreview.xaml
    /// </summary>
    public partial class MediaKitPreview : PreviewControl {

		private DispatcherTimer seekBarTimer;

        public MediaKitPreview() {

            InitializeComponent();

			this.seekBarTimer = new DispatcherTimer();
			this.seekBarTimer.Interval = TimeSpan.FromMilliseconds( 250 );

        }

		private void UnloadPreview() {

			Console.WriteLine( "unloading preview" );

			this.seekBarTimer.Stop();
			this.mediaElement.ClearValue( MediaElement.SourceProperty );
			this.mediaElement.Close();

		}

		private void BtnPause_Click( object sender, RoutedEventArgs e ) {

			this.seekBarTimer.Stop();
			this.mediaElement.Pause();

		} //

		private void BtnPlay_Click( object sender, RoutedEventArgs e ) {

			this.seekBarTimer.Start();
			this.mediaElement.Play();

		}

		private void BtnStop_Click( object sender, RoutedEventArgs e ) {

			this.seekBarTimer.Stop();
			this.mediaElement.Stop();

		}

		private bool dragging;
		private void scrubbingBar_DragStarted( object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e ) {

			dragging = true;

		}

		private void scrubbingBar_DragCompleted( object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e ) {
			dragging = false;
		}

		private void SeekBarTimer_Tick( object sender, EventArgs e ) {

			// NOTE: mediaElement.MediaPlayer.MediaPositionChanged does not appear to run on UI thread.

			if( dragging ) {
				long pos = (long)( this.scrubbingBar.Value * this.mediaElement.MediaDuration );
				this.mediaElement.MediaPosition = pos;
			} else {
				this.scrubbingBar.Value = (double)( this.mediaElement.MediaPosition );
			}

		}

		/// <summary>
		/// TODO: this is annoying because it makes a call loop with tick->value->mediaPos.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void scrubbingBar_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			if( this.mediaElement.IsLoaded ) {
				this.mediaElement.MediaPosition = (long)e.NewValue;
			}

		} //

		private void mediaElement_Unloaded( object sender, RoutedEventArgs e ) {
			Console.WriteLine( "Media_Element UNLOADED" );
			this.seekBarTimer.Stop();
		}

		private void mediaElement_SourceUpdated( object sender, DataTransferEventArgs e ) {
			Console.WriteLine( "SOURCE UPDATED" );
			this.mediaElement.MediaPosition = 0;
		}

		private void mediaElement_TargetUpdated( object sender, DataTransferEventArgs e ) {
			Console.WriteLine( "TARGET UPDATED" );
		}

		private void mediaElement_MediaOpened( object sender, RoutedEventArgs e ) {
			this.seekBarTimer.Tick += this.SeekBarTimer_Tick;
			this.mediaElement.Loop = true;
			this.mediaElement.Pause();

		}

		private void mediaElement_MediaFailed( object sender, WPFMediaKit.DirectShow.MediaPlayers.MediaFailedEventArgs e ) {

			// TODO: don't raise for minor playback errors.
			this.RaisePreviewFailed();
		}

		private void mediaElement_MediaClosed( object sender, RoutedEventArgs e ) {
			this.seekBarTimer.Tick -= this.SeekBarTimer_Tick;
		}

		
	} // class

} // namespace
