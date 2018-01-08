using Lemur.Windows.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Services {

	public class FileDialogService : IFileDialogService {

		/// <summary>
		/// The file picked must exist.
		/// </summary>
		/// <param name="dialogTitle"></param>
		/// <param name="defaultPath"></param>
		/// <param name="defaultFileName"></param>
		/// <returns></returns>
		public string PickLoadFile( string dialogTitle, string defaultPath = null, string defaultFileName = null ) {

			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			if( !String.IsNullOrEmpty( defaultPath ) ) {
				dialog.DefaultDirectory = defaultPath;
			}
			if( !string.IsNullOrEmpty( defaultFileName ) ) {
				dialog.DefaultFileName = defaultFileName;
			}

			dialog.Title = dialogTitle;
			dialog.EnsureFileExists = true;

			CommonFileDialogResult res = dialog.ShowDialog();
			if( res == CommonFileDialogResult.Ok ) {
				return dialog.FileName;
			}
			return null;

		}

		/// <summary>
		/// The file does not need to exist.
		/// </summary>
		/// <param name="dialogTitle"></param>
		/// <param name="defaultPath"></param>
		/// <param name="defaultFileName"></param>
		/// <returns></returns>
		public string PickSaveFile( string dialogTitle, string defaultPath=null, string defaultFileName=null ) {

			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			if( !String.IsNullOrEmpty( defaultPath ) ) {
				dialog.DefaultDirectory = defaultPath;
			}
			if( !string.IsNullOrEmpty( defaultFileName ) ) {
				dialog.DefaultFileName = defaultFileName;
			}

			dialog.Title = dialogTitle;
			dialog.EnsureFileExists = false;

			CommonFileDialogResult res = dialog.ShowDialog();
			if( res == CommonFileDialogResult.Ok ) {
				return dialog.FileName;
			}
			return null;

		}

		/// <summary>
		/// Returns the folder selected, or null, if none is selected.
		/// </summary>
		/// <returns></returns>
		public string PickFolder( string dialogTitle, string defaultPath=null ) {

			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;

			if( !string.IsNullOrEmpty( defaultPath ) ) {
				dialog.DefaultDirectory = defaultPath;
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
