namespace CadDrawGeometry
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;
    using Autodesk.AutoCAD.Geometry;

    /// <summary>
    /// Helpers
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Get <see cref="Point3d"/> from attributes of <see cref="XElement"/>
        /// </summary>
        /// <param name="xEl">Instance of <see cref="XElement"/></param>
        public static Point3d GetAsPoint(this XElement xEl)
        {
            if (xEl != null)
            {
                if (TryParseDouble(xEl.Attribute("X")?.Value, out var x) &&
                    TryParseDouble(xEl.Attribute("Y")?.Value, out var y) &&
                    TryParseDouble(xEl.Attribute("Z")?.Value, out var z))
                {
                    return new Point3d(x, y, z);
                }
            }

            return Point3d.Origin;
        }

        /// <summary>
        /// Try parse double extension
        /// </summary>
        /// <param name="value">String</param>
        /// <param name="d">Out double value</param>
        public static bool TryParseDouble(string value, out double d)
        {
            if (!string.IsNullOrEmpty(value))
                return double.TryParse(value.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out d);

            d = double.NaN;
            return false;
        }

        /// <summary>
        /// Print <see cref="Exception"/> to AutoCAD command line
        /// </summary>
        /// <param name="exception">Instance of <see cref="Exception"/></param>
        public static void PrintException(this Exception exception)
        {
            Autodesk.AutoCAD.ApplicationServices.Core.Application
                .DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nError: {exception}");

            if (exception.InnerException != null)
                PrintException(exception.InnerException);
        }
    }
}
