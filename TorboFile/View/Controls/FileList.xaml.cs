using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TorboFile.View.Controls {

    /// <summary>
    /// Interaction logic for FileList.xaml
    /// </summary>
    public partial class FileList : UserControl {

		public static readonly DependencyProperty ButtonsProperty =
			DependencyProperty.Register( "Buttons", typeof( ObservableCollection<FrameworkElement> ), typeof( FileList ) );

		public ObservableCollection<FrameworkElement> Buttons {
			get {
				ObservableCollection<FrameworkElement> controls = (ObservableCollection<FrameworkElement>)this.GetValue( ButtonsProperty );
				if( controls == null ) {
					controls = new ObservableCollection<FrameworkElement>();
					this.SetValue( ButtonsProperty, controls );
				}
				return controls;
			}
			set => this.SetValue( ButtonsProperty, value );
		}

		public FileList() {

			this.Buttons = new ObservableCollection<FrameworkElement>();

			//this.Buttons.CollectionChanged += Buttons_CollectionChanged;

            InitializeComponent();

        }

		/*private void Buttons_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e ) {
			Console.WriteLine( "FileList.xaml.cs: Adding FileList sub-button." );
		}*/

	} // class

} // namespace