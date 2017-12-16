using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TorboFile.Categories;
using System.IO;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack.Dialogs;
using TorboFile.Services;
using Lemur.Windows.MVVM;
using Lemur.Windows.Input;

namespace TorboFile.Windows {

    /// <summary>
    /// Interaction logic for NewCategoryWindow.xaml
    /// </summary>
    public partial class EditCategoryWindow : Window {

        public EditCategoryWindow() {

            this.InitializeComponent();

			//this.InitFields();
			//this.InitAnimations();

        }

		private void SetEditMode() {

			this.BtnAccept.Content = "Edit";

		}

		private void InitFields() {
		} //

		private void InitAnimations() {
		} //

		private void BtnCancel_Click( object sender, RoutedEventArgs e ) {

			this.Close();

		}

		/// <summary>
		/// Select the directory where category items will be moved.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnDirectory_Click( object sender, RoutedEventArgs e ) {

			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.IsFolderPicker = true;
			CommonFileDialogResult result = dialog.ShowDialog();

			if( result == CommonFileDialogResult.Ok ) {
				this.FldDirectory.Text = dialog.FileName;
			}

		} //

	} // class

} // namespace