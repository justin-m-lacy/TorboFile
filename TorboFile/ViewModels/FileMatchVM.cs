using LerpingLemur.Operations.FileMatching;
using LerpingLemur.Windows;
using LerpingLemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	/// <summary>
	/// A Model for creating a FileMatch operation.
	/// </summary>
	public class FileMatchVM : ViewModelBase {

		#region COMMANDS

		private RelayCommand _cmdAddCondition;

		/// <summary>
		/// Command to create a new Match Condition.
		/// </summary>
		public RelayCommand CmdAddCondition {

			get {
				return this._cmdAddCondition ??
					( this._cmdAddCondition = new RelayCommand( this.CreateNewTest ) );
			}
	
		}

		private RelayCommand<BaseCondition> _cmdRemoveCondition;
		/// <summary>
		/// Remove the Matching Condition specified.
		/// </summary>
		public RelayCommand<BaseCondition> CmdRemoveCondition {

			get {
				return this._cmdRemoveCondition ??
					( this._cmdRemoveCondition = new RelayCommand<BaseCondition>(
						( c ) => { this._matchConditions.Remove( c ); } )
						);
			}

		}

		/// <summary>
		/// Command to save the operation as a permanent object.
		/// </summary>
		private RelayCommand _cmdSaveOperation;
		/// <summary>
		/// TODO: Doesn't really belong in this model?
		/// </summary>
		public RelayCommand CmdSaveOperation {
			get {
				return this._cmdSaveOperation ?? ( this._cmdSaveOperation = new RelayCommand(
					() => {
						this.OnRequestSave?.Invoke( this );
					} ) );
			}
		}

		#endregion

		#region PROPERTIES

		private FileMatchSettings _settings;
		public FileMatchSettings Settings {
			get {
				return this._settings;
			}
			set {
				if( this._settings != value ) {
					this._settings = value;
					this.NotifyPropertyChanged();
				}
			}
		}
		
		/// <summary>
		/// Dispatched when the user selects to save the Match Condition list.
		/// </summary>
		public event Action<FileMatchVM> OnRequestSave;

		private ObservableCollection<BaseCondition> _matchConditions;
		public ObservableCollection<BaseCondition> MatchConditions {
			get { return this._matchConditions; }
			set {
				if( value != this._matchConditions ) {
					this._matchConditions = new ObservableCollection<BaseCondition>();
				}
			}

		}

		private FileMatchOperation operation;

		/// <summary>
		/// The operation being displayed.
		/// </summary>
		public FileMatchOperation Operation {
			get { return this.operation; }
			set {
				if( operation != value ) {
					this.operation = value;
					this.NotifyPropertyChanged();
				}
			}

		}

		#endregion

		/// <summary>
		/// Removes a condition from the File Match operation.
		/// </summary>
		/// <param name="cond"></param>
		private void RemoveCondition( BaseCondition cond ) {

			this._matchConditions.Remove( cond );

		}

		/// <summary>
		/// Creates a new File Matching condition.
		/// </summary>
		private void CreateNewTest() {

			//BaseCondition condition = new BaseCondition();

		}

	} // class

} // namespace
