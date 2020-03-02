namespace RevitTestCommand
{
    using System;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI.Selection;

    /// <inheritdoc />
    internal class WallSelectionFilter : ISelectionFilter
    {
        /// <inheritdoc />
        public bool AllowElement(Element elem)
        {
            return elem is Wall;
        }

        /// <inheritdoc />
        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}