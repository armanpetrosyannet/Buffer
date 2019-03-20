using BufferCashe.Buffer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Reciver.Extensions
{
    public static class BufferExtension
    {
        private static NameValueCollection Settings = (NameValueCollection)ConfigurationManager.GetSection("QueueSettings") as NameValueCollection;

        public static IEnumerable<BufferParametrs> ReadParametrs()
        {
            foreach (var item in Settings.AllKeys)
            {
                yield return new Parameter { Name = item, MaxCount = Int32.Parse(Settings[item]), _startDate = DateTime.Now, TimeOut = 30 };
            }
        }


        public static Dictionary<ObjectTypeEnum, List<ObjectChange>> ProcessMessage(BasicDeliverEventArgs e)
        {
            try
            {
                Dictionary<ObjectTypeEnum, List<ObjectChange>> dict =
                (Dictionary<ObjectTypeEnum, List<ObjectChange>>)JsonConvert.DeserializeObject(Decompress(e.Body).ToString(), typeof(Dictionary<ObjectTypeEnum, List<ObjectChange>>), new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Utc });

                return dict;
            }

            catch (Exception ex)
            {
                return null;
            }


        }

        public static string Decompress(byte[] data)
        {
            using (var msi = new MemoryStream(data))
            using (var mso = new MemoryStream())
            {
                using (var zipStream = new DeflateStream(msi, CompressionMode.Decompress))
                {
                    zipStream.CopyTo(mso);
                }
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }



    }
}
