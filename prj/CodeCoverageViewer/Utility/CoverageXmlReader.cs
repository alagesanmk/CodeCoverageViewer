using CodeCoverageViewer.Item;
using System;
using System.Windows.Controls;
using System.Xml;

namespace CodeCoverageViewer.Utility;

#region class Reader --------------------------------------------------------------
/// <summary>
/// Utility class to read Code Covarge Report file in Xml format
/// </summary>
internal class Reader {
   #region Properties ---------------------------------------------------------
   public string Error { get; set; } = "";
   #endregion Properties ------------------------------------------------------

   #region Methods ------------------------------------------------------------
   /// <summary>
   /// Reads Code coverate xml file and adds Coverage Drive Item and descendants Folder, Source
   /// </summary>
   /// <param name="fileName">Specifies the filename of Code Coverage xml file</param>
   /// <returns>Returns new Coverage root item which has descendants Drive, Folder and Source,  
   ///          Otherwise null if read file fails
   /// </returns>
   public RootItem Read (string fileName) {
      this.Error = ""; // No error

      RootItem rootItem = new ();
      KeyItemMap driveMap = new ();
      KeyItemMap folderMap = new ();
      XmlDocument xmlDoc = new XmlDocument ();
      try {
         xmlDoc.Load (fileName);
         XmlNode docElement = xmlDoc.DocumentElement;

         // Reads "module" nodes
         foreach (XmlNode childsNode in docElement.ChildNodes) {
            if ("modules" == childsNode.Name) {
               // Reads source nodes and adds to sources
               foreach (XmlNode moduleNode in childsNode.ChildNodes) {
                  if ("module" == moduleNode.Name)
                     parseModuleNode (rootItem, driveMap, folderMap, moduleNode);
               }
            }
         }
      } catch (Exception ex) {
         // Some read error or invalid xml format
         this.Error = $"Parsing '{fileName}' failed: {ex.Message}";
         rootItem = null;
      }

      return rootItem;
   }
   #endregion Methods --------------------------------------------------------

   #region Implementation ----------------------------------------------------
   /// <summary>
   /// Reads ModuleNode node attributes and descendants source_file/range nodes
   /// </summary>
   /// <param name="rootItem">Root item to add Drive and descendants</param>
   /// <param name="driveMap">Drive Letter to Drive Item map</param>
   /// <param name="folderMap">Folder Item map</param>
   /// <param name="moduleNode">Module xml node to read sourc_file and range xml node</param>
   void parseModuleNode (RootItem rootItem,
                         KeyItemMap driveMap, KeyItemMap folderMap,
                         XmlNode moduleNode) {
      KeyItemMap sourceIdMap = new ();
      this.parseSourceFilesNode (rootItem, driveMap, folderMap, sourceIdMap, moduleNode);

      // Get all range(s) Node -------------------------------------------------
      XmlNodeList sourceFileNodes = moduleNode.SelectNodes ("descendant::range");
      foreach (XmlNode rangeNode in sourceFileNodes) {
         // Read Attributes ---------------------------------------
         var attributes = rangeNode.Attributes;
         string rangeId = attributes["source_id"]?.Value;

         SourceItem sourceItem = sourceIdMap[rangeId] as SourceItem;
         RangeItem rangeItem = new RangeItem () {
            startLine = int.Parse (attributes["start_line"]?.Value),
            startColumn = int.Parse (attributes["start_column"]?.Value),
            endLine = int.Parse (attributes["end_line"]?.Value),
            endColumn = int.Parse (attributes["end_column"]?.Value),
            covered = "yes" == attributes["covered"]?.Value.ToLower (),
         };

         sourceItem.rangeItems.Add (rangeItem);
      }
   }

   /// <summary>
   /// Reads SourceFileNode node attributes and split path to drive, folder and source file
   /// </summary>
   /// <param name="rootItem">Root item to add Drive and descendants</param>
   /// <param name="driveMap">Drive Letter to Drive Item map</param>
   /// <param name="folderMap">Folder Item map</param>
   /// <param name="sourceIdMap">Source id to Source Item map</param>
   /// <param name="moduleNode">Module xml node to read sourc_file and range xml node</param>
   void parseSourceFilesNode (RootItem rootItem,
                              KeyItemMap driveMap, KeyItemMap folderMap,
                              KeyItemMap sourceIdMap, XmlNode moduleNode) {
      XmlNodeList sourceFileNodes = moduleNode.SelectNodes ("descendant::source_file");
      foreach (XmlNode sourceFileNode in sourceFileNodes) {
         // Read Attributes
         var attributes = sourceFileNode.Attributes;
         string sourceFileId = attributes["id"]?.Value;
         string sourceFilePath = attributes["path"]?.Value;

         this.splitSourceFileNode (rootItem, driveMap, folderMap, sourceIdMap, sourceFileId, sourceFilePath);
      }
   }

   /// <summary>
   /// Splits path name to Drive, Folder and Source and adds to rootItem
   /// </summary>
   /// <param name="rootItem">Root item to add Drive, Folder and Source</param>
   /// <param name="driveMap">Drive Letter to Drive Item map</param>
   /// <param name="folderMap">Folder Item map</param>
   /// <param name="sourceIdMap">Source id to Source Item map</param>
   /// <param name="sourceFileId">Specifies the Source Item id</param>
   /// <param name="sourceFilePath">Specifies the Source Item path</param>
   void splitSourceFileNode (RootItem rootItem, KeyItemMap driveMap, KeyItemMap folderMap,
                             KeyItemMap sourceIdMap, string sourceFileId, string sourceFilePath) {
      string key = "";
      BaseItem parentItem = null;
      string[] tokens = sourceFilePath.Split ('\\');
      int t = 0, tokenCount = tokens.Length;
      foreach (string token in tokens) {
         t++;
         if (null == parentItem) {  // First is Drive item!!
            if (tokens.Length < 1)
               break; // Insufficient tokens

            key = token;
            parentItem = this.createOrGetItem (rootItem.driveItems, driveMap, token, key);
            continue;
         }

         if (t == tokenCount) {     // Last is Source item!!
            SourceItem sourceItem = new () {
               Header = token,
               fileName = sourceFilePath,
            };

            parentItem.Items.Add (sourceItem);
            sourceIdMap[sourceFileId] = sourceItem;
         } else {                      // Folder Item!!
            key += "/" + token;
            parentItem = this.createOrGetItem (parentItem.Items, folderMap, token, key);
         }
      }
   }
   /// <summary>
   /// Return a BaseItem if available in itemMap identified by key or creates new BaseItem
   /// </summary>
   /// <param name="items">New BaseItem is added to items</param>
   /// <param name="itemMap">The item map from which to get exist or add new BaseItem</param>
   /// <param name="name">Name for the new BaseItem</param>
   /// <param name="key">Key value to identify the exiting Base item</param>
   /// <returns>Created and existing Base item</returns>
   BaseItem createOrGetItem (dynamic items, KeyItemMap itemMap,
                             string name, string key) {

      BaseItem item = null;
      if (itemMap.ContainsKey (key))
         item = itemMap[key];
      else {
         item = new () {
            Header = name
         };

         itemMap[key] = item;
         items.Add (item);
      }

      return item;
   }
   #endregion Implementation --------------------------------------------------

   #region Nested class -------------------------------------------------------
   class KeyItemMap : Dictionary<string, BaseItem> { }
   #endregion Nested class ----------------------------------------------------
}
#endregion class Reader -------------------------------------------------------------