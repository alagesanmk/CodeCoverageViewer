using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;

namespace CodeCoverageViewer.Utility;

using LineNo2RangesParaMap = Dictionary<int, RangesParagraph>;

#region TreeViewHandler class ------------------------------------------------------------
/// <summary> An UI handler class for TreeView</summary>
class TreeViewHandler {
   #region Methods ------------------------------------------------------------
   /// <summary> Initialize Tree Item to handle Selected event and call Coverage file open method</summary>
   /// <param name="treeView"> Specifies TreeView ui object to load Source Item</param>
   /// <param name="sourceViewerHandler"> Specifies depended SourceViewerHandler</param>
   /// <returns>Returns create Tree Item</returns>
   public TreeViewItem InitCoverageTree (TreeView treeView, SourceViewerHandler sourceViewerHandler) {
      this.treeView = treeView;
      this.sourceViewerHandler = sourceViewerHandler;

      TreeViewItem treeViewItem = new TreeViewItem ();
      treeViewItem.Header = "[Click to load Report ...]";  // Short cut to open report file 
      this.treeView.Items.Add (treeViewItem);
      return treeViewItem;      
   }

   /// <summary Set Coverage Items to Tree View and initialzes the Source viewer</summary>
   /// <param name="rootItem"> Coverage items to set Treeview</param>
   public void SetCoverage(Item.RootItem rootItem) {
      this.treeView.Items.Clear();

      foreach (var item in rootItem.DriveItems) {
         this.treeView.Items.Add(item);
         item.ExpandSubtree();

         this.connectSourceItemEvent (item);
      }
   }

   /// <summary>
   /// Returns LineNo2RangesParaMap identified by sourceItem.
   /// If create is true, LineNo2RangesParaMap will be created if not found
   /// </summary>
   /// <param name="sourceItem"> Identifier for LineNo2RangesParaMap</param>
   /// <param name="create"> If create is true, LineNo2RangesParaMap will be created if not found</param>
   /// <returns> Returns existing LineNo2RangesParaMap identified by sourceItem.
   /// New LineNo2RangesParaMap will be created, if not found and create is true.
   /// Otherwise null returned, if LineNo2RangesParaMap is not found and create is false
   /// </returns>
   public LineNo2RangesParaMap GetLineNo2RangesParaMap (Item.SourceItem sourceItem, bool create = false) {
      LineNo2RangesParaMap lineNo2RangesParaMap;
      if (this.source2RangesParaMap.TryGetValue (sourceItem, out lineNo2RangesParaMap) == false)
         this.source2RangesParaMap[sourceItem] = lineNo2RangesParaMap = new();

      return lineNo2RangesParaMap;
   }

   /// <summary>
   /// Returns RangesParagraph from lineNo2RangesParaMap identified by lineNo.
   /// If create is true, RangesParagraph will be created if not in lineNo2RangesParaMap
   /// </summary>
   /// <param name="lineNo2RangesParaMap"> Map lineNo2RangesParaMap from RangesParagraph is retrived or added</param>
   /// <param name="lineNo"> Identifier for RangesParagraph in Map lineNo2RangesParaMap</param>
   /// <param name="create"> If create is true, RangesParagraph will be created if not in lineNo2RangesParaMap</param>
   /// <returns> Returns existing RangesParagraph identified by lineNo from lineNo2RangesParaMap.
   /// New RangesParagraph will be created, if not founded in lineNo2RangesParaMap and create is true.
   /// Otherwise null returned, if RangesParagraph is not found and create is false
   /// </returns>
   public RangesParagraph GetRangesPara (LineNo2RangesParaMap lineNo2RangesParaMap,
                                         int lineNo, bool create = false) {
      RangesParagraph rangesPara = null;
      if (lineNo2RangesParaMap.TryGetValue(lineNo, out rangesPara) == false)
         lineNo2RangesParaMap[lineNo] = rangesPara = rangesPara = new();

      return rangesPara;
   }
   #endregion Methods

