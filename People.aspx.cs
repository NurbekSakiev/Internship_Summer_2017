using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Drawing;
using System.Configuration;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json.Linq;

public partial class _Default : System.Web.UI.Page
{

    [WebMethod]
    public static string H(PEIMS d)
    {
        JObject o = new JObject();
        #region test studnets
        if (d.sr == "*")
        {
            


        }

        if (d.sr == "profile") {

            o["gradDate"] = JObject.Parse(Util.SelectJSON(String.Format(@"Select GraduationDate from Graduates where id= {0}", d.sid[0])));
            o["email"] =  JObject.Parse(Util.SelectJSON(String.Format(@"Select email from Graduates where id= {0}",d.sid[0])));
            o["homePhone"] = JObject.Parse(Util.SelectJSON(String.Format(@"Select HomePhone from Graduates where id= {0}", d.sid[0])));
            o["gradDate"] = JObject.Parse(Util.SelectJSON(String.Format(@"Select GraduationDate from Graduates where id= {0}", d.sid[0])));
            o["birthDate"] = JObject.Parse(Util.SelectJSON(String.Format(@"Select BirthDate from Graduates where id= {0}", d.sid[0])));
            o["gender"] = JObject.Parse(Util.SelectJSON(String.Format(@"Select Gender from Graduates where id= {0}", d.sid[0])));
            o["race"] = JObject.Parse(Util.SelectJSON(String.Format(@"Select Race from Graduates where id= {0}", d.sid[0])));


            int dec = Convert.ToInt32(d.sid[0]) % 100000;
            string url = Util.GetData(@"SELECT    
                    (SELECT concat('https://',Domain,'/database/photo.asp?pid=') FROM Campuses WHERE CampusID=People.Campus) DomainName 
                FROM People 
                WHERE People.ID="+d.sid[0], "") + Util.EncPhoto(dec.ToString());
            o["url"] = url;

        }

        #endregion

        #region KS
        if (d.sr == "ks")
        {
            //o[d.sr] = "test";
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Session.Clear();
 


        }
        #endregion
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



    /// <summary>
    /// /////////////////////////////////////////////////////////////
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    #region ACTIONS

    protected void Page_Load(object sender, EventArgs e)
    {

        //Response.Write(Util.UserID);
        if (!IsPostBack)
        {
            ResetSession();
        }
        string RemoteIP = HttpContext.Current.Request.UserHostAddress;
        string LocalIP = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];
        if (!(RemoteIP.StartsWith("173.11.206.25") || RemoteIP.StartsWith("73.206.59.21") || RemoteIP.StartsWith("64.183.186.162")))///   73.206.59.21   173.11.206.25 64.183.186.162
       {
           //Response.Redirect("https://hsihouston.org");
       }
        Session["IsAdmin"] = Util.GetData("SELECT 1 FROM AllUserPermissions WHERE Permission=30 And [User]=" + Util.UserID, false);
       

    }

    void ResetSession()
    {

        Session["People_QueryList"] = null;
        Session["People_ReportList"] = null;
        Session["People_FormList"] = null;
        Session["IsAdmin"] = null;
        Session["ActiveDay"] = null;

    }

    [WebMethod]
    public static string OpenReport(string sender, string[] args)
    {
        if (Util.UserID == 0)
            throw new Exception("SessionExpired");

        DataTable dt = Util.SelectTable(String.Format(
            "BuildReport {0},'{1}',{2},{3},{4},{5}",
            args[0],
            args[1],//Util.SqlArray(string.Join(",", args[1])),
            Util.SqlString(args[2]),
            Util.SqlString(args[3]),
            Util.UserID,
            0
            ));

        if (dt.Columns.Contains("ReportFile") && dt.Rows.Count > 0)
            return  dt.Rows[0]["ReportFile"].ToString();
        else if (dt.Columns.Contains("ReportError") && dt.Rows.Count > 0)
            return dt.Rows[0]["ReportError"].ToString();
        else
            return "Unable to open the report";
        
    }
    #endregion ACTIONS


    #region SESSIONS
    public static int QueryList
    {
        get
        {
            if (Util.UserID > 0)
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["People_QueryList"] == null)
                {
                    int pg = Util.GetData
                        (
                            "SELECT TOP 1 PermissionID " +
                            "FROM Permissions " +
                            "WHERE NOT Link3 IS NULL And NOT Link4 IS NULL And (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 300 And 399 And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ") " +
                            "ORDER BY (CASE WHEN Exists(SELECT * FROM Preferences WHERE Person=" + Util.UserID + " And Preference='People_QueryList' And Choice=Convert(varchar,PermissionID)) THEN 0 ELSE 1 END), PermissionID"
                    , 0);

                    HttpContext.Current.Session["People_QueryList"] = pg;
                }

                if (HttpContext.Current.Session != null && HttpContext.Current.Session["People_QueryList"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["People_QueryList"]);
            }
            return -1;
        }

