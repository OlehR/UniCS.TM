using System.Windows;

namespace OnScreenKeyboardControl
{
	public class ViewModel
	{
		private RelayCommand _cancelCommand;
		private RelayCommand _saveCommand;

		public RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(() =>
		{
			MessageBox.Show("Cancel button pressed");
		}));
		public RelayCommand SaveCommand => _saveCommand ?? (_saveCommand = new RelayCommand(() =>
		{
			MessageBox.Show("Save button pressed");
		}));
	}
}
