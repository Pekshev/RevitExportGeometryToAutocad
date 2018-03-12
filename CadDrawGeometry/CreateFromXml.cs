using System;
using System.IO;
using System.Xml.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows.Forms;

namespace CadDrawGeometry
{
    public class CreateFromXml
    {
        /// <summary>Отрисовка геометрии из одного указанного xml-файла</summary>
        [CommandMethod("DrawFromOneXml")]
        public void Create()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            DrawGeometryFromFile(ofd.FileName);
        }
        /// <summary>Отрисовка геометрии из указанной папки в который должны располагаться xml-файлы</summary>
        [CommandMethod("DrawXmlFromFolder")]
        public void CreateFromFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var files = Directory.GetFiles(fbd.SelectedPath, "*.xml", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    DrawGeometryFromFile(file);
                }
            }
        }
        /// <summary>Отрисовка геометрии из нескольких указанных xml-файлов</summary>
        [CommandMethod("DrawFromSeveralXml")]
        public void CreateFromSeveralFiles()
        {
            OpenFileDialog ofd = new OpenFileDialog { Multiselect = true };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            foreach (string fileName in ofd.FileNames)
            {
                DrawGeometryFromFile(fileName);
            }
        }

        private void DrawGeometryFromFile(string file)
        {
            try
            {
                XElement fileXElement = XElement.Load(file);
                var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                var db = doc.Database;
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    // from root
                    CreateLines(fileXElement, tr, btr);
                    CreateArcs(fileXElement, tr, btr);
                    CreateCircles(fileXElement, tr, btr);
                    CreatePoints(fileXElement, tr, btr);
                    // all in one
                    XElement lines = fileXElement.Element("Lines");
                    if (lines != null)
                        CreateLines(lines, tr, btr);
                    XElement arcs = fileXElement.Element("Arcs");
                    if (arcs != null)
                    {
                        CreateArcs(arcs, tr, btr);
                        CreateCircles(arcs, tr, btr);
                    }
                    XElement points = fileXElement.Element("Points");
                    if (points != null) CreatePoints(points, tr, btr);

                    tr.Commit();
                }
            }
            catch (System.Exception exception)
            {
                Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                    "\nError: " + exception.Message);
            }
        }
        /// <summary>Создание отрезков</summary>
        private void CreateLines(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement curveXelement in root.Elements("Line"))
            {
                XElement startPointXElement = curveXelement.Element("StartPoint");
                Point3d startPoint = new Point3d(
                    Convert.ToDouble(startPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Z")?.Value));
                XElement endPointXElement = curveXelement.Element("EndPoint");
                Point3d endPoint = new Point3d(
                    Convert.ToDouble(endPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Z")?.Value));
                using (Line line = new Line(startPoint, endPoint))
                {
                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                }
            }
        }
        /// <summary>Создание дуг</summary>
        private void CreateArcs(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement curveXelement in root.Elements("Arc"))
            {
                XElement startPointXElement = curveXelement.Element("StartPoint");
                Point3d startPoint = new Point3d(
                    Convert.ToDouble(startPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(startPointXElement?.Attribute("Z")?.Value));
                XElement endPointXElement = curveXelement.Element("EndPoint");
                Point3d endPoint = new Point3d(
                    Convert.ToDouble(endPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(endPointXElement?.Attribute("Z")?.Value));

                XElement pointOnArcXElement = curveXelement.Element("PointOnArc");
                Point3d pointOnArc = new Point3d(
                    Convert.ToDouble(pointOnArcXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(pointOnArcXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(pointOnArcXElement?.Attribute("Z")?.Value)
                );
                // create a CircularArc3d
                CircularArc3d carc = new CircularArc3d(startPoint, pointOnArc, endPoint);

                // now convert the CircularArc3d to an Arc
                Point3d cpt = carc.Center;
                Vector3d normal = carc.Normal;
                Vector3d refVec = carc.ReferenceVector;
                Plane plan = new Plane(cpt, normal);
                double ang = refVec.AngleOnPlane(plan);
                using (Arc arc = new Arc(cpt, normal, carc.Radius, carc.StartAngle + ang, carc.EndAngle + ang))
                {
                    btr.AppendEntity(arc);
                    tr.AddNewlyCreatedDBObject(arc, true);
                }
                // dispose CircularArc3d
                carc.Dispose();
            }
        }
        /// <summary>Создание окружностей</summary>
        private void CreateCircles(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement curveXelement in root.Elements("Circle"))
            {
                XElement centerPointXElement = curveXelement.Element("CenterPoint");
                Point3d centerPoint = new Point3d(
                    Convert.ToDouble(centerPointXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(centerPointXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(centerPointXElement?.Attribute("Z")?.Value));
                XElement vectorNormalXElement = curveXelement.Element("VectorNormal");
                Vector3d vectorNormal = new Vector3d(
                    Convert.ToDouble(vectorNormalXElement?.Attribute("X")?.Value),
                    Convert.ToDouble(vectorNormalXElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(vectorNormalXElement?.Attribute("Z")?.Value));


                using (Circle circle = new Circle(centerPoint, vectorNormal, Convert.ToDouble(curveXelement.Element("Radius")?.Value)))
                {
                    btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);
                }
            }
        }
        /// <summary>Создание точек</summary>
        private void CreatePoints(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (var xElement in root.Elements("Point"))
            {
                Point3d startPoint = new Point3d(
                    Convert.ToDouble(xElement?.Attribute("X")?.Value),
                    Convert.ToDouble(xElement?.Attribute("Y")?.Value),
                    Convert.ToDouble(xElement?.Attribute("Z")?.Value));
                DBPoint dbPoint = new DBPoint(startPoint);
                btr.AppendEntity(dbPoint);
                tr.AddNewlyCreatedDBObject(dbPoint, true);
            }
        }
    }
}
