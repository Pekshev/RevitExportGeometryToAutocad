namespace CadDrawGeometry
{
    using System.IO;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using Autodesk.AutoCAD.Runtime;

    public class CreateFromXml
    {
        /// <summary>Отрисовка геометрии из одного указанного xml-файла</summary>
        [CommandMethod("DrawFromOneXml")]
        public void Create()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
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
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
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
                    if (bt != null)
                    {
                        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                        // from root
                        CreateLines(fileXElement, tr, btr);
                        CreateRays(fileXElement, tr, btr);
                        CreateArcs(fileXElement, tr, btr);
                        CreateCircles(fileXElement, tr, btr);
                        CreatePoints(fileXElement, tr, btr);

                        // all in one
                        XElement lines = fileXElement.Element("Lines");
                        if (lines != null)
                        {
                            CreateLines(lines, tr, btr);
                            CreateRays(lines, tr, btr);
                        }

                        XElement arcs = fileXElement.Element("Arcs");
                        if (arcs != null)
                        {
                            CreateArcs(arcs, tr, btr);
                            CreateCircles(arcs, tr, btr);
                        }

                        XElement points = fileXElement.Element("Points");
                        if (points != null)
                            CreatePoints(points, tr, btr);
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception exception)
            {
                exception.PrintException();
            }
        }

        /// <summary>Создание отрезков</summary>
        private void CreateLines(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement curveXelement in root.Elements("Line"))
            {
                XElement startPointXElement = curveXelement.Element("StartPoint");
                Point3d startPoint = startPointXElement.GetAsPoint();
                XElement endPointXElement = curveXelement.Element("EndPoint");
                Point3d endPoint = endPointXElement.GetAsPoint();
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
                Point3d startPoint = startPointXElement.GetAsPoint();
                XElement endPointXElement = curveXelement.Element("EndPoint");
                Point3d endPoint = endPointXElement.GetAsPoint();
                XElement pointOnArcXElement = curveXelement.Element("PointOnArc");
                Point3d pointOnArc = pointOnArcXElement.GetAsPoint();

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
                Point3d centerPoint = centerPointXElement.GetAsPoint();
                XElement vectorNormalXElement = curveXelement.Element("VectorNormal");
                Vector3d vectorNormal = vectorNormalXElement.GetAsPoint().GetAsVector();

                if (Helpers.TryParseDouble(curveXelement.Element("Radius")?.Value, out var d))
                {
                    using (Circle circle = new Circle(centerPoint, vectorNormal, d))
                    {
                        btr.AppendEntity(circle);
                        tr.AddNewlyCreatedDBObject(circle, true);
                    }
                }
            }
        }

        /// <summary>Создание лучей</summary>
        private void CreateRays(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (XElement rayXElement in root.Elements("Ray"))
            {
                XElement originXElement = rayXElement.Element("Origin");
                Point3d originPoint = originXElement.GetAsPoint();
                XElement directionXElement = rayXElement.Element("Direction");
                Vector3d direction = directionXElement.GetAsPoint().GetAsVector();

                using (Ray ray = new Ray())
                {
                    ray.BasePoint = originPoint;
                    ray.UnitDir = direction;
                    btr.AppendEntity(ray);
                    tr.AddNewlyCreatedDBObject(ray, true);
                }
            }
        }

        /// <summary>Создание точек</summary>
        private void CreatePoints(XElement root, Transaction tr, BlockTableRecord btr)
        {
            foreach (var xElement in root.Elements("Point"))
            {
                Point3d startPoint = xElement.GetAsPoint();
                DBPoint dbPoint = new DBPoint(startPoint);
                btr.AppendEntity(dbPoint);
                tr.AddNewlyCreatedDBObject(dbPoint, true);
            }
        }
    }
}
