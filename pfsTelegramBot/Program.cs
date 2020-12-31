/* 
 * All contents copyright 2016 - 2020, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by   : Microsoft Visual Studio 2015.
 * User         : AndyDingoWolf
 * Last Updated : 22/12/2019 by JessicaEira
 * -- VERSION --
 * Version      : 1.0.0.203
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ehoh = System.IO;
using File = System.IO.File;

namespace TelegramBot1
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
        public static string s_botdb = Environment.CurrentDirectory + @"\data\botdata.s3db"; // Main config
        public static string s_commandcfg = Environment.CurrentDirectory + @"\data\commandlist.xml"; // User config
        private static readonly TelegramBotClient Bot = new TelegramBotClient(nwGrabString("botapitoken")); // Don't hard-code this.
        #endregion

        /// <summary>
        /// This is the main method, if you will.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            var me = Bot.GetMeAsync().Result;
            Console.Title = "Jessica's Telegram Group Command Bot";

            DateTime dt = new DateTime(2016, 2, 2, 5, 30, 0);
            dt = DateTime.Now;

            // Do the title
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("-------------- Jessica's Telegram Group Command Bot ---------------");
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            // Events
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            // Do the initial starting routine, populate our settings file if it doesn't exist.
            nwInitialStuff(dt);

            Console.WriteLine(); // blank line

            Bot.StartReceiving(Array.Empty<UpdateType>());
            nwSystemCCWrite(dt.ToString(nwParseFormat(false)), $"Start listening for @{me.Username}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
            Bot.StopReceiving();

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

                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Loading configuration...");

                // Work item 01. Create our XML document if it doesn't exist
                //if (File.Exists(s_cfgfile) != true)
                //    nwCreateSettings();

                // Populate the strings
                str_ups = nwGrabString("updatesite"); //update site

                Console.WriteLine(); // blank line

                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Using configuration file: " + s_cfgfile);
                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Logging to file: " + Environment.CurrentDirectory + @"\logs\<this chatroom id>." + dt.ToString(nwGrabString("dateformat")) + ".log");
                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Finished loading configuration...");

                Console.WriteLine(); // blank line

                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Checking for update...");

                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "Finished checking for update...");

                nwDoUpdateCheck(dt, str_ups); // Do our update check.
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

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs e)
        {
            Console.WriteLine($"Received inline query from: {e.InlineQuery.From.Id}");
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            var callbackQuery = e.CallbackQuery;

            DateTime dt = new DateTime(2016, 2, 2);
            dt = DateTime.Now;

            if (callbackQuery.Data == "Yes" && nwCheckInReplyTimer(dt) != false)
            {

                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + callbackQuery.Message.Chat.Id + " > " + "Well, here you go! *gives you a jelly baby*");

                await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, "Well, here you go! *gives you a jelly baby*");

                await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Well, here you go! *gives you a jelly baby*");

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + callbackQuery.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "]  <A.I.D.A> " + "Well, here you go! *gives you a jelly baby*");
                }

            }

            if (callbackQuery.Data == "No" && nwCheckInReplyTimer(dt) != false)
            {

                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + callbackQuery.Message.Chat.Id + " > " + "No? A shame.");

                await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, "No? A shame.");

                await Bot.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "No? A shame.");

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + callbackQuery.Message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "]  <A.I.D.A> " + "No? A shame.");
                }

            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            ChatType ct = message.Chat.Type;

            DateTime dt = new DateTime(2016, 2, 2);
            dt = DateTime.Now;

            long n_chanid = message.Chat.Id;
            DateTime m = message.Date.ToLocalTime();

            //remove unsightly characters from first names.
            string s_mffn = message.From.FirstName;
            s_mffn = Regex.Replace(s_mffn, @"[^\u0000-\u007F]", string.Empty);

            if (s_mffn.Contains(" ") == true)
                s_mffn.Replace(" ", string.Empty);

            // variable for username, if blank, use firstname.
            string s_mfun = message.From.Username;

            if (s_mfun == " " || s_mfun == string.Empty)
                s_mfun = s_mffn;

            // SAVE MESSAGES TO LOG - THIS NEEDS TO GO HERE BECAUSE TELEGRAM IS A DICK

            using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
            {
                if (nwGrabString("debugmode") == "true")
                    nwStandardCCWrite(n_chanid, message.MessageId, m.ToString(nwParseFormat(true)), s_mffn, message.Text);
                else
                    nwStandardCCWrite(m.ToString(nwParseFormat(true)), s_mffn, message.Text);
                await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_mffn + "> " + message.Text);
            }

            if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
            {
                using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + ".csv", true))
                {
                    await sw1.WriteLineAsync(message.MessageId + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + message.From.Id + "," + message.Text);
                }
            }

            // END SAVE MESSAGES

            var httpClient = new ProHttpClient();
            var text = message.Text;
            var s_replyToUser = string.Empty;
            var replyAnimation = string.Empty; // For Gifs
            var replyAnimationCaption = string.Empty; // For Gifs
            var replyImage = string.Empty;
            var replyImageCaption = string.Empty;
            var replyText = string.Empty;
            var replyTextEvent = string.Empty;
            var replyHtml = string.Empty;
            var replyVideo = string.Empty;
            var replyVideoCaption = string.Empty;

            // Test last message date.
            dt = message.Date;
            dt = dt.ToLocalTime();

            if (message.Type == MessageType.Audio)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted an audio message.");
                    else
                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted an audio message.");
                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted an audio message.");
                }

            }

            if (message.Type == MessageType.Voice)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a voice message.");
                    else
                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a voice message.");
                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a voice message.");
                }

            }

            if (message.Type == MessageType.Contact)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared the contact information of " + message.Contact.FirstName);
                    else
                        Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared the contact information of " + message.Contact.FirstName);
                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared the contact information of " + message.Contact.FirstName);
                }

            }

            if (message.Type == MessageType.Document)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared a document of type: " + message.Document.MimeType);
                    else
                        Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared a document of type: " + message.Document.MimeType);
                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has shared a document of type: " + message.Document.MimeType);
                }

            }

            if (message.Type == MessageType.ChatMembersAdded)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * " + message.NewChatMembers[0].FirstName + " has joined the group.");
                    else
                        Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + message.NewChatMembers[0].FirstName + " has joined the group.");
                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + message.NewChatMembers[0].FirstName + " has joined the group.");
                }
            }

            if (message.Type == MessageType.Sticker)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    // download the emoji for the image, if there is one. Added in May API update.
                    string s = message.Sticker.Emoji;

                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a sticker that represents the " + s + " emoticon.");
                    else
                        Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a sticker that represents the " + s + " emoticon.");

                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a sticker that represents the " + s + " emoticon.");

                }

            }

            if (message.Type == MessageType.Video)
            {

                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * " + s_mffn + " has posted a video message.");
                    else
                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a video message.");
                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_mffn + " has posted a video message.");
                }

            }

            if (message.Type == MessageType.Venue) return;
            if (message.Type == MessageType.Location) return;

            if (message == null || message.Type != MessageType.Text) return;

            if (text.Length == 1 && text.Contains('/') || text.Length == 1 && text.Contains('!')) return;

            if (text != null && (text.StartsWith("!", StringComparison.Ordinal)) || (text.StartsWith("/", StringComparison.Ordinal)))
            {
                // Log to console
                nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "User " + s_mfun + " has used the command " + text);

                //text = "!" + text.Substring(1);

                // Strip @BotName
                //text = text.Replace("@" + me.Username, "");

                // Parse
                var command = message.Text.Split(' ').First();
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

                // Put the command in lowercase. So !Cat becomes !cat
                command = command.ToLower();

                var stringBuilder = new StringBuilder();

                // Here, we list our commands
                switch (command)
                {
                    case "!ball":
                    case "!8ball":

                        if (nwCheckInReplyTimer(dt) != false)
                        {
                            if (body == string.Empty || body == " " || body == "@" || body.Contains("?") == false || body == null)
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "You haven't given me a question to answer." + Environment.NewLine + "Usage: !8ball question to ask?";

                                break;
                            }
                            else if (body == "help")
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !8ball [question to ask, followed by a question mark]" + Environment.NewLine + "Type '!8ball help' to see this message again.";

                                break;

                            }

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = nwRandom8BallResponse();
                            break;
                        }

                        break;

                    case "!cat":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                        // if it is okay to reply, do so.
                        if (nwCheckInReplyTimer(dt) != false)
                        {
                            replyImage = "http://thecatapi.com/api/images/get?format=src&type=jpg,png";
                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!die":
                    case "!kill":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (ct == ChatType.Private)
                        {
                            if (s_mfun != "JessicaSnowMew")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "You have insufficient permissions to access this command.";
                                else
                                    Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                                break;
                            }

                            s_replyToUser = "Goodbye.";

                            await Task.Delay(10000);

                            Environment.Exit(0);
                        }
                        else
                        {
                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command can only be used in private messages.";
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                            break;
                        }

                        break;

                    case "!debug":
                    case "!debugmode":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (ct == ChatType.Private)
                        {
                            if (s_mfun != "JessicaSnowMew")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have insufficient permissions to access this command.";
                                else
                                    Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
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
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                            break;
                        }

                        break;

                    case "!dingo":

                        if (body.Contains(" ") == true)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !dingo";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !dingo" + Environment.NewLine + "Type '!dingo help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            replyImage = nwShowSpeciesImage("dingo");

                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!echo":

                        if (ct == ChatType.Private)
                        {
                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "Too short.";

                                    break;
                                }

                                replyText = body;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                            }
                        }
                        else
                        {
                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command can only be used in private messages.";
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                            break;

                        }

                        break;

                    case "!edit":

                        ChatMember[] cm_admine = await Bot.GetChatAdministratorsAsync(message.Chat.Id);

                        foreach (ChatMember x in cm_admine)
                        {

                            if (x.User.Username.Contains(s_mfun) != true || s_mfun != "JessicaSnowMew")
                            {

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have insufficient permissions to access this command.";

                                break;

                            }
                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {
                                await Bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, body);
                            }
                            break;
                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;
                        
                    case "!addevent": // TODO: Create events
                    case "/addevent": // TODO: Create events

                        //<event>
                        //  <id>4</id>
                        //  <url>http://www.furoutwest.com/</url>
                        //  <title>Fur Out West</title>
                        //  <location>Atrium Hotel Mandurah</location>
                        //  <start>2017-11-24 09:00:00</start>
                        //  <end>2017-11-26 23:59:00</end>
                        //</event>

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                replyText = "I can't let you do that Dave.";

                            }
                            else if (body == "help")
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !addevent [url],['Event Title'],[Date],[Host name]" + Environment.NewLine + "The optional parameter number can be omitted, in which it just returns events for last 15 days by default." + Environment.NewLine + "Type '!event help' to see this message again.";

                                break;

                            }
                            else
                            {

                                string[] mysplit = new string[] { "1", "1", "1" ,"1","1"};
                                mysplit = body.Split(',');

                                var fname=ehoh.Directory.GetCurrentDirectory() + @"/data/events.xml";

                                var event1 = new XElement("event", new XElement("id", dt.Ticks), 
                                    new XElement("url", mysplit[0]), 
                                    new XElement("title", mysplit[1]), 
                                    new XElement("location", mysplit[3]),
                                    new XElement("start", mysplit[2]),
                                    new XElement("end", "TBD"),
                                    new XElement("authorid", message.From.Id),
                                    new XElement("host", mysplit[4]),
                                    new XElement("created", dt.ToString("yyyy-MM-dd HH:mm")));
                                
                                var doc = new XDocument();

                                if (ehoh.File.Exists(fname))
                                {
                                    
                                    doc = XDocument.Load(fname);
                                    doc.Element("events").Add(event1);

                                }

                                doc.Save(fname);

                            }

                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!meet":
                    case "!meets":
                    case "!event":
                    case "!events": // TODO: Finish this command

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                if (nwReturnEventInfo(dt) != "maow")
                                    replyTextEvent = nwReturnEventInfo(dt);
                                else
                                    replyText = "I can't let you do that Dave.";
                                break;
                            }
                            else if (body == "help")
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

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
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!con":
                    case "!cons":
                    case "!convention": // TODO: Finish this command
                    case "!conventions": // TODO: Finish this command

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {
                            XmlDocument dook = new XmlDocument();
                            dook.Load(ehoh.Directory.GetCurrentDirectory() + @"/data/conventions.xml");
                            DateTime _dta1 = new DateTime(2016, 4, 1);
                            _dta1 = DateTime.Now;

                            DateTime _dta2 = new DateTime(2016, 4, 1);
                            _dta2 = DateTime.Now;

                            // Get our nodes
                            XmlNodeList nodes;
                            nodes = dook.GetElementsByTagName("event");

                            // Create a new string builder
                            StringBuilder eventString = new StringBuilder();
                            eventString.AppendLine("Here is a list of upcoming Australian conventions. Times are in local time, and may be subject to change.");

                            // Iterate through available events
                            for (var i1for = 0; i1for < nodes.Count; i1for++)
                            {
                                _dta1 = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("start").InnerText);
                                _dta2 = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("end").InnerText);
                                eventString.AppendLine("<b>" + nodes.Item(i1for).SelectSingleNode("title").InnerText + "</b> [" + nodes.Item(i1for).SelectSingleNode("url").InnerText + "]");
                                eventString.AppendLine("Convention starts: " + _dta1.ToString("ddd d/MM/yyy") + " (" + _dta1.ToString("h:mm tt") + ")");
                                eventString.AppendLine("Convention ends: " + _dta2.ToString("ddd d/MM/yyy") + " (" + _dta2.ToString("h:mm tt") + ")");
                                eventString.AppendLine("Location: <i>" + nodes.Item(i1for).SelectSingleNode("location").InnerText + "</i>");
                                eventString.AppendLine("");
                            }

                            replyTextEvent = eventString.ToString();

                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!canine":
                    case "!dog":
                    case "!doggo":

                        if (body.Contains(" ") == true)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !dog";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !dog" + Environment.NewLine + "Type '!dog help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            replyImage = nwShowSpeciesImage("dog");

                        }
                        else
                        {
                            Console.WriteLine("The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!corgi":

                        if (body.Contains(" ") == true)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !corgi";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !corgi" + Environment.NewLine + "Type '!corgi help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            replyImage = nwShowSpeciesImage("corgi");

                        }
                        else
                        {
                            Console.WriteLine("The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!dino":

                        if (body.Contains(" ") == true)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !dino";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !dino" + Environment.NewLine + "Type '!dino help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            replyImage = nwShowSpeciesImage("dinosaur");

                        }
                        else
                        {
                            Console.WriteLine("The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!ferret":

                        if (body.Contains(" ") == true)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !ferret";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !ferret" + Environment.NewLine + "Type '!ferret help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            replyImage = nwShowSpeciesImage("ferret");

                        }
                        else
                        {
                            Console.WriteLine("The " + command + " failed as it took too long to process.");
                        }

                        break;


                    case "!greet":
                    case "!greeting":

                        if (nwCheckInReplyTimer(dt) != false)
                        {
                            s_replyToUser = nwRandomGreeting() + " " + message.From.FirstName + "!";
                            break;
                        }

                        break;

                    case "!gif":

                        //nwInsertUserCmdValues(message.From.Id, "gif", dt);

                        if (body == string.Empty || body == " " || body == "@" || body == null)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !gif [image to look for]";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !gif [Image to look for]" + Environment.NewLine + "Type '!gif help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                        retryme:

                            //GfycatClient client = new GfycatClient("2_cggidt", "tP-MFQOQLgMbJviB7nObCbuEVhafimmJX5sG30x1_t_jLvWpbV0IulP-YAJl4DiY");
                            //await client.AuthenticateAsync();

                            //var gifs= client.GetTrendingGfysAsync(body);

                            //gifs.Result.ToString();

                            //Console.WriteLine(gifs.Result);


                            dynamic d_gif = JObject.Parse(httpClient.DownloadString("https://api.gfycat.com/v1/gfycats/search?search_text=" + body).Result);

                            //.max5mbgif.ToString()
                            //Console.WriteLine(d_gif.gfycats[0].max5mbGif);
                            //// list of urls.
                            //string html = null;

                            //// Checks to see if the channel we are posting to has nsfw, or 18+ in title.
                            //html = GetHtmlCode(body, true, false);

                            //List<string> urls = GetUrls(html);

                            List<string> urls = new List<string>(5) { "", "", "", "", "" };
                            for (int i = 0; i < 5; i++)
                            {
                                urls[i]= d_gif.gfycats[i].max5mbGif;
                            }

                            Console.WriteLine("MEOW");


                            var rnd = new Random();

                            int randomUrl = rnd.Next(0, urls.Count - 1);

                            string luckyUrl = urls[randomUrl];

                            //string luckyUrl = d_gif.gfycats[0].max5mbGif;

                            // Check if the file is valid, or throws an unwanted status code.
                            if (!string.IsNullOrEmpty(luckyUrl))
                            {
                                UriBuilder uriBuilder = new UriBuilder(luckyUrl);
                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                                HttpWebResponse response;

                                try
                                {
                                    response = (HttpWebResponse)await request.GetResponseAsync();
                                }
                                catch (WebException we)
                                {
                                    response = (HttpWebResponse)we.Response;
                                    Console.WriteLine("MEOWCIFER");
                                }

                                if (response.StatusCode == HttpStatusCode.NotFound)
                                {
                                    Console.WriteLine("Broken - 404 Not Found, attempting to retry.");
                                    goto retryme;
                                }
                                if (response.StatusCode == HttpStatusCode.Forbidden)
                                {
                                    Console.WriteLine("Broken - 403 Forbidden, attempting to retry.");
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

                            replyAnimation = luckyUrl;

                            break;

                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

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
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !image [image to look for]";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !image [Image to look for]" + Environment.NewLine + "Type '!image help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            retryme:

                            // list of urls.
                            string html = null;

                            // Checks to see if the channel we are posting to has nsfw, or 18+ in title.
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
                                HttpWebResponse response;

                                try
                                {
                                    response = (HttpWebResponse)await request.GetResponseAsync();
                                }
                                catch (WebException we)
                                {
                                    response = (HttpWebResponse)we.Response;
                                }

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
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!jelly": // TODO: Finish this command

                        if (ct == ChatType.Private)
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                await Task.Delay(500); // simulate longer running task

                                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                {

                                    new [] // first row
                                    {
                                        InlineKeyboardButton.WithCallbackData("Yes"),
                                        InlineKeyboardButton.WithCallbackData("No"),
                                    }
                                });

                                await Bot.SendTextMessageAsync(
                                    message.Chat.Id,
                                    nwRandomGreeting() + " " + message.From.FirstName + ", Would you like a jelly baby?",
                                    replyMarkup: inlineKeyboard);
                                break;
                            }
                            else
                            {
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                            }
                        }
                        else
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command can only be used in private messages.";

                            break;

                        }
                        break;

                    case "!humour":
                    case "!joke": // TODO: Fix this command
                    case "!dadjoke":

                        //    //int n_jokeuse = nwGrabGlobalUsageDB("joke");
                        //    //int n_joke_uuse = nwGrabUserUsage(s_username, "joke");
                        //    int n_joke_gmax = nwGrabGlobalMax("joke");
                        //    //int n_joke_umax = nwGrabUserMax("joke");

                        //    //if (n_jokeuse == n_joke_gmax)//|| n_joke_uuse == n_joke_umax)
                        //    //{
                        //    //    if (nwCheckInReplyTimer(dt) != false)
                        //    //        s_replyToUser = "Sorry, the /joke command has been used too many times.";
                        //    //    break;
                        //    //}

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

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
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        //    //nwSetGlobalUsageDB("joke", n_jokeuse++); // set global usage incrementally
                        //    //nwSetUserUsage(s_username, "joke", n_joke_uuse++); // set this users usage incrementally

                        break;

                    case "!rjoke":
                    case "/rjoke":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            dynamic djoke = JObject.Parse(httpClient.DownloadString("https://api.reddit.com/r/jokes/top?t=day&limit=5").Result);
                            var rjoke = new Random(DateTime.Now.Millisecond);
                            var ijokemax = Enumerable.Count(djoke.data.children);
                            if (ijokemax > 4)
                            {
                                ijokemax = 4;
                            }
                            var ijoke = rjoke.Next(0, ijokemax);
                            replyText = djoke.data.children[ijoke].data.title.ToString() + " " + djoke.data.children[ijoke].data.selftext.ToString();

                        }
                        else
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        break;

                    case "!link":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                            s_replyToUser = "Chat link: https://t.me/FOW_Official";
                        else
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        break;

                    case "!profile":
                    case "!bio":

                        if (nwCheckInReplyTimer(dt) != false)
                            replyText = "This command is not yet implemented.";
                        else
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        Console.WriteLine("[Debug] * System: The " + command + " has not been implemented.");

                        break;

                    case "!delete":
                    case "/delete":
                    case "!deletemsg":
                    case "/deletemsg":

                        ChatMember[] cm_admind = await Bot.GetChatAdministratorsAsync(message.Chat.Id);

                        foreach (ChatMember x in cm_admind)
                        {

                            if (x.User.Username.Contains(s_mfun) != true || s_mfun != "JessicaSnowMew")
                            {

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have insufficient permissions to access this command.";

                                break;

                            }
                        }
                        
                        if (body == string.Empty || body == " " || body == "@" || body == null)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !delete [message id]";

                            break;
                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {
                            int meow1 = Convert.ToInt32(body);
                            await Bot.DeleteMessageAsync(message.Chat.Id, meow1);
                        }
                        else
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        break;

                    case "!backup":

                        // check to see if private message
                        if (ct == ChatType.Private)
                        {
                            bool b_kat = false;

                            // check the username
                            if (s_mfun != "JessicaSnowMew")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "You have insufficient permissions to access this command.";
                                break;
                            }
                            // if it is okay to reply, do so.
                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                s_replyToUser = "Starting backup...";
                                cZipBackup.Instance.CreateSample(dt.ToString(nwGrabString("dateformat")) + "_backup.zip", null, Environment.CurrentDirectory + @"\logs\");
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

                    case "!count":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (ct != ChatType.Private)
                        {
                            ChatMember[] cm_admin = await Bot.GetChatAdministratorsAsync(message.Chat.Id);

                            foreach (ChatMember x in cm_admin)
                            {

                                if (x.User.Username.Contains(s_mfun) != true)
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
                                        meow = await Bot.GetChatMembersCountAsync(message.Chat.Id);
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

                    case "!getprofile":
                    case "!getbio":

                        if (nwCheckInReplyTimer(dt) != false)
                            replyText = "This command is not yet implemented.";
                        else
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        Console.WriteLine("[Debug] * System: The " + command + " has not been implemented.");

                        break;

                    case "!rules":

                        if (nwCheckInReplyTimer(dt) != false)
                            s_replyToUser = "Group rules: " +
                                Environment.NewLine + "- All content (chat, images, stickers) must be SFW at all hours of the day." +
                                Environment.NewLine + "- No flooding or spamming of ANY kind." +
                                Environment.NewLine + "- Be nice to each other.";

                        break;

                    case "!setbio":

                        if (nwCheckInReplyTimer(dt) != false)
                            replyText = "This command is not yet implemented.";

                        break;

                    case "!say":
                    case "/say":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        //nwInsertUserCmdValues(message.From.Id, "say", dt);

                        if (ct == ChatType.Private)
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                var idsetting = nwGrabString("chatid");

                                await Bot.SendTextMessageAsync(Convert.ToInt64(idsetting), body);
                            }
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        }
                        else
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command can only be used in private messages.";
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                            break;

                        }
                        break;

                    case "!announce":
                    case "!sayhtml":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        //nwInsertUserCmdValues(message.From.Id, "sayhtml", dt);

                        if (ct == ChatType.Private)
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                                await Bot.SendTextMessageAsync(Convert.ToInt64(nwGrabString("chatid")), body, ParseMode.Html);
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        }
                        else
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command can only be used in private messages.";
                            else
                                Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                            break;

                        }
                        break;

                    case "!roll":
                    case "!diceroll":

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            string s = "";

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            // Roll our dice.
                            s = nwRollDice(s_mfun, dt, body);
                            replyText = s;

                        }
                        else
                        {

                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");

                        }

                        break;

                    case "!me":
                    case "/me":
                    case "!action":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (body == string.Empty || body == " ")
                        {
                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !me [Action to perform]" + Environment.NewLine + "Type '!me help' to see this message again.";

                            break;

                        }

                        replyText = "*" + message.From.Username + " " + body + "*";

                        break;

                    case "!mow":
                    case "!homph":
                    case "!snep":

                        //nwInsertUserCmdValues(message.From.Id, "snep", dt);

                        if (body.Contains(" ") == true)
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !snep";

                            break;
                        }
                        else if (body == "help")
                        {
                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                            s_replyToUser = "Usage: !snep" + Environment.NewLine + "Type '!snep help' to see this message again.";

                            break;

                        }

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                            replyImage = nwShowSpeciesImage("snow leopard");

                            break;

                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    case "!start":
                    case "/start":
                    case "/start@PFStats_bot":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        //nwInsertUserCmdValues(message.From.Id, "start", dt);

                        if (nwCheckInReplyTimer(dt) != false)
                            s_replyToUser = "This bot does not need to be started in this fashion, see !command or !usage for a list of commands.";
                        break;

                    case "!test":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (ct == ChatType.Private)
                        {

                            await Bot.SendTextMessageAsync(
                                message.Chat.Id,
                                "Test Successful.");

                            //nwTestDBConnection();

                            //nwInsertUserCmdValues(message.From.Id, "test", dt);

                            //Console.WriteLine(nwGetTimestamp(dt));

                            nwSystemCCWrite(dt.ToString(nwParseFormat(false)), "This is a test.");

                            //nwCreateTable(message.From.Id, "test", nwGetTimestamp(dt));

                            //Console.WriteLine(UnixTimeStampToDateTime(nwGetTimestamp(dt)).ToString());

                            //DateTimeOffset dto = new DateTimeOffset(dt, TimeSpan.Zero);
                            //Console.WriteLine("{0} --> Unix Seconds: {1}", dto, dto.ToUnixTimeSeconds());

                            //nwCheckInCooldown(dt, message.Chat.Id, message.From);


                            //string mewe = GetHtmlCode("tiger", false, false);

                           
                            //happy.
                            //mewetest.XmlResolver = null;
                            //mewetest.LoadXml(mewe);

                            //XmlNodeList mewenl = mewetest.SelectNodes("/html/body/div[2]/c - wiz/div[3]");
                            //*[@id="yDmH0d"]/div[2]/c-wiz/div[3]
                            //html/body


                        }
                        else
                        {

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command can only be used in private messages.";

                            break;

                        }
                        break;

                    case "!version":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                            s_replyToUser = "Version 2.0, Release 1";

                        break;

                    case "!about":
                    case "!info":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                            s_replyToUser = "I am the best bot" + Environment.NewLine + "Version 2.0, Release 1" + Environment.NewLine + "By @JessicaSnowMew" + Environment.NewLine + "GitHub: https://github.com/AndyDingo/PerthFurStats_TelegramBot" + Environment.NewLine + "This bot uses open source software.";

                        break;

                    case "!mods":
                    case "!admin":
                    case "!admins":
                    case "/admin":
                    case "/admins":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            StringBuilder sb = new StringBuilder();

                            ChatMember[] mew = await Bot.GetChatAdministratorsAsync(message.Chat.Id);

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


                    case "!wrists":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                            s_replyToUser = "(╯°□°）╯︵ ┻━┻";

                        break;

                    case "!help":
                    case "/help":
                    case "/help@PFStats_bot":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                            replyText = "To view a list of available commands this bot accepts as valid inputs, please see: https://jessicasbots.tumblr.com/post/178207772371/available-bot-commands, or use !list to list them.";

                        break;

                    case "!list":
                    case "!usage":
                    case "!command":
                    case "!commands":

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

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
                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: !list [page number]" + Environment.NewLine + "Type '!list help' to see this message again.";

                                break;

                            }

                            if (body == "1")
                            {

                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                StringBuilder clist = new StringBuilder();
                                clist.AppendLine("Here is a partial list of regular commands the bot understands:");
                                clist.AppendLine("You are currently viewing Page <b>[1]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                clist.AppendLine("<b>!admins</b> - show who the group admins are.");
                                clist.AppendLine("<b>!alive</b> - Check if the bot is live, please use in PM with the bot.");
                                clist.AppendLine("<b>!backup</b> - Backup bot log files, please use in PM with the bot. <i>Admin only</i>.");
                                clist.AppendLine("<b>!ball</b><a title=\"Additional commands: !8ball\" href=\"#\">*</a> - consult the magic 8 ball, use a ? at the end of your question.");
                                clist.AppendLine("<b>!bio</b> - show your bio.");
                                clist.AppendLine("<b>!cat</b> - show a cat image.");
                                clist.AppendLine("<b>!con</b> - show a list of australian furry conventions");
                                clist.AppendLine("<b>!count</b> - count number of people in chat. <i>To be revised</i>.");
                                replyTextEvent = clist.ToString();

                                break;

                            }

                            if (body == "2")
                            {

                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                StringBuilder clist = new StringBuilder();
                                clist.AppendLine("Here is a partial list of regular commands the bot understands:");
                                clist.AppendLine("You are currently viewing Page <b>[2]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                clist.AppendLine("<b>!edit</b> [message id] [replacement text] - edit a message posted by the bot. <i>Admin only</i>. <i>To be revised</i>.");
                                clist.AppendLine("<b>!event</b><a title=\"Additional commands: !events, !meet, !meets\" href=\"#\">*</a> [time constraint in days, optional] - get events list.");
                                clist.AppendLine("<b>!forecast</b> - get a 7 day weather forecast.");
                                clist.AppendLine("<b>!gif</b> [topic] - show a GIF based on a given topic. <i>To be revised</i>.");
                                clist.AppendLine("<b>!image</b><a title=\"Additional commands: !img\" href=\"#\">*</a> [topic] - show an image based on a given topic.");
                                clist.AppendLine("<b>!joke</b><a title=\"Additional commands: !humour\" href=\"#\">*</a> - get the bot to tell a joke.");
                                clist.AppendLine("<b>!link</b> - generate a chat link.");
                                clist.AppendLine("<b>!list</b><a title=\"Additional commands: !command, !commands, !help\" href=\"#\">*</a> - shows this list.");
                                replyTextEvent = clist.ToString();

                                break;

                            }

                            if (body == "3")
                            {

                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                                StringBuilder clist = new StringBuilder();
                                clist.AppendLine("Here is a partial list of regular commands the bot understands:");
                                clist.AppendLine("You are currently viewing Page <b>[3]</b> of <b>[5]</b>. Use !list [page number] to switch pages");
                                clist.AppendLine("<b>!meme</b> [topic] - show an image based on a given topic.");
                                clist.AppendLine("<b>!oo</b><a title=\"Additional commands: !optout\" href=\"#\">*</a> - opt out of stats collection.");
                                clist.AppendLine("<b>!roll</b> [dice] [sides] - roll a dice, with the given number of dice and sides.");
                                clist.AppendLine("<b>!rules</b> - show group rules.");
                                clist.AppendLine("<b>!stats</b> [week|month|year|alltime|commands] - generate a link to view stats.");
                                clist.AppendLine("<b>!weather</b> - Get current weather conditions");
                                replyTextEvent = clist.ToString();

                                break;

                            }

                            if (body == "4")
                            {

                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

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

                                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

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
                        }

                        break;

                    case "!forecast":
                    case "!weather2":

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {

                            XmlDocument dok = new XmlDocument();

                            string apistring = "http://api.weatherapi.com/v1/forecast.xml?key=" + nwGrabString("weatherapi") + "&q=Perth&days=4";

                            dok.Load(apistring);

                            DateTime dta1 = new DateTime(2016, 4, 1);
                            dta1 = DateTime.Now;

                            // Get our nodes
                            XmlNodeList wnodes;
                            wnodes = dok.GetElementsByTagName("forecastday");

                            // Create a new string builder
                            StringBuilder wString = new StringBuilder();
                            wString.AppendLine("Forecast for:");

                            // Iterate through available days
                            for (var i1for = 0; i1for < wnodes.Count; i1for++)
                            {
                                dta1 = Convert.ToDateTime(wnodes.Item(i1for).SelectSingleNode("date").InnerText);

                                wString.AppendLine(dta1.ToString("ddd d/MM/yyy") + " > \r\nTemps : " + wnodes.Item(i1for).SelectSingleNode("day/mintemp_c").InnerText + " - " + wnodes.Item(i1for).SelectSingleNode("day/maxtemp_c").InnerText + "°C");
                            }

                            replyText = wString.ToString();
                        }


                        //if (nwCheckInReplyTimer(dt) != false)
                        //{
                        //    XmlDocument dok = new XmlDocument();
                        //    XmlDocument dok2 = new XmlDocument();
                        //    dok.Load("ftp://ftp.bom.gov.au/anon/gen/fwo/IDW12400.xml");
                        //    dok2.Load("ftp://ftp.bom.gov.au/anon/gen/fwo/IDW12300.xml");
                        //    DateTime dta1 = new DateTime(2016, 4, 1);
                        //    dta1 = DateTime.Now;

                        //    // Get our nodes
                        //    XmlNodeList wnodes;
                        //    wnodes = dok.GetElementsByTagName("forecast-period");

                        //    // Get our nodes
                        //    XmlNodeList wnodes2;
                        //    wnodes2 = dok2.GetElementsByTagName("forecast-period");


                        //    // Create a new string builder
                        //    StringBuilder wString = new StringBuilder();
                        //    wString.AppendLine("Forecast for:");

                        //    // Iterate through available days
                        //    for (var i1for = 0; i1for < wnodes.Count; i1for++)
                        //    {
                        //        dta1 = Convert.ToDateTime(wnodes.Item(i1for).Attributes["start-time-local"].Value);

                        //        wString.AppendLine(dta1.ToString("ddd d/MM/yyy") + ": " + wnodes.Item(i1for).SelectSingleNode("text").InnerText); // + " [" + pfn_events.url.ToString() + "]");
                        //    }

                        //    replyText = wString.ToString();
                        //}


                        break;

                    case "!weather": // TODO - change to BOM api

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {

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
                            weatherString.AppendLine("Wind speed; " + d_weather.observations.data[0].wind_spd_kmh.ToString() + "kph , Gusting up to " + d_weather.observations.data[0].gust_kmh.ToString() + "kph");
                            weatherString.AppendLine("Wind direction; " + d_weather.observations.data[0].wind_dir.ToString() + "");
                            weatherString.AppendLine("This data is refreshed every 10 mins.");

                            replyText = weatherString.ToString();

                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                    default:

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        if (nwCheckInReplyTimer(dt) != false)
                        {
                            string slist2 = "To view a list of available commands this bot accepts as valid inputs, please see: https://jessicasbots.tumblr.com/post/178207772371/available-bot-commands, or use !list to list them.";
                            await Bot.SendTextMessageAsync(
                                    message.Chat.Id,
                                    slist2,
                                    parseMode: ParseMode.Html,
                                    replyMarkup: new ReplyKeyboardRemove());
                        }
                        else
                        {
                            Console.WriteLine("[Debug] * System: The " + command + " failed as it took too long to process.");
                        }

                        break;

                }


                // Output
                replyText += stringBuilder.ToString();

                if (!string.IsNullOrEmpty(replyText))
                {

                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + message.Chat.Id + "] [" + message.MessageId + "] [" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + replyText);
                    else
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + message.Chat.Id + " > " + replyText);

                    await Bot.SendTextMessageAsync(message.Chat.Id, replyText);

                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                    {
                        await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "]  <A.I.D.A> " + replyText);
                    }

                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                    {
                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + ".csv", true))
                        {
                            await sw1.WriteLineAsync(message.MessageId + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + message.From.Id + "," + replyText);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(s_replyToUser))
                {

                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + message.Chat.Id + "] [" + message.MessageId + "] [" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + s_replyToUser);
                    else
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + message.Chat.Id + " > " + s_replyToUser);

                    int msgid = 0;
                    msgid = message.MessageId;

                    await Bot.SendTextMessageAsync(message.Chat.Id, s_replyToUser, ParseMode.Default, false, false, msgid);

                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                    {
                        await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + s_replyToUser);
                    }

                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                    {

                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + ".csv", true))
                        {
                            await sw1.WriteLineAsync(message.MessageId + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + message.From.Id + "," + s_replyToUser);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(replyTextEvent))
                {

                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + message.Chat.Id + "] [" + message.MessageId + "] [" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + replyTextEvent);
                    else
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + message.Chat.Id + " > " + replyTextEvent);

                    await Bot.SendTextMessageAsync(message.Chat.Id, replyTextEvent, ParseMode.Html, true, false);

                    using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + message.Chat.Id + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                    {
                        await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + replyTextEvent);
                    }

                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                    {
                        using (ehoh.StreamWriter sw1 = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + ".csv", true))
                        {
                            await sw1.WriteLineAsync(message.MessageId + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + message.From.Id + "," + replyTextEvent);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(replyImage) && replyImage.Length > 5)
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + message.Chat.Id + "] [" + message.MessageId + "] [" + dt.ToString(nwParseFormat(true)) + "] < A.I.D.A > " + replyImage);
                    else
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + message.Chat.Id + " > " + replyImage);

                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

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

                        if (extension == ".gif")
                        {
                            //await Bot.SendDocumentAsync(message.Chat.Id, stream, replyImageCaption == string.Empty ? replyImage : replyImageCaption, parseMode: ParseMode.Html);
                            await Bot.SendVideoAsync(message.Chat.Id, stream, caption: replyImageCaption == string.Empty ? replyImage : replyImageCaption, parseMode: ParseMode.Html);
                        }
                        else
                        {
                            await Bot.SendPhotoAsync(message.Chat.Id, stream, replyImageCaption == string.Empty ? replyImage : replyImageCaption, parseMode: ParseMode.Html);
                        }
                    }
                    catch (System.Net.Http.HttpRequestException ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }
                    catch (System.Net.WebException ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }
                    catch (NullReferenceException ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyImage + " Threw: " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }
                    catch (Exception ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyImage + " Threw: " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }

                }

                if (!string.IsNullOrEmpty(replyAnimation) && replyAnimation.Length > 5)
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + message.Chat.Id + "] [" + message.MessageId + "] [" + dt.ToString(nwParseFormat(true)) + "] < A.I.D.A > " + replyAnimation);
                    else
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <A.I.D.A> " + message.Chat.Id + " > " + replyAnimation);

                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                    try
                    {

                        var stream = httpClient.DownloadData(replyAnimation).Result;

                        //await Bot.SendDocumentAsync(message.Chat.Id, stream, replyImageCaption == string.Empty ? replyImage : replyImageCaption, parseMode: ParseMode.Html);
                        await Bot.SendVideoAsync(message.Chat.Id, stream, caption: replyImageCaption == string.Empty ? replyAnimation : replyImageCaption, parseMode: ParseMode.Html);

                    }
                    catch (System.Net.Http.HttpRequestException ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }
                    catch (System.Net.WebException ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }
                    catch (NullReferenceException ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyAnimation + " Threw: " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }
                    catch (Exception ex)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyAnimation + " Threw: " + ex.Message);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unable to download the requested image due to an error.");
                    }

                }

            }

            // Unknown Messages
            // TODO: Work out what to actually flocking do with them.
            if (message.Type == MessageType.Unknown)
            {
                m = message.Date.ToLocalTime();

                using (ehoh.StreamWriter sw = new ehoh.StreamWriter(Environment.CurrentDirectory + @"\logs\" + n_chanid + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                {
                    if (nwGrabString("debugmode") == "true")
                        Console.WriteLine("[" + n_chanid + "] [" + message.MessageId + "] [" + m.ToString(nwParseFormat(true)) + "] * System: Unknown, please report");
                    else
                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Unknown, please report");

                    await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");
                }

            }

        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            DateTime curTime = DateTime.Now; // current time

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.WriteLine("* System: Received error: {0} — {1}",
                e.ApiRequestException.ErrorCode,
                e.ApiRequestException.Message);
            Console.WriteLine("-----------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;

            using (ehoh.StreamWriter sw = new ehoh.StreamWriter(ehoh.Directory.GetCurrentDirectory() + @"\telegrambot.log", true))
            {
                sw.WriteLine("-----------------------------------------------------------------------------");
                sw.WriteLine("* System: Error has occurred at " + curTime.ToLongTimeString());
                sw.WriteLine("* System: Error has occurred: " + e.ApiRequestException.HResult + " " + e.ApiRequestException.Message + Environment.NewLine +
                   "* System: Stack Trace: " + e.ApiRequestException.StackTrace + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.Data.ToString() + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.Message + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.Source + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.StackTrace + Environment.NewLine +
                   "* System: Inner Exception: " + e.ApiRequestException.InnerException.TargetSite + Environment.NewLine +
                   "* System: Source: " + e.ApiRequestException.Source + Environment.NewLine +
                  "* System: Target Site: " + e.ApiRequestException.TargetSite + Environment.NewLine +
                  "* System: Help Link: " + e.ApiRequestException.HelpLink);
            }

        }


        /// <summary>
        /// Set animal noise counter
        /// </summary>
        /// <param name="noise"></param>
        /// <param name="userid"></param>
        private static void nwSetAnimalNoiseCount(string noise, int userid)
        {
            SQLiteConnection m_dbConnection = new SQLiteConnection(@"Data Source=" + s_botdb + ";Version=3;New=False;Compress=True;");
            m_dbConnection.Open();

            using (SQLiteCommand command = new SQLiteCommand(m_dbConnection))
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS user_noises (userid int not null, noise text not null)";
                command.Prepare();

                command.ExecuteNonQuery();
            }

            using (SQLiteCommand command = new SQLiteCommand(m_dbConnection))
            {
                command.CommandText = "INSERT INTO user_noises (userid int, noise text) values (@userid, @noise)";

                command.Parameters.AddWithValue("@userid", userid); // User ID
                command.Parameters.AddWithValue("@noise", noise); // Command they used
                command.Prepare();

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Set animal noise counter
        /// </summary>
        /// <param name="noise">The noise for this animal</param>
        /// <param name="userid">The user id for the last user of this</param>
        private static int nwGetAnimalNoiseCount(string noise, long userid)
        {
            string cs = @"Data Source=" + s_botdb + ";Version=3;New=False;Compress=True;";

            using (SQLiteConnection conn = new SQLiteConnection(cs))
            {
                conn.Open();

                string s_cmd = "SELECT * FROM user_noises WHERE userid=" + userid;
                int output=0;

                using (SQLiteCommand command = new SQLiteCommand(s_cmd, conn))
                {

                    using (SQLiteDataReader rdr = command.ExecuteReader())
                    {

                        while(rdr.HasRows)
                        {
                            rdr.Read();

                            output= rdr.StepCount;

                        }
                        return output;

                    }

                }

                conn.Close();

            }

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

        #region -=COMMAND TOTALS=-

        /// <summary>Count total commands used.</summary>
        /// <param name="lastuser">The username to alter data for.</param>
        //private static int nwCountTotalCommands(string lastuser)
        //{

        //    string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
        //    MySqlConnection conn = new MySqlConnection(connStr);

        //    try
        //    {
        //        conn.Open();

        //        string sq = "SELECT totalcommands FROM tbl_miscstats WHERE username='" + lastuser + "'";
        //        MySqlCommand msc = new MySqlCommand(sq, conn);
        //        int mews = Convert.ToInt32(msc.ExecuteScalar());

        //        return mews;

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return 0;
        //    }

        //    //conn.Close();
        //}

        /// <summary>Insert default value in database if none present.</summary>
        /// <param name="lastuser">The username to alter data for.</param>
        //private static void nwInsertTotalCommands(string lastuser)
        //{
        //    string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
        //    MySqlConnection conn = new MySqlConnection(connStr);

        //    try
        //    {
        //        conn.Open();

        //        string sql = "INSERT INTO tbl_miscstats (username, totalcommands) VALUES ('" + lastuser + "', '" + 1 + "')";
        //        MySqlCommand cmd = new MySqlCommand(sql, conn);
        //        cmd.ExecuteNonQuery();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }

        //    conn.Close();
        //}

        /// <summary>Update total commands used with a given value.</summary>
        /// <param name="value">Value to add.</param>
        /// <param name="lastuser">The username to alter data for.</param>
        //private static void nwUpdateTotalCommands(int value, string lastuser)
        //{
        //    string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
        //    MySqlConnection conn = new MySqlConnection(connStr);

        //    try
        //    {
        //        conn.Open();

        //        string sql = "UPDATE tbl_miscstats SET totalcommands='" + value + "' WHERE username='" + lastuser + "';";
        //        MySqlCommand cmd = new MySqlCommand(sql, conn);
        //        cmd.ExecuteNonQuery();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }

        //    conn.Close();
        //}


        /// <summary>
        /// Set total command usage.
        /// </summary>
        /// <param name="lastuser">The username to alter data for.</param>
        //private static void nwSetTotalCommandUsage(string lastuser)
        //{
        //    string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
        //    MySqlConnection conn = new MySqlConnection(connStr);

        //    try
        //    {
        //        conn.Open();

        //        MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_miscstats WHERE (username = @user)", conn);
        //        chkUser.Parameters.AddWithValue("@user", lastuser);
        //        MySqlDataReader reader = chkUser.ExecuteReader();

        //        if (reader.HasRows)
        //        {
        //            //User Exists
        //            int mews = nwCountTotalCommands(lastuser);

        //            mews++;

        //            nwUpdateTotalCommands(mews, lastuser);
        //        }
        //        else
        //        {
        //            //User NOT Exists
        //            nwInsertTotalCommands(lastuser);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }

        //    conn.Close();
        //}

        #endregion

        /// <summary>
        /// Show an image for a species based on inputs
        /// </summary>
        /// <param name="s_name">Species name</param>
        /// <returns>returns a url, from which an image is extracted.</returns>
        private static string nwShowSpeciesImage(string s_name /*Update update, ChatType ct*/)
        {
            retryme:

            // list of urls.
            string html = null;

            // Checks to see if the channel we are posting to has nsfw, or 18+ in title.
            //if (ct == ChatType.Private || update.Message.Chat.Title.Contains("NSFW") || update.Message.Chat.Title.Contains("18+"))
            //    html = GetHtmlCode(s_name, false, true);
            //else
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

            return luckyUrl;
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

        #region -= Bio command functions =-

        private static void nwSetBio(string username,string bio)
        {
            //string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            //MySqlConnection conn = new MySqlConnection(connStr);

            //try
            //{
            //    Console.WriteLine("Connecting to MySQL...");
            //    conn.Open();

            //    MySqlCommand chkUser = new MySqlCommand("SELECT * FROM tbl_bio WHERE (username = @user)", conn);
            //    chkUser.Parameters.AddWithValue("@user", username);
            //    MySqlDataReader reader = chkUser.ExecuteReader();

            //    if (reader.HasRows)
            //    {
            //        nwUpdateBio(username, bio);
            //    }
            //    else
            //    {
            //        //User NOT Exists
            //        nwInsertBio(username, bio);

            //    }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}

            //conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwUpdateBio(string username, string bio)
        {
            //string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            //MySqlConnection conn = new MySqlConnection(connStr);

            //try
            //{
            //    nwPrintSystemMessage("System: Connecting to MySQL...");
            //    conn.Open();

            //    string sql = "UPDATE tbl_bio SET bio='" + bio + "' WHERE username='" + username + "';";
            //    MySqlCommand cmd = new MySqlCommand(sql, conn);
            //    cmd.ExecuteNonQuery();


            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}

            //conn.Close();
            Console.WriteLine("Done.");
        }

        private static void nwInsertBio(string username, string bio)
        {

            //string connStr = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";
            //MySqlConnection conn = new MySqlConnection(connStr);

            //try
            //{

            //    nwPrintSystemMessage("System: Connecting to MySQL...");
            //    conn.Open();

            //    string sql = "INSERT INTO tbl_bio (username, bio) VALUES ('" + username + "', '" + bio + "')";
            //    MySqlCommand cmd = new MySqlCommand(sql, conn);
            //    cmd.ExecuteNonQuery();

            //}
            //catch (Exception ex)
            //{

            //    Console.WriteLine(ex.ToString());

            //}

            //conn.Close();
            Console.WriteLine("Done.");

        }

        //private static string nwShowBio(string username)
        //{
        //    string conn = "server=localhost;user=root;database=pfs;port=3306;password=KewlDude647;";

        //    MySqlConnection msc = new MySqlConnection(conn);

        //    try
        //    {
        //        string str="";
        //        Console.WriteLine("Connecting to MySQL...");
        //        msc.Open();

        //        string sql = "SELECT bio FROM tbl_bio WHERE username='" + username + "'";
        //        MySqlCommand cmd = new MySqlCommand(sql, msc);
        //        MySqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            str= Convert.ToString(rdr[0]);
        //            Console.WriteLine(str);
        //        }

        //        rdr.Close();

        //        return "@" + username + ": " + str;

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        return "";
        //    }

        //    //msc.Close();

        //    //Console.WriteLine("Done.");
            
        //}

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
        /// <param name="url">the url of the image</param>
        /// <returns>An image</returns>
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

        /// <summary>
        /// Generate a random joke line.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Roll a dice
        /// </summary>
        /// <param name="s_username">The username. Deprecated</param>
        /// <param name="dt">The date</param>
        /// <param name="body"></param>
        /// <returns></returns>
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

            HttpWebResponse response = null;
            HttpStatusCode statusCode;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException we)
            {
                response = (HttpWebResponse)we.Response;
            }

            statusCode = response.StatusCode;
            Console.WriteLine("[Debug] * System: Http Response Code: " + (int)statusCode + " - " + statusCode.ToString());

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

        /// <summary>
        /// Calculate the Real Feel.
        /// </summary>
        /// <param name="W">Wind.</param>
        /// <param name="A"></param>
        /// <param name="T"></param>
        /// <param name="D"></param>
        /// <param name="UVIndex">UV Index.</param>
        /// <param name="P2"></param>
        /// <returns></returns>
        public static double RealFeel(double W, double A, double T, double D, int UVIndex, int P2)
        {
            // Adjust Wind
            double WA = (W < 4) ? (W / 2 + 2) : (W < 56) ? W : 56;

            double WSP1 = Math.Sqrt(W) * ((Math.Sqrt(A / 10.0)) / 10.0);
            double WSP2 = (80.0 - T) * (0.566 + 0.25 * Math.Sqrt(WA) - 0.0166 * WA) * ((Math.Sqrt(A / 10.0)) / 10.0);

            double SI2 = (double)UVIndex;// UV index is already in hectoJoules/m^2 (0-16)

            double DA = (D >= (55 + Math.Sqrt(W))) ? D : 55.0 + Math.Sqrt(W);
            double H2 = (DA - 55.0 - Math.Sqrt(W)) * 2.0 / 30.0;

            double MFT = (T >= 65) ? 80.0 - WSP2 + SI2 + H2 - P2
            : T - WSP1 + SI2 + H2 - P2;
            return MFT;
            //print realfeel(5,1013,70,6,50,0) = 75.51231765840727
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

            if (span.TotalMinutes <= 2 /*|| span.Minutes == 0*/)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: LESS THAN OR EQUAL TO 2 MINUTES, PROCEED." + span.Hours.ToString() + " Hours, " + span.Minutes.ToString() + " Minutes, " + span.Seconds.ToString() + " Seconds since last command.");
                return true;
            }
            else
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: NOT LESS THAN OR EQUAL TO 2 MINUTES, DO NOT PROCEED. [" + span.Hours.ToString() + " H " + span.Minutes.ToString() + " M " + span.Seconds.ToString() + " S." + "]");
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

            using (ehoh.StreamWriter sw = new ehoh.StreamWriter(ehoh.Directory.GetCurrentDirectory() + @"\telegrambot.log", true))
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

        /// <summary>
        /// Multi-color line method.
        /// </summary>
        /// <param name="color">The ConsoleColor.</param>
        /// <param name="text">The text to write.</param>
        public static void nwColoredConsoleWrite(ConsoleColor color, string text)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }

        /// <summary>
        /// Write system messages in the appropriate colours
        /// </summary>
        /// <param name="date"></param>
        /// <param name="message"></param>
        public static void nwSystemCCWrite(string date, string message)
        {
            nwColoredConsoleWrite(ConsoleColor.White, "[" + date + "]");
            nwColoredConsoleWrite(ConsoleColor.Yellow, " * System: ");
            nwColoredConsoleWrite(ConsoleColor.Cyan, message + "\r\n");
        }

        /// <summary>
        /// Write standard messages in the appropriate colours
        /// </summary>
        /// <param name="time">Current time.</param>
        /// <param name="message">The Current message</param>
        /// <param name="chatid">The ID for the chat/group/user.</param>
        /// <param name="messageid">The ID of the message.</param>
        /// <param name="username">The user name or first name of a user.</param>
        public static void nwStandardCCWrite(long chatid, int messageid, string time, string username, string message)
        {
            nwColoredConsoleWrite(ConsoleColor.White, String.Format("[{0}] [{1}] [{2}]", chatid, messageid, time));
            nwColoredConsoleWrite(ConsoleColor.Yellow, " <" + username + "> ");
            nwColoredConsoleWrite(ConsoleColor.Green, message + "\r\n");
        }

        /// <summary>
        /// Write standard messages in the appropriate colours
        /// </summary>
        /// <param name="time">Current time.</param>
        /// <param name="message">The Current message</param>
        /// <param name="username">The user name or first name of a user.</param>
        public static void nwStandardCCWrite(string time, string username, string message)
        {
            nwColoredConsoleWrite(ConsoleColor.White, "[" + time + "] <" + username + "> ");
            nwColoredConsoleWrite(ConsoleColor.Green, message + "\r\n");
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

