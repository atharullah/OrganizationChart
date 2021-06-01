<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OtherChartUserControl.ascx.cs" Inherits="FinalChart.OtherChart.OtherChartUserControl" %>
<%@ Register Assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI.DataVisualization.Charting" TagPrefix="asp" %>

<asp:Label runat="server" ID="lblresult"></asp:Label>
<asp:Chart ID="Chart1" runat="server">
    <Series>        
        <asp:Series  Name="Series1" XValueMember="XVal" ChartType="StackedColumn100" YValueMembers="YVal" ></asp:Series>
        <asp:Series  Name="Series2" XValueMember="XVal" ChartType="StackedColumn100" YValueMembers="YVal" ></asp:Series>
        <asp:Series  Name="Series3" XValueMember="XVal" ChartType="StackedColumn100" YValueMembers="YVal" ></asp:Series>
        <asp:Series  Name="Series4" XValueMember="XVal" ChartType="StackedColumn100" YValueMembers="YVal" ></asp:Series>
    </Series>
    <ChartAreas>
        <asp:ChartArea Name="ChartArea1"></asp:ChartArea>
    </ChartAreas>
</asp:Chart>
