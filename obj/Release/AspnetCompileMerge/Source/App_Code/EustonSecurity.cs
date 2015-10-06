using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using MSE_Common;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class contains functionality for security within EUSTON,
/// which covers things like Hierarchys, Group Companies, Permissions etc.
/// </summary>
public static class EustonSecurity
{
#region User Credentials
    public static string getUsernameForDomainUser(string p)
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MSEConnectionString1"].ConnectionString);     
SqlCommand cmd = new SqlCommand("SELECT StaffName FROM MSE_Staff WHERE DomainLogin = '" + p + "'", conn);
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 120;

        string result = "";
        try
        {
            conn.Open();
            result = cmd.ExecuteScalar().ToString();

        }
        catch (Exception ex)
        {
            Common.log("ERROR encountered getting username from domain user name. Error was: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }

        return result;

    }

    public static int getStaffIDForDomainUser(string p)
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MSEConnectionString1"].ConnectionString);
        SqlCommand cmd = new SqlCommand("SELECT StaffID FROM MSE_Staff WHERE DomainLogin = '" + p + "'", conn);
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 120;

        int result = 0;
        try
        {
            conn.Open();
            result = Int32.Parse(cmd.ExecuteScalar().ToString());

        }
        catch (Exception ex)
        {
            Common.log("ERROR encountered getting StaffID from domain user name. Error was: " + ex.Message);
        }
        finally
        {
            conn.Close();
        }

        return result;

    }
    /// <summary>
    /// Returns a string for the Group Company that the
    /// current user belongs to
    /// </summary>
    /// <returns></returns>
    public static string getUserGroupCompanyId()
    {
        string sql = "SELECT GroupCompanyId FROM MSE_Staff WHERE StaffId = " + System.Web.HttpContext.Current.Session["LoginStaffID"];

        
        string groupCompanyId = Common.runSQLScalar(sql).ToString();

        if (groupCompanyId == null)
            throw new Exception("Null Group Company value for staff Id " + System.Web.HttpContext.Current.Session["LoginStaffID"] + ". Check MSE_Staff table. This could also be caused by an expired session variable.");

        return groupCompanyId;
    }

    /// <summary>
    /// Returns a string for the Company codde that the
    /// current user belongs to
    /// </summary>
    /// <returns></returns>
    public static string getUserCompanyCode()
    {
        //string sql = "SELECT CompanyCode FROM MSE_Staff WHERE StaffId = " + System.Web.HttpContext.Current.Session["LoginStaffID"] ;
        string sql = "SELECT CompanyCode FROM MSE_Staff WHERE StaffId = 226";
        string companyCode = Common.runSQLScalar(sql).ToString();

        if (companyCode == null)
            throw new Exception("Null CompanyCode value for staff Id " + System.Web.HttpContext.Current.Session["LoginStaffID"] + ". Check MSE_Staff table. This could also be caused by an expired session variable.");

        return companyCode;
    }

    /// <summary>
    /// Returns a string for the Company codde that the
    /// current user belongs to
    /// </summary>
    /// <returns></returns>
    public static string getUserCurrency()
    {
        if (System.Web.HttpContext.Current.Session["LoginStaffCurrency"] == null)
        {
            string sql = "SELECT CompanyCurrency FROM MSE_Companies WHERE CompanyCode = '" + getUserCompanyCode() + "';";
            string currencyCode = Common.runSQLScalar(sql).ToString();

            if (currencyCode == null)
                throw new Exception("Null currencyCode value for staff Id " + System.Web.HttpContext.Current.Session["LoginStaffID"] + ". Check MSE_Staff table. This could also be caused by an expired session variable.");

            System.Web.HttpContext.Current.Session["LoginStaffCurrency"] = currencyCode;
            return currencyCode;
        }
        else
        {
            return System.Web.HttpContext.Current.Session["LoginStaffCurrency"].ToString();
        }
        
    }

    /// <summary>
    /// Returns a string for the Company codde that the
    /// current user belongs to
    /// </summary>
    /// <returns></returns>
    public static string getCompanyCurrency(string companyCode)
    {
        Dictionary<string, string> cc;

        if (System.Web.HttpContext.Current.Session["CompanyCurrencies"] == null)
        {
            string sql = "SELECT CompanyCode, CompanyCurrency FROM MSE_Companies";
            cc = new Dictionary<string, string>();
            foreach (DataRow r in Common.runSQLDataset(sql).Tables[0].Rows)
                cc.Add(r["CompanyCode"].ToString(), r["CompanyCurrency"].ToString());
            System.Web.HttpContext.Current.Session["CompanyCurrencies"] = cc;
        }
        else
        {
            cc = (Dictionary<string, string>)System.Web.HttpContext.Current.Session["CompanyCurrencies"];
        }

        return cc[(companyCode == "" || companyCode == null) ? "MSEUK" : companyCode];
    }

