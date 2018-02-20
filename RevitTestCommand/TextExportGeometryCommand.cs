using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitGeometryExporter;

namespace RevitTestCommand
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TextExportGeometryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection selection = commandData.Application.ActiveUIDocument.Selection;
            try
            {
                // setup export folder
                ExportGeometryToXml.FolderName = @"C:\Temp";
                // select walls
                IList<Reference> selectionResult = selection.PickObjects(ObjectType.Element, new WallSelectionFilter(),
                    "Select walls:");
                if (selectionResult.Any())
                {
                    List<Wall> wallsToExport = new List<Wall>();
                    foreach (Reference reference in selectionResult)
                    {
                        Wall wall = (Wall)doc.GetElement(reference);
                        wallsToExport.Add(wall);
                    }
                    ExportGeometryToXml.ExportWallsByFaces(wallsToExport, "walls");
                }
                // families
                selectionResult = selection.PickObjects(ObjectType.Element, "Select families:");
                if (selectionResult.Any())
                {
                    List<FamilyInstance> familyInstances = new List<FamilyInstance>();
                    foreach (Reference reference in selectionResult)
                    {
                        Element el = doc.GetElement(reference);
                        if(el is FamilyInstance familyInstance)
                            familyInstances.Add(familyInstance);
                    }
                    ExportGeometryToXml.ExportFamilyInstancesByFaces(familyInstances, "families", false);
                }
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception exception)
            {
                message += exception.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

    internal class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Wall;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