   #region Implementation -----------------------------------------------------
   /// <summary> Connects Source Item Selected Event and add RangeItems to Source Item map</summary>
   /// <param name="parentItem">The parent BaseItem from SourceItem is accessed </param>
   void connectSourceItemEvent (Item.BaseItem parentItem) {
      Item.SourceItem sourceItem;
      LineNo2RangesParaMap lineNo2RangesParaMap;
      foreach (Item.BaseItem item in parentItem.Items) {
         if ((sourceItem = item as Item.SourceItem) != null) {
            // Set SourceItem Selected Event Handler ----------------
            sourceItem.Selected += (sender, e) => {               
               Item.SourceItem sourceItem = sender as Item.SourceItem;
               this.sourceViewerHandler.LoadSourceFile (sourceItem);
            };

            lineNo2RangesParaMap = null;
            foreach (var rangeItem in sourceItem.RangeItems) {
               for (int lineNo = rangeItem.StartLine; lineNo <= rangeItem.EndLine; lineNo++) {
                  if(lineNo2RangesParaMap == null)
                     lineNo2RangesParaMap = this.GetLineNo2RangesParaMap (sourceItem, true);

                  RangesParagraph rangesPara = this.GetRangesPara (lineNo2RangesParaMap, lineNo, true);
                  rangesPara.rangeItems.Add (rangeItem);
               }
            }
         } else
            this.connectSourceItemEvent (item);
      }
   }
   #endregion Implementation 

   // Private Data ------------------------------------------------------------
   TreeView treeView = null;
   SourceViewerHandler sourceViewerHandler = null;
   Dictionary<Item.SourceItem, LineNo2RangesParaMap> source2RangesParaMap = new ();
}
#endregion TreeViewHandler class --------------------------------------------------------

#region SourceViewerHandler class -------------------------------------------------------
/// <summary> An UI handler class for SourceViewer</summary>
class SourceViewerHandler {
   #region Methods ------------------------------------------------------------
   /// <summary> Initializes SourceViewerHandler</summary>
   /// <param name="sourceViewer"> Specifies FlowDocumentScrollViewer ui object to load Source Item file</param>
   /// <param name="sourceCoverageStatusLb"> Specifies Label ui object to show Source Coverage</param>
   /// <param name="treeViewHandler"> Specifies TreeViewHandler object</param>
   /// <param name="mainWindow"> Specifies Application MainWindow object</param>
   public void Init (FlowDocumentScrollViewer sourceViewer,
                     Label sourceCoverageStatusLb,
                     TreeViewHandler treeViewHandler,
                     Window window) {
      this.sourceViewer = sourceViewer;
      this.treeViewHandler = treeViewHandler;
      this.sourceCoverageStatusLb = sourceCoverageStatusLb;
      this.window = window;

      this.sourceViewer.Document = new FlowDocument() {
         Background = Source.BackgroundColor,
         FontFamily = new FontFamily(Source.FontFamily),
         LineHeight = double.NaN
      };
   }   

   /// <summary>
   /// File Open function is to select a Code Coverage report file and 
   /// loads report informations
   /// </summary>
   public void FileOpen () {
      OpenFileDialog dialog = new OpenFileDialog() {
         Filter = "Code Coverage Reports|*.xml" };
      if (dialog.ShowDialog () == true)
         this.openCoverageFile (dialog.FileName);
   }

   /// <summary> Recomputes / loads Coverage file again</summary>
   public void Recompute () { 
      if (this.coverageFilename != null)
         this.openCoverageFile (this.coverageFilename);
   }

   /// <summary> Return true if Coverage File already loaded and can Recompute / load Coverage file again</summary>
   public bool CanRecompute () => this.coverageFilename != null;

