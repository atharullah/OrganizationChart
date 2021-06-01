using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using System.Linq;
using Microsoft.Office.Server.UserProfiles;
using System.IO;
using Microsoft.SharePoint.Administration;

namespace OrganizationChart.Chart
{
    public partial class ChartUserControl : UserControl
    {
        string Data = "";
        string DepartmentName = "";
        public Chart webPart { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {                
                string topUserName = string.Format(webPart.LogInName);
                string listName = webPart.ListName;
                lblListName.Text = listName;
                if (!string.IsNullOrEmpty(topUserName) && !string.IsNullOrEmpty(listName))
                {
                   using(SPSite site =new SPSite(SPContext.Current.Site.Url))
                   {
                       SPList userList = site.RootWeb.Lists.TryGetList(listName);
                       if (userList != null)
                       {
                           SPQuery query = new SPQuery();
                           query.Query = string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where>", userProfileList.Name.ToString(), topUserName);

                           SPListItemCollection useritem = userList.GetItems(query);

                           if (useritem.Count > 0)
                           {
                               DepartmentName = Convert.ToString(useritem[0][userProfileList.Department.ToString()]);
                               Data += "<li>";
                               Data += CreateChild(useritem[0]);
                               AddUser(userList, topUserName);
                               Data += "</li>";
                               org.InnerHtml = Data;
                           }
                       }
                       else
                       {
                           lblResult.Text = "List does not exist";
                       }
                   }                    
                }
                else
                {
                    lblResult.Text = "Listname, domain name, and user name should not be empty";
                }
            }
            catch (Exception ex)
            {
                LogClass.WriteLog(ex.ToString());
                LogClass.LogMessage(ex);
            }
        }

