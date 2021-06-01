using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using System.Collections.Specialized;

namespace ChartSchedular
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
                        SPList olistA = web.Lists.TryGetList(listNames.TestTeam1.ToString());
                        SPList oListB = web.Lists.TryGetList(listNames.TestTeamChart.ToString());

                        if (olistA != null && oListB != null)
                        {
                            SPFieldChoice teamChoiceA = (SPFieldChoice)olistA.Fields[sourceListColumns.TeamName.ToString()];
                            StringCollection teamsA = teamChoiceA.Choices;
                            SPFieldChoice teamChoiceB = (SPFieldChoice)oListB.Fields[sourceListColumns.TeamName.ToString()];
                            StringCollection teamsB = teamChoiceB.Choices;

                            List<string> Teams = new List<string>();
                            AddChoices(teamsA, ref Teams);
                            AddChoices(teamsB, ref Teams);
                            Teams.Distinct();
                            Teams.Sort();

                            SPList chartList = web.Lists.TryGetList(listNames.ChartList.ToString());
                            if (chartList != null)
                            {
                                SPListItemCollection items;
                                foreach (string team in Teams)
                                {
                                    SPQuery query = new SPQuery();
                                    query.Query = string.Format("<Where><Eq><FieldRef Name='{0}'/><Value Type='Text'>{1}</Value></Eq></Where>", chartListColumns.Team.ToString(), team);
                                    items = chartList.GetItems(query);
                                    if (items.Count == 0)
                                    {
                                        SPListItem chartitem = chartList.AddItem();
                                        chartitem[chartListColumns.Team.ToString()] = team;
                                        chartitem[chartListColumns.WIPCount.ToString()] = 0;
                                        chartitem.Update();
                                    }
                                    else
                                    {
                                        SPQuery countQuery = new SPQuery();
                                        countQuery.Query = string.Format("<Where><And><Eq><FieldRef Name='{0}'/><Value Type='Choice'>{1}</Value></Eq><Eq><FieldRef Name='{2}' /><Value Type='Choice'>{3}</Value></Eq></And></Where>", sourceListColumns.TeamName, team, sourceListColumns.Status, sourceStatusVal.WIP);
                                        int WIPCount = olistA.GetItems(countQuery).Count + oListB.GetItems(countQuery).Count;
                                        foreach (SPListItem item in items)
                                        {
                                            item[chartListColumns.WIPCount.ToString()] = WIPCount;
                                            item.Update(); 
                                        }
                                    }
                                }
                                Console.Write("Success");
                                Console.Read();
                            }
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

        public static void AddChoices(StringCollection inputs, ref List<string> Teams)
        {
            try
            {
                foreach (string team in inputs)
                {
                    if (!Teams.Contains(team))
                    {
                        Teams.Add(team);
                    }
                }
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
            TestTeam1,
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
            TeamName,
            Status
        }

        enum sourceStatusVal
        {
            WIP
        }        
    }
}
