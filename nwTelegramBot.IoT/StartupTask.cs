/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.19
 */

using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Windows.Devices.Gpio;
using Telegram.Bot;
using System;
using System.IO;
using System.Xml;
using System.Text;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace nwTelegramBot.IoT
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private static string updfile = Directory.GetCurrentDirectory() + @"\updchk.xml";
        private static string cfgfile = Directory.GetCurrentDirectory() + @"\nwTelegramBot.cfg";

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            RunBot().Wait();

            _deferral.Complete();

        }

        /// <summary>
        /// Settings file grabber
        /// </summary>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key.</returns>
        /// <remarks>Very BETA.</remarks>
        private static string nwGrabString(string key)
        {
            using (FileStream fs = new FileStream(cfgfile, FileMode.Open))
            {
                XmlDocument doc = new XmlDocument();
                string s;

                doc.Load(fs);

                XmlNodeList nodes;
                nodes = doc.GetElementsByTagName(key);
                s = nodes[0].InnerText;

                return s;
            }
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

        private static async Task RunBot()
        {
            InitGPIO(47);

            var Bot = new Api("170729696:AAGYA8FPN4RkquTRrY-teqrn-J9YdnZX22k"); // Api key, please generate your own, don't use mine.

            var me = await Bot.GetMe();

            var offset = 0;

            while (true)
            {
                var updates = await Bot.GetUpdates(offset);

                foreach (var update in updates)
                {
                    switch (update.Type)
                    {
                        case UpdateType.MessageUpdate:
                            var message = update.Message;

                            switch (message.Type)
                            {
                                case MessageType.TextMessage:
                                    DateTime m = update.Message.Date.ToLocalTime();

                                    // If we have set the bot to be able to respond to our basic commands
                                    if (nwGrabString("botresponds") == "true" || nwGrabString("debugmode") == "true")
                                    {
                                        nwProcessSlashCommands(Bot, update, me, m);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    string filename = @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + update.Message.Text);
                                    }

                                    break;
                                case MessageType.ServiceMessage:
                                    DateTime m2 = update.Message.Date.ToLocalTime();

                                    string fn2 = @"\logs_tg\" + nwGrabString("filename") + "." + m2.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs2 = new FileStream(fn2, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs2))
                                    {
                                        sw.WriteLine("[" + m2.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has joined the group!");
                                    }

                                    break;
                                case MessageType.StickerMessage:
                                    DateTime m3 = update.Message.Date.ToLocalTime();

                                    string fn3 = @"\logs_tg\" + nwGrabString("filename") + "." + m3.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs3 = new FileStream(fn3, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs3))
                                    {
                                        sw.WriteLine("[" + m3.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted an unknown sticker.");
                                    }

                                    break;
                                case MessageType.PhotoMessage:
                                    DateTime m4 = update.Message.Date.ToLocalTime();

                                    string fn4 = @"\logs_tg\" + nwGrabString("filename") + "." + m4.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs4 = new FileStream(fn4, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs4))
                                    {
                                        // download the caption for the image, if there is one.
                                        string s = update.Message.Caption;

                                        // check to see if the caption string is empty or not
                                        if (s == string.Empty || s == null || s == "" || s == "/n")
                                            sw.WriteLine("[" + m4.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a photo with no caption.");
                                        else
                                            sw.WriteLine("[" + m4.ToString(nwParseFormat(true)) + "] " + "* " + update.Message.From.FirstName + " has posted a photo with the caption '" + s + "'.");
                                    }
                                    break;
                                case MessageType.VoiceMessage:
                                    DateTime m5 = update.Message.Date.ToLocalTime();

                                    string fn5 = @"\logs_tg\" + nwGrabString("filename") + "." + m5.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs5 = new FileStream(fn5, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs5))
                                    {
                                        sw.WriteLine("[" + m5.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a voice message.");
                                    }
                                    break;
                                case MessageType.VideoMessage:
                                    DateTime m6 = update.Message.Date.ToLocalTime();

                                    string fn6 = @"\logs_tg\" + nwGrabString("filename") + "." + m6.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs6 = new FileStream(fn6, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs6))
                                    {
                                        sw.WriteLine("[" + m6.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted a video message.");
                                    }
                                    break;
                                case MessageType.AudioMessage:
                                    DateTime m7 = update.Message.Date.ToLocalTime();

                                    string fn7 = @"\logs_tg\" + nwGrabString("filename") + "." + m7.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs7 = new FileStream(fn7, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs7))
                                    {
                                        sw.WriteLine("[" + m7.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted an audio message.");
                                    }
                                    break;
                                case MessageType.UnknownMessage:
                                    DateTime m8 = update.Message.Date.ToLocalTime();

                                    string fn8 = @"\logs_tg\" + nwGrabString("filename") + "." + m8.ToString(nwGrabString("dateformat")) + ".log";
                                    FileStream fs8 = new FileStream(fn8, FileMode.Append, FileAccess.ReadWrite);

                                    using (StreamWriter sw = new StreamWriter(fs8))
                                    {
                                        sw.WriteLine("[" + m8.ToString(nwParseFormat(true)) + "] * " + update.Message.From.FirstName + " has posted an unknown message.");
                                    }
                                    break;
                            }
                            break;
                    }

                    offset = update.Id + 1;
                }
            }
        }

        /// <summary>
        /// Process our slash commands
        /// </summary>
        /// <param name="bot">The bot API.</param>
        /// <param name="update">The update</param>
        /// <param name="me">The user, or bot.</param>
        /// <remarks> May have issues depending on the command.</remarks>
        private static async void nwProcessSlashCommands(Api bot, Update update, User me, DateTime dt)
        {
            // read configuration
            var wundergroundKey = nwGrabString("weatherapi");
            var exchangeKey = nwGrabString("exchangeapi");

            // Process request
            try
            {
                //var httpClient = new ProHttpClient();
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
                    //Console.WriteLine(update.Message.Chat.Id + " < " + update.Message.From.Username + " - " + text);

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
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "Group admins are @Inflatophin and @AndyDingoFolf.";
                            break;
                        case "/alive":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "Hi " + update.Message.From.FirstName + ", I am indeed alive.";
                            break;
                        case "/backup":
                            if (update.Message.From.FirstName != "Andy" || update.Message.From.Username != "AndyDingoFolf")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "This function is not available in the Windows IoT version of the bot :(";
                            break;
                        case "/bot":
                            if (update.Message.From.FirstName != "Andy" || update.Message.From.Username != "AndyDingoFolf")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            replyText = "This command is not yet implemented.";
                            break;
                        case "/cat":
                            int catuse = nwGrabInt("cusage/cat");
                            int catmax = nwGrabInt("climits/cat");

                            if (catuse == catmax)
                            {
                                replyText = "Sorry, the /cat command has been used too many times.";
                                break;
                            }

                            nwSetString("cusage/cat", Convert.ToString(catuse++)); // increment usage

                            replyImage = "http://thecatapi.com/api/images/get?format=src&type=jpg,png";
                            break;
                        case "/die":
                            if (update.Message.From.FirstName != "Andy" || update.Message.From.Username != "AndyDingoFolf")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            replyText = "Goodbye.";
                            break;
                        case "/killcmds":
                            if (update.Message.From.FirstName != "Andy" ||
                                update.Message.From.Username != "AndyDingoFolf" ||
                                update.Message.From.FirstName != "Cyrin" ||
                                update.Message.From.Username != "Inflatophin")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }

                            replyText = "This command is not yet implemented.";
                            break;
                        case "/count":
                            if (update.Message.From.FirstName != "Andy" ||
                                update.Message.From.Username != "AndyDingoFolf" ||
                                update.Message.From.FirstName != "Cyrin" ||
                                update.Message.From.Username != "Inflatophin")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            replyText = "This command is not yet implemented.";
                            break;
                        case "/help":
                        case "/commands":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "Hi " + update.Message.From.FirstName + ", Here's a list of commands I can respond to: http://www.perthfurstats.net/node/11 Note that it hasn't been properly updated for Telegram yet.";
                            break;
                        case "/debug":
                            if (update.Message.From.FirstName != "Andy" ||
                                update.Message.From.Username != "AndyDingoFolf" ||
                                update.Message.From.FirstName != "Cyrin" ||
                                update.Message.From.Username != "Inflatophin")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }

                            if (nwGrabString("debugMode") == "false")
                            {
                                nwSetString("debugMode", "true");
                            }
                            else
                            {
                                nwSetString("debugMode", "false");
                            }

                            break;
                        case "/echo":
                            if (update.Message.From.FirstName != "Andy" ||
                                update.Message.From.Username != "AndyDingoFolf" ||
                                update.Message.From.FirstName != "Cyrin" ||
                                update.Message.From.Username != "Inflatophin")
                            {
                                replyText = "You have insufficient permissions to access this command.";
                                break;
                            }
                            if (body.Length < 2)
                            {
                                body = "Echo command needs more than 2 characters.";
                            }

                            replyText = body;
                            break;
                        case "/jelly":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "This function is not available in the Windows IoT version of the bot :(";
                            break;
                        case "/joke":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "This function is not available in the Windows IoT version of the bot :(";
                            break;
                        case "/link":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "Chat link: https://telegram.me/joinchat/ByYWcALujRjo8iSlWvbYIw";
                            break;
                        case "/oo":
                        case "/optout":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following form to opt-out from stats collection. Bare in mind that your request might not be implemented till the next stats run, as it requires manual intervention. URL: http://www.perthfurstats.net/node/10";
                            break;
                        case "/roll":
                            int rolluse = nwGrabInt("cusage/roll");
                            int rollmax = nwGrabInt("climits/roll");

                            if (body == string.Empty || body == " ")
                            {
                                replyText = "Usage: /roll -d[number of sides] -a[amount of dice]";
                                break;
                            }

                            string basestr = body;
                            string[] mysplit = new string[] { "", "", "" };
                            mysplit = basestr.Split('-');

                            string ms1 = mysplit[1].Remove(0, 1);
                            string ms2 = mysplit[2].Remove(0, 1);

                            int i, j;
                            i = Convert.ToInt32(ms1);
                            j = Convert.ToInt32(ms2);

                            if (j <= 5)
                            {
                                string test1 = cDiceBag.Instance.Roll(j, i);
                                replyText = "You have rolled: " + Environment.NewLine + test1;
                                nwSetString("cusage/roll", Convert.ToString(rolluse++));
                            }
                            else
                                break;

                            break;
                        case "/rules":
                            replyText = "Group rules: " + Environment.NewLine + "All content (chat, images, stickers) must be SFW at all hours of the day" + Environment.NewLine + "No flooding / spamming" + Environment.NewLine + "Be nice to each other";
                            break;
                        case "/say":
                            int sayuse = nwGrabInt("cusage/say");
                            int saymax = nwGrabInt("climits/say");


                            if (update.Message.From.FirstName != "Andy" || update.Message.From.Username != "AndyDingoFolf")
                            {
                                replyText = "You have insufficient privledges to access this command.";
                                break;
                            }
                            if (body.Length < 2)
                            {
                                body = "test message";
                            }

                            replyText2 = body;
                            nwSetString("cusage/say", Convert.ToString(sayuse++));
                            break;
                        case "/stats": // change to /stats [week|month|year|alltime]
                            int statuse = nwGrabInt("cusage/stats");
                            int statmax = nwGrabInt("climits/stats");

                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "Hi " + update.Message.From.FirstName + ", Please use the following URL to view stats: http://www.perthfurstats.net/node/stats/thisweek/perthfurs.html";

                            nwSetString("cusage/stats", Convert.ToString(1));
                            break;
                        case "/user":
                            replyText = "This command is not yet implemented.";
                            nwGetUserPermissions(update.Message.From.FirstName);
                            nwSetString("cusage/user", Convert.ToString(1));
                            break;
                        case "/exchange":
                        case "/rate":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "This function is not available in the Windows IoT version of the bot :(";
                            break;
                        case "/forecast":
                        case "/weather":
                            await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                            replyText = "This function is not available in the Windows IoT version of the bot :(";

                            break;
                        case "/version":
                            replyText = "Release 22";
                            break;
                        case "/about":
                        case "/info":
                            replyText = "PerthFurStats is the best bot" + Environment.NewLine + "Release 22" + Environment.NewLine + "By @AndyDingoWolf" + Environment.NewLine + "This bot uses open source software.";

                            nwSetString("cusage/about", Convert.ToString(1));
                            break;
                        case "/wiki":
                            break;
                    }

                    // Output
                    replyText += stringBuilder.ToString();
                    if (!string.IsNullOrEmpty(replyText))
                    {
                        await bot.SendTextMessage(update.Message.Chat.Id, replyText);

                        string filename = @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log";
                        FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite);

                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[" + dt.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + replyText);
                        }
                    }
                    replyText2 += stringBuilder.ToString();
                    if (!string.IsNullOrEmpty(replyText2))
                    {
                        await bot.SendTextMessage(-1001032131694, replyText2);

                        string filename = @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log";
                        FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite);

                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[" + dt.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + replyText2);
                        }
                    }
                    if (!string.IsNullOrEmpty(replyTextMarkdown))
                    {
                        await bot.SendTextMessage(update.Message.Chat.Id, replyTextMarkdown, false, 0, null, ParseMode.Markdown);

                        string filename = @"\logs_tg\" + nwGrabString("filename") + "." + dt.ToString(nwGrabString("dateformat")) + ".log";
                        FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite);

                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("[" + dt.ToString(nwParseFormat(true)) + "] " + "<" + update.Message.From.FirstName + "> " + replyTextMarkdown);
                        }
                    }

                    //if (!string.IsNullOrEmpty(replyImage) && replyImage.Length > 5)
                    //{
                    //    await bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                    //    try
                    //    {
                    //        var stream = httpClient.DownloadData(replyImage).Result;
                    //        var extension = ".jpg";
                    //        if (replyImage.Contains(".gif") || replyImage.Contains("image/gif"))
                    //        {
                    //            extension = ".gif";
                    //        }
                    //        else if (replyImage.Contains(".png") || replyImage.Contains("image/png"))
                    //        {
                    //            extension = ".png";
                    //        }
                    //        else if (replyImage.Contains(".tif"))
                    //        {
                    //            extension = ".tif";
                    //        }
                    //        else if (replyImage.Contains(".bmp"))
                    //        {
                    //            extension = ".bmp";
                    //        }
                    //        var photo = new FileToSend("Photo" + extension, stream);
                    //        await bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    //        if (extension == ".gif")
                    //        {
                    //            await bot.SendDocument(update.Message.Chat.Id, photo);
                    //        }
                    //        else
                    //        {
                    //            await bot.SendPhoto(update.Message.Chat.Id, photo, replyImageCaption == string.Empty ? replyImage : replyImageCaption);
                    //        }
                    //    }
                    //    catch (System.Net.Http.HttpRequestException ex)
                    //    {
                    //        await bot.SendTextMessage(update.Message.Chat.Id, replyImage);
                    //    }
                    //    catch (System.Net.WebException ex)
                    //    {
                    //        await bot.SendTextMessage(update.Message.Chat.Id, replyImage);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        await bot.SendTextMessage(update.Message.Chat.Id, replyImage);
                    //    }
                    //}
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

        private static void nwErrorCatcher(Exception ex)
        {
            // NOT YET IMPLEMENTED
        }

        private static void nwGetUserPermissions(string firstName)
        {
            // NOT YET IMPLEMENTED
        }

        private static int nwGrabInt(string key)
        {
            using (FileStream fs = new FileStream(cfgfile, FileMode.Open))
            {
                XmlDocument doc = new XmlDocument();
                int s;

                doc.Load(fs);

                XmlNodeList nodes;
                nodes = doc.GetElementsByTagName(key);
                s = Convert.ToInt32(nodes[0].InnerText);

                return s;
            }
        }

        private static void nwSetString(string v1, string v2)
        {
            // NOT YET IMPLEMENTED
        }

        private static GpioPin LED;
        private static bool LEDOn;

        private static void InitGPIO(int pinNumber)
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null) return;

            LED = gpio.OpenPin(pinNumber);

            LED.Write(GpioPinValue.Low);
            LED.SetDriveMode(GpioPinDriveMode.Output);
        }

        private static void ToggleLED()
        {
            LEDOn = !LEDOn;
            LED?.Write(LEDOn ? GpioPinValue.High : GpioPinValue.Low);
        }
    }
}
