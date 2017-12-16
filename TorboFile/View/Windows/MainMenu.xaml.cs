using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Lemur.Utils;
using Lemur.Tasks;
using System.Threading;
using static Lemur.Debug.DebugUtils;
using TorboFile.Windows;
using Lemur.Windows.Converters;
using System.ComponentModel;
using System.Windows.Controls;

namespace TorboFile.Windows {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private TabNameConverter tabConverter = new TabNameConverter();

		private string _lastSelectedTab;

		public int LastSelectedTab {
			get {
				int returnTab = (int)tabConverter.Convert( this._lastSelectedTab, typeof(int), this.MainTabView, null );
				//Console.WriteLine( "returning tab: " + returnTab );
				return returnTab;
			}
			set {

				//Console.WriteLine( "Selected index: " + value.ToString() );
				/// still need to compute this, in case Settings.saveLastView changes, and in case getter is called by system.
				this._lastSelectedTab = (string)tabConverter.ConvertBack( value, typeof(int), this.MainTabView, null );
	
				if( Properties.Settings.Default.saveLastView ) {
					Properties.Settings.Default.lastView = this._lastSelectedTab;
					Properties.Settings.Default.Save();
				}

			}
		}

		public MainWindow() {

			if( Properties.Settings.Default.saveLastView ) {
				this._lastSelectedTab = Properties.Settings.Default.lastView;
			} else {
				this._lastSelectedTab = string.Empty;
			}

			this.InitializeComponent();

			this.RestoreLastView();

		}

		private void RestoreLastView() {

			if( Properties.Settings.Default.saveLastView ) {

				foreach( var item in MainTabView.Items ) {

					TabItem tabItem = item as TabItem;
					if( tabItem != null && tabItem.Name == this._lastSelectedTab ) {
						tabItem.IsSelected = true;
						break;
					}

				}

			}

			string winSize = Properties.Settings.Default.lastViewSize;
			if( !string.IsNullOrEmpty( winSize ) ) {

				string[] coords = winSize.Split( ',' );

				if( coords.Length > 0 ) {

					int width;
					if( int.TryParse( coords[0], out width ) ) {
						this.Width = width;
					}
					int height;
					if( coords.Length > 1 && int.TryParse( coords[1], out height ) ) {
						this.Height = height;
					}

				}


			} //

		}

		private void BtnSettings_Click( object sender, RoutedEventArgs e ) {
			App.Instance.ShowSettings();
		}

		private void MenuSettings_Click( object sender, RoutedEventArgs e ) {
			App.Instance.ShowSettings();
		} //

		private void MenuExit_Click( object sender, RoutedEventArgs e ) {
			System.Windows.Application.Current.Shutdown();
		}

		private void mainWindow_SizeChanged( object sender, SizeChangedEventArgs e ) {

			string winSize = this.Width.ToString() + ',' + this.Height.ToString();
			Properties.Settings.Default.lastViewSize = winSize;
			Properties.Settings.Default.Save();

		}


	} // class

} // namespace