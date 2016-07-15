using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using Fleck;

namespace WSRC_Plugin
{
    [ApiVersion(1, 23)]
    public class WSRC_Plugin : TerrariaPlugin
    {
        public static Version buildVersion => Assembly.GetExecutingAssembly().GetName().Version;
        public static Config config;
        public static TaskReader ConsoleInput;
        public static List<ConsoleClient> clients;
        public static WebSocketServer g_server;

        public static string[] MessagesBuffer;
        public static byte[] ColorBuffer;

        public override string Author => "MBoss";
        public override string Description => "Allows for remote control of the tshock server[允许远程访问TShock服务器]";
        public override string Name => "Websocket Remote Console[远程管理]";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public WSRC_Plugin(Main game) : base(game)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Order = -1;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            if (dllName.EndsWith("_resources")) return null;

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", Assembly.GetExecutingAssembly());
            
            byte[] bytes = (byte[])rm.GetObject(dllName);

            return Assembly.Load(bytes);
        }

        public override void Initialize()
        {
            if (!File.Exists(Config.ConfigPath))
            {
                Directory.CreateDirectory(Config.ConfigPath.Replace("WSRC_config.json", ""));
                config = new Config();
                config.Save();
            }
            config = Config.Load();

            MessagesBuffer = new string[config.MessageBufferLength];
            ColorBuffer = new byte[config.MessageBufferLength];

            clients = new List<ConsoleClient>();
            g_server = new WebSocketServer("ws://0.0.0.0:"+config.ListenPort);
            g_server.Start(socket =>
            {
                WebSocketConnection connection = socket as WebSocketConnection;
                if (connection != null)
                {
                    connection.OnOpen = () => OnOpen(connection);
                }
            });

            ConsoleInput = new TaskReader(Console.In);
            Console.SetIn(ConsoleInput);
            Console.SetOut(new TaskWriter(Console.Out, SendInputToClients));

            TShockAPI.Commands.ChatCommands.Add(new TShockAPI.Command("wsc.admin", ChatCommand, "wsc"));
        }
        public void ChatCommand(TShockAPI.CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendInfoMessage("插件用法:");
                args.Player.SendInfoMessage("\t输入 /wsc get 读取当前配置");
                args.Player.SendInfoMessage("\t输入 /wsc set <port/username/password> 新值 修改对应配置");
                args.Player.SendInfoMessage("\t输入 /wsc reload 重新载入配置");
                return;
            }

            if (args.Parameters[0].Equals("get", StringComparison.CurrentCultureIgnoreCase))
            {
                args.Player.SendInfoMessage("当前配置为:");
                args.Player.SendInfoMessage("\tport: " + config.ListenPort);
                args.Player.SendInfoMessage("\tusername: " + config.Username);
                args.Player.SendInfoMessage("\tpassword: " + config.Password);
                args.Player.SendInfoMessage("其他配置请查看文件");
            }
            else if (args.Parameters[0].Equals("set", StringComparison.CurrentCultureIgnoreCase) && args.Parameters.Count > 2 && args.Parameters[2].Length > 0)
            {
                if (args.Parameters.Count < 3)
                {
                    args.Player.SendErrorMessage("正确语法: /wsc set <port/username/password> 新值");
                }
                else if (args.Parameters[2].Length < 1)
                {
                    args.Player.SendErrorMessage("参数值未设置");
                }
                else
                {
                    switch (args.Parameters[1].ToLower())
                    {
                        case "port":
                            int p;
                            if (Int32.TryParse(args.Parameters[2], out p))
                            {
                                config.ListenPort = p;
                            }
                            break;
                        case "username":
                            config.Username = args.Parameters[2].Trim();
                            break;
                        case "password":
                            config.Password = args.Parameters[2].Trim();
                            break;
                        default:
                            args.Player.SendErrorMessage("输入 /wsc set <port/username/password> 修改对应配置");
                            break;
                    }
                    config.Save();
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("配置已更新，请刷新浏览器重新连接!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else if (args.Parameters[0].Equals("reload", StringComparison.CurrentCultureIgnoreCase))
            {
                config = Config.Load();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("配置已更新，请刷新浏览器重新连接!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                args.Player.SendInfoMessage("插件用法:");
                args.Player.SendInfoMessage("\t输入 /wsc get 读取当前配置");
                args.Player.SendInfoMessage("\t输入 /wsc set <port/username/password> 新值 修改对应配置");
                args.Player.SendInfoMessage("\t输入 /wsc reload 重新载入配置");
            }
        }

        public static void AddToMessageBuffer(string message, byte color)
        {
            for (int i = 1; i < MessagesBuffer.Length; i++)
                MessagesBuffer[i - 1] = MessagesBuffer[i];

            for (int i = 1; i < ColorBuffer.Length; i++)
                ColorBuffer[i - 1] = ColorBuffer[i];

            MessagesBuffer[MessagesBuffer.Length - 1] = message;
            ColorBuffer[ColorBuffer.Length - 1] = color;
        }

        public Action<string> SendInputToClients = (s) =>
        {
            if (s.Length > 0)
            {
                if (s == ": ")
                    return;

                AddToMessageBuffer(s, (byte)Console.ForegroundColor);
                string msg = JSON.Format(new Message("%" + (byte)Console.ForegroundColor + "%" + s, PacketType.Message));
                foreach (ConsoleClient clnt in clients)
                    clnt?.client?.Send(msg);
            }
        };

        private void OnOpen(WebSocketConnection scoket)
        {
            if (clients.Count >= config.MaxConnections)
            {
                Message msg = new Message("连接数超限!", PacketType.Disconnect);
                scoket.Send(JSON.Format(msg));
                scoket.Close();
            }
            clients.Add(new ConsoleClient(scoket));
        }
    }
}
