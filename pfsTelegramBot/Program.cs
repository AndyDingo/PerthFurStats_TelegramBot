/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.38
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
using System.Web;

namespace nwTelegramBot
{
#pragma warning disable 4014 // Allow for bot.SendChatAction to not be awaited
    // ReSharper disable FunctionNeverReturns
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
    // ReSharper disable CatchAllClause
    class Program
    {
        // Declare Variables
        public static string logfile = Environment.CurrentDirectory + @"\pfsTelegramBot.log";
        public static string updfile = Environment.CurrentDirectory + @"\updchk.xml";
        public static string cfgfile = Environment.CurrentDirectory + @"\pfsTelegramBot.cfg"; // Main config
        public static string ucfgfile = Environment.CurrentDirectory + @"\pfsTelegramBot.User.cfg"; // User config

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

                if (isAvailable == true)
                    Run().Wait();
                else
                    Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: No valid Internet Connection detected.");
            }
            catch (TaskCanceledException ex)
            {
                nwErrorCatcher(ex);
            }
            catch (Exception ex)
            {
                nwErrorCatcher(ex);
            }
        }

        /// <summary>
        /// Our initial configuration and update run.
        /// </summary>
        /// <param name="dt">a datetime object that we need to add correct timestamps.</param>
        private static void nwInitialStuff(DateTime dt)
        {
            try
            {
                string s, t, u, str_ups;

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Loading configuration...");

                // Work item 01. Create our XML document if it doesn't exist
                if (File.Exists(cfgfile)!=true)
                    nwCreateSettings();

                // Populate the strings
                s = nwGrabString("filename"); //file name [NYI]
                str_ups = nwGrabString("updatesite"); //update site

                t = @"\";
                u = t + s;

                Console.WriteLine(); // blank line

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Using configuration file: " + cfgfile);
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Logging to file: " + Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log");
                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Finished loading configuration...");

                Console.WriteLine(); // blank line

                Console.WriteLine("[" + dt.ToString(nwParseFormat(false)) + "] * System: Checking for update...");

                nwDoUpdateCheck(dt, str_ups); // Do our update check.
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
            using (XmlWriter writer = XmlWriter.Create(cfgfile, settings))
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

            doc.Load(cfgfile);

            s = doc.SelectSingleNode("config/" + key).InnerText;

            return s;
        }

        /// <summary>
        /// Settings file grabber
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key.</returns>
        /// <remarks>Very BETA.</remarks>
        private static int nwGrabInt(string key)
        {
            XmlDocument doc = new XmlDocument();
            int s;

            doc.Load(cfgfile);

            s = Convert.ToInt32(doc.SelectSingleNode("config/" + key).InnerText);

            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <param name="value">The value to write.</param>
        /// <remarks>Under construction.</remarks>
        private static void nwSetString(string key, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(cfgfile);
            doc.SelectSingleNode("config/" + key).InnerText = value;
            doc.Save(cfgfile);
        }

        /// <summary>
        /// Returns a string to be used as part of a date time format.
        /// </summary>
        /// <param name="nodate">If true, don't return a date, false otherwise.</param>
        /// <returns></returns>
        private static string nwParseFormat(bool nodate)
        {
            string t;

            t = nwGrabString("timeformat");

            if (nodate == false)
                return "dd/MM/yyyy " + t;
            else
                return t;
        }


        /// <summary>
        /// This is what we use to grab the logs from the server and download them into a readable format.
        /// </summary>
        /// <returns></returns>
        static async Task Run()
        {
            var Bot = new Api("170729696:AAGYA8FPN4RkquTRrY-teqrn-J9YdnZX22k"); // Api key, please generate your own, don't use mine.

            var me = await Bot.GetMe();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("-----------------------------------------------------------------");
            Console.WriteLine("Hello my name is {0}, I'm a bot for Perthfurs SFW Telegram.", me.Username);
            Console.WriteLine("-----------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Green;


            var offset = 0; // status offset

            while (true)
            {
                var updates = await Bot.GetUpdates(offset); // get updates

                //Bot.InlineQueryReceived += new EventHandler<InlineQueryEventArgs>(nwHandleShit);

                // For each update in the list
                foreach (var update in updates)
                {
                    // remove unsightly characters from usernames.
                    string s_cleanname = update.Message.From.FirstName;
                    s_cleanname = Regex.Replace(s_cleanname, @"[^\u0000-\u007F]", string.Empty);

                    if (update.Message.Type == MessageType.TextMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        //If we have set the bot to be able to respond to our basic commands
                        if (nwGrabString("botresponds") == "true" || nwGrabString("debugmode") == "true")
                        {
                            // TODO: MOVE ALL COMMANDS TO pfsCommandBot
                            nwProcessSlashCommands(Bot, update, me, m);
                        }
                        else
                        {
                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has attempted to use a command, but they were disabled.");
                        }

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            if (nwGrabString("debugmode") == "true")
                                Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_cleanname + "> " + update.Message.Text);
                            else
                                Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_cleanname + "> " + update.Message.Text);
                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_cleanname + "> " + update.Message.Text);
                        }
                    }

                    // UNKNOWN MESSAGES.
                    // TODO: Work out what to actually flocking do with them.
                    if (update.Message.Type == MessageType.UnknownMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");

                            //sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");
                        }
                    }

                    // SERVICE MESSAGES
                    if (update.Message.Type == MessageType.ServiceMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: A user (" + s_cleanname + ") has joined or left the group.");

                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has joined or left the group!");
                        }
                    }

                    // Venue messages. Added in API v2.0
                    // TODO: IMPLEMENT PROPERLY
                    if (update.Message.Type == MessageType.VenueMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            if (nwGrabString("debugmode") == "true")
                                nwPrintSystemMessage("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " posted about a venue on Foursquare.");
                            else
                                nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " posted about a venue on Foursquare.");

                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted about a venue on Foursquare.");
                        }
                    }

                    if (update.Message.Type == MessageType.StickerMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            if (nwGrabString("debugmode") == "true")
                                Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted an unknown sticker.");
                            else
                                Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted an unknown sticker.");

                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted an unknown sticker.");
                        }

                        if (nwGrabString("dloadMedia") == "true")
                        {
                            var file = await Bot.GetFile(update.Message.Sticker.FileId);

                            nwPrintSystemMessage(string.Format("[" + m.ToString(nwParseFormat(true)) + "] * System: Received Sticker: {0}", file.FilePath));

                            var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                            using (var profileImageStream = File.Open(filename, FileMode.Create))
                            {
                                await file.FileStream.CopyToAsync(profileImageStream);
                            }
                        }
                        else { nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Setting 'dloadmedia' has been set to 'false', ignoring download request."); }

                    }

                    if (update.Message.Type == MessageType.VoiceMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a voice message.");
                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a voice message.");
                        }
                    }

                    if (update.Message.Type == MessageType.VideoMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a video message.");
                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a video message.");
                        }
                    }

                    if (update.Message.Type == MessageType.PhotoMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime(); // Get date/time

                        //write following to file stream.
                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            // download the caption for the image, if there is one.
                            string s = update.Message.Caption;

                            // check to see if the caption string is empty or not
                            if (s == string.Empty || s == null || s == "" || s == "/n")
                            {
                                nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted a photo with no caption.");
                                sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a photo with no caption.");
                            }
                            else
                            {
                                nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted a photo with the caption '" + s + "'.");
                                sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a photo with the caption '" + s + "'.");
                            }
                        }

                        if (nwGrabString("dloadImages") == "true")
                        {
                            var file = await Bot.GetFile(update.Message.Photo.LastOrDefault()?.FileId);

                            nwPrintSystemMessage(string.Format("[" + m.ToString(nwParseFormat(true)) + "] * System: Received Photo: {0}", file.FilePath));

                            var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                            using (var profileImageStream = File.Open(filename, FileMode.Create))
                            {
                                await file.FileStream.CopyToAsync(profileImageStream);
                            }
                        }
                        else { nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Setting 'dloadimages' has been set to 'false', ignoring download request."); }
                    }

                    if (update.Message.Type == MessageType.AudioMessage)
                    {
                        await Task.Delay(2000);

                        DateTime m = update.Message.Date.ToLocalTime();

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted an audio message.");
                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted an audio message.");
                        }
                    }

                    offset = update.Id + 1; // do not touch.
                }

                await Task.Delay(1000);
            }
        }

        private static void nwHandleShit(object sender, InlineQueryEventArgs e)
        {
            try
            {
                Console.WriteLine(e.InlineQuery.From);
                Console.WriteLine(e.InlineQuery.Id);
                Console.WriteLine(e.InlineQuery.Offset);
                Console.WriteLine(e.InlineQuery.Query);
            }
            catch(Exception ex)
            {
                nwErrorCatcher(ex);
            }
        }

        /// <summary>
        /// Process all of our slash commands
        /// </summary>
        /// <param name="bot">The bot API.</param>
        /// <param name="update">The update</param>
        /// <param name="me">The user, or bot.</param>
        /// <param name="m">The date/time component.</param>
        /// <remarks>Only designed to work if regular commands are enabled.</remarks>
        private static async void nwProcessSlashCommands(Api bot, Update update, User me, DateTime dt)
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
                                replyText = "Group admins are @Inflatophin and @AndyDingoFolf.";
                            break;
                        case "/alive":
                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Hi " + update.Message.From.FirstName + ", I am indeed alive.";
                            break;
                        case "/backup":
                            if (s_chattype == "Private")
                            {
                                if (s_username != "AndyDingoFolf")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        replyText = "You have insufficient permissions to access this command.";
                                    break;
                                }
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Starting backup...";
                                cZipBackup.Instance.CreateSample(dt.ToString(nwGrabString("dateformat")) + "_backup.zip", null, Environment.CurrentDirectory + @"\logs_tg\");
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
                            int catmax = nwGrabInt("climits/cat");

                            if (catuse == catmax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "Sorry, the /cat command has been used too many times.";
                                break;
                            }

                            nwSetString("cusage/cat", Convert.ToString(catuse++)); // increment usage

                            replyImage = "http://thecatapi.com/api/images/get?format=src&type=jpg,png";
                            break;
                        case "/dog":
                        case "/doge":
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
                                replyText = "Hi " + update.Message.From.FirstName + ", Here's a list of commands I can respond to: http://www.perthfurstats.net/node/11 Note that it hasn't been properly updated for Telegram yet.";
                            break;
                        case "/event":
                        case "/events": // TODO: Finish this command
                            XmlDocument dook = new XmlDocument();
                            dook.Load("http://www.perthfurs.net/events.xml");
                            DateTime dta = new DateTime(2016, 4, 1);
                            dta = DateTime.Now;
                            
                            XmlNodeList nodes;
                            nodes = dook.GetElementsByTagName("event");

                            for (var i1for = 0; i1for < nodes.Count; i1for++)
                            {
                                dta = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("start").InnerText);
                                stringBuilder.AppendLine(dta.ToString("ddd d/MM/yyy") + " (" + dta.ToString("h:mm tt") + "): " + nodes.Item(i1for).SelectSingleNode("title").InnerText + " [" + nodes.Item(i1for).SelectSingleNode("url").InnerText + "]"); // + " [" + pfn_events.url.ToString() + "]");
                            }
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
                            int emmax = nwGrabInt("climits/emote");

                            if (body == string.Empty || body == " ")
                            {
                                bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "*@PFStats_bot slaps @" + update.Message.From.Username + " around with a large trout!*";

                                nwSetString("cusage/emote", Convert.ToString(emuse++));
                                break;
                            }

                            string basestr1 = body;
                            string[] mysplit1 = new string[] { "", "" };
                            mysplit1 = basestr1.Split('@');

                            string ms11 = mysplit1[1];

                            if (nwCheckInReplyTimer(dt) != false && ms11 != string.Empty)
                                replyText = "*@" + update.Message.From.Username + " slaps " + ms11 + " around with a large trout!*";
                            else
                                replyText = "*@PFStats_bot slaps @" + update.Message.From.Username + " around with a large trout!*";

                            nwSetString("cusage/stats", Convert.ToString(emuse++));
                            break;
                        case "/image": // TODO: Finish this command
                            // 
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/humour":
                        case "/joke": // TODO: Fix this command
                            int jokeuse = nwGrabInt("cusage/joke");
                            int jokemax = nwGrabInt("climits/joke");

                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "I'm sorry " + update.Message.From.FirstName + ", My humor emitter array requires recharging. Please try again another time.";

                            nwSetString("cusage/joke", Convert.ToString(jokeuse++));
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
                            int rollmax = nwGrabInt("climits/roll");

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
                            int saymax = nwGrabInt("climits/say");
                            
                            if (s_username != "AndyDingoFolf")
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            if (body.Length < 2)
                            {
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                                replyText2 = body;
                            nwSetString("cusage/say", Convert.ToString(sayuse++));
                            break;
                        case "/stats": // change to /stats [week|month|year|alltime]
                            int statuse = nwGrabInt("cusage/stats");
                            int statmax = nwGrabInt("climits/stats");

                            if (body == string.Empty || body == " ")
                            {
                                bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular usage: /stats -[week|month|year|alltime|archive|commands]";

                                nwSetString("cusage/stats", Convert.ToString(statuse++));
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
                        case "/target":
                            // usage /em -[action (see list of actions)] -[@username of target]
                            // performs an action on a target
                            emuse = nwGrabInt("cusage/emote");
                            emmax = nwGrabInt("climits/emote");

                            if (body == string.Empty || body == " ")
                            {
                                break;
                            }

                            replyText = nwRandomGreeting() + ". Coming soon";

                            nwSetString("cusage/emote", Convert.ToString(emuse++));
                            break;
                        case "/me": // TODO: Finish this command
                                    // performs an action on the caller
                                    // usage /em -[action (see list of actions)]
                                    //usage
                            emuse = nwGrabInt("cusage/emote");
                            emmax = nwGrabInt("climits/emote");

                            if (body == string.Empty || body == " ")
                            {
                                break;
                            }

                            replyText = nwRandomGreeting() + ". Coming soon";

                            nwSetString("cusage/emote", Convert.ToString(emuse++));
                            break;
                        case "/user":

                            int useruse = nwGrabInt("cusage/user");
                            int usermax = nwGrabInt("climits/user");

                            string s_cleanname = update.Message.From.FirstName;
                            s_cleanname = Regex.Replace(s_cleanname, @"[^\u0000-\u007F]", string.Empty);


                            nwGetUserPermissions(update.Message.From.FirstName);
                            nwSetString("cusage/user", Convert.ToString(useruse++));


                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";
                            break;
                        case "/exchange":
                        case "/rate":
                            string exo = httpClient.DownloadString("https//www.exchangerate-api.com/AUD/USD?k=" + exchangeKey).Result;
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "1 USD = " + exo;
                            break;
                        case "/forecast":
                        case "/weather": // TODO - change to BOM api
                            if (body.Length < 2)
                            {
                                body = "Perth, Australia";
                            }

                            bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);

                            //dynamic dfor = JObject.Parse(httpClient.DownloadString("http://api.wunderground.com/api/" + wundergroundKey + "/forecast/q/" + body + ".json").Result);
                            dynamic dfor = JObject.Parse(httpClient.DownloadString("http://www.bom.gov.au/fwo/IDW60801/IDW60801.94608.json").Result);
                            if (dfor.forecast == null || dfor.forecast.txt_forecast == null)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", you have disappointed me.  \"" + body + "\" is sadly, not going to work.  Please try \"City, ST\" or \"City, Country\" next time.";
                                break;
                            }
                            for (var ifor = 0; ifor < Enumerable.Count(dfor.observations.data.sort_order) - 1; ifor++)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    stringBuilder.AppendLine(dfor.observations.data.sort_order[ifor].title.ToString() + ": " + dfor.observations.data.sort_order[ifor].fcttext_metric.ToString());
                            }

                            break;
                        case "/version":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart;
                            break;
                        case "/about":
                        case "/info":
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "PerthFurStats is the best bot" + Environment.NewLine + "Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart + Environment.NewLine + "By @AndyDingoWolf" + Environment.NewLine + "This bot uses open source software.";

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
                            sw.WriteLine("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText);
                        }
                    }
                    replyText2 += stringBuilder.ToString();
                    if (!string.IsNullOrEmpty(replyText2))
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyText2);
                        await bot.SendTextMessage(-1001032131694, replyText2);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            sw.WriteLine("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText2);
                        }
                    }
                    if (!string.IsNullOrEmpty(replyTextMarkdown))
                    {
                        nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyTextMarkdown);
                        await bot.SendTextMessage(update.Message.Chat.Id, replyTextMarkdown, false, 0, null, ParseMode.Markdown);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
                        {
                            sw.WriteLine("[" + dt.ToString(nwParseFormat(true)) + "] " + me.Username + " " + replyTextMarkdown);
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
                        catch(NullReferenceException ex)
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

        /// <summary>
        /// Spit out a randomly selected greeting.
        /// </summary>
        /// <returns>A greeting, as a string.</returns>
        private static string nwRandomGreeting()
        {
           int i =cDiceBag.Instance.d8(1);
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
        /// <returns></returns>
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

            using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory()+@"\pfsTelegramBot.log", true))
            {
                sw.WriteLine("* System: Error has occurred: " + ex.HResult + " " + ex.Message + Environment.NewLine + "* System: " + ex.StackTrace+ Environment.NewLine+ex.InnerException+Environment.NewLine + ex.Source + Environment.NewLine + ex.TargetSite);
            }
        }

        // Permission system below this line.
        // TODO : Finish this
        private static PermissionType nwGetUserPermissions(string firstName)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("nwTelegramBot.User.cfg");

            XmlNodeList xnl = xdoc.GetElementsByTagName("user");
            
            string s;

            for (var io = 0; io > xnl.Count; io++)
            {
                //if (xnl.)

                Console.WriteLine(xnl.Item(io).SelectSingleNode("name").InnerText);
                Console.WriteLine(xnl.Item(io).SelectSingleNode("username").InnerText);
                Console.WriteLine(xnl.Item(io).SelectSingleNode("group").InnerText);

                s = xnl.Item(io).SelectSingleNode("group").InnerText;

                switch (s)
                {
                    case "Admin":
                        return PermissionType.Admin;
                    case "User":
                        return PermissionType.User;
                    case "PowerUser":
                        return PermissionType.PowerUser;
                    case "Banned":
                        return PermissionType.Banned;
                    default:
                        return PermissionType.User;
                }
            }
            return PermissionType.User;
        }
        private static bool nwHasPermissions()
        {
            return false;
        }

        private static string nwGetGroupPermissions()
        {
            return "";
        }

    }

    enum PermissionType
    {
        Admin,
        User,
        PowerUser,
        Banned,
    }
}

