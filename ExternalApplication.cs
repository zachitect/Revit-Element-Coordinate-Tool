using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace External_Buttons
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string TabName = "External Commands";
            //Create Ribbon Tab
            application.CreateRibbonTab(TabName);

            //Get Assembly
            string path = Assembly.GetExecutingAssembly().Location;
            
            //Create Ribbon Panel
            RibbonPanel panelCoordinate = application.CreateRibbonPanel(TabName, "Populate Coordinate");

            //Theme and Colours
            PushButtonData populateCoordinate = new PushButtonData("populateCoordinate", "Populate\nCoordinate", path, "External_Buttons.Populate_Coordinates_Cat");
            PushButton bupopulateCoordinate = panelCoordinate.AddItem(populateCoordinate) as PushButton;
            bupopulateCoordinate.LargeImage = GetEmbeddedImage("populateCoordinate.png");
            bupopulateCoordinate.ToolTip = "Populate Location Geo-Coordinate Values of Family Instances into Parameters, all System Families and In-Place-Families will be Ignored.";
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        public static BitmapSource GetEmbeddedImage(string embeddedPath)
        {
            try
            {
                Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("External_Buttons.Icons." + embeddedPath);
                PngBitmapDecoder decoder = new PngBitmapDecoder(myStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return decoder.Frames[0];
            }
            catch
            {
                return null;
            }
        }
    }
}
