using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace RevitGeometryExporter
{
    /// <summary>Экспорт геометрии в xml для последующей отрисовки в AutoCAD</summary>
    public static class ExportGeometryToXml
    {
        public static string FolderName = @"C:\Temp\RevitExportXml";

        #region Elements

        public static void ExportWallsByFaces(List<Wall> walls, string header)
        {
            Options options = new Options();
            List<Curve> curves = new List<Curve>();
            foreach (Wall wall in walls)
            {
                IEnumerable<GeometryObject> geometry = wall.get_Geometry(options);
                foreach (GeometryObject geometryObject in geometry)
                {
                    if (geometryObject is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            foreach (EdgeArray edgeArray in face.EdgeLoops)
                            {
                                foreach (Edge edge in edgeArray)
                                {
                                    curves.Add(edge.AsCurve());
                                }
                            }
                        }
                    }
                }
            }
            ExportCurves(curves, header);
        }

        #endregion

        #region Geometry objects
        public static void ExportSolidsByFaces(List<Solid> solids, string header)
        {
            List<Face> faces = new List<Face>();
            foreach (Solid solid in solids)
            {
                foreach (Face solidFace in solid.Faces)
                {
                    faces.Add(solidFace);
                }
            }
            if (faces.Any())
                ExportFaces(faces, header);
        }
        public static void ExportSolidByFaces(Solid solid, string header)
        {
            List<Face> faces = new List<Face>();
            foreach (Face solidFace in solid.Faces)
            {
                faces.Add(solidFace);
            }
            if (faces.Any())
                ExportFaces(faces, header);
        }
        public static void ExportFaces(List<Face> faces, string header)
        {
            List<Curve> wallCurves = new List<Curve>();

            foreach (Face face in faces)
            {
                EdgeArrayArray edgeArrayArray = face.EdgeLoops;
                foreach (EdgeArray edgeArray in edgeArrayArray)
                {
                    foreach (Edge edge in edgeArray)
                    {
                        wallCurves.Add(edge.AsCurve());
                    }
                }
            }
            ExportCurves(wallCurves, header);
        }
        public static void ExportCurves(List<Curve> curves, string header)
        {
            CheckFolder();
            XElement root = new XElement("Curves");
            XElement linesRootXElement = new XElement("Lines");
            XElement arcsRootXElement = new XElement("Arcs");
            foreach (Curve curve in curves)
            {
                Line line = curve as Line;
                if (line != null)
                {
                    var lineXel = GetGeometryFromObjects.GetXElementFromLine(line);
                    if (lineXel != null)
                        linesRootXElement.Add(lineXel);
                }
                Arc arc = curve as Arc;
                if (arc != null)
                {
                    var arcXel = GetGeometryFromObjects.GetXElementFromArc(arc);
                    if (arcXel != null)
                        arcsRootXElement.Add(arcXel);
                }
            }
            if (linesRootXElement.HasElements) root.Add(linesRootXElement);
            if (arcsRootXElement.HasElements) root.Add(arcsRootXElement);

            root.Save(Path.Combine(FolderName, GetFileName(header)));
        }
        public static void ExportLines(List<Line> lines, string header)
        {
            CheckFolder();
            XElement rootXElement = new XElement("Lines");
            foreach (Line line in lines)
            {
                rootXElement.Add(GetGeometryFromObjects.GetXElementFromLine(line));
            }
            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }
        public static void ExportArcs(List<Arc> arcs, string header)
        {
            CheckFolder();
            XElement rootXElement = new XElement("Arcs");
            foreach (Arc arc in arcs)
            {
                rootXElement.Add(GetGeometryFromObjects.GetXElementFromArc(arc));
            }
            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }
        public static void ExportPoints(List<XYZ> points, string header)
        {
            CheckFolder();
            XElement rootXElement = new XElement("Points");
            foreach (XYZ point in points)
            {
                rootXElement.Add(GetGeometryFromObjects.GetXElementFromPoint(point));
            }
            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }
        #endregion

        #region Helpers

        private static void CheckFolder()
        {
            if (!Directory.Exists(FolderName))
                Directory.CreateDirectory(FolderName);
        }

        private static string GetFileName(string header)
        {
            return DateTime.Now.Minute + "_" + DateTime.Now.Second + "_" + DateTime.Now.Millisecond + "_" + header +
                   ".xml";
        }

        #endregion
    }
}
