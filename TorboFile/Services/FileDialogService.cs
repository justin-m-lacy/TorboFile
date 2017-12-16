using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Services {

	public class FileDialogService {

		/// <summary>
		/// Returns the folder selected, or null, if none is selected.
		/// </summary>
		/// <returns></returns>
		public string PickFolder( string dialogTitle, string startPath=null ) {

			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;

			if( !string.IsNullOrEmpty( startPath ) ) {
				dialog.DefaultDirectory = startPath;
			}

			dialog.Title = dialogTitle;
			dialog.EnsureFileExists = true;
			CommonFileDialogResult result = dialog.ShowDialog();

			if( result == CommonFileDialogResult.Ok ) {

				return dialog.FileName;

			}

			return null;

		}

	} // class

} // namespace
