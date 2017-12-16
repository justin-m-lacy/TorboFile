using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

namespace TorboFile {

	public class FileDataList : ObservableCollection<FileData> { }
	public class SelectableFileList : ObservableCollection<FileListItem> { }

}