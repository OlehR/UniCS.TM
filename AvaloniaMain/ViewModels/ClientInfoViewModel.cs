using AvaloniaMain.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelMID;
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
        

        private MainViewModel _mainViewModel;
        private Client _client;
        public Client Client
        {
            get { return _client; }
            set
            {
                if(value != _client) 
                {
                    _client = value;
                    OnPropertyChanged(nameof(Client));
                }
            }
        }

        private ReactiveCommand<Unit, Unit> _closeCommand;

        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(CloseAsync);

        public ClientInfoViewModel(MainViewModel mainViewModel, Client client) 
        {
             Client = client;
            _mainViewModel = mainViewModel;
        }

        private async Task CloseAsync()
        {
             _mainViewModel.Close();
        }
    }
}