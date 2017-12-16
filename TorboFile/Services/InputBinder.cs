using Lemur.Windows.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TorboFile.Services {

	/// <summary>
	/// Binds inputs from a ViewModel to its view.
	/// </summary>
	public class InputBinder {

		private Dictionary<ViewModelBase, List<InputBinding>> bindingGroups;

		public InputBinder() {

			this.bindingGroups = new Dictionary<ViewModelBase, List<InputBinding>>();

		}

		/// <summary>
		/// Gets the list of input bindings associated with the given ViewModel,
		/// or creates a new one if none exists.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		private List<InputBinding> GetBindingList( ViewModelBase model ) {

			List<InputBinding> list;
			if( bindingGroups.TryGetValue( model, out list ) ) {
				return list;
			}
			list = new List<InputBinding>();
			this.bindingGroups[model] = list;

			return list;

		}

		/// <summary>
		/// Replace a binding with the given InputGesture with the new input binding.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="old_binding"></param>
		/// <param name="new_binding"></param>
		public void ReplaceBinding( ViewModelBase model, InputGesture gesture, InputBinding new_binding ) {

			FrameworkElement element = model.ViewElement;
			if( element == null ) {
				return;
			}

			InputBindingCollection elementBindings = element.InputBindings;
			List<InputBinding> viewBindingList = this.GetBindingList( model );

			for( int i = elementBindings.Count - 1; i >= 0; i-- ) {

				InputBinding b = elementBindings[i];

				// searching for a matching gesture - this gesture is being replaced.
				if( !b.Gesture.Equals( gesture ) ) {
					continue;
				}

				Console.WriteLine( "OVERWRITING BINDING" );
				// NOTE: b is being removed because 'old_binding' is just logically equivalent,
				// not necessarily the same object.
				// TODO: replace with Predicate remove.
				viewBindingList.Remove( b );
				viewBindingList.Add( new_binding );

				//elementBindings.RemoveAt( i );
				elementBindings[i] = new_binding;
				break;


			} // for-loop.

		}

		public void ReplaceBinding( ViewModelBase model, InputBinding old_binding, InputBinding new_binding ) {

			FrameworkElement element = model.ViewElement;
			if( element == null ) {
				return;
			}

			InputBindingCollection elementBindings = element.InputBindings;
			List<InputBinding> viewBindingList = this.GetBindingList( model );

			for( int i = elementBindings.Count-1; i>= 0;i-- ) {

				InputBinding b = elementBindings[i];

				if( !BindingsEqual( b, old_binding ) ) {
					continue;
				}
	
				Console.WriteLine( "OVERWRITING BINDING" );
				// NOTE: b is being removed because 'old_binding' is just logically equivalent,
				// not necessarily the same object.
				// TODO: replace with Predicate remove.
				viewBindingList.Remove( b );

				viewBindingList.Add( new_binding );

				//elementBindings.RemoveAt( i );
				elementBindings[i] = new_binding;

				return;


			} // for-loop.

		}

		/// <summary>
		/// Attempts to determine if two input bindings are basically the same.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		private bool BindingsEqual( InputBinding a, InputBinding b ) {

			return ( ( a.CommandTarget == b.CommandTarget ) && (a.Gesture.Equals(b.Gesture) )
				&& ( a.CommandParameter.Equals( b.CommandParameter ) ) &&
				( a.Command == b.Command ) );

		}

		public void AddBindings( ViewModelBase viewModel, IEnumerable<InputBinding> new_bindings ) {
			
			if( viewModel.ViewElement == null ) {
				Console.WriteLine( "ViewElement null. No Bindings created." );
				return;
			}

			InputBindingCollection elementBindings = viewModel.ViewElement.InputBindings;

			foreach( InputBinding b in new_bindings ) {
				Console.WriteLine( "adding binding: " + b.Gesture.ToString() );
				elementBindings.Add( b );
			}
			Console.WriteLine( "BINDING ADD COUNT: " + new_bindings.Count() );
			this.GetBindingList( viewModel ).AddRange( new_bindings );

		} //

		public void AddBinding( ViewModelBase model, InputBinding binding ) {

			FrameworkElement element = model.ViewElement;
			if( element == null ) {
				return;
			}
			Console.WriteLine( "adding binding: " + binding.Gesture );
			element.InputBindings.Add( binding );
			this.GetBindingList( model ).Add( binding );

		}

		/// <summary>
		/// Remove a binding according to an InputGesture.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="gesture"></param>
		public void RemoveBinding( ViewModelBase model, InputGesture gesture ) {

			FrameworkElement element = model.ViewElement;
			if( element == null ) {
				return;
			}

			InputBindingCollection elementBindings = element.InputBindings;
			// need to find the exact class instance that was added.
			for( int i = elementBindings.Count - 1; i >= 0; i-- ) {

				if( !gesture.Equals( elementBindings[i].Gesture ) ) {
					continue;
				}

				Console.WriteLine( "Removing binding" );
				this.GetBindingList( model ).Remove( elementBindings[i] );
				elementBindings.RemoveAt( i );

				return;

			} // for-loop.

		}

		public void RemoveBinding( ViewModelBase model, InputBinding binding ) {

			FrameworkElement element = model.ViewElement;
			if( element == null ) {
				return;
			}

			InputBindingCollection elementBindings = element.InputBindings;
			// need to find the exact class instance that was added.
			for( int i = elementBindings.Count - 1; i >= 0; i-- ) {

				if( !BindingsEqual( elementBindings[i], binding ) ) {
					continue;
				}
				Console.WriteLine( "removing binding: " + binding.Gesture );
				this.GetBindingList( model ).Remove( elementBindings[i] );
				elementBindings.RemoveAt( i );

				return;

			} // for-loop.

		}

		/// <summary>
		/// Clear any bindings currently tied to a view model.
		/// </summary>
		/// <param name="model"></param>
		public void RemoveBindings( ViewModelBase model ) {

			FrameworkElement element = model.ViewElement;
			if( element == null ) {
				return;
			}

			InputBindingCollection elementBindings = element.InputBindings;
			List<InputBinding> bindings;

			if( bindingGroups.TryGetValue( model, out bindings ) ) {

				foreach( InputBinding b in bindings ) {
					elementBindings.Remove( b );
				}
				Console.WriteLine( "BINDING REMOVE COUNT: " + bindings.Count );
				bindings.Clear();

			}

		} //

	} // class

	/// <summary>
	/// TODO: Separate helper class to simplify code.
	/// </summary>
	/*class InputViewBinder {

		/// <summary>
		/// Combination of elements that own the binding.
		/// </summary>
		private Tuple<ViewModelBase, FrameworkElement> bindingSource;


		/// <summary>
		/// Bindings set through this viewModel.
		/// Storing this allows the bindings to be cleared as a group.
		/// </summary>
		List<InputBinding> bindings;

	} // class*/

} // namespace
