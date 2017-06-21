using System;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Dialog;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class AddMessageDialogViewModel : BindableBase, IDialogViewModel
    {
        private readonly IAutomatedMessagesManager _messagesManager;
        private readonly IChatManager _chatManager;
        private AutomatedMessageWrapper _editedMessage;
        private string _message;
        private int _interval;

        public Action Close { get; set; }
        public string Message { get { return _message; } set { _message = value; RaisePropertyChanged(); } }
        public int Interval { get { return _interval; } set { _interval = value; RaisePropertyChanged(); } }

        public AddMessageDialogViewModel(IAutomatedMessagesManager messagesManager, IChatManager chatManager)
        {
            _messagesManager = messagesManager;
            _chatManager = chatManager;
        }

        public void SetData(object data)
        {
            var msg = data as AutomatedMessageWrapper;
            if (msg == null)
                return;

            _editedMessage = msg;
            Message = msg.Message;
            Interval = msg.Interval;
        }

        [Command]
        public void Add()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;
            if (Interval < 1)
                return;

            if (_editedMessage == null)
                _messagesManager.AddAutomatedMessage(Message, Interval, _chatManager.IsConnected, true);
            else
                _messagesManager.UpdateAutomatedMessage(_editedMessage.Id, Message, Interval, _editedMessage.IsRunning, _editedMessage.Enabled);
            Close?.Invoke();
        }
    }
}
