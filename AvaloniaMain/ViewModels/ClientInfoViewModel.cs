using AvaloniaMain.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public class ClientInfoViewModel : ViewModelBase
    {
        private string _clientName="dfczad";

        public string ClientName
        {
            get => _clientName;
            set => this.RaiseAndSetIfChanged(ref _clientName, value);
        }

        private MainViewModel _mainViewModel;


        private ReactiveCommand<Unit, Unit> _closeCommand;

        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(CloseAsync);

        public ClientInfoViewModel(MainViewModel mainViewModel) 
        {
            _mainViewModel = mainViewModel;
        }

        private async Task CloseAsync()
        {
             _mainViewModel.Close();
        }
    }
}