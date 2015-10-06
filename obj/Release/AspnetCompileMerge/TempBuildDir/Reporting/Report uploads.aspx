<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="True" CodeBehind="Report uploads.aspx.cs" Inherits="linx_tablets.CustomerService.Report_uploads" %>

<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>



    <script type="@"></script>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <h1>Report uploads</h1>
                    <h2>Apple product range upload</h2>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Download existing range</th>
                            <th>Dowload blank template file</th>
                        </tr>
                        <tr>
                            <td><asp:Button ID="btnDownloadRange" runat="server" Text="Download" OnClick="btnDownloadRange_Click" /></td>
                            <td><asp:Button ID="btnDownloadRangeTemplate" runat="server" Text="Download" OnClick="btnDownloadRangeTemplate_Click" /></td>
                        </tr>
                    </table>
                    <br />
                    <asp:FileUpload ID="fupProductRange" runat="server" /><br />
                    <asp:Button ID="btnUploadProductRange" runat="server" Text="Upload" OnClick="btnUploadProductRange_Click" />

                </div>
            </div>
        </div>
    </div>
</asp:Content>
