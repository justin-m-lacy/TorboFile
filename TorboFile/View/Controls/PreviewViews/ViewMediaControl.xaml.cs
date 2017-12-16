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
using static LerpingLemur.Debug.DebugUtils;

namespace TorboFile.Controls {

	/// <summary>
	/// Interaction logic for ViewMediaControl.xaml
	/// </summary>
	public partial class ViewMediaControl : PreviewControl {

		public ViewMediaControl() {
			InitializeComponent();

			//this.CloseMediaAction = UnloadPreview;

		}
	
		private void UnloadPreview() {

			Console.WriteLine( "unloading preview" );

			this.mediaElement.ClearValue( MediaElement.SourceProperty );
			this.mediaElement.Close();

		}

		private void BtnPause_Click( object sender, RoutedEventArgs e ) {

			this.mediaElement.Pause();

		} //

		private void BtnPlay_Click( object sender, RoutedEventArgs e ) {

			this.mediaElement.Play();

		}

		private void Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {

			Log( "old value: " + e.OldValue );

			this.mediaElement.Position = TimeSpan.FromSeconds( e.NewValue );

		}

		private void BtnStop_Click( object sender, RoutedEventArgs e ) {

			this.mediaElement.Stop();

		}

		private void mediaElement_MediaFailed( object sender, ExceptionRoutedEventArgs e ) {

			Console.WriteLine( "media fail: " + e.ErrorException.InnerException.ToString() );
			Console.WriteLine( "exception: " + e.ErrorException.ToString() );

			// TODO: don't raise for minor playback errors.
			//this.RaisePreviewFailed();

		}

		private void mediaElement_Loaded( object sender, RoutedEventArgs e ) {

			//Console.WriteLine( "MEDIA LOADED" );
			MediaElement media = ( (MediaElement)sender );
			Console.WriteLine( "SOURCE URI: " + media.Source.AbsoluteUri );

			Duration mediaDuration = media.NaturalDuration;
			if( !mediaDuration.HasTimeSpan ) {
				Console.WriteLine( "LOADED: DURATION IS AUTOMATIC" );
			} else {
				Console.WriteLine( "DURATION IS: " + mediaDuration.TimeSpan );
			}

			this.mediaElement.ScrubbingEnabled = true;
			this.mediaElement.Pause();

		}

		private void mediaElement_Unloaded( object sender, RoutedEventArgs e ) {
			Console.WriteLine( "Media_Element UNLOADED" );

		}

		private void mediaElement_SourceUpdated( object sender, DataTransferEventArgs e ) {
			Console.WriteLine( "SOURCE UPDATED" );
		}

		private void mediaElement_TargetUpdated( object sender, DataTransferEventArgs e ) {
			Console.WriteLine( "TARGET UPDATED" );
		}

		private void mediaElement_MediaOpened( object sender, RoutedEventArgs e ) {

			Console.WriteLine( "MEDIA OPENED" );
			Duration mediaDuration = ( (MediaElement)sender ).NaturalDuration;
			if( !mediaDuration.HasTimeSpan ) {
				Console.WriteLine( "DURATION IS AUTOMATIC" );
			} else {
				Console.WriteLine( "DURATION IS: " + mediaDuration.TimeSpan );
			}

		}

	} // class

} // namespace