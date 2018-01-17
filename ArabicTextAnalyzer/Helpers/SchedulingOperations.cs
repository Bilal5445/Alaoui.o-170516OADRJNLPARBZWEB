
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net;
using ArabicTextAnalyzer.Domain.Models;
using System.Data.Entity;
using System.Configuration;
using System.Data.SqlClient;
using OADRJNLPCommon.Models;
using Dapper;
using ArabicTextAnalyzer.BO;
using ArabicTextAnalyzer;
using ArabicTextAnalyzer.Models;
using System.Net.Mail;
using System.Data;

namespace IcoApp.WebUI.Helpers
{

    public class SchedulingOperations
    {
        public string CurrentServerPath { get; set; }
        public SchedulingOperations(string currentServerPath)
        {
            CurrentServerPath = currentServerPath;
        }

        public void RunAllAutoSchedulingTasks()
        {

            try
            {
                GlobalVariables.SchedulingTasksGlobal.IsStartAutoScheduleTask = true;
                if (GlobalVariables.SchedulingTasksGlobal.PeriodicSchedulingTasks != null)
                {
                    GlobalVariables.SchedulingTasksGlobal.PeriodicSchedulingTasks.Cancel();
                    CancelTokenSchedulingTask();
                }

                var cancelToken = new CancellationTokenSource();

                Task perdiodicSchedulingTasks = Task.Factory.StartNew(obj =>
                {
                    var tk = (CancellationTokenSource)obj;
                    while (!tk.IsCancellationRequested)
                    {
                        var timeIntervalToCheck = 1000;/// *60 * 1;// Check Every 5 min to make sure the New email account is Added.
                        Thread.Sleep(timeIntervalToCheck);
                        if (GlobalVariables.SchedulingTasksGlobal.IsStartAutoScheduleTask)
                        {
                            
                            var db = new ArabiziDbContext();

                            var scheduleTasks = db.ScheduleTasks.Where(s => s.Active == true).ToList();
                            var scheduleTaskDate = GlobalVariables.CurrentDateTime;
                            foreach (var item in scheduleTasks)
                            {
                                DateTime? taskToRunDate = null;
                                if (item.TaskType == GlobalVariables.TaskType.Daily)
                                {
                                    taskToRunDate = (item.NextRunDate.Value.ToShortDateString() + " " + item.TimeToStart).ToDateTime();
                                }
                                else
                                {
                                    taskToRunDate = item.NextRunDate;
                                }


                                if (taskToRunDate <= GlobalVariables.CurrentDateTime)
                                {
                                    try
                                    {
                                        Type type = typeof(SchedulingOperations);
                                        MethodInfo info = type.GetMethod(item.MethodName);
                                        if (info != null)
                                        {
                                            info.Invoke(new SchedulingOperations(CurrentServerPath), null);
                                        }
                                    }
                                    catch (Exception ex)
                                    {


                                    }
                                    DateTime? nextRunDate = null;
                                    if (item.TaskType == GlobalVariables.TaskType.Daily)
                                    {
                                        nextRunDate = ((GlobalVariables.CurrentDateTime.Value.AddDays(item.RepeatDays)).ToShortDateString() + " " + item.TimeToStart).ToDateTime();
                                    }
                                    else if (item.TaskType == GlobalVariables.TaskType.Hourly)
                                    {
                                        nextRunDate = GlobalVariables.CurrentDateTime.Value.AddHours(item.RepeatDays);
                                    }
                                    else if (item.TaskType == GlobalVariables.TaskType.Monthly)
                                    {
                                        nextRunDate = ((GlobalVariables.CurrentDateTime.Value.AddMonths(item.RepeatDays)).ToShortDateString() + " " + item.TimeToStart).ToDateTime();
                                    }
                                    else if (item.TaskType == GlobalVariables.TaskType.Yearly)
                                    {
                                        nextRunDate = ((GlobalVariables.CurrentDateTime.Value.AddYears(item.RepeatDays)).ToShortDateString() + " " + item.TimeToStart).ToDateTime();
                                    }
                                    else if (item.TaskType == GlobalVariables.TaskType.Minutes)
                                    {
                                        nextRunDate = GlobalVariables.CurrentDateTime.Value.AddMinutes(item.RepeatDays);
                                    }
                                    item.NextRunDate = nextRunDate;
                                    db.Entry(item).State = EntityState.Modified;
                                    db.SaveChanges();                                  
                                }
                            }
                        }

                    }
                }, cancelToken);

                //perdiodicTaskGetallTheEmailBoxes.di

                GlobalVariables.SchedulingTasksGlobal.PeriodicSchedulingTasks = cancelToken;

            }
            catch (Exception exception)
            {

            }
        }

