using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Properties {

	internal sealed partial class CustomSearchSettings {

		public CustomSearchSettings() {

			this.SettingsLoaded += CustomSearchSettings_SettingsLoaded;
			this.SettingsSaving += CustomSearchSettings_SettingsSaving;
		}

		private void CustomSearchSettings_SettingsSaving( object sender, System.ComponentModel.CancelEventArgs e ) {

			if( this.saveLastSearch ) {
			}

			if( this.saveLastDirectory ) {
				this.lastDirectory = this._lastDirectory;
			} else {
				this.lastDirectory = null;
			}

		}

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


		private void CustomSearchSettings_SettingsLoaded( object sender, System.Configuration.SettingsLoadedEventArgs e ) {

			if( this.saveLastDirectory ) {
				this._lastDirectory = this.lastDirectory;
			} else {
				this._lastDirectory = this.lastDirectory = null;
			}

		} //

	} // class

} // namespace
