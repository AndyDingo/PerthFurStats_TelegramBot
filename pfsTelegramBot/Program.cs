/* 
 * All contents copyright 2016 - 2017, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by   : Microsoft Visual Studio 2015.
 * User         : AndyDingoWolf
 * Last Updated : 03/03/2017 by AndyDingo
 * -- VERSION --
 * Version      : 1.0.0.119
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ehoh = System.IO;
using File = System.IO.File;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Net.Mail;

namespace nwTelegramBot
{
#pragma warning disable 4014 // Allow for bot.SendChatAction to not be awaited
    // ReSharper disable FunctionNeverReturns
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
    // ReSharper disable CatchAllClause
    class Program
    {
        #region -= VARIABLES =-
        //public static string s_logfile = Environment.CurrentDirectory + @"\pfsTelegramBot.log"; // error log
        public static string s_cfgfile = Environment.CurrentDirectory + @"\pfsTelegramBot.cfg"; // Main config
        public static string s_ucfgfile = Environment.CurrentDirectory + @"\pfsPermConfig.cfg"; // User perm config
        public static string s_offsetcfg = Environment.CurrentDirectory + @"\data\offset.xml"; // User config
        public static string s_commandcfg = Environment.CurrentDirectory + @"\data\commandlist.xml"; // User config
        #endregion

        /// <summary>
        /// This is the main method, if you will.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Title = "PerthFurs SFW Telegram Group Command Bot";

            try
            {
                DateTime dt = new DateTime(2016, 2, 2);
                dt = DateTime.Now;

                bool isAvailable = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

                // Do the title
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("-----------------------------------------------------------------");
                Console.WriteLine("----------- PerthFurs SFW Telegram Group Command Bot ------------");
                Console.WriteLine("-----------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;

                // Do the initial starting routine, populate our settings file if it doesn't exist.
                nwInitialStuff(dt);

                Console.WriteLine(); // blank line

                // Network available?
                if (isAvailable == true)
                    Run().Wait(-1);
                else
                {
                    Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: No valid Internet Connection detected.");
                    Environment.Exit(0);
                }
            }
            catch (TaskCanceledException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (NullReferenceException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (ApiRequestException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (XmlException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (AggregateException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (Exception ex)
            {
                nwErrorCatcher(ex);
            }
        }

        #region -= Initial routines =-

        /// <summary>
        /// Our initial configuration and update run.
        /// </summary>
        /// <param name="dt">a datetime object that we need to add correct timestamps.</param>
        private static void nwInitialStuff(DateTime dt)
        {
            try
            {
                string str_ups;

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Loading configuration...");

                // Work item 01. Create our XML document if it doesn't exist
                if (File.Exists(s_cfgfile) != true)
                    nwCreateSettings();

                // Populate the strings
                str_ups = nwGrabString("updatesite"); //update site

                Console.WriteLine(); // blank line

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Using configuration file: " + s_cfgfile);
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Logging to file: " + Environment.CurrentDirectory + @"\logs_tg\<this chatroom id>." + dt.ToString(nwGrabString("dateformat")) + ".log");
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Finished loading configuration...");

                Console.WriteLine(); // blank line

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Checking for update...");

                //nwDoUpdateCheck(dt, str_ups); // Do our update check.
            }
            catch (Exception ex)
            {
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: " + ex.Message);
            }
        }

        /// <summary>
        /// Create the settings file, if it doesn't exist. It should, but just to be on the safe side.
        /// </summary>
        private static void nwCreateSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("  ");
            using (XmlWriter writer = XmlWriter.Create(s_cfgfile, settings))
            {
                // Write XML data.
                writer.WriteStartElement("config");
                writer.WriteAttributeString("version", "0.7"); // See NW Framework docs for features each version has

                writer.WriteComment("PerthFurStats Bot configuration File");
                writer.WriteComment("This file is generated (mostly)automatically, please don't edit manually");

                writer.WriteElementString("logformat", "mircstats");
                writer.WriteElementString("dateformat", "yyyyMMdd");
                writer.WriteElementString("timeformat", "HH:mm");
                writer.WriteElementString("filename", "#perthfurs");
                writer.WriteElementString("basename", "perthfurs.log");
                writer.WriteElementString("dloadImages", "false");
                writer.WriteElementString("dloadMedia", "false");
                writer.WriteElementString("botresponds", "true");
                writer.WriteElementString("debugmode", "true");
                // End CONFIG element
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        /// <summary>
        /// Checks for updates to this program from a specified update site.
        /// </summary>
        /// <param name="site">Site URL must be a sting and must be a valid url of no more than 255 characters, must include the name of a file.</param>
        /// <param name="dt">A DateTime object.</param>
        /// <remarks>This subroutine is still in testing. Do not use in regular releases at the current time.</remarks>
        private static void nwDoUpdateCheck(DateTime dt, string site)
        {
            // Declare the old and new version number variables.
            int n_newver = 0, n_oldver = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(site);

            string remote = doc.SelectSingleNode("updchk/new_version").InnerText;
            Version v = new Version(remote);

            // Assign numbers to version number variables
            n_newver = v.Revision;
            n_oldver = cExtensions.nwGetFileVersionInfo.FilePrivatePart;

            // If the new version is greater than the old version.
            if (n_newver == n_oldver)
            {
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: You already have the latest update for this program... Your Version [" + n_oldver.ToString() + "] Version on web [" + n_newver.ToString() + "]");
            }
            else if (n_newver > n_oldver)
            {
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: There are updates available. Your Version [" + n_oldver.ToString() + "] Version on web [" + n_newver.ToString() + "]");

            }
            else if (n_newver < n_oldver)
            {
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: No updates to this program are available at this time... Please try again later...");
            }
            else
            {
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Find out how the hell we got to this point.");
            }
        }

        #endregion

        #region -= Settings file IO routines =-

        private static XmlElement GetChildByName(XmlElement parent, string childName, XmlDocument xmlDocument)
        {
            // Try to find it in the parent element.
            XmlElement childElement = parent.SelectSingleNode(childName) as XmlElement;
            if (null == childElement)
            {
                // The child element does not exists, so create it.
                childElement = xmlDocument.CreateElement(childName);
                parent.AppendChild(childElement);
            }
            return childElement;
        }


        /// <summary>
        /// Settings file grabber
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key.</returns>
        /// <remarks>Very BETA.</remarks>
        private static string nwGrabString(string key)
        {
            XmlDocument doc = new XmlDocument();
            string s;

            doc.Load(s_cfgfile);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                s = doc.SelectSingleNode("config/" + key).InnerText;
                return s;

            }
            else
            {

                //Console.WriteLine("Error!");
                return "Error";

            }
        }

        /// <summary>
        /// Settings file grabber
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key. On error, it will return -1.</returns>
        /// <remarks>Very BETA.</remarks>
        [Obsolete]
        private static int nwGrabInt(string key)
        {
            XmlDocument doc = new XmlDocument();
            int s = 0;

            doc.Load(s_cfgfile);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("config/" + key).InnerText);
                return s;

            }
            else { return -1; }
        }

        //private static int nwGrabGlobalUsageDB(string command)
        //{
        //    var output = 0;

        //    //Mew.SQLiteConnection conn = new Mew.SQLiteConnection(@"Data Source=" + s_botdb + ";Version=3;Compress=True;");
        //    //Mew.SQLiteCommand cmd = new Mew.SQLiteCommand(conn);
        //    //Mew.SQLiteDataAdapter da = new Mew.SQLiteDataAdapter("SELECT [count] FROM [tbl_cmduse_global] WHERE [command]==" + command + ";", conn);
        //    //DataSet ds = new DataSet("mew");
        //    //da.Fill(ds);
        //    //Console.WriteLine(ds.Tables[0].Rows[0].ToString());

        //    conn.Open();

        //    using (var cmd_contents = conn.CreateCommand())
        //    {
        //        cmd_contents.CommandText = "SELECT [count] FROM [tbl_cmduse_global] WHERE [command]==\"" + command + "\";";
        //        var r = cmd_contents.ExecuteReader();

        //        while (r.Read())
        //        {
        //            output = Convert.ToInt32(r["count"]);
        //        }
        //        return output;
        //    }
        //}

        //private static void nwSetGlobalUsageDB(string command, int value)
        //{
        //    var output = 0;

        //    Mew.SQLiteConnection conn = new Mew.SQLiteConnection(@"Data Source=" + s_botdb + ";Version=3;Compress=True;");
        //    //Mew.SQLiteCommand cmd = new Mew.SQLiteCommand(conn);
        //    //Mew.SQLiteDataAdapter da = new Mew.SQLiteDataAdapter("SELECT [count] FROM [tbl_cmduse_global] WHERE [command]==" + command + ";", conn);
        //    //DataSet ds = new DataSet("mew");
        //    //da.Fill(ds);
        //    //Console.WriteLine(ds.Tables[0].Rows[0].ToString());

        //    conn.Open();

        //    using (var cmd_contents = conn.CreateCommand())
        //    {
        //        cmd_contents.CommandText = "UPDATE [tbl_cmduse_global] SET [count]==\"" + value + "\" WHERE [command]==\"" + command + "\";";
        //        var r = cmd_contents.ExecuteReader();

        //        while (r.Read())
        //        {
        //            output = Convert.ToInt32(r["count"]);
        //            Console.WriteLine(output);
        //        }
        //    }
        //}


        /// <summary>
        /// Set a string within settings.
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <param name="value">The value to write.</param>
        /// <remarks>Under construction.</remarks>
        private static void nwSetString(string key, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_cfgfile);
            doc.SelectSingleNode("config/" + key).InnerText = value;
            doc.Save(s_cfgfile);
        }
        
        /// <summary>
        /// Grab the per-user maximum for a given command
        /// </summary>
        /// <param name="key">The command that we need to lookup</param>
        /// <returns>The per user maximum for the command.</returns>
        private static int nwGrabUserMax(string key)
        {
            XmlDocument doc = new XmlDocument();
            int s = 0;

            doc.Load(s_commandcfg);

            if (doc.SelectSingleNode("commands/cmd_max_peruser/" + key) != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("commands/cmd_max_peruser/" + key).InnerText);
                return s;

            }
            else { return -1; }
        }
        
        /// <summary>
        /// Grab the amount of times a user has used a given command.
        /// </summary>
        /// <param name="s_username">A username</param>
        /// <param name="key">The key</param>
        /// <returns>An int.</returns>
        private static int nwGrabUserUsage(string s_username, string key)
        {
            XmlDocument doc = new XmlDocument();
            int s = 0;

            doc.Load(s_commandcfg);

            XmlElement root = doc.DocumentElement;

            XmlElement username = GetChildByName(root, s_username, doc);

            XmlElement keystone = GetChildByName(username, key, doc);

            if (doc.SelectSingleNode("commands/" + s_username + "/" + key) != null)
            {
                
                s = Convert.ToInt32(doc.SelectSingleNode("commands/" + s_username + "/" + key).InnerText);
                return s;
            }
            else { return -1; }
        }

        /// <summary>
        /// Grab the amount of times a command can be used globally.
        /// </summary>
        /// <param name="key">The command to check.</param>
        /// <returns>An INT, representing the amount of times a command can be used.</returns>
        /// <remarks>Very BETA. Replaces nwGrabInt.</remarks>
        private static int nwGrabGlobalMax(string key)
        {
            XmlDocument doc = new XmlDocument();
            int s = 0;

            doc.Load(s_commandcfg);

            if (doc.SelectSingleNode("commands/cmd_max_global/" + key) != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("commands/cmd_max_global/" + key).InnerText);
                return s;

            }
            else { return -1; }
        }

        /// <summary>
        /// Grab the amount of times a command has been used globally.
        /// </summary>
        /// <param name="key">The command to check.</param>
        /// <returns>An INT, representing the amount of times a command has been used.</returns>
        /// <remarks>Very BETA. Replaces nwGrabInt.</remarks>
        private static int nwGrabGlobalUsage(string key)
        {
            XmlDocument doc = new XmlDocument();
            int s = 0;

            doc.Load(s_commandcfg);

            if (doc.SelectSingleNode("commands/cmd_usage/" + key) != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("commands/cmd_usage/" + key).InnerText);

                Console.WriteLine("This command has been used " + s + " times.");

                return s;

            }
            else { return -1; }
        }

        private static int nwGrabOffset()
        {
            XmlDocument doc = new XmlDocument();
            int s = 0;

            doc.Load(s_offsetcfg);

            if (doc.SelectSingleNode("config/offset") != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("config/offset").InnerText);
                return s;

            }
            else { return -1; }
        }


        /// <summary>
        /// Returns a string to be used as part of a date time format.
        /// </summary>
        /// <param name="nodate">If true, don't return a date, false otherwise.</param>
        /// <returns>Returns the format.</returns>
        private static string nwParseFormat(bool nodate)
        {
            string t;

            t = nwGrabString("timeformat");

            if (nodate == false)
                return "dd/MM/yyyy " + t;
            else
                return t;
        }

        #endregion

        /// <summary>
        /// This is what we use to grab the logs from the server and download them into a readable format.
        /// </summary>
        /// <returns>Doesn't actually return much other than a HTTP status code.</returns>
        static async Task Run()
        {
            var Bot = new TelegramBotClient("170729696:AAGYA8FPN4RkquTRrY-teqrn-J9YdnZX22k"); // Api key, please generate your own, don't use mine.

            var me = await Bot.GetMeAsync();

            Bot.PollingTimeout = TimeSpan.FromDays(1);
            Bot.UploadTimeout = TimeSpan.FromMinutes(5);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Hello my name is {0}, I'm a bot for Perthfurs SFW Telegram.", me.Username);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;


            int offset = 0; // status offset
            offset = nwGrabOffset();

            while (true)
            {
                Update[] updates;
                updates = await Bot.GetUpdatesAsync(offset); // get updates
                //updates = await Bot.GetUpdatesAsync(); // get updates

                // For each update in the list
                foreach (Update update in updates)
                {
                    
                    switch (update.Type)
                    {
                        case UpdateType.MessageUpdate:

                            Telegram.Bot.Types.Message message = update.Message;
                            long n_chanid = update.Message.Chat.Id;
                            DateTime m = update.Message.Date.ToLocalTime();

                            //remove unsightly characters from first names.
                            string s_mffn = update.Message.From.FirstName;
                            s_mffn = Regex.Replace(s_mffn, @"[^\u0000-\u007F]", string.Empty);

                            if (s_mffn.Contains(" ") == true)
                                s_mffn.Replace(" ", string.Empty);

                            // variable for username, if blank, use firstname.
                            string s_mfun = update.Message.From.Username;

                            if (s_mfun == " " || s_mfun == string.Empty)
                                s_mfun = s_mffn;



                            // Do stuff if we are a text message
                            switch (message.Type)
                            {

                                case MessageType.ContactMessage:

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared the contact information of " + update.Message.Contact.FirstName);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared the contact information of " + update.Message.Contact.FirstName);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared the contact information of " + update.Message.Contact.FirstName);
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + update.Message.Chat.Id.ToString() + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Contact information sharing of " + update.Message.Contact.FirstName);
                                        }
                                    }

                                        break;

                                case MessageType.DocumentMessage:

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared a document of type: " + update.Message.Document.MimeType);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared a document of type: " + update.Message.Document.MimeType);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared a document of type: " + update.Message.Document.MimeType);
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Document sharing of type " + update.Message.Document.MimeType);
                                        }
                                    }

                                    break;

                                case MessageType.TextMessage:
                                    
                                        string umt = update.Message.Text;

                                    //If we have set the bot to be able to respond to our basic commands
                                    if (nwGrabString("botresponds") == "true" && (umt.StartsWith("!") == true))
                                    {
                                        
                                        nwProcessCommands(Bot, update, me, m).Wait(-1);
                                        
                                    }

                                    //If we have set the bot to be able to respond to our basic commands
                                    if (nwGrabString("botresponds") == "true" && (umt.StartsWith("/") == true))
                                    {
                                        
                                        nwProcessSlashCommands(Bot, update, me, m).Wait(-1);
                                        
                                    }

                                    #region Animal Noises

                                    if (Regex.IsMatch(umt, @"\bmow\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\bmew\b", RegexOptions.IgnoreCase) == true || umt.Contains("mrew") == true || umt.Contains("mjau") == true || umt.Contains("maow") == true || umt.Contains("meow") == true)
                                    {

                                        //int n_cat = nwCountCatNoises();
                                        nwSetCatNoiseCount(update.Message.From.Username);

                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has mowed, increase mow count.");

                                    }

                                    if (Regex.IsMatch(umt, @"\baroo\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\bawoo\b", RegexOptions.IgnoreCase) == true || umt.Contains("aruu") == true || umt.Contains("awuu") == true || umt.Contains("bark") == true || umt.Contains("bork") == true)
                                    {

                                        //int n_dog = nwCountDogNoises();
                                        nwSetDogNoiseCount(update.Message.From.Username);

                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has barked, increase bark count.");

                                    }

                                    if (Regex.IsMatch(umt, @"\bskree\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\brawr\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\bsqueak\b", RegexOptions.IgnoreCase) == true)
                                    {

                                        //int n_dog = nwCountDogNoises();
                                        nwSetDragonNoiseCount(update.Message.From.Username);

                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has made dragon noises, increase dragon noise count.");

                                    }

                                    #endregion

                                    // If we have easter eggs enabled.
                                    if (nwGrabString("eastereggs") == "true")
                                    {
                                        if (Regex.IsMatch(umt, @"\b" + update.Message.From.Username + "\b", RegexOptions.IgnoreCase) == true || umt.Contains(update.Message.From.Username))
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "Narcissist Alert! :P", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bboobs\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\btits\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "(  .  Y  .  )", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bping\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "pong", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bmarco\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "polo", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bwat\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "65 Wat", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bharambe\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            string ret = null;

                                            ret = nwGenRandomPhrase();

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, ret, false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bAm I ever gonna see your face again\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "https://www.youtube.com/watch?v=i_py6WbMV1k", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bblake De Bruyn\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\bblake DeBruyn\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            string ret = null;

                                            ret = nwGenRandomPhrase2();

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, ret, false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bneko\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "Aww, so cute", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bkawaii desu\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\bkawaii\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "Sugoi!", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bdat boi\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "OH SHIT WHADDUP", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bhalal snack pack\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "Best thing ever invented.", false, false, update.Message.MessageId);

                                        }

                                        if (Regex.IsMatch(umt, @"\bleaving the fandom\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(umt, @"\bquitting the fandom\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "https://www.youtube.com/watch?v=HhI3zf0Pgf4", false, false, update.Message.MessageId);

                                        }

                                        // if message contains skynet
                                        if (Regex.IsMatch(update.Message.Text, @"\bskynet\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(update.Message.Text, @"\bcyberdyne\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendTextMessageAsync(update.Message.Chat.Id, "https://www.youtube.com/watch?v=XcNXq5DUZnk", false, false, update.Message.MessageId);

                                        }

                                        // if message contains eeyup
                                        if (Regex.IsMatch(update.Message.Text, @"\beeyup\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            string s_rande = null;

                                            s_rande = nwGenRandomSuffix();

                                            Bot.SendTextMessageAsync(update.Message.Chat.Id, "Eeyup" + s_rande, false, false, update.Message.MessageId);

                                        }

                                        // if message contains eeyup
                                        if (Regex.IsMatch(update.Message.Text, @"\btrigger\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendTextMessageAsync(update.Message.Chat.Id, "Go back to tumblr with your 'triggers' and 'microaggressions'.", false, false, update.Message.MessageId);

                                        }

                                        // If message contains OWO
                                        if (Regex.IsMatch(update.Message.Text, @"\bowo\b", RegexOptions.IgnoreCase) == true || Regex.IsMatch(update.Message.Text, @"\buwu\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, "What's this?", false, false, update.Message.MessageId);

                                        }

                                        // If message contains buttstripe
                                        if (Regex.IsMatch(update.Message.Text, @"\bbuttstripe\b", RegexOptions.IgnoreCase) == true)
                                        {

                                            Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(m) != false)
                                                Bot.SendTextMessageAsync(update.Message.Chat.Id, ">:|", false, false, update.Message.MessageId);

                                        }

                                    }

                                    if (nwGrabString("botresponds") == "false" && (umt.StartsWith("/") == true || umt.StartsWith("!") == true))
                                    {
                                        if (nwGrabString("debugMode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has attempted to use a command, but they were disabled.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has attempted to use a command, but they were disabled, along with debug mode.");
                                    }

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_mffn + "> " + update.Message.Text);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_mffn + "> " + update.Message.Text);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_mffn + "> " + update.Message.Text);
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + update.Message.Text);
                                        }
                                    }

                                    break;

                                case MessageType.UnknownMessage: // UNKNOWN MESSAGES.
                                    // TODO: Work out what to actually flocking do with them.

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * System: Unknown, please report");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Unknown, please report");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Unknown message");
                                        }
                                    }

                                    break;

                                case MessageType.ServiceMessage: // Service messages (user leaves or joins)

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* System: A user (" + s_mffn + ") has joined or left the group.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has joined or left the group!");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "System message");
                                        }
                                    }

                                    break;

                                case MessageType.LocationMessage: // Venue messages. Added in API v2.0
                                                               // TODO: IMPLEMENT PROPERLY

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " posted about a location.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " posted about a location.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted about a location.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Location message");
                                        }
                                    }

                                    break;

                                case MessageType.VenueMessage: // Venue messages. Added in API v2.0
                                // TODO: IMPLEMENT PROPERLY

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " posted about a venue on Foursquare.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " posted about a venue on Foursquare.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted about a venue on Foursquare.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Venue message");
                                        }
                                    }

                                    break;
                                
                                case MessageType.StickerMessage: // Do stuff if we are a sticker message

                                    if (update.Message.Sticker.Emoji == "😺")
                                    {
                                        nwSetCatNoiseCount(update.Message.From.Username);

                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has mowed, increase mow count.");
                                    }

                                    if (update.Message.Sticker.Emoji == "🐺")
                                    {
                                        nwSetDogNoiseCount(update.Message.From.Username);

                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has barked, increase bark count.");
                                    }

                                    if (update.Message.Sticker.Emoji == "🐲")
                                    {
                                        nwSetDragonNoiseCount(update.Message.From.Username);

                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has done a dragon call, increase squeak count.");
                                    }


                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        // download the emoji for the image, if there is one. Added in May API update.
                                        string s = update.Message.Sticker.Emoji;

                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a sticker that represents the " + s + " emoticon.");
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a sticker that represents the " + s + " emoticon.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a sticker that represents the " + s + " emoticon.");

                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            // download the emoji for the image, if there is one. Added in May API update.
                                            string s = update.Message.Sticker.Emoji;

                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Sticker message (" + s + ")");
                                        }
                                    }

                                    break;
                                
                                case MessageType.VoiceMessage: // Do stuff if we are a voice message

                                    m = update.Message.Date.ToLocalTime();

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a voice message.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a voice message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a voice message.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Voice message");
                                        }
                                    }

                                    break;

                                case MessageType.VideoMessage:

                                    m = update.Message.Date.ToLocalTime();

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a video message.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a video message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a video message.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Video message");
                                        }
                                    }

                                    break;
                                    
                                case MessageType.PhotoMessage: // Do stuff if we are a photo message

                                    m = update.Message.Date.ToLocalTime(); // Get date/time

                                    //write following to file stream.
                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        // download the caption for the image, if there is one.
                                        string s = update.Message.Caption;

                                        // check to see if the caption string is empty or not
                                        if (s == string.Empty || s == null || s == "" || s == "/n")
                                        {
                                            if (nwGrabString("debugmode") == "true")
                                                Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a photo with no caption.");
                                            else
                                                nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a photo with no caption.");
                                            await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a photo with no caption.");
                                        }
                                        else
                                        {
                                            if (nwGrabString("debugmode") == "true")
                                                Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a photo with the caption '" + s + "'.");
                                            else
                                                nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a photo with the caption '" + s + "'.");
                                            await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a photo with the caption '" + s + "'.");
                                        }
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Photo message");
                                        }
                                    }

                                    break;
                                    
                                case MessageType.AudioMessage: // Do stuff if we are an audio message

                                    m = update.Message.Date.ToLocalTime();

                                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + n_chanid + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted an audio message.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted an audio message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted an audio message.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + n_chanid + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + s_mffn + "," + "Audio message");
                                        }
                                    }

                                    break;

                                default:

                                    nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Find out how we got to this point.");
                                    break;
                                    
                            }

                            offset = update.Id + 1; // do not touch.
                            nwSetOffset(offset.ToString());

                            break;
                        case UpdateType.InlineQueryUpdate:

                            DateTime m2 = update.Message.Date.ToLocalTime();

                            nwPrintSystemMessage("[" + m2.ToString(nwParseFormat(true)) + "] * System: User tried an Inline query.");

                            break;
                        default:
                            //nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Find out how we got to this point.");
                            break;
                    }
                    
                }

            }

        }

        /// <summary>
        /// Process all of our slash commands
        /// </summary>
        /// <param name="bot">The bot API.</param>
        /// <param name="update">The update</param>
        /// <param name="me">The user, or bot.</param>
        /// <param name="dt">The date/time component.</param>
        /// <remarks>Only designed to work if regular commands are enabled.</remarks>
        private static async Task nwProcessSlashCommands(TelegramBotClient bot, Update update, Telegram.Bot.Types.User me, DateTime dt)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void nwSetDragonNoiseCount(string username)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_miscstats WHERE (username = @user)", conn);
                chkUser.Parameters.AddWithValue("@user", username);
                MySqlDataReader reader = chkUser.ExecuteReader();

                if (reader.HasRows)
                {
                    //User Exists
                    int mews = nwCountDragonNoises(username);

                    mews++;

                    nwUpdateDragonNoises(mews, username);
                }
                else
                {
                    //User NOT Exists
                    nwInsertDragonNoises(username);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwInsertDragonNoises(string username)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "INSERT INTO tbl_miscstats (username, totalsqueaks) VALUES ('" + username + "', '" + 1 + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwUpdateDragonNoises(int mews, string username)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "UPDATE tbl_miscstats SET totalsqueaks='" + mews + "' WHERE username='" + username + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static int nwCountDragonNoises(string username)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sq = "SELECT totalsqueaks FROM tbl_miscstats WHERE username='" + username + "'";
                MySqlCommand msc = new MySqlCommand(sq, conn);
                int mews = Convert.ToInt32(msc.ExecuteScalar());

                return mews;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static string nwGenRandomPhrase2()
        {
            int i = cDiceBag.Instance.d4(1);
            switch (i)
            {
                case 1:
                    return "The man!";
                case 2:
                    return "The myth!";
                case 3:
                    return "The legend!";
                case 4:
                    return "The Amazing!";
                default:
                    return @" \o/ ";
            }
        }

        private static string nwGenRandomPhrase()
        {
            int i = cDiceBag.Instance.d4(1);
            switch (i)
            {
                case 1:
                    return "I miss him :C";
                case 2:
                    return "<insert body part> out for Harambe!";
                case 3:
                    return "http://knowyourmeme.com/memes/harambe-the-gorilla";
                case 4:
                    return "Justice for Harambe!";
                default:
                    return @" \o/ ";
            }
        }

        private static string nwGenRandomSuffix()
        {
            int i = cDiceBag.Instance.d4(1);
            switch (i)
            {
                case 1:
                    return ".";
                case 2:
                    return "!";
                case 3:
                    return "?";
                case 4:
                    return "!?!";
                default:
                    return @" \o/ ";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastuser"></param>
        private static void nwSetDogNoiseCount(string lastuser)
        {

            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_miscstats WHERE (username = @user)", conn);
                chkUser.Parameters.AddWithValue("@user", lastuser);
                MySqlDataReader reader = chkUser.ExecuteReader();

                if (reader.HasRows)
                {
                    //User Exists
                    int mews = nwCountDogNoises(lastuser);

                    mews++;

                    nwUpdateDogNoises(mews, lastuser);
                }
                else
                {
                    //User NOT Exists
                    nwInsertDogNoises(lastuser);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastuser"></param>
        private static void nwSetCatNoiseCount(string lastuser)
        {

            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_miscstats WHERE (username = @user)", conn);
                chkUser.Parameters.AddWithValue("@user", lastuser);
                MySqlDataReader reader = chkUser.ExecuteReader();

                if (reader.HasRows)
                {
                    //User Exists
                    int mews = nwCountCatNoises(lastuser);

                    mews++;

                    nwUpdateCatNoises(mews, lastuser);
                }
                else
                {
                    //User NOT Exists
                    nwInsertCatNoises(lastuser);
                    
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");

        }

        /// <summary>
        /// Insert cat noises into database.
        /// </summary>
        private static void nwInsertCatNoises(string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "INSERT INTO tbl_miscstats (username, totalmows) VALUES ('" + lastuser + "', '" + 1 + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Insert cat noises into database.
        /// </summary>
        private static void nwUpdateCatNoises(int mews, string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "UPDATE tbl_miscstats SET totalmows='" + mews + "' WHERE username='" + lastuser + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static int nwCountCatNoises(string lastuser)
        {

            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sq = "SELECT totalwoofs FROM tbl_miscstats WHERE username='" + lastuser + "'";
                MySqlCommand msc = new MySqlCommand(sq, conn);
                int mews = Convert.ToInt32(msc.ExecuteScalar());

                return mews;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Insert dog noises into database.
        /// </summary>
        private static void nwInsertDogNoises(string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "INSERT INTO tbl_miscstats (username, totalwoofs) VALUES ('" + lastuser + "', '" + 1 + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Insert dog noises into database.
        /// </summary>
        private static void nwUpdateDogNoises(int mews, string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "UPDATE tbl_miscstats SET totalwoofs='" + mews + "' WHERE username='" + lastuser + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static int nwCountDogNoises(string lastuser)
        {

            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sq = "SELECT totalwoofs FROM tbl_miscstats WHERE username='" + lastuser + "'";
                MySqlCommand msc = new MySqlCommand(sq, conn);
                int mews = Convert.ToInt32(msc.ExecuteScalar());

                return mews;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        #region -=COMMAND TOTALS=-

        /// <summary>Count total commands used.</summary>
        /// <param name="lastuser">The username to alter data for.</param>
        private static int nwCountTotalCommands(string lastuser)
        {

            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();

                string sq = "SELECT totalcommands FROM tbl_miscstats WHERE username='" + lastuser + "'";
                MySqlCommand msc = new MySqlCommand(sq, conn);
                int mews = Convert.ToInt32(msc.ExecuteScalar());

                return mews;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }

            conn.Close();
        }

        /// <summary>Insert default value in database if none present.</summary>
        /// <param name="lastuser">The username to alter data for.</param>
        private static void nwInsertTotalCommands(string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();

                string sql = "INSERT INTO tbl_miscstats (username, totalcommands) VALUES ('" + lastuser + "', '" + 1 + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
        }

        /// <summary>Update total commands used with a given value.</summary>
        /// <param name="value">Value to add.</param>
        /// <param name="lastuser">The username to alter data for.</param>
        private static void nwUpdateTotalCommands(int value, string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();

                string sql = "UPDATE tbl_miscstats SET totalcommands='" + value + "' WHERE username='" + lastuser + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
        }


        /// <summary>
        /// Set total command usage.
        /// </summary>
        /// <param name="lastuser">The username to alter data for.</param>
        private static void nwSetTotalCommandUsage(string lastuser)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();

                MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_miscstats WHERE (username = @user)", conn);
                chkUser.Parameters.AddWithValue("@user", lastuser);
                MySqlDataReader reader = chkUser.ExecuteReader();

                if (reader.HasRows)
                {
                    //User Exists
                    int mews = nwCountTotalCommands(lastuser);

                    mews++;

                    nwUpdateTotalCommands(mews, lastuser);
                }
                else
                {
                    //User NOT Exists
                    nwInsertTotalCommands(lastuser);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
        }

        #endregion

        /// <summary>
        /// Set the chat message offset.
        /// </summary>
        /// <param name="value">the new offset.</param>
        private static void nwSetOffset(string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_offsetcfg);
            doc.SelectSingleNode("config/offset").InnerText = value;
            doc.Save(s_offsetcfg);
        }

        /// <summary>
        /// Process all of our slash commands
        /// </summary>
        /// <param name="bot">The bot API.</param>
        /// <param name="update">The update</param>
        /// <param name="me">The user, or bot.</param>
        /// <param name="dt">The date/time component.</param>
        /// <remarks>Only designed to work if regular commands are enabled.</remarks>
        private static async Task nwProcessCommands(TelegramBotClient bot, Update update, Telegram.Bot.Types.User me, DateTime dt)
        {
            // read configuration and extract api keys
            var wundergroundKey = nwGrabString("weatherapi");
            var nsfwKey = nwGrabString("e621api");
            string exchangeKey = "";

            // Apparently Telegram sends usernames with extra characters on either side, that the bot hates
            // This should remove them.
            string s_username;
            if (update.Message.From.Username != null)
            {
                s_username = update.Message.From.Username.Trim(' ').Trim('\r').Trim('\n');
            }
            else
            {
                s_username = update.Message.From.FirstName; // Use firstname if username is null
            }

            // This one is to get the group type.
            ChatType ct = update.Message.Chat.Type;

            // Process request
            try
            {
                var httpClient = new ProHttpClient();
                var text = update.Message.Text;
                var replyText = string.Empty;
                var replyText2 = string.Empty;
                var s_replyToUser = string.Empty;
                var replyTextEvent = string.Empty;
                var replyTextMarkdown = string.Empty;
                var replyImage = string.Empty;
                var replyVideo = string.Empty;
                var replyImageCaption = string.Empty;
                var replyDocument = string.Empty;
                var thischat = bot.GetChatAsync(update.Message.Chat.Id);

                if (text != null && (text.StartsWith("!", StringComparison.Ordinal)))
                {
                    // Log to console
                    Console.WriteLine(update.Message.Chat.Id + " < " + update.Message.From.Username + " - " + text);

                    text = "!" + text.Substring(1);

                    // Strip @BotName
                    text = text.Replace("@" + me.Username, "");

                    // Parse
                    string command;
                    string body;

                    if (text.StartsWith("/s/", StringComparison.Ordinal))
                    {

                        command = "/s"; // special case for sed
                        body = text.Substring(2);

                    }
                    else
                    {

                        command = text.Split(' ')[0];
                        body = text.Replace(command, "").Trim();

                    }

                    var stringBuilder = new StringBuilder();

                    //

                    switch (command.ToLowerInvariant())
                    {
                        case "!ball":
                        case "!8ball":

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                if (body == string.Empty || body == " " || body == "@" || body.Contains("?") == false || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "You haven't given me a question to answer." + Environment.NewLine + "Usage: !8ball question to ask?";

                                    break;
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !8ball [question to ask, followed by a question mark]" + Environment.NewLine + "Type '!8ball help' to see this message again.";

                                    break;

                                }

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = nwRandom8BallResponse();
                                break;
                            }

                            break;

                        case "!mods":
                        case "!admin":
                        case "!admins":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                StringBuilder sb = new StringBuilder();

                                ChatMember[] mew = await bot.GetChatAdministratorsAsync(update.Message.Chat.Id);

                                sb.AppendLine("The group admins, to whom all must obey, are:");

                                foreach (ChatMember x in mew)
                                {
                                    sb.AppendLine("@" + x.User.Username);
                                }

                                replyText = sb.ToString();
                            }
                            else
                            {
                                Console.WriteLine(" The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!alive":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Hi " + update.Message.From.FirstName + ", I am indeed alive.";
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!backup":

                            // check to see if private message
                            if (ct == ChatType.Private)
                            {
                                bool b_kat = false;

                                // check the username
                                if (s_username != "AnwenSnowMew")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        s_replyToUser = "You have insufficient permissions to access this command.";
                                    break;
                                }
                                // if it is okay to reply, do so.
                                if (nwCheckInReplyTimer(dt) != false)
                                {
                                    s_replyToUser = "Starting backup...";
                                    cZipBackup.Instance.CreateSample(dt.ToString(nwGrabString("dateformat")) + "_backup.zip", null, Environment.CurrentDirectory + @"\logs_tg\");
                                    b_kat = true;
                                }

                                if (b_kat == true)
                                {
                                    s_replyToUser = "Backup complete";
                                }
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "This command can only be used in private messages.";
                                break;
                            }

                            break;

                        case "!cat":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                            //int n_catuse = nwGrabGlobalUsageDB("cat");
                            //int n_cat_uuse = nwGrabUserUsage(s_username, "cat");
                            //int n_cat_gmax = nwGrabGlobalMax("cat");
                            //int n_cat_umax = nwGrabUserMax("cat");

                            //if (n_catuse == n_cat_gmax || n_catuse == n_cat_umax)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        s_replyToUser = "Sorry, the /cat command has been used too many times.";
                            //    break;
                            //}

                            // if it is okay to reply, do so.
                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                replyImage = "http://thecatapi.com/api/images/get?format=src&type=jpg,png";
                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            //nwSetGlobalUsageDB("cat", n_catuse++); // set global usage incrementally
                                                                   //nwSetUserUsage(s_username, "cat", n_cat_uuse++); // set this users usage incrementally

                            break;

                        case "!die":
                        case "!kill":

                            if (ct == ChatType.Private)
                            {
                                if (s_username != "AnwenSnowMew")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        s_replyToUser = "You have insufficient permissions to access this command.";
                                    break;
                                }

                                s_replyToUser = "Goodbye.";

                                Task.Delay(1000);

                                Environment.Exit(0);
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "This command can only be used in private messages.";
                                break;
                            }

                            break;

                        case "!e621":

                            if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
                            {

                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                    body = body.Replace(' ', '+');
                                    body = body.Replace("@", string.Empty);

                                    string s_img_grabbed = nwGrabImage("https://e621.net/post/index.xml?limit=220");

                                    await bot.SendTextMessageAsync(update.Message.Chat.Id, nwGrabImage("https://e621.net/post/index.xml?limit=220&tags=order:score"), false, false, 0, null, ParseMode.Html);

                                    break;

                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !e621 [tag to look for, leave blank for random]" + Environment.NewLine +
                                        "Tips:" + Environment.NewLine +
                                        "If you wish to prevent undesireable artwork from appearing you should use some more complex tags." + Environment.NewLine +
                                        "This bot relies on e621 API and if it's not working, and/or servers are down, then the bot can't do anything about it." + Environment.NewLine +
                                        "Flash content is completely ignored by this, due to the Telegram api." + Environment.NewLine +
                                        "Type '!e621 help' to see this message again.";

                                    break;

                                }
                                else
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                    body = body.Replace(' ', '+');
                                    body = body.Replace("@", string.Empty);

                                    string s_img_grabbed = nwGrabImage("https://e621.net/post/index.xml?limit=220&tags=" + body);

                                    await bot.SendTextMessageAsync(update.Message.Chat.Id, s_img_grabbed, false, false, 0, null, ParseMode.Html);

                                    break;

                                }

                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Not happening, outside of private messages, or NSFW channels that is.";
                            }

                            break;

                        case "!count":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (ct != ChatType.Private)
                            {
                                ChatMember[] cm_admin = await bot.GetChatAdministratorsAsync(update.Message.Chat.Id);

                                foreach (ChatMember x in cm_admin)
                                {

                                    if (x.User.Username.Contains(s_username) != true)
                                    {

                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "You have insufficient permissions to access this command.";
                                        break;

                                    }
                                    else
                                    {

                                        if (nwCheckInReplyTimer(dt) != false)
                                        {

                                            int meow;
                                            meow = await bot.GetChatMembersCountAsync(update.Message.Chat.Id);
                                            s_replyToUser = "There are currently " + meow + " people in chat.";

                                        }
                                        else
                                        {

                                            Console.WriteLine(" The " + command + " failed as it took too long to process.");

                                        }

                                        break;
                                    }

                                }

                            }

                            break;

                        case "!con":
                        case "!cons":
                        case "!convention": // TODO: Finish this command
                        case "!conventions": // TODO: Finish this command

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                XmlDocument dook = new XmlDocument();
                                dook.Load(ehoh.Directory.GetCurrentDirectory() + @"/data/conventions.xml");
                                DateTime dta1 = new DateTime(2016, 4, 1);
                                dta1 = DateTime.Now;

                                DateTime dta2 = new DateTime(2016, 4, 1);
                                dta2 = DateTime.Now;

                                // Get our nodes
                                XmlNodeList nodes;
                                nodes = dook.GetElementsByTagName("event");

                                // Create a new string builder
                                StringBuilder eventString = new StringBuilder();
                                eventString.AppendLine("Here is a list of upcoming (public) Australian conventions. Times are in local time, and may be subject to change.");

                                // Iterate through available events
                                for (var i1for = 0; i1for < nodes.Count; i1for++)
                                {
                                    dta1 = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("start").InnerText);
                                    dta2 = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("end").InnerText);
                                    eventString.AppendLine("<b>"+nodes.Item(i1for).SelectSingleNode("title").InnerText + "</b> [" + nodes.Item(i1for).SelectSingleNode("url").InnerText + "]");
                                    eventString.AppendLine("Convention starts: " + dta1.ToString("ddd d/MM/yyy") + " (" + dta1.ToString("h:mm tt") + ")");
                                    eventString.AppendLine("Convention ends: " + dta2.ToString("ddd d/MM/yyy") + " (" + dta2.ToString("h:mm tt") + ")");
                                    eventString.AppendLine("");
                                }

                                replyTextEvent = eventString.ToString();

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!meet":
                        case "!meets":
                        case "!event":
                        case "!events": // TODO: Finish this command

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    if (nwReturnEventInfo(dt) != "maow")
                                        replyTextEvent = nwReturnEventInfo(dt);
                                    else
                                        replyText = "I can't let you do that Dave.";
                                    break;
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !event [number]" + Environment.NewLine + "The optional parameter number can be omitted, in which it just returns events for last 15 days by default." + Environment.NewLine + "Type '!event help' to see this message again.";

                                    break;

                                }
                                else
                                {
                                    if (nwReturnEventInfo(dt) != "maow")
                                        replyTextEvent = nwReturnEventInfo(dt, Convert.ToInt32(body));
                                    else
                                        replyText = "I can't let you do that Dave.";
                                }

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!debug":

                            if (ct == ChatType.Private)
                            {
                                if (s_username != "AnwenSnowMew" || s_username != "Inflatophin")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        replyText = "You have insufficient permissions to access this command.";
                                    break;
                                }

                                if (nwGrabString("debugMode") == "false")
                                {
                                    nwSetString("debugMode", "true");
                                    nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: DEBUG MODE ENABLED!");
                                }
                                else
                                {
                                    nwSetString("debugMode", "false");
                                    nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: DEBUG MODE DISABLED!");
                                }
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "This command can only be used in private messages.";
                                break;
                            }

                            break;

                        case "!dingo":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dingo";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dingo" + Environment.NewLine + "Type '!dingo help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("dingo", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!canine":
                        case "!dog":
                        case "!doggo":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dog";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dog" + Environment.NewLine + "Type '!dog help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("dog", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!corgi":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !corgi";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !corgi" + Environment.NewLine + "Type '!corgi help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("corgi", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!dino":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dino";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dino" + Environment.NewLine + "Type '!dino help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("dinosaur", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!ferret":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !ferret";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !ferret" + Environment.NewLine + "Type '!ferret help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("ferret", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!fennec":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !fennec";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !fennec" + Environment.NewLine + "Type '!fennec help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("fennec fox", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!deer":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !deer";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !deer" + Environment.NewLine + "Type '!dragon help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("deer", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!dragon":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dragon";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !dragon" + Environment.NewLine + "Type '!dragon help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("dragon", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!edit":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (s_username != "AnwenSnowMew")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "You have insufficient permissions to access this command.";
                                else
                                {
                                    Console.WriteLine("The " + command + " failed as it took too long to process.");
                                }

                                break;
                            }

                            // Roll our dice.
                            nwEditBotMessage(dt, bot, update.Message.Chat.Id, body);

                            break;

                        case "!echo":

                            if (ct == ChatType.Private)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                {
                                    if (body == string.Empty || body == " " || body == "@" || body == null)
                                    {
                                        bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                        s_replyToUser = "Too short.";

                                        break;
                                    }

                                    replyText = body;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("The " + command + " failed as it took too long to process.");
                                }
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "This command can only be used in private messages.";

                                break;

                            }

                            break;

                        case "!ermine":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !ermine";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !ermine" + Environment.NewLine + "Type '!ermine help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("ermine", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!fox":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !fox";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !fox" + Environment.NewLine + "Type '!fox help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("fox", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!hare":
                        case "!rabbit":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !rabbit";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !rabbit" + Environment.NewLine + "Type '!rabbit help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("rabbit", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!gif":

                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !gif [image to look for]";

                                    break;
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !gif [Image to look for]" + Environment.NewLine + "Type '!gif help' to see this message again.";

                                    break;

                                }

                                if (nwCheckInReplyTimer(dt) != false)
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                    retryme:

                                    // list of urls.
                                    string html = null;

                                    // Checks to see if the channel we are posting to has nsfw, or 18+ in title.
                                    if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
                                        html = GetHtmlCode(body, false, true);
                                    else
                                        html = GetHtmlCode(body, false, false);

                                    List<string> urls = GetUrls(html);
                                    var rnd = new Random();

                                    int randomUrl = rnd.Next(0, urls.Count - 1);

                                    string luckyUrl = urls[randomUrl];

                                    // Check if the file is valid, or throws an unwanted status code.
                                    if (!string.IsNullOrEmpty(luckyUrl))
                                    {
                                        UriBuilder uriBuilder = new UriBuilder(luckyUrl);
                                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                        if (response.StatusCode == HttpStatusCode.NotFound)
                                        {
                                            Console.WriteLine("Broken - 404 Not Found, attempting to retry.");
                                            goto retryme;
                                        }
                                    if (response.StatusCode == HttpStatusCode.BadRequest)
                                    {
                                        Console.WriteLine("Broken - 400 Bad Request, attempting to retry.");
                                        goto retryme;
                                    }
                                    if (response.StatusCode == HttpStatusCode.OK)
                                        {
                                            Console.WriteLine("URL appears to be good.");
                                        }
                                        else //There are a lot of other status codes you could check for...
                                        {
                                            Console.WriteLine(string.Format("URL might be ok. Status: {0}.",
                                                                       response.StatusCode.ToString()));
                                        }
                                    }

                                    if (luckyUrl.Contains(" ") == true)
                                        luckyUrl.Replace(" ", "%20");

                                    replyImage = luckyUrl;

                                    //nwSetGlobalUsageDB("img", n_imguse++); // set global usage incrementally
                                    //nwSetUserUsage(s_username, "img", n_img_uuse++); // set this users usage incrementally

                                    break;

                                }
                                else
                                {
                                    Console.WriteLine("The " + command + " failed as it took too long to process.");
                                }

                            break;

                        case "!jelly": // TODO: Finish this command

                            // usage /jelly [yes|no] leave blank and bot will repeat query.
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";

                            break;

                        case "!bio": // Get a persons bio.

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = nwShowBio(update.Message.From.Username);

                                break;

                            }
                            else if (body == "help")
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !bio" + Environment.NewLine + "Type '!bio help' to see this message again.";

                                break;

                            }
                            else
                            {
                                s_replyToUser = "Usage: !bio [username]" + Environment.NewLine + "Type '!bio help' to see this message again.";
                            }

                            break;

                        case "!getbio":

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = nwShowBio(body);

                                break;

                            }
                            else if (body == "help")
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !getbio [username]" + Environment.NewLine + "Type '!getbio help' to see this message again.";

                                break;

                            }
                            else
                            {
                                s_replyToUser = "Usage: !getbio [username]" + Environment.NewLine + "Type '!getbio help' to see this message again.";
                            }

                            break;

                        case "!setbio": // SET YOUR BIO

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "You haven't given me a message to put in your bio.";

                                break;

                            }
                            else
                            {
                                
                                nwSetBio(update.Message.From.Username, body);
                            }


                            if (body == "help")
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !setbio [text to send]" + Environment.NewLine + "Type '!setbio help' to see this message again.";

                                break;

                            }

                            break;

                        case "!genstats":

                            nwGenerateStatsPage(update.Message.Chat.Id);

                            break;

                        case "!img":
                        case "!image": // TODO: Finish this command

                            //int n_imguse = nwGrabGlobalUsageDB("img"); // GLOBAL USAGE
                            //int n_img_uuse = nwGrabUserUsage(s_username, "img");
                            //int n_img_gmax = nwGrabGlobalMax("img"); // GLOBAL MAXIMUM
                            //int n_img_umax = nwGrabUserMax("img"); // USER MAXIMUM

                            //if (n_imguse == n_img_gmax || n_img_uuse == n_img_umax)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        s_replyToUser = "Sorry, the /image command has been used too many times.";

                            //    break;
                            //}

                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !image [image to look for]";

                                    break;
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !image [Image to look for]" + Environment.NewLine + "Type '!image help' to see this message again.";

                                    break;

                                }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                retryme:

                                // list of urls.
                                string html = null;

                                // Checks to see if the channel we are posting to has nsfw, or 18+ in title.
                                if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
                                    html = GetHtmlCode(body, false, true);
                                else
                                    html = GetHtmlCode(body, false, false);

                                List<string> urls = GetUrls(html);
                                var rnd = new Random();

                                int randomUrl = rnd.Next(0, urls.Count - 1);

                                // Select url from url list.
                                string luckyUrl = urls[randomUrl];

                                // Check if the file is valid, or throws an unwanted status code.
                                if (!string.IsNullOrEmpty(luckyUrl))
                                {
                                    UriBuilder uriBuilder = new UriBuilder(luckyUrl);
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                                    if (response.StatusCode == HttpStatusCode.BadRequest)
                                    {
                                        Console.WriteLine("Broken - 400 Bad Request, attempting to retry.");
                                        goto retryme;
                                    }
                                    if (response.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        Console.WriteLine("Broken - 403 Forbidden, attempting to retry.");
                                        goto retryme;
                                    }
                                    if (response.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        Console.WriteLine("Broken - 404 Not Found, attempting to retry.");
                                        goto retryme;
                                    }
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        Console.WriteLine("URL appears to be good.");
                                    }
                                    else //There are a lot of other status codes you could check for...
                                    {
                                        Console.WriteLine(string.Format("URL might be ok. Status: {0}.",
                                                                   response.StatusCode.ToString()));
                                    }

                                }

                                if (luckyUrl.Contains(" ") == true)
                                    luckyUrl.Replace(" ", "%20");

                                replyImage = luckyUrl;

                                //nwSetGlobalUsageDB("img", n_imguse++); // set global usage incrementally
                                                                       //nwSetUserUsage(s_username, "img", n_img_uuse++); // set this users usage incrementally

                                break;

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!help":
                        case "!command":
                        case "!commands":
                        case "!list":

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    StringBuilder clist = new StringBuilder();
                                    clist.AppendLine("Here is a partial list of commands the bot understands:");
                                    clist.AppendLine("You are currently viewing Page <b>[1]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                    clist.AppendLine("<b>!admins</b> - show who the group admins are.");
                                    clist.AppendLine("<b>!alive</b> - Check if the bot is live, please use in PM with the bot.");
                                    clist.AppendLine("<b>!backup</b> - Backup bot log files, please use in PM with the bot. <i>Admin only</i>.");
                                    clist.AppendLine("<b>!ball</b> - consult the magic 8 ball, use a ? at the end of your question.");
                                    clist.AppendLine("<b>!bio</b> - show your bio.");
                                    clist.AppendLine("<b>!cat</b> - show a cat image.");
                                    clist.AppendLine("<b>!con</b> - show a list of australian furry conventions");
                                    clist.AppendLine("<b>!count</b> - count number of people in chat. <i>To be revised</i>.");
                                    clist.AppendLine("<b>!debug</b> - enable the bots debug mode. Can only be used in PM. <i>Admin only</i>.");
                                    clist.AppendLine("<b>!e621</b> [topics] - search for stuff on e621, can only be used in PM, or in a group with NSFW or 18+ in the title.");
                                    replyTextEvent = clist.ToString();

                                    break;
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !list [page number]" + Environment.NewLine + "Type '!list help' to see this message again.";

                                    break;

                                }

                                if (body == "1")
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    StringBuilder clist = new StringBuilder();
                                    clist.AppendLine("Here is a partial list of regular commands the bot understands:");
                                    clist.AppendLine("You are currently viewing Page <b>[1]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                    clist.AppendLine("<b>!admins</b> - show who the group admins are.");
                                    clist.AppendLine("<b>!alive</b> - Check if the bot is live, please use in PM with the bot.");
                                    clist.AppendLine("<b>!backup/b> - Backup bot log files, please use in PM with the bot. <i>Admin only</i>.");
                                    clist.AppendLine("<b>!ball</b><a title=\"Additional commands: !8ball\" href=\"#\">*</a> - consult the magic 8 ball, use a ? at the end of your question.");
                                    clist.AppendLine("<b>!bio</b> - show your bio.");
                                    clist.AppendLine("<b>!cat</b> - show a cat image.");
                                    clist.AppendLine("<b>!con</b> - show a list of australian furry conventions");
                                    clist.AppendLine("<b>!count</b> - count number of people in chat. <i>To be revised</i>.");
                                    clist.AppendLine("<b>!debug</b> - enable the bots debug mode. Can only be used in PM. <i>Admin only</i>.");
                                    clist.AppendLine("<b>!e621</b> [topics] - search for stuff on e621, can only be used in PM, or in a group with NSFW or 18+ in the title.");
                                    replyTextEvent = clist.ToString();

                                    break;

                                }

                                if (body == "2")
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    StringBuilder clist = new StringBuilder();
                                    clist.AppendLine("Here is a partial list of regular commands the bot understands:");
                                    clist.AppendLine("You are currently viewing Page <b>[2]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                    clist.AppendLine("<b>!edit</b> [message id] [replacement text] - edit a message posted by the bot. <i>Admin only</i>. <i>To be revised</i>.");
                                    clist.AppendLine("<b>!event</b><a title=\"Additional commands: !events, !meet, !meets\" href=\"#\">*</a> [time constraint in days, optional] - get events list.");
                                    clist.AppendLine("<b>!forecast</b> - get a 7 day weather forecast.");
                                    clist.AppendLine("<b>!getbio</b> [username] - get anothers bio via their username. <i>To be revised</i>.");
                                    clist.AppendLine("<b>!getstats</b> - generate stats. <i>Admin only</i>. <i>To be revised</i>.");
                                    clist.AppendLine("<b>!gif</b> [topic] - show a GIF based on a given topic. <i>To be revised</i>.");
                                    clist.AppendLine("<b>!image</b><a title=\"Additional commands: !img\" href=\"#\">*</a> [topic] - show an image based on a given topic.");
                                    clist.AppendLine("<b>!joke</b><a title=\"Additional commands: !humour\" href=\"#\">*</a> - get the bot to tell a joke.");
                                    clist.AppendLine("<b>!kill</b><a title=\"Additional commands: !die\" href=\"#\">*</a> - kill the bot. Can only be used in PM. <i>Admin only</i>.");
                                    clist.AppendLine("<b>!link</b> - generate a chat link.");
                                    clist.AppendLine("<b>!list</b><a title=\"Additional commands: !command, !commands, !help\" href=\"#\">*</a> - shows this list.");
                                    replyTextEvent = clist.ToString();

                                    break;

                                }

                                if (body == "3")
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    StringBuilder clist = new StringBuilder();
                                    clist.AppendLine("Here is a partial list of regular commands the bot understands:");
                                    clist.AppendLine("You are currently viewing Page <b>[3]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                    clist.AppendLine("<b>!meme</b> [topic] - show an image based on a given topic.");
                                    clist.AppendLine("<b>!oo</b><a title=\"Additional commands: !optout\" href=\"#\">*</a> - opt out of stats collection.");
                                    clist.AppendLine("<b>!quote</b> - get the bot to quote something from chat.");
                                    clist.AppendLine("<b>!roll</b> [dice] [sides] - roll a dice, with the given number of dice and sides.");
                                    clist.AppendLine("<b>!rules</b> - show group rules.");
                                    clist.AppendLine("<b>!setbio</b> - set your bio.");
                                    clist.AppendLine("<b>!stats</b> [week|month|year|alltime|commands] - generate a link to view stats.");
                                    clist.AppendLine("<b>!weather</b> - Get current weather conditions");
                                    replyTextEvent = clist.ToString();

                                    break;

                                }

                                if (body == "4")
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    StringBuilder clist = new StringBuilder();
                                    clist.AppendLine("Here is a partial list of species commands the bot understands:");
                                    clist.AppendLine("You are currently viewing Page <b>[4]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                    clist.AppendLine("<b>!clep</b> - show a clouded leopard pic.");
                                    clist.AppendLine("<b>!corgi</b> - show a corgi pic.");
                                    clist.AppendLine("<b>!deer</b> - show a corgi pic.");
                                    clist.AppendLine("<b>!dingo</b> - show a dingo pic.");
                                    clist.AppendLine("<b>!dino</b> - show a dino pic.");
                                    clist.AppendLine("<b>!dog</b><a title=\"Additional commands: !canine, !doggo\" href=\"#\">*</a> - show a dog pic.");
                                    clist.AppendLine("<b>!dragon</b> - show a dragon pic.");
                                    clist.AppendLine("<b>!ermine</b> - show a dragon pic.");
                                    clist.AppendLine("<b>!fennec</b> - show a fennec fox pic.");
                                    clist.AppendLine("<b>!ferret</b> - show a ferret pic.");
                                    clist.AppendLine("<b>!fox</b> - show a fox pic.");
                                    replyTextEvent = clist.ToString();

                                    break;

                                }

                                if (body == "5")
                                {

                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    StringBuilder clist = new StringBuilder();
                                    clist.AppendLine("Here is a partial list of species commands the bot understands:");
                                    clist.AppendLine("You are currently viewing Page <b>[5]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                    clist.AppendLine("<b>!gshep</b> - show a german shephard pic.");
                                    clist.AppendLine("<b>!leopard</b> - show a leopard pic.");
                                    clist.AppendLine("<b>!rabbit</b> - show a rabbit pic.");
                                    clist.AppendLine("<b>!rat</b> - show a rat pic.");
                                    clist.AppendLine("<b>!shibe</b> - show a shibe pic.");
                                    clist.AppendLine("<b>!snep</b> - show a snow leopard pic.");
                                    replyTextEvent = clist.ToString();

                                    break;

                                }

                                //StringBuilder clist = new StringBuilder();
                                //clist.AppendLine("Here is a partial list of commands the bot understands:");
                                //clist.AppendLine("<b>!admins</b> - show who the group admins are.");
                                //clist.AppendLine("<b>!alive</b> - Check if the bot is live, please use in PM with the bot.");
                                //clist.AppendLine("<b>!backup/b> - Backup bot log files, please use in PM with the bot. <i>Admin only</i>.");
                                //clist.AppendLine("<b>!ball</b> - consult the magic 8 ball, use a ? at the end of your question.");
                                //clist.AppendLine("<b>!bio</b> - show your bio.");
                                //clist.AppendLine("<b>!cat</b> - show a cat image.");
                                //clist.AppendLine("<b>!con</b> - show a list of australian furry conventions");
                                //clist.AppendLine("<b>!count</b> - count number of people in chat. <i>To be revised</i>.");
                                //clist.AppendLine("<b>!debug</b> - enable the bots debug mode. Can only be used in PM. <i>Admin only</i>.");
                                //clist.AppendLine("<b>!e621</b> [topics] - search for stuff on e621, can only be used in PM, or in a group with NSFW or 18+ in the title.");
                                //clist.AppendLine("<b>!edit</b> [message id] [replacement text] - edit a message posted by the bot. <i>Admin only</i>. <i>To be revised</i>.");
                                //clist.AppendLine("<b>!event</b> [time constraint in days, optional] - get events list.");
                                //clist.AppendLine("<b>!forecast</b> - get a 7 day weather forecast.");
                                //clist.AppendLine("<b>!getbio</b> [username] - get anothers bio via their username. <i>To be revised</i>.");
                                //clist.AppendLine("<b>!getstats</b> - generate stats. <i>Admin only</i>. <i>To be revised</i>.");
                                //clist.AppendLine("<b>!gif</b> [topic] - show a GIF based on a given topic. <i>To be revised</i>.");
                                //clist.AppendLine("<b>!image</b> [topic] - show an image based on a given topic.");
                                //clist.AppendLine("<b>!joke</b> - get the bot to tell a joke.");
                                //clist.AppendLine("<b>!kill</b> - kill the bot. Can only be used in PM. <i>Admin only</i>.");
                                //clist.AppendLine("<b>!link</b> - generate a chat link.");
                                //clist.AppendLine("<b>!list</b> - shows this list.");
                                //clist.AppendLine("<b>!meme</b> [topic] - show an image based on a given topic.");
                                //clist.AppendLine("<b>!oo</b> - opt out of stats collection.");
                                //clist.AppendLine("<b>!quote</b> - get the bot to quote something from chat.");
                                //clist.AppendLine("<b>!roll</b> [dice] [sides] - roll a dice, with the given number of dice and sides.");
                                //clist.AppendLine("<b>!rules</b> - show group rules.");
                                //clist.AppendLine("<b>!setbio</b> - set your bio.");
                                //clist.AppendLine("<b>!stats</b> [week|month|year|alltime|commands] - generate a link to view stats.");
                                //clist.AppendLine("<b>!weather</b> - Get current weather conditions");
                                ////clist.AppendLine("<b>!profile</b> - get perthfurs.net profile for yourself.");
                                ////clist.AppendLine("<b>!getprofile</b> - get perthfurs.net profile for someone else.");
                                //clist.AppendLine("");
                                //clist.AppendLine("Commands for species: ");
                                //clist.AppendLine("<b>!corgi</b> - show a corgi pic.");
                                //clist.AppendLine("<b>!dingo</b> - show a dingo pic.");
                                //clist.AppendLine("<b>!dino</b> - show a dino pic.");
                                //clist.AppendLine("<b>!dog</b> - show a dog pic.");
                                //clist.AppendLine("<b>!dragon</b> - show a dragon pic.");
                                //clist.AppendLine("<b>!fennec</b> - show a fennec fox pic.");
                                //clist.AppendLine("<b>!ferret</b> - show a ferret pic.");
                                //clist.AppendLine("<b>!fox</b> - show a fox pic.");
                                //clist.AppendLine("<b>!gshep</b> - show a german shephard pic.");
                                //clist.AppendLine("<b>!shibe</b> - show a shibe pic.");
                                //clist.AppendLine("<b>!snep</b> - show a snow leopard pic.");
                                ////clist.AppendLine("<b>!deer</b> - show a deer pic.");
                                ////clist.AppendLine("<b>!rabbit</b> - show a deer pic.");
                                ////clist.AppendLine("<b>!ermine</b> - show a deer pic.");
                                ////clist.AppendLine("<b>!bat</b> - show a ferret pic.");
                                ////clist.AppendLine("<b>!bear</b> - show a ferret pic.");



                                //replyTextEvent = clist.ToString();

                            }

                            break;

                        case "!humour":
                        case "!joke": // TODO: Fix this command

                            //int n_jokeuse = nwGrabGlobalUsageDB("joke");
                            //int n_joke_uuse = nwGrabUserUsage(s_username, "joke");
                            int n_joke_gmax = nwGrabGlobalMax("joke");
                            //int n_joke_umax = nwGrabUserMax("joke");

                            //if (n_jokeuse == n_joke_gmax)//|| n_joke_uuse == n_joke_umax)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        s_replyToUser = "Sorry, the /joke command has been used too many times.";
                            //    break;
                            //}

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                string textomatic = nwRandomJokeLine();

                                string[] s_mysplit = new string[] { "", "", "" };
                                string[] s_mysep = new string[] { "\\r", "\\n" };
                                s_mysplit = textomatic.Split(s_mysep, StringSplitOptions.RemoveEmptyEntries);

                                StringBuilder jokesb = new StringBuilder();

                                foreach (string s_meow in s_mysplit)
                                {
                                    jokesb.AppendLine(s_meow);
                                }

                                replyText = jokesb.ToString();
                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            //nwSetGlobalUsageDB("joke", n_jokeuse++); // set global usage incrementally
                            //nwSetUserUsage(s_username, "joke", n_joke_uuse++); // set this users usage incrementally

                            break;

                        case "!link":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Chat link: https://telegram.me/joinchat/ByYWcD2FFG72gZK04d6lLg";

                            break;

                        case "!oo":
                        case "!optout":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                //s_replyToUser = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following form to opt-out from stats collection. Bare in mind that your request might not be implemented till the next stats run, as it requires manual intervention. URL: http://www.perthfurstats.net/node/10";

                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !oo [reason for the]";

                                    break;
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !oo [reason, leave blank if you want]" + Environment.NewLine + "Type '!oo help' to see this message again.";

                                    break;

                                }


                                // write request to database
                                nwWriteORToDB(update.Message.From.Username, body);

                                // write and send email
                                nwSendEmail(update.Message.From.Username, body);

                                // final



                            }

                            break;

                        case "!profile":

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";

                            break;

                        case "!getprofile":

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";

                            break;

                        case "!quote":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                string fchosen = null;
                                string[] fileEntries = null;

                                if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
                                    fileEntries = ehoh.Directory.GetFiles(Environment.CurrentDirectory + @"\logs_tg", @"-1001052518605.*.log");
                                else
                                    fileEntries = ehoh.Directory.GetFiles(Environment.CurrentDirectory + @"\logs_tg", @"-1001032131694.*.log");

                                fchosen = fileEntries[new Random().Next(0, fileEntries.Length)];

                                if (fchosen == null) { replyText = "An error has occurred, please try this command again, and/or quote 'fchosen returned null' to @AndyDingoFolf"; }

                                string textomatic1 = "";

                                blah:

                                // if blank, get quote line without a body
                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    textomatic1 = nwRandomQuoteLine(fchosen);
                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !quote [name of person to quote, or leave blank for random]" + Environment.NewLine + "Type '!quote help' to see this message again.";

                                    break;

                                }
                                else
                                {
                                    textomatic1 = nwRandomQuoteLine(fchosen, body);
                                }

                                // If our code doesn't contain our test string
                                if (textomatic1.Contains("Test message. Do not report."))
                                {
                                    textomatic1 = nwRandomQuoteLine(fchosen);
                                    goto blah;
                                }

                                string sayrandom = nwRandomSaidLine();

                                string[] s_mysplit1 = new string[] { "", "", "" };
                                string[] s_mysplit2 = new string[] { "", "", "" };
                                int[] n_mysplit = new int[] { 2016, 9, 1, 0, 0 };
                                string[] s_mysep1 = new string[] { "\\r", "\\n", "[", "] <", "> " };
                                string[] s_splitfile = new string[] { "." };

                                s_mysplit1 = textomatic1.Split(s_mysep1, StringSplitOptions.RemoveEmptyEntries);
                                s_mysplit2 = fchosen.Split(s_splitfile, StringSplitOptions.RemoveEmptyEntries);

                                n_mysplit[0] = Convert.ToInt32(s_mysplit2[1].Substring(0, 4)); // YEAR
                                n_mysplit[1] = Convert.ToInt32(s_mysplit2[1].Substring(4, 2)); // MONTH
                                n_mysplit[2] = Convert.ToInt32(s_mysplit2[1].Substring(6)); // DAY
                                n_mysplit[3] = Convert.ToInt32(s_mysplit1[0].Substring(0, 2)); // HOUR
                                n_mysplit[4] = Convert.ToInt32(s_mysplit1[0].Substring(3, 2)); // MINUTE

                                DateTime mdt = new DateTime(n_mysplit[0], n_mysplit[1], n_mysplit[2], n_mysplit[3], n_mysplit[4], 0);

                                replyText = string.Format("On {0}, at {1}, the user \"{2}\" {3} the following: " + Environment.NewLine + "{4}", mdt.ToString("ddd d/MM/yyy"), mdt.ToShortTimeString(), s_mysplit1[1], sayrandom, s_mysplit1[2]);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!roll":
                        case "!diceroll":

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                string s = "";

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                // Roll our dice.
                                s = nwRollDice(s_username, dt, body);
                                replyText = s;

                            }
                            else
                            {

                                Console.WriteLine("The " + command + " failed as it took too long to process.");

                            }

                            break;

                        case "!rules":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Group rules: " + Environment.NewLine + "- All content (chat, images, stickers) must be SFW at all hours of the day." + Environment.NewLine + "- No flooding or spamming of ANY kind." + Environment.NewLine + "- Be nice to each other.";

                            break;

                        case "!slap": // TODO: Finish this command

                            // usage /slap [nickname] bot will slap the person matching the nickname.
                            // will return "yournickname slaps targetnickname around with [randomobject]

                            //int n_emouse = nwGrabGlobalUsageDB("emote");
                            //int n_emo_uuse = nwGrabUserUsage(s_username, "emote");
                            //int n_emo_gmax = nwGrabGlobalMax("emote");
                            //int n_emo_umax = nwGrabUserMax("emote");

                            //if (n_emouse == n_emo_gmax || n_emo_uuse == n_emo_umax)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        s_replyToUser = "Sorry, the /slap command has been used too many times.";

                            //    break;

                            //}

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    replyText = "*@PFStats_bot slaps @" + s_username + " around a bit with a large trout!*";

                                    //nwSetGlobalUsageDB("emote", n_emouse++); // set global usage incrementally
                                    //nwSetUserUsage(s_username, "emote", n_emouse++); // set this users usage incrementally

                                    break;

                                }
                                else if (body == "help")
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Usage: !slap [name of person to slap, or leave blank for random]" + Environment.NewLine + "Type '!slap help' to see this message again.";

                                    break;

                                }

                                // Sanitise target string.
                                string s_target = body;

                                //break on empty strings
                                if (s_target == string.Empty || s_target == " " || s_target == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);
                                    s_replyToUser = "No target was selected. Usage: !slap @username";
                                    break;
                                }

                                if (s_username == string.Empty)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);
                                    s_replyToUser = "I'm sorry, I can't let you do that Dave";

                                    //nwSetGlobalUsageDB("emote", n_emouse++); // Write new value. // set global usage incrementally
                                    //nwSetUserUsage(s_username, "emote", n_emouse++); // set this users usage incrementally

                                    break;
                                }

                                if (s_target != string.Empty)
                                {
                                    replyText = "*@" + s_username + " slaps @" + s_target + " around a bit with a large sea trout!*";
                                }
                                else
                                {
                                    replyText = "*@PFStats_bot slaps @" + s_username + " around a bit with a large sea trout!*";
                                }
                            }

                            //nwSetGlobalUsageDB("emote", n_emouse++); // Write new value. // set global usage incrementally
                            //nwSetUserUsage(s_username, "emote", n_emo_uuse++); // set this users usage incrementally

                            break;

                        case "!sfw":
                        case "!safeforwork":

                            //int n_sfwuse = nwGrabGlobalUsageDB("sfw");
                            int n_sfwmax1 = nwGrabGlobalMax("sfw");
                            int n_sfwmax2 = nwGrabUserMax("sfw");

                            //if (n_sfwuse == n_sfwmax1 || n_sfwuse == n_sfwmax2)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        s_replyToUser = "Sorry, the /sfw command has been used too many times.";
                            //    break;
                            //}

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadVideo);
                            string s_fname = ehoh.Directory.GetCurrentDirectory() + @"\data\sfw.mp4";

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                // Create an instance of StreamReader to read from a file.
                                // The using statement also closes the StreamReader.
                                using (ehoh.StreamReader sr = new ehoh.StreamReader(s_fname))
                                {
                                    FileToSend fts = new FileToSend();
                                    fts.Content = sr.BaseStream;
                                    fts.Filename = ehoh.Directory.GetCurrentDirectory() + @"\data\sfw.mp4";
                                    // Send to the channel
                                    await bot.SendVideoAsync(update.Message.Chat.Id, fts, 0, "", false, update.Message.MessageId);
                                }
                                break;
                            }

                            //nwSetGlobalUsageDB("sfw", n_sfwuse++); // Write new value. // set global usage incrementally
                            //nwSetUserUsage(s_username, "sfw", n_sfwuse++); // set this users usage incrementally

                            break;

                        case "!set":
                        case "!settings":

                            // TODO: This would ideally need to be one of any of the config file settings
                            // Example of Usage: !set -[option to set] -[new value]

                            if (ct != ChatType.Private)
                            {

                                ChatMember[] cm_admin2 = await bot.GetChatAdministratorsAsync(update.Message.Chat.Id);

                                foreach (ChatMember x in cm_admin2)
                                {

                                    if (x.User.Username.Contains(s_username) != true)
                                    {

                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "You have insufficient permissions to access this command.";

                                        break;

                                    }
                                    else
                                    {

                                        string[] s_splitfile = new string[] { "-" };
                                        string[] sp_body = body.Split(s_splitfile, StringSplitOptions.RemoveEmptyEntries);

                                        switch (sp_body[0])
                                        {
                                            case "egg":

                                                if (sp_body[1] != null && sp_body[1] == "disable")
                                                    nwSetString("eastereggs", "false");
                                                if (sp_body[1] != null && sp_body[1] == "enable")
                                                    nwSetString("eastereggs", "true");

                                                if (nwCheckInReplyTimer(dt) != false)
                                                    replyText = "The setting " + sp_body[0] + " was changed.";

                                                break;
                                            default:
                                                break;
                                        }

                                        //if (nwCheckInReplyTimer(dt) != false)
                                        //    replyText = "This command is not yet implemented.";

                                        break;

                                    }

                                }

                            }
                            else
                            {

                                string[] stuff1 = nwGrabAdminsFromList(Environment.CurrentDirectory + @"\data\adminlist.txt");
                                
                                foreach (string x in stuff1)
                                {

                                    if (x.Contains(s_username) != true)
                                    {

                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "You have insufficient permissions to access this command.";
                                        break;

                                    }
                                    else
                                    {
                                        
                                        string[] s_splitfile = new string[] { "-" };
                                        string[] sp_body = body.Split(s_splitfile, StringSplitOptions.RemoveEmptyEntries);

                                        switch (sp_body[0])
                                        {
                                            case "egg":

                                                if (sp_body[1] != null && sp_body[1] == "disable")
                                                    nwSetString("eastereggs", "false");
                                                if (sp_body[1] != null && sp_body[1] == "enable")
                                                    nwSetString("eastereggs", "true");

                                                if (nwCheckInReplyTimer(dt) != false)
                                                    replyText = "The setting " + sp_body[0] + " was changed.";

                                                break;
                                            default:
                                                break;
                                        }

                                        //if (nwCheckInReplyTimer(dt) != false)
                                        //    replyText = "This command is not yet implemented.";

                                        break;

                                    }

                                }
                            }

                            break;


                        case "!test":

                            if (ct == ChatType.Private)
                            {

                                //int n = nwGrabGlobalUsageDB("total");
                                //replyText = n.ToString();
                                nwTestColoredConsoleWrite(" mrew ");
                                // WAITING FOR THE NEXT TEST

                                //await bot.SendTextMessageAsync(update.Message.Chat.Id, nwGrabImage("https://e621.net/post/index.xml?limit=100&tags=feline%20dickgirl%20solo%20rating:safe%20order:score"), false, false, 0, null, ParseMode.Html);

                            }
                            else
                            {

                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "This command can only be used in private messages.";

                                break;

                            }

                            break;

                        case "!shibe":
                        case "!shiba":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !shibe";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !shibe" + Environment.NewLine + "Type '!shibe help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("shiba inu", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!say":

                            //int n_sayuse = nwGrabGlobalUsageDB("say");
                            //int n_say_gmax = nwGrabGlobalMax("say");

                            // if (n_sayuse == n_say_gmax)
                            //  {
                            //      if (nwCheckInReplyTimer(dt) != false)
                            //          s_replyToUser = "Sorry, the /say command has been used too many times.";
                            //      break;
                            //  }

                            if (ct == ChatType.Private)
                            {

                                string[] stuff = nwGrabAdminsFromList(Environment.CurrentDirectory + @"\data\adminlist.txt");


                                foreach (string x in stuff)
                                {

                                    if (x.Contains(s_username) != true)
                                    {

                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "You have insufficient permissions to access this command.";
                                        break;

                                    }
                                    else
                                    {

                                        if (body == string.Empty || body == " ")
                                        {
                                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(dt) != false)
                                                s_replyToUser = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular Usage: !stats [week|fortnight|month|year|alltime|archive|commands]";

                                            break;

                                        }
                                        else if (body == "help")
                                        {
                                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            s_replyToUser = "Usage: !say [thing to say]" + Environment.NewLine + "Type '!say help' to see this message again.";

                                            break;

                                        }
                                        else
                                        {
                                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                                if (nwCheckInReplyTimer(dt) != false)
                                                    await bot.SendTextMessageAsync(-1001032131694, body);
                                                
                                            break;

                                        }

                                    }

                                }

                            }
                            else
                            {

                                if (nwCheckInReplyTimer(dt) != false)
                                    //replyText2 = body;
                                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Not working.");
                                break;

                            }

                            //if (body.Length > 2)
                            //{
                            //    break;
                            //}

                            //nwSetGlobalUsage("say", n_sayuse++); // Write new value. // set global usage incrementally

                            break;

                        case "!sayhtml":

                            //int n_sayuse = nwGrabGlobalUsageDB("say");
                            //int n_say_gmax = nwGrabGlobalMax("say");

                            // if (n_sayuse == n_say_gmax)
                            //  {
                            //      if (nwCheckInReplyTimer(dt) != false)
                            //          s_replyToUser = "Sorry, the /say command has been used too many times.";
                            //      break;
                            //  }

                            if (ct == ChatType.Private)
                            {

                                string[] stuff = nwGrabAdminsFromList(Environment.CurrentDirectory + @"\data\adminlist.txt");


                                foreach (string x in stuff)
                                {

                                    if (x.Contains(s_username) != true)
                                    {

                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "You have insufficient permissions to access this command.";
                                        break;

                                    }
                                    else
                                    {

                                        if (body == string.Empty || body == " ")
                                        {
                                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            if (nwCheckInReplyTimer(dt) != false)
                                                s_replyToUser = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular Usage: !stats [week|fortnight|month|year|alltime|archive|commands]";

                                            break;

                                        }
                                        else if (body == "help")
                                        {
                                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            s_replyToUser = "Usage: !sayhtml [thing to say]" + Environment.NewLine + "Type '!sayhtml help' to see this message again.";

                                            break;

                                        }
                                        else
                                        {
                                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                            string[] s_splitfile = new string[] { "-" };
                                            string[] boob = body.Split(s_splitfile, StringSplitOptions.RemoveEmptyEntries);

                                            StringBuilder saysb = new StringBuilder();

                                            if (Regex.IsMatch(boob[0], @"\bsfw\b", RegexOptions.IgnoreCase) == true)
                                            {

                                                boob[0] = boob[0].Replace("sfw", "");

                                                //foreach (string s_meow in boob)
                                                //{
                                                //    saysb.Append(s_meow);
                                                //}

                                                //if (boob[1] == null) { replyText = "You didn't use the prefix, silly"; }

                                                if (nwCheckInReplyTimer(dt) != false)
                                                    await bot.SendTextMessageAsync(-1001032131694, boob[0].ToString(), true, false, 0, null, ParseMode.Html);



                                            }
                                            else if (Regex.IsMatch(boob[0], @"\bnsfw\b", RegexOptions.IgnoreCase) == true)
                                            {
                                                boob[0] = boob[0].Replace("nsfw", "");

                                                //foreach (string s_meow in boob)
                                                //{
                                                //    saysb.Append(s_meow);
                                                //}

                                                if (boob[1] == null) { replyText = "You didn't use the prefix, silly"; }

                                                if (nwCheckInReplyTimer(dt) != false)
                                                    await bot.SendTextMessageAsync(-1001052518605, boob[1].ToString(), true, false, 0, null, ParseMode.Html);
                                            }
                                            //boob.

                                            //if boob.



                                            break;

                                        }

                                    }

                                }

                            }
                            else
                            {

                                if (nwCheckInReplyTimer(dt) != false)
                                    //replyText2 = body;
                                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Not working.");
                                break;

                            }

                            //if (body.Length > 2)
                            //{
                            //    break;
                            //}

                            //nwSetGlobalUsage("say", n_sayuse++); // Write new value. // set global usage incrementally

                            break;

                        case "!mow":
                        case "!homph":
                        case "!snep":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !snep";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !snep" + Environment.NewLine + "Type '!snep help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("snow leopard", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!clep":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !clep";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !clep" + Environment.NewLine + "Type '!clep help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("clouded leopard", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!leopard":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !leopard";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !leopard" + Environment.NewLine + "Type '!leopard help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("leopard", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!stat":
                        case "!stats": // change to /stats [week|month|year|alltime]

                            int n_statuse = nwGrabGlobalUsage("stats");
                            int n_stat_gmax = nwGrabGlobalMax("stats");
                            int n_stat_umax = nwGrabUserMax("stats");

                            if (n_statuse == n_stat_gmax || n_statuse == n_stat_umax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /stat command has been used too many times.";
                                break;
                            }

                            if (body == string.Empty || body == " ")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular Usage: !stats [week|fortnight|month|year|alltime|archive|commands]";

                                nwSetGlobalUsage("stats", n_statuse++); // set global usage incrementally
                                nwSetUserUsage(s_username, "stats", n_statuse++); // set this users usage incrementally

                                break;

                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !stats [timeframe to generate a link for, could be week, month, fortnight, year or alltime]" + Environment.NewLine + "Type '!stats help' to see this message again.";

                                break;

                            }
                            else
                            {

                                string ms2a = body;

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                switch (ms2a)
                                {
                                    case "week":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last weeks stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "month":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last months stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "fortnight":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last fortnights stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "year:":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last years stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "decade":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last decades stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "alltime":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view the alltime stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "archive":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view the stats archives: http://www.perthfurstats.net/node/2";
                                        break;
                                    case "command":
                                    case "commands":
                                        int tuse = nwCountTotalCommands("total");
                                        int tuse2 = nwCountTotalCommands(update.Message.From.Username);
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Since inception on Feb 15 2016, this bot has processed " + Convert.ToString(tuse) + " total commands." + Environment.NewLine + "You have personally sent the bot " + Convert.ToString(tuse2) + " total commands.";
                                        break;
                                    default:
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last weeks stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular Usage: !stats [week|month|year|alltime|archive]";
                                        break;
                                }

                                nwSetGlobalUsage("stats", n_statuse++); // set global usage incrementally
                                nwSetUserUsage(s_username, "stats", n_statuse++); // set this users usage incrementally

                                break;

                            }

                        case "!start":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This bot does not need to be started in this fashion, see /command for a list of commands.";
                            break;

                        case "!greet":
                        case "!greeting":

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                s_replyToUser = nwRandomGreeting() + " " + update.Message.From.FirstName + "!";
                                break;
                            }

                            break;

                        case "!meme":

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !meme [image to look for]";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !meme [image to look for]" + Environment.NewLine + "Type '!mee help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                retryme:

                                // list of urls.
                                string html1 = null;

                                // Checks to see if the channel we are posting to has nsfw, or 18+ in title.
                                if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
                                    html1 = GetHtmlCode(body, false, true);
                                else
                                    html1 = GetHtmlCode(body, false, false);

                                List<string> urls1 = GetUrls(html1);

                                int memecount = urls1.Count;

                                string s_luckyUrl = urls1[0];

                                // Check if the file is valid, or throws an unwanted status code.
                                if (!string.IsNullOrEmpty(s_luckyUrl))
                                {
                                    UriBuilder uriBuilder = new UriBuilder(s_luckyUrl);
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                                    if (response.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        Console.WriteLine("Broken - 403 Forbidden, attempting to retry.");
                                        goto retryme;
                                    }
                                    if (response.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        Console.WriteLine("Broken - 404 Not Found, attempting to retry.");
                                        goto retryme;
                                    }
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        Console.WriteLine("URL appears to be good.");
                                    }
                                    else //There are a lot of other status codes you could check for...
                                    {
                                        Console.WriteLine(string.Format("URL might be ok. Status: {0}.",
                                                                   response.StatusCode.ToString()));
                                    }
                                }

                                if (s_luckyUrl.Contains(" ") == true)
                                    s_luckyUrl.Replace(" ", "%20");

                                replyImage = s_luckyUrl;
                                break;

                            }

                            break;

                        case "!shep":
                        case "!gshep":

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !shep";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !shep" + Environment.NewLine + "Type '!shep help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("german shepherd", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        //Added by LachBee, 01/03/2017
                        //Just a copy-paste of !gshep (It's a learning experience okay?)
                        case "!rat": 

                            if (body.Contains(" ") == true)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !rat";

                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !rat" + Environment.NewLine + "Type '!rat help' to see this message again.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                replyImage = nwShowSpeciesImage("rat", update, ct);

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "!action":
                        case "!me": // TODO: Finish this command
                                    // performs an action on the caller
                                    // usage /em -[action (see list of actions)]
                                    //            //usage
                                    //    emuse = nwGrabInt("cusage/emote");
                                    //    int n_memax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/emote");
                                    //    int n_memax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/emote");

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (body == string.Empty || body == " ")
                            {
                                break;
                            }
                            else if (body == "help")
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !me [Action to perform]" + Environment.NewLine + "Type '!me help' to see this message again.";

                                break;

                            }

                            replyText = "*" + update.Message.From.Username + " " + body + "*";

                            //    replyText = nwRandomGreeting() + ". This command is coming soon. *pokes @TsarTheErmine *";

                            //    nwSetString("cusage/emote", Convert.ToString(emuse++));
                            //    nwSetUserString(update.Message.From.FirstName + "/cmd_counts/emote", Convert.ToString(emuse++));

                            break;

                        case "!exchange":
                        case "!rate":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            exchangeKey = nwGrabString("exchangeapi");

                            //https://www.exchangerate-api.com/AUD/USD?k=4a9ee4be2f2c0675d0417e64
                            string exo = httpClient.DownloadString("https://www.exchangerate-api.com/AUD/USD?k=" + exchangeKey).Result;
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "1 USD = " + exo + " AUD";

                            break;
                            
                        case "!weather": // TODO - change to BOM api

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            ////dynamic dfor = JObject.Parse(httpClient.DownloadString("http://api.wunderground.com/api/" + wundergroundKey + "/forecast/q/" + body + ".json").Result);
                            dynamic d_weather = JObject.Parse(httpClient.DownloadString("http://www.bom.gov.au/fwo/IDW60801/IDW60801.94608.json").Result);

                            // Create a new string builder
                            StringBuilder weatherString = new StringBuilder();
                            weatherString.AppendLine("Here are the current weather conditions for Perth: ");
                            weatherString.AppendLine("(" + d_weather.observations.header[0].refresh_message.ToString() + ")");

                            weatherString.AppendLine("Apparent temperature; " + d_weather.observations.data[0].apparent_t.ToString());
                            weatherString.AppendLine("Air temperature; " + d_weather.observations.data[0].air_temp.ToString());
                            weatherString.AppendLine("Dew point; " + d_weather.observations.data[0].dewpt.ToString());
                            weatherString.AppendLine("Humidity; " + d_weather.observations.data[0].rel_hum.ToString());

                            // insert rain chance here?

                            weatherString.AppendLine("Rain since 9am; " + d_weather.observations.data[0].rain_trace.ToString() + "mm");
                            weatherString.AppendLine("Wind speed; " + d_weather.observations.data[0].wind_spd_kmh.ToString() + "kph , Gusting up to " + d_weather.observations.data[0].gust_kmh.ToString()+ "kph");
                            weatherString.AppendLine("Wind direction; " + d_weather.observations.data[0].wind_dir.ToString() + "");
                            weatherString.AppendLine("This data is refreshed every 10 mins.");
                            
                            replyText = weatherString.ToString();

                            break;

                        case "!forecast":
                        case "!weather2":
                           
                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                XmlDocument dok = new XmlDocument();
                                dok.Load("ftp://ftp.bom.gov.au/anon/gen/fwo/IDW12400.xml");
                                DateTime dta1 = new DateTime(2016, 4, 1);
                                dta1 = DateTime.Now;

                                // Get our nodes
                                XmlNodeList wnodes;
                                wnodes = dok.GetElementsByTagName("forecast-period");

                                // Create a new string builder
                                StringBuilder wString = new StringBuilder();
                                wString.AppendLine("Forecast for:");

                                // Iterate through available days
                                for (var i1for = 0; i1for < wnodes.Count; i1for++)
                                {
                                    dta1 = Convert.ToDateTime(wnodes.Item(i1for).Attributes["start-time-local"].Value);

                                    wString.AppendLine(dta1.ToString("ddd d/MM/yyy") + ": " + wnodes.Item(i1for).SelectSingleNode("text").InnerText); // + " [" + pfn_events.url.ToString() + "]");
                                }

                                replyText = wString.ToString();
                            }

                                break;

                        case "!warn":
                        case "!warning":
                            // TODO : FINISH THIS COMMAND.

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (body == string.Empty || body == " ")
                            {
                                break;
                            }

                            if (ct == ChatType.Private)
                            {

                                string[] stuff = nwGrabAdminsFromList(Environment.CurrentDirectory + @"\data\adminlist.txt");


                                foreach (string x in stuff)
                                {

                                }
                            }
                            else
                            {
                                replyText = "This command only works in private messages.";
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Sorry, the /stat command has been used too many times.";

                            break;
                        case "!wiki":
                            // TODO : Finish this command.

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (body == string.Empty || body == " ")
                            {
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Sorry, the /stat command has been used too many times.";

                            break;
                        case "!user": // TODO : Finish this command
                            // This command returns a users permission level.
                            // Defaults to the person who used the command.

                            int n_useruse = nwGrabGlobalUsage("user");
                            int n_user_gmax = nwGrabGlobalMax("user");
                            int n_user_umax = nwGrabUserMax("user");

                            if (n_useruse == n_user_gmax || n_useruse == n_user_umax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /user command has been used too many times.";
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                if (ct == ChatType.Private)
                                {

                                    replyText = "Your permissions are listed below: " + Environment.NewLine + nwGetUserPermissions(s_username);

                                    break;
                                    
                                }
                                else
                                {
                                    replyText = "This command can only be used in a private message.";
                                }
                            
                            }

                            nwSetGlobalUsage("user", n_useruse++); // set global usage incrementally
                            nwSetUserUsage(s_username, "user", n_useruse++); // set this users usage incrementally

                            break;

                        case "!version":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Version " + cExtensions.nwGetFileVersionInfo.FileMajorPart + "." + cExtensions.nwGetFileVersionInfo.FileMinorPart + ", Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart;

                            break;

                        case "!about":
                        case "!info":

                            //int n_nfouse = nwGrabGlobalUsage("about");

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "PerthFurStats is the best bot" + Environment.NewLine + "Version " + cExtensions.nwGetFileVersionInfo.FileMajorPart + "." + cExtensions.nwGetFileVersionInfo.FileMinorPart + ", Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart + Environment.NewLine + "By @AnwenSnowMew" + Environment.NewLine + "GitHub: https://github.com/AndyDingo/PerthFurStats_TelegramBot" + Environment.NewLine + "This bot uses open source software.";

                            //nwSetGlobalUsage("about", n_nfouse++); // set global usage incrementally
                            //nwSetUserUsage(s_username, "about", n_nfouse++); // set this users usage incrementally

                            break;

                        case "!wrist":
                        case "!wrists":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "(╯°□°）╯︵ ┻━┻";

                            break;

                        case "!zalgo":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "O҉̢͎̗̯̪̤͍̯͎n̠̖̙͘é͕̜̦͉̤ ̷̷̩͖̹͔̲͕̻̼d͏͖͕͟o͏̼̺̰͘͠e̴̢͖̺̕s̵̵̮͇͈̩͎͢ ̢͓̱̪͇̞̮̦͉͟ͅn̝̪̩͙͘͡ͅò̢̬͈̮̙̘t̴̪̳͉̳͢͡ ̵͍̬͔̝͘ͅͅͅs҉̟͎̖͓į̳͓́m͏̰̼̻͔̩͉̺̙p̶͕̙ͅl̛͓̝̪͘y̟̝͝ ̗̪͜i̷̺͉̹n̷̢͎̮͖̜̤̼̻̙v͙͉̘͉̘͍̳o̧̖͈̩̘͝k͎͖̬̘̣̭͟e͏̟̳͚͈͈́ ̵̜͖͜Ẕ̨͎̖̖̘͟Ḁ̞͚̮̝̻͞L̶͎̙̘͠G҉̴͖̺̹̳̘͕̬͇O̸͔̞͎̻ͅ,̩͉ͅͅ";

                            break;

                        default:

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                s_replyToUser = "The command '" + update.Message.Text + "' was not found in my command database.";
                                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyText);
                            }

                            break;

                    }

                    // Add to total command use
                    nwSetTotalCommandUsage("global");
                    // Add to total command use for a given user
                    nwSetTotalCommandUsage(update.Message.From.Username);

                    // Output
                    replyText += stringBuilder.ToString();

                    if (!string.IsNullOrEmpty(replyText))
                    {

                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyText);

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, replyText);

                        using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + update.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText);
                        }
                    }

                    if (!string.IsNullOrEmpty(s_replyToUser))
                    {

                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + s_replyToUser);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + s_replyToUser);

                        int msgid = 0;
                        msgid = update.Message.MessageId;

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, s_replyToUser, false, false, msgid);

                        using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + update.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + s_replyToUser);
                        }
                    }


                    replyText2 += stringBuilder.ToString();
                    if (!string.IsNullOrEmpty(replyText2))
                    {

                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText2);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyText2);

                        await bot.SendTextMessageAsync(-1001032131694, replyText2);

                        using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + update.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText2);
                        }
                    }
                    // replyText3 For text containing urls
                    if (!string.IsNullOrEmpty(replyTextEvent))
                    {

                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyTextEvent);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyTextEvent);

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, replyTextEvent, true, false, 0, null, ParseMode.Html);

                        using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + update.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] " + me.Username + " " + replyTextEvent);
                        }
                    }
                    if (!string.IsNullOrEmpty(replyTextMarkdown))
                    {
                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyTextMarkdown);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyTextMarkdown);

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, replyTextMarkdown, false, false, 0, null, ParseMode.Markdown);

                        using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + update.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] " + me.Username + " " + replyTextMarkdown);
                        }
                    }

                    if (!string.IsNullOrEmpty(replyImage) && replyImage.Length > 5)
                    {
                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyImage);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyImage);

                        bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                        try
                        {
                            var stream = httpClient.DownloadData(replyImage).Result;
                            var extension = ".jpg";
                            if (replyImage.Contains(".gif") || replyImage.Contains("image/gif"))
                            {
                                extension = ".gif";
                            }
                            else if (replyImage.Contains(".png") || replyImage.Contains("image/png"))
                            {
                                extension = ".png";
                            }
                            else if (replyImage.Contains(".tif"))
                            {
                                extension = ".tif";
                            }
                            else if (replyImage.Contains(".bmp"))
                            {
                                extension = ".bmp";
                            }
                            var photo = new FileToSend("Photo" + extension, stream);
                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);
                            if (extension == ".gif")
                            {
                                await bot.SendDocumentAsync(update.Message.Chat.Id, photo);
                            }
                            else
                            {
                                await bot.SendPhotoAsync(update.Message.Chat.Id, photo, replyImageCaption == string.Empty ? replyImage : replyImageCaption);
                            }
                        }
                        catch (System.Net.Http.HttpRequestException ex)
                        {
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id, replyImage);
                        }
                        catch (System.Net.WebException ex)
                        {
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id, replyImage);
                        }
                        catch (NullReferenceException ex)
                        {
                            nwErrorCatcher(ex);
                        }
                        catch (Exception ex)
                        {
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyImage + " Threw: " + ex.Message);
                            await bot.SendTextMessageAsync(update.Message.Chat.Id, replyImage);
                        }
                    }
                }
            }
            catch (ehoh.IOException ex)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: System has caught an error! " + ex.Message);
                nwErrorCatcher(ex);
            }
            catch (AggregateException ex)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: System has caught an error! " + ex.Message);
                nwErrorCatcher(ex);
            }
            catch (FormatException ex)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: System has caught an error! " + ex.Message);
                nwErrorCatcher(ex);
            }
            catch (NullReferenceException ex)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: System has caught an error! " + ex.Message);
                nwErrorCatcher(ex);
            }
            catch (Exception ex)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: System has caught an error! " + ex.Message);
                nwErrorCatcher(ex);
            }
        }

        /// <summary>
        /// Show an image for a species based on inputs
        /// </summary>
        /// <param name="s_name">Species name</param>
        /// <param name="ct">chat type</param>
        /// <param name="update">the update message type.</param>
        /// <returns>returns a url, from which an image is extracted.</returns>
        private static string nwShowSpeciesImage(string s_name, Update update, ChatType ct)
        {
            retryme:

            // list of urls.
            string html = null;

            // Checks to see if the channel we are posting to has nsfw, or 18+ in title.
            if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
                html = GetHtmlCode(s_name, false, true);
            else
                html = GetHtmlCode(s_name, false, false);

            List<string> urls = GetUrls(html);
            var rnd = new Random();

            int randomUrl = rnd.Next(0, urls.Count - 1);

            // Select url from url list.
            string luckyUrl = urls[randomUrl];

            // Check if the file is valid, or throws an unwanted status code.
            if (!string.IsNullOrEmpty(luckyUrl))
            {
                UriBuilder uriBuilder = new UriBuilder(luckyUrl);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Console.WriteLine("Broken - 400 Bad Request, attempting to retry.");
                    goto retryme;
                }
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Broken - 404 Not Found, attempting to retry.");
                    goto retryme;
                }
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("URL appears to be good.");
                }
                else //There are a lot of other status codes you could check for...
                {
                    Console.WriteLine(string.Format("URL might be ok. Status: {0}.",
                                               response.StatusCode.ToString()));
                }

            }

            if (luckyUrl.Contains(" ") == true)
                luckyUrl.Replace(" ", "%20");

            return luckyUrl;
        }

        private static void nwWriteORToDB(string username, string body)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                string sql = "INSERT INTO tbl_oo (username, reason) VALUES ('" + username + "', '" + body + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwSendEmail(string username,string reason)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add("nwcreations@gmail.com");
            mail.From = new MailAddress("perthfurstats@hotmail.com", "OptOut request", System.Text.Encoding.UTF8);
            mail.Subject = "This mail is send from asp.net application";
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = "A user has requested to be opted out of stats generation: " + username + Environment.NewLine + "Reason: " + reason;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            SmtpClient client = new SmtpClient();
            client.Credentials = new NetworkCredential("perthfurstats@hotmail.com", "OrangeMango321");
            client.Port = 587;
            client.Host = "smtp-mail.outlook.com";
            client.EnableSsl = true;
            try
            {
                client.Send(mail);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        #region -= Bio command functions =-

        private static void nwSetBio(string username,string bio)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_bio WHERE (username = @user)", conn);
                chkUser.Parameters.AddWithValue("@user", username);
                MySqlDataReader reader = chkUser.ExecuteReader();

                if (reader.HasRows)
                {
                    nwUpdateBio(username, bio);
                }
                else
                {
                    //User NOT Exists
                    nwInsertBio(username, bio);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwUpdateBio(string username, string bio)
        {
            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "UPDATE tbl_bio SET bio='" + bio + "' WHERE username='" + username + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwInsertBio(string username, string bio)
        {

            string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {

                nwPrintSystemMessage("System: Connecting to MySQL...");
                conn.Open();

                string sql = "INSERT INTO tbl_bio (username, bio) VALUES ('" + username + "', '" + bio + "')";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

            }

            conn.Close();
            Console.WriteLine("Done.");

        }

        private static string nwShowBio(string username)
        {
            string conn = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";

            MySqlConnection msc = new MySqlConnection(conn);

            try
            {
                string str="";
                Console.WriteLine("Connecting to MySQL...");
                msc.Open();

                string sql = "SELECT bio FROM tbl_bio WHERE username='" + username + "'";
                MySqlCommand cmd = new MySqlCommand(sql, msc);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    str= Convert.ToString(rdr[0]);
                    Console.WriteLine(str);
                }

                rdr.Close();

                return "@" + username + ": " + str;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }

            msc.Close();

            Console.WriteLine("Done.");
            
        }

        #endregion

        /// <summary>
        /// Returns event information from the events xml document.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="days">Number of days to show events for.</param>
        /// <returns>A string with the information requested.</returns>
        private static string nwReturnEventInfo(DateTime dt, int days = 15)
        {

            // Current date / time
            DateTime dta = DateTime.Now;

            //Load xml
            XDocument xdoc = XDocument.Load(ehoh.Directory.GetCurrentDirectory() + @"/data/events.xml"); //you'll have to edit your path

            if (days <= 0)
                return "";
            else
            {

                //Run query
                var lv1s = from item in xdoc.Descendants("event")
                           where (Convert.ToDateTime(item.Element("start").Value) - dta).TotalDays < days
                           select new
                           {
                               title = item.Element("title").Value,
                               location = item.Element("location").Value,
                               url = item.Element("url").Value,
                               start = item.Element("start").Value,
                           };

                StringBuilder result = new StringBuilder(); //had to add this to make the result work
                result.AppendLine("Here is a list of upcoming (public) events, limited to those in the next " + days + " days. Times are in GMT +8:00.");

                //Loop through results
                foreach (var item in lv1s)
                {
                    result.AppendLine("<b>" + item.title + "</b> (<a href=\"" + item.url + "\">link</a>)");
                    result.AppendLine(Convert.ToDateTime(item.start).ToString("ddd d/MM/yyy h:mm tt") + " at <i>" + item.location + "</i>");
                    result.AppendLine("");
                }

                return result.ToString();

            }

        }

        /// <summary>
        /// Grab an image from a given url and return a string to post to the channel.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string nwGrabImage(string url)
        {
            string s_returnedImage = null;
            string s_furl = null;
            string s_randxml;

            meow:

            // Check if the file is valid, or throws an unwanted status code.
            if (!string.IsNullOrEmpty(url))
            {
                UriBuilder uriBuilder = new UriBuilder(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                request.Referer = "http://www.perthfurstats.net";
                request.KeepAlive = true;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:44.0) Gecko/20100101 Firefox/44.0 WolingoPaws/1.0";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
                if (response.StatusCode == HttpStatusCode.BadRequest)
                    goto meow;
                if (response.StatusCode == HttpStatusCode.NotFound)
                    goto meow;
                if (response.StatusCode == HttpStatusCode.Forbidden)
                    goto meow;
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    goto meow;
                if (response.StatusCode == HttpStatusCode.BadGateway)
                    goto meow;
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    goto meow;

                XmlDocument doc1 = new XmlDocument();

                // Create a new doc from the loaded xml in prev example.
                XmlDocument xdoc = new XmlDocument();

                using (ehoh.Stream dataStream = response.GetResponseStream())
                {
                    doc1.Load(dataStream);

                    if (doc1.SelectSingleNode("posts/post/file_url") != null)
                    {

                        XmlNodeList xnl_test2 = doc1.SelectNodes("posts/post");
                        
                        s_randxml = xnl_test2[new Random().Next(0, xnl_test2.Count)].OuterXml;

                        xdoc.LoadXml(s_randxml);

                        s_furl = doc1.SelectSingleNode("posts/post/file_url").InnerText;

                        Uri uri = new Uri(s_furl);
                        string filename = ehoh.Path.GetFileName(uri.LocalPath);

                        string s_sample = xdoc.SelectSingleNode("post/sample_url").InnerText;

                        string s_rating = nwConvertRating(xdoc.SelectSingleNode("post/rating").InnerText);

                        s_returnedImage = string.Format("Image: <a href=\"{1}\">{0}</a>" + Environment.NewLine +
                            "Post: https://e621.net/post/show/" + "{7}" + Environment.NewLine +
                            "Artist: <b>{2}</b> Source: (<a href=\"{3}\">link</a>)" + Environment.NewLine +
                            "Score: <b>{4}</b> Favorites: <b>{5}</b> Rating: <b>{6}</b>", filename, xdoc.SelectSingleNode("post/file_url").InnerText, xdoc.SelectSingleNode("post/artist/artist").InnerText, xdoc.SelectSingleNode("post/source").InnerText, xdoc.SelectSingleNode("post/score").InnerText, xdoc.SelectSingleNode("post/fav_count").InnerText, s_rating, xdoc.SelectSingleNode("post/id").InnerText);

                        //+Environment.NewLine +
                        //   "{8}", filename, xdoc.SelectSingleNode("post/file_url").InnerText, xdoc.SelectSingleNode("post/artist/artist").InnerText, xdoc.SelectSingleNode("post/source").InnerText, xdoc.SelectSingleNode("post/score").InnerText, xdoc.SelectSingleNode("post/fav_count").InnerText, xdoc.SelectSingleNode("post/rating").InnerText, xdoc.SelectSingleNode("post/id").InnerText, s_sample);

                        Console.WriteLine("System: The following image was selected: " + s_sample);

                    }
                    else
                    {
                        return "No image was found with the specified tag(s).";
                    }
                }
            }
            return s_returnedImage;
        }

        private static string nwConvertRating(string innerText)
        {
            switch (innerText)
            {
                case "e":
                    return "Explicit";
                case "q":
                    return "Questionable";
                case "s":
                    return "Safe";
                default:
                    return innerText;
            }
        }

        /// <summary>
        /// Return a random quotation.
        /// </summary>
        /// <param name="fchosen">The file.</param>
        /// <param name="body">The user name.</param>
        /// <returns></returns>
        private static string nwRandomQuoteLine(string fchosen, string body)
        {
            string chosen = null;
            var rng = new Random();
            int indicator = 0;

            using (var reader = File.OpenText(fchosen))
            {
                while (reader.ReadLine() != null)
                {
                    if (rng.Next(++indicator) == 0)
                    {
                        if (reader.ReadLine().Contains("] <" + body) == true)
                            chosen = reader.ReadLine();
                        else
                            chosen = "[00:00] <PerthFurStats> Test message. Do not report.";
                    }
                    indicator++;
                }
            }
            return chosen;
        }

        /// <summary>
        /// Adds randomness to the quote command.
        /// </summary>
        /// <returns></returns>
        private static string nwRandomSaidLine()
        {
            int i = cDiceBag.Instance.d4(1);
            switch (i)
            {
                case 1:
                    return "said";
                case 2:
                    return "quipped";
                case 3:
                    return "typed";
                case 4:
                    return "posted";
                default:
                    return "wrote";
            }
        }

        /// <summary>
        /// Return a random quotation.
        /// </summary>
        /// <param name="file_chosen">The file.</param>
        private static string nwRandomQuoteLine(string file_chosen)
        {
            string chosen = null;
            var rng = new Random();
            int indicator = 0;

            using (var reader = File.OpenText(file_chosen))
            {
                while (reader.ReadLine() != null)
                {
                    if (rng.Next(++indicator) == 0)
                    {
                        if (reader.ReadLine().Contains("] <") == true)
                            chosen = reader.ReadLine();
                        else
                            chosen = "[00:00] <PerthFurStats> Test message. Do not report.";
                    }
                    indicator++;
                }
            }
            return chosen;
        }

        private static void nwGenerateStatsPage(long id)
        {
            string result = id.ToString() + ".html";
            var s_filename = Environment.CurrentDirectory + @"\logs_tg\#perthfurs.csv";
            string[] log = File.ReadLines(s_filename).ToArray();

            string bot = log.Count(c => c == ",Andy,").ToString();


            var lineCount = 0;
            using (var reader = File.OpenText(s_filename))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }
            Console.WriteLine("Line count: " + lineCount.ToString());



        }

        /// <summary>
        /// Environment.CurrentDirectory + @"\data\adminlist.txt"
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static string[] nwGrabAdminsFromList(string filename)
        {
            string[] chosen = new string[] { "" };

            chosen = File.ReadAllLines(filename);

            return chosen;
        }


        private static string nwRandomJokeLine()
        {
            string chosen = null;
            var rng = new Random();
            int indicator = 0;

            using (var reader = File.OpenText(Environment.CurrentDirectory + @"\data\jokelist.txt"))
            {
                while (reader.ReadLine() != null)
                {
                    if (rng.Next(++indicator) == 0)
                    {
                        chosen = reader.ReadLine();
                    }
                    indicator++;
                }
            }
            return chosen;

        }

        /// <summary>
        /// Edit a message posted by the bot.
        /// </summary>
        /// <param name="dt">Time</param>
        /// <param name="boto">The bot.</param>
        /// <param name="n_chatid">The chat ID that the message was posted in.</param>
        /// <param name="body">The chat id and message.</param>
        private static void nwEditBotMessage(DateTime dt, TelegramBotClient boto, long n_chatid, string body)
        {
            if (body == string.Empty || body == " ")
            {

                if (nwCheckInReplyTimer(dt) != false)
                    boto.SendTextMessageAsync(n_chatid, "Usage: !edit [messageid] [the text to change]");

            }
            else
            {

                if (nwCheckInReplyTimer(dt) != false)
                {

                    string[] mysplit = new string[] { "", "", "" };
                    mysplit = body.Split('~');

                    string ms1 = mysplit[0];
                    string ms2 = mysplit[1];

                    int msg = Convert.ToInt32(ms1);

                    boto.EditMessageTextAsync(n_chatid, msg, ms2);

                }

            }

        }

        private static string nwRollDice(string s_username, DateTime dt, string body)
        {
            string tst1 = "";

            if (body == string.Empty || body == " ")
            {
                if (nwCheckInReplyTimer(dt) != false)
                    return "Usage: !roll [number of sides] [amount of dice]";
            }

            if (Regex.IsMatch(body, @"^\d+$"))
            {
                return "There's a problem with the input:"+Environment.NewLine+"Usage: !roll [number of sides] [amount of dice]";
            }

            string[] mysplit = new string[] { "1", "1", "1" };
             mysplit = body.Split(' ');

            string ms1 = mysplit[0];
            string ms2 = mysplit[1];

            //if (Regex.IsMatch(ms1, @"^\d+$"))
            //{
            //    return "Sides must actually be a number";
            //}

            //if (Regex.IsMatch(ms2, @"^\d+$"))
            //{
            //    return "Dice must actually be a number";
            //}

            int i, j;
            i = Convert.ToInt32(ms1);
            j = Convert.ToInt32(ms2);

            if (i <= 0)
            {
                return "Sides must be a positive number.";
            }

            if (j <= 0)
            {
                return "Dice must be a positive number.";
            }

            if (j <= 5)
            {
                tst1 = cDiceBag.Instance.Roll(j, i);
                return "You have rolled: " + Environment.NewLine + tst1;
            }
            else
            {
                return "Dice can't exceed 5.";
            }
        }

        /// <summary>
        /// Set a string within settings.
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="s_username">the username.</param>
        /// <remarks>Under construction.</remarks>
        private static void nwSetUserUsage(string s_username, string key, int value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_commandcfg);

            XmlElement root = doc.DocumentElement;

            XmlElement username = GetChildByName(root, s_username, doc);

            XmlElement keystone = GetChildByName(username, key, doc);

            doc.SelectSingleNode("commands/" + s_username + "/" + key).InnerText = value.ToString();
            doc.Save(s_commandcfg);
        }


        /// <summary>
        /// Set a string within settings.
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <param name="value">The value to write.</param>
        /// <remarks>Under construction.</remarks>
        private static void nwSetGlobalUsage(string key, int value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_commandcfg);
            doc.SelectSingleNode("commands/cmd_usage/" + key).InnerText = value.ToString();
            doc.Save(s_commandcfg);
        }

        /// <summary>
        /// Set a string within settings.
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <param name="value">The value to write.</param>
        /// <remarks>Under construction.</remarks>
        private static void nwSetGlobalUsage(string key, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_commandcfg);
            doc.SelectSingleNode("commands/cmd_usage/" + key).InnerText = value;
            doc.Save(s_commandcfg);
        }

        private static string nwRandom8BallResponse()
        {
            int i = cDiceBag.Instance.d20(1);
            switch (i)
            {
                case 1:
                    return "Outcome is as likely as my not caring about it.";
                case 2:
                    return "Chances are good.... if you're a betting person.";
                case 3:
                    return "Pfft, You wish.";
                case 4:
                    return "You've got to be kidding.";
                case 5:
                    return "Ask me if I care.";
                case 6:
                    return "Dear god no.";
                case 7:
                    return "Not in a million years. Maybe in fewer.";
                case 8:
                    return "Can't decide.";
                case 9:
                    return "Maybe, In a few weeks, if you're lucky.";
                case 10:
                    return "If your mommy says it is OK.";
                case 11:
                    return "Have you considered going to a psychologist?";
                case 12:
                    return "If you accentuate the positive.";
                case 13:
                    return "Between a rock and a hard place.";
                case 14:
                    return "Don't bet on it.";
                case 15:
                    return "Listen to your heart. If it's beating you're alive.";
                case 16:
                    return "One is the loneliest number that you will ever do.";
                case 17:
                    return "It smells like it.";
                case 18:
                    return "Consult me later, experiencing a Guru Meditation error.";
                case 19:
                    return "All signs point to no.";
                case 20:
                    return "If you grease a few palms.";
                default:
                    return "Yes, now give the screen a little kiss.";
            }
        }

        [Obsolete]
        private static string GetHtmlCode(string s_topic)
        {
            string url = "https://www.google.com/search?q=" + s_topic + "&safe=active&tbm=isch";
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            var response = (HttpWebResponse)request.GetResponse();

            using (ehoh.Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new ehoh.StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }

        /// <summary>
        /// Gets code
        /// </summary>
        /// <param name="s_topic">The topic to search for</param>
        /// <param name="isgif">Is this a gif?</param>
        /// <param name="isnsfw">Is this NOT safe for work/18+?</param>
        /// <returns></returns>
        private static string GetHtmlCode(string s_topic, bool isgif, bool isnsfw)
        {

            meow:

            string url = "https://www.google.com/search?q=" + s_topic + "&safe=active&tbm=isch";

            if (isnsfw == true)
            {
                url = "https://www.google.com/search?q=" + s_topic + "&tbm=isch";
            }

            string data = "";

            if (isgif == true)
            {
                url = "https://www.google.com/search?q=" + s_topic + "&safe=active&tbs=ift:gif&tbm=isch";
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                goto meow;
            if (response.StatusCode == HttpStatusCode.NotFound)
                goto meow;
            if (response.StatusCode == HttpStatusCode.Forbidden)
                goto meow;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                goto meow;
            if (response.StatusCode == HttpStatusCode.BadGateway)
                goto meow;
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                goto meow;

            using (ehoh.Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new ehoh.StreamReader(dataStream))
                {
                    data = sr.ReadToEnd();
                }
            }

            return data;

        }

        private static List<string> GetUrls(string html)
        {
            var urls = new List<string>();

            int ndx = html.IndexOf("\"ou\"", StringComparison.Ordinal);

            while (ndx >= 0)
            {
                ndx = html.IndexOf("\"", ndx + 4, StringComparison.Ordinal);
                ndx++;
                int ndx2 = html.IndexOf("\"", ndx, StringComparison.Ordinal);
                string url = html.Substring(ndx, ndx2 - ndx);
                urls.Add(url);
                ndx = html.IndexOf("\"ou\"", ndx2, StringComparison.Ordinal);
            }
            return urls;
        }

        private static byte[] GetImage(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();

            using (ehoh.Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return null;
                using (var sr = new ehoh.BinaryReader(dataStream))
                {
                    byte[] bytes = sr.ReadBytes(100000000);

                    return bytes;
                }
            }
            
        }

        /// <summary>
        /// Get the users permissions, display to the user.
        /// </summary>
        /// <param name="s_username"></param>
        private static string nwGetUserPermissions(string s_username)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(s_ucfgfile);

            XmlNodeList xnl1 = xdoc.GetElementsByTagName("owners");
            XmlNodeList xnl2 = xdoc.GetElementsByTagName("admins");
            XmlNodeList xnl3 = xdoc.GetElementsByTagName("developers");
            
            StringBuilder sb = new StringBuilder();

            for (var io = 0; io > xnl1.Count; io++) { sb.AppendLine(xnl1.Item(io).SelectSingleNode("admins").InnerText); }
            for (var io = 0; io > xnl2.Count; io++) { sb.AppendLine(xnl2.Item(io).SelectSingleNode("admins").InnerText); }
            for (var io = 0; io > xnl3.Count; io++) { sb.AppendLine(xnl3.Item(io).SelectSingleNode("admins").InnerText); }

            return sb.ToString();
        }

        /// <summary>
        /// Multi-color line method.
        /// </summary>
        /// <param name="color">The ConsoleColor.</param>
        /// <param name="text">The text to write.</param>
        public static void ColoredConsoleWrite(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Spit out a randomly selected greeting.
        /// </summary>
        /// <returns>A greeting, as a string.</returns>
        private static string nwRandomGreeting()
        {
            int i = cDiceBag.Instance.d8(1);
            switch (i)
            {
                case 1:
                    return "Hi";
                case 2:
                    return "Greetings";
                case 3:
                    return "Howdy";
                case 4:
                    return "Ciao";
                case 5:
                    return "Hello";
                case 6:
                    return "Good day";
                case 7:
                    return "Hi-ya";
                case 8:
                    return "Shalom";
                default:
                    return "Hallo";
            }

        }

        /// <summary>
        /// Returns whether or not we are in the 2 min grace period for commands.
        /// </summary>
        /// <param name="dt">A DateTime object.</param>
        /// <returns>TRUE, or FALSE.</returns>
        private static bool nwCheckInReplyTimer(DateTime dt)
        {
            //insert delay here
            DateTime endTime = DateTime.Now; // current time
            DateTime startTime = dt; // message time
            TimeSpan span = endTime.Subtract(startTime);

            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: CHECK IN REPLY EVENT TRIGGERED.");

            if (span.Minutes <= 2 || span.Minutes == 0)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: LESS THAN OR EQUAL TO 2 MINUTES, PROCEED.");
                return true;
            }
            else
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: NOT LESS THAN OR EQUAL TO 2 MINUTES, DO NOT PROCEED. [" + span.Minutes + "]");
                return false;
            }
        }

        /// <summary>
        /// Print a system message in ConsoleColor.Yellow
        /// </summary>
        /// <param name="text">The text to print.</param>
        private static void nwPrintSystemMessage(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Green;
        }

        public static void nwTestColoredConsoleWrite(string text)
        {
            ColoredConsoleWrite(ConsoleColor.Yellow, "System:");
            ColoredConsoleWrite(ConsoleColor.Cyan, text);
            ColoredConsoleWrite(ConsoleColor.DarkGray, "Meow");
        }

        /// <summary>
        /// Catch an error, do a few things to it.
        /// </summary>
        /// <param name="ex"></param>
        private static void nwErrorCatcher(Exception ex)
        {
            DateTime curTime = DateTime.Now; // current time

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("* System: Error has occurred at " + curTime.ToLongTimeString());
            Console.WriteLine("* System: Error details: " + ex.HResult + " " + ex.Message + Environment.NewLine + "* System: " + ex.StackTrace);
            Console.ForegroundColor = ConsoleColor.Green;

            using (ehoh.StreamWriter sw = new ehoh.StreamWriter(ehoh.Directory.GetCurrentDirectory() + @"\pfsTelegramBot.log", true))
            {
                 sw.WriteLine("-----------------------------------------------------------------------------");
                 sw.WriteLine("* System: Error has occurred at " + curTime.ToLongTimeString());
                 sw.WriteLine("* System: Error has occurred: " + ex.HResult + " " + ex.Message + Environment.NewLine +
                    "* System: Stack Trace: " + ex.StackTrace + Environment.NewLine +
                    "* System: Inner Exception: " + ex.InnerException + Environment.NewLine +
                    "* System: Inner Exception: " + ex.InnerException.Data.ToString() + Environment.NewLine +
                    "* System: Inner Exception: " + ex.InnerException.Message + Environment.NewLine +
                    "* System: Inner Exception: " + ex.InnerException.Source + Environment.NewLine +
                    "* System: Inner Exception: " + ex.InnerException.StackTrace + Environment.NewLine +
                    "* System: Inner Exception: " + ex.InnerException.TargetSite + Environment.NewLine +
                    "* System: Source: " + ex.Source + Environment.NewLine +
                   "* System: Target Site: " + ex.TargetSite + Environment.NewLine +
                   "* System: Help Link: " + ex.HelpLink);
            }

        }
       
    }

    /// <summary>
    /// The channel designation
    /// </summary>
    public enum eChannel
    {
        sfw,
        nsfw,
    }

}

