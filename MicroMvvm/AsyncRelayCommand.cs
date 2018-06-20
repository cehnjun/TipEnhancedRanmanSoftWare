using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroMvvm
{
    public class AsyncRelayCommand : RelayCommand
    {
        private bool _isExecuting = false;
        public bool IsExecuting { get => _isExecuting; private set => _isExecuting = value; }
        private void onRunWorkerCompleted(EventArgs e)
        {
            IsExecuting = false;
            Ended?.Invoke(this, e);
        }

        public event EventHandler Started;
        public event EventHandler Ended;

        public AsyncRelayCommand(Action execute, Func<bool> canExecute) : base(execute, canExecute) { }
        public AsyncRelayCommand (Action execute) : base(execute) { }
        public override bool CanExecute(object parameter) => base.CanExecute(parameter) && !IsExecuting;
        public override void Execute(object parameter)
        {
            IsExecuting = true;
            Started?.Invoke(this, EventArgs.Empty);
            var task = Task.Factory.StartNew(() => base.Execute(parameter));
            task.ContinueWith(t => onRunWorkerCompleted(EventArgs.Empty), TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
