using System.Globalization;
using System.Xml.Linq;
using Autodesk.AutoCAD.Geometry;

namespace CadDrawGeometry
{
    public static class Helpers
    {
        public static Point3d GetAsPoint(this XElement xEl)
        {
            if (xEl != null)
            {
                if (double.TryParse(xEl.Attribute("X")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var x) &&
                    double.TryParse(xEl.Attribute("Y")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var y) &&
                    double.TryParse(xEl.Attribute("Z")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var z))
                {
                    return new Point3d(x, y, z);
                }
            }
            return Point3d.Origin;
        }
    }
}
