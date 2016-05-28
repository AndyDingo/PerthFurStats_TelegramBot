﻿/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.51
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Xml;
using System.Linq;
using File = System.IO.File;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.Enums;

namespace nwTelegramBot
{
#pragma warning disable 4014 // Allow for bot.SendChatAction to not be awaited
    // ReSharper disable FunctionNeverReturns
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
    // ReSharper disable CatchAllClause
    class Program
    {
        #region -= VARIABLES =-
        public static string s_logfile = Environment.CurrentDirectory + @"\pfsTelegramBot.log"; // error log
        public static string s_cfgfile = Environment.CurrentDirectory + @"\pfsTelegramBot.cfg"; // Main config
        public static string s_ucfgfile = Environment.CurrentDirectory + @"\pfsPermConfig.cfg"; // User config
        public static string s_ucmd_cfgfile = Environment.CurrentDirectory + @"\data\pfsUserCmdConfig.cfg"; // User config
        public static string s_gcmd_cfgfile = Environment.CurrentDirectory + @"\data\pfsGlobalCmdConfig.cfg"; // User config
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
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Logging to file: " + Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log");
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Finished loading configuration...");

                Console.WriteLine(); // blank line

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Checking for update...");

                //nwDoUpdateCheck(dt, str_ups); // Do our update check.
            }
            catch (Exception ex)
            {
                nwErrorCatcher(ex);
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

            s = doc.SelectSingleNode("config/" + key).InnerText;

            return s;
        }

        /// <summary>
        /// Settings file grabber
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key. On error, it will return -1.</returns>
        /// <remarks>Very BETA.</remarks>
        private static int nwGrabInt(string key)
        {
            XmlDocument doc = new XmlDocument();
            int s;

            doc.Load(s_cfgfile);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("config/" + key).InnerText);
                return s;

            }
            else { return -1; }
        }

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
        /// Set a string within settings.
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="b_usecmdcfg">Are we using the command config file instead?</param>
        /// <remarks>Under construction.</remarks>
        private static void nwSetString(string key, string value, bool b_usecmdcfg = false)
        {

            XmlDocument doc = new XmlDocument();

            if (b_usecmdcfg == false)
            {

                doc.Load(s_cfgfile);
                doc.SelectSingleNode("config/" + key).InnerText = value;
                doc.Save(s_cfgfile);

            }
            else
            {

                doc.Load(s_ucmd_cfgfile);
                doc.SelectSingleNode("config/" + key).InnerText = value;
                doc.Save(s_ucmd_cfgfile);

            }

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
            var Bot = new Client("170729696:AAGYA8FPN4RkquTRrY-teqrn-J9YdnZX22k"); // Api key, please generate your own, don't use mine.

            var me = await Bot.GetMe();

            Bot.PollingTimeout = TimeSpan.FromDays(1);
            Bot.UploadTimeout = TimeSpan.FromMinutes(5);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Hello my name is {0}, I'm a bot for Perthfurs SFW Telegram.", me.Username);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;


            int offset = 0; // status offset
            //offset = nwGrabInt("offset");

            while (true)
            {
                Update[] updates;
                updates = await Bot.GetUpdates(offset, 10000); // get updates
                //updates = await Bot.GetUpdates(); // get updates
                // For each update in the list
                foreach (Update update in updates)
                {
                    // remove unsightly characters from usernames.
                    //string ss = update.Message.From.FirstName;
                    //ss = Regex.Replace(ss, @"[^\u0000-\u007F]", string.Empty);

                    switch (update.Type)
                    {
                        case UpdateType.MessageUpdate:

                            Message message = update.Message;
                            DateTime m = update.Message.Date.ToLocalTime();

                            // Do stuff if we are a text message
                            switch (message.Type)
                            {
                                case MessageType.TextMessage:

                                    //If we have set the bot to be able to respond to our basic commands
                                    if (nwGrabString("botresponds") == "true" && update.Message.Text.StartsWith("/"))
                                    {
                                        // TODO: MOVE ALL COMMANDS TO pfsCommandBot
                                        nwProcessSlashCommands(Bot, update, me, m);
                                    }
                                    else
                                    {
                                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has attempted to use a command, but they were disabled.");
                                    }

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                    }

                                    break;

                                case MessageType.UnknownMessage:

                                    // UNKNOWN MESSAGES.
                                    // TODO: Work out what to actually flocking do with them.
                                    await Task.Delay(2000);


                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");

                                        //sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");
                                    }
                                    break;

                                case MessageType.ServiceMessage:
                                    await Task.Delay(2000);


                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: A user (" + update.Message.From.FirstName + ") has joined or left the group.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has joined or left the group!");
                                    }

                                    break;
                                // Venue messages. Added in API v2.0
                                // TODO: IMPLEMENT PROPERLY
                                case MessageType.VenueMessage:
                                    await Task.Delay(2000);


                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " posted about a venue on Foursquare.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " posted about a venue on Foursquare.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted about a venue on Foursquare.");
                                    }

                                    break;
                                // Do stuff if we are a sticker message
                                case MessageType.StickerMessage:
                                    await Task.Delay(2000);


                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        // download the emoji for the image, if there is one. Added in May API update.
                                        string s = update.Message.Sticker.Emoji;

                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a sticker that represents the " + s + " emoticon.");
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a sticker that represents the " + s + " emoticon.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a sticker that represents the " + s + " emoticon.");
                                    }

                                    break;
                                // Do stuff if we are a voice message
                                case MessageType.VoiceMessage:
                                    await Task.Delay(2000);

                                    m = update.Message.Date.ToLocalTime();

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a voice message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a voice message.");
                                    }
                                    break;

                                case MessageType.VideoMessage:
                                    await Task.Delay(2000);

                                    m = update.Message.Date.ToLocalTime();

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a video message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a video message.");
                                    }

                                    break;
                                // Do stuff if we are a photo message
                                case MessageType.PhotoMessage:
                                    await Task.Delay(2000);

                                    m = update.Message.Date.ToLocalTime(); // Get date/time

                                    //write following to file stream.
                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        // download the caption for the image, if there is one.
                                        string s = update.Message.Caption;

                                        // check to see if the caption string is empty or not
                                        if (s == string.Empty || s == null || s == "" || s == "/n")
                                        {
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a photo with no caption.");
                                            await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a photo with no caption.");
                                        }
                                        else
                                        {
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a photo with the caption '" + s + "'.");
                                            await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a photo with the caption '" + s + "'.");
                                        }
                                    }
                                    break;

                                // Do stuff if we are an audio message
                                case MessageType.AudioMessage:
                                    await Task.Delay(2000);

                                    m = update.Message.Date.ToLocalTime();

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted an audio message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted an audio message.");
                                    }
                                    break;
                                default:
                                    Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * System: BORK BORK BORK");
                                    break;

                                    
                            }

                            offset = update.Id + 1; // do not touch.
                            nwSetString("offset", offset.ToString());

                            break;
                        case UpdateType.InlineQueryUpdate:
                            break;
                        default:
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
        private static async void nwProcessSlashCommands(Client bot, Update update, User me, DateTime dt)
        {
            // read configuration and extract api keys
            var wundergroundKey = nwGrabString("weatherapi");
            var exchangeKey = nwGrabString("exchangeapi");
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
            string s_chattype = update.Message.Chat.Type.ToString().Trim(' ').Trim('\r').Trim('\n');

            // Process request
            try
            {
                var httpClient = new ProHttpClient();
                var text = update.Message.Text;
                var replyText = string.Empty;
                var replyText2 = string.Empty;
                var replyTextEvent = string.Empty;
                var replyTextMarkdown = string.Empty;
                var replyImage = string.Empty;
                var replyImageCaption = string.Empty;
                var replyDocument = string.Empty;

                if (text != null && (text.StartsWith("/", StringComparison.Ordinal) || text.StartsWith("!", StringComparison.Ordinal)))
                {
                    // Log to console
                    Console.WriteLine(update.Message.Chat.Id + " < " + update.Message.From.Username + " - " + text);

                    // Allow ! or /
                    if (text.StartsWith("!", StringComparison.Ordinal))
                    {
                        text = "/" + text.Substring(1);
                    }

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

                    switch (command.ToLowerInvariant())
                    {
                        case "/admins":
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "The group admins, to whom all must obey, are @Inflatophin and @AndyDingoFolf.";
                            break;
                        case "/alive":
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Hi " + update.Message.From.FirstName + ", I am indeed alive.";
                            break;
                        case "/backup":
                            // check to see if private message
                            if (s_chattype == "Private")
                            {
                                bool b_kat = false;
                                // check the username
                                if (s_username != "AndyDingoFolf")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        replyText = "You have insufficient permissions to access this command.";
                                    break;
                                }
                                // if it is okay to reply, do so.
                                if (nwCheckInReplyTimer(dt) != false)
                                {
                                    replyText = "Starting backup...";
                                    cZipBackup.Instance.CreateSample(dt.ToString(nwGrabString("dateformat")) + "_backup.zip", null, Environment.CurrentDirectory + @"\logs_tg\");
                                    b_kat = true;
                                }

                                if (b_kat == true)
                                {
                                    replyText = "Backup complete";
                                }
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "This command can only be used in private messages.";
                                break;
                            }
                            break;
                        case "/cat":
                            int catuse = nwGrabInt("cusage/cat");
                            int n_catmax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/cat");
                            int n_catmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/cat");

                            if (catuse == n_catmax1 || catuse == n_catmax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /cat command has been used too many times.";
                                break;
                            }

                            replyImage = "http://thecatapi.com/api/images/get?format=src&type=jpg,png";
                            nwSetString("cusage/cat", Convert.ToString(catuse++)); // increment usage
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/cat", Convert.ToString(catuse++));
                            break;
                        case "/dog":
                        case "/doge":
                        case "/shiba":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/die":
                        case "/kill":
                            if (s_chattype == "Private")
                            {
                                if (s_username != "AndyDingoFolf")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        replyText = "You have insufficient permissions to access this command.";
                                    break;
                                }

                                replyText = "Goodbye.";
                                Environment.Exit(0);
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "This command can only be used in private messages.";
                                break;
                            }
                            break;
                        case "/e621":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Not happening!";
                            break;
                        case "/dook":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Dook!";
                            break;
                        case "/count":
                            if (s_username != "AndyDingoFolf" ||
                                s_username != "Inflatophin")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/help":
                        case "/commands":
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            if (nwCheckInReplyTimer(dt) != false)
                                replyTextEvent = "Hi " + update.Message.From.FirstName + ", Here's a list of commands I can respond to: http://www.perthfurstats.net/node/11 Note that it hasn't been properly updated for Telegram yet.";
                            break;
                        case "/event":
                        case "/events": // TODO: Finish this command
                            XmlDocument dook = new XmlDocument();
                            dook.Load(Directory.GetCurrentDirectory() + @"/data/events.xml");
                            DateTime dta = new DateTime(2016, 4, 1);
                            dta = DateTime.Now;

                            // Get our nodes
                            XmlNodeList nodes;
                            nodes = dook.GetElementsByTagName("event");

                            // Create a new string builder
                            StringBuilder eventString = new StringBuilder();

                            // Iterate through available events
                            for (var i1for = 0; i1for < nodes.Count; i1for++)
                            {
                                dta = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("start").InnerText);
                                eventString.AppendLine(dta.ToString("ddd d/MM/yyy") + " (" + dta.ToString("h:mm tt") + "): " + nodes.Item(i1for).SelectSingleNode("title").InnerText + " [" + nodes.Item(i1for).SelectSingleNode("url").InnerText + "]"); // + " [" + pfn_events.url.ToString() + "]");
                            }

                            replyTextEvent = eventString.ToString();
                            break;
                        case "/debug":
                            if (s_chattype == "Private")
                            {
                                if (s_username != "AndyDingoFolf" || s_username != "Inflatophin")
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
                        case "/echo":
                            if (s_chattype == "Private")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = body;
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "This command can only be used in private messages.";
                                break;
                            }
                            break;
                        case "/jelly": // TODO: Finish this command
                            // usage /jelly [yes|no] leave blank and bot will repeat query.
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/slap": // TODO: Finish this command
                            // usage /slap [nickname] bot will slap the person matching the nickname.
                            // will return "yournickname slaps targetnickname around with [randomobject]
                            int emuse = nwGrabInt("cusage/emote");
                            int emmax = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/emote");
                            int emmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/emote");

                            if (emuse == emmax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /slap command has been used too many times.";
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                if (body == string.Empty || body == " " || body == "@")
                                {
                                    bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                                    replyText = "*@PFStats_bot slaps @" + s_username + " around a bit with a large trout!*";

                                    nwSetString("cusage/emote", Convert.ToString(emuse++));
                                    break;
                                }

                                string basestr1 = body;
                                string[] mysplit1 = new string[] { "", "" };
                                mysplit1 = basestr1.Split('@');

                                // Sanitise target string.
                                string s_target = mysplit1[1];

                                //break on empty strings
                                if (s_target == string.Empty || s_target == " ")
                                {
                                    bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    replyText = "No target was selected. Usage: /slap @username";
                                    break;
                                }

                                if (s_username == string.Empty)
                                {
                                    bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    replyText = "I'm sorry, I can't let you do that Dave";
                                    nwSetString("cusage/emote", Convert.ToString(emuse++));
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

                            nwSetString("cusage/emote", Convert.ToString(emuse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/emote", Convert.ToString(emuse++));
                            break;
                        case "/sfw":
                        case "/safeforwork":
                            int n_sfwuse = nwGrabInt("cusage/sfw");
                            int n_sfwmax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/sfw");
                            int n_sfwmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/sfw");

                            if (n_sfwuse == n_sfwmax1 || n_sfwuse == n_sfwmax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /sfw command has been used too many times.";
                                break;
                            }

                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadVideo);
                            string s_fname = Directory.GetCurrentDirectory() + @"\data\sfw.mp4";

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                // Create an instance of StreamReader to read from a file.
                                // The using statement also closes the StreamReader.
                                using (StreamReader sr = new StreamReader(s_fname))
                                {
                                    FileToSend fts = new FileToSend();
                                    fts.Content = sr.BaseStream;
                                    fts.Filename = Directory.GetCurrentDirectory() + @"\data\sfw.mp4";
                                    // Send to the channel
                                    await bot.SendVideo(update.Message.Chat.Id, fts, 0, "", false, update.Message.MessageId);
                                }
                                break;
                            }

                            nwSetString("cusage/sfw", Convert.ToString(n_sfwuse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/sfw", Convert.ToString(n_sfwuse++));
                            break;
                        case "/image": // TODO: Finish this command
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/humour":
                        case "/joke": // TODO: Fix this command
                            int jokeuse = nwGrabInt("cusage/joke");
                            int n_jokemax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/joke");
                            int n_jokemax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/joke");

                            if (jokeuse == n_jokemax1 || jokeuse == n_jokemax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /joke command has been used too many times.";
                                break;
                            }

                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "I'm sorry " + update.Message.From.FirstName + ", My humor emitter array requires recharging. Please try again another time.";

                            nwSetString("cusage/joke", Convert.ToString(jokeuse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/joke", Convert.ToString(jokeuse++));
                            break;
                        case "/link":
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Chat link: https://telegram.me/joinchat/ByYWcALujRjo8iSlWvbYIw";
                            break;
                        case "/oo":
                        case "/optout":
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following form to opt-out from stats collection. Bare in mind that your request might not be implemented till the next stats run, as it requires manual intervention. URL: http://www.perthfurstats.net/node/10";
                            break;
                        case "/roll":
                            int rolluse = nwGrabInt("cusage/roll");
                            int n_rollmax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/roll");
                            int n_rollmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/roll");

                            if (rolluse == n_rollmax1 || rolluse == n_rollmax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /roll command has been used too many times.";
                                break;
                            }

                            if (body == string.Empty || body == " ")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Usage: /roll -[number of sides] -[amount of dice]";
                                break;
                            }

                            string basestr = body;
                            string[] mysplit = new string[] { "", "", "" };
                            mysplit = basestr.Split('-');

                            string ms1 = mysplit[1];
                            string ms2 = mysplit[2];

                            int i, j;
                            i = Convert.ToInt32(ms1);
                            j = Convert.ToInt32(ms2);

                            if (j <= 5)
                            {
                                string tst1 = cDiceBag.Instance.Roll(j, i);
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have rolled: " + Environment.NewLine + tst1;
                                nwSetString("cusage/roll", Convert.ToString(rolluse++));
                                nwSetUserString(update.Message.From.FirstName + "/cmd_counts/roll", Convert.ToString(rolluse++));
                            }
                            else
                                break;

                            break;
                        case "/rules":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Group rules: " + Environment.NewLine + "- All content (chat, images, stickers) must be SFW at all hours of the day." + Environment.NewLine + "- No flooding or spamming of ANY kind." + Environment.NewLine + "- Be nice to each other.";
                            break;
                        case "/test":
                            if (s_chattype == "Private")
                            {
                                long ltest1 = update.Message.Chat.Id;
                                nwPrintSystemMessage(ltest1.ToString());
                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "This command can only be used in private messages.";
                                break;
                            }
                            break;
                        case "/set":
                        case "/settings":
                            // TODO: This would ideally need to be one of any of the config file settings
                            // Example of usage: /set -[option to set] -[new value]
                            if (s_username != "AndyDingoFolf")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/say":
                            int sayuse = nwGrabInt("cusage/say");
                            int n_saymax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/say");
                            int n_saymax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/say");

                            if (sayuse == n_saymax1 || sayuse == n_saymax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /say command has been used too many times.";
                                break;
                            }

                            if (s_chattype == "Private")
                            {
                                if (s_username != "AndyDingoFolf")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        replyText = "You have insufficient permissions to access this command.";
                                    break;
                                }

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText2 = body;
                            }

                            if (body.Length < 2)
                            {
                                break;
                            }

                            nwSetString("cusage/say", Convert.ToString(sayuse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/say", Convert.ToString(sayuse++));
                            break;
                        case "/stats": // change to /stats [week|month|year|alltime]
                            int statuse = nwGrabInt("cusage/stats");
                            int statmax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/stats");
                            int statmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/stats");

                            if (statuse == statmax1 || statuse == statmax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /stat command has been used too many times.";
                                break;
                            }

                            if (body == string.Empty || body == " ")
                            {
                                bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular usage: /stats -[week|month|year|alltime|archive|commands]";

                                nwSetString("cusage/stats", Convert.ToString(statuse++));
                                nwSetUserString(update.Message.From.FirstName + "/cmd_counts/stats", Convert.ToString(statuse++));
                                break;
                            }
                            else
                            {
                                string basestr2 = body;
                                string[] mysplit2 = new string[] { "", "" };
                                mysplit2 = basestr2.Split('-');

                                string ms2a = mysplit2[1];

                                bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                                switch (ms2a)
                                {
                                    case "week":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view this last weeks stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "month":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view this last months stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "fortnight":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view this last fortnights stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "year:":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view this last years stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "decade":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view this last decades stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "alltime":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view the alltime stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";
                                        break;
                                    case "archive":
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view the stats archives: http://www.perthfurstats.net/node/2";
                                        break;
                                    case "command":
                                    case "commands":
                                        int tuse = nwGrabInt("cusage/total");
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Since inception on Feb 15 2016, this bot has processed " + Convert.ToString(tuse) + " total commands.";
                                        break;
                                    default:
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view this last weeks stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular usage: /stats -f[week|month|year|alltime|archive]";
                                        break;
                                }
                                nwSetString("cusage/stats", Convert.ToString(statuse++));
                                break;
                            }
                        case "/start":
                        case "/greet":
                        case "/greeting":
                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + "!";
                                break;
                            }
                            break;
                        case "/em": // TODO: Finish this command
                            // usage /em -[action (see list of actions)] -[@username of target]
                            // performs an action on a target
                            emuse = nwGrabInt("cusage/emote");
                            int n_emmax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/emote");
                            int n_emmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/emote");

                            if (emuse == n_emmax1 || emuse == n_emmax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /em command has been used too many times.";
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                                if (body == string.Empty || body == " ")
                                {
                                    break;
                                }

                                if (s_chattype == "Private")
                                {



                                }


                                replyText = nwRandomGreeting() + ". This command is coming soon.";
                            }

                            nwSetString("cusage/emote", Convert.ToString(emuse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/emote", Convert.ToString(emuse++));
                            break;
                        case "/action":
                        case "/me": // TODO: Finish this command
                                    // performs an action on the caller
                                    // usage /em -[action (see list of actions)]
                                    //usage
                            emuse = nwGrabInt("cusage/emote");
                            int n_memax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/emote");
                            int n_memax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/emote");

                            if (body == string.Empty || body == " ")
                            {
                                break;
                            }

                            replyText = nwRandomGreeting() + ". This command is coming soon. *pokes @TsarTheErmine *";

                            nwSetString("cusage/emote", Convert.ToString(emuse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/emote", Convert.ToString(emuse++));
                            break;
                        case "/exchange":
                        case "/rate":
                            //string exo = httpClient.DownloadString("https//www.exchangerate-api.com/AUD/USD?k=" + exchangeKey).Result;
                            //if (nwCheckInReplyTimer(dt) != false)
                            //    replyText = "1 USD = " + exo;
                            break;
                        case "/forecast":
                        case "/weather": // TODO - change to BOM api
                            //if (body.Length < 2)
                            //{
                            //    body = "Perth, Australia";
                            //}

                            //bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                            ////dynamic dfor = JObject.Parse(httpClient.DownloadString("http://api.wunderground.com/api/" + wundergroundKey + "/forecast/q/" + body + ".json").Result);
                            //dynamic dfor = JObject.Parse(httpClient.DownloadString("http://www.bom.gov.au/fwo/IDW60801/IDW60801.94608.json").Result);
                            //if (dfor.forecast == null || dfor.forecast.txt_forecast == null)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", you have disappointed me.  \"" + body + "\" is sadly, not going to work.  Please try \"City, ST\" or \"City, Country\" next time.";
                            //    break;
                            //}
                            //for (var ifor = 0; ifor < Enumerable.Count(dfor.observations.data.sort_order) - 1; ifor++)
                            //{
                            //    if (nwCheckInReplyTimer(dt) != false)
                            //        stringBuilder.AppendLine(dfor.observations.data.sort_order[ifor].title.ToString() + ": " + dfor.observations.data.sort_order[ifor].fcttext_metric.ToString());
                            //}

                            break;
                        case "/user": // TODO : Finish this command
                            // This command returns a users permission level.
                            // Defaults to the person who used the command.
                            int useruse = nwGrabInt("cusage/user");
                            int n_usermax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/user");
                            int n_usermax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/user");

                            if (useruse == n_usermax1 || useruse == n_usermax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /user command has been used too many times.";
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                            
                                if (s_chattype == "Private")
                                {

                                    replyText = "Your permissions are listed below: " + Environment.NewLine + nwGetUserPermissions(s_username);

                                    break;
                                    
                                }
                                else
                                {
                                    replyText = "This command can only be used in a private message.";
                                }
                            
                            }

                            nwSetString("cusage/user", Convert.ToString(useruse++));
                            nwSetUserString(update.Message.From.FirstName + "/cmd_counts/user", Convert.ToString(useruse++));
                            break;
                        case "/version":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Version " + cExtensions.nwGetFileVersionInfo.FileMajorPart + "." + cExtensions.nwGetFileVersionInfo.FileMinorPart + ", Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart;
                            break;
                        case "/about":
                        case "/info":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "PerthFurStats is the best bot" + Environment.NewLine + "Version " + cExtensions.nwGetFileVersionInfo.FileMajorPart + "." + cExtensions.nwGetFileVersionInfo.FileMinorPart + ", Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart + Environment.NewLine + "By @AndyDingoWolf" + Environment.NewLine + "This bot uses open source software.";

                            nwSetString("cusage/about", Convert.ToString(1));
                            break;
                        case "/wrist":
                        case "/wrists":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "(╯°□°）╯︵ ┻━┻";
                            break;
                        default:
                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                replyText = "The command '" + update.Message.Text + "' was not found in my command database.";
                                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyText);
                            }
                            break;
                    }

                    // Add to total command use
                    int totaluse = nwGrabInt("cusage/total");
                    totaluse++;
                    nwSetString("cusage/total", Convert.ToString(totaluse));

                    // Output
                    replyText += stringBuilder.ToString();

                    if (!string.IsNullOrEmpty(replyText))
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyText);
                        await bot.SendTextMessage(update.Message.Chat.Id, replyText);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText);
                        }
                    }
                    replyText2 += stringBuilder.ToString();
                    if (!string.IsNullOrEmpty(replyText2))
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyText2);
                        await bot.SendTextMessage(-1001032131694, replyText2);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText2);
                        }
                    }
                    // replyText3 For text containing urls
                    if (!string.IsNullOrEmpty(replyTextEvent))
                    {
                        await bot.SendTextMessage(update.Message.Chat.Id, replyTextEvent, true);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] " + me.Username + " " + replyTextEvent);
                        }
                    }
                    if (!string.IsNullOrEmpty(replyTextMarkdown))
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyTextMarkdown);
                        await bot.SendTextMessage(update.Message.Chat.Id, replyTextMarkdown, false, false, 0, null, ParseMode.Markdown);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            await sw.WriteLineAsync("[" + dt.ToString(nwParseFormat(true)) + "] " + me.Username + " " + replyTextMarkdown);
                        }
                    }

                    if (!string.IsNullOrEmpty(replyImage) && replyImage.Length > 5)
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyImage);
                        bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
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
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadPhoto);
                            if (extension == ".gif")
                            {
                                await bot.SendDocument(update.Message.Chat.Id, photo);
                            }
                            else
                            {
                                await bot.SendPhoto(update.Message.Chat.Id, photo, replyImageCaption == string.Empty ? replyImage : replyImageCaption);
                            }
                        }
                        catch (System.Net.Http.HttpRequestException ex)
                        {
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                            await bot.SendTextMessage(update.Message.Chat.Id, replyImage);
                        }
                        catch (System.Net.WebException ex)
                        {
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: Unable to download " + ex.HResult + " " + ex.Message);
                            await bot.SendTextMessage(update.Message.Chat.Id, replyImage);
                        }
                        catch (NullReferenceException ex)
                        {
                            nwErrorCatcher(ex);
                        }
                        catch (Exception ex)
                        {
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: " + replyImage + " Threw: " + ex.Message);
                            await bot.SendTextMessage(update.Message.Chat.Id, replyImage);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (AggregateException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (NullReferenceException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (Exception ex)
            {
                nwErrorCatcher(ex);
            }
        }

        private static void nwSetUserString(string key, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_gcmd_cfgfile);
            doc.SelectSingleNode("config/" + key).InnerText = value;
            doc.Save(s_gcmd_cfgfile);
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
        /// Returns whether or not we are in the 10 min grace period for commands.
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

            if (span.Minutes <= 10 || span.Minutes == 0)
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: LESS THAN OR EQUAL TO 10, PROCEED.");
                return true;
            }
            else
            {
                nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] * System: NOT LESS THAN OR EQUAL TO 10, DO NOT PROCEED. [" + span.Minutes + "]");
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

        /// <summary>
        /// Catch an error, do a few things to it.
        /// </summary>
        /// <param name="ex"></param>
        private static void nwErrorCatcher(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("* System: Error has occurred: " + ex.HResult + " " + ex.Message + Environment.NewLine + "* System: " + ex.StackTrace);
            Console.ForegroundColor = ConsoleColor.Green;

            using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\pfsTelegramBot.log", true))
            {
                sw.WriteLine("-----------------------------------------------------------------------------");
                sw.WriteLine("* System: Error has occurred: " + ex.HResult + " " + ex.Message + Environment.NewLine +
                    "* System: Stack Trace: " + ex.StackTrace + Environment.NewLine +
                    //"* System: Inner Exception: " + ex.InnerException + Environment.NewLine +
                    //"* System: Inner Exception: " + ex.InnerException.Data.ToString() + Environment.NewLine +
                    //"* System: Inner Exception: " + ex.InnerException.Message + Environment.NewLine +
                    //"* System: Inner Exception: " + ex.InnerException.Source + Environment.NewLine +
                    //"* System: Inner Exception: " + ex.InnerException.StackTrace + Environment.NewLine +
                    //"* System: Inner Exception: " + ex.InnerException.TargetSite + Environment.NewLine +
                    "* System: Source: " + ex.Source + Environment.NewLine +
                   "* System: Target Site: " + ex.TargetSite + Environment.NewLine +
                   "* System: Help Link: " + ex.HelpLink);
            }
        }

        //private static InlineKeyboardMarkup GeneratePagination(int total, int current)
        //{
        //    if (total < 2)
        //        throw new ArgumentOutOfRangeException(nameof(total));

        //    if (current > total)
        //        throw new ArgumentOutOfRangeException(nameof(current));

        //    var result = new InlineKeyboardMarkup(new[]
        //        {
        //    new InlineKeyboardButton[total > 4 ? 5 : total]
        //}
        //    );

        //    if (current == 1)
        //        result.InlineKeyboard[0][0] = new InlineKeyboardButton("·1·", "1");
        //    else if (current < 4 || total < 6)
        //        result.InlineKeyboard[0][0] = new InlineKeyboardButton(" 1 ", "1");
        //    else
        //        result.InlineKeyboard[0][0] = new InlineKeyboardButton("«1 ", "1");

        //    if (current == 2)
        //        result.InlineKeyboard[0][1] = new InlineKeyboardButton("·2·", "2");
        //    else if (current < 4 || total < 6)
        //        result.InlineKeyboard[0][1] = new InlineKeyboardButton(" 2 ", "2");
        //    else if (current > total - 2)
        //        result.InlineKeyboard[0][1] = new InlineKeyboardButton($"‹{total - 3} ", $"{total - 3}");
        //    else
        //        result.InlineKeyboard[0][1] = new InlineKeyboardButton($"‹{current - 1} ", $"{current - 1}");

        //    if (total > 2)
        //        if (current < 3 || (total < 5 && current != 3))
        //            result.InlineKeyboard[0][2] = new InlineKeyboardButton(" 3 ", "3");
        //        else if (current != 3 && current > total - 2)
        //            result.InlineKeyboard[0][2] = new InlineKeyboardButton($" {total - 2} ", $"{ total - 2 }");
        //        else
        //            result.InlineKeyboard[0][2] = new InlineKeyboardButton($"·{current}·", $"{current}");

        //    if (total == 4)
        //        if (current == 4)
        //            result.InlineKeyboard[0][3] = new InlineKeyboardButton("·4·", "4");
        //        else
        //            result.InlineKeyboard[0][3] = new InlineKeyboardButton(" 4 ", "4");
        //    else if (total > 3)
        //        if (current < 3 && total > 5)
        //            result.InlineKeyboard[0][3] = new InlineKeyboardButton(" 4›", "4");
        //        else if (current < total - 2 && total > 5)
        //            result.InlineKeyboard[0][3] = new InlineKeyboardButton($" {current + 1}›", $"{current + 1}");
        //        else if (current == total - 1)
        //            result.InlineKeyboard[0][3] = new InlineKeyboardButton($"·{current}·", $"{current}");
        //        else
        //            result.InlineKeyboard[0][3] = new InlineKeyboardButton($" {total - 1} ", $"{total - 1}");

        //    if (total > 4)
        //        if (current == total)
        //            result.InlineKeyboard[0][4] = new InlineKeyboardButton($"·{current}·", $"{current}");
        //        else if (current > total - 3 || total < 6)
        //            result.InlineKeyboard[0][4] = new InlineKeyboardButton($" {total} ", $"{total}");
        //        else
        //            result.InlineKeyboard[0][4] = new InlineKeyboardButton($" {total}»", $"{total}");

        //    return result;
        //}
    }
}

