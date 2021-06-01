using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Administration;

namespace ExcelUploader.ExcelMigrate
{
    public partial class ExcelMigrateUserControl : UserControl
    {
        const string SELECT = "Select";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    using (SPSite site = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            ddlUniqueColName.Items.Add(SELECT);
                            SPListCollection lists = web.Lists;
                            var listCollection = from SPList lst in lists
                                                 where lst.Hidden == false && lst.BaseTemplate == SPListTemplateType.GenericList
                                                 select lst;
                            ddlListNames.Items.Add(SELECT);
                            foreach (SPList list in listCollection)
                            {
                                ddlListNames.Items.Add(list.Title);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(txtLogPath.Text))
                {
                    LogClass.fullFileName = txtLogPath.Text;
                }
                LogClass.WriteLog(ex.Message);
                LogClass.LogMessage(ex);
                lblresult.Text += "Other erro<br/>";
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtExcelFilePath.Text != null && !string.IsNullOrEmpty(ddlListNames.SelectedValue) && !string.IsNullOrEmpty(txtSheetName.Text))
                {
                    string filePath = txtExcelFilePath.Text;
                    string ext = Path.GetExtension(filePath);
                    lblresult.Text += "extension is " + ext + "\n file name " + filePath;
                    if (ext == ".xlsx")
                    {
                        if (excelFileOpen(filePath))
                        {
                            List<string> excelColumns = getExcelColumns(filePath);
                            using (SPSite site = new SPSite(SPContext.Current.Web.Url))
                            {
                                using (SPWeb web = site.OpenWeb())
                                {
                                    SPList olist = web.Lists.TryGetList(ddlListNames.SelectedValue);

                                    if (olist != null)
                                    {
                                        lblresult.Text += "Find list " + olist.Title;

                                        List<string> listColNames = getColumnNames(olist);
                                        if (checkColumns(listColNames, excelColumns))
                                        {
                                            lblresult.Text += "Column Same";
                                            DataTable excelData = ConvertExcelInToDataTable(filePath, txtSheetName.Text, false, false);
                                            lblresult.Text += "Fetch excel data";
                                            SPListItemCollection listItems = olist.GetItems(new SPQuery());
                                            string uniqueColumn = ddlUniqueColName.SelectedValue;
                                            if (!string.IsNullOrEmpty(uniqueColumn) && uniqueColumn != SELECT)
                                            {
                                                lblresult.Text += "In unique column";
                                                foreach (DataRow row in excelData.Rows)
                                                {
                                                    lblresult.Text += "In rows";
                                                    string uniqueColValue = Convert.ToString(row[ddlUniqueColName.SelectedItem.Text]);
                                                    lblresult.Text += "Uniq col value";
                                                    var reqField = olist.Fields.GetFieldByInternalName(uniqueColumn);
                                                    var filterColumnItems = (from SPListItem filter in listItems
                                                                             where reqField.GetFieldValueAsText(filter[reqField.Id]) == uniqueColValue
                                                                             select filter).FirstOrDefault();
                                                    lblresult.Text += "Filter column";
                                                    if (filterColumnItems != null)
                                                    {
                                                        lblresult.Text += "Count gt " + 0;
                                                        bool flag = Convert.ToBoolean(filterColumnItems[listColumns.flag.ToString()]);
                                                        if (flag)
                                                        {
                                                            updateItem(filterColumnItems, excelData, row);
                                                            lblresult.Text += "Item update";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        lblresult.Text += "Count lt 0";
                                                        SPListItem newitem = listItems.Add();
                                                        updateItem(newitem, excelData, row);
                                                        lblresult.Text += "All Item update";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lblresult.Text += "Not unique column";
                                                foreach (DataRow row in excelData.Rows)
                                                {
                                                    SPListItem newitem = listItems.Add();
                                                    updateItem(newitem, excelData, row);
                                                }
                                                lblresult.Text += "Items inserted";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lblresult.Text += "List not found\n";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        lblresult.Text += "File is not able to open or extension is not xlsx\n";
                    }
                }
                else
                {
                    lblresult.Text += "File or list name should not be empty\n";
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(txtLogPath.Text))
                {
                    LogClass.fullFileName = txtLogPath.Text;
                }
                LogClass.WriteLog(ex.Message);
                LogClass.LogMessage(ex);
                lblresult.Text += "Other erro<br/>";
            }
        }

        private void updateItem(SPListItem item, DataTable excelData, DataRow row)
        {
            try
            {
                lblresult.Text += "In Item update<br/>";
                foreach (DataColumn column in excelData.Columns)
                {
                    string columnVal = Convert.ToString(row[column]);
                    string internalname = ddlUniqueColName.Items.FindByText(column.ColumnName).Value;
                    if (!string.IsNullOrEmpty(columnVal))
                    {
                        item[internalname] = columnVal;
                        item[listColumns.Manager.ToString()] = getManager();
                    }
                }

                item.Update();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private bool excelFileOpen(string filePath)
        {
            bool open = true;
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (System.IO.File.Open(filePath, FileMode.Open)) { lblresult.Text += "File Open\n"; }
                });
            }
            catch (Exception ex)
            {
                lblresult.Text += "Opening error\n" + ex;
                open = false;
            }
            return open;
        }

        private List<string> getExcelColumns(string filePath)
        {
            lblresult.Text += "In getExcelColumns <br/>";
            List<string> list = new List<string>();
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    DataTable dt = ConvertExcelInToDataTable(filePath, txtSheetName.Text, false, true);
                    foreach (DataColumn col in dt.Columns)
                    {
                        list.Add(Convert.ToString(col.ColumnName));
                    }
                });
            }
            catch (System.Exception)
            {
                throw;
            }
            return list;
            lblresult.Text += "Out getExcelColumns <br/>";
        }

        private DataTable ConvertExcelInToDataTable(string excelFilePath, string SheetName, bool addHeader, bool oneRow)
        {

            lblresult.Text += "In Conversion";
            DataTable excelData = new DataTable();
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        string con = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelFilePath + ";Extended Properties=Excel 12.0;";
                        lblresult.Text += "before connection";
                        using (OleDbConnection connection = new OleDbConnection(con))
                        {
                            connection.Open();
                            lblresult.Text += "connection open";
                            if (!oneRow)
                            {
                                lblresult.Text += "One row";
                                OleDbCommand command = new OleDbCommand("select * from [" + SheetName + "$]", connection);
                                using (OleDbDataReader dr = command.ExecuteReader())
                                {
                                    excelData.Load(dr);
                                }
                            }
                            else
                            {
                                lblresult.Text += "Multiple";
                                OleDbCommand command = new OleDbCommand("select top 1 * from [" + SheetName + "$]", connection);
                                using (OleDbDataReader dr = command.ExecuteReader())
                                {
                                    excelData.Load(dr);
                                }
                            }
                            if (addHeader)
                            {
                                lblresult.Text += "Header";
                                // in some excel header column is missing, to creater header with the first record of excel sheet, so that the row is considered in Foreach loop
                                DataRow row = excelData.NewRow();
                                for (int i = 0; i < excelData.Columns.Count; i++)
                                {
                                    row[i] = excelData.Columns[i].ColumnName;
                                }
                                excelData.Rows.InsertAt(row, 0);
                            }
                            else
                            {
                                //do nothing
                            }
                        }
                    });
            }
            catch
            {
                lblresult.Text += "Exception In Conversion <br/>";
                throw;
            }
            return excelData;
        }

