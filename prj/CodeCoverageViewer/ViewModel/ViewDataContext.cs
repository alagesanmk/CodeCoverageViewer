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
   #region Commands -----------------------------------------------------------
   #region "FileOpenCommand"
   /// <summary> File Open menu handler</summary>
   public ICommand FileOpenCommand {
      get {
         if (this.fileOpenCommand == null)
            this.fileOpenCommand = new Base.Command (param => this.fileOpen ());

         return this.fileOpenCommand;
      }
   }
   private ICommand fileOpenCommand;
   #endregion "FileOpenCommand"

   #region "RecomputeCommand"
   /// <summary> File Open menu handler</summary>
   public ICommand RecomputeCommand {
      get {
         if (this.recomputeCommmand == null)
            this.recomputeCommmand = new Base.Command (
               param => {
                  // Handles FileOpenCommand ----------------------------------------------
                  if (null == this.coverageFilename)
                     return;

                  this.loadCoverageFile (this.coverageFilename);
               }, 
               canExcute => null != this.coverageFilename);

         return this.recomputeCommmand;
      }
   }
   private ICommand recomputeCommmand;
   #endregion
   #endregion

   // Methods -----------------------------------------------------------------
   /// <summary> Initializes UI object such as Tree, SourceView...</summary>
   public void Init () {
      TreeViewItem treeViewItem = this.treeViewHandler.InitCoverageTree (this.coverageTree, this.sourceViewerHandler);
      treeViewItem.Selected += (s, e) => this.fileOpen ();      

      MainWindow mainWindow = this.ui as MainWindow;
      this.sourceViewerHandler.Init (mainWindow.SourceViewer, this.sourceBlocksStatusLb,
                                     this.treeViewHandler, mainWindow);
   }

   #region Implementation -----------------------------------------------------
   /// <summary>
   /// File Open function is to select a Code Coverage report file and 
   /// loads report informations
   /// </summary>
   public void fileOpen () {
      OpenFileDialog dialog = new OpenFileDialog ();
      dialog.Filter = "Code Coverage Reports|*.xml";

      if (false == dialog.ShowDialog ())
         return;

      this.loadCoverageFile (dialog.FileName);
   }

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
   #endregion

   #region Private Properties -------------------------------------------------
   TreeView coverageTree {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.CoverageItems;
      }
   }
   Label sourceBlocksStatusLb {
      get {
         MainWindow mainWindow = this.ui as MainWindow;
         return mainWindow.SourceBlocksStatusLb;
      }
   }
   #endregion

   // Private Data ------------------------------------------------------------
   public Window ui = null;
   public Utility.TreeViewHandler treeViewHandler { get; } = new ();
   private Utility.SourceViewerHandler sourceViewerHandler { get; } = new ();

   string coverageFilename = null;
}
#endregion ViewDataContext class --------------------------------------------------------
