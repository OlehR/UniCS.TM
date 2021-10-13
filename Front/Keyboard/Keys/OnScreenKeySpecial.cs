using System.Windows;

namespace OnScreenKeyboardControl.Keyboard.Keys
{
	public class OnScreenKeySpecial : OnScreenKey
	{
		internal OnScreenKeySpecial(int row, int column, string label, ExecuteDelegate executeDelegate, bool pIsEnabled = true)
			: base(row, column, new[] { label }, null, executeDelegate){ IsEnabled = pIsEnabled; }

		internal OnScreenKeySpecial(int row, int column, string label, string value, bool pIsEnabled = true)
		: base(row, column, new[] { label })
		{
			Value = value;
			IsEnabled = pIsEnabled;
		}

		internal override void KeyPressEventHandler(object sender, RoutedEventArgs routedEventArgs)
		{
			IsChecked = false;
			OnClick(new OnScreenKeyPressEventArgs(Execute));
		}

		protected override void Update() { }

		public override void CanType(bool canPress) { }
	}
}