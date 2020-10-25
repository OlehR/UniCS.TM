using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace OnScreenKeyboardControl.Keyboard.Keys
{
	//internal delegate void OnScreenKeyEventHandler(DependencyObject sender, OnScreenKeyEventArgs e);

	public abstract class OnScreenKey : ToggleButton
	{
		private readonly string[] _values;
		private readonly ExecuteDelegate _executeFunction;
		private readonly CaptionUpdateDelegate _valueIndexFunction;

		internal List<OnScreenKeyStateModifier> Modifiers { get; private set; } = new List<OnScreenKeyStateModifier>();

		protected OnScreenKey(int row, int column, string[] values,
			CaptionUpdateDelegate valueIndexFunction = null,
			ExecuteDelegate executeFunction = null)
		{
			_values = values;
			_valueIndexFunction = valueIndexFunction ?? CaptionUpdateDelegateDelegateFunction.FirstInArray;
			_executeFunction = executeFunction ?? ExecuteDelegateFunctions.DefaultExecuteDelegate;
			Content = Value = values[0];
			GridRow = row;
			GridColumn = column;

			Click += KeyPressEventHandler;
		}

		public virtual void CanType(bool canPress) => IsEnabled = canPress;

		protected string GetCurrentValue(int index)
		{
			return _values[Math.Min(index, _values.Length - 1)];
		}

		internal abstract void KeyPressEventHandler(object sender, RoutedEventArgs routedEventArgs);

		internal void Update(List<OnScreenKeyStateModifier> modifiers)
		{
			Modifiers = modifiers;
			Update();
		}

		protected virtual void Update()
		{
			Content = Value = GetCurrentValue(_valueIndexFunction(Modifiers));
		}

		internal event EventHandler<OnScreenKeyPressEventArgs> OnScreenKeyPressEvent;

		internal void OnClick(OnScreenKeyPressEventArgs arg)
		{
			var handler = OnScreenKeyPressEvent;
			handler?.Invoke(this, arg);
		}

		public int GridRow
		{
			get { return (int)GetValue(Grid.RowProperty); }
			set { SetValue(Grid.RowProperty, value); }
		}

		public int GridColumn
		{
			get { return (int)GetValue(Grid.ColumnProperty); }
			set { SetValue(Grid.ColumnProperty, value); }
		}

		public GridLength GridWidth { get; set; }

		public string Value { get; protected set; }

		protected virtual void Execute(FrameworkElement frameworkElement)
		{
				_executeFunction(this, frameworkElement);
		}

		public string ClickCommand
		{
			get { return _clickCommand; }
			set
			{
				_clickCommand = value;
				SetBinding(Button.CommandProperty, new Binding(_clickCommand)
				{ RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(OnScreenKeyboard), 1) });
			}
		}
		private string _clickCommand;
	}
}