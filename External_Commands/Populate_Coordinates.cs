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
    public class Populate_Coordinates : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;
            BaseLocation = doc.ActiveProjectLocation.GetProjectPosition(new XYZ(0, 0, 0));
            //Open Form
            try
            {
                ICollection<ElementId> SelectedObj = uidoc.Selection.GetElementIds();
                if (SelectedObj.Count == 0)
                {
                    MessageBox.Show("Please select family instances first.", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return Result.Failed;
                }
                List<ElementId> SlectedIds = SelectedObj.ToList();
                
                //Filter Elements
                for (int i =0; i < SlectedIds.Count; i++)
                {
                    GetNonSystemNonInPlaceInstances(SlectedIds[i], doc);
                }

                //Open Form
                formPopulate formDialog = new formPopulate();
                formDialog.InstanceNames = formInstanceNames;
                formDialog.InstanceIds = formInstanceIds;
                formDialog.InstanceLocations = formInstanceLocations;
                if(formDialog.ShowDialog() == DialogResult.OK)
                {
                    NSName = formDialog.ParaNS;
                    EWName = formDialog.ParaEW;
                    ELName = formDialog.ParaEL;
                    Decimal = formDialog.DecimalPlaces;

                    //Transaction
                    using (Transaction trans = new Transaction(doc, "Populate Instance Coordinates"))
                    {
                        trans.Start();
                        for (int i = 0; i < Instances.Count; i++)
                        {
                            FamilyInstance ele = Instances[i];
                            if (ele.LookupParameter(NSName) != null && ele.LookupParameter(NSName) != null && ele.LookupParameter(NSName) != null)
                            {
                                if(ele.LookupParameter(EWName).StorageType == StorageType.Double && ele.LookupParameter(NSName).StorageType == StorageType.Double && ele.LookupParameter(ELName).StorageType == StorageType.Double)
                                {
                                    double EW = Math.Round(LocationConverted[i][0], Decimal);
                                    double NS = Math.Round(LocationConverted[i][1], Decimal);
                                    double EL = Math.Round(LocationConverted[i][2], Decimal);
                                    ele.LookupParameter(EWName).Set(EW); 
                                    ele.LookupParameter(NSName).Set(NS); 
                                    ele.LookupParameter(ELName).Set(EL); 
                                    CompleteIds.Add(ele.Id);
                                }
                            }
                        }
                        view.IsolateElementsTemporary(CompleteIds);
                        trans.Commit();
                    }

                    //Report
                    if(CompleteIds.Count > 0)
                    {
                        uidoc.Selection.SetElementIds(CompleteIds);
                        MessageBox.Show(string.Format("Location information have been populated into the highlighted {0} family instances.", CompleteIds.Count.ToString()), "Operation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("None of the selected family instances has the input parameters. Check input parameter names and if parameter type is numerical value.", "Operation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private ProjectPosition BaseLocation { get; set; }
        private string NSName { get; set; }
        private string EWName { get; set; }
        private string ELName { get; set; }
        private int Decimal { get; set; }

        private List<ElementId> CompleteIds = new List<ElementId>();
        private List<ElementId> InstanceIds = new List<ElementId>();
        private List<FamilyInstance> Instances = new List<FamilyInstance>();
        private List<double[]> LocationConverted = new List<double[]>();

        private List<string> formInstanceIds = new List<string>();
        private List<string> formInstanceNames = new List<string>();
        private List<string> formInstanceLocations = new List<string>();

        private XYZ LocationToGeo(XYZ loc, ProjectPosition basepoint)
        {
            XYZ locGeoCo = ToGeoCoordinate(loc, basepoint.Angle);
            XYZ Output = new XYZ(basepoint.EastWest + locGeoCo.X, basepoint.NorthSouth + locGeoCo.Y, basepoint.Elevation + locGeoCo.Z);
            return Output;
        }

        private void GetNonSystemNonInPlaceInstances(ElementId id, Document doc)
        {
            Element ele = doc.GetElement(id);
            bool NonSystemInstance = ele.GetType() == typeof(FamilyInstance);
            if(NonSystemInstance)
            {
                bool NonInPlaceInstance = (ele as FamilyInstance).Symbol.Family.IsInPlace;
                if (NonInPlaceInstance == false)
                {
                    Location loc = (ele as FamilyInstance).Location;
                    try
                    {
                        XYZ location = (loc as LocationPoint).Point;
                        XYZ GeoLocation = LocationToGeo(location, BaseLocation);
                        double xx = UnitUtils.ConvertFromInternalUnits(GeoLocation.X, DisplayUnitType.DUT_METERS);
                        double yy = UnitUtils.ConvertFromInternalUnits(GeoLocation.Y, DisplayUnitType.DUT_METERS);
                        double zz = UnitUtils.ConvertFromInternalUnits(GeoLocation.Z, DisplayUnitType.DUT_METERS);

                        LocationConverted.Add(new double[3] {xx,yy,zz});
                        Instances.Add(ele as FamilyInstance);
                        InstanceIds.Add(id);

                        formInstanceNames.Add((ele as FamilyInstance).Symbol.Family.Name);
                        formInstanceIds.Add(id.ToString());
                        formInstanceLocations.Add("EW" + xx.ToString() + ", NS" + yy.ToString() + ", EL" + zz.ToString());
                    }
                    catch { }
                }
            }
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
