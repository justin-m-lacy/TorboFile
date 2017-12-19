using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Lemur.Debug.DebugUtils;

namespace TorboFile.View.Controls {

	/// <summary>
	/// Interaction logic for FileSelectionList.xaml
	/// </summary>
	public partial class FileSelectionView : UserControl {

		GridViewColumnHeader _lastHeaderClicked = null;
		ListSortDirection _lastDirection = ListSortDirection.Ascending;
	
		public FileSelectionView() {
			this.InitializeComponent();
		} //

		/// <summary>
		/// List Header was clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListResults_Click( object sender, RoutedEventArgs e ) {

			GridViewColumnHeader clickedHeader = e.OriginalSource as GridViewColumnHeader;
			if( clickedHeader == null || clickedHeader.Role == GridViewColumnHeaderRole.Padding ) {
				return;
			}
			ListSortDirection direction;

			if( clickedHeader == _lastHeaderClicked ) {
				// header repeat click.
				direction = ( _lastDirection == ListSortDirection.Ascending ) ? ListSortDirection.Descending : ListSortDirection.Ascending;
			} else {
				// new header clicked.
				direction = ListSortDirection.Ascending;
			}

			Binding binding = ( clickedHeader.Column.DisplayMemberBinding as Binding );

			if( binding == null ) {
				return;
			}

			this.Sort( binding.Path, direction );

			// header arrow: TODO: Header Arrow does not exist???
			if( direction == ListSortDirection.Ascending ) {
				clickedHeader.Column.HeaderTemplate =
				  Resources["HeaderTemplateArrowUp"] as DataTemplate;
			} else {
				clickedHeader.Column.HeaderTemplate =
					  Resources["HeaderTemplateArrowDown"] as DataTemplate;
			}

			this._lastDirection = direction;
			this._lastHeaderClicked = clickedHeader;

		} //

		private void Sort( PropertyPath sortPath, ListSortDirection direction ) {

			ICollectionView dataView = CollectionViewSource.GetDefaultView( this.ListResults.ItemsSource );
			dataView.SortDescriptions.Clear();

			SortDescription sort = new SortDescription( sortPath.Path, direction );

			dataView.SortDescriptions.Add( sort );
			dataView.Refresh();

		} //

	} // class

} // namespace
