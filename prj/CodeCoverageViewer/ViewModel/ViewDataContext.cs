using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CodeCoverageViewer.Base;
using CodeCoverageViewer.Utility;

namespace CodeCoverageViewer;

#region ViewDataContext class ----------------------------------------------------------
/// <summary> Data context class for MainWindow.xaml</summary>
class ViewDataContext : ModelNotifyPropertyChanged {
   #region Commands -----------------------------------------------------------
   // FileOpenCommand ---------------------------------------------------------
   /// <summary> File Open menu handler</summary>
   public ICommand FileOpenCommand {
      get {
         if (this.fileOpenCommand == null)
            this.fileOpenCommand = new Base.Command (param => this.sourceViewerHandler.FileOpen ());

         return this.fileOpenCommand;
      }
   }
   private ICommand fileOpenCommand;

   // RecomputeCommand ---------------------------------------------------------
   /// <summary> File Open menu handler</summary>
   public ICommand RecomputeCommand {
      get {
         if (this.recomputeCommmand == null)
            this.recomputeCommmand = new Base.Command (
               param => this.sourceViewerHandler.Recompute (),
               canExcute => this.sourceViewerHandler.CanRecompute ());

         return this.recomputeCommmand;
      }
   }
   private ICommand recomputeCommmand;   
   #endregion

   // Methods -----------------------------------------------------------------
   /// <summary> Initializes UI object such as Tree, SourceView...</summary>
   public void Init () {
      TreeViewItem treeViewItem = this.treeViewHandler.InitCoverageTree (this.coverageTree, this.sourceViewerHandler);
      treeViewItem.Selected += (s, e) => this.sourceViewerHandler.FileOpen ();      

      MainWindow mainWindow = this.ui as MainWindow;
      this.sourceViewerHandler.Init (mainWindow.SourceViewer, this.sourceBlocksStatusLb,
                                     this.treeViewHandler, mainWindow);
   }

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
}
#endregion ViewDataContext class --------------------------------------------------------
