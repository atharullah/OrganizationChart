using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Server;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using System.Web;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Workflow;
using System.IO;
using System.Configuration;


namespace UserProfileSchedular
{
    public class Metadata
    {
        public static string siteURL
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings.Get("siteURL");
                }
                catch
                {
                    throw;
                }
            }
        }

        public static string listName
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings.Get("listName");
                }
                catch
                {
                    throw;
                }
            }
        }
    }
    class Program
    {
        enum customUserProperties
        {
            EmployeeID
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
            Mobile,
            Email,
            FriendlyName,
            DeptInbound,
            DeptOutbound
        }

        static void Main(string[] args)
        {
            try
            {
                if (Metadata.siteURL != null && Metadata.listName != null)
                {
                    using (SPSite site = new SPSite(Metadata.siteURL))
                    {
                        SPServiceContext serviceContext = SPServiceContext.GetContext(site);
                        UserProfileManager profileMgr = new UserProfileManager(serviceContext);

                        IEnumerator profiles = profileMgr.GetEnumerator();
                        SPWeb web = site.RootWeb;
                        if (profiles.MoveNext())
                        {
                            SPList profileList = web.Lists.TryGetList(Metadata.listName);

                            if (profileList != null)
                            {
                                //deleteListData(profileList, site);
                                deleteAllItems(profileList);
                                profiles.Reset();
                                Console.WriteLine("Getting user profiles");
                                int i = 1;
                                while (profiles.MoveNext())
                                {
                                    UserProfile profile = (UserProfile)profiles.Current;
                                    SPListItem item = profileList.AddItem();

                                    string loginName = Convert.ToString(profile.AccountName);
                                    if (!string.IsNullOrEmpty(loginName))
                                        item[userProfileList.Name.ToString()] = loginName.Substring(loginName.LastIndexOf('\\') + 1);

                                    item[userProfileList.AboutMe.ToString()] = Convert.ToString(profile[PropertyConstants.AboutMe].Value);
                                    item[userProfileList.Department.ToString()] = Convert.ToString(profile[PropertyConstants.Department].Value);
                                    item[userProfileList.Designation.ToString()] = Convert.ToString(profile[PropertyConstants.Responsibility].Value);
                                    item[userProfileList.ProfilePicture.ToString()] = Convert.ToString(profile[PropertyConstants.PictureUrl].Value);
                                    item[userProfileList.DisplayName.ToString()] = Convert.ToString(profile.DisplayName);
                                    item[userProfileList.PersonalURL.ToString()] = Convert.ToString(profile.PersonalUrl);

                                    string managerName = Convert.ToString(profile[PropertyConstants.Manager].Value);
                                    if (!string.IsNullOrEmpty(managerName))
                                        item[userProfileList.Manager.ToString()] = managerName.Substring(managerName.LastIndexOf('\\') + 1);

                                    item[userProfileList.EmployeeID.ToString()] = Convert.ToString(profile[customUserProperties.EmployeeID.ToString()].Value);
                                    item[userProfileList.Mobile.ToString()] = Convert.ToString(profile[PropertyConstants.WorkPhone].Value);
                                    item[userProfileList.Email.ToString()] = Convert.ToString(profile[PropertyConstants.WorkEmail].Value);
                                    item[userProfileList.FriendlyName.ToString()] = Convert.ToString(profile.DisplayName);

                                    item.Update();

                                    Console.WriteLine(i + " " + profile.AccountName + " user profiles done ");
                                    i++;
                                }
                                setInboundOutbound(profileList);
                            }
                        }
                    }
                }
                else
                {
                    Exception ex = new Exception("siteURL and listname in config file should not be empty");
                    LogClass.WriteLog(ex.Message);
                }
            }
            catch (Exception ex)
            {
                LogClass.WriteLog(ex.Message);
                LogClass.LogMessage(ex);
                Console.Read();
            }
        }

        public static void setInboundOutbound(SPList profileList)
        {
            Console.WriteLine("setting user inbound outbound");
            int j = 0;
            foreach (SPListItem item in profileList.GetItems(new SPQuery()))
            {
                string name = Convert.ToString(item[userProfileList.Name.ToString()]);
                if (!string.IsNullOrEmpty(name))
                {
                    SPQuery query = new SPQuery();
                    query.Query = string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where><OrderBy><FieldRef Name='{2}' Ascending='True'/></OrderBy>", userProfileList.Manager.ToString(), name, userProfileList.Name.ToString());
                    SPListItemCollection items = profileList.GetItems(query);

                    SPQuery deptQuery = new SPQuery();
                    string deptName = Convert.ToString(item[userProfileList.Department.ToString()]);
                    deptQuery.Query = string.Format("<Where><And><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq><Eq><FieldRef Name='{2}'/><Value Type='Text'>{3}</Value></Eq></And></Where><OrderBy><FieldRef Name='{4}' Ascending='True'/></OrderBy>", userProfileList.Manager.ToString(), name, userProfileList.Department.ToString(), deptName, userProfileList.Name.ToString());
                    SPListItemCollection deptItems = profileList.GetItems(deptQuery);
                    int inbound = 0;
                    int outbound = 0;
                    int deptInbound = 0;
                    int deptOutbound = 0;
                    if (items.Count > 0)
                    {
                        foreach (SPListItem useritem in items)
                        {
                            string EmpName = Convert.ToString(useritem[userProfileList.Name.ToString()]);
                            if (!string.IsNullOrEmpty(EmpName))
                            {
                                if (Convert.ToInt64(EmpName.ToLower()[0]) <= 109)
                                {
                                    inbound++;
                                }
                                else
                                {
                                    outbound++;
                                }
                            }
                            else
                            {
                                inbound++;
                            }
                        }
                    }
                    if (deptItems.Count > 0)
                    {
                        foreach (SPListItem useritem in deptItems)
                        {
                            string EmpName = Convert.ToString(useritem[userProfileList.Name.ToString()]);
                            if (!string.IsNullOrEmpty(EmpName))
                            {
                                if (Convert.ToInt64(EmpName.ToLower()[0]) <= 109)
                                {
                                    deptInbound++;
                                }
                                else
                                {
                                    deptOutbound++;
                                }
                            }
                            else
                            {
                                deptInbound++;
                            }
                        }
                    }
                    item[userProfileList.Inbound.ToString()] = inbound;
                    item[userProfileList.Outbound.ToString()] = outbound;
                    item[userProfileList.DeptInbound.ToString()] = deptInbound;
                    item[userProfileList.DeptOutbound.ToString()] = deptOutbound;
                    item.Update();

                    Console.WriteLine(j + " " + name + " user inbound outbound done ");
                    j++;
                }
            }
        }

        public static void deleteAllItems(SPList list)
        {
            SPListItemCollection items = list.GetItems(new SPQuery());
            for (int intIndex = items.Count - 1; intIndex > -1; intIndex--)
            {
                items.Delete(intIndex);
            }
        }

        #region Batch Delete
        //public static void deleteListData(SPList Olist, SPSite site)
        //{
        //    try
        //    {
        //        StringBuilder deletebuilder = BatchCommand(Olist);
        //        site.RootWeb.ProcessBatchData(deletebuilder.ToString());
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //private static StringBuilder BatchCommand(SPList spList)
        //{
        //    try
        //    {
        //        StringBuilder deletebuilder = new StringBuilder();
        //        deletebuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>");
        //        string command = "<Method><SetList Scope=\"Request\">" + spList.ID +
        //            "</SetList><SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";

        //        foreach (SPListItem item in spList.Items)
        //        {
        //            deletebuilder.Append(string.Format(command, item.ID.ToString()));
        //        }
        //        deletebuilder.Append("</Batch>");
        //        return deletebuilder;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}         
        #endregion

        #region Log

        class LogClass
        {

            public static void WriteLog(string LogText)
            {
                try
                {
                    string fullFileName = @"C:\FGITOrgChartSchedularError.txt";
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
}
