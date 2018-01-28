using Lemur.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TorboFile.Model;
using Lemur.Operations.FileMatching.Actions;

namespace TorboFile.Operations {

	/// <summary>
	/// Performs a sequence of actions on the given list of file/directory targets.
	/// </summary>
	[Serializable]
	public class FileActionOperation : ProgressOperation {

		public event Action<IFileAction, FileSystemInfo> ActionComplete;
		public event Action<Exception> OnError;

		#region PROPERTIES

		/// <summary>
		/// Conditions that files or directories must match in order
		/// to be included in a match operation.
		/// </summary>
		public ICollection<IFileAction> Actions {
			get { return this._actions; }
			set { this._actions = value; }
		}
		private ICollection<IFileAction> _actions;

		/// <summary>
		/// List of Exceptions encountered during the search.
		/// </summary>
		private readonly List<Exception> errorList = new List<Exception>();
		public Exception[] ErrorList {
			get { return this.errorList.ToArray(); }
		}

		/// <summary>
		/// Files or Directories which are the targets of the Operation.
		/// </summary>
		public IList<FileSystemInfo> Targets {
			get { return this.targets; }
			set { this.targets = value; }
		}

		public CustomSearchOptions Options {
			get => _opts; set {
	
				this._opts = value;
				// cache flags.
				this._flags = value.Flags;

			}
		}
		public bool SkipItemOnError {
			get => this._flags.HasFlag( CustomSearchFlags.SkipItemOnError );
		}

		private IList<FileSystemInfo> targets = new List<FileSystemInfo>();
		
		CustomSearchFlags _flags;
		CustomSearchOptions _opts;

		#endregion

		public override void Run() {

			this.errorList.Clear();
			this.ResetProgress();

			if( this._actions == null ) {

				Console.WriteLine( "FileActionOperation: NO ACTIONS FOUND" );
			} else if( this.targets == null ) {
				Console.WriteLine( "FileActionOperation: NO TARGETS FOUND" );

			} else {

				Console.WriteLine( "FileActionOperation: Run()" );
				this.RunActions();

			}

			this.OperationComplete();

		}

		private void RunActions() {
			
			IList<FileSystemInfo> files = this.targets;

			/// used to advance progress of skipped items.
			int actionsRun = 0;

			int fileCount = this.targets.Count;

			int runOnceActions;
			int fileActions;

			this.CountActionTypes( this._actions, out fileActions, out runOnceActions );

			// takes RunOnce actions into account.
			this.SetMaxProgress( runOnceActions + fileActions*fileCount );

			foreach( IFileAction action in this._actions ) {

				if( this.CancelRequested() ) {
					return;
				}

				if( action.RunOnce ) {

					this.RunOnce( action );

				} else {

					for( int i = 0; i < fileCount; i++ ) {

						FileSystemInfo info = files[i];

						try {

							FileActionResult result = action.Run( info );
							if( result.success && result.fileReplace != null ) {
								files[i] = result.fileReplace;
							}

						} catch( Exception e ) {

							this.AddError( e );
							if( CancelRequested() ) {
								return;
							} else if( SkipItemOnError ) {
								// Advances progress over the number of actions remaining for this item.
								this.SkipItem( actionsRun, fileActions );
								break;
							}

						}
						this.AdvanceProgress();
						if( this.CancelRequested() ) {
							return;
						}

					} //

				}

				actionsRun++;

			} // action loop.

		}

		private void RunOnce( IFileAction action ) {

			try {

				action.Run( new FileInfo( "/" ) );

			} catch( Exception e ) {
				this.AddError( e );
			}
			this.AdvanceProgress();

		}

		/// <summary>
		/// Counts the number of actions that will actually be applied to every file
		/// (as opposed to RunOnce actions)
		/// </summary>
		/// <param name="actions"></param>
		/// <returns></returns>
		private void CountActionTypes( ICollection<IFileAction> actions, out int fileActions, out int runOnceActions ) {

			fileActions = 0;
			runOnceActions = 0;

			foreach( IFileAction action in actions ) {
				if( action.RunOnce ) {
					runOnceActions++;
				} else {
					fileActions++;
				}

			}

		}

		/// <summary>
		/// Unforunate function.
		/// </summary>
		/// <param name="files"></param>
		/// <returns></returns>
		private long CountMaxProgress( int fileCount ) {

			long max = 0;
			foreach( IFileAction action in this._actions ) {

				if( action.RunOnce ) {
					max++;
				} else {
					max += fileCount;
				}

			}

			return max;

		}


		/// <summary>
		/// If an item needs to be skipped because of an error, all progress relating to the item
		/// has to be advanced.
		/// </summary>
		/// <param name="actionsDone"></param>
		private void SkipItem( int actionsDone, int totalActions ) {
			this.AdvanceProgress( totalActions - actionsDone );
		}

		private void AddError( Exception e ) {

			Dispatch( () => {

				this.errorList.Add( e );
				this.OnError?.Invoke( e );

			} );

			if( this._flags.HasFlag( CustomSearchFlags.HaltOnError ) ) {
				this.Cancel();
			}

		}

	} // class

} // namespace