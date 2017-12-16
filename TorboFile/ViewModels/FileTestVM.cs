using LerpingLemur.Operations.FileMatching;
using LerpingLemur.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorboFile.ViewModels {

	/// <summary>
	/// Model for a FileMatch Test.
	/// </summary>
	public class FileTestVM : ViewModelBase {

		/// <summary>
		/// Conditions available for creation.
		/// </summary>
		private static Type[] conditionTypes;
		public static Type[] ConditionTypes {
			get {
				return FileTestVM.conditionTypes;
			}
			set {
				FileTestVM.conditionTypes = value;
			}
		}

		public Type TestType {
			get {
				if( condition == null ) {
					return null;
				}
				return condition.GetType();
			}
		}

		private BaseCondition condition;
		public BaseCondition Condition {
			get { return this.condition; }
			set {
				if( value != this.condition ) {
					this.condition = value;
					this.NotifyPropertyChanged();
				}
			}
		}

	} // class

} // namespace
