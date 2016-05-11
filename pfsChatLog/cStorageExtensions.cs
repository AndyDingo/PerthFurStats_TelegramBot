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

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace pfsChatLog
{
    /// <summary>
    /// Extensions to windows storage classes.
    /// </summary>
    public static class cStorageExtensions
    {
        ///// <summary>
        ///// The big phat instance. It doesn't have a boss, and it doesn't give phat lewtz.
        ///// </summary>
        //public static cStorageExtensions Instance = new cStorageExtensions();

        /// <summary>
        /// Save to windows storage
        /// </summary>
        /// <param name="filename">The name of the file to write to.</param>
        /// <param name="content">The content to write.</param>
        /// <returns></returns>
        public static async Task nwSaveMessageToLocalFile(string filename, string content)
        {
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            // write the char array created from the content string into the file
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
        }
    }
}
