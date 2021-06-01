using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace OrganizationChart.Chart
{
    [ToolboxItemAttribute(false)]
    public class Chart : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/OrganizationChart/Chart/ChartUserControl.ascx";

        protected override void CreateChildControls()
        {
            ChartUserControl control = (ChartUserControl)Page.LoadControl(_ascxPath);
            control.webPart = this;
            Controls.Add(control);

            //Control control = Page.LoadControl(_ascxPath);
            //Controls.Add(control);
        }

        public string ListName;
        [Category("Extended Settings"),
        Personalizable(PersonalizationScope.Shared),
        WebBrowsable(true),
        WebDisplayName("List Name"),
        WebDescription("Please Choose a list name to show chart")]
        public string _ListName
        {
            get { return ListName; }
            set { ListName = value; }
        }       

        public string LogInName;
        [Category("Extended Settings"),
        Personalizable(PersonalizationScope.Shared),
        WebBrowsable(true),
        WebDisplayName("LogIn Name"),
        WebDescription("Please Choose a login name to show chart")]
        public string _LogInName
        {
            get { return LogInName; }
            set { LogInName = value; }
        }       

        public bool SameDepartment;
        [Category("Extended Settings"),
        Personalizable(PersonalizationScope.Shared),
        WebBrowsable(true),
        WebDisplayName("Only from this department"),
        WebDescription("Check if you want user from only entered department")]
        public bool _SameDepartment
        {
            get { return SameDepartment; }
            set { SameDepartment = value; }
        }
    }
}
