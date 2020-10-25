using System.Windows;

namespace OnScreenKeyboardControl.Keyboard.Keys
{
	public class OnScreenKeyNormal : OnScreenKey
	{
		internal OnScreenKeyNormal(int row, int column, string[] values, CaptionUpdateDelegate valueIndexFunction) 
			: base(row, column, values, valueIndexFunction) { }
		internal override void KeyPressEventHandler(object sender, RoutedEventArgs routedEventArgs)
		{
			IsChecked = false;
			OnClick(new OnScreenKeyPressEventArgs( Execute));
		}
	}
}