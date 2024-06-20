using System.Windows.Input;

namespace CodeCoverageViewer.Base;

#region Command class -------------------------------------------------------------------
/// <summary> Base class to use in Xaml Binding Command</summary>
public class Command : ICommand {
   #region Constructor ---------------------------------------------------------
   public Command (Action<object> execute): this(execute, null) {}

   public Command(Action<object> execute, Predicate<object> canExecute=null)
   {
      if (execute == null)
            throw new ArgumentNullException("execute");

      _execute = execute;
      _canExecute = canExecute;
   }
   #endregion

   #region ICommand ------------------------------------------------------------
   public bool CanExecute(object parameter) {
      return _canExecute == null ? true : _canExecute(parameter);         
   }
   private readonly Predicate<object> _canExecute = null;

   public event EventHandler CanExecuteChanged {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
   }

   public void Execute(object parameter) {
      _execute(parameter);
   }
   private readonly Action<object> _execute;
   #endregion
}
#endregion Command class ----------------------------------------------------------------