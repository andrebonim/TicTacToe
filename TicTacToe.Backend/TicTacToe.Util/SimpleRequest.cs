using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Next_V4.Util
{
    public class SimpleRequest
    {
        public static HttpWebRequest Create(string headerChrome, string url, long? dataLength = null)
        {
            HttpWebRequest retorno = (HttpWebRequest)HttpWebRequest.Create(url);
            string[] arrHeader = headerChrome.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var h in arrHeader)
            {
                string header = h.Trim();
                string headerLower = h.Trim().ToLower();
                if (header.Substring(0, 4) == "POST" || header.Substring(0, 3) == "GET")
                {
                    if (header.Substring(0, 4) == "POST")
                        retorno.Method = "POST";
                    else if (header.Substring(0, 3) == "GET")
                        retorno.Method = "GET";
                }
                else
                {
                    if (headerLower.Contains("connection:"))
                    {
                        if (headerLower.Contains("keep-alive"))
                            retorno.KeepAlive = true;
                    }
                    else if (headerLower.Contains("content-length:"))
                    {
                        if (dataLength != null)
                        {
                            retorno.ContentLength = dataLength.Value;
                        }
                        else
                        {
                            long length = 0;
                            if (long.TryParse(header.Split(new char[] { ':' }, 2)[1].Trim(), out length))
                                retorno.ContentLength = length;
                        }
                    }
                    else if (headerLower.Contains("host:"))
                    {
                        retorno.Host = headerLower.Split(new char[] { ':' }, 2)[1].Trim();
                    }
                    else if (headerLower.Contains("accept:"))
                    {
                        retorno.Accept = header.Split(new char[] { ':' }, 2)[1].Trim();
                    }
                    else if (headerLower.Contains("user-agent:"))
                    {
                        retorno.UserAgent = header.Split(new char[] { ':' }, 2)[1].Trim();
                    }
                    else if (headerLower.Contains("content-type:"))
                    {
                        retorno.ContentType = header.Split(new char[] { ':' }, 2)[1].Trim();
                    }
                    else if (headerLower.Contains("referer:"))
                    {
                        retorno.Referer = header.Split(new char[] { ':' }, 2)[1].Trim();
                    }
                    else if (headerLower.Contains("accept-encoding:"))
                    { }
                    else
                    {
                        string[] arr = header.Split(new char[] { ':' }, 2);
                        retorno.Headers.Add(arr[0].Trim(), arr[1].Trim());
                    }
                }
            }
            return retorno;
        }

        public static Stream ExecuteGetStream(HttpWebRequest request, byte[] byteArray = null)
        {
            string retorno = string.Empty;
            Stream dataStream = null;
            try
            {

                if (request.Method.ToUpper() == "POST")
                {
                    dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray != null ? byteArray.Length : 0);
                    dataStream.Close();
                }

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                retorno = reader.ReadToEnd();

                reader.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dataStream;
        }

        public static string ExecuteGetString(HttpWebRequest request, Encoding encoding = null, byte[] byteArray = null)
        {
            try
            {
                return ExecuteGetStringAsync(request, encoding, byteArray).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public static async Task<string> ExecuteGetStringAsync(HttpWebRequest request, Encoding encoding = null, byte[] byteArray = null)
        {

            string retorno;
            Stream dataStream;

            try
            {
                if (request.Method.ToUpper() == "POST")
                {
                    request.ContentLength = byteArray.Length;
                    dataStream = await request.GetRequestStreamAsync();
                    dataStream.Write(byteArray, 0, byteArray != null ? byteArray.Length : 0);
                    dataStream.Close();
                }
                var response = await request.GetResponseAsync();
                dataStream = response.GetResponseStream();
                StreamReader reader;
                if (encoding != null)
                    reader = new StreamReader(dataStream, encoding);
                else
                    reader = new StreamReader(dataStream);

                retorno = reader.ReadToEnd();

                reader.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retorno;
        }

        public static void Execute(HttpWebRequest request, ref HttpWebResponse response, ref Stream stream, byte[] byteArray = null)
        {
            try
            {
                try
                {
                    if (request.Method.ToUpper() == "POST")
                    {
                        stream = request.GetRequestStream();
                        stream.Write(byteArray, 0, byteArray != null ? byteArray.Length : 0);
                        stream.Close();
                    }

                    response = (HttpWebResponse)request.GetResponse();
                    stream = response.GetResponseStream();


                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
