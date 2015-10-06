<%@ Page Language="C#" MasterPageFile="~/Site.Master" MaintainScrollPositionOnPostback="true" AutoEventWireup="True" CodeBehind="AppleReporting.aspx.cs" Inherits="linx_tablets.CustomerService.AppleReporting" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <script type="@"></script>
    <div class="container">
        <div class="row">
            <div class="col-lg-12 download-page">
                <div class="row">
                    <asp:SqlDataSource ID="sqlDSOrderStatus" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select OrderStatusDesc,OrderStatusID from MSE_Orderstatus "></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSInventoryReports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select ir.ReportID,lm.PlantCode,lm.PlantDescription,ir.Filename,ir.DateCreated 
from MSE_AppleInventoryreports ir inner join MSE_AppleLocaleMapping lm on lm.LocaleID=ir.localeid
order by datecreated desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSForecastCommitReports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select fc.ReportID,fc.Filename,fc.ReportDate 
from MSE_AppleForecastCommitReports fc
order by ReportDate desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSForecastDiscrepancieReports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select fcr.DiscrepancieReportID,src.ReportID as SourceReportID, src.FileName as SourceFilename, src.ReportDate as SourceReportDate
,prv.ReportID as PreviousReportID, prv.FileName as PreviousFilename, prv.ReportDate as PreviousReportDate
 from mse_appleforecastcommitreportdiscrepencies fcr 
inner join MSE_AppleForecastCommitReports src on src.reportid=fcr.currentreportid
inner join MSE_AppleForecastCommitReports prv on prv.reportid=fcr.PreviousReportID"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDsVMIReportSource" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct vmi.* from mse_applevmidatareports vmi inner join mse_applevmidatareportlines vmil on vmil.VMIReportID=vmi.VMIReportID
order by datecreated desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDsFCReportSource" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select distinct fc.* from MSE_AppleForecastCommitReports fc inner join MSE_AppleForecastCommitReportLines fcl on fcl.ReportID=fc.ReportID
order by reportdate desc"></asp:SqlDataSource>

                    <asp:SqlDataSource ID="sqlDSInventoryreportsLatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select * from  (select ROW_NUMBER() OVER(PARTITION BY localeid ORDER BY datecreated desc) AS Row,* 
from MSE_AppleInventoryreports
) as a inner join MSE_AppleLocaleMapping lm on lm.localeid=a.LocaleID
where a.Row=1 order by a.datecreated desc"></asp:SqlDataSource>

                    <asp:SqlDataSource ID="sqlDSVMILatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select top 1 * from mse_applevmidatareports order by datecreated desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSFCLatest" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select top 1 * from MSE_AppleForecastCommitReports fc order by fc.ReportDate desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSReplenEntries" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select replenid,DateGenerated,SentToOracleDate,SentToOracleFilename,case when SentToOracle =1 then 'Yes' else 'No' end as SentToOracle,createdby,LocaleReplen,lm.LocaleID,lm.plantcode,lm.ExertisOutAccount
