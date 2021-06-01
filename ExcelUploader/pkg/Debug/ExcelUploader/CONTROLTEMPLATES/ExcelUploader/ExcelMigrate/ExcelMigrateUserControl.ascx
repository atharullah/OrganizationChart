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
        <table>
            <tr>
                <td>
                    <asp:Label runat="server">Excel Path</asp:Label>
                </td>
                <td>
                    <%--<asp:FileUpload runat="server" ID="fileUploadExcelPath" ValidationGroup="Migration" /><span style="color: red">*</span>--%>
                    <%--<input type="file" runat="server" id="newUpload" />--%>
                    <asp:TextBox runat="server" ID="txtExcelFilePath"></asp:TextBox>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtExcelFilePath" ErrorMessage="This cant be empty" ValidationGroup="Migration" ForeColor="Red"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <tr>
                        <td>
                            <asp:Label runat="server">List Name</asp:Label>
                        </td>
                        <td>
                            <asp:DropDownList runat="server" ID="ddlListNames" ValidationGroup="Migration" OnSelectedIndexChanged="ddlListNames_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                            <span style="color: red">*</span>
                            <asp:RequiredFieldValidator runat="server" InitialValue="Select" ControlToValidate="ddlListNames" ErrorMessage="This cant be empty" ValidationGroup="Migration" ForeColor="Red"></asp:RequiredFieldValidator>

                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label runat="server">Unique Column Name</asp:Label>
                        </td>
                        <td>
                            <asp:DropDownList runat="server" ID="ddlUniqueColName" ValidationGroup="Migration"></asp:DropDownList>
                        </td>
                    </tr>
                </ContentTemplate>
            </asp:UpdatePanel>
            <tr>
                <td>
                    <asp:Label runat="server">Excel Sheet Name</asp:Label>
                </td>
                <td>
                    <asp:TextBox runat="server" ID="txtSheetName" ValidationGroup="Migration"></asp:TextBox><span style="color: red">*</span>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSheetName" ErrorMessage="This cant be empty" ValidationGroup="Migration" ForeColor="Red"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label runat="server">Log File Path</asp:Label>
                </td>
                <td>
                    <asp:FileUpload runat="server" ID="fileUploadLogFile" ValidationGroup="Migration" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSubmit" OnClick="btnSubmit_Click" Text="Submit" ValidationGroup="Migration" />
                </td>
            </tr>
        </table>
    </fieldset>
</div>
