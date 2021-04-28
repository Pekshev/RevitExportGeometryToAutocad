namespace RevitGeometryExporter
{
    using System;
    using System.Xml.Linq;
    using Autodesk.Revit.DB;

    internal static class GetGeometryFromObjects
    {
        internal static XElement GetXElementFromLine(Line line)
        {
            try
            {
                if (line.IsBound)
                {
                    XElement lineXElement = new XElement("Line");
                    XElement startPointXElement = new XElement("StartPoint");
                    startPointXElement.SetAttributeValue("X", line.GetEndPoint(0).X.ConvertToExportUnits());
                    startPointXElement.SetAttributeValue("Y", line.GetEndPoint(0).Y.ConvertToExportUnits());
                    startPointXElement.SetAttributeValue("Z", line.GetEndPoint(0).Z.ConvertToExportUnits());
                    lineXElement.Add(startPointXElement);
                    XElement endPointXElement = new XElement("EndPoint");
                    endPointXElement.SetAttributeValue("X", line.GetEndPoint(1).X.ConvertToExportUnits());
                    endPointXElement.SetAttributeValue("Y", line.GetEndPoint(1).Y.ConvertToExportUnits());
                    endPointXElement.SetAttributeValue("Z", line.GetEndPoint(1).Z.ConvertToExportUnits());
                    lineXElement.Add(endPointXElement);
                    return lineXElement;
                }
                else
                {
                    XElement rayXElement = new XElement("Ray");
                    XElement originPointXElement = new XElement("Origin");
                    originPointXElement.SetAttributeValue("X", line.Origin.X.ConvertToExportUnits());
                    originPointXElement.SetAttributeValue("Y", line.Origin.Y.ConvertToExportUnits());
                    originPointXElement.SetAttributeValue("Z", line.Origin.Z.ConvertToExportUnits());
                    rayXElement.Add(originPointXElement);
                    XElement directionXElement = new XElement("Direction");
                    directionXElement.SetAttributeValue("X", line.Direction.X.ConvertToExportUnits());
                    directionXElement.SetAttributeValue("Y", line.Direction.Y.ConvertToExportUnits());
                    directionXElement.SetAttributeValue("Z", line.Direction.Z.ConvertToExportUnits());
                    rayXElement.Add(directionXElement);
                    return rayXElement;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static XElement GetXElementFromArc(Arc arc)
        {
            try
            {
                if (arc.IsBound) //// arc!
                {
                    XElement arcXElement = new XElement("Arc");
                    XElement element = new XElement("StartPoint");
                    element.SetAttributeValue("X", arc.GetEndPoint(0).X.ConvertToExportUnits());
                    element.SetAttributeValue("Y", arc.GetEndPoint(0).Y.ConvertToExportUnits());
                    element.SetAttributeValue("Z", arc.GetEndPoint(0).Z.ConvertToExportUnits());
                    arcXElement.Add(element);
                    element = new XElement("EndPoint");
                    element.SetAttributeValue("X", arc.GetEndPoint(1).X.ConvertToExportUnits());
                    element.SetAttributeValue("Y", arc.GetEndPoint(1).Y.ConvertToExportUnits());
                    element.SetAttributeValue("Z", arc.GetEndPoint(1).Z.ConvertToExportUnits());
                    arcXElement.Add(element);
                    element = new XElement("PointOnArc");
                    element.SetAttributeValue("X", arc.Tessellate()[1].X.ConvertToExportUnits());
                    element.SetAttributeValue("Y", arc.Tessellate()[1].Y.ConvertToExportUnits());
                    element.SetAttributeValue("Z", arc.Tessellate()[1].Z.ConvertToExportUnits());
                    arcXElement.Add(element);
                    return arcXElement;
                }
                else //// circle!
                {
                    XElement circleXElement = new XElement("Circle");
                    XElement centerPoint = new XElement("CenterPoint");
                    centerPoint.SetAttributeValue("X", arc.Center.X.ConvertToExportUnits());
                    centerPoint.SetAttributeValue("Y", arc.Center.Y.ConvertToExportUnits());
                    centerPoint.SetAttributeValue("Z", arc.Center.Z.ConvertToExportUnits());
                    circleXElement.Add(centerPoint);
                    XElement vector = new XElement("VectorNormal");
                    vector.SetAttributeValue("X", arc.Normal.X.ConvertToExportUnits());
                    vector.SetAttributeValue("Y", arc.Normal.Y.ConvertToExportUnits());
                    vector.SetAttributeValue("Z", arc.Normal.Z.ConvertToExportUnits());
                    circleXElement.Add(vector);

                    circleXElement.SetElementValue("Radius", arc.Radius);
                    return circleXElement;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static XElement GetXElementFromPoint(XYZ point)
        {
            XElement pointXElement = new XElement("Point");
            pointXElement.SetAttributeValue("X", point.X.ConvertToExportUnits());
            pointXElement.SetAttributeValue("Y", point.Y.ConvertToExportUnits());
            pointXElement.SetAttributeValue("Z", point.Z.ConvertToExportUnits());
            return pointXElement;
        }

        private static double ConvertToExportUnits(this double valueInFt)
        {
            // The UnitUtils is not used here so that there is no error in Revit 2021 and higher
            if (ExportGeometryToXml.ExportUnits == ExportUnits.Mm)
                return valueInFt * 304.8;

            return valueInFt;
        }
    }
}
