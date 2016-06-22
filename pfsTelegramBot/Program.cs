/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.62
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

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
                //nwErrorCatcher(ex);
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

                            Message message = update.Message;
                            DateTime m = update.Message.Date.ToLocalTime();

                            //remove unsightly characters from first names.
                            string ss = update.Message.From.FirstName;
                            ss = Regex.Replace(ss, @"[^\u0000-\u007F]", string.Empty);
                            //ss.Trim(' ');

                            // Do stuff if we are a text message
                            switch (message.Type)
                            {

                                case MessageType.ContactMessage:

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has shared the contact information of " + update.Message.Contact.FirstName);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has shared the contact information of " + update.Message.Contact.FirstName);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has shared the contact information of " + update.Message.Contact.FirstName);
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Contact information sharing of " + update.Message.Contact.FirstName);
                                        }
                                    }

                                        break;

                                case MessageType.DocumentMessage:

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has shared a document of type: " + update.Message.Document.MimeType);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has shared a document of type: " + update.Message.Document.MimeType);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has shared a document of type: " + update.Message.Document.MimeType);
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Document sharing of type " + update.Message.Document.MimeType);
                                        }
                                    }

                                    break;

                                case MessageType.TextMessage:

                                    //If we have set the bot to be able to respond to our basic commands
                                    if (nwGrabString("botresponds") == "true" && update.Message.Text.StartsWith("/") == true)
                                    {
                                        // TODO: MOVE ALL COMMANDS TO pfsCommandBot
                                        nwProcessSlashCommands(Bot, update, me, m);
                                    }
                                    else
                                    {
                                        if (nwGrabString("debugMode") == "true")
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has attempted to use a command, but they were disabled.");
                                    }

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + update.Message.Text);
                                        }
                                    }

                                    break;

                                case MessageType.UnknownMessage: // UNKNOWN MESSAGES.
                                    // TODO: Work out what to actually flocking do with them.

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * System: Unknown, please report");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Unknown, please report");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* System: Unknown, please report");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Unknown message");
                                        }
                                    }

                                    break;

                                case MessageType.ServiceMessage: // Service messages (user leaves or joins)

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        nwPrintSystemMessage("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* System: A user (" + update.Message.From.FirstName + ") has joined or left the group.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has joined or left the group!");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "System message");
                                        }
                                    }

                                    break;

                                case MessageType.LocationMessage: // Venue messages. Added in API v2.0
                                                               // TODO: IMPLEMENT PROPERLY

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " posted about a location.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " posted about a location.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted about a location.");
                                    }

                                    break;

                                case MessageType.VenueMessage: // Venue messages. Added in API v2.0
                                // TODO: IMPLEMENT PROPERLY

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            nwPrintSystemMessage("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " posted about a venue on Foursquare.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " posted about a venue on Foursquare.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted about a venue on Foursquare.");
                                    }

                                    break;
                                
                                case MessageType.StickerMessage: // Do stuff if we are a sticker message

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        // download the emoji for the image, if there is one. Added in May API update.
                                        string s = update.Message.Sticker.Emoji;

                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a sticker that represents the " + s + " emoticon.");
                                        else
                                            Console.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a sticker that represents the " + s + " emoticon.");

                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a sticker that represents the " + s + " emoticon.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            // download the emoji for the image, if there is one. Added in May API update.
                                            string s = update.Message.Sticker.Emoji;

                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Sticker message (" + s + ")");
                                        }
                                    }

                                    break;
                                
                                case MessageType.VoiceMessage: // Do stuff if we are a voice message

                                    m = update.Message.Date.ToLocalTime();

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a voice message.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a voice message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a voice message.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Voice message");
                                        }
                                    }

                                    break;

                                case MessageType.VideoMessage:

                                    m = update.Message.Date.ToLocalTime();

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a video message.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a video message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a video message.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Video message");
                                        }
                                    }

                                    break;
                                    
                                case MessageType.PhotoMessage: // Do stuff if we are a photo message

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

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Photo message");
                                        }
                                    }

                                    break;
                                    
                                case MessageType.AudioMessage: // Do stuff if we are an audio message

                                    m = update.Message.Date.ToLocalTime();

                                    using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log", true))
                                    {
                                        if (nwGrabString("debugmode") == "true")
                                            Console.WriteLine("[" + update.Id + "] [" + m.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted an audio message.");
                                        else
                                            nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted an audio message.");
                                        await sw.WriteLineAsync("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted an audio message.");
                                    }

                                    if (nwGrabString("logformat") == "csv" || nwGrabString("debugmode") == "true")
                                    {
                                        using (StreamWriter sw1 = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + ".csv", true))
                                        {
                                            await sw1.WriteLineAsync(update.Id + "," + m.ToString("dd/MM/yyyy,HH:mm") + "," + update.Message.From.FirstName + "," + "Audio message");
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
                            break;
                        default:
                            //nwPrintSystemMessage("[" + m.ToString(nwParseFormat(true)) + "] * System: Find out how we got to this point.");
                            break;
                    }
                    
                }

            }

        }

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
        private static async Task nwProcessSlashCommands(TelegramBotClient bot, Update update, User me, DateTime dt)
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
                var s_replyToUser = string.Empty;
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
                        case "/ball":
                        case "/8ball":

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                if (body == string.Empty || body == " " || body == "@" || body.Contains("?") == false || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    s_replyToUser = "You haven't given me a question to answer." + Environment.NewLine + "Usage: /8ball question to ask?";

                                    break;
                                }

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = nwRandom8BallResponse();
                                break;
                            }

                            break;

                        case "/mods":
                        case "/admin":
                        case "/admins":

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

                        case "/alive":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Hi " + update.Message.From.FirstName + ", I am indeed alive.";
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

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

                        case "/cat":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                            int n_catuse = nwGrabGlobalUsage("cat");
                            int n_cat_uuse = nwGrabUserUsage(s_username, "cat");
                            int n_cat_gmax = nwGrabGlobalMax("cat");
                            int n_cat_umax = nwGrabUserMax("cat");

                            if (n_catuse == n_cat_gmax || n_catuse == n_cat_umax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Sorry, the /cat command has been used too many times.";
                                break;
                            }

                            // if it is okay to reply, do so.
                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                replyImage = "http://thecatapi.com/api/images/get?format=src&type=jpg,png";
                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            nwSetGlobalUsage("cat", n_catuse++); // set global usage incrementally
                            nwSetUserUsage(s_username, "cat", n_cat_uuse++); // set this users usage incrementally
                            
                            break;

                        case "/die":
                        case "/kill":

                            if (s_chattype == "Private")
                            {
                                if (s_username != "AndyDingoFolf")
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

                        case "/e621":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (s_chattype == "Private")
                            {

                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "You have insufficient permissions to access this command.";
                                break;

                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Not happening, outside of private messages that is.";
                            }

                            break;

                        case "/dook":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Dook!";
                            break;

                        case "/count":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (s_username != "AndyDingoFolf" ||
                                s_username != "Inflatophin")
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
                                    meow = await bot.GetChatMembersCountAsync(-1001032131694);
                                    s_replyToUser = "There are currently " + meow + " people in chat.";

                                }
                                else
                                {

                                    Console.WriteLine(" The " + command + " failed as it took too long to process.");

                                }

                            }

                            break;

                        case "/help":
                        case "/commands":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                replyTextEvent = "Hi " + update.Message.From.FirstName + ", Here's a list of commands I can respond to: http://www.perthfurstats.net/node/11 Note that it is currently a work in progress.";
                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            break;

                        case "/event":
                        case "/events": // TODO: Finish this command

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                XmlDocument dook = new XmlDocument();
                                dook.Load(Directory.GetCurrentDirectory() + @"/data/events.xml");
                                DateTime dta = new DateTime(2016, 4, 1);
                                dta = DateTime.Now;

                                // Get our nodes
                                XmlNodeList nodes;
                                nodes = dook.GetElementsByTagName("event");

                                // Create a new string builder
                                StringBuilder eventString = new StringBuilder();
                                eventString.AppendLine("Here is a list of upcoming events. Times are in GMT +8:00.");

                                // Iterate through available events
                                for (var i1for = 0; i1for < nodes.Count; i1for++)
                                {
                                    dta = Convert.ToDateTime(nodes.Item(i1for).SelectSingleNode("start").InnerText);
                                    eventString.AppendLine(dta.ToString("ddd d/MM/yyy") + " (" + dta.ToString("h:mm tt") + "): " + nodes.Item(i1for).SelectSingleNode("title").InnerText + " [" + nodes.Item(i1for).SelectSingleNode("url").InnerText + "]"); // + " [" + pfn_events.url.ToString() + "]");
                                }

                                replyTextEvent = eventString.ToString();

                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
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

                        case "/edit":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (s_username != "AndyDingoFolf")
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

                        case "/echo":
                            
                            if (s_chattype == "Private")
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

                        case "/jelly": // TODO: Finish this command

                            // usage /jelly [yes|no] leave blank and bot will repeat query.
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "This command is not yet implemented.";

                            break;

                        case "/slap": // TODO: Finish this command

                            // usage /slap [nickname] bot will slap the person matching the nickname.
                            // will return "yournickname slaps targetnickname around with [randomobject]

                            int n_emouse = nwGrabGlobalUsage("emote");
                            int n_emo_uuse = nwGrabUserUsage(s_username, "emote");
                            int n_emo_gmax = nwGrabGlobalMax("emote");
                            int n_emo_umax = nwGrabUserMax("emote");

                            if (n_emouse == n_emo_gmax || n_emo_uuse == n_emo_umax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Sorry, the /slap command has been used too many times.";

                                break;

                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                if (body == string.Empty || body == " " || body == "@" || body == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                    replyText = "*@PFStats_bot slaps @" + s_username + " around a bit with a large trout!*";

                                    nwSetGlobalUsage("emote", n_emouse++); // set global usage incrementally
                                    nwSetUserUsage(s_username, "emote", n_emouse++); // set this users usage incrementally

                                    break;

                                }

                                // Sanitise target string.
                                string s_target = body;

                                //break on empty strings
                                if (s_target == string.Empty || s_target == " " || s_target == null)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);
                                    s_replyToUser = "No target was selected. Usage: /slap @username";
                                    break;
                                }

                                if (s_username == string.Empty)
                                {
                                    bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);
                                    s_replyToUser = "I'm sorry, I can't let you do that Dave";

                                    nwSetGlobalUsage("emote", n_emouse++); // Write new value. // set global usage incrementally
                                    nwSetUserUsage(s_username, "emote", n_emouse++); // set this users usage incrementally

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

                            nwSetGlobalUsage("emote", n_emouse++); // Write new value. // set global usage incrementally
                            nwSetUserUsage(s_username, "emote", n_emo_uuse++); // set this users usage incrementally

                            break;

                        case "/sfw":
                        case "/safeforwork":

                            int n_sfwuse = nwGrabGlobalUsage("sfw");
                            int n_sfwmax1 = nwGrabGlobalMax("sfw");
                            int n_sfwmax2 = nwGrabUserMax("sfw");

                            if (n_sfwuse == n_sfwmax1 || n_sfwuse == n_sfwmax2)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Sorry, the /sfw command has been used too many times.";
                                break;
                            }

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadVideo);
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
                                    await bot.SendVideoAsync(update.Message.Chat.Id, fts, 0, "", false, update.Message.MessageId);
                                }
                                break;
                            }

                            nwSetGlobalUsage("sfw", n_sfwuse++); // Write new value. // set global usage incrementally
                            nwSetUserUsage(s_username, "sfw", n_sfwuse++); // set this users usage incrementally
                            
                            break;

                        case "/image": // TODO: Finish this command

                            int n_imguse = nwGrabGlobalUsage("img"); // GLOBAL USAGE
                            int n_img_uuse = nwGrabUserUsage(s_username, "img");
                            int n_img_gmax = nwGrabGlobalMax("img"); // GLOBAL MAXIMUM
                            int n_img_umax = nwGrabUserMax("img"); // USER MAXIMUM

                            if (n_imguse == n_img_gmax || n_img_uuse == n_img_umax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Sorry, the /image command has been used too many times.";

                                break;
                            }

                            if (body == string.Empty || body == " " || body == "@" || body == null)
                            {
                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                                s_replyToUser = "Usage: /image [image to look for]";
                                
                                break;
                            }

                            if (nwCheckInReplyTimer(dt) != false)
                            {

                                bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);

                                string html = GetHtmlCode(body);
                                List<string> urls = GetUrls(html);
                                var rnd = new Random();

                                int randomUrl = rnd.Next(0, urls.Count - 1);

                                string luckyUrl = urls[randomUrl];

                                if (luckyUrl.Contains(" ") == true)
                                    luckyUrl.Replace(" ", "%20");

                                replyImage = luckyUrl;

                                nwSetGlobalUsage("img", n_imguse++); // set global usage incrementally
                                nwSetUserUsage(s_username, "img", n_img_uuse++); // set this users usage incrementally

                                break;

                            }

                            break;

                        case "/humour":
                        case "/joke": // TODO: Fix this command

                            int n_jokeuse = nwGrabGlobalUsage("joke");
                            int n_joke_uuse = nwGrabUserUsage(s_username, "joke");
                            int n_joke_gmax = nwGrabGlobalMax("joke");
                            int n_joke_umax = nwGrabUserMax("joke");

                            if (n_jokeuse == n_joke_gmax || n_joke_uuse == n_joke_umax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Sorry, the /joke command has been used too many times.";
                                break;
                            }

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                string textomatic = nwRandomJokeLine();

                                replyText = textomatic.Replace("\r\n", Environment.NewLine);
                            }
                            else
                            {
                                Console.WriteLine("The " + command + " failed as it took too long to process.");
                            }

                            nwSetGlobalUsage("joke", n_jokeuse++); // set global usage incrementally
                            nwSetUserUsage(s_username, "joke", n_joke_uuse++); // set this users usage incrementally

                            break;
                        case "/link":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Chat link: https://telegram.me/joinchat/ByYWcALujRjo8iSlWvbYIw";

                            break;

                        case "/oo":
                        case "/optout":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following form to opt-out from stats collection. Bare in mind that your request might not be implemented till the next stats run, as it requires manual intervention. URL: http://www.perthfurstats.net/node/10";

                            break;

                        case "/roll":
                        case "/diceroll":

                            string s = "";

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            // Roll our dice.
                            s = nwRollDice(s_username, dt, body);
                            replyText = s;

                            break;

                        case "/rules":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Group rules: " + Environment.NewLine + "- All content (chat, images, stickers) must be SFW at all hours of the day." + Environment.NewLine + "- No flooding or spamming of ANY kind." + Environment.NewLine + "- Be nice to each other.";
                            break;

                        case "/test":

                            if (s_chattype == "Private")
                            {
                                
                                // WAITING FOR THE NEXT TEST

                            }
                            else
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "This command can only be used in private messages.";
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
                                    s_replyToUser = "You have insufficient permissions to access this command.";

                                break;
                            }
                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command is not yet implemented.";

                            break;

                        case "/eeyup":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "This command is not yet implemented.";

                            break;

                        case "/say":

                            int n_sayuse = nwGrabGlobalUsage("say");
                            int n_say_gmax = nwGrabGlobalMax("say");

                            if (n_sayuse == n_say_gmax)
                            {
                                if (nwCheckInReplyTimer(dt) != false)
                                    s_replyToUser = "Sorry, the /say command has been used too many times.";
                                break;
                            }

                            if (s_chattype == "Private")
                            {
                                if (s_username != "AndyDingoFolf")
                                {
                                    if (nwCheckInReplyTimer(dt) != false)
                                        s_replyToUser = "You have insufficient permissions to access this command.";
                                    break;
                                }

                                if (nwCheckInReplyTimer(dt) != false)
                                    replyText2 = body;
                            }

                            if (body.Length < 2)
                            {
                                break;
                            }

                            nwSetGlobalUsage("say", n_sayuse++); // Write new value. // set global usage incrementally

                            break;

                        case "/stats": // change to /stats [week|month|year|alltime]

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
                                    replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular usage: /stats -[week|month|year|alltime|archive|commands]";

                                nwSetGlobalUsage("stats", n_statuse++); // set global usage incrementally
                                nwSetUserUsage(s_username, "stats", n_statuse++); // set this users usage incrementally

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
                                        int tuse = nwGrabGlobalUsage("total");
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Since inception on Feb 15 2016, this bot has processed " + Convert.ToString(tuse) + " total commands.";
                                        break;
                                    default:
                                        if (nwCheckInReplyTimer(dt) != false)
                                            replyText = nwRandomGreeting() + " " + update.Message.From.FirstName + ", Please use the following URL to view this last weeks stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html" + Environment.NewLine + "Note: Regular usage: /stats -f[week|month|year|alltime|archive]";
                                        break;
                                }

                                nwSetGlobalUsage("stats", n_statuse++); // set global usage incrementally
                                nwSetUserUsage(s_username, "stats", n_statuse++); // set this users usage incrementally

                                break;

                            }
                        case "/start":
                        case "/greet":
                        case "/greeting":

                            if (nwCheckInReplyTimer(dt) != false)
                            {
                                s_replyToUser = nwRandomGreeting() + " " + update.Message.From.FirstName + "!";
                                break;
                            }

                            break;
                        case "/meme":




                            break;
                        //case "/em": // TODO: Finish this command
                        //    // usage /em -[action (see list of actions)] -[@username of target]
                        //    // performs an action on a target
                        //    emuse = nwGrabInt("cusage/emote");
                        //    int n_emmax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/emote");
                        //    int n_emmax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/emote");

                        //    if (emuse == n_emmax1 || emuse == n_emmax2)
                        //    {
                        //        if (nwCheckInReplyTimer(dt) != false)
                        //            replyText = "Sorry, the /em command has been used too many times.";
                        //        break;
                        //    }

                        //    if (nwCheckInReplyTimer(dt) != false)
                        //    {
                        //        bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                        //        if (body == string.Empty || body == " ")
                        //        {
                        //            break;
                        //        }

                        //        if (s_chattype == "Private")
                        //        {



                        //        }


                        //        replyText = nwRandomGreeting() + ". This command is coming soon.";
                        //    }

                        //    nwSetString("cusage/emote", Convert.ToString(emuse++));
                        //    nwSetUserString(update.Message.From.FirstName + "/cmd_counts/emote", Convert.ToString(emuse++));
                        //    break;
                        //case "/action":
                        //case "/me": // TODO: Finish this command
                        //            // performs an action on the caller
                        //            // usage /em -[action (see list of actions)]
                        //            //usage
                        //    emuse = nwGrabInt("cusage/emote");
                        //    int n_memax1 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits_user/emote");
                        //    int n_memax2 = cSettings.Instance.nwGrabInt(s_gcmd_cfgfile, "climits/emote");

                        //    if (body == string.Empty || body == " ")
                        //    {
                        //        break;
                        //    }

                        //    replyText = nwRandomGreeting() + ". This command is coming soon. *pokes @TsarTheErmine *";

                        //    nwSetString("cusage/emote", Convert.ToString(emuse++));
                        //    nwSetUserString(update.Message.From.FirstName + "/cmd_counts/emote", Convert.ToString(emuse++));

                        //    break;

                        case "/exchange":
                        case "/rate":

                            bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

                            replyText = nwRandomGreeting() + ". This command is coming soon.";

                            string exo = httpClient.DownloadString("https://www.exchangerate-api.com/AUD/USD?k=" + exchangeKey).Result;
                            if (nwCheckInReplyTimer(dt) != false)
                                replyText = "1 USD = " + exo + " AUD";

                            break;

                        case "/forecast":
                        case "/weather": // TODO - change to BOM api

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
                            weatherString.AppendLine("Rain since 9am; " + d_weather.observations.data[0].rain_trace.ToString());
                            weatherString.AppendLine("Wind speed; " + d_weather.observations.data[0].wind_spd_kmh.ToString() + "kph , Gusting up to " + d_weather.observations.data[0].gust_kmh.ToString()+ "kph");
                            weatherString.AppendLine("Wind direction; " + d_weather.observations.data[0].wind_dir.ToString() + "");
                            weatherString.AppendLine("This data is refreshed every 10 mins.");
                            
                            replyText = weatherString.ToString();

                            break;

                        case "/wiki":
                            break;
                        case "/user": // TODO : Finish this command
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

                            nwSetGlobalUsage("user", n_useruse++); // set global usage incrementally
                            nwSetUserUsage(s_username, "user", n_useruse++); // set this users usage incrementally

                            break;

                        case "/version":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "Version " + cExtensions.nwGetFileVersionInfo.FileMajorPart + "." + cExtensions.nwGetFileVersionInfo.FileMinorPart + ", Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart;

                            break;

                        case "/about":
                        case "/info":

                            int n_nfouse = nwGrabGlobalUsage("about");

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "PerthFurStats is the best bot" + Environment.NewLine + "Version " + cExtensions.nwGetFileVersionInfo.FileMajorPart + "." + cExtensions.nwGetFileVersionInfo.FileMinorPart + ", Release " + cExtensions.nwGetFileVersionInfo.FilePrivatePart + Environment.NewLine + "By @AndyDingoWolf" + Environment.NewLine + "This bot uses open source software.";

                            nwSetGlobalUsage("about", n_nfouse++); // set global usage incrementally
                            nwSetUserUsage(s_username, "about", n_nfouse++); // set this users usage incrementally

                            break;

                        case "/wrist":
                        case "/wrists":

                            if (nwCheckInReplyTimer(dt) != false)
                                s_replyToUser = "(╯°□°）╯︵ ┻━┻";

                            break;

                        case "/zalgo":

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
                    int totaluse = nwGrabGlobalUsage("total");
                    totaluse++;
                    nwSetGlobalUsage("total", totaluse++);

                    // Output
                    replyText += stringBuilder.ToString();

                    if (!string.IsNullOrEmpty(replyText))
                    {

                        if (nwGrabString("debugmode") == "true")
                            Console.WriteLine("[" + update.Message.Chat.Id + "] [" + update.Id + "] [" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + replyText);
                        else
                            nwPrintSystemMessage("[" + dt.ToString(nwParseFormat(true)) + "] <" + me.FirstName + "> " + update.Message.Chat.Id + " > " + replyText);

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, replyText);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
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

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, s_replyToUser, false,false, update.Message.MessageId);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
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

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
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

                        await bot.SendTextMessageAsync(update.Message.Chat.Id, replyTextEvent, true);

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
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

                        using (StreamWriter sw = new StreamWriter(Environment.CurrentDirectory + @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log", true))
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

                        bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

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
                //nwErrorCatcher(ex);
            }
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
                    boto.SendTextMessageAsync(n_chatid, "Usage: /edit [messageid] [the text to change]");

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
                    return "Usage: /roll [number of sides] [amount of dice]";
            }
            
            string[] mysplit = new string[] { "", "", "" };
            mysplit = body.Split(' ');

            string ms1 = mysplit[0];
            string ms2 = mysplit[1];

            int i, j;
            i = Convert.ToInt32(ms1);
            j = Convert.ToInt32(ms2);

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
                    return "Maybe, In  a few weeks, if you're lucky.";
                case 10:
                    return "If your mommy says it is OK.";
                case 11:
                    return "Have you considered going to a psychologist?";
                case 12:
                    return "If you accentuate the positive.";
                case 13:
                    return "Between a rock and a hard place.";
                case 14:
                    return "Be careful, you might get a slap in your face!";
                case 15:
                    return "Listen to your heart. If it's beating you're alive.";
                case 16:
                    return "One is the loneliest number that you will ever do.";
                case 17:
                    return "It smells like it.";
                case 18:
                    return "Some say 1+1 does not equal two...";
                case 19:
                    return "Check the glove compartment of your car.";
                case 20:
                    return "If you grease a few palms.";
                default:
                    return "Yes, now give the screen a little kiss.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string nwRandom8BallResponse2()
        {
            int i = cDiceBag.Instance.d8(1);

            switch (i)
            {
                case 1:
                    return "Outcome is as likely as my not caring about it";
                case 2:
                    return "Chances are good.... if you're a betting person";
                case 3:
                    return "Pfft, You wish.";
                case 4:
                    return "You've got to be kidding.";
                case 5:
                    return "Ask me if I care";
                case 6:
                    return "Dear god no.";
                case 7:
                    return "Not in a million years.";
                case 8:
                    return "Can't decide.";
                default:
                    return "Yes, now give the screen a little kiss.";
            }
        }


        private static string GetHtmlCode(string s_topic)
        {
            string url = "https://www.google.com/search?q=" + s_topic + "&safe=active&tbm=isch";
            string data = "";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";

            var response = (HttpWebResponse)request.GetResponse();

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return "";
                using (var sr = new StreamReader(dataStream))
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

            using (Stream dataStream = response.GetResponseStream())
            {
                if (dataStream == null)
                    return null;
                using (var sr = new BinaryReader(dataStream))
                {
                    byte[] bytes = sr.ReadBytes(100000000);

                    return bytes;
                }
            }

            return null;
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

}

