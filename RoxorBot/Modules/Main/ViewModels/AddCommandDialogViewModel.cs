using System;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Dialog;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class AddCommandDialogViewModel : BindableBase, IDialogViewModel
    {
        private string _command;
        private string _reply;
        private UserCommandWrapper _selectedCommand;
        private readonly IUserCommandsManager _commandsManager;

        public Action Close { get; set; }
        public string Command { get { return _command; } set { _command = value; RaisePropertyChanged(); } }
        public string Reply { get { return _reply; } set { _reply = value; RaisePropertyChanged(); } }

        public AddCommandDialogViewModel(IUserCommandsManager commandsManager)
        {
            _commandsManager = commandsManager;
        }

        public void SetData(object data)
        {
            var command = data as UserCommandWrapper;
            if (command == null)
                return;

            _selectedCommand = command;
            Command = command.Command;
            Reply = command.Reply;
        }

        [Command]
        public void Add()
        {
            if (string.IsNullOrWhiteSpace(Command))
                return;
            if (string.IsNullOrWhiteSpace(Reply))
                return;

            if (_selectedCommand == null)
                _commandsManager.AddCommand(Command, Reply);
            else
                _commandsManager.UpdateCommand(_selectedCommand.Id, Command, Reply);
            Close?.Invoke();
        }
    }
}
