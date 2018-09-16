/* 
 * All contents copyright 2016-2018, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by   : Microsoft Visual Studio 2015.
 * User         : AndyDingoWolf
 * Last Updated : 13/10/2017 by AnwenEira
 * -- VERSION --
 * Version      : 1.0.0.120
 */

using System;
using System.Xml;

namespace nwTelegramBot
{
    /// <summary>
    /// Settings class.
    /// </summary>
    public class cSettings
    {
        /// <summary>
        /// Phat instance, with all the loot.
        /// </summary>
        public static cSettings Instance = new cSettings();

        /// <summary>
        /// Get max commands for a given user globally and per user.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="user_key"></param>
        /// <param name="global_key"></param>
        /// <returns></returns>
        public int[] nwGetMaxCmds(string filename, string user_key, string global_key)
        {
            XmlDocument doc = new XmlDocument();
            int[] maxarray = new int[2];
            int n_user, n_global;

            doc.Load(filename);

            if (doc.SelectSingleNode("config/" + user_key) != null)
            {

                n_user = Convert.ToInt32(doc.SelectSingleNode("config/" + user_key).InnerText);
                maxarray.SetValue(n_user, 0);

            }
            if (doc.SelectSingleNode("config/" + global_key) != null)
            {

                n_global = Convert.ToInt32(doc.SelectSingleNode("config/" + global_key).InnerText);
                maxarray.SetValue(global_key, 1);

            }
            return maxarray;
        }

        /// <summary>
        /// Settings file grabber
        /// </summary>
        /// <param name="filename">name of the file to grab setting from.</param>
        /// <param name="key">the setting key to grab.</param>
        /// <returns>The value of the settings key. On error, it will return -1.</returns>
        /// <remarks>Very BETA.</remarks>
        public int nwGrabInt(string filename, string key)
        {
            XmlDocument doc = new XmlDocument();
            int s;

            doc.Load(filename);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                s = Convert.ToInt32(doc.SelectSingleNode("config/" + key).InnerText);
                return s;

            }
            else { return -1; }
        }

        /// <summary>
        /// Set a user setting.
        /// </summary>
        /// <param name="filename">name of the file to grab setting from.</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void nwSetUserString(string filename, string key, string value)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                doc.SelectSingleNode("config/" + key).InnerText = value;

            }
            else
            {

                doc.CreateElement(key);
                doc.SelectSingleNode("config/" + key).InnerText = value;

            }

            doc.Save(filename);

        }

        /// <summary>
        /// Set a global setting.
        /// </summary>
        /// <param name="filename">name of the file to grab setting from.</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void nwSetGlobalString(string filename, string key, string value)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            if (doc.SelectSingleNode("config/" + key) != null)
            {

                doc.SelectSingleNode("config/" + key).InnerText = value;

            }
            else
            {

                doc.CreateElement(key);
                doc.SelectSingleNode("config/" + key).InnerText = value;

            }

            doc.Save(filename);

        }
    }
}
