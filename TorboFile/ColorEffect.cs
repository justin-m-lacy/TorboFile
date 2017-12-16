using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace TorboFile {

	public class ColorEffect : ShaderEffect {

		private static PixelShader _pixShader = new PixelShader() {
			UriSource = new Uri(
				"pack://siteoforigin:,,,/assets/shaders/ForegroundShader.cso" ) };

		private static Uri MakePackUri( string relativeFile ) { 

			Assembly a = typeof( ColorEffect ).Assembly;

			string assemblyName = a.ToString().Split( ',' )[0];
			Console.WriteLine( "ASSEMBLY: " + assemblyName );
			string uriString =
				@"pack://application:,,,/" + assemblyName + @";component/" + relativeFile;
			Console.WriteLine( "URI: " + uriString );
			return new Uri( uriString );

		}

		public Color Color {
			get { return (Color)this.GetValue( ColorProperty ); }
			set { this.SetValue( ColorProperty, value );
				this.UpdateShaderValue( ColorProperty );
			}
		}
		public Brush Input {
			get { return (Brush)this.GetValue( InputProperty ); }
			set { this.SetValue( InputProperty, value ); }
		}

		public ColorEffect() {

			try {

				this.PixelShader = new PixelShader() {
					UriSource = new Uri(
				"pack://siteoforigin:,,,/assets/shaders/ForegroundShader.cso" )
				};
				/// Update Shader variables with class Properties.
				UpdateShaderValue( ColorProperty );
				UpdateShaderValue( InputProperty );

				Console.WriteLine( "SHADER COLOR: " + this.Color.ToString() );

			} catch( Exception e ) {
				Console.WriteLine( "SHADER: " + e.ToString() );
			}

		}

		public static readonly DependencyProperty InputProperty =
			// !Associates the Property with the Shader's 0-index sampler variable.
			ShaderEffect.RegisterPixelShaderSamplerProperty( "Input", typeof( ColorEffect ), 0 );

		public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
			"Color", typeof( Color ), typeof( ColorEffect ), new UIPropertyMetadata(
				Colors.White,
				
				/// Associates the Color Property with the shader's 0-index float variable.
				PixelShaderConstantCallback(0) )
		);

    } // class

} // namespace
