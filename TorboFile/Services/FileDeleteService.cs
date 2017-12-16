using Lemur.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Services {

	public class FileDeleteService {

		/// <summary>
		/// Attempts to delete an enumeration of files, returning an operation which can be used
		/// to monitor and cancel the operation in progress.
		/// </summary>
		/// <param name="file_paths"></param>
		/// <param name="moveToTrash"></param>
		/// <returns></returns>
		public async Task<ProgressOperation> DeleteFilesAsync( IEnumerable<string> file_paths, bool moveToTrash=false, float maxWait=-1) {

			//Console.WriteLine( "deleting files: " + checked_files.Count() );

			FileDeleteOp op = new FileDeleteOp(
				file_paths,
				moveToTrash
			);

			await App.RunOperationAsync( op );

			return op;

			//Console.WriteLine( "DELETE COMPLETE" );

		} //

		/// <summary>
		/// Attempts to delete a file asynchronously.
		/// All exceptions are caught.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="moveToTrash"></param>
		public async void DeleteFileAsync( string path, bool moveToTrash=false ) {

			try {
				await Task.Run( () => {

					if( moveToTrash ) {
						RecycleBinDeleter.Delete( path );
					} else if( File.Exists( path ) ) {
						File.Delete( path );
					} else if( Directory.Exists( path ) ) {
						Directory.Delete( path );
					}

				}
				);

			} catch( Exception ) {
			}

		}

	} // class

} // namespace
