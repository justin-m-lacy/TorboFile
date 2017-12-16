using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Properties {

    internal sealed partial class SortingSettings {

		public SortingSettings() {

			this.SettingsLoaded += Sorting_SettingsLoaded;
			this.SettingChanging += Sorting_SettingChanging;
		}

		private void Sorting_SettingChanging( object sender, System.Configuration.SettingChangingEventArgs e ) {

			if( e.SettingName == "saveLastDirectory" && (bool)e.NewValue == true ) {
				this.lastDirectory = this._lastDirectory;
			}

		}

		/// <summary>
		/// Non-saved lastDirectory for the current session.
		/// </summary>
		private string _lastDirectory;
		public string LastDirectory {
			get { return this._lastDirectory; }
			set {

				if( value != this._lastDirectory ) {

					if( this.saveLastDirectory ) {
						this.lastDirectory = value;
					}
					this._lastDirectory = value;
					this.OnPropertyChanged( this, new System.ComponentModel.PropertyChangedEventArgs( "LastDirectory" ) );

				}

			}

		} //

		private void Sorting_SettingsLoaded( object sender, System.Configuration.SettingsLoadedEventArgs e ) {

			if( this.saveLastDirectory ) {
				this._lastDirectory = this.lastDirectory;
			} else {
				this._lastDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyPictures );
			}

		}


	} // class

}