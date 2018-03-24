using System;
using System.Xml.Linq;
using Autodesk.Revit.DB;

namespace RevitGeometryExporter
{
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
                    startPointXElement.SetAttributeValue("X", line.GetEndPoint(0).X);
                    startPointXElement.SetAttributeValue("Y", line.GetEndPoint(0).Y);
                    startPointXElement.SetAttributeValue("Z", line.GetEndPoint(0).Z);
                    lineXElement.Add(startPointXElement);
                    XElement endPointXElement = new XElement("EndPoint");
                    endPointXElement.SetAttributeValue("X", line.GetEndPoint(1).X);
                    endPointXElement.SetAttributeValue("Y", line.GetEndPoint(1).Y);
                    endPointXElement.SetAttributeValue("Z", line.GetEndPoint(1).Z);
                    lineXElement.Add(endPointXElement);
                    return lineXElement;
                }
                else
                {
                    XElement rayXElement = new XElement("Ray");
                    XElement originPointXElement = new XElement("Origin");
                    originPointXElement.SetAttributeValue("X", line.Origin.X);
                    originPointXElement.SetAttributeValue("Y", line.Origin.Y);
                    originPointXElement.SetAttributeValue("Z", line.Origin.Z);
                    rayXElement.Add(originPointXElement);
                    XElement directionXElement = new XElement("Direction");
                    directionXElement.SetAttributeValue("X", line.Direction.X);
                    directionXElement.SetAttributeValue("Y", line.Direction.Y);
                    directionXElement.SetAttributeValue("Z", line.Direction.Z);
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
                if (arc.IsBound) // arc!
                {
                    XElement arcXElement = new XElement("Arc");
                    XElement element = new XElement("StartPoint");
                    element.SetAttributeValue("X", arc.GetEndPoint(0).X);
                    element.SetAttributeValue("Y", arc.GetEndPoint(0).Y);
                    element.SetAttributeValue("Z", arc.GetEndPoint(0).Z);
                    arcXElement.Add(element);
                    element = new XElement("EndPoint");
                    element.SetAttributeValue("X", arc.GetEndPoint(1).X);
                    element.SetAttributeValue("Y", arc.GetEndPoint(1).Y);
                    element.SetAttributeValue("Z", arc.GetEndPoint(1).Z);
                    arcXElement.Add(element);
                    element = new XElement("PointOnArc");
                    element.SetAttributeValue("X", arc.Tessellate()[1].X);
                    element.SetAttributeValue("Y", arc.Tessellate()[1].Y);
                    element.SetAttributeValue("Z", arc.Tessellate()[1].Z);
                    arcXElement.Add(element);
                    return arcXElement;
                }
                else // circle!
                {
                    XElement circleXElement = new XElement("Circle");
                    XElement centerPoint = new XElement("CenterPoint");
                    centerPoint.SetAttributeValue("X", arc.Center.X);
                    centerPoint.SetAttributeValue("Y", arc.Center.Y);
                    centerPoint.SetAttributeValue("Z", arc.Center.Z);
                    circleXElement.Add(centerPoint);
                    XElement vector = new XElement("VectorNormal");
                    vector.SetAttributeValue("X", arc.Normal.X);
                    vector.SetAttributeValue("Y", arc.Normal.Y);
                    vector.SetAttributeValue("Z", arc.Normal.Z);
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
            pointXElement.SetAttributeValue("X", point.X);
            pointXElement.SetAttributeValue("Y", point.Y);
            pointXElement.SetAttributeValue("Z", point.Z);
            return pointXElement;
        }
    }
}
