using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Model {

	[Flags]
	[Serializable]
	public enum CustomSearchFlags {

		None,

		/// <summary>
		/// Apply the search recursively.
		/// </summary>
		Recursive,

		/// <summary>
		/// Do not wait for confirmation or file selection before running
		/// the actions associated with a search. Run actions immediately.
		/// </summary>
		AutoRun,

		/// <summary>
		/// Any errors in the search should stop all actions.
		/// </summary>
		HaltOnError,

		/// <summary>
		/// If actions on a single item cause an error, stop processing
		/// further actions for that item.
		/// </summary>
		SkipItemOnError
	
	}

	[Serializable]
	public class CustomSearchOptions {

		/// <summary>
		/// Starting directory can be null or empty.
		/// </summary>
		private string baseLocation;
		public string BaseDirectory {
			get => this.baseLocation;
			set => this.baseLocation = value;
		}

		private CustomSearchFlags _flags;
		public CustomSearchFlags Flags {
			get => this._flags;
			set => this._flags = value;
		}

		public void SetFlag( CustomSearchFlags flags ) {
			this._flags |= flags;
		}

		public CustomSearchOptions() {
		}
		public CustomSearchOptions( CustomSearchFlags flags ) {
			this._flags = flags;
		}

	} // class

} // namespace
