using Network;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace JT1078Http2RTP
{
    /// <summary>
    /// 
    /// TODO: 未做线程安全处理，未对HTTP补0做去除
    /// </summary>
    public class JT1078Client
    {
        /// <summary>
        /// 1078 TCP链路
        /// </summary>
        TCPChannel channel1078;
        /// <summary>
        /// 启动状态
        /// </summary>
        bool Started = true;

        List<byte[]> lstBuffs = new List<byte[]>();
        /// <summary>
        /// 指示当前是否可以直接发送
        /// </summary>
        bool CanSend = false;
        /// <summary>
        /// 
        /// </summary>
        JTHClient jtHttp;

        public static JT1078Client Start(JTHClient jtHttp, string Server1078, int Port1078)
        {
            JT1078Client jt1078 = new JT1078Client();
            jt1078.jtHttp = jtHttp;
            var channel = new TCPChannel(Server1078, Port1078);
            jt1078.channel1078 = channel;
            channel.DataReceive = jt1078.Receive;
            channel.DataSend = jt1078.DataSend;
            channel.ChannelDispose += jt1078.ChannelDispose;
            channel.ChannelConnect += jt1078.ChannelConnect;
            channel.Connect();
            return jt1078;
        }
        public bool Stop()
        {
            if (Started)
            {
                Started = false;
                jtHttp.Stop();
                channel1078.Close();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 发送1078封包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void Send(byte[] data, int offset, int count)
        {
            try
            {
                if (CanSend)
                {
                    channel1078.Send(data, offset, count);
                }
                else
                {
                    //必须copy一遍，因为此处直接用的socket通信的buff
                    var bts = new byte[count];
                    Array.Copy(data, offset, bts, 0, count);
                    lstBuffs.Add(bts);
                }
            }
            catch (Exception ex)
            {
                SQ.Base.Log.WriteLog4Ex("JT1078Client.Send", ex);
            }
        }

        private void ChannelConnect(object sender, ChannelConnectArg arg)
        {
            if (arg.SocketError == SocketError.Success)
            {
                while (lstBuffs.Count > 0)
                {
                    channel1078.Send(lstBuffs[0]);
                    lstBuffs.RemoveAt(0);
                }
                CanSend = true;
            }
            else
            {
                Stop();
            }
        }

        private void ChannelDispose(object sender, ChannelDisposeArg arg)
        {
            Stop();
        }

        private void DataSend(object sender, ChannelSendArg arg)
        {
        }

        private void Receive(object sender, ChannelReceiveArg arg)
        {
        }
    }
}