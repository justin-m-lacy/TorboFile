using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels.Main {

	/// <summary>
	/// TODO: Apply ViewModel to MainMenu Window.
	/// </summary>
	public class MainVM : ViewModelBase {

		#region COMMANDS

		private RelayCommand<string> _cmdTabSelected;
		public RelayCommand<string> CmdTabSelected {
			get {
				return this._cmdTabSelected ?? ( this._cmdTabSelected = new RelayCommand<string>( this.SelectTab ) );
			}
		}

		#endregion

		#region PROPERTIES
		

		#endregion

		public MainVM() { }

		private void SelectTab( string tabName ) {



		} //


    } // class

} // namespace
