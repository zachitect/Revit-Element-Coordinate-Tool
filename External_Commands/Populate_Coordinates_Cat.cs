using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;
using View = Autodesk.Revit.DB.View;

namespace External_Buttons
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Populate_Coordinates_Cat : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;
            try
            {
                SortedList<string, Category> allcats = AllCats(doc);
                Category cat = PickCatForm(allcats);
                if(cat == null)
                {
                    return Result.Cancelled;
                }
                ICollection<Element> elems = GetEleOfCat(doc, cat);
                List<SetOutElement>  setelems = GetNonSystemNonInPlaceInstances(elems, doc);
                string[] paramnames = ShowFormPopulate(setelems);
                if (paramnames == null)
                {
                    return Result.Cancelled;
                }
                TransactionCoordinates(doc, uidoc, view, setelems, paramnames);
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private void TransactionCoordinates(Document doc, UIDocument uidoc, View view, List<SetOutElement> SetElementData, string[] ParamNames)
        {
            string nameNS = ParamNames[0];
            string nameEW = ParamNames[1];
            string nameEL = ParamNames[2];
            int DeciPlace = Convert.ToInt32(ParamNames[3]);
            List<ElementId> AllIds = new List<ElementId>();
            List<ElementId> CompletedIds = new List<ElementId>();
            bool AnyGroup = false;
            foreach(SetOutElement selem in SetElementData)
            {
                if (selem.element.GroupId.ToString() != "-1")
                {
                    AnyGroup = true;
                }
            }
            if(AnyGroup == true)
            {
                if(MessageBox.Show("Some family instances are grouped!\nDo you want to skip them?", "Grouped Elements Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    AnyGroup = true;
                }
                else
                {
                    AnyGroup = false;
                }
            }
            if(AnyGroup == true)
            {
                //Transaction
                using (Transaction trans = new Transaction(doc, "Populate Instance Coordinates"))
                {
                    trans.Start();
                    for (int i = 0; i < SetElementData.Count; i++)
                    {
                        Element elem = SetElementData[i].element;
                        AllIds.Add(elem.Id);
                        if (elem.LookupParameter(nameNS) != null && elem.LookupParameter(nameEW) != null && elem.LookupParameter(nameEL) != null)
                        {
                            if (elem.LookupParameter(nameNS).StorageType == StorageType.Double && elem.LookupParameter(nameEW).StorageType == StorageType.Double && elem.LookupParameter(nameEL).StorageType == StorageType.Double)
                            {
                                if (elem.GroupId.ToString() == "-1")
                                {
                                    double EW = Math.Round(SetElementData[i].EW, DeciPlace);
                                    double NS = Math.Round(SetElementData[i].NS, DeciPlace);
                                    double EL = Math.Round(SetElementData[i].EL, DeciPlace);
                                    elem.LookupParameter(nameEW).Set(EW);
                                    elem.LookupParameter(nameNS).Set(NS);
                                    elem.LookupParameter(nameEL).Set(EL);
                                    CompletedIds.Add(elem.Id);
                                }
                            }
                        }
                    }
                    view.IsolateElementsTemporary(AllIds);
                    trans.Commit();
                }
            }
            else
            {
                //Transaction
                using (Transaction trans = new Transaction(doc, "Populate Instance Coordinates"))
                {
                    trans.Start();
                    for (int i = 0; i < SetElementData.Count; i++)
                    {
                        Element elem = SetElementData[i].element;
                        AllIds.Add(elem.Id);
                        if (elem.LookupParameter(nameNS) != null && elem.LookupParameter(nameEW) != null && elem.LookupParameter(nameEL) != null)
                        {
                            if (elem.LookupParameter(nameNS).StorageType == StorageType.Double && elem.LookupParameter(nameEW).StorageType == StorageType.Double && elem.LookupParameter(nameEL).StorageType == StorageType.Double)
                            {
                                double EW = Math.Round(SetElementData[i].EW, DeciPlace);
                                double NS = Math.Round(SetElementData[i].NS, DeciPlace);
                                double EL = Math.Round(SetElementData[i].EL, DeciPlace);
                                elem.LookupParameter(nameEW).Set(EW);
                                elem.LookupParameter(nameNS).Set(NS);
                                elem.LookupParameter(nameEL).Set(EL);
                                CompletedIds.Add(elem.Id);
                            }
                        }
                    }
                    view.IsolateElementsTemporary(AllIds);
                    trans.Commit();
                }
            }

            if (CompletedIds.Count > 0)
            {
                uidoc.Selection.SetElementIds(CompletedIds);
                MessageBox.Show(string.Format("Location information successfully populated.\nHighlighted {0} instances\n\nWhat-if values look strange?\nMake sure parameter type is Number\n\ni.e. NOT Length", CompletedIds.Count.ToString()), "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("None of the instances in the selected cateogry has valid parameters assigned before running this operation", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string[] ShowFormPopulate(List<SetOutElement> SetElementData)
        {
            formPopulate FP = new formPopulate();
            FP.ElementData = SetElementData;
            if(FP.ShowDialog() == DialogResult.OK)
            {
                return new string[] { FP.ParaNS, FP.ParaEW, FP.ParaEL, FP.DecimalPlaces.ToString()};
            }
            return null;
        }
        private SortedList<string, Category> AllCats(Document doc)
        {
            Categories cats = doc.Settings.Categories;
            SortedList<string, Category> sortedcats = new SortedList<string, Category>();
            foreach (Category cat in cats)
            {
                if(cat.AllowsBoundParameters)
                {
                    sortedcats.Add(cat.Name, cat);
                }
            }
            return sortedcats;
        }

        private Category PickCatForm(SortedList<string, Category> sCats)
        {
            Category SelectedCat;
            formCate FA = new formCate();
            FA.CatNames = sCats.Keys.ToArray();
            if(FA.ShowDialog() == DialogResult.OK)
            {
                sCats.TryGetValue(FA.Selected, out SelectedCat);
                return SelectedCat;
            }
            return null;
        }

        private ICollection<Element> GetEleOfCat(Document doc, Category cat)
        {
            FilteredElementCollector FE = new FilteredElementCollector(doc).OfCategoryId(cat.Id);
            return FE.ToElements();
        }

        private List<SetOutElement> GetNonSystemNonInPlaceInstances(ICollection<Element> elems, Document doc)
        {
            List<SetOutElement> SetElements = new List<SetOutElement>();
            ProjectPosition BaseLocation = doc.ActiveProjectLocation.GetProjectPosition(new XYZ(0, 0, 0));
            foreach (Element elem in elems)
            {
                bool NonSystemInstance = elem.GetType() == typeof(FamilyInstance);
                if(NonSystemInstance)
                {
                    bool NonInPlaceInstance = (elem as FamilyInstance).Symbol.Family.IsInPlace == false ;
                    if(NonInPlaceInstance)
                    {
                        Location loc = (elem as FamilyInstance).Location;
                        try
                        {
                            XYZ location = (loc as LocationPoint).Point;
                            XYZ GeoLocation = LocationToGeo(location, BaseLocation);
                            SetOutElement SeElem = new SetOutElement();
                            SeElem.element = elem as FamilyInstance;
                            SeElem.name = elem.Name;
                            SeElem.id = elem.Id;
                            SeElem.EW = UnitUtils.ConvertFromInternalUnits(GeoLocation.X, DisplayUnitType.DUT_METERS);
                            SeElem.NS = UnitUtils.ConvertFromInternalUnits(GeoLocation.Y, DisplayUnitType.DUT_METERS);
                            SeElem.EL = UnitUtils.ConvertFromInternalUnits(GeoLocation.Z, DisplayUnitType.DUT_METERS);
                            SetElements.Add(SeElem);
                        }
                        catch { }
                    }
                }
            }
            return SetElements;
        }

        public class SetOutElement
        {
            public string name { get; set;}
            public FamilyInstance element { get; set; }
            public ElementId id { get; set; }
            public double NS { get; set; }
            public double EW { get; set; }
            public double EL { get; set; }
        }

        private XYZ LocationToGeo(XYZ loc, ProjectPosition basepoint)
        {
            XYZ locGeoCo = ToGeoCoordinate(loc, basepoint.Angle);
            XYZ Output = new XYZ(basepoint.EastWest + locGeoCo.X, basepoint.NorthSouth + locGeoCo.Y, basepoint.Elevation + locGeoCo.Z);
            return Output;
        }

        public XYZ ToGeoCoordinate(XYZ Input, double AngleToTrueNorth)
        {
            double x = Input.X;
            double y = Input.Y;
            double z = Input.Z;
            double newx = x * Math.Cos(AngleToTrueNorth) - y * Math.Sin(AngleToTrueNorth);
            double newy = y * Math.Cos(AngleToTrueNorth) + x * Math.Sin(AngleToTrueNorth);

            XYZ Output = new XYZ(newx, newy, z);
            return Output;
        }
    }
}
