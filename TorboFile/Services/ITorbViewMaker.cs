using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorboFile.ViewModels;

namespace TorboFile.Services {

	/// <summary>
	/// Actually referenced, though probably not used.
	/// </summary>
	public interface ITorbViewMaker {

		void InitFindCopiesView( FindCopiesVM model );
		void InitSortView( FileSortVM model );
		void InitCustomSearchView( CustomSearchVM model );
		void InitCleanFoldersView( CleanFoldersModel model );

    } // class

} // namespace
