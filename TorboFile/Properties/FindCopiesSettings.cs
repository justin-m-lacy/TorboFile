using System;
using System.ComponentModel;
using System.Linq;
using Lemur.Utils;
namespace TorboFile.Properties {

	/// <summary>
	/// NOTE: AppSettingsBase already implements INotifyPropertyChanged.
	/// </summary>
	internal sealed partial class FindCopiesSettings {

		private const char EXTENSION_CHAR = '.';

		private char[] illegalExtensionChars = { '?', '/', '\\' };
		private readonly char[] separators = { ';', ',' };

		public FindCopiesSettings() {

			/// Add any System-level invalid file characters, and ensure they are all distinct.
			this.illegalExtensionChars = (char[])this.illegalExtensionChars.Concat( System.IO.Path.GetInvalidFileNameChars() ).Distinct().ToArray();

			this.SettingsLoaded += FindDuplicates_SettingsLoaded;

		} // FindDuplicates()

		private void FindDuplicates_SettingsLoaded( object sender, System.Configuration.SettingsLoadedEventArgs e ) {

			if( this.rememberExtensions ) {

				this._include = this.includeExtensions;
				this._exclude = this.excludeExtensions;

			}

		} //

		/// <summary>
		/// Used to store the 'includeExtensions' setting when the setting is not being
		/// remembered between uses. The official setting is not saved if remember option is
		/// not enabled.
		/// </summary>
		private string _include;
		public string IncludeExtensions {

			get { return this._include; }

			set {

				if( value != this._include ) {
					this._include = value;
					if( this.rememberExtensions ) {
						this.includeExtensions = value;
					}
					this.OnPropertyChanged( this, new PropertyChangedEventArgs( "IncludeExtensions" ) );
				}

			} // set()

		} // IncludeExtensions()

		/// <summary>
		/// Used to store the 'excludeExtensions' setting when the setting is not being
		/// remembered between uses. The official setting is not saved if remember option is
		/// not enabled.
		/// </summary>
		private string _exclude;
		public string ExcludeExtensions {

			get { return this._exclude; }

			set {

				if( value != this._exclude ) {
					this._exclude = value;
					if( this.rememberExtensions ) {
						this.excludeExtensions = value;
					}
					this.OnPropertyChanged( this, new PropertyChangedEventArgs( "ExcludeExtensions" ) );

				}

			} // set()

		} // ExcludeExtensions()
		
		/// <summary>
		/// Parse a string into an extensions array.
		/// </summary>
		/// <param name="extensionString"></param>
		/// <returns></returns>
		public string[] ParseExtensions( string extensionString ) {

			if( string.IsNullOrEmpty( extensionString ) ) {
				return null;
			}

			string[] extensions = extensionString.Split( this.separators, StringSplitOptions.RemoveEmptyEntries );
			int len = extensions.Length;
			for( int i = len - 1; i >= 0; i-- ) {

				string ext = extensions[i];
				if( ext.Contains( this.illegalExtensionChars ) ) {
					ext = extensions[i] = ext.RemoveChars( this.illegalExtensionChars );
				}

				if( ext[0] != EXTENSION_CHAR ) {
					extensions[i] = EXTENSION_CHAR + ext;
				}

			}

			if( extensions.Length == 0 ) {
				return null;
			}
			return extensions;

		}

	} // class

} // namespace