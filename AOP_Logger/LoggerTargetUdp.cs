using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AopLogger
{
    public class LoggerTargetUdp:ILoggerTarget
    {
        private string clientIp,serverIp;
        private int clientPort,serverPort;
        private UdpClient udpClient;

        public LoggerTargetUdp(string serverIp, int serverPort,string clientIp, int clientPort)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            this.clientIp = clientIp;
            this.clientPort = clientPort;
        }

        public bool Flush(LogInfo logInfo)
        {
            Write(Encoding.Default.GetBytes(logInfo.ConvertToString().ToArray()));
            udpClient.Close();
            return true;
        }

        public async Task<bool> FlushAsync(LogInfo logInfo)
        {
            return true;
        }

        public void Write(byte[] log)
        {
            try
            {
                udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(clientIp), clientPort));
                udpClient.Send(log, log.Length,new IPEndPoint(IPAddress.Parse(serverIp),serverPort));
            }
            finally 
            {
                udpClient.Close();
            }
        }

        public void Close()
        { }
    }
}
