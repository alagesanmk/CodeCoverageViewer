using System.Windows;

namespace CodeCoverageViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

   ViewDataContext viewModelContext = new ViewDataContext();
   public MainWindow () {
      InitializeComponent ();
      this.DataContext = this.viewModelContext;
      this.viewModelContext.ui = this;
      this.viewModelContext.Init ();
   }
}
