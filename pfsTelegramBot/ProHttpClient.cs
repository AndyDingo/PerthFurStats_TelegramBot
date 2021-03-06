﻿/*
 * All contents copyright 2016 - 2020, Andy Dingo and Contributors
 * All rights reserved.  YOU MAY NOT REMOVE THIS NOTICE.
 * Please read docs/gpl.txt for licensing information.
 * ---------------------------------------------------------------
 * -- CREATOR INFORMATION --
 * Created by   : Microsoft Visual Studio 2015.
 * User         : AndyDingoWolf
 * Last Updated : 13/10/2017 by AndyDingo
 * -- VERSION --
 * Version      : 1.0.0.203
 */

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelegramBot1
{
    /// <summary>
    /// A http client
    /// </summary>
    public class ProHttpClient : HttpClient
    {
        /// <summary>
        /// http client constructor
        /// </summary>
        public ProHttpClient()
        {
            Timeout = new TimeSpan(0, 0, 30);
            ReferrerUri = "https://duckduckgo.com";
            AuthorizationHeader = string.Empty;
            DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.5");
            DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            DefaultRequestHeaders.TryAddWithoutValidation("DNT", "1");
            DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:44.0) Gecko/20100101 Firefox/44.0");
        }

        public string AuthorizationHeader { get; set; }

        public string ReferrerUri { get; set; }

        public async Task<string> DownloadString(string uri)
        {
            BuildHeaders();
            var response = await GetStringAsync(uri);
            CleanHeaders();
            return response;
        }

        public async Task<Stream> DownloadData(string uri)
        {
            BuildHeaders();
            var response = await GetStreamAsync(uri);
            CleanHeaders();
            return response;
        }

        private void BuildHeaders()
        {
            DefaultRequestHeaders.Referrer = new Uri(ReferrerUri);
            if (AuthorizationHeader != string.Empty)
            {
                DefaultRequestHeaders.TryAddWithoutValidation("Authorization", AuthorizationHeader);
            }
        }

        private void CleanHeaders()
        {
            ReferrerUri = "https://duckduckgo.com";
            AuthorizationHeader = string.Empty;
            DefaultRequestHeaders.Remove("Authorization");
        }
    }
}