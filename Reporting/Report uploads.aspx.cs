using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MSE_Common;
using System.IO;

namespace linx_tablets.CustomerService
{
    public partial class Report_uploads : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnUploadProductRange_Click(object sender, EventArgs e)
        {
            if (fupProductRange.HasFile)
            {
                Common.runSQLNonQuery("delete from productdataloader_apple_productrange_tempload_upload");
                string filename = Path.GetFileNameWithoutExtension(fupProductRange.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupProductRange.FileName);
                string filePath = @"\\10.16.72.129\company\";
                string filePathLocale = @"C:\linx-tablets\replen files\";
                //do some shit here
                try
                {
                    fupProductRange.SaveAs(filePath + filename);
                    string bulkInsert = string.Format(@"BULK INSERT productdataloader_apple_productrange_tempload_upload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='\n', FIRSTROW = 2, FIRE_TRIGGERS  ) ", filePathLocale + filename);
                    Common.runSQLNonQuery(bulkInsert);
                    if(int.Parse(Common.runSQLScalar("select count(*) from productdataloader_apple_productrange_tempload_upload").ToString()) == 0)
                        throw new Exception();

                    string updateSQL = string.Format("exec sp_oracleproductrangeimport_upload '{0}'", fupProductRange.FileName);
                    Common.runSQLNonQuery(updateSQL);
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the range has been updated');", true);
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload unsuccessful, please check the format of the file and comapre with the sample report');", true);
                }
            }
        }

        protected void btnDownloadRange_Click(object sender, EventArgs e)
        {
            string filename = "ProductRange_Existing_"+Common.timestamp()+".csv";
            runReport("select Applecode, Exertis_Part_Number,Material_Description,Status,EOLDate,Npidate,Npiforecast,EolType from mse_appleproductmapping", filename);
        }
        protected void btnDownloadRangeTemplate_Click(object sender, EventArgs e)
        {
            string filename = "ProductRange_Template_" + Common.timestamp() + ".csv";
            runReport("select *  from mse_appleproductmapping where applecode='999'", filename);
        }
        private void runReport(string query, string filename)
        {
            this.Session["ReportQuery"] = (object)query;
            this.Session["ReportQueryIsSp"] = (object)false;
            this.Session["ReportDelimiter"] = (object)",";
            this.Session["ReportHasHeader"] = (object)true;
            this.Session["ReportFileName"] = (object)filename;
            this.Session["ReportTextQualifier"] = (object)"\"";
            this.Response.Redirect("~/reporting/report-export-csv.aspx");
        }

        
    }
}