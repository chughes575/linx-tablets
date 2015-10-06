using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MSE_Common;
using System.Data.SqlClient;

/// <summary>
/// This class implements a delimited text file generator. The user should pass
/// in a filename, query and a boolean value indicating if the query
/// is an SP (otherwise it's a text query), as request parameters,
/// and the page will render the results of the query as a delimited text file,
/// quoting out the number values as required. 
/// </summary>
public partial class reporting : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        String myFileName = Session["ReportFileName"].ToString().Replace("[TIMESTAMP]", Common.timestamp());

        Response.ContentType = "text/comma-separated-values";
        Response.AddHeader("content-disposition", "attachment; filename=" + myFileName);
        Response.Buffer = true;

        generateCSVData();
        //Response.Write(myFile);
        
        Response.End();
    }

    private void generateCSVData()
    {
        string query = Session["ReportQuery"].ToString();
        bool isQuerySp = Boolean.Parse(Session["ReportQueryIsSp"].ToString());
        string delimiter = Session["ReportDelimiter"].ToString();
        bool header = Boolean.Parse(Session["ReportHasHeader"].ToString());
        string textQualifier = Session["ReportTextQualifier"].ToString();

        string output = "";

        SqlConnection conn = new SqlConnection(Common.getConfigFileValue("SqlConnectionString"));
        SqlCommand cmd = new SqlCommand(query, conn);
        if (isQuerySp)
        {
            cmd.CommandType = CommandType.StoredProcedure;
        }
        else
        {
            cmd.CommandType = CommandType.Text;
        }
        cmd.CommandTimeout = 500;

        SqlDataReader reader;
        try
        {
            conn.Open();

            //enable dirty reads on the connection to avoid deadlocks and speed up performance
            Common.enableDirtyReadsOnConnection(ref conn);

            reader = cmd.ExecuteReader();

            //write the header
            if (header)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    output += reader.GetName(i).Replace(delimiter, "") + delimiter;
                }
                //remove the last delimiter and replace with line break
                output = output.Substring(0, output.Length - delimiter.Length);
                output += "\n";

                Response.Write(output);
                output = "";
            }

            //write the data
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string field;
                    if (reader[i].GetType() == typeof(String))
                    {
                        field = textQualifier + reader[i].ToString().Replace(delimiter, "").Replace(textQualifier, "") + textQualifier;
                    }
                    else
                    {
                        field = reader[i].ToString().Replace(delimiter, "");
                    }
                    output += field + delimiter;
                }

                //remove the last delimiter and replace with line break
                output = output.Substring(0, output.Length - delimiter.Length);
                output += "\n";

                Response.Write(output);
                output = "";
            }
        }
        catch (Exception ex)
        {
            Common.log("ERROR encountered populating CSV output from database for query " + query + ". Error was: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }
    }

}
