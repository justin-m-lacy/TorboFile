using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace TorboFile.ViewModels {

	public class FileDetailsModel : FilePreviewModel {


		private readonly ObservableCollection<string> _tags = new ObservableCollection<string>();
		public ObservableCollection<string> Tags {
			get { return this._tags; }
		}

		public FileDetailsModel() {

			this.PropertyChanged += FileDetailsModel_PropertyChanged;
		}

		private void FileDetailsModel_PropertyChanged( object sender, PropertyChangedEventArgs e ) {

			if( e.PropertyName == FilePathPropertyName ) {

				this.ReadFileDetails( this.FilePath );
			}

		} //

		private void ReadFileDetails( string path ) {

			using( ShellObject shellObj = ShellObject.FromParsingName( path ) ) {

				using( ShellProperties props = shellObj.Properties ) {
					this.WriteProperties( props );


					IShellProperty shellProp = props.GetProperty( SystemProperties.System.Category );
					ShellPropertyDescription desc = SystemProperties.GetPropertyDescription( SystemProperties.System.Category );

					Console.WriteLine( "Prop value: " + desc.ValueType );

				}
			}

		}

		private void WriteProperties( ShellProperties properties ) {
		} //

	} // class

} // namespace
