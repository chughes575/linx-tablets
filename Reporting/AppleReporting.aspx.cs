// Decompiled with JetBrains decompiler
// Type: linx_tablets.CustomerService.AppleReporting
// Assembly: linx-tablets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 79E24B2C-4AF9-4E0E-BA5E-3953F1521489
// Assembly location: C:\apple site\Apple Reporting\WebApplication1\bin\linx-tablets.dll

using MSE_Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace linx_tablets.CustomerService
{
  public partial class AppleReporting : Page
  {

    protected void Page_Load(object sender, EventArgs e)
    {

        IFTP ftpClient = new FTP("ftp.msent.co.uk", "/", "extranet", "Extranet1");
        try
        {
            ftpClient.connect();
            ftpClient.uploadFile(@"C:\nexus 7\test.csv");
        }
        catch (Exception ex)
        {
        }

        
        if (HttpContext.Current.User.Identity.Name.ToString() == "appleadmin1")
      {
        this.pnlAdmin.Visible = true;
        this.oustandingFileCounts();
      }
      this.oracleExportDates();
      if (this.IsPostBack)
        return;
      this.bindSafetyData();
    }

    private void runReport(string query, string filename)
    {
      this.Session["ReportQuery"] = (object) query;
      this.Session["ReportQueryIsSp"] = (object) false;
      this.Session["ReportDelimiter"] = (object) ",";
      this.Session["ReportHasHeader"] = (object) true;
      this.Session["ReportFileName"] = (object) filename;
      this.Session["ReportTextQualifier"] = (object) "\"";
      this.Response.Redirect("~/reporting/report-export-csv.aspx");
    }

    private void oracleExportDates()
    {
    }

    private void createStockUpOrders(bool runToday, bool testing)
    {
      DataRowCollection dataRowCollection1 = Common.runSQLRows("select * from mse_applelocalemapping");
      int num1 = int.Parse(Common.runSQLScalar("select coalesce((select count(*) from MSE_AppleReplenDates where cast(ReplenReportDate as date) = cast(getdate() as date) and processed=0) ,0)").ToString());
      int num2 = int.Parse(Common.runSQLScalar("select count(*) from MSE_AppleInventoryreports where DATEDIFF(HOUR,datecreated,GETDATE())<24").ToString());
      int num3 = 1;
      if (!(num1 == num3 | runToday))
        return;
      if (num2 == 5)
      {
        Common.runSQLNonQuery("insert into MSE_AppleReplenDates \r\nselect getdate(),0 from MSE_AppleReplenDates where cast(getdate() as date) not in (select ReplenReportDate from MSE_AppleReplenDates)");
        List<string> list = new List<string>();
        foreach (DataRow dataRow in (InternalDataCollectionBase) dataRowCollection1)
        {
          try
          {
            string contents = Common.dataTableToTextFile(Common.runSQLDataset(string.Format("select * from vw_applestockuplocales where plantcode='{0}'", (object) dataRow[1].ToString())).Tables[0], ",", "\r\n", true);
            string str1 = "\\\\10.16.72.129\\company\\applefiles\\";
            string str2 = string.Format("StockUp_Suggestions_{0}_{1}_{2}.csv", (object) dataRow[1].ToString(), (object) dataRow[2].ToString(), (object) Common.timestamp());
            System.IO.File.AppendAllText(str1 + str2, contents);
            list.Add(str1 + str2);
          }
          catch
          {
          }
        }
        DataRow dataRow1 = Common.runSQLRow("SELECT        TOP (1) * FROM dbo.MSE_AppleForecastCommitReports ORDER BY ReportDate DESC");
        int index1 = 2;
        string str3 = dataRow1[index1].ToString();
        int index2 = 3;
        string str4 = dataRow1[index2].ToString();
        if (list.Count <= 0)
          return;
        MailMessage message = new MailMessage();
        SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
        message.From = new MailAddress("chris.hughes@exertis.co.uk");
        if (!testing)
          message.CC.Add("alina.gavenyte@exertis.co.uk");
        if (testing)
          message.To.Add("chris.hughes@exertis.co.uk");
        else
          message.To.Add("chris.hughes@exertis.co.uk");
        message.Subject = "Apple_StockUp_Suggestions_" + Common.timestamp();
        message.Body = string.Format("Apple Forecast Commit report filename: {0} \\n Apple Forecast Commit report imported at: {1} \\n \\n ", (object) str3, (object) str4);
        foreach (string fileName in list)
        {
          try
          {
            Attachment attachment = new Attachment(fileName);
            message.Attachments.Add(attachment);
          }
          catch
          {
          }
        }
        smtpClient.Port = 587;
        smtpClient.Credentials = (ICredentialsByHost) new NetworkCredential("chris.hughes@exertis.co.uk", "Shetland992");
        smtpClient.EnableSsl = true;
        try
        {
          smtpClient.Send(message);
          Common.runSQLNonQuery(string.Format("update MSE_AppleReplenDates set Processed=1 where cast(getdate() as date) = ReplenReportDate", (object) 1));
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        if (DateTime.Now.Hour < 9)
          return;
        MailMessage message = new MailMessage();
        SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
        message.From = new MailAddress("chris.hughes@exertis.co.uk");
        message.To.Add("chris.hughes@exertis.co.uk");
        message.Subject = (string) (object) int.Parse(Common.runSQLScalar("select 5-count(*) from MSE_AppleInventoryreports where cast(datecreated as date)=cast(getdate() as date)").ToString()) + (object) " Outstanding inventory reports - Replen orders not run";
        DataRowCollection dataRowCollection2 = Common.runSQLRows("select * from MSE_AppleLocaleMapping lm \r\nleft outer join (select localeid,reportid from MSE_AppleInventoryreports where cast(datecreated as date)=cast(getdate() as date)) as reports on reports.LocaleID=lm.LocaleID\r\nwhere reports.reportid is null");
        string str = "";
        int num4 = 0;
        foreach (DataRow dataRow in (InternalDataCollectionBase) dataRowCollection2)
        {
          str = str + dataRow[1].ToString() + " - " + dataRow[2].ToString() + ", ";
          ++num4;
        }
        if (num4 > 0)
          str = str.Substring(0, str.Length - 2);
        message.Body = "Locales missing: " + str;
        smtpClient.Port = 587;
        smtpClient.Credentials = (ICredentialsByHost) new NetworkCredential("chris.hughes@exertis.co.uk", "Shetland992");
        smtpClient.EnableSsl = true;
        try
        {
          smtpClient.Send(message);
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void processInventoryFiles()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\IT\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UK\\";
      string path3 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UAE\\";
      string path4 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\NL\\";
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path2)))
        {
          string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
          string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
          try
          {
            System.IO.File.AppendAllText(path2 + "\\tempfiles\\" + str2, contents, Encoding.Default);
            System.IO.File.Move(path2 + "\\tempfiles\\" + str2, path2 + "\\processed\\" + str2);
            if (System.IO.File.Exists(path2 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path2 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path2 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path2 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path3)))
        {
          string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
          string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
          try
          {
            System.IO.File.AppendAllText(path3 + "\\tempfiles\\" + str2, contents, Encoding.Default);
            System.IO.File.Move(path3 + "\\tempfiles\\" + str2, path3 + "\\processed\\" + str2);
            if (System.IO.File.Exists(path3 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path3 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path3 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path3 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path4)))
        {
          string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
          string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
          try
          {
            System.IO.File.AppendAllText(path4 + "\\tempfiles\\" + str2, contents, Encoding.Default);
            System.IO.File.Move(path4 + "\\tempfiles\\" + str2, path4 + "\\processed\\" + str2);
            if (System.IO.File.Exists(path4 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path4 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path4 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path4 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      try
      {
        foreach (string str1 in new ArrayList((ICollection) Directory.GetFiles(path1)))
        {
          string str2 = System.IO.File.ReadAllText(str1, Encoding.Default).Replace("\n", "\r\n");
          int startIndex = str2.IndexOf("Product");
          string contents = str2.Substring(startIndex, str2.Length - startIndex);
          string str3 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".xls";
          try
          {
            System.IO.File.AppendAllText(path1 + "\\tempfiles\\" + str3, contents, Encoding.Default);
            System.IO.File.Move(path1 + "\\tempfiles\\" + str3, path1 + "\\processed\\" + str3);
            if (System.IO.File.Exists(path1 + "\\original files\\" + Path.GetFileName(str1)))
              System.IO.File.Delete(path1 + "\\original files\\" + Path.GetFileName(str1));
            System.IO.File.Move(str1, path1 + "\\original files\\" + Path.GetFileName(str1));
          }
          catch (Exception ex)
          {
            try
            {
              System.IO.File.Move(str1, path1 + "\\rejects\\" + Path.GetFileName(str1));
            }
            catch
            {
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void oustandingFileCounts()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Purchase Files\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Product Range Files\\";
      string path3 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Inventory\\";
      string path4 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oustanding Hub Orders\\";
      string path5 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\IT\\";
      string path6 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UK\\";
      string path7 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\UAE\\";
      string path8 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Inventory\\NL\\";
      string path9 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Forecast\\";
      string path10 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple VMI Report\\";
      ArrayList arrayList1 = new ArrayList((ICollection) Directory.GetFiles(path9));
      ArrayList arrayList2 = new ArrayList((ICollection) Directory.GetFiles(path10));
      ArrayList arrayList3 = new ArrayList((ICollection) Directory.GetFiles(path1));
      ArrayList arrayList4 = new ArrayList((ICollection) Directory.GetFiles(path2));
      ArrayList arrayList5 = new ArrayList((ICollection) Directory.GetFiles(path3));
      ArrayList arrayList6 = new ArrayList((ICollection) Directory.GetFiles(path4));
      ArrayList arrayList7 = new ArrayList((ICollection) Directory.GetFiles(path6));
      ArrayList arrayList8 = new ArrayList((ICollection) Directory.GetFiles(path7));
      ArrayList arrayList9 = new ArrayList((ICollection) Directory.GetFiles(path8));
      ArrayList arrayList10 = new ArrayList((ICollection) Directory.GetFiles(path5));
      int count1 = arrayList7.Count;
      int count2 = arrayList8.Count;
      int count3 = arrayList9.Count;
      this.lblIT.Text = arrayList10.Count.ToString();
      this.lblUAE.Text = count2.ToString();
      this.lblNL.Text = count3.ToString();
      this.lblUK.Text = count1.ToString();
      this.lblPO.Text = arrayList3.Count.ToString();
      this.lblPR.Text = arrayList4.Count.ToString();
      this.lblOS.Text = arrayList5.Count.ToString();
      this.lblBO.Text = arrayList6.Count.ToString();
    }

    private void procesOracleFiles()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Purchase Files\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Product Range Files\\";
      string path3 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oracle Inventory\\";
      string path4 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Oustanding Hub Orders\\";
      ArrayList arrayList1 = new ArrayList((ICollection) Directory.GetFiles(path1));
      ArrayList arrayList2 = new ArrayList((ICollection) Directory.GetFiles(path2));
      ArrayList arrayList3 = new ArrayList((ICollection) Directory.GetFiles(path3));
      ArrayList arrayList4 = new ArrayList((ICollection) Directory.GetFiles(path4));
      foreach (string str1 in arrayList2)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path2 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path2 + "\\tempfiles\\" + str2, path2 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path2 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path2 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList1)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended4" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path1 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path1 + "\\tempfiles\\" + str2, path1 + "\\processed\\" + str2);
          System.IO.File.Delete(str1);
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path1 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList3)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path3 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path3 + "\\tempfiles\\" + str2, path3 + "\\processed\\" + str2);
          System.IO.File.Delete(str1);
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path3 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList4)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty).Replace("\r\n\n", "\r\n");
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path4 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path4 + "\\tempfiles\\" + str2, path4 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path4 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path4 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
    }

    private void createVmiReport(bool testing)
    {
      List<string> list1 = new List<string>();
      foreach (DataRow dataRow in (InternalDataCollectionBase) Common.runSQLRows(string.Format("select vmireportid,filename,replace(convert(varchar,cast(datecreated as date),103),'/','') from mse_applevmidatareports where case when {0} is then 1 else 0coalesce(responsesent,0)=0 order by datecreated asc", (object) Convert.ToInt32(testing))))
      {
        List<string> list2 = new List<string>();
        string str1 = string.Format("VMI_Report_{1}_{0}.csv", (object) Common.timestamp(), (object) dataRow[2].ToString());
        try
        {
          string contents = Common.dataTableToTextFile(Common.runSQLDataset(string.Format("select * from vw_applevmireportresponse where vmireportid={1}", (object) Common.timestamp(), (object) dataRow[0].ToString())).Tables[0], ",", "\r\n", true);
          string str2 = "\\\\10.16.72.129\\company\\applefiles\\";
          System.IO.File.AppendAllText(str2 + str1, contents);
          list2.Add(str2 + str1);
        }
        catch
        {
        }
        if (list2.Count > 0)
        {
          MailMessage message = new MailMessage();
          SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
          message.From = new MailAddress("chris.hughes@exertis.co.uk");
          if (!testing)
            message.CC.Add("alina.gavenyte@exertis.co.uk");
          message.To.Add("chris.hughes@exertis.co.uk");
          message.Subject = string.Format("VMI_Report_{1}__{0}_", (object) Common.timestamp(), (object) dataRow[2].ToString());
          message.Body = string.Format("Apple VMI report filename: {0}  VMIReportID: {1}  VMI Report Date: {2}", (object) str1, (object) dataRow[0].ToString(), (object) dataRow[2].ToString());
          foreach (string fileName in list2)
          {
            try
            {
              Attachment attachment = new Attachment(fileName);
              message.Attachments.Add(attachment);
            }
            catch
            {
            }
          }
          smtpClient.Port = 587;
          smtpClient.Credentials = (ICredentialsByHost) new NetworkCredential("chris.hughes@exertis.co.uk", "Shetland992");
          smtpClient.EnableSsl = true;
          try
          {
            smtpClient.Send(message);
          }
          catch (Exception ex)
          {
          }
        }
      }
    }

    private void processOtherAppleFiles()
    {
      string path1 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple Forecast\\";
      string path2 = "\\\\10.16.72.129\\company\\FTP\\root\\MSESRVDOM\\apple\\In\\Apple VMI Report\\";
      ArrayList arrayList1 = new ArrayList((ICollection) Directory.GetFiles(path1));
      ArrayList arrayList2 = new ArrayList((ICollection) Directory.GetFiles(path2));
      foreach (string str1 in arrayList1)
      {
        string contents = System.IO.File.ReadAllText(str1, Encoding.Default).Replace(",\n", ",\"\"\n");
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path1 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path1 + "\\tempfiles\\" + str2, path1 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path1 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path1 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
      foreach (string str1 in arrayList2)
      {
        string contents = Regex.Replace(System.IO.File.ReadAllText(str1, Encoding.Default), ",(?=[^\"]*\"(?:[^\"]*\"[^\"]*\")*[^\"]*$)", string.Empty);
        string str2 = Path.GetFileNameWithoutExtension(str1) + "_amended" + Common.timestamp() + ".csv";
        try
        {
          System.IO.File.AppendAllText(path2 + "\\tempfiles\\" + str2, contents, Encoding.Default);
          System.IO.File.Move(path2 + "\\tempfiles\\" + str2, path2 + "\\processed\\" + str2);
          System.IO.File.Move(str1, path2 + "\\original files\\" + Path.GetFileName(str1));
        }
        catch (Exception ex)
        {
          try
          {
            System.IO.File.Move(str1, path2 + "\\rejects\\" + Path.GetFileName(str1));
          }
          catch
          {
          }
        }
      }
    }

    private void createPoSuggestion(bool testing)
    {
      DataRowCollection dataRowCollection = Common.runSQLRows("select * from mse_applelocalemapping");
      List<string> list = new List<string>();
      foreach (DataRow dataRow in (InternalDataCollectionBase) dataRowCollection)
      {
        try
        {
          string contents = Common.dataTableToTextFile(Common.runSQLDataset(string.Format("select * from vw_ApplePoSuggestionsLocale where plantcode='{0}'", (object) dataRow[1].ToString())).Tables[0], ",", "\r\n", true);
          string str1 = "\\\\10.16.72.129\\company\\applefiles\\";
          string str2 = string.Format("Po_Suggestions_{0}_{1}_{2}.csv", (object) dataRow[1].ToString(), (object) dataRow[2].ToString(), (object) Common.timestamp());
          System.IO.File.AppendAllText(str1 + str2, contents);
          list.Add(str1 + str2);
        }
        catch
        {
        }
      }
      string contents1 = Common.dataTableToTextFile(Common.runSQLDataset("select  *from vw_ApplePoSuggestionsConsolidated").Tables[0], ",", "\r\n", true);
      string str3 = "\\\\10.16.72.129\\company\\applefiles\\";
      string str4 = string.Format("Po_Suggestions_Consolidated_{0}.csv", (object) Common.timestamp());
      System.IO.File.AppendAllText(str3 + str4, contents1);
      list.Add(str3 + str4);
      if (list.Count <= 0)
        return;
      MailMessage message = new MailMessage();
      SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
      message.From = new MailAddress("chris.hughes@exertis.co.uk");
      if (!testing)
        message.CC.Add("alina.gavenyte@exertis.co.uk");
      if (testing)
        message.To.Add("chris.hughes@exertis.co.uk");
      else
        message.To.Add("chris.hughes@exertis.co.uk");
      message.Subject = "Apple_Po_Suggestions_" + Common.timestamp();
      message.Body = "";
      foreach (string fileName in list)
      {
        try
        {
          Attachment attachment = new Attachment(fileName);
          message.Attachments.Add(attachment);
        }
        catch
        {
        }
      }
      smtpClient.Port = 587;
      smtpClient.Credentials = (ICredentialsByHost) new NetworkCredential("chris.hughes@exertis.co.uk", "Shetland992");
      smtpClient.EnableSsl = true;
      try
      {
        smtpClient.Send(message);
      }
      catch (Exception ex)
      {
      }
    }

    protected void btnDownloadVMI_ClickNew(object sender, EventArgs e)
    {
      int num = 15;
      try
      {
        num = int.Parse(this.ddlVMIReports.SelectedValue);
      }
      catch
      {
      }
      if (num == 0)
        return;
      List<string> list = new List<string>();
      string filename = string.Format("VMI_Report_{0}.csv", (object) Common.timestamp());
      this.runReport(string.Format("select * from vw_applevmireportresponse where vmireportid={0}", (object) num), filename);
    }

    protected void btnDownloadFC_Click(object sender, EventArgs e)
    {
      int reportID = int.Parse(this.ddlForecastReports.SelectedValue);
      if (reportID == 0)
        return;
      this.createForecastCommitResponse(reportID, false);
    }

    private void bindSafetyData()
    {
      this.gvSafety.DataSource = (object) Common.runSQLDataset("select * from MSE_AppleLocaleSafetyLevels sf inner join mse_applelocalemapping lm on lm.localeid=sf.localeid");
      this.gvSafety.DataBind();
    }

    private void createForecastCommitResponse(int reportID, bool testing)
    {
      string filename = Common.runSQLScalar(string.Format("select substring(FileName,0,39) from MSE_AppleForecastCommitReports where reportid={0}", (object) reportID)).ToString() + ".csv";
      this.runReport(string.Format("sp_appleforecastcommitresponse {0}\r\n", (object) reportID), filename);
    }

    protected void btnRunStockReplen_Click(object sender, EventArgs e)
    {
      this.createStockUpOrders(true, false);
    }

    protected void btnRunPoSUggestions_Click(object sender, EventArgs e)
    {
      this.createPoSuggestion(false);
    }

    protected void btnProcessAllFiles_Click(object sender, EventArgs e)
    {
      try
      {
        this.processInventoryFiles();
      }
      catch
      {
      }
      try
      {
        this.processOtherAppleFiles();
      }
      catch
      {
      }
      try
      {
        this.procesOracleFiles();
      }
      catch
      {
      }
    }

    protected void btnRunVMI_Click(object sender, EventArgs e)
    {
      this.createVmiReport(true);
    }

    protected void btnUkReplenFile_Command(object sender, CommandEventArgs e)
    {
      string str1 = e.CommandArgument.ToString().Substring(0, 1);
      string str2 = e.CommandArgument.ToString().Substring(2, 1);
      string str3 = "";
      if (!(str2 == "1"))
      {
        if (!(str2 == "2"))
        {
          if (!(str2 == "3"))
          {
            if (!(str2 == "4"))
            {
              if (str2 == "5")
                str3 = "Lu50";
            }
            else
              str3 = "Lu40";
          }
          else
            str3 = "Lu30";
        }
        else
          str3 = "Lu20";
      }
      else
        str3 = "Lu10";
      if (str1 == "r" && str2 == "0")
      {
        this.runReport("select * from vw_ApplePoSuggestionsLocale order by plantcode", "POSuggestions_Report_consolidated_" + Common.timestamp() + ".csv");
      }
      else
      {
        if (!(str1 == "r"))
          return;
        this.runReport("select * from vw_ApplePoSuggestionsLocale where plantcode='" + str3 + "'", "POSuggestions_Report_" + str3 + "_" + Common.timestamp() + ".csv");
      }
    }

    protected void btnUkReplenReport_Command(object sender, CommandEventArgs e)
    {
      string str1 = e.CommandArgument.ToString().Substring(0, 1);
      string str2 = e.CommandArgument.ToString().Substring(2, 1);
      string str3 = "";
      string locale = "";
      if (!(str2 == "1"))
      {
        if (!(str2 == "2"))
        {
          if (!(str2 == "3"))
          {
            if (!(str2 == "4"))
            {
              if (str2 == "5")
              {
                str3 = "Lu50";
                locale = "UAE";
              }
            }
            else
            {
              str3 = "Lu40";
              locale = "IT";
            }
          }
          else
          {
            str3 = "Lu30";
            locale = "CZ";
          }
        }
        else
        {
          str3 = "Lu20";
          locale = "NL";
        }
      }
      else
      {
        str3 = "Lu10";
        locale = "UK";
      }
      if (str1 == "r")
      {
          this.runReport("exec [sp_StockUpLocale] " + str2, "StockUp_Report_" + str3 + "_" + Common.timestamp() + ".csv");
      }
      else
      {
        string query = string.Format("select lm.PlantDescription,lm.ExertisOutAccount,\r\nlm.PlantCode, vw.AppleCode, vw.Exertis_Part_Number, toorder,lm.edilocationcode\r\n from vw_applestockuplocales vw inner join mse_applelocalemapping lm on lm.plantcode = vw.plantcode\r\n where vw.plantcode = '{0}' and toorder> 0", (object) str3);
        string str4 = Common.runSQLScalar(string.Format("declare @IDNL table (ReplenID int)\r\n\t\tinsert into mse_applereplens\r\noutput inserted.ReplenID into @IDNL\r\nvalues (getdate(),'appleuser',0,1,{0},null,null)\r\n\r\n\r\ninsert into mse_applereplenlines\r\nselect (select ReplenID from @IDNL),lm.localeid,vw.AppleCode,vw.Exertis_Part_Number,toorder\r\n from vw_applestockuplocales vw inner join mse_applelocalemapping lm on lm.plantcode = vw.plantcode\r\n where vw.plantcode = '{1}' and toorder> 0\r\n\r\nselect ReplenID from @IDNL", (object) str2, (object) str3)).ToString();
        this.sqlDSReplenEntries.DataBind();
         
        this.gvReplenEntries.DataBind();
        this.createReplenOrderFile(query, locale, true);
        ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen created, ReplenID: " + str4 + "');", true);
      }
    }

    public string replenFileFromEntry(int replenID)
    {
      string sql = string.Format("select lm.PlantDescription,lm.ExertisOutAccount,\r\nlm.PlantCode, rl.AppleCode, rl.Exertis_Part_Number, Qty,lm.edilocationcode from mse_applereplenlines rl \r\ninner join MSE_AppleLocaleMapping lm on lm.LocaleID=rl.LocaleID\r\ninner join mse_applereplens ap on ap.replenid=rl.replenid\r\nwhere rl.replenid={0}", (object) replenID);
      string str1 = "Apple_" + Common.runSQLRow(string.Format("select lm.*\r\nfrom mse_applereplens ar inner join mse_applelocalemapping lm on lm.localeid=ar.LocaleID\r\nwhere ar.ReplenID={0}", (object) replenID))[6].ToString() + "_" + Common.runSQLScalar("select CAST(DATEPART(d,COALESCE(getdate(),'01/01/1901')) AS Varchar)+ '-'+\r\n                                 UPPER(CONVERT(varchar(3), DATENAME(MONTH, COALESCE(getdate(),'01/01/1901'))))+ '-'+\r\n                                     CAST(DATEPART(yyyy,COALESCE(getdate(),'01/01/1901')) AS Varchar)").ToString();
      List<string> list = new List<string>();
      string str2 = string.Empty;
      foreach (DataRow dataRow in (InternalDataCollectionBase) Common.runSQLRows(sql))
      {
        string str3 = dataRow[5].ToString();
        string str4 = dataRow[6].ToString();
        string str5 = string.Empty;
        string str6 = string.Empty;
        string str7 = string.Empty;
        string str8 = string.Empty;
        string str9 = string.Empty;
        string str10 = string.Empty;
        string str11 = dataRow[1].ToString() + "," + str1 + "," + dataRow[4].ToString() + "," + dataRow[4].ToString() + "," + str3 + "," + string.Empty + "," + string.Empty + "," + str2 + "," + str4 + "," + str5 + "," + str6 + "," + str7 + "," + str8 + "," + str9 + ",0.00," + str10 + "," ?? "";
        list.Add(str11);
      }
      string str12 = "";
      try
      {
        str12 = "C:\\Linx-tablets\\replen files\\" + ("MSE_ORDERS_" + Common.timestamp() + ".csv");
        if (System.IO.File.Exists(str12))
          System.IO.File.Move(str12, str12 + ".OLD");
        System.IO.File.WriteAllLines(str12, list.ToArray());
      }
      catch (Exception ex)
      {
      }
      return str12;
    }

    public string createReplenOrderFile(string query, string locale, bool oracleSubmission)
    {
      string str1 = "Apple_" + locale + "_" + Common.runSQLScalar("select CAST(DATEPART(d,COALESCE(getdate(),'01/01/1901')) AS Varchar)+ '-'+\r\n                                 UPPER(CONVERT(varchar(3), DATENAME(MONTH, COALESCE(getdate(),'01/01/1901'))))+ '-'+\r\n                                     CAST(DATEPART(yyyy,COALESCE(getdate(),'01/01/1901')) AS Varchar)").ToString();
      List<string> list = new List<string>();
      string str2 = string.Empty;
      foreach (DataRow dataRow in (InternalDataCollectionBase) Common.runSQLRows(query))
      {
        string str3 = dataRow[5].ToString();
        string str4 = dataRow[6].ToString();
        string str5 = string.Empty;
        string str6 = string.Empty;
        string str7 = string.Empty;
        string str8 = string.Empty;
        string str9 = string.Empty;
        string str10 = string.Empty;
        string str11 = dataRow[1].ToString() + "," + str1 + "," + dataRow[4].ToString() + "," + dataRow[4].ToString() + "," + str3 + "," + string.Empty + "," + string.Empty + "," + str2 + "," + str4 + "," + str5 + "," + str6 + "," + str7 + "," + str8 + "," + str9 + ",0.00," + str10 + "," ?? "";
        list.Add(str11);
      }
      string str12 = "";
      try
      {
        string str3 = "MSE_ORDERS_" + Common.timestamp() + ".csv";
        str12 = "C:\\Linx-tablets\\replen files\\" + str3;
        if (System.IO.File.Exists(str12))
          System.IO.File.Move(str12, str12 + ".OLD");
        if (!oracleSubmission)
        {
          System.IO.File.WriteAllLines(str12, list.ToArray());
          this.Response.ContentType = "text/comma-separated-values";
          this.Response.AddHeader("content-disposition", "attachment; filename=" + str3);
          this.Response.Buffer = true;
          this.Response.Write(System.IO.File.ReadAllText(str12));
        }
      }
      catch (Exception ex)
      {
      }
      return str12;
    }

    protected void gvSafety_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
      try
      {
        GridViewRow gridViewRow = this.gvSafety.Rows[e.RowIndex];
        string str = this.gvSafety.DataKeys[e.RowIndex].Value.ToString();
        string id = "txtSafety";
        TextBox textBox = (TextBox) gridViewRow.FindControl(id);
        Common.runSQLNonQuery(string.Format("update MSE_AppleLocaleSafetyLevels set safetystockpercentage={1} where localeid='{0}' ", (object) str, (object) textBox.Text));
        this.gvSafety.EditIndex = -1;
        this.bindSafetyData();
      }
      catch
      {
      }
    }

    protected void gvSafety_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
      this.gvSafety.EditIndex = -1;
      this.bindSafetyData();
    }

    protected void gvSafety_RowEditing(object sender, GridViewEditEventArgs e)
    {
      this.gvSafety.EditIndex = e.NewEditIndex;
      this.bindSafetyData();
    }

    protected void gvReplenEntries_RowCommand(object sender, GridViewCommandEventArgs e)
    {
      string str1 = e.CommandName.ToString();
      if (!(str1 == "submitReplenToOracle") && !(str1 == "resubmitReplenToOracle"))
      {
        if (!(str1 == "downloadReplenLines"))
          return;
        this.runReport(string.Format(" select rl.ReplenID,lm.Localeid as ExertisLocaleID,lm.PlantCode,rl.AppleCode,rl.Exertis_part_number,Qty from mse_applereplenlines rl\r\n inner join mse_applereplens rlh on rlh.ReplenID=rl.ReplenID\r\n inner join mse_applelocalemapping lm on lm.LocaleID = rlh.LocaleID\r\n Where rlh.replenid={0}", (object) e.CommandArgument.ToString()), string.Format("AppleReplenReport_ReplenID_{0}_{1}.csv", (object) e.CommandArgument.ToString(), (object) Common.timestamp()));
      }
      else
      {
        try
        {
          string str2 = this.replenFileFromEntry(int.Parse(e.CommandArgument.ToString()));
          string ftpFolder = Common.runSQLScalar("select configvalue from mse_appleconfig where configkey='Oracle Ftp Order Directory'").ToString();
            if (System.IO.File.Exists(str2))
          {
              new FTP("ftp.micro-p.com", ftpFolder, "exertismse", "m1cr0p").uploadFile(str2);
            Common.runSQLNonQuery(string.Format("update mse_applereplens set senttooracle=1, SentToOracleDate=getdate(),SentToOracleFilename='{1}' where replenid={0}", (object) e.CommandArgument.ToString(), (object) Path.GetFileName(str2)));
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen exported to oracle, ReplenID: " + e.CommandArgument.ToString() + " Filename: " + Path.GetFileName(str2) + "');", true);  
          }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Replen export failed, please try again');", true); 
        }
        this.sqlDSReplenEntries.DataBind();
        this.gvReplenEntries.DataBind();
      }
    }

    protected void gvReplenEntries_RowDataBound(object sender, GridViewRowEventArgs e)
    {
      if (e.Row.RowType != DataControlRowType.DataRow)
        return;
      Button button1 = (Button) e.Row.FindControl("btnSubmitToOracle");
      Button button2 = (Button) e.Row.FindControl("btnResubmitToOracle");
      if (DataBinder.Eval(e.Row.DataItem, "SentToOracle").ToString().Equals("Yes"))
        button1.Enabled = false;
      else
        button2.Enabled = false;
    }

    protected void gvLastImportedOracle_RowDataBound(object sender, GridViewRowEventArgs e)
    {
      if (e.Row.RowType != DataControlRowType.DataRow)
        return;
      Button button1 = (Button) e.Row.FindControl("btnSubmitToOracle");
      Button button2 = (Button) e.Row.FindControl("btnResubmitToOracle");
      if (int.Parse(DataBinder.Eval(e.Row.DataItem, "dateDiffImport").ToString()) < 24)
        return;
      e.Row.BackColor = Color.Orange;
      e.Row.ForeColor = Color.Black;
      e.Row.Font.Bold = true;
    }


    protected void btnUploadProductRange_Click(object sender, EventArgs e)
    {
        if (fupProductRange.HasFile)
        {
            Common.runSQLNonQuery("delete from productdataloader_apple_productrange_tempload_upload");
            string filename = Path.GetFileNameWithoutExtension(fupProductRange.FileName) + "_" + Common.timestamp() + Path.GetExtension(fupProductRange.FileName);
            string filePath = @"\\10.16.72.129\company\";
            string filePathLocale = "C:\\Linx-tablets\\replen files\\";
            //do some shit here
            try
            {
                try
                {
                    fupProductRange.SaveAs(filePathLocale + filename);

                }
                catch
                {
                    ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('File save failure');", true);
                }
                
                string bulkInsert = string.Format(@"BULK INSERT productdataloader_apple_productrange_tempload_upload FROM '{0}' 
WITH (CODEPAGE = 1252, CHECK_CONSTRAINTS, FIELDTERMINATOR =',', ROWTERMINATOR ='\n', FIRSTROW = 2, FIRE_TRIGGERS  ) ", filePathLocale + filename);
                Common.runSQLNonQuery(bulkInsert);
                if (int.Parse(Common.runSQLScalar("select count(*) from productdataloader_apple_productrange_tempload_upload").ToString()) == 0)
                    throw new Exception();

                string updateSQL = string.Format("exec sp_oracleproductrangeimport_upload '{0}'", fupProductRange.FileName);
                Common.runSQLNonQuery(updateSQL);
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('Upload successful, the range has been updated');", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(this.Page, this.Page.GetType(), "alert", "alert('newUpload unsuccessful, please check the format of the file and comapre with the sample report');", true);
            }
        }
    }

    protected void btnDownloadRange_Click(object sender, EventArgs e)
    {
        string filename = "ProductRange_Existing_" + Common.timestamp() + ".csv";
        runReport("select Applecode, Exertis_Part_Number,Material_Description,Status,EOLDate,Npidate,Npiforecast,EolType from mse_appleproductmapping", filename);
    }
    protected void btnDownloadRangeTemplate_Click(object sender, EventArgs e)
    {
        string filename = "ProductRange_Template_" + Common.timestamp() + ".csv";
        runReport("select *  from mse_appleproductmapping where applecode='999'", filename);
    }
  }
}
