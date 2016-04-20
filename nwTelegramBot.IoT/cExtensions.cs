/* 
 * All contents copyright 2016, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by: Microsoft Visual Studio 2015.
 * User      : AndyDingoWolf
 * -- VERSION --
 * Version   : 1.0.0.26
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nwTelegramBot.IoT
{
    public static class cExtensions
    {
        public static bool IsPositive(this int number)
        {
            return number > 0;
        }

        public static bool IsNegative(this int number)
        {
            return number < 0;
        }

        public static bool IsZero(this int number)
        {
            return number == 0;
        }

        public static bool IsAwesome(this int number)
        {
            return IsNegative(number) && IsPositive(number) && IsZero(number);
        }


        //public static FileVersionInfo nwGetFileVersionInfo
        //{
        //    get
        //    {
        //        var location = System.Reflection.Assembly.GetEntryAssembly().Location;
        //        var directory = System.IO.Path.GetDirectoryName(location);
        //        var file = System.IO.Path.Combine(directory,
        //          Process.GetCurrentProcess().ProcessName + ".exe");


        //        return FileVersionInfo.GetVersionInfo(file);
        //    }
        //}
    }
}
