using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using OnScreenKeyboardControl.Keyboard.Keys;

namespace OnScreenKeyboardControl.Keyboard
{
	public delegate void ExecuteDelegate(OnScreenKey button, FrameworkElement frameworkElement);
	internal static class ExecuteDelegateFunctions
	{

		public static ExecuteDelegate BackspaceExecuteDelegate = (key, frameworkElement) =>
		{
			if (frameworkElement is TextBoxBase)
			{
				var textBoxBase = (TextBox)frameworkElement;
				var start = textBoxBase.Text.Substring(0, textBoxBase.SelectionStart);
				var end = textBoxBase.Text.Substring(textBoxBase.SelectionStart + textBoxBase.SelectionLength);
				if (textBoxBase.SelectionLength > 0)
				{
					textBoxBase.Text = start + end;
					textBoxBase.SelectionLength = 0;
				}
				else if (start.Length > 0)
				{
					textBoxBase.Text = start.Substring(0, start.Length - 1) + end;
					textBoxBase.SelectionStart = start.Length - 1;
					textBoxBase.SelectionLength = 0;
				}
			}
			else if (frameworkElement is PasswordBox)
			{
				var passwordBoxBase = (PasswordBox)frameworkElement;
				passwordBoxBase.Password = passwordBoxBase.Password.Substring(0, passwordBoxBase.Password.Length - 1);
			}
		};

		public static ExecuteDelegate DefaultExecuteDelegate = (key, frameworkElement) =>
		{
			if (frameworkElement is TextBoxBase)
			{
				var textBoxBase = (TextBox)frameworkElement;
				var start = textBoxBase.Text.Substring(0, textBoxBase.SelectionStart);
				var end = textBoxBase.Text.Substring(textBoxBase.SelectionStart + textBoxBase.SelectionLength);
				textBoxBase.Text = start + key.Value + end;
				textBoxBase.SelectionStart = start.Length + 1;
				textBoxBase.SelectionLength = 0;
			}
			else if (frameworkElement is PasswordBox)
			{
				var passwordBoxBase = (PasswordBox)frameworkElement;
				passwordBoxBase.Password = passwordBoxBase.Password + key.Value;
			}
		};

		public static ExecuteDelegate ClearExecuteDelegate = (key, frameworkElement) =>
		{
			if (frameworkElement is TextBoxBase)
			{
				var textBoxBase = (TextBox)frameworkElement;
				textBoxBase.Text = string.Empty;
			}
			else if (frameworkElement is PasswordBox)
			{
				var passwordBoxBase = (PasswordBox)frameworkElement;
				passwordBoxBase.Password = string.Empty;
			}
		};

		public static ExecuteDelegate MoveNextExecuteDelegate = (key, frameworkElement) =>
		{
			frameworkElement.MoveFocus(key.Modifiers.Any(i => i.ModifierType == OnScreenKeyModifierType.Shift)
				? new TraversalRequest(FocusNavigationDirection.Previous)
				: new TraversalRequest(FocusNavigationDirection.Next));
		};
	}
}