        public void CancelTokenSchedulingTask()
        {
            try
            {
                var tokenSource2 = GlobalVariables.SchedulingTasksGlobal.PeriodicSchedulingTasks;
                CancellationToken ct = tokenSource2.Token;

                var task = Task.Factory.StartNew(() =>
                {

                    // Were we already canceled?
                    ct.ThrowIfCancellationRequested();

                    bool moreToDo = true;
                    while (moreToDo)
                    {
                        // Poll on this property if you have to do
                        // other cleanup before throwing.
                        if (ct.IsCancellationRequested)
                        {
                            // Clean up here, then...
                            ct.ThrowIfCancellationRequested();
                        }

                    }
                }, tokenSource2.Token); // Pass same token to StartNew.

                tokenSource2.Cancel();

                // Just continue on this thread, or Wait/WaitAll with try-catch:
                try
                {
                    task.Wait();
                }
                catch (AggregateException e)
                {
                    foreach (var v in e.InnerExceptions)
                        Console.WriteLine(e.Message + " " + v.Message);
                }
                finally
                {
                    tokenSource2.Dispose();
                }

            }
            catch (Exception ex)
            {


            }
        }
        

        public void AutoGainFbPosts()
        {            
            var db = new ArabiziDbContext();          
            var date = DateTime.Now;
            //Get all the influencer list from the table;
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            var influencerList = new List<T_FB_INFLUENCER>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_FB_INFLUENCER";

                conn.Open();
                influencerList= conn.Query<T_FB_INFLUENCER>(qry).ToList();
            }
            if(influencerList!=null && influencerList.Count>0)
            {
                foreach(var item in influencerList)
                {
                    if(item!=null)
                    {
                        try
                        {
                            try
                            {
                                object status = RetrieveFBPost(item, date);//.url_name,item.id,date,item.fk_theme);
                            }
                            catch(Exception e)
                            {

                            }
                            
                                           
                        }
                        catch(Exception e)
                        {

                        }
                    }
                }
            }           
        }

        public async Task<object> RetrieveFBPost(T_FB_INFLUENCER influencer,DateTime? date)
        {
            string errMessage = string.Empty;
            bool status = false;
            string translatedstring = "";

            string result = null;

            var url = ConfigurationManager.AppSettings["FBWorkingAPI"] + "/" + "Data/FetchFBInfluencerPosts?CallFrom=" + influencer.url_name;
            result = await HtmlHelpers.PostAPIRequest(url, "", type: "POST");

            if (result.ToLower().Contains("true"))
            {
                status = true;
                // translatedstring = result;
                TranslateFBPost(influencer, date);
                //GetFBPostAndComments(influencer,date);
                // return true;
            }
            else
            {
                errMessage = result;
                //return false;
            }
            return status;
        }