        public void AddUser(SPList userList, string topUserName)
        {
            try
            {
                SPQuery query = new SPQuery();
                if (webPart.SameDepartment)
                {
                    query.Query = string.Format("<Where><And><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq><Eq><FieldRef Name='{2}'/><Value Type='Text'>{3}</Value></Eq></And></Where>", userProfileList.Manager.ToString(), topUserName, userProfileList.Department.ToString(), DepartmentName);
                }
                else
                {
                    query.Query = string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where>",userProfileList.Manager.ToString(),topUserName);
                }
                SPListItemCollection topItems = userList.GetItems(query);

                if (topItems.Count > 0)
                {
                    Data += "<ul>";
                    string classname = "class='collapsed'";
                    SPListItem item = null;
                    for (int ix = 0; ix < topItems.Count; ix++, classname = "")
                    {
                        item = topItems[ix];
                        Data += "<li " + classname + ">";
                        Data += CreateChild(item);
                        topUserName = Convert.ToString(item[userProfileList.Name.ToString()]);
                        AddUser(userList, topUserName);
                        Data += "</li>";
                    }
                    Data += "</ul>";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string CreateChild(SPListItem item)
        {
            try
            {
                string imgUrl = Convert.ToString(item[userProfileList.ProfilePicture.ToString()]);
                if (string.IsNullOrEmpty(imgUrl))
                {
                    imgUrl = "/SiteAssets/Images/Organisation Chart/profile_1.png";
                }
                string inbound;
                string outbound;
                if(webPart.SameDepartment)
                {
                    inbound = Convert.ToString(item[userProfileList.DeptInbound.ToString()]);
                    outbound = Convert.ToString(item[userProfileList.DeptOutbound.ToString()]);
                }
                else
                {
                    inbound = Convert.ToString(item[userProfileList.Inbound.ToString()]);
                    outbound = Convert.ToString(item[userProfileList.Outbound.ToString()]);
                }
                
                string personalUrl = Convert.ToString(item[userProfileList.PersonalURL.ToString()]);
                string Name = Convert.ToString(item[userProfileList.DisplayName.ToString()]);
                string designation = Convert.ToString(item[userProfileList.Designation.ToString()]);
                int itemID = item.ID;
                if (string.IsNullOrEmpty(designation))
                {
                    designation = "FGIT Empty";
                }

                string litrelControl = string.Format(@"<div class='chart_1'>
                                                   <div class='profile_chart'>
                                                   <img src='{0}' width='60' height='60' />
                                                   </div>
                                                    <div class='inbound'>
                                                        <div class='labe_1'>
                                                            <label>F-{1}</label></div>
                                                        <div class='labe_1'>
                                                            <label>O-{2}</label></div>
                                                    </div>
                                                    <div class='clear_fix'></div>

                                                    <div class='discribtion'>
                                                        <a href='{3}' id='trigger' data-itemID='{6}'>{4}</a>
                                                    </div>
                                                    <div class='clear_fix'></div>
                                                    <div class='discribtion_2'>{5}</div>
                                                </div>", imgUrl, inbound, outbound, personalUrl, Name, designation, itemID);

                return litrelControl;
            }
            catch (Exception)
            {
                throw;
            }
        }

        enum userProfileList
        {
            Name,
            ProfilePicture,
            Manager,
            Department,
            AboutMe,
            Designation,
            WhoseManager,
            DisplayName,
            PersonalURL,
            EmployeeID,
            Inbound,
            Outbound,
            DeptInbound,
            DeptOutbound
        }

    }

    #region Log

    class LogClass
    {

        public static void WriteLog(string LogText)
        {
            try
            {
                string fullFileName = @"C:\FGITOrgChartException.txt";
                string folderpath = string.Empty;
                string fileName = string.Empty;
                string ext = string.Empty;

                string file = fullFileName;

                folderpath = file.Remove(file.LastIndexOf("\\") + 1);
                fileName = file.Substring(file.LastIndexOf("\\") + 1);
                ext = fileName.Substring(fileName.LastIndexOf("."));
                fileName = fileName.Remove(fileName.LastIndexOf("."));

                string LogFilePath = folderpath + fileName + ext; ;
                if (File.Exists(fullFileName))
                {
                    FileInfo fi = new FileInfo(fullFileName);
                    long size = fi.Length;

                    if (size > 10485760)
                    {
                        string date = DateTime.Today.Year.ToString() + DateTime.Today.Month.ToString() + DateTime.Today.Day.ToString();
                        string time = DateTime.Now.TimeOfDay.Hours.ToString() + DateTime.Now.TimeOfDay.Minutes.ToString() + DateTime.Now.TimeOfDay.Seconds.ToString();
                        string dateTime = date + "T" + time;
                        string newPath = folderpath + fileName + "_" + dateTime + ext;
                        File.Copy(LogFilePath, newPath);
                        File.Delete(LogFilePath);

                    }
                }

                StreamWriter SW;

                SW = File.AppendText(LogFilePath);
                SW.WriteLine(DateTime.Now + ":~UserPolling~:" + LogText);
                SW.Close();


            }
            catch
            {
                //LogMessage(ex);
                throw;

            }
        }

        /// <summary>
        /// Private method Logs Error Message
        /// </summary>
        /// <param name="severity">TraceSeverity severity</param>
        /// <param name="message">String Message</param>
        private static void LogMessage(TraceSeverity severity, string message)
        {
            try
            {
                uint uintEventID = 8000;//event ID
                string CategoryName = "Log Message";
                SPDiagnosticsCategory category = new SPDiagnosticsCategory(CategoryName, TraceSeverity.Medium, EventSeverity.Error);
                SPDiagnosticsService.Local.WriteTrace(uintEventID, category, TraceSeverity.Unexpected, message);
            }
            catch (Exception ex)
            {

                LogMessage(ex);

            }


        }

        /// <summary>
        /// Log Error Message
        /// </summary>
        /// <param name="ex">Exception ex</param>
        public static void LogMessage(Exception ex)
        {
            LogMessage(TraceSeverity.High, ex.Message + ex.StackTrace);
        }

        /// <summary>
        /// Log Message
        /// </summary>
        /// <param name="message">string</param>
        public static void LogMessage(string message)
        {
            LogMessage(TraceSeverity.Medium, message);
        }
    }
    #endregion
}