    /// <summary>
    /// Returns a string for the Company codde that the
    /// current user belongs to
    /// </summary>
    /// <returns></returns>
    public static string getUserCulture()
    {
        if (System.Web.HttpContext.Current.Session["LoginStaffCulture"] == null)
        {
            string sql = "SELECT CultureCode FROM MSE_Companies WHERE CompanyCode = '" + getUserCompanyCode() + "';";
            string CultureCode = Common.runSQLScalar(sql).ToString();

            if (CultureCode == null)
                throw new Exception("Null CultureCode value for staff Id " + System.Web.HttpContext.Current.Session["LoginStaffID"] + ". Check MSE_Staff table. This could also be caused by an expired session variable.");

            System.Web.HttpContext.Current.Session["LoginStaffCulture"] = CultureCode;
            return CultureCode;
        }
        else
        {
            return System.Web.HttpContext.Current.Session["LoginStaffCulture"].ToString();
        }

    }

    /// <summary>
    /// Checks user permissions for a particular permission object,
    /// and if the permission is not granted, redirects the response
    /// to an access denied screen.
    /// </summary>
    /// <param name="permission">The permission to check</param>
    public static void checkUserPermissionWithRedirect(string permission)
    {
        if (!checkUserPermission(permission))
            System.Web.HttpContext.Current.Response.Redirect("~/access-denied.aspx?permission=" + permission);
    }


    /// <summary>
    /// Checks the current users permissions to access a specific
    /// page or operation. In the event of a permission not being
    /// defined, the system defaults to allowable default defined in
    /// the permission def. If this is not specified, or there is
    /// an error, permission is denied by default.
    /// </summary>
    /// <param name="permission">The permission definition to check</param>
    /// <returns>True if the user is granted permission, false otherwise</returns>
    public static bool checkUserPermission(string permission)
    {
        System.Web.HttpContext.Current.Session["LoginDomainUsername"] = HttpContext.Current.User.Identity.Name.ToString();

        //update the interface for the username
        if (System.Web.HttpContext.Current.Session["LoginUsername"] == null || System.Web.HttpContext.Current.Session["LoginUsername"].ToString() == "")
        {
            System.Web.HttpContext.Current.Session["LoginUsername"] = EustonSecurity.getUsernameForDomainUser(HttpContext.Current.User.Identity.Name.ToString());
            System.Web.HttpContext.Current.Session["LoginStaffID"] = EustonSecurity.getStaffIDForDomainUser(HttpContext.Current.User.Identity.Name.ToString());

        }

        //first check the permission definition exists, and if
        //it does, it'll tell us if the permission is granted
        //by default if no specific deny permission entry exists
        bool allowedByDefault = false;

        try
        {
            string sql = "SELECT IsAllowedByDefault FROM MSE_EustonPermissionDefs WHERE Permission = '" + permission + "'";
            allowedByDefault = Boolean.Parse(Common.runSQLScalar(sql).ToString());
        }
        catch (NullReferenceException nullex)
        {
            //nothing returned from the query
            throw new Exception("Undefined permission " + permission + " requested in checkUserPermission (EustonSecurity.cs)");

        }
        catch (Exception ex)
        {
            //genuine error
            string err = "ERROR encountered getting EUSTON permission defaults for permission " + permission + ". Error was: " + ex.Message;
            Common.log(err);
            Common.sendEmailErrorMessage(err);
            return false; //error situation, so deny
        }

        //assuming we got here, the permission must be a valid recognised def,
        //so see if the user has permission to do what they're asking
        try
        {
            string sql = "SELECT IsDeny FROM MSE_Staff_Permissions WHERE Active = 1 AND Permission = '" + permission + "' AND StaffID = " + System.Web.HttpContext.Current.Session["LoginStaffID"];
            object result = Common.runSQLScalar(sql);

            if (result != null)
            {
                bool isDeny = Boolean.Parse(result.ToString());

                if (isDeny)
                {
                    return false; //explicit deny
                }
                else
                {
                    return true; //explicit allow
                }
            }
            else
            {
                if (allowedByDefault)
                {
                    return true; //allowed by default
                }
                else
                {
                    return false; //not allowed by default
                }
            }
        }
        catch (Exception ex)
        {
            string err = "ERROR encountered getting user permission details for permission " + permission + " and user ID " + System.Web.HttpContext.Current.Session["LoginStaffID"] + ". Error was: " + ex.Message;
            Common.log(err);
            Common.sendEmailErrorMessage(err);
            return false; //error situation, so deny
        }

        //if we got here, something is wrong with the logic above,
        //so we're in an error situation, return false
        return false; // error situation, so deny
            
    }
#endregion

}
