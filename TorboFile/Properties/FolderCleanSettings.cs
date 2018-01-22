using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Properties {

	internal sealed partial class FolderCleanSettings {

		private string _lastDirectory;
		public string LastDirectory {
			get {
				return this._lastDirectory;
			}
			set {
				this._lastDirectory = value;
				if( this.saveLastDirectory ) {
					this.lastDirectory = value;
				}
			}
		}

		public FolderCleanSettings() {

			this.SettingsLoaded += FindEmpty_SettingsLoaded;
			this.SettingChanging += FolderCleanSettings_SettingChanging;
		}

		private void FolderCleanSettings_SettingChanging( object sender, System.Configuration.SettingChangingEventArgs e ) {

			if( this.saveLastDirectory ) {
				this.lastDirectory = this._lastDirectory;
			} else {
				this.lastDirectory = null;
			}

		}

		private void FindEmpty_SettingsLoaded( object sender, System.Configuration.SettingsLoadedEventArgs e ) {

			if( this.saveLastDirectory ) {
				this._lastDirectory = this.lastDirectory;
			} else {
				this.lastDirectory = string.Empty;
			}
			
			//Console.WriteLine( "DELETE RANGE: " + deleteRange.MinSize + " -> " + deleteRange.MaxSize );
			//Console.WriteLine( "PRESERVE RANGE: " + preserveRange.MinSize + " -> " + preserveRange.MaxSize );

		}

	} // class

} // namespace
