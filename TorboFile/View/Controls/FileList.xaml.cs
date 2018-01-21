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

		public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register( "Buttons", typeof( ObservableCollection<Control> ), typeof( FileList ) );

		public ObservableCollection<Control> Buttons {
			get {
				ObservableCollection<Control> controls = (ObservableCollection<Control>)this.GetValue( ButtonsProperty );
				if( controls == null ) {
					controls = new ObservableCollection<Control>();
					this.SetValue( ButtonsProperty, controls );
				}
				return controls;
			}
			set => this.SetValue( ButtonsProperty, value );
		}

		public FileList() {

			this.Buttons = new ObservableCollection<Control>();
			this.Buttons.CollectionChanged += Buttons_CollectionChanged;

            InitializeComponent();

        }

		private void Buttons_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e ) {
			Console.WriteLine( "ADDING BUTTON" );
		}
	} // class

} // namespace