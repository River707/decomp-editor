using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DecompEditor.Utils {
  public class DelegateCommand<T> : ICommand {
    private readonly Action<object> ExecuteAction;
    private readonly Func<object, bool> CanExecuteFunc;

    public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteFunc = null) {
      ExecuteAction = executeAction;
      CanExecuteFunc = canExecuteFunc;
    }

    public bool CanExecute(object parameter) {
      return CanExecuteFunc == null || CanExecuteFunc(parameter);
    }

    public void Execute(object parameter) {
      ExecuteAction(parameter);
    }

    public event EventHandler CanExecuteChanged;

    public void RaiseCanExecuteChanged() {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}
