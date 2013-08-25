using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using AsyncOAuth;
using Microsoft.Phone.Net.NetworkInformation;
using Tools;

namespace VikingApi.ApiTools
{

    public static class ApiTools
    {
        public static bool HasInternetConnection()
        {
            return (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None);
        }

        public static string SerializeAccessToken(AccessToken token)
        {
            var serializer = new XmlSerializer(typeof(AccessToken));

            TextWriter writer = new StringWriter();
            serializer.Serialize(writer, token);

            return writer.ToString();
        }

        public static AccessToken DeserializeAccessToken(string tData)
        {
            var serializer = new XmlSerializer(typeof(AccessToken));

            TextReader reader = new StringReader(tData);

            return (AccessToken)serializer.Deserialize(reader);
        }
    }
}
