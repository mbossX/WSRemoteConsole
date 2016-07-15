using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WSRC_Plugin
{
    public static class JSON
    {
        public static Message Parse(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<Message>(data);
            }
            catch
            {
                return new Message("");
            }
        }

        public static String Format(Message message)
        {
            try
            {
                return JsonConvert.SerializeObject(message, Formatting.Indented);
            }
            catch
            {
                return "";
            }
        }
    }

    public class Message
    {
        public PacketType type = PacketType.Input;
        public string UserName = "";
        public string Password = "";
        public string Command = "";

        public Message(string c, PacketType t = PacketType.Input, string u = "", string p = "")
        {
            Command = c;
            type = t;
            UserName = u;
            Password = p;
        }
    }
}