        private bool checkColumns(List<string> getListColNames, List<string> getExcelColNames)
        {
            bool equal = true;
            try
            {
                foreach (string Excelitem in getExcelColNames)
                {
                    if (!getListColNames.Contains(Excelitem))
                    {
                        equal = false;
                    }
                    else
                    {
                        //do nothing
                    }

                }
            }
            catch (System.Exception)
            {
                throw;
            }
            return equal;
        }

        private List<string> getColumnNames(SPList Olist)
        {
            List<string> list = new System.Collections.Generic.List<string>();
            var fileterFields = from SPField f in Olist.Fields
                                where f.FromBaseType == false
                                select f;
            try
            {
                foreach (SPField field in fileterFields)
                {
                    list.Add(Convert.ToString(field.Title));
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            return list;
        }

        protected void ddlListNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddllist = (DropDownList)sender;
            string listName = ddllist.SelectedValue;
            if (!string.IsNullOrEmpty(listName) && listName != SELECT)
            {
                using (SPSite site = new SPSite(SPContext.Current.Web.Url))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPList olist = web.Lists.TryGetList(listName);
                        if (olist != null)
                        {
                            SPFieldCollection fields = olist.Fields;
                            var fieldsCollections = from SPField f in fields
                                                    where f.FromBaseType == false
                                                    select f;
                            foreach (SPField field in fieldsCollections)
                            {
                                ListItem item = new ListItem();
                                item.Text = field.Title;
                                item.Value = field.InternalName;
                                ddlUniqueColName.Items.Add(item);
                            }
                        }
                    }
                }
            }
            else
            {
                ddlUniqueColName.Items.Clear();
                ddlUniqueColName.Items.Add(SELECT);
            }
        }

