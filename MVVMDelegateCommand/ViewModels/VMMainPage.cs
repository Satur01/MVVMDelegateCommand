using System;
using System.Windows.Input;
using Windows.UI.Popups;
using MVVMDelegateCommand.ViewModels.ViewModelBase;

namespace MVVMDelegateCommand.ViewModels
{
    public class VMMainPage : BindableBase
    {

        public VMMainPage()
        {
            _sendMessageCommand =
                new Lazy<DelegateCommand>(
                    () => new DelegateCommand(SendMessageCommandExecute, SendMessageCommandCanExecute));
        }

        #region Properties

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
                _sendMessageCommand.Value.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands

        private Lazy<DelegateCommand> _sendMessageCommand;

        public ICommand SendMessageCommand
        {
            get { return _sendMessageCommand.Value; }
        }

        #endregion


        #region Methods

        private bool SendMessageCommandCanExecute()
        {
            return !string.IsNullOrEmpty(Name);
        }

        private async void SendMessageCommandExecute()
        {
            MessageDialog messageDialog = new MessageDialog(string.Format("Hola {0}!!", Name));
            await messageDialog.ShowAsync();
        }

        #endregion

    }
}
