using System.Windows.Controls;

namespace CodeCoverageViewer.Item;

// region RootItem class ----------------------------------------------------------------
internal class RootItem {
   public List<BaseItem> driveItems = new ();   
}

// region BaseItem class ----------------------------------------------------------------
internal class BaseItem : TreeViewItem {}

// region ModuleItem class --------------------------------------------------------------
class ModuleItem  {
   public string blockCoverage;   
}

// region SourceItem class --------------------------------------------------------------
class SourceItem : BaseItem {
   public ModuleItem moduleItem = null;

   public string fileName;
   public string nameSpace;
   public List<RangeItem> rangeItems = new ();

   public string blockCoverage;
}

// region RangeItem class ---------------------------------------------------------------
class RangeItem {
   public int startLine;
   public int startColumn;
   public int endLine;
   public int endColumn;
   public bool covered;
}

