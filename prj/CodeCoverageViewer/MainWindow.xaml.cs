using System.Windows;

namespace CodeCoverageViewer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

   ViewDataContext viewModelContext = new ();
   public MainWindow () {
      InitializeComponent ();
      this.DataContext = this.viewModelContext;
      this.viewModelContext.mainWindow = this;
      this.viewModelContext.Init ();
   }
}
