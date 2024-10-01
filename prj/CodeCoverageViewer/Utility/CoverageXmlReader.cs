using CodeCoverageViewer.Item;
using System.Xml;

namespace CodeCoverageViewer.Utility;

using KeyItemMap = Dictionary<string, BaseItem>;
using ModuleMap = Dictionary<string, ModuleItem>;

#region Reader class --------------------------------------------------------------------
/// <summary> Utility class to read Code Covarge Report file in Xml format</summary>
internal class Reader {
   // Properties --------------------------------------------------------------
   public string Error { get; set; } = "";
   
   #region Methods ------------------------------------------------------------
   /// <summary> Reads Code coverate xml file and adds Coverage Drive Item and descendants Folder, Source</summary>
   /// <param name="fileName"> Specifies the filename of Code Coverage xml file</param>
   /// <returns> Returns new Coverage root item which has descendants Drive, Folder and Source,  
   ///          Otherwise null if read file fails
   /// </returns>
   public RootItem Read (string fileName) {
      this.Error = ""; // No error

      RootItem rootItem = new ();
      ModuleMap moduleMap = new ();
      KeyItemMap driveMap = new ();
      KeyItemMap folderMap = new ();
      
      try {
         XmlDocument xmlDoc = new();
         xmlDoc.Load (fileName);

         double blockCoverage;
         int blocksCovered, notBlocksCovered;
         string moduleId, bc, bsc, nbsc;

         ModuleItem moduleItem;
         XmlNode docElement = xmlDoc.DocumentElement;
         foreach(XmlNode modulesNode in docElement.SelectNodes("descendant::modules")) {
            foreach (XmlNode moduleNode in modulesNode.SelectNodes("descendant::module")) {  
               var attributes = moduleNode.Attributes;
               moduleId = attributes["id"]?.Value;
               bc = attributes["block_coverage"]?.Value;
               blockCoverage = double.Parse (bc);

               bsc = attributes["blocks_covered"]?.Value;
               blocksCovered = int.Parse (bsc);

               nbsc = attributes["blocks_covered"]?.Value;
               notBlocksCovered = int.Parse(nbsc);

               moduleItem = this.createOrGetModuleItem(moduleMap, moduleId);
               moduleItem.BlockCoverage = $"{blocksCovered} / {notBlocksCovered + blocksCovered} : {blockCoverage}%";

               this.parseModuleNode (rootItem, driveMap, folderMap, moduleNode, moduleItem);
            }
         }
      } catch (Exception ex) {
         // Some read error or invalid xml format
         this.Error = $"Parsing '{fileName}' failed: {ex.Message}";
         rootItem = null;
      }

      return rootItem;
   }
   #endregion Methods 

   #region Implementation -----------------------------------------------------
   /// <summary> Reads ModuleNode node attributes and descendants source_file/range nodes</summary>
   /// <param name="rootItem"> Root item to add Drive and descendants</param>
   /// <param name="driveMap"> Drive Letter to Drive Item map</param>
   /// <param name="folderMap"> Folder Item map</param>
   /// <param name="moduleNode"> Module xml node to read sourc_file and range xml node</param>
   /// <param name="moduleItem"> Module Item to update block Coverage and 
   /// refereence to SourceItem
   /// </param>
   void parseModuleNode (RootItem rootItem,
                         KeyItemMap driveMap, KeyItemMap folderMap,
                         XmlNode moduleNode, ModuleItem moduleItem) {
      KeyItemMap sourceIdMap = new ();
      this.parseSourceFilesNode (rootItem, driveMap, folderMap, sourceIdMap, moduleNode, moduleItem);
     
      // Get all range(s) Node -------------------------------------------------
      foreach (XmlNode rangeNode in moduleNode.SelectNodes("descendant::range")) {
         // Read Attributes ---------------------------------------
         var attributes = rangeNode.Attributes;
         string rangeId = attributes["source_id"]?.Value;

         SourceItem sourceItem = sourceIdMap[rangeId] as SourceItem;
         RangeItem rangeItem = new () {
            SourceItem = sourceItem,
            StartLine = int.Parse (attributes["start_line"]?.Value),
            StartColumn = int.Parse (attributes["start_column"]?.Value),
            EndLine = int.Parse (attributes["end_line"]?.Value),
            EndColumn = int.Parse (attributes["end_column"]?.Value),
            covered = "yes" == attributes["covered"]?.Value.ToLower (),
         };

         sourceItem.RangeItems.Add (rangeItem);
         if (sourceItem.RangeItems.Count == 1) {   // First rangeItem?
            XmlNode functionNode = rangeNode.ParentNode.ParentNode;
            sourceItem.NameSpace = functionNode.Attributes["namespace"]?.Value ?? "None";
         }
      }
   }

   /// <summary> Reads SourceFileNode node attributes and split path to drive, folder and source file</summary>
   /// <param name="rootItem"> Root item to add Drive and descendants</param>
   /// <param name="driveMap"> Drive Letter to Drive Item map</param>
   /// <param name="folderMap"> Folder Item map</param>
   /// <param name="sourceIdMap"> Source id to Source Item map</param>
   /// <param name="moduleNode"> Module xml node to read sourc_file and range xml node</param>
   void parseSourceFilesNode (RootItem rootItem,
                              KeyItemMap driveMap, KeyItemMap folderMap,
                              KeyItemMap sourceIdMap, 
                              XmlNode moduleNode, ModuleItem moduleItem) {
      SourceItem sourceItem;
      XmlNodeList sourceFileNodes = moduleNode.SelectNodes ("descendant::source_file");
      foreach (XmlNode sourceFileNode in sourceFileNodes) {
         // Read Attributes
         var attributes = sourceFileNode.Attributes;
         string sourceFileId = attributes["id"]?.Value;
         string sourceFilePath = attributes["path"]?.Value;

         if((sourceItem = this.splitSourceFileNode (rootItem, driveMap, folderMap, 
                                                    sourceIdMap, sourceFileId, sourceFilePath,
                                                    moduleItem)) != null)
            this.updateSourceItemCoverge (sourceItem, sourceFileId, moduleNode, moduleItem);
      }
   }

