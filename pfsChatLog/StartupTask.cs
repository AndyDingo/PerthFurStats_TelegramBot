/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.42
 */

using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Windows.Storage;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace pfsChatLog
{
    public sealed class StartupTask : IBackgroundTask
    {
        // Declare Variables
        private static string logfile = Directory.GetCurrentDirectory() + @"\pfsChatLogBot.log";
        private static string updfile = Directory.GetCurrentDirectory() + @"\updchk.xml";
        private static string cfgfile = Directory.GetCurrentDirectory() + @"\pfsChatLogBot.cfg"; // Main config
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            RunBot().Wait(-1);

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

        /// <summary>
        /// Try to run the bot.
        /// </summary>
        /// <returns></returns>
        private static async Task RunBot()
        {
            InitGPIO(47);

            var Bot = new Api("203067277:AAGzj4MXygP0FIWjoC80Oog0nTOQJGKXbEI");

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

                            DateTime m = update.Message.Date.ToLocalTime();
                            string filename = @"\logs_tg\" + nwGrabString("filename") + "." + m.ToString(nwGrabString("dateformat")) + ".log";
                            FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.ReadWrite);

                            // remove unsightly characters from usernames.
                            string s_cleanname = update.Message.From.FirstName;
                            s_cleanname = Regex.Replace(s_cleanname, @"[^\u0000-\u007F]", string.Empty);

                            switch (message.Type)
                            {
                                case MessageType.TextMessage:

                                    if (message.Text == "/toggle")
                                    {
                                        ToggleLED();
                                        break;
                                    }

                                    //cStorageExtensions cse = new cStorageExtensions();
                                    await cStorageExtensions.nwSaveMessageToLocalFile(filename, "[" + m.ToString(nwParseFormat(true)) + "] " + "<" + s_cleanname + "> " + update.Message.Text);
                                    
                                    break;
                                case MessageType.StickerMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted an unknown sticker!");
                                    }
                                    break;
                                case MessageType.PhotoMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        // download the caption for the image, if there is one.
                                        string s = update.Message.Caption;

                                        // check to see if the caption string is empty or not
                                        if (s == string.Empty || s == null || s == "" || s == "/n")
                                        {
                                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a photo message with no caption.");
                                        }
                                        else
                                        {
                                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a photo message with the caption '" + s + "'.");
                                        }
                                    }
                                    break;
                                case MessageType.AudioMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted an audio file!");
                                    }
                                    break;
                                case MessageType.ServiceMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has joined the group!");
                                    }
                                    break;
                                case MessageType.VideoMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        // download the caption for the image, if there is one.
                                        string s = update.Message.Caption;

                                        // check to see if the caption string is empty or not
                                        if (s == string.Empty || s == null || s == "" || s == "/n")
                                        {
                                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a video message with no caption.");
                                        }
                                        else
                                        {
                                            sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] " + "* " + s_cleanname + " has posted a video message with the caption '" + s + "'.");
                                        }
                                    }
                                    break;
                                case MessageType.VoiceMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted a voice message!");
                                    }
                                    break;
                                case MessageType.UnknownMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted an unknown message!");
                                    }
                                    break;
                                case MessageType.LocationMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted a location message!");
                                    }
                                    break;
                                case MessageType.VenueMessage:
                                    using (StreamWriter sw = new StreamWriter(fs))
                                    {
                                        sw.WriteLine("[" + m.ToString(nwParseFormat(true)) + "] * " + s_cleanname + " has posted a venue message via Foursquare!");
                                    }
                                    break;
                            }
                            break;
                    }

                    offset = update.Id + 1;
                }
            }
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
