using Microsoft.SharePoint;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.DataVisualization;
using System.Drawing;
using System.Data;
namespace FinalChart.OtherChart
{
    public partial class OtherChartUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    using(SPSite site=new SPSite("http://fgit-sp-srv:3030"))
                    {
                        using(SPWeb web=site.OpenWeb())
                        {
                            SPList olist = web.Lists.TryGetList("Test");
                            if(olist!=null)
                            {
                                DataTable table = new DataTable("MainTable");
                                table.Columns.Add("XVal");
                                table.Columns.Add("YVal");
                                table.Rows.Add("A","10");
                                table.Rows.Add("B", "10");
                                table.Rows.Add("C", "10");
                                table.Rows.Add("D", "10");
                                
                                Chart1.DataSource = table;
                                Chart1.DataBind();

                                Series s1 = Chart1.Series.FindByName("Series1");
                                Series s2 = Chart1.Series.FindByName("Series2");
                                Series s3 = Chart1.Series.FindByName("Series3");
                                Series s4 = Chart1.Series.FindByName("Series4");

                                Color[] s1Colors = { Color.Red, Color.Blue, Color.Black, Color.Green };
                                Color[] s2Colors = { Color.Wheat, Color.Violet, Color.Yellow, Color.SlateBlue };
                                Color[] s3Colors = { Color.Silver, Color.RoyalBlue, Color.PowderBlue, Color.Purple };
                                Color[] s4Colors = { Color.Pink, Color.Orchid, Color.Olive, Color.MistyRose };

                                for(int i=0;i<s1.Points.Count;i++)
                                {
                                    s1.Points[i].Color = s1Colors[i];
                                }
                                for (int i = 0; i < s2.Points.Count; i++)
                                {
                                    s2.Points[i].Color = s2Colors[i];
                                }
                                for (int i = 0; i < s3.Points.Count; i++)
                                {
                                    s3.Points[i].Color = s3Colors[i];
                                }
                                for (int i = 0; i < s4.Points.Count; i++)
                                {
                                    s4.Points[i].Color = s4Colors[i];
                                }

                                s1.AxisLabel = "S1";
                                s1.Label = "1";
                                s1.YValueType = ChartValueType.String;

                                s2.AxisLabel = "S2";
                                s2.Label = "2";
                                s2.YValueType = ChartValueType.String;

                                s3.AxisLabel = "S3";
                                s3.Label = "3";
                                s3.YValueType = ChartValueType.String;

                                s4.AxisLabel = "S4";
                                s4.Label = "4";
                                s4.YValueType = ChartValueType.String;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }
    }
}
