using System.Windows.Controls;

namespace CodeCoverageViewer.Item;

// RootItem class -----------------------------------------------------------------------
internal class RootItem {
   public List<BaseItem> driveItems = new ();   
}

// BaseItem class -----------------------------------------------------------------------
internal class BaseItem : TreeViewItem {}

// ModuleItem class ---------------------------------------------------------------------
class ModuleItem  {
   public string blockCoverage;   
}

// SourceItem class ---------------------------------------------------------------------
class SourceItem : BaseItem {
   public ModuleItem moduleItem = null;

   public string fileName;
   public string nameSpace;
   public List<RangeItem> rangeItems = new ();

   public string blockCoverage;
}

// RangeItem class ----------------------------------------------------------------------
class RangeItem {
   public int startLine;
   public int startColumn;
   public int endLine;
   public int endColumn;
   public bool covered;
}

