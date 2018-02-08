using Lemur.Tasks;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Links an Operation in progress for display in UI.
	/// On Completion or Cancel, the ProgressVM will cancel, but not dispose the operation.
	/// </summary>
	public class ProgressVM : ViewModelBase {

		~ProgressVM() {
			Console.WriteLine( "ProgressVM Destructor called" );
		}

		#region PROPERTIES

		public bool IsRunning {
			get { return ( this._operation != null ) ? this._operation.IsRunning : false; }
		}

		/// <summary>
		/// The Operation being watched.
		/// </summary>
		public ProgressOperation Operation {
			get { return this._operation; }

			set {

				if( this._operation != null ) {
					this._operation.ProgressChanged -= Operation_ProgressChanged;
				}

				this._operation = value;

				if( value != null ) {
					value.ProgressChanged += Operation_ProgressChanged;
	
					ProgressInformation info = value.ProgressInformation;
					this.Message = info.Message;
					this.CurProgress = info.CurProgress;
					this.MaxProgress = info.MaxProgress;
				}
				this.NotifyPropertyChanged();
				this.NotifyPropertyChanged( "IsRunning" );
				this.CmdCancel.RaiseCanExecuteChanged();

			}

		}
		private ProgressOperation _operation;

		/// <summary>
		/// Current message or item being processed by the operation.
		/// </summary>
		public string Message {
			get { return this._lastMessage; }
			set {
				if( this._lastMessage != value ) {
					this._lastMessage = value;
					this.NotifyPropertyChanged();
				}
			}

		}
		private string _lastMessage;

		/// <summary>
		/// Maximum progress number.
		/// </summary>
		public long MaxProgress {
			get { return this._lastMax; }
			set {
				if( this._lastMax != value ) {
					this._lastMax = value;
					this.NotifyPropertyChanged();
				}
			}
		}
		private long _lastMax;

		
		/// <summary>
		/// Current progress amount.
		/// </summary>
		public long CurProgress {
			get { return this._lastProgress; }
			set {
				if( this._lastProgress != value ) {
					this._lastProgress = value;
					this.NotifyPropertyChanged();
				}
			}
		}
		private long _lastProgress;

		#endregion

		#region COMMANDS

		/// <summary>
		/// Command to cancel the operation in progress.
		/// </summary>
		public RelayCommand CmdCancel {
			get {
				return this._cmdCancel ?? ( this._cmdCancel = new RelayCommand(

					this.Cancel, this.CanCancel

				) );
			}
		}
		private RelayCommand _cmdCancel;

		#endregion


		public ProgressVM() {
		}

		public ProgressVM( ProgressOperation operation ) {

			this.Operation = operation;

		} //

		private void Cancel() {

			if( this._operation != null ) {

				ProgressOperation op = this._operation;
				this.Operation = null;

				op.Cancel();

			}
		}

		private bool CanCancel() {
			return this._operation != null && this._operation.IsRunning;
		}


		private void Operation_Complete() {
			this.Operation = null;
		}

		private void Operation_ProgressChanged( object sender, ProgressInformation e ) {

			this.Message = e.Message;
			this.CurProgress = e.CurProgress;
			this.MaxProgress = e.MaxProgress;

			if( e.IsComplete ) {
				Console.WriteLine( "ProgressVM: Operation complete." );
				this.Operation_Complete();
			}

		} //

	} // class

} // namespace
