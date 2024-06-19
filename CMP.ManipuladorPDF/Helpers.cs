using iText.Kernel.Font;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;

namespace CMP.ManipuladorPDF
{
    public static class APIRequest
    {
        private static async Task<string> DoRequest(string type, string url, Dictionary<string, string> values = null)
        {
            using HttpClient client = new HttpClient();
            type = type.ToLower();
            HttpResponseMessage response = new HttpResponseMessage();
            if (type == "post")
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                response = await client.PostAsync(url, content);
            }
            else
            {
                response = await client.GetAsync(url);
            }

            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
        public static async Task<string> Post(string url, Dictionary<string, string> values)
        {
            return await DoRequest("POST",url,values);
        }
        public static async Task<string> Get(string url)
        {
            return await DoRequest("GET", url);
        }

    }

    internal static class EmbeddedResource
    {
        public static Stream GetStream(string fileName)
        {
            string name = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        public static byte[] GetByteArray(string fileName)
        {
            using Stream Stream = EmbeddedResource.GetStream(fileName);
            byte[] ByteArray = new byte[Stream.Length];
            Stream.Read(ByteArray, 0, ByteArray.Length);
            return ByteArray;
        }
    }

    public static class ExtendString
    {
        public static string ToTitleCase(this string s)
        {
            TextInfo textInfo = new CultureInfo("pt-BR", false).TextInfo;
            s = textInfo.ToLower(s);
            s = textInfo.ToTitleCase(s);
            s = s.Replace(" De ", " de ");
            s = s.Replace(" Da ", " da ");
            s = s.Replace(" Do ", " do ");
            s = s.Replace(" Dos ", " dos ");
            s = s.Replace(" Das ", " das ");
            s = s.Replace(" E ", " e ");
            return s;
        }
    }

}