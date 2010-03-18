﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;
using System.Net;

namespace FlickrNet
{
    public partial class Flickr
    {

        private T GetResponseNoCache<T>(Dictionary<string, string> parameters) where T : IFlickrParsable, new()
        {
            return GetResponse<T>(parameters, TimeSpan.MinValue);
        }

        private T GetResponseAlwaysCache<T>(Dictionary<string, string> parameters) where T : IFlickrParsable, new()
        {
            return GetResponse<T>(parameters, TimeSpan.MaxValue);
        }

        private T GetResponseCache<T>(Dictionary<string, string> parameters) where T : IFlickrParsable, new()
        {
            return GetResponse<T>(parameters, Cache.CacheTimeout);
        }

        private T GetResponse<T>(Dictionary<string, string> parameters, TimeSpan cacheTimeout) where T : IFlickrParsable, new()
        {
            CheckApiKey();

            parameters["api_key"] = ApiKey;

            if (!String.IsNullOrEmpty(AuthToken))
            {
                parameters["auth_token"] = AuthToken;
            }

            Uri url;
            if (!String.IsNullOrEmpty(_sharedSecret))
                url = CalculateUri(parameters, true);
            else
                url = CalculateUri(parameters, false);

            _lastRequest = url.AbsoluteUri;

            string responseXml = String.Empty;

            if (CacheDisabled)
            {
                responseXml = DoGetResponse(url);
            }
            else
            {
                string urlComplete = url.AbsoluteUri;

                ResponseCacheItem cached = (ResponseCacheItem)Cache.Responses.Get(urlComplete, cacheTimeout, true);
                if (cached != null)
                {
                    responseXml = cached.Response;
                }
                else
                {
                    responseXml = DoGetResponse(url);

                    ResponseCacheItem resCache = new ResponseCacheItem();
                    resCache.Response = responseXml;
                    resCache.Url = urlComplete;
                    resCache.CreationTime = DateTime.UtcNow;

                    Cache.Responses.Shrink(Math.Max(0, Cache.CacheSizeLimit - responseXml.Length));
                    Cache.Responses[urlComplete] = resCache;
                }
            }

            _lastResponse = responseXml;

            XmlTextReader reader = new XmlTextReader(new StringReader(responseXml));
            reader.WhitespaceHandling = WhitespaceHandling.None;

            if (!reader.ReadToDescendant("rsp"))
            {
                throw new XmlException("Unable to find response element 'rsp' in Flickr response");
            }
            while (reader.MoveToNextAttribute())
            {
                if (reader.LocalName == "stat" && reader.Value == "fail")
                    throw ExceptionHandler.CreateResponseException(reader);
                continue;
            }

            reader.MoveToElement();
            reader.Read();

            T item = new T();
            item.Load(reader);

            return item;

        }

        /// <summary>
        /// A private method which performs the actual HTTP web request if
        /// the details are not found within the cache.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <param name="variables">The query string parameters to be added to the end of the URL.</param>
        /// <returns>A string containing the response XML.</returns>
        /// <remarks>If the final length of the URL would be greater than 2000 characters 
        /// then they are sent as part of the body instead.</remarks>
        private string DoGetResponse(string url, string variables)
        {
            return DoGetResponse(new Uri(url + "?" + variables));
        }

        private string DoGetResponse(Uri url)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;

            string postContents = String.Empty;

            if (url.AbsoluteUri.Length > 2000)
            {
                postContents = url.Query.Substring(1);
                url = new Uri(url, "");
            }

            // Initialise the web request
            req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = CurrentService == SupportedService.Zooomr ? "GET" : "POST";

            if (req.Method == "POST") req.ContentLength = postContents.Length;

            req.UserAgent = UserAgent;
            if (Proxy != null) req.Proxy = Proxy;
            req.Timeout = HttpTimeout;
            req.KeepAlive = false;
            if (postContents.Length > 0)
            {
                req.ContentType = "application/x-www-form-urlencoded";
                StreamWriter sw = new StreamWriter(req.GetRequestStream());
                sw.Write(postContents);
                sw.Close();
            }
            else
            {
                // This is needed in the Compact Framework
                // See for more details: http://msdn2.microsoft.com/en-us/library/1afx2b0f.aspx
                req.GetRequestStream().Close();
            }

            try
            {
                // Get response from the internet
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse res2 = (HttpWebResponse)ex.Response;
                    if (res2 != null)
                    {
                        throw new FlickrWebException(String.Format(System.Globalization.CultureInfo.InvariantCulture,"HTTP Error {0}, {1}", (int)res2.StatusCode, res2.StatusDescription), ex);
                    }
                }
                throw new FlickrWebException(ex.Message, ex);
            }

            string responseString = string.Empty;

            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
            {
                responseString = sr.ReadToEnd();
            }

            return responseString;
        }



    }
}
