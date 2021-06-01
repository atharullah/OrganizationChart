<%@ Assembly Name="ExcelUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9ea2cfb4f5178847" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExcelMigrateUserControl.ascx.cs" Inherits="ExcelUploader.ExcelMigrate.ExcelMigrateUserControl" %>
<asp:Label runat="server" ID="lblresult"></asp:Label>
<div>
    <fieldset>
        <legend>Migration details</legend>
        <asp:Label runat="server">Excel path</asp:Label>
        <asp:FileUpload runat="server" ID="fileUpload" />*
    <asp:RequiredFieldValidator runat="server" ControlToValidate="fileUpload" ErrorMessage="This cant be empty" ValidationGroup="Migration" ForeColor="Red"></asp:RequiredFieldValidator>
        <br />
        <asp:Label runat="server">Excel sheet name</asp:Label>
        <asp:TextBox runat="server" ID="txtSheetName"></asp:TextBox>*
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSheetName" ErrorMessage="This cant be empty" ValidationGroup="Migration" ForeColor="Red"></asp:RequiredFieldValidator>
        <br />
        <asp:Label runat="server">List Name</asp:Label>
        <asp:TextBox runat="server" ID="txtListName"></asp:TextBox>*
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtListName" ErrorMessage="This cant be empty" ValidationGroup="Migration" ForeColor="Red"></asp:RequiredFieldValidator>
        <br />
        <asp:Button runat="server" ID="btnSubmit" OnClick="btnSubmit_Click" Text="Submit" ValidationGroup="Migration" />
    </fieldset>
</div>