        public void GetFBPostAndComments(T_FB_INFLUENCER influencer,DateTime? date)
        {           
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            //Get the fb_post that is added recently by scheduling.
            var fbPost = new List<FB_POST>();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT * FROM T_FB_POST WHERE fk_influencer = '" + influencer.id + "' and EntryDate>='" + date + "' ";
                    conn.Open();
                    fbPost = conn.Query<FB_POST>(qry).ToList();                    
                }
            }
            catch (Exception e)
            {

            }
            if (fbPost != null && fbPost.Count > 0)
            {
                foreach (var post in fbPost)
                {
                    //Translate the fb post and retrieve the comments and translate it.
                   // TranslateFBPost(post,influencer,date);

                }
                //  var abc = loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL();
            }
          
        }


        public async Task<object> TranslateFBPost( T_FB_INFLUENCER influencer,DateTime? date)
        {
            var influencerTheme = new M_XTRCTTHEME();
            Object thisLock = new Object();
            var isNegative = false;
            string errMessage = string.Empty;
            object status = false;
            string translatedstring = "";


            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            //Get the fb_post that is added recently by scheduling.
            var fbPost = new List<FB_POST>();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT * FROM T_FB_POST WHERE fk_influencer = '" + influencer.id + "' and EntryDate>='" + date + "' ";
                    conn.Open();
                    fbPost = conn.Query<FB_POST>(qry).ToList();
                }
            }
            catch (Exception e)
            {

            }
            if (fbPost != null && fbPost.Count > 0)
            {
             
                foreach (var post in fbPost)
                {
                    try
                    {
                        var url = ConfigurationManager.AppSettings["TranslateDomain"] + "/" + "api/Arabizi/GetArabicDarijaEntryForFbPost?text=" + post.post_text;
                        var result = await HtmlHelpers.PostAPIRequest(url, "", type: "GET");

                        if (result.Contains("Success") || string.IsNullOrEmpty(post.post_text))
                        {
                            if (result.Contains("Success"))
                            {
                                try
                                {
                                    status = true;
                                    translatedstring = result.Replace("Success", "");
                                    var @singleQuote = "CHAR(39)";
                                    translatedstring = translatedstring.Replace("'", "");
                                    var returndata = SaveTranslatedPost(post.id, translatedstring);


                                    // MC081217 furthermore translate via train to populate NER, analysis data, ... (TODO LATER : should be the real code instead of API or API should do the real complete work)
                                    // Arabizi to arabic script via direct call to perl script

                                    String ConnectionString2 = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

                                    using (SqlConnection conn = new SqlConnection(ConnectionString2))
                                    {
                                        String qry = "SELECT * FROM T_XTRCTTHEME WHERE ID_XTRCTTHEME = '" + influencer.fk_theme + "'";

                                        conn.Open();
                                        influencerTheme = conn.QueryFirst<M_XTRCTTHEME>(qry);
                                    }
                                    if (influencerTheme != null)
                                    {
                                        try
                                        {
                                            dynamic expando = new Arabizer().train(new M_ARABIZIENTRY
                                            {
                                                ArabiziText = post.post_text.Trim(),
                                                ArabiziEntryDate = DateTime.Now
                                            }, influencerTheme.ThemeName, thisLock: thisLock);

                                            // keep only arabizi + arabic + ner
                                            expando.M_ARABICDARIJAENTRY_LATINWORDs = null;

                                            // limit to positive/negative ner
                                            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = expando.M_ARABICDARIJAENTRY_TEXTENTITYs;

                                            if (textEntities.Where(c => c.TextEntity.Type == "NEGATIVE").Count() > 0)
                                            {
                                                isNegative = true;
                                            }
                                        }
                                        catch (Exception e)
                                        {

                                        }

                                    }
                                }
                                catch (Exception e)
                                {

                                }

                            }
                            status = GetFBComments(post, influencer, date, isNegative);
                        }              
                    }
                    catch(Exception e)
                    {

                    }
                }
            }

            return status;
        }

        public async Task<object> GetFBComments(FB_POST post, T_FB_INFLUENCER influencer,DateTime? date,bool isNegative)
        {
            var influencerTheme = new M_XTRCTTHEME();
            bool resultmail = false;
            Object thisLock = new Object();
            var isNegativeComments = false;
            List<FBFeedComment> comments= new List<FBFeedComment>();
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            string id = post.id.Split('_')[1];
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "";
                if (!string.IsNullOrEmpty(post.id))
                {
                    qry = "select * from FBFeedComments where id like '" + id + "%' and entrydate>='"+date+"'";
                }             
                conn.Open();
                comments= conn.Query<FBFeedComment>(qry).ToList();
            }
            if(comments!=null && comments.Count>0)
            {
                foreach(var comment in comments)
                {
                    string translatedstring = "";
                    try
                    {
                        var url = ConfigurationManager.AppSettings["TranslateDomain"] + "/" + "api/Arabizi/GetArabicDarijaEntryForFbPost?text=" + comment.message;
                        var result = await HtmlHelpers.PostAPIRequest(url, "", type: "GET");

                        if (result.Contains("Success"))
                        {
                            //status = true;
                            translatedstring = result.Replace("Success", "");
                            var @singleQuote = "CHAR(39)";
                            translatedstring = translatedstring.Replace("'", "");
                            try
                            {
                                var returndata = SaveTranslatedComments(comment.Id, translatedstring);                              
                            }
                            catch (Exception e)
                            {

                            }

                            // MC081217 furthermore translate via train to populate NER, analysis data, ... (TODO LATER : should be the real code instead of API or API should do the real complete work)
                            // Arabizi to arabic script via direct call to perl script

                            String ConnectionString2 = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

                            using (SqlConnection conn = new SqlConnection(ConnectionString2))
                            {
                                String qry = "SELECT * FROM T_XTRCTTHEME WHERE ID_XTRCTTHEME = '" + influencer.fk_theme + "'";

                                conn.Open();
                                influencerTheme = conn.QueryFirst<M_XTRCTTHEME>(qry);
                            }
                            if (influencerTheme != null)
                            {
                                try
                                {
                                    dynamic expando = new Arabizer().train(new M_ARABIZIENTRY
                                    {
                                        ArabiziText = comment.message.Trim(),
                                        ArabiziEntryDate = DateTime.Now
                                    }, influencerTheme.ThemeName, thisLock: thisLock);

                                    // keep only arabizi + arabic + ner
                                    expando.M_ARABICDARIJAENTRY_LATINWORDs = null;

                                    // limit to positive/negative ner
                                    List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = expando.M_ARABICDARIJAENTRY_TEXTENTITYs;

                                    if (textEntities.Where(c => c.TextEntity.Type == "NEGATIVE").Count() > 0)
                                    {
                                        isNegativeComments = true;
                                    }
                                }
                                catch (Exception e)
                                {

                                }

                            }
                        }
                    }
                   catch(Exception e)
                    {

                    }
                                           
                }
            }
            if(isNegative==true||isNegativeComments==true)
            {                
                    var userid = influencerTheme.UserID;

                    var db1 = new ApplicationDbContext();
                    var user = db1.Users.FirstOrDefault(c => c.Id == userid);
                    if (user != null)
                    {
                        try
                        {
                            var email = user.Email;
                            if (!string.IsNullOrEmpty(email))
                            {
                                string subject = "Negative word on fb post";
                                string body = "Hello user,<br/>You have some negative words on posts on your facebook page" + influencer.name + ".<br/>Please remove the words from the posts.<br/>Thanks.";
                                SendEmail(email, subject, body);
                            resultmail = true;
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }
                
            }
            return resultmail;
        }

        private int SaveTranslatedPost(string postid, string TranslatedText)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            int returndata = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "";
                    if (!string.IsNullOrEmpty(postid) && !string.IsNullOrEmpty(TranslatedText))
                    {
                        qry = "update T_FB_POST set translated_text=N'" + TranslatedText + "' where id='" + postid + "'";
                    }
                    using (SqlCommand cmd = new SqlCommand(qry, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        returndata = cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                }
            }
            catch (Exception e)
            {
                returndata = 0;
            }
            return returndata;
        }

        private int SaveTranslatedComments(string postid, string TranslatedText)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            int returndata = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "";
                    if (!string.IsNullOrEmpty(postid) && !string.IsNullOrEmpty(TranslatedText))
                    {
                        qry = "update FBFeedComments set translated_message=N'" + TranslatedText + "' where id='" + postid + "'";
                    }
                    using (SqlCommand cmd = new SqlCommand(qry, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        returndata = cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    // conn.Open();

                    // return conn.Query<FB_POST>(qry).ToList();
                }
            }
            catch (Exception e)
            {
                returndata = 0;
            }
            return returndata;

        }


        public void SendEmail( string toEmailAddress,string subject,string body)
        {
            try
            {
                var emailsetting = UtilityFunctions.GetEmailSetting();
                var username = emailsetting.SMTPServerLoginName;
                string password = emailsetting.SMTPServerPassword;               

                MailMessage msg = new MailMessage();
                msg.Subject = subject;


                msg.From = new MailAddress(emailsetting.NoReplyEmailAddress);

                foreach (var email in toEmailAddress.Split(','))
                {
                    msg.To.Add(new MailAddress(email));
                }
                var bccAddress = ConfigurationManager.AppSettings["BCCEmailAddress"];
                if (!string.IsNullOrEmpty(bccAddress))
                {
                    msg.Bcc.Add(new MailAddress(bccAddress));
                }

                msg.IsBodyHtml = true;
                msg.Body = body;

               
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = emailsetting.SMTPServerUrl;
                smtpClient.Port = emailsetting.SMTPServerPort;



                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = emailsetting.SMTPSecureConnectionRequired;
                smtpClient.Credentials = new System.Net.NetworkCredential(username, password);
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }




    }

    public static class GlobalVariables
    {
        public class SchedulingTasksGlobal
        {
            public static bool IsStartAutoScheduleTask { get; set; }
            public static CancellationTokenSource PeriodicSchedulingTasks { get; set; }
        }

        public static DateTime? CurrentDateTime { get { return DateTime.Now; } }
        public class TaskType
        {
            public static int Hourly = 0;
            public static int Daily = 1;
            public static int Monthly = 2;
            public static int Yearly = 3;
            public static int Minutes = 4;
        }

        public static DateTime ToDateTime(this string value)
        {
            DateTime result = DateTime.Now;
            if(value!=null)
            {
                DateTime.TryParse(value, out result);
            }
            return result;
        }
    }

    public static class UtilityFunctions
    {
        public static EmailSettings GetEmailSetting()
        {
            EmailSettings emailsetting = new EmailSettings();        
                emailsetting.SMTPServerUrl = ConfigurationManager.AppSettings["SMTPServerUrl1"];
                emailsetting.SMTPServerPort = Convert.ToInt16(ConfigurationManager.AppSettings["SMTPServerPort1"]);
                emailsetting.SMTPSecureConnectionRequired = Convert.ToBoolean(ConfigurationManager.AppSettings["SMTPSecureConnectionRequired1"]);
                emailsetting.SMTPServerLoginName = ConfigurationManager.AppSettings["SMTPServerLoginName1"];
                emailsetting.SMTPServerPassword = ConfigurationManager.AppSettings["SMTPServerPassword1"];
                emailsetting.NoReplyEmailAddress = ConfigurationManager.AppSettings["NoReplyEmailAddress1"];           
            return emailsetting;
        }
    }

    public class EmailSettings
    {
        public string SMTPServerUrl { get; set; }
        public int SMTPServerPort { get; set; }
        public bool SMTPSecureConnectionRequired { get; set; }
        public string SMTPServerLoginName { get; set; }
        public string SMTPServerPassword { get; set; }
        public string NoReplyEmailAddress { get; set; }
    }

}