        set
        {
            if (Util.UserID > 0)
            {
                if (value >= 0 && Util.GetData("SELECT TOP 1 1 FROM Permissions WHERE NOT Link3 IS NULL And NOT Link4 IS NULL And (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 300 And 399 And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ")", false))
                {
                    HttpContext.Current.Session["People_QueryList"] = value;
                    Util.SavePreferences("People_QueryList", value.ToString());
                }
            }
        }
    }

    public static int ReportList
    {
        get
        {
            if (Util.UserID > 0)
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["People_ReportList"] == null)
                {
                    int rl = Util.GetData
                    (
                            "SELECT TOP 1 PermissionID " +
                            "FROM Permissions " +
                            "WHERE (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 400 And 999 And Link3 IS NULL And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ") " +
                            "ORDER BY (CASE WHEN Exists(SELECT * FROM Preferences WHERE Person=" + Util.UserID + " And Preference='People_ReportList' And Choice=Convert(varchar,PermissionID)) THEN 0 ELSE 1 END), Permission"
                    , 0);

                    HttpContext.Current.Session["People_ReportList"] = rl;
                }

                if (HttpContext.Current.Session != null && HttpContext.Current.Session["People_ReportList"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["People_ReportList"]);
            }
            return -1;
        }

        set
        {
            if (Util.UserID > 0)
            {
                if (value >= 0 && Util.GetData("SELECT TOP 1 1 FROM Permissions WHERE (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 400 And 999 And Link3 IS NULL And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ")", false))
                {
                    HttpContext.Current.Session["People_ReportList"] = value;
                    Util.SavePreferences("People_ReportList", value.ToString());
                }
            }
        }
    }

    public static int FormList
    {
        get
        {
            if (Util.UserID > 0)
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["People_FormList"] == null)
                {
                    int rl = Util.GetData(String.Format
                    (@"
                        SELECT TOP 1 Convert(int,Link2) Link2
                        FROM Permissions
                        WHERE
                            IsNumeric(Link2)=1 And
                            PermissionID Between 200 And 299 And 
                            (CASE {0} WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And
                            Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]={1})
                        ORDER BY (CASE WHEN Exists(SELECT * FROM Preferences WHERE Person={1} And Preference='People_FormList' And Choice=Link2) THEN 1 ELSE 2 END), Permission
                    ", Util.PeopleGroup, Util.UserID), 0);

                    HttpContext.Current.Session["People_FormList"] = rl;
                }

                if (HttpContext.Current.Session != null && HttpContext.Current.Session["People_FormList"] != null)
                    return Convert.ToInt32(HttpContext.Current.Session["People_FormList"]);
            }
            return -1;
        }

        set
        {
            if (Util.UserID > 0)
            {
                if (value >= 0 && Util.GetData("SELECT TOP 1 1 FROM Permissions WHERE IsNumeric(Link2)=1 And (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 200 And 299 And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ")", false))
                {
                    HttpContext.Current.Session["People_FormList"] = value;
                    Util.SavePreferences("People_FormList", value.ToString());
                }
            }
        }
    }
    #endregion SESSIONS


    [WebMethod]
    public static string PeopleHandler(string sender, string[] args)
    {
        //if (Util.UserID == 0)
        //    return "SessionExpired";

        PEOPLE jclass = new PEOPLE(sender, args);
        
        return jclass.Post();
    }


    class PEOPLE
    {
        string json = "";
        string sender = "";
        string[] args;

        public PEOPLE(string s, string[] a)
        {
            sender = s;
            args = a;
        }

        void AddJSON(string js)
        {
            Util.AddJSON(ref json, js);
        }

        void AddJSON(string prm, string val)
        {
            Util.AddJSON(ref json, prm, val);
        }

        void AddJSON(string prm, int val)
        {
            Util.AddJSON(ref json, prm, val);
        }

        void AddJSON(string prm, bool val)
        {
            Util.AddJSON(ref json, prm, val);
        }

        public string Post()
        {
            //MultiTables();

            JObject o = new JObject();

            #region Main
            if (sender == "All")
            {
                //return Util.UserID.ToString();
                Util.SelectedPerson = 0;
            }
            
            if (sender == "PeopleList")
                {
                    if (args != null && args.Length == 1)
                    {
                        if (args[0].Length > 0)
                            Util.SelectedPerson = Convert.ToInt32(Util.strFirst(args[0]));
                        else
                            Util.SelectedPerson = 0;
                    }
                }

            #endregion

            #region LoadPeopleGroupList
                if (sender == "PeopleGroup" && args != null && args.Length == 1 && Util.IsNumeric(args[0]))
                {
                    Util.PeopleGroup = Convert.ToInt32(args[0]);
                }

                if (sender == "All")
                {

             //       o["PeopleGroup"] = JObject.Parse(Util.SelectJSON(
             //  "SELECT Permissions.Permission, PermissionID-89 AS PermissionID " +
             //               "FROM Permissions INNER JOIN UserPermissions ON Permissions.PermissionID = UserPermissions.Permission " +
             //               "WHERE [User]=" + Util.UserID + " And PermissionID Between 90 And 99 " +
             //               "ORDER BY PermissionID"
             //));


                    AddJSON("\"PeopleGroup\":" + Util.SelectJSON
                    (
                        "SELECT Permissions.Permission, PermissionID-89 AS PermissionID ,2 Selected " +
                            "FROM Permissions INNER JOIN UserPermissions ON Permissions.PermissionID = UserPermissions.Permission " +
                            "WHERE [User]=" + Util.UserID + " And PermissionID Between 90 And 99 " +
                            "ORDER BY PermissionID"
                    ));
                }
            #endregion

            #region LoadQueryList
            if (sender == "QueryList" && args != null && args.Length == 1 && Util.IsNumeric(args[0]))
            {
                QueryList = Convert.ToInt32(args[0]);
            }


            if (sender == "All" || sender == "PeopleGroup")
            {
                AddJSON("\"QueryList\":" + Util.SelectJSON
                (
                    "SELECT Permission, PermissionID " +
                                "FROM Permissions " +
                                "WHERE (CASE " + (Util.PeopleGroup <= 0 ? 5 : Util.PeopleGroup) + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 300 And 399 And Exists(SELECT * FROM UserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ") " +
                                "ORDER BY Permission"
                ));
            }
            #endregion


            #region start end date
            //if (sender == "StartDate")
            //{ 
            //    HttpContext.Current.Session["StartDate"] = args[0];
            //}

            //if (sender == "EndDate")
            //{
            //    HttpContext.Current.Session["EndDate"] = args[0];

            //}

            #endregion

            #region LoadSubList
            if (sender == "All" || sender == "PeopleGroup" || sender == "QueryList")
            {
                string SQL1 = Util.GetData("SELECT Link3 FROM Permissions WHERE PermissionID=" + (QueryList < 300 ? 300 : QueryList), "");
                
                if (SQL1.Length > 0) //&& Util.PeopleGroup > 0)
                {
                    AddJSON("\"SubList\":" + Util.SelectJSON
                    (
                        SQL1
                        .Replace("@Cat", Util.PeopleGroup.ToString())
                        .Replace("@UserID", "1")
                        .Replace("@Filter", "1=1")
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Replace("\t", " ")
                    ));
                }
            }
            #endregion

            #region LoadPeopleList
            string SubList = "NULL";
            string PeopleNameFilter = "";
            string PeopleClassFilter = "";
            string PeopleNoteFilter = "";
            string PeopleSort = "";
            string PeopleNameSort = "";

            if (sender == "SubList" && args != null && args.Length == 6)
            {
                SubList = args[0];
                PeopleNameFilter = args[1];
                PeopleClassFilter = args[2];
                PeopleNoteFilter = args[3];
                PeopleSort = args[4];
                PeopleNameSort = args[5];
            }

            string nsort = PeopleNameSort == "LastName" ? "LastName + ', ' + FirstName" : "FirstName + ' ' + LastName";

            if (sender == "All" || sender == "PeopleGroup" || sender == "QueryList" || sender == "SubList" || sender == "PeopleFilter")
            {
                string SQL2 = Util.GetData("SELECT Link4 FROM Permissions WHERE Link3>'' And Link4>'' And PermissionID=" + (QueryList < 300 ? 300 : QueryList), "");

                if (SQL2.Length > 0)// && Util.PeopleGroup > 0)
                {
                    AddJSON("\"PeopleList\":" + Util.SelectJSON
                    (
                        "SELECT " +
                                    "ID, " +
                                    nsort + " FullName, " +
                                    "Class, " +
                                    "Note " +
                                "FROM (" +
                        SQL2
                        .Replace("@CRITERIA", SubList.Length > 0 ? SubList : "NULL")
                        .Replace("@UserID", Util.UserID.ToString())
                        .Replace("\r", " ")
                        .Replace("\n", " ")
                        .Replace("\t", " ") +
                    ") PeopleList " +
                    " WHERE " + (PeopleNameFilter.Length > 0 ? " CharIndex(' ' + " + Util.SqlString(PeopleNameFilter) + ",' ' + FirstName + ' ' + LastName)>0" : "1=1") +
                    " AND " + (PeopleClassFilter.Length > 0 ? " CharIndex(' ' + " + Util.SqlString(PeopleClassFilter) + ",' ' + Replace(Class,'-',' '))>0" : "1=1") +
                    " AND " + (PeopleNoteFilter.Length > 0 ? " CharIndex(' ' + " + Util.SqlString(PeopleNoteFilter) + ",' ' + Note)>0" : "1=1") +
                    "ORDER BY " + (PeopleSort.Length > 0 ? (PeopleSort + ", ") : "") + nsort
                    ));
                }
            }
            #endregion


            //< img alt = "No Photo" height = "100" width = "100" id = "SelPersonPhoto" src = "../photo.asp?pid=ediafajh" >

            #region LoadReportList
            //if (sender == "ReportList" && args != null && args.Length == 1 && Util.IsNumeric(args[0]))
            //    ReportList = Convert.ToInt32(args[0]);

            //if (sender == "All" || sender == "PeopleGroup")
            //{
            //    AddJSON(Util.SelectJSON
            //    (
            //        "SELECT Permission, PermissionID , (CASE WHEN PermissionID=" + ReportList + " THEN 1 ELSE 0 END) Selected " +
            //        "FROM Permissions " +
            //        "WHERE (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 400 And 999 And Link3 IS NULL And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ") " +
            //        "ORDER BY Permission",
            //        "ReportList"
            //    ));
            //}
            #endregion

            #region LoadFormList
            //if (sender == "FormList" && args != null && args.Length == 1 && Util.IsNumeric(args[0]))
            //    FormList = Convert.ToInt32(args[0]);

            //if (sender == "All" || sender == "PeopleGroup")
            //{
            //    AddJSON(Util.SelectJSON
            //    (
            //        "SELECT Permission, Convert(int,Link2) Link2 , (CASE WHEN Link2='" + FormList + "' THEN 1 ELSE 0 END) Selected " +
            //        //"SELECT Permission, Convert(int,Link2) Link2 , (CASE WHEN Link2=3 THEN 1 ELSE 0 END) Selected " +
            //        "FROM Permissions " +
            //        "WHERE IsNumeric(Link2)=1 And (CASE " + Util.PeopleGroup + " WHEN 1 THEN Students WHEN 2 THEN Withdrawns WHEN 3 THEN (CASE WHEN Students=1 OR Withdrawns=1 THEN 1 ELSE 0 END) WHEN 4 THEN Applicants WHEN 5 THEN Staff WHEN 6 THEN OldStaff WHEN 7 THEN JobApplicant WHEN 8 THEN Other END)=1 And PermissionID Between 200 And 299 And Exists(SELECT * FROM AllUserPermissions WHERE Permission=Permissions.PermissionID And [User]=" + Util.UserID + ") " +
            //        "ORDER BY Permission",
            //        "FormList"
            //    ));
            //}
            #endregion

            return json.Length > 0 ? ("{" + json + "}") : "";
        }

        public void MultiTables()
        {
            DataSet ds = Util.SelectTables("MultiTable");
            DataTable d1 = ds.Tables[0];
            DataTable d2 = ds.Tables[1];
        }
    }

    [WebMethod]
    public static void KillSession()
    {
        HttpContext.Current.Session.Abandon();
        HttpContext.Current.Session.Clear();
 
    }
}
