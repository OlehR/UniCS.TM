using System.Windows;

namespace OnScreenKeyboardControl.Keyboard
{
	internal class OnScreenKeyPressEventArgs
	{
		public delegate void ExecuteKeyPress(FrameworkElement frameworkElement);

		public OnScreenKeyStateModifier StateModifier { get; }

		internal OnScreenKeyPressEventArgs(OnScreenKeyStateModifier stateModifier)
		{
			StateModifier = stateModifier;
		}

		public OnScreenKeyPressEventArgs(ExecuteKeyPress execute)
		{
			Execute = execute;
		}

		public ExecuteKeyPress Execute { get; }
	}
}