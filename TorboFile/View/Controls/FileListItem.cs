using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile {

    public class FileListItem {

		public FileData File {
			get;
			set;
		}

		public bool IsSelected {
			get;
			set;
		}

		public FileListItem() {
		}

		public FileListItem( FileData file, bool selected = false ) {

			this.File = file;
			this.IsSelected = selected;

		}

    } // class

} // namespace