   /// <summary> Loads a file (specified by filenName) into Source View and scroll to Line if lineNumber is given(not null)</summary>
   /// <param name="sourceItem"> Specifies the sourceItem whose file name to load</param>
   public void LoadSourceFile (Item.SourceItem sourceItem) {
      // Already loaded?
      string oldFileName = this.sourceViewer.Tag as string;
      if (oldFileName != sourceItem.FileName) {
         // Load source file
         try {
            if (this.readSourceFileLines (sourceItem))
               this.sourceViewer.Tag = sourceItem.FileName;
         } catch (Exception ex) {
            MessageBox.Show ($"Loading source file failed: {ex.Message}", "Load Source Error",
                             MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
   }
   #endregion Methods

   #region Implementation ----------------------------------------------------- 
   /// <summary> Opens a Coverage Xml file</summary>
   /// <param name="coverageFilename "> Specifies the Xml coverage filename</param>
   void openCoverageFile (string coverageFilename) {
      this.coverageFilename = coverageFilename;

      // Loads Code Coveage informations
      Reader reader = new ();
      Item.RootItem rootItem = reader.Read (this.coverageFilename);
      if (rootItem == null) {
         MessageBox.Show (reader.Error, "Code Coverage Report File Read Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
         return;
      }

      // Set coverage items to TreeView
      this.treeViewHandler.SetCoverage (rootItem);
      this.sourceViewer.Tag = null;
   }
   
   /// <summary> Split line and add highlight RangeItem Text Block to inLines</summary>
   /// <param name="inLines">Paragraph inlines property to which 
   /// xml node range text are created as Blocks</param>
   /// <param name="line"> Source Line text</param>
   /// <param name="lineNo"> Line in source </param>
   /// <param name="lineNo2RangesParaMap"> Source LineNo to Ranges Paragraph Map</param>
   void highLightRange (InlineCollection inLines,
                        string line, int lineNo,
                        LineNo2RangesParaMap lineNo2RangesParaMap) {
      RangesParagraph rangesPara = this.treeViewHandler.GetRangesPara (lineNo2RangesParaMap, lineNo);
      // No range item for this line
      if (rangesPara == null || rangesPara.rangeItems.Count == 0) {
         inLines.Add (new Run (string.Format (Source.LineNoFormat, lineNo)) {
            Foreground = Source.LineNoBrush
         });

         inLines.Add (line);
         return;
      }

      bool addLineNo = true;
      string rangeText = null;
      int length, lineLength = line.Length;
      int startColumn, endColumn, lineStartColumn = 0;

      if (false == rangesPara.ordered) {
         rangesPara.rangeItems = rangesPara.rangeItems.OrderBy (range => range.StartColumn).ToList ();
         rangesPara.ordered = true;
      }

      foreach (Item.RangeItem rangeItem in rangesPara.rangeItems) {
         // Prepend Line No
         if (addLineNo) {
            inLines.Add (new Run(string.Format(Source.LineNoFormat, lineNo)) {
               Foreground = Source.LineNoBrush });
            addLineNo = false;
         }

         int _startColumn = rangeItem.StartColumn - 1;
         int _endColumn = rangeItem.EndColumn - 2;

         #region Range Part
         if (rangeItem.StartLine == rangeItem.EndLine) {    // One line
            startColumn = _startColumn;
            endColumn = _endColumn;
         } else if (lineNo == rangeItem.StartLine) {        // Start line
            startColumn = _startColumn;
            endColumn = lineLength - 1;
         } else if (lineNo == rangeItem.EndLine) {          // End line
            startColumn = 0;
            endColumn = _endColumn;
         } else {                                           // Middle line
            startColumn = 0;
            endColumn = lineLength - 1;
         }

         // Left part before Range Part?
         if (lineStartColumn < startColumn) {
            rangeText = line.Substring (lineStartColumn, startColumn - lineStartColumn);
            inLines.Add (rangeText);
         }

         // Range part
         length = endColumn - startColumn + 1;
         rangeText = line.Substring (startColumn, length);

         var run = new Run(rangeText) {
            Background = rangeItem.covered ? Source.CoveredBackgroundColor
                                           : Source.NotCoveredBackgroundColor
         };

         run.PreviewMouseUp += Run_PreviewMouseLeftButtonUp;
         run.Tag = rangeItem;
         inLines.Add (run);         
         #endregion Range Part 

         lineStartColumn = endColumn + 1; // Range endColumn is lineStartColumn for next Range
      }

      // Right part?
      if (lineStartColumn < lineLength) {                   
         rangeText = line.Substring (lineStartColumn, lineLength - lineStartColumn);
         inLines.Add (rangeText);
      }
   }

   private void Run_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      Run run = (Run)sender;
      Item.RangeItem rangeItem = (Item.RangeItem)run.Tag;
      rangeItem.SourceItem.IsSelected = true;
      e.Handled = true;
   }

   /// <summary> Reads source file and creates as paragraphs </summary>
   /// <param name="flowDocument"> FlowDocument object to add paragraphs(each line in source file)</param>
   /// <param name="sourceItem"> Specifies the sourceItem whose file name to load</param>
   /// <returns> Returns true if successfully read source file otherwise false</returns>
   private bool readSourceFileLines (Item.SourceItem sourceItem) {
      FlowDocument flowDocument = this.sourceViewer.Document;
      flowDocument.Blocks.Clear ();

      // Set SourceItem Block Coverage in Status
      this.sourceCoverageStatusLb.Content = sourceItem.BlockCoverage;

      // Set ModuleItem Block Coverage in Title
      Item.ModuleItem moduleItem = sourceItem.ModuleItem;
      this.window.Title = $"{sourceItem.NameSpace}: {moduleItem.BlockCoverage}";

      bool success = true;
      LineNo2RangesParaMap lineNo2RangesParaMap = null;
      try {
         using (StreamReader stream = File.OpenText (sourceItem.FileName)) {
            string line;
            int lineNo = 1;
            while ((line = stream.ReadLine ()) != null) {
               Paragraph paragraph = new Paragraph() {
                  Margin = new Thickness(0),
                  FontSize = Source.FontSize,
                  FontFamily = new FontFamily(Source.FontFamily),
                  Tag = lineNo
               };

               if(lineNo2RangesParaMap == null)
                  lineNo2RangesParaMap = this.treeViewHandler.GetLineNo2RangesParaMap (sourceItem, true);

               this.highLightRange (paragraph.Inlines, line, lineNo, lineNo2RangesParaMap);

               RangesParagraph rangesPara = this.treeViewHandler.GetRangesPara (lineNo2RangesParaMap, lineNo, true);
               rangesPara.paragraph = paragraph;

               flowDocument.Blocks.Add (paragraph);
               lineNo++;
            }
         }
      } catch (Exception ex) {
         success = false;
         MessageBox.Show ($"Loading source file failed: {ex.Message}", "Load Source Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
      }

      return success;
   }
   #endregion Implementation

   // Private Data -------------------------------------------------------------
   TreeViewHandler treeViewHandler = null;
   FlowDocumentScrollViewer sourceViewer = null;
   Label sourceCoverageStatusLb = null;
   Window window = null;

   string coverageFilename = null;

   // Nested class ------------------------------------------------------------
   // class to Definitions Soruce Styles
   class Source {
      internal static readonly double FontSize = 12;
      internal static readonly string FontFamily = "CONSOLAS";
      internal static readonly Brush LineNoBrush = Brushes.Gray;
      internal static readonly SolidColorBrush BackgroundColor = Brushes.Gainsboro;

      internal static readonly SolidColorBrush CoveredBackgroundColor = Brushes.DeepSkyBlue;
      internal static readonly SolidColorBrush NotCoveredBackgroundColor = Brushes.DarkOrange;

      internal static readonly string LineNoFormat = $"{{0,5}}: ";
   }
}
#endregion SourceViewerHandler class ----------------------------------------------------

#region Private classes -----------------------------------------------------------------
/// <summary>Class to hold RangeItem(s) and Paragraph</summary>
class RangesParagraph {
   internal bool ordered = false;
   internal List<Item.RangeItem> rangeItems = new ();

   internal Paragraph paragraph;
}
#endregion Private classes --------------------------------------------------------------
