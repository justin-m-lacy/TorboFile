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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LerpingLemur.Windows.Controls {

	/// <summary>
	/// A label that displays a feedback message with optional timer/icon.
	/// </summary>
	public partial class MessageLabel : UserControl {

		/// <summary>
		/// Color of the message.
		/// </summary>
		public Brush TextColor {

			get { return (Brush)GetValue( UserControl.ForegroundProperty ); }
			set { this.SetValue( UserControl.ForegroundProperty, value ); }

		} // TextColor

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof( string ), typeof( MessageLabel ), new PropertyMetadata( string.Empty ) );

		public string Text {
			get { return (string)GetValue( TextProperty ); }

			set {
				this.LblMessage.Content = value;
				SetValue( TextProperty, value );
			}

		}

		/// <summary>
		/// Error feedback animation.
		/// </summary>
		private DoubleAnimation fadeFeedbackAnimation;

		/// <summary>
		/// Animation for feedback fade-out.
		/// </summary>
		private Storyboard fadeFeedbackBoard;

		public MessageLabel() {

			this.InitializeComponent();

		} // MessageLabel()

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message">Message to display in the LblMessage.</param>
		/// <param name="displayTime">Time in seconds to display the message. Values of zero or less means the message will not go away.</param>
		public void Display( string message, float displayTime=0 ) {

			//this.LblMessage.Text = message;
			this.LblMessage.Content = message;

			// fade out error message.
			if( displayTime > 0 ) {

				if( this.fadeFeedbackAnimation == null ) {
					this.InitAnimations();
				}

				//Console.WriteLine( "Attempting to start animation." );

				this.fadeFeedbackAnimation.Duration = new Duration( new TimeSpan( (long)( TimeSpan.TicksPerSecond * displayTime ) ) );
				/*this.fadeFeedbackBoard.Completed += ( object sender, EventArgs e ) => {
					Console.WriteLine( "ANIMATION COMPLETE" );
				};*/

				this.fadeFeedbackBoard.Begin( this, true );

			}

		} // Display()

		private void InitAnimations() {

			/// boilerplate things to make animations work, apparently.
			NameScope.SetNameScope( this, new NameScope() );
			this.RegisterName( this.LblMessage.Name, this.LblMessage );

			DoubleAnimation fadeFeedback = new DoubleAnimation( 1.0, 0, new Duration( new TimeSpan( 0, 0, 3 ) ) );
			this.fadeFeedbackAnimation = fadeFeedback;

			this.fadeFeedbackBoard = new Storyboard();
			this.fadeFeedbackBoard.Children.Add( fadeFeedback );

			Storyboard.SetTargetName( fadeFeedback, LblMessage.Name );
			Storyboard.SetTargetProperty( fadeFeedback, new PropertyPath( Label.OpacityProperty ) );

		}

	} // class

} // namespace