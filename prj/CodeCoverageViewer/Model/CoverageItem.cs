using System.Windows.Controls;

namespace CodeCoverageViewer.Item;

#region class RootItem -------------------------------------------------------------
internal class RootItem {
   #region Data ---------------------------------------------------------------
   public List<BaseItem> driveItems = new ();
   #endregion Data ------------------------------------------------------------
}
#endregion class RootItem -----------------------------------------------------------

#region class BaseItem --------------------------------------------------------------
internal class BaseItem : TreeViewItem {
}
#endregion class BaseItem -----------------------------------------------------------

#region class ModuleItem -----------------------------------------------------------
class ModuleItem  {
   #region Data ---------------------------------------------------------------
   public string blockCoverage;
   #endregion Data ------------------------------------------------------------
}
#endregion class ModuleItem --------------------------------------------------------

#region class SourceItem -----------------------------------------------------------
class SourceItem : BaseItem {
   #region Data ---------------------------------------------------------------
   public ModuleItem moduleItem = null;

   public string fileName;
   public string nameSpace;
   public List<RangeItem> rangeItems = new ();

   public string blockCoverage;
   #endregion Data ------------------------------------------------------------
}
#endregion class SourceItem --------------------------------------------------------

#region class RangeItem -------------------------------------------------------------
class RangeItem {
   #region Data ---------------------------------------------------------------
   public int startLine;
   public int startColumn;
   public int endLine;
   public int endColumn;
   public bool covered;
   #endregion Data ------------------------------------------------------------
}
#endregion class RangeItem ---------------------------------------------------------

