using System.Windows.Controls;
using System.Windows.Input;

using CodeCoverageViewer.Base;

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
   private ICommand fileOpenCommand = null!;

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
   private ICommand recomputeCommmand = null!;
   #endregion

   // Methods -----------------------------------------------------------------
   /// <summary> Initializes UI object such as Tree, SourceView...</summary>
   public void Init () {
      TreeViewItem treeViewItem = this.treeViewHandler.InitCoverageTree (this.mainWindow.CoverageTreeView, this.sourceViewerHandler);
      treeViewItem.Selected += (s, e) => this.sourceViewerHandler.FileOpen ();

      this.sourceViewerHandler.Init (mainWindow.SourceViewer, this.mainWindow.SourceBlocksStatusLb,
                                     this.treeViewHandler, this.mainWindow);
   }

   // Private Data ------------------------------------------------------------
   public MainWindow mainWindow = null!;
   public Utility.TreeViewHandler treeViewHandler { get; } = new ();
   private Utility.SourceViewerHandler sourceViewerHandler { get; } = new ();   
}
#endregion ViewDataContext class --------------------------------------------------------
