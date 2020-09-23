using Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace JT1078Http2RTP
{
    public class JTHClient
    {
        /// <summary>
        /// HTTP TCP链路
        /// </summary>
        TCPChannel channelhttp;
        /// <summary>
        /// HTTP请求地址
        /// </summary>
        string httpUrl;
        /// <summary>
        /// 握手协议地址 会转义
        /// </summary>
        string HandShakeUrl;
        /// <summary>
        /// HTTP协议解析类
        /// </summary>
        JTHttpProtocol httpProtocol = new JTHttpProtocol();
        /// <summary>
        /// 1078RTP客户端
        /// </summary>
        JT1078Client jt1078Client;
        /// <summary>
        /// 1078服务器地址
        /// </summary>
        string Server1078;
        /// <summary>
        /// 1078服务器端口
        /// </summary>
        int Port1078;
        /// <summary>
        /// 启动状态
        /// </summary>
        bool Started = true;
        /// <summary>
        /// 
        /// </summary>
        string Key;
        /// <summary>
        /// 
        /// </summary>
        private JTTask MyTask;

        /// <summary>
        /// URL字符串正则
        /// </summary>
        static System.Text.RegularExpressions.Regex regUrl = new System.Text.RegularExpressions.Regex(@"http://([^:/]+):(\d+)/(([^.]+)\.(\d+)\.(\d+)\.(\d+)\.(.+))", System.Text.RegularExpressions.RegexOptions.Compiled);
        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="httpUrl"></param>
        /// <param name="Server1078"></param>
        /// <param name="Port1078"></param>
        /// <returns></returns>
        public static JTHClient Start(JTTask MyTask, string Key, string httpUrl, string Server1078, int Port1078)
        {
            var mth = regUrl.Match(httpUrl);
            if (!mth.Success)
            {
                return null;
            }
            var ip = mth.Groups[1].Value;
            var port = Convert.ToInt32(mth.Groups[2].Value);

            var jthc = new JTHClient();
            jthc.httpUrl = httpUrl;
            jthc.HandShakeUrl = "/" + HttpUtility.UrlEncode(mth.Groups[3].Value);
            jthc.Server1078 = Server1078;
            jthc.Port1078 = Port1078;
            jthc.Key = Key;
            jthc.MyTask = MyTask;

            var channel = new TCPChannel(ip, port);
            jthc.channelhttp = channel;
            channel.DataReceive = jthc.Receive;
            channel.DataSend = jthc.DataSend;
            channel.ChannelDispose += jthc.Http_ChannelDispose;
            channel.ChannelConnect += jthc.Http_ChannelConnect;
            channel.Connect();


            return jthc;
        }
        public bool Stop()
        {
            if (Started)
            {
                Started = false;
                channelhttp.Close();
                jt1078Client?.Stop();

                MyTask.RemoveItem(this.Key);

                return true;
            }
            return false;
        }

        private void Http_ChannelConnect(object sender, ChannelConnectArg arg)
        {
            if (arg.SocketError == SocketError.Success)
            {
                jt1078Client = JT1078Client.Start(this, Server1078, Port1078);

                var bts = JTHttpProtocol.HandShake(HandShakeUrl);
                channelhttp.Send(bts);
            }
            else
            {
                Stop();
            }
        }

        private void Http_ChannelDispose(object sender, ChannelDisposeArg arg)
        {
            Stop();
        }

        private void DataSend(object sender, ChannelSendArg arg)
        {

        }

        private void Receive(object sender, ChannelReceiveArg arg)
        {
            try
            {
                httpProtocol.SplitChunkedData(arg.Buffer, arg.BufferOffset, arg.BufferSize, jt1078Client.Send);
            }
            catch (Exception ex)
            {
                SQ.Base.Log.WriteLog4Ex("Http_Receive", ex);
            }
        }
    }
}
