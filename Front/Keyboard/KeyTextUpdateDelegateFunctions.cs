using System.Collections.Generic;
using System.Linq;

namespace OnScreenKeyboardControl.Keyboard
{
	public delegate int CaptionUpdateDelegate(IEnumerable<OnScreenKeyStateModifier> values);

	internal static class CaptionUpdateDelegateDelegateFunction
	{
		public static int ShiftAndSpecial(IEnumerable<OnScreenKeyStateModifier> values)
		{
			var capValue = values.Any(i => (i.ModifierType == OnScreenKeyModifierType.Shift || i.ModifierType == OnScreenKeyModifierType.CapsLock)) ? 1 : 0;
			var specialValue =values.Any(i => i.ModifierType == OnScreenKeyModifierType.Special) ? 2 : 0;
			return capValue + specialValue;
		}

		public static int CapsLookAndSpecial(IEnumerable<OnScreenKeyStateModifier> values)
		{
			var capValue = values.Any(i => (i.ModifierType == OnScreenKeyModifierType.Shift )) ? 1 : 0;
			var specialValue = values.Any(i => i.ModifierType == OnScreenKeyModifierType.Special) ? 2 : 0;
			return capValue + specialValue;
		}

		public static int NumberOnly(IEnumerable<OnScreenKeyStateModifier> values)
		{
			return values.Any(i => i.ModifierType != OnScreenKeyModifierType.NumLock) ? 0 : 1;
		}
		public static int FirstInArray(IEnumerable<OnScreenKeyStateModifier> values)
		{
			return 0;
		}
	}
}