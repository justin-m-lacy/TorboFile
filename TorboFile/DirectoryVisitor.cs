using System;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using static LerpingLemur.Debug.DebugUtils;
using System.Windows;
using LerpingLemur.Utils;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LerpingLemur.Collections;
using LerpingLemur.Windows;

namespace TorboFile {

	/// <summary>
	/// Focuses on files within a directory, visiting one file at a time which is considered 'Current.'
	/// 
	/// Class is Disposable because it utilizes a FileSystemWatcher that receives notifications
	/// when files are added or removed from the Directory being focused.
	/// </summary>
	public class DirectoryVisitor : ListVisitor<FileInfo>, IDisposable {

		private bool _isDisposed;
		public bool IsDisposed { get { return this._isDisposed; } }

		/// <summary>
		/// Information about the current directory being visited.
		/// </summary>
		private DirectoryInfo currentDir;
		public string CurrentDirectory {
			get {
				if( this.currentDir == null ) {
					return string.Empty;
				}
				return this.currentDir.FullName;
			}
		}

		/// <summary>
		/// Path of current File in directory being 'visited'
		/// </summary>
		public string CurrentPath {
			get {
				FileInfo cur = this.Current;
				if( cur != null ) {
					return cur.FullName;
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Watches for changes to the observed directory.
		/// </summary>
		private FileSystemWatcher eventWatcher;

		/// <summary>
		/// Marks whether the directory path provided is a valid directory.
		/// </summary>
		private bool _validDirectory;
		/// <summary>
		/// True if the path provided to the DirectoryVisitor was a valid directory path.
		/// </summary>
		public bool ValidDirectory {
			get { return this._validDirectory; }
		}

		public DirectoryVisitor( string path ) : base( new List<FileInfo>() ) {

			this.eventWatcher = new FileSystemWatcher();
			this.eventWatcher.IncludeSubdirectories = false;
			this.eventWatcher.SynchronizingObject = new WpfSynchronizer( Application.Current.Dispatcher );

			this.eventWatcher.Created += EventWatcher_Created;
			this.eventWatcher.Deleted += EventWatcher_Deleted;
			this.eventWatcher.Renamed += EventWatcher_Renamed;
			this.eventWatcher.Changed += EventWatcher_Changed;
			this.eventWatcher.Error += EventWatcher_Error;

			//this.PropertyChanged += DirectoryVisitor_PropertyChanged;
			this.SetDirectory( path );

		} // DirectoryEnumerator()

		/// <summary>
		/// Attempts to set the Current directory to a new directory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>false if there was an error changing the directory.</returns>
		public bool SetDirectory( string path ) {

			if( string.IsNullOrEmpty(path) || !Directory.Exists( path ) ) {
				return false;
			}

			try {

				string current = this.CurrentDirectory;

				if( !string.IsNullOrEmpty( current ) && Path.GetFullPath( path ) == Path.GetFullPath( current ) ) {
					// path has not changed. NOTE: refresh here?
					return true;
				}

				this._validDirectory = true;

				DirectoryInfo info = new DirectoryInfo( path );
				this.currentDir = info;

				this.eventWatcher.Path = path;

				/// turn on events -- note: only do this if path is valid.
				this.eventWatcher.EnableRaisingEvents = true;

				this.NotifyPropertyChanged( "CurrentDirectory" );

			} catch {
				return false;
			}

			this.Refresh();

			return true;

		}

		/// <summary>
		/// FileSystemWatcher Error event occurred.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventWatcher_Error( object sender, ErrorEventArgs e ) {

			Log( "error: " + e.ToString() );

		}

		/// <summary>
		/// FileSystemWatcher changed event occurred.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventWatcher_Changed( object sender, FileSystemEventArgs e ) {

			switch ( e.ChangeType ) {

				case WatcherChangeTypes.Created:
					break;
				case WatcherChangeTypes.Deleted:
					break;
				case WatcherChangeTypes.Renamed:
					break;
				
			}

			Log( "changed: " + e.FullPath );

		}

		/// <summary>
		/// FileSystemWatcher Renamed event occurred.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventWatcher_Renamed( object sender, RenamedEventArgs e ) {

			Log( "renamed: " + e.FullPath );

			IList<FileInfo> myList = this.List;
			int len = myList.Count;
			FileInfo info;

			string oldPath = e.OldName;

			for ( int i = len - 1; i >= 0; i-- ) {

				info = myList[i];
				if ( info.FullName == oldPath ) {

					this[i] = new FileInfo( e.FullPath );
					break;

				}

			} // for-loop.
	
		}

