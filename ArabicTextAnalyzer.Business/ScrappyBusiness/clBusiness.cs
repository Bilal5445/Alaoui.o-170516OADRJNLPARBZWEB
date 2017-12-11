using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Business.ScrappyBusiness
{
    public class clBusiness
    {
        /*public static void getTwitterApplications(ref List<TwitterApplication> _appList)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {
                _appList = context.TwitterApplications
                                      .ToList();
            }
        }

        public static void searchInTwitter(int? appid, Search searchTwitter, ref string Message)
        {
            try
            {

                //Search searchTwitter = getSearchCriteria();

                HttpWebRequest request = (HttpWebRequest)CreateOauthAndRequest(getApplicationDetails(appid), searchTwitter);
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                var objText = reader.ReadToEnd();
                try
                {
                    JObject jObjects = JObject.Parse(objText);
                    foreach (var v in jObjects)
                    {
                        JObject Objects = new JObject(jObjects);
                        JArray items = (JArray)Objects["statuses"];
                        var length = items.Count;
                        if (length == 0)
                        {
                            Message = "No tweet(s) found against the provided location, try to change the location/latitude,longitude by dragg /move and left clicking on the map";
                            break;
                        }
                        foreach (var status in Objects["statuses"])
                        {
                            ScrapyWeb.Models.TweetSet tweet = new ScrapyWeb.Models.TweetSet();

                            getTweetFromJObj(status, ref tweet);
                            AddTweetTODb(tweet);
                        }
                    }

                }
                catch (Exception twit_error)
                {
                    Console.WriteLine(twit_error.ToString());
                }

            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        public static void searchInTwitterPlaces(ref string Message)
        {
            try
            {

                ScrapyWeb.Models.TwitterApplication app = getApplicationDetails(null);
                //oauth application keys
                var oauth_token = app.AccessToken;//Util.getKeyValueFromAppSetting("oauth_token");
                var oauth_token_secret = app.AccessTokenSecret;// Util.getKeyValueFromAppSetting("oauth_token_secret");
                var oauth_consumer_key = app.ConsumerKey;// Util.getKeyValueFromAppSetting("oauth_consumer_key");
                var oauth_consumer_secret = app.ConsumerSecret;// Util.getKeyValueFromAppSetting("oauth_consumer_secret");

                //oauth implementation details
                var oauth_version = "1.0";
                var oauth_signature_method = "HMAC-SHA1";

                // unique request details
                var oauth_nonce = Convert.ToBase64String(
                    new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
                var timeSpan = DateTime.UtcNow
                    - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
                // var geocode = "33.6436653,-6.8618025,15mi";
                var query = new StringBuilder().Append("Morocco").ToString();


                // create oauth signature
                var baseFormat = "";
                baseFormat = "geocode={6}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";//&count=" + search.Count_toSearch;//&rpp=" + tweetCount + "&include_entities=true" + "&page=" + page +"&until=

                var baseString = string.Format(baseFormat,
                                            oauth_consumer_key,
                                            oauth_nonce,
                                            oauth_signature_method,
                                            oauth_timestamp,
                                            oauth_token,
                                            oauth_version,
                                            Uri.EscapeDataString(query)
                                            );

                baseString = string.Concat("GET&", Uri.EscapeDataString("https://api.twitter.com/1.1/geo/search.json"), "&", Uri.EscapeDataString(baseString));

                var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                        "&", Uri.EscapeDataString(oauth_token_secret));

                string oauth_signature;
                using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
                {
                    oauth_signature = Convert.ToBase64String(
                        hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
                }

                // create the request header
                var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                   "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                   "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                   "oauth_version=\"{6}\"";

                var authHeader = string.Format(headerFormat,
                                        Uri.EscapeDataString(oauth_nonce),
                                        Uri.EscapeDataString(oauth_signature_method),
                                        Uri.EscapeDataString(oauth_timestamp),
                                        Uri.EscapeDataString(oauth_consumer_key),
                                        Uri.EscapeDataString(oauth_token),
                                        Uri.EscapeDataString(oauth_signature),
                                        Uri.EscapeDataString(oauth_version)
                                );


                ServicePointManager.Expect100Continue = false;
                var URL = "https://api.twitter.com/1.1/geo/search.json";

                URL = URL + "?query=" + query;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";


                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream());
                var objText = reader.ReadToEnd();
                try
                {
                    JObject jObjects = JObject.Parse(objText);
                    foreach (var v in jObjects)
                    {
                        JObject Objects = new JObject(jObjects);
                        JArray items = (JArray)Objects["statuses"];
                        var length = items.Count;
                        if (length == 0)
                        {
                            Message = "No tweet(s) found against the provided location, try to change the location/latitude,longitude by dragg /move and left clicking on the map";
                            break;
                        }
                        foreach (var status in Objects["statuses"])
                        {
                            ScrapyWeb.Models.TweetSet tweet = new ScrapyWeb.Models.TweetSet();

                            getTweetFromJObj(status, ref tweet);
                            AddTweetTODb(tweet);


                        }
                    }



                }
                catch (Exception twit_error)
                {
                    Console.WriteLine(twit_error.ToString());
                }

            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        public static Search getSearchCriteria()
        {
            return new Search()
            {
                Latitude = Convert.ToDouble(Util.getKeyValueFromAppSetting("Latitude"), CultureInfo.InvariantCulture.NumberFormat),
                Longitude = Convert.ToDouble(Util.getKeyValueFromAppSetting("Longitude"), CultureInfo.InvariantCulture.NumberFormat),
                Radius = Convert.ToInt32(Util.getKeyValueFromAppSetting("Radius")),
                IsRadiusInMiles = Convert.ToBoolean(Util.getKeyValueFromAppSetting("IsRadiusInMile")),
                URL = Util.getKeyValueFromAppSetting("resource_url"),
                Count_toSearch = Convert.ToString(Util.getKeyValueFromAppSetting("Count_toSearch")),
                TimeLineURL = Util.getKeyValueFromAppSetting("TimeLineURL")
            };
        }

        public static void AddTweetTODb(TweetSet tweet)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {
                var result = context.TweetSets.SingleOrDefault(t => t.Tweet_Id == tweet.Tweet_Id);
                if (result == null)
                {

                    context.TweetSets.Add(tweet);
                    context.SaveChanges();
                }
            }
        }

        static void getTweetFromJObj(dynamic jobj, ref TweetSet tweet)
        {
            var HashTags = "";
            var Mentions = "";

            JObject jj = new JObject(jobj["entities"]);
            var hashTags = jj["hashtags"];
            foreach (var hashTag in hashTags)
            {
                HashTags += hashTag["text"] + ",";
            }

            var userMentions = jj["user_mentions"];
            foreach (var mention in userMentions)
            {
                Mentions += "@" + mention["screen_name"] + ",";
            }

            var geo = jobj["geo"];
            var place = jobj["place"];
            var user = jobj["user"];
            var createdOn = Convert.ToString(jobj["created_at"]);
            string Const_TwitterDateTemplate = "ddd MMM dd HH:mm:ss +ffff yyyy";
            DateTime createdAt = DateTime.ParseExact(createdOn, Const_TwitterDateTemplate, new System.Globalization.CultureInfo("en-US"));
            tweet.Tweet_Id = Convert.ToString(jobj["id"]);
            tweet.TweetText = Convert.ToString(jobj["text"]);
            tweet.DateDownload = DateTime.Now;
            tweet.DateCreated = createdAt;
            tweet.HashTags = HashTags;
            tweet.Mentions = Mentions;
            tweet.ScreenName = Convert.ToString(user["screen_name"]);
            tweet.UserId = Convert.ToString(user["id"]);
            if (geo.HasValues == null)
                tweet.LatLong = Convert.ToString(Convert.ToString(geo["coordinates"])).Replace("]", "").Replace("[", "").Replace("\r\n", "");
            if (place.HasValues == null)
            {
                tweet.PlaceId = Convert.ToString(place["id"]);
                tweet.PlaceName = Convert.ToString(place["name"]);
            }
            tweet.Language = Convert.ToString(jobj["lang"]);
            tweet.FollowersCount = Convert.ToInt32(user["followers_count"]);
            tweet.FriendsCouunt = Convert.ToInt32(user["friends_count"]);
        }

        public static TwitterApplication getApplicationDetails(int? id)
        {
            if (id == null)
            {// oauth application keys
                return new TwitterApplication
                {
                    AccessToken = Util.getKeyValueFromAppSetting("oauth_token"),
                    AccessTokenSecret = Util.getKeyValueFromAppSetting("oauth_token_secret"),
                    ConsumerKey = Util.getKeyValueFromAppSetting("oauth_consumer_key"),
                    ConsumerSecret = Util.getKeyValueFromAppSetting("oauth_consumer_secret")

                };
            }
            else
            {
                return GetApplication(Convert.ToInt32(id));

            }


        }

        public static void ReadUserTimelineInTwitter(Search search, TwitterApplication app)
        {
            search.Max_Id = getMaxIdFromTweetSets(search.ScreenName);
            search.Count_toSearch = "100";
            // oauth application keys
            var oauth_token = app.AccessToken;
            var oauth_token_secret = app.AccessTokenSecret;
            var oauth_consumer_key = app.ConsumerKey;
            var oauth_consumer_secret = app.ConsumerSecret;

            // oauth implementation details
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow
                - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();


            // create oauth signature
            var baseFormat = "count={8}&max_id={7}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                            "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&screen_name={6}";//&count = {7}";

            var baseString = string.Format(baseFormat,
                                        oauth_consumer_key,
                                        oauth_nonce,
                                        oauth_signature_method,
                                        oauth_timestamp,
                                        oauth_token,
                                        oauth_version,
                                        //Uri.EscapeDataString(search.Count_toSearch),
                                        Uri.EscapeDataString(search.ScreenName),
                                          Uri.EscapeDataString(search.Max_Id), Uri.EscapeDataString(search.Count_toSearch)
                                        );


            baseString = string.Concat("GET&", Uri.EscapeDataString(search.TimeLineURL), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                    "&", Uri.EscapeDataString(oauth_token_secret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(oauth_nonce),
                                    Uri.EscapeDataString(oauth_signature_method),
                                    Uri.EscapeDataString(oauth_timestamp),
                                    Uri.EscapeDataString(oauth_consumer_key),
                                    Uri.EscapeDataString(oauth_token),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(oauth_version)
                            );



            ServicePointManager.Expect100Continue = false;

            // make the request
            var postBody = "screen_name=" + Uri.EscapeDataString(search.ScreenName) + "&max_id=" + Uri.EscapeDataString(search.Max_Id) + "&count=" + Uri.EscapeDataString(search.Count_toSearch);
            search.TimeLineURL += "?" + postBody;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(search.TimeLineURL);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var objText = reader.ReadToEnd();


            try
            {
                JArray jsonDat = JArray.Parse(objText);
                foreach (var status in jsonDat)
                {
                    ScrapyWeb.Models.TweetSet tweet = new ScrapyWeb.Models.TweetSet();

                    getTweetFromJObj(status, ref tweet);
                    AddTweetTODb(tweet);


                }
                //myDiv.InnerHtml = html;
            }
            catch (Exception twit_error)
            {
                //myDiv.InnerHtml = html + twit_error.ToString();
            }
        }

        static WebRequest CreateOauthAndRequest(TwitterApplication app, Search search)
        {
            //oauth application keys
            var oauth_token = app.AccessToken;//Util.getKeyValueFromAppSetting("oauth_token");
            var oauth_token_secret = app.AccessTokenSecret;// Util.getKeyValueFromAppSetting("oauth_token_secret");
            var oauth_consumer_key = app.ConsumerKey;// Util.getKeyValueFromAppSetting("oauth_consumer_key");
            var oauth_consumer_secret = app.ConsumerSecret;// Util.getKeyValueFromAppSetting("oauth_consumer_secret");

            //oauth implementation details
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(
                new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow
                - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            // var geocode = "33.6436653,-6.8618025,15mi";
            var geocode = new StringBuilder().Append(search.Latitude).Append(",").Append(search.Longitude).Append(",").Append(search.Radius).Append(search.IsRadiusInMiles ? "mi" : "km").ToString();


            // create oauth signature
            var baseFormat = "";
            if (search.SearchUserTimeLine)
            {
                baseFormat = "screen_name={6}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                            "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";//&since_id=" + search.Since_Id;// +"&count=" + search.Count_toSearch;//&rpp=" + tweetCount + "&include_entities=true" + "&page=" + page +"&until=
                geocode = search.ScreenName;
            }
            else
            {
                if (!string.IsNullOrEmpty(search.Since_Id))
                {
                    baseFormat = "geocode={6}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                                "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&result_type=mixed&since_id=" + search.Since_Id;// +"&count=" + search.Count_toSearch;//&rpp=" + tweetCount + "&include_entities=true" + "&page=" + page +"&until=
                }
                else baseFormat = "geocode={6}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                                "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&result_type=mixed";//&count=" + search.Count_toSearch;//&rpp=" + tweetCount + "&include_entities=true" + "&page=" + page +"&until=
            }

            var baseString = string.Format(baseFormat,
                                        oauth_consumer_key,
                                        oauth_nonce,
                                        oauth_signature_method,
                                        oauth_timestamp,
                                        oauth_token,
                                        oauth_version,
                                        Uri.EscapeDataString(geocode)
                                        );

            baseString = string.Concat("GET&", Uri.EscapeDataString(search.SearchUserTimeLine ? search.TimeLineURL : search.URL), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                                    "&", Uri.EscapeDataString(oauth_token_secret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(oauth_nonce),
                                    Uri.EscapeDataString(oauth_signature_method),
                                    Uri.EscapeDataString(oauth_timestamp),
                                    Uri.EscapeDataString(oauth_consumer_key),
                                    Uri.EscapeDataString(oauth_token),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(oauth_version)
                            );


            ServicePointManager.Expect100Continue = false;
            var URL = "";
            if (search.SearchUserTimeLine)
            {
                URL = search.TimeLineURL + "?screen_name=" + search.ScreenName;
            }
            else
            {
                if (!string.IsNullOrEmpty(search.Since_Id))
                {
                    URL = search.URL + "?geocode=" + geocode + "&result_type=mixed&since_id=" + search.Since_Id;// +"&count=" + search.Count_toSearch;
                }
                else URL = search.URL + "?geocode=" + geocode + "&result_type=mixed";//&count=" + search.Count_toSearch;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            return request;

        }

        public static void AddAplication(TwitterApplication app, ref string error)
        {
            try
            {
                using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
                {
                    var result = context.TwitterApplications.SingleOrDefault(a => a.ApplicationId == app.ApplicationId);
                    if (result != null)
                    {
                        result.ApplicationName = app.ApplicationName;
                        result.ConsumerKey = app.ConsumerSecret;
                        result.ConsumerKey = app.ConsumerKey;
                        result.AccessTokenSecret = app.AccessTokenSecret;
                        result.AccessToken = app.AccessToken;
                        //context.TwitterApplications.Attach(app);
                        context.Entry(result).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        context.TwitterApplications.Add(app);
                    }
                    context.SaveChanges();
                    error = "";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
        }

        public static void AddFBAplication(FBApplication app, ref string error)
        {
            try
            {
                using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
                {
                    var result = context.FBApplications.SingleOrDefault(a => a.ApplicationId == app.ApplicationId);
                    if (result != null)
                    {
                        result.ApplicationName = app.ApplicationName;
                        result.FbAppId = app.FbAppId;
                        result.FbAppSecret = app.FbAppSecret;

                        context.Entry(result).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        context.FBApplications.Add(app);
                    }
                    context.SaveChanges();
                    error = "";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;

            }
        }

        public static void getDownloadedTweetSets(ref List<TweetSet> _tweetList)
        {
            using (var context = new ScrapyWebEntities())
            {
                _tweetList = context.TweetSets.ToList();
            }
        }

        public static void getDownloadedGroupFeeds(ref List<FacebookGroupFeed> _fbFeedList)
        {
            using (var context = new ScrapyWebEntities())
            {
                // feed_id = 946166772123762_1538159976257769 => group_id = 946166772123762 (first part)
                // since I can not use split inside a lambda expression, take the 15 first chars
                _fbFeedList = context.FacebookGroupFeeds
                    // .OrderBy(x => x.GroupPostId.Substring(0, 15))
                    .OrderByDescending(x => x.UpdatedTime)
                    .ToList();
            }
        }

        public static void getFBPostsFromDB(ref List<FacebookGroupFeed> posts, String influencerId)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {
                // feed_id = 946166772123762_1538159976257769 => group_id = 946166772123762 (first part)
                // since I can not use split inside a lambda expression, take the 15 first chars
                // MC290617 not sure if working everytime
                posts = context.FacebookGroupFeeds
                    .Where(x => x.GroupPostId.Substring(0, 15) == influencerId)
                    .OrderByDescending(x => x.UpdatedTime)
                    .ToList();
            }
        }

        public static void getDownloadedFeedComments(ref List<FBFeedComment> _fbCommentList)
        {
            using (var context = new ScrapyWebEntities())
            {
                _fbCommentList = context.FBFeedComments
                    .OrderByDescending(x => x.created_time)
                    .ToList();
            }
        }

        public static void getDownloadedFBGroups(ref List<FBGroup> _fbGroupList)
        {
            using (var context = new ScrapyWebEntities())
            {
                _fbGroupList = context.FBGroups
                    .ToList();
            }
        }

        public static string getSinceIdFromTweetSets()
        {
            using (var context = new ScrapyWebEntities())
            {
                var topTweet = (from tweet in context.TweetSets
                                orderby tweet.Tweet_Id descending
                                select tweet).Take(1);
                var firstTopTweet = topTweet.FirstOrDefault<TweetSet>();
                if (firstTopTweet == null)
                    return String.Empty;
                else
                    return firstTopTweet.Tweet_Id;
            }
        }

        public static string getMaxIdFromTweetSets(string Screen_name)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {
                var topTweet = (from tweet in context.TweetSets
                                orderby tweet.Tweet_Id ascending
                                where tweet.ScreenName == Screen_name
                                select tweet).Take(1);
                return topTweet.FirstOrDefault<TweetSet>().Tweet_Id;

            }

        }

        public static TwitterApplication GetApplication(int ApplicationId)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {

                var query = (from app in context.TwitterApplications
                             where app.ApplicationId == ApplicationId

                             select app).Take(1);
                return query.FirstOrDefault<TwitterApplication>();

            }
        }
        */

        /* MC112917 TMP COMMENT SO IT CAN COMPILE 
        public static FBApplication GetFBApplication(int ApplicationId)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {
                var query = (from app in context.FBApplications
                             where app.ApplicationId == ApplicationId

                             select app).Take(1);
                return query.FirstOrDefault<FBApplication>();
            }
        }

        public static FBApplication GetFbApplication(int ApplicationId)
        {
            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {

                var query = (from app in context.FBApplications
                             where app.ApplicationId == ApplicationId

                             select app).Take(1);
                return query.FirstOrDefault<FBApplication>();

            }
        }

        public static void getFBApplications(ref List<FBApplication> _appList)
        {

            using (var context = new ScrapyWeb.Models.ScrapyWebEntities())
            {

                _appList = context.FBApplications
                                      .ToList();


            }
        }

        public static String FacebookGetAccessToken(FBApplication app)
        {
            string vals = "";
            string url = string.Format(Util.getKeyValueFromAppSetting("FbTokenURL") + "?client_id={0}&client_secret={1}&grant_type=client_credentials", app.FbAppId, app.FbAppSecret);

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                vals = reader.ReadToEnd();
            }

            return vals;
        }

        public static void getFacebookGroupFeed(Search search, FBApplication app, ref string Error)
        {
            try
            {
                // parse json token : eg : {"access_token":"360921534307030|ykMyj0iA9WcteYKnC_fNdYe-PEk","token_type":"bearer"}
                JObject jObject = JObject.Parse(search.FbAccessToken);
                String access_token = (String)jObject["access_token"];
                String token_type = (String)jObject["token_type"];

                // first get page/group feed (ie: list of posts)
                string objText = "";
                string url = search.FbAccessGroupFeedURL + search.GroupId + "/feed" + "?key=" + app.FbAppId + "&access_token=" + access_token + "&token_type=" + token_type;

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    objText = reader.ReadToEnd();

                    JObject jObjects = JObject.Parse(objText);
                    JObject Objects = new JObject(jObjects);
                    JArray items = (JArray)Objects["data"];

                    foreach (var status in items)
                    {
                        if (status["message"] != null)
                        {
                            var feed = new FacebookGroupFeed();

                            var message = status["message"] != null ? Convert.ToString(status["message"]) : null;

                            // MC240517 quick hack differentiation between group and page
                            String updated_created_time;
                            if (Regex.IsMatch(search.GroupId, @"^\d"))
                            {
                                // starting with a digit ex : 142220009186235 then group id then we use updated_time
                                updated_created_time = Convert.ToString(status["updated_time"]);
                            }
                            else
                            {
                                // not starting with a digit ex : tanjazzofficiel then page then we use created_time
                                updated_created_time = Convert.ToString(status["created_time"]);
                            }
                            var date = DateTime.Parse(updated_created_time);

                            // get comments as well
                            var feedId = Convert.ToString(status["id"]);
                            getFacebookGroupFeedComment(search, access_token, feedId, app, ref Error);

                            // save FB feed to DB
                            feed.GroupPostId = feedId;
                            feed.PostText = message;
                            feed.UpdatedTime = date;
                            AddGroupFeedTODb(feed);
                        }
                    }

                    // save FB group infor to DB
                    if (items != null)
                    {
                        var laststatus = items.Last();
                        if (laststatus != null)
                        {
                            var group = new FBGroup();
                            group.FbGroupId = Convert.ToString(laststatus["id"]).Split(new char[] { '_' })[0];
                            group.GroupName = search.GroupId;
                            AddFbGroupTODb(group);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)wex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            Error = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }*/

        /*public static List<T_FB_POST> getFBInfluencerPostsFromFB(String fbInfluencerUrlName, String fbAppId, String fbAccessToken)
        {
            // parse json token : eg : {"access_token":"360921534307030|ykMyj0iA9WcteYKnC_fNdYe-PEk","token_type":"bearer"}
            JObject jObject = JObject.Parse(fbAccessToken);
            String access_token = (String)jObject["access_token"];
            String token_type = (String)jObject["token_type"];

            //
            var graphFBApi28Url = Util.getKeyValueFromAppSetting("FbGroupFeed");
            Util.getKeyValueFromAppSetting("FbTokenURL");

            //
            var posts = new List<T_FB_POST>();

            // first get page/group feed (ie: list of posts) with count of like and count of comments
            string objText = "";
            string url = graphFBApi28Url + fbInfluencerUrlName + "/feed"
                + "?limit=100"
                + "&fields=comments.limit(0).summary(true),likes.limit(0).summary(true),message,created_time"
                + "&key=" + fbAppId + "&access_token=" + access_token + "&token_type=" + token_type;

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                objText = reader.ReadToEnd();

                JObject jObjects = JObject.Parse(objText);
                JObject Objects = new JObject(jObjects);
                JArray items = (JArray)Objects["data"];

                //
                foreach (var status in items)
                {
                    var post = new T_FB_POST();

                    //
                    var message = status["message"] != null ? Convert.ToString(status["message"]) : String.Empty;

                    //
                    String updated_created_time = Convert.ToString(status["created_time"]);
                    var date = DateTime.Parse(updated_created_time);

                    //
                    var feedId = Convert.ToString(status["id"]);

                    //
                    int likes_count = Convert.ToInt32(status["likes"]["summary"]["total_count"]);
                    int comments_count = Convert.ToInt32(status["comments"]["summary"]["total_count"]);

                    //
                    post.id = feedId;
                    post.post_text = message;
                    post.date_publishing = date;
                    post.likes_count = likes_count;
                    post.comments_count = comments_count;

                    // influencer
                    post.fk_influencer = feedId.Split(new char[] { '_' })[0];

                    //
                    posts.Add(post);
                }
            }

            return posts;
        }

        public static void getFacebookGroupFeedComment(Search search, String access_token, String feedId, FBApplication app, ref string Error)
        {
            try
            {
                //
                string objText = "";
                string url = search.FbAccessGroupFeedURL + feedId + "/comments"
                    + "?key=" + app.FbAppId
                    + "&access_token=" + access_token;

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    objText = reader.ReadToEnd();

                    JObject jObjects = JObject.Parse(objText);
                    JObject Objects = new JObject(jObjects);
                    JArray items = (JArray)Objects["data"];

                    foreach (var status in items)
                    {
                        if (status["message"] != null)
                        {
                            var feedComment = new FBFeedComment();

                            var message = Convert.ToString(status["message"]);
                            String created_time = Convert.ToString(status["created_time"]);
                            var date = DateTime.Parse(created_time);

                            // save FB feed comment to DB
                            feedComment.Id = Convert.ToString(status["id"]);
                            feedComment.message = message;
                            feedComment.created_time = date;
                            AddFeedCommentToDb(feedComment);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)wex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            Error = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        public static T_FB_INFLUENCER getFBInfluencerInfoFromFB(String fbInfluencerUrlName, String pro_or_anti, FBApplication app, String fbAccessToken)
        {
            // parse json token : eg : {"access_token":"360921534307030|ykMyj0iA9WcteYKnC_fNdYe-PEk","token_type":"bearer"}
            JObject jObject = JObject.Parse(fbAccessToken);
            String access_token = (String)jObject["access_token"];
            String token_type = (String)jObject["token_type"];

            //
            var graphFBApi28Url = Util.getKeyValueFromAppSetting("FbGroupFeed");

            // create influencer and set url name and bias
            var fbInfluencer = new T_FB_INFLUENCER();
            fbInfluencer.url_name = fbInfluencerUrlName;
            fbInfluencer.pro_or_anti = pro_or_anti;

            // first get influencer name and id
            String id, name;
            getInfluencerFirstInfoFromFB(fbInfluencerUrlName, app.FbAppId, graphFBApi28Url, access_token, token_type, out id, out name);
            fbInfluencer.id = id;
            fbInfluencer.name = name;

            // second get influencer fan count
            int fan_count;
            getInfluencerSecondInfoFromFB(fbInfluencerUrlName, app.FbAppId, graphFBApi28Url, access_token, token_type, out fan_count);
            fbInfluencer.fan_count = fan_count;

            // date last update
            fbInfluencer.date_last_update = DateTime.Now;

            //
            return fbInfluencer;
        }

        public static FB_KEYWORD getFBKeywordInfoFromFBViaTwingly(String fbKeywordKeyword)
        {
            //
            String access_token = Util.getKeyValueFromAppSetting("TwinglyApiKey");

            //
            var twinglyApi15Url = Util.getKeyValueFromAppSetting("TwinglyApi15Url");

            // create object and set values whatever available
            var fbKeyword = new FB_KEYWORD();
            fbKeyword.keyword = fbKeywordKeyword;
            fbKeyword.date_oldest_retrieve = DateTime.Today;
            fbKeyword.date_latest_retrieve = DateTime.Today;
            fbKeyword.matched_posts_count = 0;
            fbKeyword.matched_comments_count = 0;
            fbKeyword.matched_total_count = 0;
            fbKeyword.social_stats_likes = 0;
            fbKeyword.social_stats_comments = 0;
            fbKeyword.social_stats_shares = 0;
            fbKeyword.matched_posts_count_ma = 0;
            fbKeyword.matched_comments_count_ma = 0;
            fbKeyword.matched_total_count_ma = 0;
            fbKeyword.social_stats_likes_ma = 0;
            fbKeyword.social_stats_comments_ma = 0;
            fbKeyword.social_stats_shares_ma = 0;

            // assign object rest of value from FB via twingly
            getKeywordInfoFromFBViaTwingly(fbKeyword, twinglyApi15Url, access_token);
            getKeywordInfoFromFBViaTwingly(fbKeyword, twinglyApi15Url, access_token, limitToMorocco: true);

            //
            return fbKeyword;
        }
        */

        #region FRONT YARD PERSIST
        /* MC112917 TMP COMMENT SO IT CAN COMPILE 
        public static void AddGroupFeedTODb(FacebookGroupFeed feed)
        {
            using (var context = new ScrapyWebEntities())
            {
                var result = context.FacebookGroupFeeds.SingleOrDefault(f => f.GroupPostId == feed.GroupPostId);
                if (result == null)
                {
                    context.FacebookGroupFeeds.Add(feed);
                    context.SaveChanges();
                }
            }
        }

        public static void AddFBPostsToDB(List<T_FB_POST> posts)
        {
            // posts = posts.Take(15).ToList();
            using (var context = new ScrapyWebEntities())
            {
                foreach (var post in posts)
                {
                    context.T_FB_POST.Add(post);
                }
                context.SaveChanges();
            }
        }

        public static void getFBPostsFromDB(ref List<T_FB_POST> posts)
        {
            using (var context = new ScrapyWebEntities())
            {
                posts = context.T_FB_POST.ToList();
            }
        }

        public static void AddFbGroupTODb(FBGroup fbGroup)
        {
            using (var context = new ScrapyWebEntities())
            {
                // int id
                int? max = context.FBGroups.Max(x => (int?)x.GroupId);
                fbGroup.GroupId = max ?? 0 + 1;

                //
                var result = context.FBGroups.SingleOrDefault(f => f.FbGroupId == fbGroup.FbGroupId);
                if (result == null)
                {
                    context.FBGroups.Add(fbGroup);
                    context.SaveChanges();
                }
            }
        }

        public static void AddFeedCommentToDb(FBFeedComment feedComment)
        {
            using (var context = new ScrapyWebEntities())
            {
                var result = context.FBFeedComments.SingleOrDefault(f => f.Id == feedComment.Id);
                if (result == null)
                {
                    context.FBFeedComments.Add(feedComment);
                    context.SaveChanges();
                }
            }
        }

        public static void AddFBInfluencerToDB(T_FB_INFLUENCER influencer)
        {
            using (var context = new ScrapyWebEntities())
            {
                context.T_FB_INFLUENCER.Add(influencer);
                context.SaveChanges();
            }
        }

        public static void addFBKeywordToSerialization(FB_KEYWORD fbKeyword, String path)
        {
            //
            List<FB_KEYWORD> fbKeywords = new List<FB_KEYWORD>();
            XmlSerializer serializer = new XmlSerializer(fbKeywords.GetType());

            // deserialize / serialize FB keywords : read / add / write back
            using (var reader = new System.IO.StreamReader(path))
            {
                fbKeywords = (List<FB_KEYWORD>)serializer.Deserialize(reader);
            }

            // add or update
            var infbKeyword = fbKeywords.Find(m => m.keyword == fbKeyword.keyword);
            if (infbKeyword != null)
            {
                // MC110717 for now we do not manage merging overlapping date keywords, later we will need to keep data for keayword day by day 
                // in order to be able to cumulate/update keyword refresh of numbers

                // for now replace (TODO later cumulate see above comment)
                infbKeyword.date_oldest_retrieve = fbKeyword.date_oldest_retrieve;
                infbKeyword.date_latest_retrieve = fbKeyword.date_latest_retrieve;
                infbKeyword.matched_posts_count = fbKeyword.matched_posts_count;
                infbKeyword.matched_comments_count = fbKeyword.matched_comments_count;
                infbKeyword.matched_total_count = fbKeyword.matched_total_count;
                infbKeyword.social_stats_likes = fbKeyword.social_stats_likes;
                infbKeyword.social_stats_comments = fbKeyword.social_stats_comments;
                infbKeyword.social_stats_shares = fbKeyword.social_stats_shares;
                infbKeyword.matched_posts_count_ma = fbKeyword.matched_posts_count_ma;
                infbKeyword.matched_comments_count_ma = fbKeyword.matched_comments_count_ma;
                infbKeyword.matched_total_count_ma = fbKeyword.matched_total_count_ma;
                infbKeyword.social_stats_likes_ma = fbKeyword.social_stats_likes_ma;
                infbKeyword.social_stats_comments_ma = fbKeyword.social_stats_comments_ma;
                infbKeyword.social_stats_shares_ma = fbKeyword.social_stats_shares_ma;
            }
            else
                fbKeywords.Add(fbKeyword);

            // save
            using (var writer = new System.IO.StreamWriter(path))
            {
                serializer.Serialize(writer, fbKeywords);
                writer.Flush();
            }
        }
        */
        #endregion

        #region BACK YARD BO
        /* MC112917 TMP COMMENT SO IT CAN COMPILE 
        private static void getInfluencerFirstInfoFromFB(String fbInfluencerUrlName, String fbAppId, String graphFBApi28Url, String access_token, String token_type, out String id, out String name)
        {
            string objText = "";
            string url = graphFBApi28Url + fbInfluencerUrlName + "?key=" + fbAppId + "&access_token=" + access_token + "&token_type=" + token_type;

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                objText = reader.ReadToEnd();
                JObject obj = JObject.Parse(objText);

                // set id, name
                id = Convert.ToString(obj["id"]);
                name = Convert.ToString(obj["name"]);
            }
        }

        private static void getInfluencerSecondInfoFromFB(String fbInfluencerUrlName, String fbAppId, String graphFBApi28Url, String access_token, String token_type, out int fan_count)
        {
            string objText = "";
            string url = graphFBApi28Url + fbInfluencerUrlName + "?fields=fan_count" + "&key=" + fbAppId + "&access_token=" + access_token + "&token_type=" + token_type;

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                objText = reader.ReadToEnd();
                JObject obj = JObject.Parse(objText);

                // set fan_count
                fan_count = Convert.ToInt32(obj["fan_count"]);
            }
        }

        public static void getFBInfluencersFromDB(ref List<T_FB_INFLUENCER> influencers)
        {
            using (var context = new ScrapyWebEntities())
            {
                influencers = context.T_FB_INFLUENCER.ToList();
            }
        }

        private static void getKeywordInfoFromFBViaTwingly(FB_KEYWORD fbKeyword, String twinglyApi15Url, String access_token, bool limitToMorocco = false)
        {
            String url = twinglyApi15Url + "chart" + "?apikey=" + access_token + "&one=\"" + fbKeyword.keyword + "\"";

            // limit to Morocco
            if (limitToMorocco)
                url += "&country=ma";

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                String objText = reader.ReadToEnd();
                JObject jObjects = JObject.Parse(objText);
                JObject Objects = new JObject(jObjects);
                JArray items = (JArray)Objects["data"];

                //
                foreach (var item in items)
                {
                    // update date of latest and oldest
                    DateTime time_iso8610 = Convert.ToDateTime(item["time_iso8610"]);
                    if (time_iso8610 < fbKeyword.date_oldest_retrieve) fbKeyword.date_oldest_retrieve = time_iso8610;
                    if (time_iso8610 > fbKeyword.date_latest_retrieve) fbKeyword.date_latest_retrieve = time_iso8610;

                    // cumulate
                    if (!limitToMorocco)
                    {
                        fbKeyword.matched_posts_count += Convert.ToInt32(item["matched_posts_count"]);
                        fbKeyword.matched_comments_count += Convert.ToInt32(item["matched_comments_count"]);
                        fbKeyword.matched_total_count += Convert.ToInt32(item["matched_total_count"]);
                        fbKeyword.social_stats_likes += Convert.ToInt32(item["social_stats"]["likes"]);
                        fbKeyword.social_stats_comments += Convert.ToInt32(item["social_stats"]["comments"]);
                        fbKeyword.social_stats_shares += Convert.ToInt32(item["social_stats"]["shares"]);
                    }
                    else
                    {
                        fbKeyword.matched_posts_count_ma += Convert.ToInt32(item["matched_posts_count"]);
                        fbKeyword.matched_comments_count_ma += Convert.ToInt32(item["matched_comments_count"]);
                        fbKeyword.matched_total_count_ma += Convert.ToInt32(item["matched_total_count"]);
                        fbKeyword.social_stats_likes_ma += Convert.ToInt32(item["social_stats"]["likes"]);
                        fbKeyword.social_stats_comments_ma += Convert.ToInt32(item["social_stats"]["comments"]);
                        fbKeyword.social_stats_shares_ma += Convert.ToInt32(item["social_stats"]["shares"]);
                    }
                }
            }
        }*/
        #endregion
    }
}