from mse_applereplens ar inner join mse_applelocalemapping lm on lm.localeid=ar.LocaleID
order by DateGenerated desc"></asp:SqlDataSource>
                    <asp:SqlDataSource ID="sqlDSorscleLastRunImports" runat="server" ConnectionString="<%$ ConnectionStrings:MSEConnectionString1 %>"
                        SelectCommand="select *,datediff(hour,lastfiledate,getdate()) as dateDiffImport from mse_appleoracleimportmanagement order by lastfiledate desc"></asp:SqlDataSource>
                    
                    <asp:Panel runat="server" ID="pnlAdmin" Visible="false">
                        <h3>Inventory files outstanding</h3>
                        <table class="CSSTableGenerator">
                            <tr>

                                <td>United Kingdom</td>
                                <td>Italy</td>
                                <td>United Arab Emirates</td>
                                <td>Czech Republic</td>
                                <td>Netherlands</td>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblUK" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblIT" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblUAE" runat="server"></asp:Label></td>
                                <td>0</td>
                                <td>
                                    <asp:Label ID="lblNL" runat="server"></asp:Label></td>
                            </tr>
                        </table>

                        <h3>Oracle files outstanding</h3>
                        <table class="CSSTableGenerator">
                            <tr>
                                <th>Oracle Purchase orders</th>
                                <th>Product range files</th>
                                <th>Oracle Stock files</th>
                                <th>Locale back order files</th>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblPO" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblPR" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblOS" runat="server"></asp:Label></td>
                                <td>
                                    <asp:Label ID="lblBO" runat="server"></asp:Label></td>
                            </tr>
                        </table>
                        <br />
                        <asp:Button ID="btnProcessAllFiles" runat="server" Text="Process all files" OnClick="btnProcessAllFiles_Click" />
                    </asp:Panel>

                    <br />
                    <h2>Report management/download</h2>
                    <h3>VMI report</h3>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Report</th>
                            <td>
                                <asp:DropDownList ID="ddlVMIReports" DataTextField="Filename" DataValueField="vmireportid" runat="server" DataSourceID="sqlDsVMIReportSource" AppendDataBoundItems="true">
                                    <asp:ListItem Text="select a report" Selected="True" Value="0">select a report</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="btnDownloadVMI" runat="server" Text="Download report" OnClick="btnDownloadVMI_ClickNew" /></td>
                        </tr>
                    </table>

                    <h3>Forecast commit report</h3>
                    <table class="CSSTableGenerator">
                        <tr>
                            <th>Report</th>
                            <td>
                                <asp:DropDownList ID="ddlForecastReports" DataTextField="Filename" DataValueField="reportid" runat="server" DataSourceID="sqlDsFCReportSource" AppendDataBoundItems="true">
                                    <asp:ListItem Text="select a report" Selected="True" Value="0">select a report</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="Button1" runat="server" Text="Download report" OnClick="btnDownloadFC_Click" /></td>
                        </tr>
                    </table>

                    <h2>Stock replen</h2>
                    <h3>Safety stock level management</h3>

                        <asp:GridView ID="gvSafety" DataKeyNames="LocaleID" CssClass="CSSTableGenerator" runat="server" OnRowUpdating="gvSafety_RowUpdating" OnRowCancelingEdit="gvSafety_RowCancelingEdit" OnRowEditing="gvSafety_RowEditing"
                            AutoGenerateColumns="false" AutoGenerateEditButton="true">
                            <Columns>
                                <asp:BoundField ReadOnly="true" DataField="PlantCode" HeaderText="Plant Code" />
                                <asp:BoundField ReadOnly="true" DataField="PLantDescription" HeaderText="Plant Description" />
                                <asp:TemplateField>
                                    <ItemTemplate>
                                        <asp:Label ID="lblSafety" runat="server" Text='<%# Eval("SafetyStockPercentage") %>'></asp:Label>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtSafety" runat="server" Text='<%# Eval("SafetyStockPercentage") %>'></asp:TextBox><asp:RegularExpressionValidator ID="revNumOnlySafety" runat="server" ControlToValidate="txtSafety" Text="Please enter a numerical value" ValidationExpression="^[0-9]*$"></asp:RegularExpressionValidator>

                                    </EditItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    
                    <h3>Create a replen</h3>
                    <table class="CSSTableGenerator">
                        <!--<tr>
    <th>Apple stock replen</th><td><asp:Button ID="btnRunStockReplen" runat="server" Text="Email replen" OnClick="btnRunStockReplen_Click" /></td>
    </tr>-->
                        <tr>
                            <th>Locale</th>
                            <th>Download replen report</th>
                            <th>Create replen entry</th>
                            <tr>
                                <td>Lu10- United Kingdom</td>
                                <td>
                                    <asp:Button ID="btnUkReplenReport" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-1" /></td>
                                <td>
                                    <asp:Button ID="btnUkReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-1" /></td>
                            </tr>
                        <tr>
                            <td>Lu20- Netherlands</td>
                            <td>
                                <asp:Button ID="Button2" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-2" /></td>
                            <td>
                                <asp:Button ID="btnNLReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-2" /></td>
                        </tr>
                        <tr>
                            <td>Lu30- Czech Republic</td>
                            <td>
                                <asp:Button ID="Button3" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-3" /></td>
                            <td>
                                <asp:Button ID="btnCZReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-3" /></td>
                        </tr>
                        <tr>
                            <td>Lu40- Italy</td>
                            <td>
                                <asp:Button ID="Button4" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-4" /></td>
                            <td>
                                <asp:Button ID="btnITReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-4" /></td>
                        </tr>
                        <tr>
                            <td>Lu50- United Arab Emirates</td>
                            <td>
                                <asp:Button ID="Button5" runat="server" Text="Download" OnCommand="btnUkReplenReport_Command" CommandArgument="r-5" /></td>
                            <td>
                                <asp:Button ID="btnUAEReplenFile" runat="server" Text="Create replen" OnCommand="btnUkReplenReport_Command" CommandArgument="f-5" /></td>
                        </tr>

                    </table>
                    <h4>Replen entries</h4>
                    <asp:GridView ID="gvReplenEntries" style="height:200px; overflow:auto"  cssclass="CSSTableGenerator" runat="server" DataSourceID="sqlDSReplenEntries" DataKeyNames="ReplenID" AutoGenerateColumns="false" OnRowCommand="gvReplenEntries_RowCommand" OnRowDataBound="gvReplenEntries_RowDataBound" >
                        <Columns>
                            <asp:BoundField ReadOnly="true" DataField="ReplenID" HeaderText="ReplenID" />
                            <asp:BoundField ReadOnly="true" DataField="DateGenerated" HeaderText="Date created" DataFormatString="{0:f}" />
                            <asp:BoundField ReadOnly="true" DataField="PlantCode" HeaderText="Plant Code" />
                            <asp:BoundField ReadOnly="true" DataField="ExertisOutAccount" HeaderText="Oracle Account Code" />                            
                            <asp:BoundField ReadOnly="true" DataField="SentToOracle" HeaderText="Submited to Oracle" />
                            <asp:BoundField ReadOnly="true" DataField="SentToOracleDate" DataFormatString="{0:f}" HeaderText="Submited to Oracle Date" />
                            <asp:BoundField ReadOnly="true" DataField="SentToOracleFilename" HeaderText="Submited to Oracle Filename" />
                            <asp:TemplateField>
                                <HeaderTemplate>Submit to Oracle</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnSubmitToOracle" runat="server" Text="Submit to Oracle" CommandName="submitReplenToOracle" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Resubmit to Oracle</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnResubmitToOracle" runat="server" Text="Resubmit to Oracle" CommandName="resubmitReplenToOracle" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <HeaderTemplate>Download Replen Lines</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Button ID="btnDownloadReplenLines" runat="server" Text="Download replen lines" CommandName="downloadReplenLines" CommandArgument='<%# Eval("ReplenID") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>

                    <h3>Oracle PO Suggestions</h3>
                    <table class="CSSTableGenerator">
                        <table class="CSSTableGenerator">
                            <tr>
                                <th>Locale</th>
                                <th>Download po suggestions report by locale</th>
                            </tr>
                            <tr>
                                <td>All locales- Consolidated</td>
                                <td>
                                    <asp:Button ID="Button11" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-0" /></td>
                            </tr>
                            <tr>
                                <td>Lu10- United Kingdom</td>
                                <td>
                                    <asp:Button ID="Button6" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-1" /></td>
                            </tr>
                            <tr>
                                <td>Lu20- Netherlands</td>
                                <td>
                                    <asp:Button ID="Button7" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-2" /></td>
                            </tr>
                            <tr>
                                <td>Lu30- Czech Republic</td>
                                <td>
                                    <asp:Button ID="Button8" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-3" /></td>
                            </tr>
                            <tr>
                                <td>Lu40- Italy</td>
                                <td>
                                    <asp:Button ID="Button9" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-4" /></td>
                            </tr>
                            <tr>
                                <td>Lu50- United Arab Emirates</td>
                                <td>
                                    <asp:Button ID="Button10" runat="server" Text="Download" OnCommand="btnUkReplenFile_Command" CommandArgument="r-5" /></td>
                            </tr>
                        </table>                       

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

                        <h2>Latest reports (details of most recent reports we have processed)</h2>
                        <h3>Inventory balance reports</h3>
                        <asp:GridView ID="gvLatestInventory" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" DataSourceID="sqlDSInventoryreportsLatest">
                            <Columns>
                                <asp:BoundField DataField="PlantCode" HeaderText="Plant Code" />
                                <asp:BoundField DataField="PLantDescription" HeaderText="Plant Description" />
                                <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                                <asp:BoundField DataField="DateCreated" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                            </Columns>
                        </asp:GridView>

                        <h3>VMI</h3>
                        <asp:GridView ID="gvVMI" CssClass="CSSTableGenerator" runat="server" AutoGenerateColumns="false" DataSourceID="sqlDSVMILatest">
                            <Columns>
                                <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                                <asp:BoundField DataField="DateCreated" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                            </Columns>
                        </asp:GridView>

                        <h3>Forecast Commit</h3>
                        <asp:GridView CssClass="CSSTableGenerator" ID="gvFC" runat="server" AutoGenerateColumns="false" DataSourceID="sqlDSFCLatest">
                            <Columns>
                                <asp:BoundField DataField="Filename" HeaderText="Latest file (filename)" />
                                <asp:BoundField DataField="ReportDate" HeaderText="Latest file (date)" DataFormatString="{0:f}" />
                            </Columns>
                        </asp:GridView>

                        <h3>Oracle Exports</h3>
                        <asp:GridView ID="gvLastImportedOracle" CssClass="CSSTableGenerator" runat="server" DataSourceID="sqlDSorscleLastRunImports" AutoGenerateColumns="false" OnRowDataBound="gvLastImportedOracle_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ReportName" HeaderText="Import name/desc" />
                                <asp:BoundField DataField="LastFileName" HeaderText="Filename" />
                                <asp:BoundField DataField="LastFileDate" DataFormatString="{0:f}" HeaderText="Import Date" />
                                <asp:BoundField DataField="dateDiffImport" HeaderText="Hours since last file received" />

                            </Columns>

                        </asp:GridView>
                        <%--<asp:Table ID="tblLastRun" runat="server" CssClass="CSSTableGenerator" Width="75%">
                            <asp:TableRow>
                                <asp:TableCell></asp:TableCell>
                                <asp:TableHeaderCell>Oracle ASN</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Oracle Open PO Book</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Oracle Stocksi</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Apple Product Range</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Apple Order Response</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Apple Despatch Advise</asp:TableHeaderCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell>Filename</asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblASNFilename" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblPOBOOKFilename" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblStockSIFilename" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblAppleRangeFilename" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblAppleOrderResponseFilename" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblAppleDespatchAdviceFilename" runat="server"></asp:Label></asp:TableCell>
                            </asp:TableRow>
                            <asp:TableRow>
                                <asp:TableCell>Report date</asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblASNFileDate" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblPOBOOKFileDate" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblStockSIFileDate" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblAppleRangeFiledate" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblAppleOrderResponseFiledate" runat="server"></asp:Label></asp:TableCell>
                                <asp:TableCell>
                                    <asp:Label ID="lblAppleDespatchAdviceFiledate" runat="server"></asp:Label></asp:TableCell>

                            </asp:TableRow>


                        </asp:Table>--%>
                        <asp:Panel ID="pnlHide" runat="server" Visible="false">

                            <h3>Forecast commit reporting</h3>
                            <h4>Forecast commit reports</h4>
                            <asp:GridView ID="gvForecastCommitReports" runat="server" DataSourceID="sqlDSForecastCommitReports"
                                AutoGenerateColumns="true">
                            </asp:GridView>
                            <br />
                            <h4>Forecast commit report discrepancie reports</h4>
                            <asp:GridView ID="gvForecastCommitDiscrepancieReports" runat="server" DataSourceID="sqlDSForecastDiscrepancieReports"
                                AutoGenerateColumns="true">
                            </asp:GridView>

                            <h3>Apple depot inventory reports</h3>
                            <asp:GridView ID="gvAppleInventoryReports" runat="server" DataSourceID="sqlDSInventoryReports"
                                AutoGenerateColumns="true">
                            </asp:GridView>

                            <h3>Oracle asns</h3>
                            *Expected delivery dates are based on the carrier lead times to that depot location
    <asp:GridView ID="gvOracleAsns" runat="server" DataSourceID="sqlDSInventoryReports"
        AutoGenerateColumns="true">
    </asp:GridView>
                        </asp:Panel>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
