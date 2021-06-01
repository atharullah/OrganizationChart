using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
namespace MonthRangeChartSchedular
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (SPSite site = new SPSite(MetaData.siteUrl))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPList olist = web.Lists.TryGetList(listNames.TestTeamChart.ToString());
                        SPList chartList = web.Lists.TryGetList(listNames.ChartList.ToString());
                        if (olist != null && chartList != null)
                        {
                            SPQuery allItemQuery = new SPQuery();
                            allItemQuery.Query = string.Format("<Where><And><Neq><FieldRef Name='{0}'/><Value Type='DateTime'>{1}</Value></Neq><Eq><FieldRef Name='{2}'/><Value Type='Choice'>{3}</Value></Eq></And></Where>", sourceListColumns.TargetDate, "", sourceListColumns.Status, sourceStatusVal.Open);
                            SPListItemCollection items = olist.GetItems(allItemQuery);
                            Dictionary<string, int> monthCount = new Dictionary<string, int>(12);
                            foreach (SPListItem item in items)
                            {
                                DateTime targetDate = Convert.ToDateTime(item[sourceListColumns.TargetDate.ToString()]);
                                if (targetDate != null)
                                {
                                    string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(targetDate.Month);
                                    if (monthCount.ContainsKey(month))
                                    {
                                        int value;
                                        bool isValue = monthCount.TryGetValue(month, out value);
                                        if (isValue)
                                        {
                                            bool monthRemove = monthCount.Remove(month);
                                            if (monthRemove)
                                            {
                                                monthCount.Add(month, value + 1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        monthCount.Add(month, 1);
                                    }
                                }
                            }

                            deleteListData(chartList, site);
                            if (monthCount.Count > 0)
                            {                                
                                SPListItemCollection chartItems = chartList.GetItems(new SPQuery());
                                foreach (string reqMonth in GetMonthRange())
                                {
                                    SPListItem chartItem = olist.AddItem();
                                    chartItem[chartListColumns.Team.ToString()] = reqMonth;
                                    int count;
                                    bool isCount = monthCount.TryGetValue(reqMonth, out count);
                                    if (isCount)
                                    {
                                        chartItem[chartListColumns.WIPCount.ToString()] = count;
                                    }
                                    chartItem.Update();
                                }
                            }
                            else
                            {
                                foreach (string reqMonth in GetMonthRange())
                                {
                                    SPListItem chartItem = olist.AddItem();
                                    chartItem[chartListColumns.Team.ToString()] = reqMonth;
                                    chartItem[chartListColumns.WIPCount.ToString()] = 0;
                                    chartItem.Update();
                                }
                            }
                            Console.Write("Success");
                            Console.Read();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception is \n" + ex.Message);
                Console.Read();
            }
        }

        public static void deleteListData(SPList Olist, SPSite site)
        {
            StringBuilder deletebuilder = BatchCommand(Olist);
            site.OpenWeb().ProcessBatchData(deletebuilder.ToString());
        }

        private static StringBuilder BatchCommand(SPList spList)
        {
            StringBuilder deletebuilder = new StringBuilder();
            deletebuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>");
            string command = "<Method><SetList Scope=\"Request\">" + spList.ID +
                "</SetList><SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";

            foreach (SPListItem item in spList.Items)
            {
                deletebuilder.Append(string.Format(command, item.ID.ToString()));
            }
            deletebuilder.Append("</Batch>");
            return deletebuilder;
        }

        public static List<string> GetMonthRange()
        {
            try
            {
                string[] allMonths = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
                int currentMonth = DateTime.Now.Month;
                List<string> actualMonthsList = allMonths.Take(currentMonth - 1).ToList();
                actualMonthsList.Add(allMonths[currentMonth]);
                actualMonthsList.AddRange(allMonths.Skip(currentMonth).ToList());
                return actualMonthsList;
            }
            catch
            {
                throw;
            }
        }

        class MetaData
        {
            public static string siteUrl { get { return "http://fgit-sp-srv:3030/"; } }
        }

        enum listNames
        {
            TestTeamChart,
            ChartList
        }

        enum chartListColumns
        {
            Team,
            WIPCount
        }

        enum sourceListColumns
        {
            TargetDate,
            Status
        }

        enum sourceStatusVal
        {
            Open
        }
    }
}
