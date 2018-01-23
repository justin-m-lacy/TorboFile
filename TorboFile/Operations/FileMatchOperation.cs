using Lemur.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TorboFile.Model;
using Lemur.Operations.FileMatching;

namespace TorboFile.Operations {

	/// <summary>
	/// Operation that matches files or directories by a list of matching criteria,
	/// and reports the match results.
	/// </summary>
	public class FileMatchOperation : ProgressOperation {

		private const string ROOT_DIR = "/";

		public event Action<FileSystemInfo> OnMatchFound;

		#region PROPERTIES

		private IEnumerable<IMatchCondition> _conditions;

		/// <summary>
		/// Conditions that files or directories must match in order
		/// to be included in a match operation.
		/// </summary>
		public IEnumerable<IMatchCondition> Conditions {
			get { return this._conditions; }

		}

		/// <summary>
		/// Condition to determine whether a subdirectory should
		/// be recursively followed. This does not not affect whether
		/// or not the directory is added as a match itself.
		/// </summary>
		private IEnumerable<IMatchCondition> _recursionConditions = new List<IMatchCondition>();

		/// <summary>
		/// Conditions a directory must meet in order to continue the search into
		/// a subdirectory. Directories meeting the recursion criteria will not be included
		/// in the search results unless they also meet the MatchConditions.
		/// </summary>
		public IEnumerable<IMatchCondition> RecursionConditions {
			get { return this._recursionConditions; }
		}

		private string folderPath;
		public string FolderPath => this.folderPath;

		/// <summary>
		/// List of Exceptions encountered during the search.
		/// </summary>
		private readonly List<Exception> errorList = new List<Exception>();
		public Exception[] ErrorList {
			get { return this.errorList.ToArray(); }
		}

		private CustomSearchOptions _opts;
		public CustomSearchOptions Options {
			get => _opts; set {

				this._opts = value;
				// cache flags.
				this._flags = value.Flags;

			}
		}

		/// <summary>
		/// Object to lock before adding items to the results list.
		/// </summary>
		private object resultLock;

		/// <summary>
		/// Settings used for the operation.
		/// </summary>
		private CustomSearchFlags _flags;

		/// <summary>
		/// List of files or directories that matched the search conditions.
		/// The list is cleared at the start of every call to Run()
		/// </summary>
		public List<FileSystemInfo> Matches { get { return this.matches; } }
		private readonly List<FileSystemInfo> matches = new List<FileSystemInfo>();

		#endregion

		public FileMatchOperation( string basePath, IEnumerable<IMatchCondition> conditions = null, CustomSearchOptions opts = null, object resultLock=null ) {

			this.folderPath = basePath;
			this._conditions = conditions;
			this.resultLock = resultLock;
			this.Options = opts;

		}

		public FileMatchOperation( IEnumerable<IMatchCondition> conditions, CustomSearchOptions opts =null ) {

			this._conditions = conditions;
			this.Options = opts;

		}

		public FileMatchOperation() {} //

		/// <summary>
		/// Runs the operation with an object is locked before results are updated.
		/// </summary>
		/// <param name="resultLock"></param>
		public void Run( object resultLock ) {

			this.resultLock = resultLock;
			this.Run();

		}

		public override void Run() {

			this.matches.Clear();
			this.errorList.Clear();

			if( string.IsNullOrEmpty( this.folderPath ) ) {

				Console.WriteLine( "WARNING: NO BASE SEARCH PATH SET. USING ROOT" );
				this.VisitDirectory( ROOT_DIR );

			} else {
				Console.WriteLine( "SEARCHING: " + folderPath );
				this.VisitDirectory( this.folderPath );
			}

			this.OperationComplete();

		}

		/// <summary>
		/// Visit a directory, attempt to search any subdirectories, and add
		/// any file system matches.
		/// </summary>
		/// <param name="dir"></param>
		private void VisitDirectory( string dir ) {

			if( !Directory.Exists( dir ) ) {
				return;
			}

			try {

				if( this._flags.HasFlag( CustomSearchFlags.Recursive ) ) {
					this.VisitSubDirs( dir );
				}

			} catch( Exception e ) {
				this.AddError( e );
			}

			//Console.WriteLine( "FileMatchOperation: Visiting Files In: " + dir );
			this.VisitEntries( dir );

		} // VisitDirectory()

		/// <summary>
		/// Visit the files in a directory and add any matches.
		/// </summary>
		/// <param name="parentDir"></param>
		private void VisitEntries( string parentDir ) {

			//MatchType type = this._settings.Types;

			foreach( string dir in Directory.EnumerateDirectories( parentDir ) ) {

				try {

					DirectoryInfo info = new DirectoryInfo( dir );
					if( this.TestFile( info ) ) {
						this.AddResult( info );
					}

				} catch( Exception e ) {
					this.errorList.Add( e );
				}

			} // foreach.

			foreach( string file in Directory.EnumerateFiles( parentDir ) ) {

				try {

					
					FileInfo info = new FileInfo( file );
					if( this.TestFile( info ) ) {
						this.AddResult( info );
					}

				} catch( Exception e ) {
					this.errorList.Add( e );
				}

			} // foreach.

		}

		private void AddResult( FileSystemInfo result ) {

			this.Dispatch( () => {

				if( resultLock != null ) {

					lock( this.resultLock ) {

						this.matches.Add( result );
						this.OnMatchFound?.Invoke( result );

					}

				} else {
					this.matches.Add( result );
					this.OnMatchFound?.Invoke( result );
				}

			});

		} //

		private void AddError( Exception err ) {

			this.Dispatch( () => {

				this.errorList.Add( err );

			} );

		}

		/// <summary>
		/// Returns true if FileSystemInfo entry is a valid match,
		/// false otherwise.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool TestFile( FileSystemInfo fileInfo ) {

			foreach( IMatchCondition cond in this.Conditions ) {

				if( !cond.IsMatch( fileInfo ) ) {
					return false;
				}

			} // foreach

			return true;

		} //

		/// <summary>
		/// Visit all available subdirectories of the current directory.
		/// </summary>
		/// <param name="parentDir"></param>
		private void VisitSubDirs( string parentDir ) {

			/// Not using FileUtils since the recursive option is not yet applied?
			string[] dirs = Directory.GetDirectories( parentDir );

			if( this._recursionConditions == null ) {

				/// No Conditions for visiting subdirectories:
				foreach( string subdir in dirs ) {
					this.VisitDirectory( subdir );
				}
				return;
			} //

			/// Conditions for visiting subdirectories.
			foreach( string subdir in dirs ) {

				bool follow = true;

				DirectoryInfo info = new DirectoryInfo( subdir );

				foreach( IMatchCondition cond in this._recursionConditions ) {

					if( !cond.IsMatch( info ) ) {
						follow = false;
						break;
					}

				}
				if( follow ) {
					this.VisitDirectory( subdir );
				}

			} //

		} // VisitSubDirs()

	} // class

} // namespace