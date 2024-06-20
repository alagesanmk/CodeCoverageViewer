using System.Windows.Controls;

namespace CodeCoverageViewer.Item;

// RootItem class -----------------------------------------------------------------------
internal class RootItem {
   public List<BaseItem> DriveItems = new ();   
}

// BaseItem class -----------------------------------------------------------------------
internal class BaseItem : TreeViewItem {}

// ModuleItem class ---------------------------------------------------------------------
class ModuleItem  {
   public string BlockCoverage;   
}

// SourceItem class ---------------------------------------------------------------------
class SourceItem : BaseItem {
   public ModuleItem ModuleItem = null;

   public string FileName;
   public string NameSpace;
   public List<RangeItem> RangeItems = new ();

   public string BlockCoverage;
}

// RangeItem class ----------------------------------------------------------------------
class RangeItem {
   public int StartLine;
   public int StartColumn;
   public int EndLine;
   public int EndColumn;
   public bool covered;
}

