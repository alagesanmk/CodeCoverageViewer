using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CodeCoverageViewer.Base;
using CodeCoverageViewer.Utility;

namespace CodeCoverageViewer;

#region class ViewDataContext ------------------------------------------------------------
/// <summary>
/// ViewDataContext is data context class for MainWindow.xaml
/// </summary>
class ViewDataContext : ModelNotifyPropertyChanged {
   // region Constructor -------------------------------------------------------
   public ViewDataContext () { }

   #region Properties ----------------------------------------------------------
   private TreeView CoverageTree {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.CoverageItems;
      }
   }
   private Label SourceCoverage {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.SourceCoverage;
      }
   }
   #endregion Properties 

   #region "FileOpenCommand"
   /// <summary>
   /// File Open menu handler
   /// </summary>
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
   /// <summary> Loads a Coverage Xml file
   /// </summary>
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
      this.treeViewHandler.SetCoverage (rootItem, this.CoverageTree, this.sourceViewerHandler);

      MainWindow mainWindow = this.ui as MainWindow;
      this.sourceViewerHandler.SetCoverage (rootItem, mainWindow.SourceViewer, this.SourceCoverage, 
                                            this.treeViewHandler, (Window)this.ui);

   }
   #endregion "FileOpenCommand"

   #region "Recompute"
   /// <summary>
   /// File Open menu handler
   /// </summary>
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

   // region Methods ----------------------------------------------------------
   /// <summary>
   /// Initializes Coverage Tree with shortcut item to load report file
   /// </summary>
   public void InitCoverageTree () {
      TreeViewItem treeViewItem = this.treeViewHandler.InitCoverageTree (this.CoverageTree);
      treeViewItem.Selected += loadReport_Selected;
   }   

   // region Implementation ---------------------------------------------------
   /// <summary>
   /// Report file open shortcut Event handler 
   /// </summary>
   private void loadReport_Selected (object sender, RoutedEventArgs e) {
      this.FileOpen ();
   }      

   // region Private Data -----------------------------------------------------
   public Window ui = null;
   public Utility.TreeViewUIHandler treeViewHandler { get; } = new ();
   private Utility.SourceViewerHandler sourceViewerHandler { get; } = new ();   
}
#endregion class ViewDataContext -------------------------------------------------------
