using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SPED_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Util.UserID == 0)
        {
            Response.Redirect(Util.GetData("select Replace(dbo.GetValue('SchoolWeb'),'http','https')", "") + "/database/KillSession.asp");
        }
        //Response.Write(HttpContext.Current.Request.Url.AbsoluteUri);
        //Response.Write(HttpContext.Current.Request.Url.AbsolutePath);

        //Response.Write(HttpContext.Current.Request.Url.AbsoluteUri.Contains("database/dspln/Private.aspx"));

        //HttpContext.Current.Session["UserID"] = 4450;
        //for security reason
        string RemoteIP = HttpContext.Current.Request.UserHostAddress;
        //string LocalIP = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];
        
    }


      [WebMethod]
    public static string H(PEIMS d)
    {
        
        if (Util.UserID == 0)
            throw new Exception("SessionExpired");
        JObject o = new JObject();
        o["view"] = Util.GetData(String.Format("select top 1 * from people where id = {0} and class>0", d.sid[0]), false);
        return o.ToString();
    }

    public class PEIMS
    {
        public string sr { get; set; } //main reciever
        public string[] sid { get; set; } //main reciever
        //data
        public string prec { get; set; } //main reciever
        public string pmode { get; set; } //main reciever
        public string pval { get; set; } //main reciever
        public string dt1 { get; set; } //main reciever
        public string dt2 { get; set; } //main reciever

    }

}