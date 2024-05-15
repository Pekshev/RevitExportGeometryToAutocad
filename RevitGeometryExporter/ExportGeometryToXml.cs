namespace RevitGeometryExporter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Экспорт геометрии в xml для последующей отрисовки в AutoCAD
    /// </summary>
    public static class ExportGeometryToXml
    {
        /// <summary>
        /// Full path to the folder where xml files will be saved. The default path is "C:\Temp\RevitExportXml"
        /// </summary>
        public static string FolderName = @"C:\Temp\RevitExportXml";

        /// <summary>
        /// Output units
        /// </summary>
        public static ExportUnits ExportUnits = ExportUnits.Ft;

        /// <summary>
        /// Clear <see cref="FolderName"/> (remove all files) if folder exists
        /// </summary>
        public static void ClearFolder()
        {
            if (Directory.Exists(FolderName))
            {
                foreach (var file in Directory.GetFiles(FolderName))
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="folderName">Full path to the folder where xml files will be saved. The default path is "C:\Temp\RevitExportXml"</param>
        [Conditional("DEBUG")]
        public static void Init(string folderName)
        {
            FolderName = folderName;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="folderName">Full path to the folder where xml files will be saved. The default path is "C:\Temp\RevitExportXml"</param>
        /// <param name="exportUnits">Output units</param>
        [Conditional("DEBUG")]
        public static void Init(string folderName, ExportUnits exportUnits)
        {
            FolderName = folderName;
            ExportUnits = exportUnits;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="folderName">Full path to the folder where xml files will be saved. The default path is "C:\Temp\RevitExportXml"</param>
        /// <param name="exportUnits">Output units</param>
        /// <param name="clearFolder">Clear <see cref="FolderName"/> (remove all files) if folder exists</param>
        [Conditional("DEBUG")]
        public static void Init(string folderName, ExportUnits exportUnits, bool clearFolder)
        {
            FolderName = folderName;
            ExportUnits = exportUnits;
            if (clearFolder)
                ClearFolder();
        }

        #region Elements

        [Conditional("DEBUG")]
        public static void ExportWallsByFaces(IEnumerable<Wall> walls, string header)
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
                                    curves.Add(edge.AsCurve());
                            }
                        }
                    }
                }
            }

            ExportCurves(curves, header);
        }

        [Conditional("DEBUG")]
        public static void ExportWallByFaces(Wall wall, string header)
        {
            Options options = new Options();
            List<Curve> curves = new List<Curve>();

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
                                curves.Add(edge.AsCurve());
                        }
                    }
                }
            }

            ExportCurves(curves, header);
        }

        [Conditional("DEBUG")]
        public static void ExportFamilyInstancesByFaces(
            IEnumerable<FamilyInstance> families, string header, bool includeNonVisibleObjects)
        {
            Options options = new Options
            {
                IncludeNonVisibleObjects = includeNonVisibleObjects
            };
            List<Curve> curves = new List<Curve>();
            foreach (FamilyInstance familyInstance in families)
            {
                curves.AddRange(GetCurvesFromFamilyGeometry(familyInstance, options));
            }

            ExportCurves(curves, header);
        }

        [Conditional("DEBUG")]
        public static void ExportFamilyInstanceByFaces(
            FamilyInstance familyInstance, string header, bool includeNonVisibleObjects)
        {
            Options options = new Options
            {
                IncludeNonVisibleObjects = includeNonVisibleObjects
            };
            List<Curve> curves = GetCurvesFromFamilyGeometry(familyInstance, options).ToList();
            
            ExportCurves(curves, header);
        }

        #endregion

        #region Geometry objects

        [Conditional("DEBUG")]
        public static void ExportSolidsByFaces(IEnumerable<Solid> solids, string header)
        {
            CreateFolder();
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

        [Conditional("DEBUG")]
        public static void ExportSolid(Solid solid, string header)
        {
            CreateFolder();
            List<Face> faces = new List<Face>();

            foreach (Face solidFace in solid.Faces)
            {
                faces.Add(solidFace);
            }

            if (faces.Any())
                ExportFaces(faces, header);
        }

        [Conditional("DEBUG")]
        public static void ExportFaces(IEnumerable<Face> faces, string header)
        {
            CreateFolder();
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

        [Conditional("DEBUG")]
        public static void ExportFace(Face face, string header)
        {
            CreateFolder();
            List<Curve> wallCurves = new List<Curve>();

            EdgeArrayArray edgeArrayArray = face.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeArrayArray)
            {
                foreach (Edge edge in edgeArray)
                {
                    wallCurves.Add(edge.AsCurve());
                }
            }

            ExportCurves(wallCurves, header);
        }

        [Conditional("DEBUG")]
        public static void ExportFaces(IEnumerable<PlanarFace> planarFaces, string header)
        {
            CreateFolder();
            List<Curve> wallCurves = new List<Curve>();

            foreach (PlanarFace planarFace in planarFaces)
            {
                EdgeArrayArray edgeArrayArray = planarFace.EdgeLoops;
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

        [Conditional("DEBUG")]
        public static void ExportCurves(IEnumerable<Curve> curves, string header)
        {
            CreateFolder();
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

            if (linesRootXElement.HasElements)
                root.Add(linesRootXElement);
            if (arcsRootXElement.HasElements)
                root.Add(arcsRootXElement);

            root.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        [Conditional("DEBUG")]
        public static void ExportCurve(Curve curve, string header)
        {
            CreateFolder();
            XElement root = new XElement("Curves");
            XElement linesRootXElement = new XElement("Lines");
            XElement arcsRootXElement = new XElement("Arcs");
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

            if (linesRootXElement.HasElements)
                root.Add(linesRootXElement);
            if (arcsRootXElement.HasElements)
                root.Add(arcsRootXElement);

            root.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        [Conditional("DEBUG")]
        public static void ExportEdges(IEnumerable<Edge> edges, string header)
        {
            CreateFolder();
            List<Curve> curves = new List<Curve>();
            foreach (Edge edge in edges)
            {
                curves.Add(edge.AsCurve());
            }

            ExportCurves(curves, header);
        }

        [Conditional("DEBUG")]
        public static void ExportLines(IEnumerable<Line> lines, string header)
        {
            CreateFolder();
            XElement rootXElement = new XElement("Lines");
            foreach (Line line in lines)
            {
                rootXElement.Add(GetGeometryFromObjects.GetXElementFromLine(line));
            }

            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        [Conditional("DEBUG")]
        public static void ExportLine(Line line, string header)
        {
            CreateFolder();
            XElement rootXElement = new XElement("Lines");
            rootXElement.Add(GetGeometryFromObjects.GetXElementFromLine(line));
            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        [Conditional("DEBUG")]
        public static void ExportArcs(IEnumerable<Arc> arcs, string header)
        {
            CreateFolder();
            XElement rootXElement = new XElement("Arcs");
            foreach (Arc arc in arcs)
            {
                rootXElement.Add(GetGeometryFromObjects.GetXElementFromArc(arc));
            }

            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        [Conditional("DEBUG")]
        public static void ExportPoints(IEnumerable<XYZ> points, string header)
        {
            CreateFolder();
            XElement rootXElement = new XElement("Points");
            foreach (XYZ point in points)
            {
                rootXElement.Add(GetGeometryFromObjects.GetXElementFromPoint(point));
            }

            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        [Conditional("DEBUG")]
        public static void ExportPoint(XYZ point, string header)
        {
            CreateFolder();
            XElement rootXElement = new XElement("Points");
            rootXElement.Add(GetGeometryFromObjects.GetXElementFromPoint(point));
            rootXElement.Save(Path.Combine(FolderName, GetFileName(header)));
        }

        #endregion

        #region Helpers

        private static void CreateFolder() => Directory.CreateDirectory(FolderName);

        private static string GetFileName(string header)
        {
            header = RemoveInvalidChars(header);
            return $"{DateTime.Now.Minute}_{DateTime.Now.Second}_{DateTime.Now.Millisecond}_{header}.xml";
        }

        private static IEnumerable<Curve> GetCurvesFromFamilyGeometry(FamilyInstance familyInstance, Options options)
        {
            // Если брать сразу трансформированную геометрию с параметром Transform.Identity
            // то отпадает необходимость получения GeometryInstance
            var geometryElement = familyInstance.get_Geometry(options)?.GetTransformed(Transform.Identity);

            if (geometryElement != null)
            {
                foreach (GeometryObject geometryObject in geometryElement)
                {
                    if (geometryObject is Solid solid)
                    {
                        foreach (Face solidFace in solid.Faces)
                        {
                            foreach (EdgeArray edgeArray in solidFace.EdgeLoops)
                            {
                                foreach (Edge edge in edgeArray)
                                    yield return edge.AsCurve();
                            }
                        }
                    }

                    if (geometryObject is Face face)
                    {
                        foreach (EdgeArray edgeArray in face.EdgeLoops)
                        {
                            foreach (Edge edge in edgeArray)
                                yield return edge.AsCurve();
                        }
                    }
                }
            }
        }

        private static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        #endregion
    }
}
