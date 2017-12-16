﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TorboFile {

	// SyncTrayzor
	// via: http://stackoverflow.com/a/3282481/1086121
	public static class RecycleBinDeleter {

		/// <summary>
		/// Possible flags for the SHFileOperation method.
		/// </summary>
		[Flags]
		private enum FileOperationFlags : ushort {
			/// <summary>
			/// Do not show a dialog during the process
			/// </summary>
			FOF_SILENT = 0x0004,
			/// <summary>
			/// Do not ask the user to confirm selection
			/// </summary>
			FOF_NOCONFIRMATION = 0x0010,
			/// <summary>
			/// Delete the file to the recycle bin.  (Required flag to send a file to the bin
			/// </summary>
			FOF_ALLOWUNDO = 0x0040,
			/// <summary>
			/// Do not show the names of the files or folders that are being recycled.
			/// </summary>
			FOF_SIMPLEPROGRESS = 0x0100,
			/// <summary>
			/// Surpress errors, if any occur during the process.
			/// </summary>
			FOF_NOERRORUI = 0x0400,
			/// <summary>
			/// Warn if files are too big to fit in the recycle bin and will need
			/// to be deleted completely.
			/// </summary>
			FOF_AUTODELETEWARNING = 0x4000,
		}

		/// <summary>
		/// File Operation Function Type for SHFileOperation
		/// </summary>
		private enum FileOperationType : uint {
			/// <summary>
			/// Move the objects
			/// </summary>
			FO_MOVE = 0x0001,
			/// <summary>
			/// Copy the objects
			/// </summary>
			FO_COPY = 0x0002,
			/// <summary>
			/// Delete (or recycle) the objects
			/// </summary>
			FO_DELETE = 0x0003,
			/// <summary>
			/// Rename the object(s)
			/// </summary>
			FO_RENAME = 0x0004,
		}



		/// <summary>
		/// SHFILEOPSTRUCT for SHFileOperation from COM
		/// </summary>
		[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
		private struct SHFILEOPSTRUCT {

			public IntPtr hwnd;
			[MarshalAs( UnmanagedType.U4 )]
			public FileOperationType wFunc;
			public string pFrom;
			public string pTo;
			public FileOperationFlags fFlags;
			[MarshalAs( UnmanagedType.Bool )]
			public bool fAnyOperationsAborted;
			public IntPtr hNameMappings;
			public string lpszProgressTitle;

		}

		/// <summary>
		/// Send file to recycle bin.
		/// </summary>
		/// <param name="path">Location of directory or file to recycle</param>
		public static bool Delete( string path ) {

			try {
				var fs = new SHFILEOPSTRUCT {
					wFunc = FileOperationType.FO_DELETE,
					pFrom = path + '\0' + '\0',
					fFlags = FileOperationFlags.FOF_ALLOWUNDO |
							 FileOperationFlags.FOF_SILENT |
							 FileOperationFlags.FOF_NOCONFIRMATION |
							 FileOperationFlags.FOF_AUTODELETEWARNING,
				};
				int result = NativeMethods.SHFileOperation( ref fs );
				if ( result != 0 ) {
					//Logger( String.Format( "Delete file operation on {0} failed with error {1}", path, result ) );
					return false;
				}
				return true;

			} catch ( Exception ) {
				return false;
			}

		}

		private static class NativeMethods {

			[DllImport( "shell32.dll", CharSet = CharSet.Auto )]
			public static extern int SHFileOperation( ref SHFILEOPSTRUCT FileOp );

		}

	}

} // namespace