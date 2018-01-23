using Lemur.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorboFile.Model;

namespace TorboFile.Properties {

	internal sealed partial class CustomSearchSettings {

		/// <summary>
		/// Directory of IsolatedStorage for saving CustomSearch related information.
		/// </summary>
		private const string CUSTOM_SEARCH_DIR = "CustomSearch";

		/// <summary>
		/// Name of file for storing the last custom search data.
		/// </summary>
		private const string LAST_SEARCH_FILE = "lastSearch.tsch";

		public CustomSearchSettings() {

			this.SettingsLoaded += CustomSearchSettings_SettingsLoaded;
			this.PropertyChanged += CustomSearchSettings_PropertyChanged;
		}

		/// <summary>
		/// Used PropertyChanged instead of SettingsSaving() so the changes go through even if the program or computer
		/// crashes before the settings have saved.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CustomSearchSettings_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {

			if( e.PropertyName == "saveLastDirectory" ) {

				if( this.saveLastDirectory ) {
					this.lastDirectory = this._lastDirectory;
				} else {
					this.lastDirectory = null;
				}
				this.Save();

			}

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
		/// Saves the last search into isolated storage.
		/// </summary>
		/// <param name="search"></param>
		static public void SaveLastSearch( CustomSearchData search ) {

			if( IsolatedStorageFile.IsEnabled ) {

				try {
	
					IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

					if( !store.DirectoryExists( CUSTOM_SEARCH_DIR ) ) {
						store.CreateDirectory( CUSTOM_SEARCH_DIR );
					}

					using( IsolatedStorageFileStream stream = store.OpenFile(
						Path.Combine( CUSTOM_SEARCH_DIR + LAST_SEARCH_FILE ),
						System.IO.FileMode.OpenOrCreate ) ) {
						FileUtils.WriteBinary( stream, search );
					}

				} catch( Exception e ) {
					Console.WriteLine( e.ToString() );
				}

			}

		}

		static public CustomSearchData LoadLastSearch() {

			if( IsolatedStorageFile.IsEnabled ) {

				try {

					IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
					string filePath = Path.Combine( CUSTOM_SEARCH_DIR + LAST_SEARCH_FILE );

					if( store.FileExists( filePath ) ) {

						using( IsolatedStorageFileStream stream = store.OpenFile( filePath, System.IO.FileMode.Open ) ) {

							return FileUtils.ReadBinary<CustomSearchData>( stream );

						}

					}

				} catch( Exception e ) {
					Console.WriteLine( e.ToString() );
				}

			}

			return null;

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
