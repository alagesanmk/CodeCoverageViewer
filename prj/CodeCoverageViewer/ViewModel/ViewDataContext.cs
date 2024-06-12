using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CodeCoverageViewer.Base;
using CodeCoverageViewer.Utility;

namespace CodeCoverageViewer;

#region class ViewDataContext ----------------------------------------------------------
/// <summary>
/// ViewDataContext is data context class for MainWindow.xaml
/// </summary>
class ViewDataContext : ModelNotifyPropertyChanged {
   #region Constructor ---------------------------------------------------------
   public ViewDataContext () { }
   #endregion

   #region Properties ----------------------------------------------------------
   private TreeView CoverageTree {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.CoverageItems;
      }
   }
   #endregion Properties -------------------------------------------------------

   #region "FileOpenCommand"
   /// <summary>
   /// File Open menu handler
   /// </summary>
   public ICommand FileOpenCommand {
      get {
         if (fileOpenCommand == null)
            fileOpenCommand = new Base.Command (param => this.FileOpen (), null);

         return fileOpenCommand;
      }
   }
   private ICommand fileOpenCommand;

   /// <summary>
   /// File Open function is to select a Code Coverage report file and 
   /// loads report informations
   /// </summary>
   public void FileOpen () {
      OpenFileDialog dialog = new OpenFileDialog ();
      dialog.Filter = "Code Coverage Reports|*.xml";

      if (false == dialog.ShowDialog ())
         return;

      // Loads Code Coveage informations
      Reader reader = new ();
      Item.RootItem rootItem = reader.Read (dialog.FileName);
      if (null == rootItem) {
         MessageBox.Show (reader.Error, "Code Coverage Report File Read Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      // Set coverage items to TreeView, SourceViewer
      this.treeViewHandler.SetCoverage (rootItem, this.CoverageTree, this.sourceViewerHandler);

      MainWindow mainWindow = this.ui as MainWindow;
      this.sourceViewerHandler.SetCoverage (rootItem, mainWindow.SourceViewer, this.treeViewHandler);
   }
   #endregion "FileOpenCommand"

   #region Methods -------------------------------------------------------------
   /// <summary>
   /// Initializes Coverage Tree with shortcut item to load report file
   /// </summary>
   public void InitCoverageTree () {
      TreeViewItem treeViewItem = this.treeViewHandler.InitCoverageTree (this.CoverageTree);
      treeViewItem.Selected += loadReport_Selected;
   }
   #endregion Methods ----------------------------------------------------------

   #region Implementation ------------------------------------------------------
   /// <summary>
   /// Report file open shortcut Event handler 
   /// </summary>
   private void loadReport_Selected (object sender, RoutedEventArgs e) {
      this.FileOpen ();
   }   
   #endregion Implementation ---------------------------------------------------

   #region Private Data --------------------------------------------------------
   public Window ui = null;
   public Utility.TreeViewHandler treeViewHandler { get; } = new ();
   private Utility.SourceViewerHandler sourceViewerHandler { get; } = new ();
   #endregion
}
#endregion class ViewDataContext ----------------------------------------------------------
