using Fleck;
using System.Text;
using TShockAPI;
using TShockAPI.DB;

namespace WSRC_Plugin
{
    public class ConsoleClient
    {
        public IWebSocketConnection client;
        public bool Authenticated;

        public ConsoleClient(IWebSocketConnection socket)
        {
            client = socket;
            client.OnMessage += OnMessage;
            client.OnClose += OnClose;
        }

        private void OnMessage(string data)
        {
            Message msg = JSON.Parse(data);
            PacketType packetType = msg.type;

            //Disconnect the user if he attempts to do anything else before authenticating.
            if (packetType != PacketType.Authenticate && !Authenticated)
            {
                Disconnect("未验证登录!");
                return;
            }
            switch (packetType)
            {
                case PacketType.Authenticate:                    
                    string Username = msg.UserName;
                    string Password = msg.Password;

                    if (!(Username == WSRC_Plugin.config.Username&& Password == WSRC_Plugin.config.Password))
                    {
                        Disconnect("用户名/密码错误!");
                        return;
                    }
                    Authenticated = true;
                    SendConsole();
                    break;
                case PacketType.Input:
                    string text = msg.Command;// e.Reader.ReadString();
                    try
                    {
                        WSRC_Plugin.ConsoleInput.SendText(text);
                    }
                    catch
                    {
                        Message msg_ = new Message("参数格式错误!", PacketType.Message);
                        client.Send(JSON.Format(msg_));
                    }
                    break;
            }
        }

        public void Disconnect(string message)
        {
            Message msg = new Message(message, PacketType.Disconnect);
            client.Send(JSON.Format(msg));
            client.Close();
        }

        private void OnClose()
        {
            WSRC_Plugin.clients.Remove(this);
        }

        public void SendConsole()
        {
            try
            {
                StringBuilder res_data = new StringBuilder();
                for (int i = 0; i < WSRC_Plugin.MessagesBuffer.Length; i++)
                {
                    if (!string.IsNullOrEmpty(WSRC_Plugin.MessagesBuffer[i]))
                    {
                        //res_data.AppendFormat("", WSRC_Plugin.ColorBuffer[i]);
                        res_data.AppendLine("%" + (byte)WSRC_Plugin.ColorBuffer[i] + "%" + WSRC_Plugin.MessagesBuffer[i]);
                    }

                }
                Message rse = new Message(res_data.ToString(), PacketType.MessageBuffer);
                client.Send(JSON.Format(rse));
            }
            catch { }
        }
    }

    //To be used for when I decide to add a graphical version of the remote console that uses additional packets.
    enum InterfaceType
    {
        CLI,
        GUI
    }
}
