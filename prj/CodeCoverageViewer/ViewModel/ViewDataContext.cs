using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CodeCoverageViewer.Base;
using CodeCoverageViewer.Utility;

namespace CodeCoverageViewer;

#region ViewDataContext class ----------------------------------------------------------
/// <summary> ViewDataContext is data context class for MainWindow.xaml</summary>
class ViewDataContext : ModelNotifyPropertyChanged {
   // Constructor -------------------------------------------------------------
   public ViewDataContext () { }

   // Properties --------------------------------------------------------------
   private TreeView CoverageTree {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.CoverageItems;
      }
   }
   private Label SourceBlocksStatusLb {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.SourceBlocksStatusLb;
      }
   }

   #region "FileOpenCommand"
   /// <summary> File Open menu handler</summary>
   public ICommand FileOpenCommand {
      get {
         if (this.fileOpenCommand == null)
            this.fileOpenCommand = new Base.Command (param => this.FileOpen (), null);

         return this.fileOpenCommand;
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

      this.loadCoverageFile (dialog.FileName);
   }

   string coverageFilename = null;
   /// <summary> Loads a Coverage Xml file</summary>
   /// <param name="fileName"> Specifies the Xml coverage filename</param>
   void loadCoverageFile (string fileName) {
      this.coverageFilename = fileName;
      // Loads Code Coveage informations
      Reader reader = new ();
      Item.RootItem rootItem = reader.Read (this.coverageFilename);
      if (null == rootItem) {
         MessageBox.Show (reader.Error, "Code Coverage Report File Read Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      // Set coverage items to TreeView, SourceViewer
      this.treeViewHandler.SetCoverage (rootItem);
      this.sourceViewerHandler.SetCoverage (rootItem);

   }
   #endregion "FileOpenCommand"

   #region "Recompute"
   /// <summary> File Open menu handler</summary>
   public ICommand RecomputeCommand {
      get {
         if (this.recomputeCommmand == null)
            this.recomputeCommmand = new Base.Command (param => this.Recompute (), 
                                                       canExcute => null != this.coverageFilename);

         return this.recomputeCommmand;
      }
   }
   private ICommand recomputeCommmand;
   public void Recompute () {
      if (null == this.coverageFilename)
         return;

      this.loadCoverageFile (this.coverageFilename);
   }
   #endregion 

   // Methods -----------------------------------------------------------------
   /// <summary> Initializes UI object such as Tree, SourceView...</summary>
   public void Init () {
      TreeViewItem treeViewItem = this.treeViewHandler.InitCoverageTree (this.CoverageTree, this.sourceViewerHandler);
      treeViewItem.Selected += loadReport_Selected;

      MainWindow mainWindow = this.ui as MainWindow;
      this.sourceViewerHandler.Init (mainWindow.SourceViewer, this.SourceBlocksStatusLb,
                                     this.treeViewHandler, mainWindow);
   }

   // Implementation ----------------------------------------------------------
   /// <summary> Report file open shortcut Event handler </summary>
   private void loadReport_Selected (object sender, RoutedEventArgs e) {
      this.FileOpen ();
   }

   // Private Data ------------------------------------------------------------
   public Window ui = null;
   public Utility.TreeViewHandler treeViewHandler { get; } = new ();
   private Utility.SourceViewerHandler sourceViewerHandler { get; } = new ();   
}
#endregion ViewDataContext class --------------------------------------------------------
