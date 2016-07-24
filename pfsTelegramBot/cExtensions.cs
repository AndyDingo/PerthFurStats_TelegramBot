/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.75
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace nwTelegramBot
{
    /// <summary>
    /// Extensions class, for a few things.
    /// </summary>
    public static class cExtensions
    {
        /// <summary>
        /// Is this a positive number?
        /// </summary>
        /// <returns>true, if it is indeed, or false.</returns>
        public static bool IsPositive(this int number)
        {
            return number > 0;
        }

        /// <summary>
        /// Is this a negative number?
        /// </summary>
        /// <returns>true, if it is indeed, or false.</returns>
        public static bool IsNegative(this int number)
        {
            return number < 0;
        }

        /// <summary>
        /// Is this zero?
        /// </summary>
        /// <returns>true, if it is indeed, or false.</returns>
        public static bool IsZero(this int number)
        {
            return number == 0;
        }

        public static bool IsAwesome(this int number)
        {
            return IsNegative(number) && IsPositive(number) && IsZero(number);
        }

        public static FileVersionInfo nwGetFileVersionInfo
        {
            get
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = Path.GetDirectoryName(location);
                var file = Path.Combine(directory,
                  Process.GetCurrentProcess().ProcessName + ".exe");

                return FileVersionInfo.GetVersionInfo(file);
            }
        }

        /// <summary>
        /// Censor a given string.
        /// </summary>
        /// <param name="input">The string to censor.</param>
        /// <returns>The censored string.</returns>
        public static string nwStringCensor(string input)
        {
            string exceptions;
            using (StreamReader sr = new StreamReader(Environment.CurrentDirectory+@"\data\censorlist.txt"))
            {
                exceptions = sr.ReadToEnd();
            }

            string[] exceptionsList = exceptions.Split(Environment.NewLine.ToCharArray());
            string[] wordList = input.Split(' ');

            string final = null;
            var result = wordList.Except(exceptionsList).ToArray();
            final = string.Join(" ", result);

            return final;
        }
    }

    /// <summary>
    /// The type of the permission
    /// </summary>
    enum PermissionType : int
    {
        Admin,
        Developer,
        User,
        PowerUser,
        Banned,
    }
}