		/// <summary>
		/// FileSystemWatcher Deleted event occurred.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventWatcher_Deleted( object sender, FileSystemEventArgs e ) {

			Log( "deleted: " + e.FullPath );
			this._RemoveFile( e.FullPath );

		}

		/// <summary>
		/// FileSystemWatcher Created event occurred.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EventWatcher_Created( object sender, FileSystemEventArgs e ) {

			FileInfo newInfo = new FileInfo( e.FullPath );

			base.Add( newInfo );

			Log( "created: " + e.FullPath );

		}

		/// <summary>
		/// Refreshes the file list from the current directory.
		/// </summary>
		public void Refresh() {

			base.Clear();

			FileInfo[] files = this.currentDir.GetFiles();

			this.AddItems( files );

			this.CurrentIndex = 0;

		}

		/// <summary>
		/// Removes the current file from the list but does not delete it.
		/// If the File is not otherwise deleted, and the list is refreshed,
		/// the file will reappear in the list.
		/// </summary>
		public void RemoveCurrent() {
			this.RemoveAt( this.CurrentIndex );
		}

		/// <summary>
		/// If the given file is in the Directory list, the file is removed from the list.
		/// The file is not deleted; if the DirectoryVisitor is refreshed without the file
		/// being deleted by another source, it will reappear in the list.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True on success, false on failure.</returns>
		public override bool Remove( FileInfo item ) {

			return this._RemoveFile( item.FullName );

		} //

		/// <summary>
		/// Removes the file from the list. The File is not Automatically deleted.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private bool _RemoveFile( string filePath ) {

			IList<FileInfo> myList = this.List;
			string fullPath = Path.GetFullPath( filePath );

			for ( int i = myList.Count - 1; i >= 0; i-- ) {

				FileInfo info = myList[i];
				if ( info.FullName == fullPath ) {
					
					this.RemoveAt( i );
					return true;
				}

			} // for-loop.

			return false;

		} //

		public void Dispose() {

			this.Dispose( true );
			GC.SuppressFinalize( this );

		} //

		private void Dispose( bool not_finalizer ) {

			if( this._isDisposed ) {
				return;
			}
			this._isDisposed = true;

			if( this.eventWatcher != null ) {
				this.eventWatcher.Created -= this.EventWatcher_Created;
				this.eventWatcher.Deleted -= this.EventWatcher_Deleted;
				this.eventWatcher.Renamed -= this.EventWatcher_Renamed;
				this.eventWatcher.Changed -= this.EventWatcher_Changed;
				this.eventWatcher.Error -= this.EventWatcher_Error;

				this.eventWatcher.Dispose();
				this.eventWatcher = null;
			}

		} //

		~DirectoryVisitor() {
			this.Dispose( false );
		}

		/// <summary>
		/// Add() is unsupported because the files are from a system directory.
		/// Files created within the current Directory will be added to the
		/// list automatically.
		/// </summary>
		/// <param name="item"></param>
		override public void Add( FileInfo item ) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Clears the files listed by the Visitor. If the files are
		/// not actually deleted, they will reappear if the directory
		/// is refreshed.
		/// </summary>
		override public void Clear() {
			base.Clear();
		}

		override public bool Contains( FileInfo item ) {

			foreach ( FileInfo info in this.List ) {
				if ( info.FullName == item.FullName ) {
					return true;
				}
			}
			return false;

		}

	} // class

} // namespace