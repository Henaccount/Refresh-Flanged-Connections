using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.DataObjects;
using Autodesk.ProcessPower.PnP3dDataLinks;
using Autodesk.ProcessPower.PnP3dObjects;
using PlantApp = Autodesk.ProcessPower.PlantInstance.PlantApplication;

[assembly: Autodesk.AutoCAD.Runtime.CommandClass(typeof(RefreshFlangeConnections.Class1))]

namespace RefreshFlangeConnections
{
    public class Class1
    {
        [CommandMethod("RefreshFlangeConnections")]
        public static void ReconnectAllFlanges()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            string configstr = "";
            PromptResult pr = ed.GetString("\nplease provide configuration string (default:\"JointType=Flanged,TargetJointType=dummy\"):");
            if (pr.Status != PromptStatus.OK)
            {
                return;
            }
            else
                configstr = pr.StringResult;

            StringCollection configColl = new StringCollection();
            IDictionary<string, string> configDict = new Dictionary<string, string>();
            string targetjointtype = "";

            if (configstr.Equals(""))
            {
                ed.WriteMessage("No configuration string was provided, using defaults\n");
                configstr = "JointType=Flanged,TargetJointType=dummy";
            }

            string[] configArr = configstr.Split(new char[] { ',' });

            foreach (string tmpstr in configArr)
            {
                string key = tmpstr.Split(new char[] { '=' })[0].Trim();
                string value = tmpstr.Split(new char[] { '=' })[1].Trim();
                if (!key.Equals("TargetJointType"))
                {
                    configColl.Add(key);
                    configDict.Add(key, value);
                }
                else { targetjointtype = value; }
            }


            TypedValue[] tvs = new TypedValue[] { new TypedValue(0, "ACPPCONNECTOR") };
            SelectionFilter filter = new SelectionFilter(tvs);
            PromptSelectionResult selResult = ed.SelectAll(filter);

            SelectionSet selset = selResult.Value;

            ObjectId[] objIds = selset.GetObjectIds();

            foreach (ObjectId objId in objIds)
            {

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    if (!objId.ObjectClass.Name.Equals("AcPpDb3dConnector")) continue;

                    Connector connector = tr.GetObject(objId, OpenMode.ForWrite) as Connector;


                    StringCollection tmpcoll = PlantApp.CurrentProject.ProjectParts["Piping"].DataLinksManager.GetProperties(objId, configColl, true);

                    bool breakflag = false;
                    int i = 0;

                    //the intend here was to make it possible to filter for specific flanges, but the connector properties are not coming over
                    //(just very little), so only the JointType is currently working
                    foreach (KeyValuePair<string, string> configDictItem in configDict)
                    {
                        if (!tmpcoll[i].Equals(configDictItem.Value))
                        {
                            breakflag = true;
                            break;
                        }
                        ++i;
                    }

                    if (breakflag) continue;

                    ObjectId[] eleIdArray = new ObjectId[1];

                    eleIdArray[0] = objId;

                    ed.SetImpliedSelection(eleIdArray);


                    SendKeys.Send(targetjointtype.Substring(0, 1));
                    ed.Command("_.PLANTSUBSTITUTE");

                    //Thread.Sleep(2000);

                    tr.Commit();

                }


            }

            ed.WriteMessage("\nend of script\n");
        }

    }
}
