using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.Properties {

	/// <summary>
	/// Determines how an image is displayed within its space.
	/// </summary>
	public enum ViewMode {
		ActualSize = 0,
		FitWidth = 1,
		FitHeight = 2
	}

	/// <summary>
	/// NOTE: AppSettingsBase already implements INotifyPropertyChanged.
	/// </summary>
	internal sealed partial class PreviewSettings {

		public PreviewSettings() {

		}

	} // class

} // namespace