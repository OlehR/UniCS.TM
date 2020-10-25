namespace OnScreenKeyboardControl.Keyboard
{
	public class OnScreenKeyStateModifier
	{
		public OnScreenKeyStateModifier(OnScreenKeyModifierType modifierType, bool singleInstance, bool clear)
		{
			Clear = clear;
			ModifierType = modifierType;
			SingleInstance = singleInstance;
		}

		public OnScreenKeyModifierType ModifierType { get; }
		public bool SingleInstance { get; }
		public bool Clear { get; }
	}
}