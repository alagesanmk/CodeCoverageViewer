using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeCoverageViewer.Base;

#region ModelNotifyPropertyChanged class ----------------------------------------------------------
/// <summary> Base class to use in Xaml Binding Notify Property change in Datacontext</summary>
public class ModelNotifyPropertyChanged : INotifyPropertyChanged
{
   public event PropertyChangedEventHandler PropertyChanged;

   #region INotifyPropertyChanged ---------------------------------------------------------
   protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
      if (PropertyChanged!=null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
   }
   #endregion INotifyPropertyChanged -----------------------------------------------------
}

#endregion ModelNotifyPropertyChanged class -------------------------------------------------------