<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChartUserControl.ascx.cs" Inherits="OrganizationChart.Chart.ChartUserControl" %>

<link rel="stylesheet" href="/SiteAssets/CSS/Organisation Chart/jquery.jOrgChart.css" />
<link rel="stylesheet" href="/SiteAssets/CSS/Organisation Chart/custom.css" />

<script type="text/javascript" src="/SiteAssets/Scripts/Organisation Chart/prettify.js"></script>

<script type="text/javascript" src="/SiteAssets/Scripts/Organisation Chart/jquery.min.1.7.1.js"></script>
<script type="text/javascript" src="/SiteAssets/Scripts/Organisation Chart/jquery-ui.min.1.8.16.js"></script>

<script src="/SiteAssets/Scripts/Organisation Chart/jquery.jOrgChart.js"></script>

<script src="/SiteAssets/Scripts/Organisation Chart/OrgChart.js"></script>

<asp:Label runat="server" ID="lblResult"></asp:Label>
<div id="metadata" style="display: none">
    <asp:Label runat="server" ID="lblListName" ClientIDMode="Static"></asp:Label>
</div>

<div class="main">
    <ul id="org" runat="server" clientidmode="Static" style="display:none">
    </ul>
</div>

<div id="chart" class="orgChart"></div>

<div id="pop-up">

    <div id="product_information">
        <p id="EmpName"></p>
        <p id="EmpDesignation"></p>
        <p id="EmpDepartment"></p>
        <p id="EmpPhone"></p>
        <div class="panel_down">
            <ul>
                <li><a href="#" id="EmpViewProfile">VIEW PROFILE</a></li>
                <li><a href="#" id="EmpEmail">EMAIL</a></li>
                <li><a href="#" id="EmpConnect">CONNECT </a></li>
                <li><a href="#" id="EmpFollow">FOLLOW</a></li>
            </ul>
        </div>
    </div>

</div>

