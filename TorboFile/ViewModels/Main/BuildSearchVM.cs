using Lemur.Operations.FileMatching;
using Lemur.Operations.FileMatching.Actions;
using Lemur.Operations.FileMatching.Models;
using Lemur.Utils;
using Lemur.Windows;
using Lemur.Windows.MVVM;
using Lemur.Windows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorboFile.Model;

namespace TorboFile.ViewModels {

	/// <summary>
	/// ViewModel for creating a custom search with arbitrary file test conditions
	/// and an arbitrary list of actions to apply to results.
	/// 
	/// This ViewModel does not actually represent running the search.
	/// </summary>
	public class BuildSearchVM : ViewModelBase {

		#region COMMANDs

		/// <summary>
		/// Command to load a Search Operation saved to disk.
		/// </summary>
		private RelayCommand _cmdLoadOperation;
		/// <summary>
		/// Command to load an existing search from disk.
		/// </summary>
		public RelayCommand CmdLoadOperation {
			get {
				return this._cmdLoadOperation ?? ( this._cmdLoadOperation = new RelayCommand(
					this.LoadSearch ) );
			}
		}

		/// <summary>
		/// Command to save the operation as a permanent object.
		/// </summary>
		private RelayCommand _cmdSaveOperation;
		/// <summary>
		/// TODO: Doesn't really belong in this model?
		/// </summary>
		public RelayCommand CmdSaveOperation {
			get {
				return this._cmdSaveOperation ?? ( this._cmdSaveOperation = new RelayCommand(
					this.SaveCurrent ) );
			}
		}

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Model for building a list of file test conditions.
		/// </summary>
		private readonly MatchBuilder matchBuilder;
		public MatchBuilder MatchBuilder {
			get => matchBuilder;
		}

		/// <summary>
		/// Model for building a list of actions.
		/// </summary>
		public ActionBuilder ActionBuilder {
			get {
				return this.actionBuilder;
			}
		}
		private readonly ActionBuilder actionBuilder;

		public CustomSearchData CustomSearch {
			get => _customSearch;
			set {

				if( this.SetProperty( ref this._customSearch, value ) ) {
					this.actionBuilder.SetItems( this._customSearch.Actions );
					this.matchBuilder.SetItems( this._customSearch.Conditions );
				}
			}

		}
		private CustomSearchData _customSearch;


		#endregion


		private FileMatchOperation BuildMatchOperation() {

			FileMatchOperation matchOp = new FileMatchOperation( this._customSearch.Conditions );
			return matchOp;
		}

		private void SaveCurrent() {

			this.CloneSearch();

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string saveFile = dialog.PickSaveFile( "Save Search..." );
				if( !string.IsNullOrEmpty( saveFile ) ) {

					/// TODO: Make async?
					FileUtils.WriteBinary( saveFile, this._customSearch );

				}

			}

		} //

		public void LoadSearch() {

			IFileDialogService dialog = this.GetService<IFileDialogService>();
			if( dialog != null ) {

				string loadFile = dialog.PickLoadFile( "Load Search..." );
				if( !string.IsNullOrEmpty( loadFile ) ) {

					/// TODO: Make async?
					CustomSearchData loadedSearch = FileUtils.ReadBinary<CustomSearchData>( loadFile );
					if( loadedSearch != null ) {
						this.CustomSearch = loadedSearch;
					}

				}

			}

		} // LoadSearch()

		/// <summary>
		/// Checks if any conditions have been added to the current match builder.
		/// </summary>
		/// <returns></returns>
		public bool HasConditions() {
			return this.matchBuilder.HasItems();
		}

		/// <summary>
		/// Checks if any actions have been added to the current search.
		/// </summary>
		/// <returns></returns>
		public bool HasActions() {
			return this.actionBuilder.HasItems();
		}

		/// <summary>
		/// Checks if any test conditions or actions have been added to the search.
		/// </summary>
		/// <returns></returns>
		public bool HasItems() {
			return this.HasConditions() || this.HasActions();
		}

		/// <summary>
		/// Build the current search from the currently displayed items.
		/// </summary>
		private void CloneSearch() {

			if( this._customSearch == null ) {
				this._customSearch = new CustomSearchData();
			} else {
				this._customSearch.Clear();
			}

			this._customSearch.Conditions = this.matchBuilder.CloneCollection();
			this._customSearch.Actions = this.actionBuilder.CloneCollection();

		}

		public BuildSearchVM( IServiceProvider provider ) : base( provider ) {

			this.matchBuilder = new MatchBuilder();
			this.matchBuilder.ServiceProvider = provider;

			this.actionBuilder = new ActionBuilder();
			this.actionBuilder.ServiceProvider = provider;

			this.InitViewModelCreators();

			this.matchBuilder.Picker.ExcludeTypes = new Type[] { typeof( ContainedIn ), typeof( ConditionList ), typeof( ConditionEnumeration ) };

		}

		/// <summary>
		/// Initializes ViewModel creation functions to use for various data types.
		/// </summary>
		private void InitViewModelCreators() {

			ViewModelBuilder builder = this.GetService<ViewModelBuilder>();
			if( builder != null ) {

				//Console.WriteLine( "SETTING DEFAULT VM BUILDERS" );
				builder.SetCreator<BaseCondition>( this.CreateVM<FileTestVM> );
				builder.SetCreator<MoveFileAction>( this.CreateVM<MoveFileVM> );
				builder.SetCreator<FileActionBase>( this.CreateVM<DataObjectVM> );

			}

		}

		private VM CreateVM<VM>( object data, object view = null ) where VM : DataObjectVM, new() {

			Console.WriteLine( "Creating ViewModel: " + typeof( VM ).Name );
			VM vm = new VM();
			vm.ServiceProvider = this.ServiceProvider;
			vm.Data = data;
			return vm;

		} //

} // class

} // namespace
