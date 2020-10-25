using System.Windows;

namespace OnScreenKeyboardControl.Keyboard.Keys
{
	public class OnScreenKeySpecial : OnScreenKey
	{
		internal OnScreenKeySpecial(int row, int column, string label, ExecuteDelegate executeDelegate)
			: base(row, column, new[] { label }, null, executeDelegate){}

		internal OnScreenKeySpecial(int row, int column, string label, string value)
		: base(row, column, new[] { label })
		{
			Value = value;
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