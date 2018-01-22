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

		#region RUNTIME PROPERTIES

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
					this.OnPropertyChanged( this, new PropertyChangedEventArgs( "IncludeExtensions" ) );
				}

			} // set()

		} // IncludeExtensions()

		/// <summary>
		/// Used to store the 'excludeExtensions' setting when the setting is not being
		/// remembered between uses. The backing setting is not saved if remember option is
		/// not enabled.
		/// </summary>
		private string _exclude;
		public string ExcludeExtensions {

			get { return this._exclude; }

			set {

				if( value != this._exclude ) {
					this._exclude = value;
					this.OnPropertyChanged( this, new PropertyChangedEventArgs( "ExcludeExtensions" ) );

				}

			} // set()

		} // ExcludeExtensions()

		  /// <summary>
		  /// Only stored to the backing setting if saveLastDirectory is true.
		  /// Events don't make the LastDirectory property unnecessary, since the setting still needs
		  /// to remember the value for the running application, even if the backing setting isn't
		  /// changed.
		  /// </summary>
		private string _lastDirectory;
		public string LastDirectory {
			get => this._lastDirectory;
			set {

				if( this._lastDirectory != value ) {

					this._lastDirectory = value;
					this.OnPropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( "LastDirectory" ) );
				}

			}
		}

		#endregion

		public FindCopiesSettings() {

			/// Add any System-level invalid file characters, and ensure they are all distinct.
			this.illegalExtensionChars = (char[])this.illegalExtensionChars.Concat( System.IO.Path.GetInvalidFileNameChars() ).Distinct().ToArray();

			this.SettingsLoaded += FindDuplicates_SettingsLoaded;
			this.SettingsSaving += FindCopiesSettings_SettingsSaving;

		} // FindDuplicates()

		private void FindCopiesSettings_SettingsSaving( object sender, CancelEventArgs e ) {

			if( this.saveExtensions ) {
				this.includeExtensions = this._include;
				this.excludeExtensions = this._exclude;
			} else {
				this.includeExtensions = this.excludeExtensions = null;
			}

			if( this.saveLastDirectory ) {
				this.lastDirectory = this._lastDirectory;
			} else {
				this.lastDirectory = null;
			}

		}

		private void FindDuplicates_SettingsLoaded( object sender, System.Configuration.SettingsLoadedEventArgs e ) {

			if( this.saveExtensions ) {

				this._include = this.includeExtensions;
				this._exclude = this.excludeExtensions;

			}
			if( this.saveLastDirectory ) {
				this._lastDirectory = this.lastDirectory;
			}

		} //
		
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