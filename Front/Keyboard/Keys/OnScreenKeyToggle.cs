using System.Linq;
using System.Windows;

namespace OnScreenKeyboardControl.Keyboard.Keys
{
	public class OnScreenKeyToggle : OnScreenKey
	{
		private readonly OnScreenKeyModifierType _modifierType;
		public OnScreenKeyToggle(int row, int column, string[] values, OnScreenKeyModifierType modifierType) : base(row, column, values)
		{
			_modifierType = modifierType;
		}

		internal override void KeyPressEventHandler(object sender, RoutedEventArgs routedEventArgs)
		{
			OnClick(new OnScreenKeyPressEventArgs(new OnScreenKeyStateModifier(_modifierType, false, IsChecked == false)));
		}

		protected override void Update()
		{
			IsChecked = Modifiers.Any(i => i.ModifierType == _modifierType && i.SingleInstance == false);
		}
	}
}