   /// <summary> Update Source item block coverage values and percentage</summary>
   /// <param name="sourceItem"> SourceItem whose block coverage to be need updated</param>
   /// <param name="sourceId"> The value to identifiy the range xml nodes' with source_id  /param>
   /// <param name="moduleNode"> Module xml node to read range xml node</param>
   /// <param name="moduleItem"> Module Item to update block Coverage and 
   void updateSourceItemCoverge (SourceItem sourceItem, string sourceId, 
                                 XmlNode moduleNode, ModuleItem moduleItem) {
      string cb, ncb, bc;
      int coveredBlock, notCoveredBlock;
      int blocks = 0, coveredBlocks = 0;

      XmlNodeList functionNodes = moduleNode.SelectNodes (
                  $"descendant::functions/function[ranges/range/@source_id='{sourceId}']");
      foreach (XmlNode functionNode in functionNodes) {
         cb = functionNode.Attributes["blocks_covered"]?.Value;
         coveredBlock = int.Parse (cb);
         coveredBlocks += coveredBlock;

         ncb = functionNode.Attributes["blocks_not_covered"]?.Value;
         notCoveredBlock = int.Parse (cb);
         blocks = coveredBlock + notCoveredBlock;

         bc = functionNode.Attributes["block_coverage"]?.Value;
      }

      double coveredBlockPercent = (double)coveredBlocks / blocks;
      sourceItem.BlockCoverage = $"{coveredBlocks} / {blocks} : {coveredBlockPercent}%";
   }

   /// <summary> Splits path name to Drive, Folder and Source and adds to rootItem</summary>
   /// <param name="rootItem"> Root item to add Drive, Folder and Source</param>
   /// <param name="driveMap"> Drive Letter to Drive Item map</param>
   /// <param name="folderMap"> Folder Item map</param>
   /// <param name="sourceIdMap"> Source id to Source Item map</param>
   /// <param name="sourceFileId"> Specifies the Source Item id</param>
   /// <param name="sourceFilePath"> Specifies the Source Item path</param>
   /// <returns> Returns new SourceItem with data from sourceFilePath</returns>
   SourceItem splitSourceFileNode (RootItem rootItem, KeyItemMap driveMap, KeyItemMap folderMap,
                                   KeyItemMap sourceIdMap, string sourceFileId, string sourceFilePath,
                                   ModuleItem moduleItem) {
      SourceItem sourceItem = null;

      do {
         string key = "";
         BaseItem parentItem = null;
         string[] tokens = sourceFilePath.Split ('\\');
         if (tokens.Length <= 0)
            break;                     // Insufficient tokens

         int t = 0;
         foreach (string token in tokens) {
            t++;
            if (parentItem == null) {  // First is Drive item!!
               key = token;
               parentItem = this.createOrGetItem (rootItem.DriveItems, driveMap, token, key);
               continue;
            } else if (t == tokens.Length) {  // Last is Source item!!
               parentItem.Items.Add(sourceIdMap[sourceFileId] = sourceItem = new SourceItem() {
                  Header = token,
                  FileName = sourceFilePath,
                  ModuleItem = moduleItem,
               });
            } else {                   // Folder Item!!
               key += "/" + token;
               parentItem = this.createOrGetItem (parentItem.Items, folderMap, token, key);
            }
         }
      } while (false);

      return sourceItem;
   }

   /// <summary> Return a BaseItem if available in itemMap identified by key or creates new BaseItem</summary>
   /// <param name="items"> New BaseItem is added to items</param>
   /// <param name="itemMap"> The item map from which to get exist or add new BaseItem</param>
   /// <param name="name"> Name for the new BaseItem</param>
   /// <param name="key"> Key value to identify the exiting Base item</param>
   /// <returns> Created and existing Base item</returns>
   BaseItem createOrGetItem (dynamic items, KeyItemMap itemMap,
                             string name, string key) {
      BaseItem item = null;
      if (itemMap.TryGetValue (key, out item) == false) {
         itemMap[key] = item = new () {
            Header = name
         };

         items.Add (item);
      }
           
      return item;
   }

   /// <summary>
   /// Return a ModuleITem if available in moduleMap identified by id 
   /// or creates new ModuleItem
   /// </summary>
   /// <param name="items"> New BaseItem is added to items</param>
   /// <param name="moduleMap"> The Module map from which to get exist 
   /// or add new ModuleItem</param>
   /// <param name="id"> Key value to identify the exiting ModuleItem</param>
   /// <returns> Created and existing ModuleItem</returns>
   ModuleItem createOrGetModuleItem (ModuleMap moduleMap, string id) {
      ModuleItem moduleItem = null;
      if(moduleMap.TryGetValue(id, out moduleItem) == false)
         moduleMap[id] = moduleItem = new();

      return moduleItem;
   }
   #endregion Implementation
}
#endregion Reader class -----------------------------------------------------------------