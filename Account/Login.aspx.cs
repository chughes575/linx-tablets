using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace linx_tablets.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack && User.Identity.IsAuthenticated) 
            {
                Response.Redirect("~/Reporting/AppleReporting.aspx");
            }
        }
        
    }
}