        public string getManager(string userName)
        {
           SPList oList= _currentWeb.Lists.TryGetList(listNames.UserProfile.ToString());
            SPQuery query=new SPQuery();
            query.Query=string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where>",listUserProfile.Name,userName);
            SPListItemCollection items = oList.GetItems(query);
            if (items.Count > 0)
            {
                string managerVal = Convert.ToString(items[0][listUserProfile.Name.ToString()]);
                return managerVal;
            }
            else
                return null;
        }

        public SPWeb _currentWeb
        {
            get
            {
                using (SPSite site = new SPSite(SPContext.Current.Web.Url))
                {
                    return site.OpenWeb();
                }
            }
        }

        public SPWeb _rootWeb
        {
            get
            {
                using (SPSite site = new SPSite(SPContext.Current.Web.Url))
                {
                    return site.RootWeb;
                }
            }
        }

        enum listColumns
        {
            flag,
            Manager
        }

        enum listUserProfile
        {
            Name
        }

        enum listNames
        {
            UserProfile
        }

        #region Log

        class LogClass
        {
            public static string fullFileName = @"C:\FGITError.txt";

            public static void WriteLog(string LogText)
            {
                try
                {

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

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        try
    //        {
    //            if (System.IO.File.Exists(fileUpload.PostedFile.FileName))
    //            {

    //            }
    //            else
    //            {
    //                Console.WriteLine(ConfigurationManager.AppSettings[MSG_FILEABSENT] + ConfigurationManager.AppSettings[EXCELFILENAME_NOPATH] + ConfigurationManager.AppSettings[MSG_RESTART]);
    //                addLogToFile(ConfigurationManager.AppSettings[MSG_FILEABSENT] + ConfigurationManager.AppSettings[EXCELFILENAME_NOPATH] + ConfigurationManager.AppSettings[MSG_RESTART]);
    //                Console.WriteLine(ConfigurationManager.AppSettings[MSG_PRESSANYKEY]);
    //                Console.ReadKey();
    //                Environment.Exit(0);
    //            }

    //            Console.ReadKey();
    //        }
    //        catch (Exception ex)
    //        {
    //            WriteError(ex);
    //            Console.WriteLine(ConfigurationManager.AppSettings[MSG_PRESSANYKEY]);
    //            Console.ReadKey();
    //            Environment.Exit(0);
    //        }
    //    }

    //    #region Methods

    //    //Added on 24/3/15 by Nafe


    //    private static bool AddDocument(ClientContext context, List docs, List<string> getReqColNames)
    //    {
    //        bool error = false;
    //        try
    //        {
    //            DataTable dt = ConvertExcelInToDataTable(CurrentDirectory + ConfigurationManager.AppSettings[EXCELFILENAME_NOPATH], ConfigurationManager.AppSettings[EXCELSHEETNAME], false, false);
    //            List<string> excelColumn = getExcelColumns();
    //            int i = 1;
    //            int j = dt.Rows.Count;
    //            int k = 0;
    //            foreach (DataRow row in dt.Rows)
    //            {
    //                FileCreationInformation newFile = new FileCreationInformation();
    //                i += 1;

    //                try
    //                {
    //                    if (!filePresent(context, docs, Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])))
    //                    {
    //                        bool isFilePresent = true;
    //                        string requiredColName = string.Empty;

    //                        newFile.Content = System.IO.File.ReadAllBytes(CurrentDirectory +
    //                            "\\" + Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]]));
    //                        newFile.Url = Path.GetFileName(CurrentDirectory + "\\" +
    //                            Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]]));
    //                        newFile.Overwrite = true;
    //                        context.Load(docs.RootFolder.Files);
    //                        context.ExecuteQuery();
    //                        Microsoft.SharePoint.Client.File uploadFile = docs.RootFolder.Files.Add(newFile);
    //                        context.Load(uploadFile);
    //                        context.ExecuteQuery();
    //                        //string g = uploadFile.ServerRelativeUrl;
    //                        context.Load(uploadFile.ListItemAllFields);

    //                        var spTimeZone = context.Web.RegionalSettings.TimeZone;
    //                        context.Load(spTimeZone);

    //                        context.ExecuteQuery();
    //                        foreach (string col in excelColumn)
    //                        {
    //                            if (!string.IsNullOrEmpty(Convert.ToString(row[col]))) //Change on 30-Mar-15
    //                            {
    //                                Type type = row[col].GetType();
    //                                if (type == typeof(DateTime))
    //                                {
    //                                    //Resolve System.TimeZoneInfo from Microsoft.SharePoint.Client.TimeZone 
    //                                    var fixedTimeZoneName = spTimeZone.Description.Replace("and", "&");
    //                                    var timeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.DisplayName == fixedTimeZoneName);

    //                                    uploadFile.ListItemAllFields[col] = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(row[col]), timeZoneInfo);
    //                                }
    //                                else
    //                                {
    //                                    uploadFile.ListItemAllFields[col] = row[col];
    //                                    //uploadFile.ListItemAllFields[col] = Convert.ToString(row[col]);
    //                                }
    //                            }
    //                            else
    //                            {
    //                                if (!getReqColNames.Contains(col)) //Change on 30-Mar-15
    //                                {
    //                                    string fieldType = getFieldType(col);
    //                                    if (!fieldType.Contains(typeof(DateTime).Name))
    //                                    {
    //                                        //uploadFile.ListItemAllFields[col] = Convert.ToString(row[col]);
    //                                        uploadFile.ListItemAllFields[col] = row[col];
    //                                    }
    //                                    else
    //                                    {
    //                                        //do nothing
    //                                    }
    //                                }
    //                                else //Change on 26-Feb-15
    //                                {
    //                                    deleteUploadedDoc(docs, newFile.Url, context);
    //                                    requiredColName = col;
    //                                    isFilePresent = false;
    //                                    break;
    //                                    //Console.WriteLine(ConfigurationManager.AppSettings[MSG_FILEPRESENT1] +
    //                                    //    Path.GetFileName(CurrentDirectory + "\\" +
    //                                    //    Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]]))+
    //                                    //    ConfigurationManager.AppSettings[MSG_FILEPRESENT1] + Convert.ToString(row[col])
    //                                    //    );
    //                                }
    //                            }

    //                        }

    //                        if (isFilePresent) //Change on 26-Feb-15
    //                        {
    //                            uploadFile.ListItemAllFields.Update();
    //                            context.Load(uploadFile.ListItemAllFields);
    //                            context.ExecuteQuery();
    //                            k = k + 1;
    //                            Console.WriteLine(ConfigurationManager.AppSettings[MSG_FILEPRESENT1] + Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])) + ConfigurationManager.AppSettings[MSG_FILEMIGRATED]);
    //                            addLogToFile(ConfigurationManager.AppSettings[MSG_FILEPRESENT1] + Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])) + ConfigurationManager.AppSettings[MSG_FILEMIGRATED]);
    //                            setItemLevelPermission(docs.Title, uploadFile.ListItemAllFields.Id, context);
    //                        }
    //                        else //Change on 26-Feb-15
    //                        {
    //                            string errorMsg = string.Format(ConfigurationManager.AppSettings[MSG_FILENOTUPLOADED], Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])), requiredColName, i - 1);
    //                            Console.WriteLine(errorMsg);
    //                            addLogToFile(errorMsg);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        Console.WriteLine(ConfigurationManager.AppSettings[MSG_FILEPRESENT1] +
    //                        Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])) +
    //                        ConfigurationManager.AppSettings[MSG_FILEPRESENT2]);

    //                        addLogToFile(ConfigurationManager.AppSettings[MSG_FILEPRESENT1] +
    //                        Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])) +
    //                        ConfigurationManager.AppSettings[MSG_FILEPRESENT2]);
    //                    }
    //                }
    //                catch (Exception e)
    //                {
    //                    error = true;
    //                    WriteError(e);
    //                    Console.WriteLine(ConfigurationManager.AppSettings[MSG_COPYROW] + (i - 1));
    //                    addLogToFile(ConfigurationManager.AppSettings[MSG_COPYROW] + (i - 1));
    //                    Console.WriteLine(ConfigurationManager.AppSettings[MSG_ERRORMIGRATED] + Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])));
    //                    addLogToFile(ConfigurationManager.AppSettings[MSG_ERRORMIGRATED] + Path.GetFileName(Convert.ToString(row[ConfigurationManager.AppSettings[COLNAMEDOCUMENTLOCATION]])));
    //                    deleteUploadedDoc(docs, newFile.Url, context);
    //                }

    //            }


    //            if (error)
    //            {
    //                Console.WriteLine(ConfigurationManager.AppSettings[MSG_COMPLETEDOUT1] + k + ConfigurationManager.AppSettings[MSG_COMPLETEDOUT2] + j + ConfigurationManager.AppSettings[MSG_COMPLETEDOUT3]);
    //                addLogToFile(ConfigurationManager.AppSettings[MSG_COMPLETEDOUT1] + k + ConfigurationManager.AppSettings[MSG_COMPLETEDOUT2] + j + ConfigurationManager.AppSettings[MSG_COMPLETEDOUT3]);
    //                Console.WriteLine(ConfigurationManager.AppSettings[MSG_DONE_ERROR1]);
    //                addLogToFile(ConfigurationManager.AppSettings[MSG_DONE_ERROR1]);
    //                Console.WriteLine(ConfigurationManager.AppSettings[MSG_DONE_ERROR2] + CurrentDirectory + "\\ErrorsLogs\\MigrationException" + DateTime.Today.ToString("yyyy_MM_dd") + ".txt");
    //                addLogToFile(ConfigurationManager.AppSettings[MSG_DONE_ERROR2] + CurrentDirectory + "\\ErrorsLogs\\MigrationException" + DateTime.Today.ToString("yyyy_MM_dd") + ".txt");

    //            }
    //            else
    //            {
    //                //do nothing
    //            }

    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //        return error;
    //    }        

    //    private static void deleteUploadedDoc(List docs, string url, ClientContext context)
    //    {
    //        try
    //        {
    //            addLogToFile("In Delete Code");
    //            Web web = context.Web;
    //            Microsoft.SharePoint.Client.File f = docs.RootFolder.Files.GetByUrl(url);
    //            addLogToFile("deleting document: ");
    //            context.Load(f);
    //            f.DeleteObject();
    //            context.ExecuteQuery(); // Delete file here but throw Exception 
    //            addLogToFile("deleted document");

    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }        

    //    private static List getLibrary(ClientContext context, out bool myAddPermission)
    //    {
    //        List docLib = null;
    //        myAddPermission = false;
    //        try
    //        {
    //            docLib = context.Web.Lists.GetByTitle(ConfigurationManager.AppSettings[SPLIBRARYNAME]);
    //            context.Load(docLib);
    //            context.ExecuteQuery();
    //            context.Load(docLib, l => l.EffectiveBasePermissions);
    //            context.ExecuteQuery();

    //            myAddPermission = docLib.EffectiveBasePermissions.Has(PermissionKind.AddListItems);
    //        }
    //        catch (System.Exception)
    //        {
    //            Console.WriteLine(ConfigurationManager.AppSettings[MSG_NOLIBRARY] + ConfigurationManager.AppSettings[SPLIBRARYNAME]);
    //            addLogToFile(ConfigurationManager.AppSettings[MSG_NOLIBRARY] + ConfigurationManager.AppSettings[SPLIBRARYNAME]);
    //            Console.WriteLine(ConfigurationManager.AppSettings[MSG_PRESSANYKEY]);
    //            Console.ReadKey();
    //            Environment.Exit(0);
    //        }
    //        return docLib;
    //    }





    //    private static List<string> getReqColumnNames(List docs, ClientContext context)
    //    {
    //        List<string> listReq = new System.Collections.Generic.List<string>();
    //        FieldCollection fields = docs.Fields;
    //        var query = from f in fields
    //                    where f.Required == true
    //                    select f;

    //        foreach (Field item in query)
    //        {
    //            if (item.Title != "Name")
    //            {
    //                context.Load(item);
    //                context.ExecuteQuery();
    //                listReq.Add(Convert.ToString(item.Title));
    //            }
    //            else
    //            {
    //                //do nothing
    //            }
    //        }

    //        //IEnumerable<Field> filteredFields = context.Load(fields.Where(f => f.Required == true));
    //        //context.ExecuteQuery();
    //        //foreach (Field field in filteredFields)
    //        //{
    //        //    listReq.Add(Convert.ToString(field.Title));
    //        //}

    //        return listReq;
    //    }

    //    private static bool checkReqColumnsInExcel(List<string> getLibColNames, List<string> getExcelColNames)
    //    {
    //        bool equal = true;

    //        try
    //        {
    //            foreach (string Reqitem in getLibColNames)
    //            {
    //                if (!getExcelColNames.Contains(Reqitem))
    //                {
    //                    equal = false;
    //                    Console.WriteLine(ConfigurationManager.AppSettings[MSG_COLUMNREQ1] + Reqitem + ConfigurationManager.AppSettings[MSG_COLUMNREQ2]);
    //                    addLogToFile(ConfigurationManager.AppSettings[MSG_COLUMNREQ1] + Reqitem + ConfigurationManager.AppSettings[MSG_COLUMNREQ2]);
    //                }
    //                else
    //                {
    //                    //do nothing
    //                }

    //            }
    //        }
    //        catch (System.Exception)
    //        {
    //            throw;
    //        }
    //        return equal;
    //    }

    //    private static bool checkColumns(List<string> getLibColNames, List<string> getExcelColNames)
    //    {
    //        bool equal = true;

    //        try
    //        {
    //            foreach (string Excelitem in getExcelColNames)
    //            {
    //                if (!getLibColNames.Contains(Excelitem))
    //                {
    //                    equal = false;
    //                    Console.WriteLine(ConfigurationManager.AppSettings[MSG_COLUMN1] + Excelitem + ConfigurationManager.AppSettings[MSG_COLUMN2]);
    //                    addLogToFile(ConfigurationManager.AppSettings[MSG_COLUMN1] + Excelitem + ConfigurationManager.AppSettings[MSG_COLUMN2]);
    //                }
    //                else
    //                {
    //                    //do nothing
    //                }

    //            }
    //        }
    //        catch (System.Exception)
    //        {
    //            throw;
    //        }
    //        return equal;
    //    }

    //    private static string getFieldType(string fieldName)
    //    {
    //        string fieldType = string.Empty;

    //        try
    //        {
    //            bool isAuthorized = false;
    //            ClientContext context = getContext();
    //            List docLib = getLibrary(context, out isAuthorized);

    //            FieldCollection fieldCol = docLib.Fields;
    //            context.Load(fieldCol);
    //            context.ExecuteQuery();

    //            Field field = (from Field f in fieldCol
    //                           where f.InternalName.ToUpper() == fieldName.ToUpper()
    //                           select f).FirstOrDefault();

    //            if (field != null)
    //            {
    //                fieldType = field.GetType().Name;
    //            }
    //            else
    //            {
    //                //do nothing
    //            }
    //        }
    //        catch
    //        {
    //            throw;
    //        }

    //        return fieldType;
    //    }

    //    #endregion

    //    #region Exception

    //    protected static void addLogToFile(string logMessage)
    //    {
    //        try
    //        {
    //            string path = CurrentDirectory + "\\Logs\\" + "MigrationLogs" + fileNames + ".txt";
    //            string subPath = CurrentDirectory;
    //            bool isExists = System.IO.Directory.Exists(subPath);
    //            if (!isExists)
    //                System.IO.Directory.CreateDirectory(subPath);
    //            if (!System.IO.File.Exists(path))
    //            {
    //                System.IO.File.Create(path).Close();
    //            }
    //            using (StreamWriter w = System.IO.File.AppendText(path))
    //            {
    //                //w.WriteLine("\r\nLog Entry : ");
    //                //w.WriteLine("{0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
    //                w.WriteLine("\r\nLog Entry : {0}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
    //                string err = "Log Message: " + "" +
    //                      logMessage;
    //                w.WriteLine(err);
    //                w.WriteLine("__________________________");
    //                w.Flush();
    //                w.Close();
    //            }
    //        }
    //        catch (Exception)
    //        {
    //            //WriteError(ex.Message);
    //        }
    //    }


    //    private static string GetExceptionMessage(Exception e)
    //    {
    //        StringBuilder message = new StringBuilder();
    //        bool flag = false;
    //        string strInnerExeption;
    //        while (e != null)
    //        {
    //            if (flag)
    //            {
    //                strInnerExeption = "Inner Exception";
    //            }
    //            else
    //            {
    //                strInnerExeption = string.Empty;
    //            }
    //            // Get stack trace for the exception with source file information
    //            var st = new StackTrace(e, true);
    //            // Get the top stack frame
    //            var frame = st.GetFrame(st.FrameCount - 1);
    //            // Get the line number from the stack frame
    //            var line = frame.GetFileLineNumber();
    //            string method = frame.GetMethod().ToString();
    //            int line1 = frame.GetFileLineNumber();
    //            message.Append(Environment.NewLine);
    //            message.Append("----------" + strInnerExeption + " Exception----------");
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\nType : " + e.GetType().FullName);
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\nMessage  : " + e.Message);
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\nSource : " + e.Source);
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\nTargetSite : " + e.TargetSite);
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\nLine Number : " + line + " : " + method);
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\nStack Trace : \n" + e.StackTrace);
    //            message.Append(Environment.NewLine);
    //            message.Append("\t\r\n----------" + strInnerExeption + " Exception----------");
    //            message.Append(Environment.NewLine);
    //            flag = true;
    //            e = e.InnerException;
    //        }
    //        return message.ToString();
    //    }

    //    /// Handles error by accepting the error message
    //    /// Displays the page on which the error occured

    //    protected static void WriteError(Exception e)
    //    {
    //        try
    //        {
    //            string errorMessage = GetExceptionMessage(e);
    //            addToFile(errorMessage);
    //            //int errorWriteKey = Convert.ToInt32(ConfigurationManager.AppSettings[ERRORWRITEKEY]);
    //            //switch (errorWriteKey)
    //            //{
    //            //    case 1: addToFile(errorMessage);
    //            //        break;
    //            //    case 2: addToErrorLogList(errorMessage);
    //            //        break;
    //            //    case 3: addToFile(errorMessage);
    //            //        addToErrorLogList(errorMessage);
    //            //        break;
    //            //}
    //        }
    //        catch (Exception)
    //        {
    //        }
    //    }

    //    protected static void addToFile(string errorMessage)
    //    {
    //        try
    //        {
    //            string path = CurrentDirectory + "\\ErrorsLogs\\" + "MigrationException" + fileNames + ".txt";
    //            string subPath = CurrentDirectory;
    //            bool isExists = System.IO.Directory.Exists(subPath);
    //            if (!isExists)
    //                System.IO.Directory.CreateDirectory(subPath);
    //            if (!System.IO.File.Exists(path))
    //            {
    //                System.IO.File.Create(path).Close();
    //            }
    //            using (StreamWriter w = System.IO.File.AppendText(path))
    //            {
    //                //w.WriteLine("\r\nLog Entry : ");
    //                //w.WriteLine("{0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
    //                w.WriteLine("\r\nLog Entry : {0}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
    //                string err = "Error in: " + "" +
    //                     ". Error Message:" + errorMessage;
    //                w.WriteLine(err);
    //                w.WriteLine("__________________________");
    //                w.Flush();
    //                w.Close();
    //            }
    //        }
    //        catch (Exception)
    //        {
    //            //WriteError(ex.Message);
    //        }
    //    }

    //    //protected void addToErrorLogList(string errorMessage)
    //    //{
    //    //    try
    //    //    {
    //    //        List errorList = clientContext.Web.Lists.GetByTitle(ERRORLIST);

    //    //        ListItemCreationInformation listItemCreationInformation = new ListItemCreationInformation();

    //    //        ListItem item = errorList.AddItem(listItemCreationInformation);
    //    //        item.Update();
    //    //        clientContext.ExecuteQuery();

    //    //        item[ERRORLISTCOLUMN] = errorMessage;

    //    //        item.Update();
    //    //        clientContext.ExecuteQuery();
    //    //    }
    //    //    catch (Exception)
    //    //    {
    //    //        //throw;
    //    //    }
    //    //}
    //    #endregion
    //}
}
