using Network;
using SQ.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace JT1078Http2RTP
{
    public class JTHttpProtocol
    {
        public delegate void AnalyzeData(byte[] data, int offset, int len);
        public JTHttpProtocol()
        {
            MustReadHeader = true;
            ChunkedLenData = new List<byte>();
        }
        /// <summary>
        /// 客户端发送握手
        /// </summary>
        /// <returns></returns>
        public static byte[] HandShake(string url)
        {
            StringBuilder response = new StringBuilder();
            response.Append("GET " + url + " HTTP/1.1\r\n");
            response.Append("Host: 127.0.0.1\r\n\r\n");
            return Encoding.UTF8.GetBytes(response.ToString());
        }
        /// <summary>
        /// HTTP头数据
        /// </summary>
        public List<byte> headerBytes = new List<byte>();
        /// <summary>
        /// 是否需要读HTTP头
        /// </summary>
        public bool MustReadHeader { get; protected set; }
        /// <summary>
        /// 没有HTTP协议封装
        /// </summary>
        public bool NoHttpPackage { get; protected set; }
        /// <summary>
        /// HTTP头标识符
        /// </summary>
        private bool f1, f2, f3;
        /// <summary>
        /// Chunked分包长度数据
        /// </summary>
        private List<byte> ChunkedLenData;
        /// <summary>
        /// 下包需求长度
        /// </summary>
        private int NeedLen;

        bool flag0D = false, flag0A = false, flag0D_2 = false;
        /// <summary>
        /// 找HTTP头
        /// </summary>
        /// <param name="bts"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>-1 表示未找到结尾 其他表示结尾位置</returns>
        public int ReadHeader(byte[] bts, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                headerBytes.Add(bts[offset + i]);
                //头结束
                if (f1 && f2 && f3 && bts[offset + i] == 0xa)
                {
                    return offset + i + 1;
                }
                if (headerBytes.Count > 2)
                {
                    f1 = headerBytes[headerBytes.Count - 3] == 0xd;
                    f2 = headerBytes[headerBytes.Count - 2] == 0xa;
                    f3 = headerBytes[headerBytes.Count - 1] == 0xd;
                }
            }
            return -1;
        }

        /// <summary>
        /// 读取Chunked数据长度(如未找到会缓存之前0D0A之后的数据)
        /// </summary>
        /// <param name="bts"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="dataIndex"></param>
        /// <returns>为null表示未找到 其他表示找到的长度二进制数据</returns>
        protected byte[] ReadChunkedDataLen(byte[] bts, int offset, int count, out int dataIndex)
        {
            dataIndex = -1;
            count = count + offset;
            for (int i = offset; i < count; i++)
            {
                if (!flag0D)//找第一个0D
                {
                    flag0D = bts[i] == 0xD;
                    continue;
                }
                if (!flag0A)//第二个0A
                {
                    flag0A = bts[i] == 0xA;
                    if (!flag0A)//不为0A继续找第一个0D
                    {
                        flag0D = false;
                    }
                    //找到第一个0D0A 开始新缓存数据
                    ChunkedLenData.Clear();
                    continue;
                }
                if (!flag0D_2)//找第二个0D
                {
                    flag0D_2 = bts[i] == 0xD;
                    if (!flag0D_2)
                    {
                        ChunkedLenData.Add(bts[i]);
                    }
                    continue;
                }
                if (bts[i] == 0xA)//找结束
                {
                    var bb = ChunkedLenData.ToArray();
                    ChunkedLenData.Clear();
                    dataIndex = i + 1;
                    flag0D = flag0A = flag0D_2 = false;
                    return bb;
                }
                else
                {
                    ChunkedLenData.Add(0xD);//补上一个未添加0D
                    ChunkedLenData.Add(bts[i]);
                }
            }
            return null;
        }
        /// <summary>
        /// 获取Chunked数据部分的索引
        /// </summary>
        /// <param name="bts"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="dataIndex">Chunked数据开始位置索引</param>
        /// <param name="datalen">Chunked数据长度</param>
        /// <returns>是否找到Chunked封装</returns>
        public bool GetChunkedDataIndex(byte[] bts, int offset, int count, out int dataIndex, out int datalen)
        {
            datalen = -1;
            var hbts = ReadChunkedDataLen(bts, offset, count, out dataIndex);
            if (hbts != null)
            {
                string hexlen = ByteHelper.GBKToString(hbts);
                datalen = Convert.ToInt32(hexlen, 16);


                return true;
            }
            return false;
        }

        ///// <summary>
        ///// 获取Chunked数据部分的索引
        ///// </summary>
        ///// <param name="bts"></param>
        ///// <param name="offset"></param>
        ///// <param name="count"></param>
        ///// <param name="dataIndex">Chunked数据开始位置索引</param>
        ///// <param name="datalen">Chunked数据长度</param>
        ///// <returns>是否找到Chunked封装</returns>
        //public bool GetChunkedDataIndex(byte[] bts, int offset, int count, out int dataIndex, out int datalen)
        //{
        //    //TODO:未处理第二个0D0A 分开发送bug
        //    dataIndex = datalen = -1;

        //    //找两个0D0A之间的数据
        //    int index1;
        //    if (flag0D)//上一包结尾是0D
        //    {
        //        flag0A = bts[offset] == 0xA;
        //        if (flag0A)
        //        {
        //            offset++;
        //        }
        //    }
        //    if (flag0D && flag0A)//已读第一个0D0A
        //    {
        //        index1 = offset;
        //    }
        //    else
        //    {
        //        index1 = bts.FindNext0D0AIndex(offset, count);
        //        if (index1 < 0)//未找到0D0A
        //        {
        //            flag0D = bts[offset + count] == 0xD;
        //            LastData2.Clear();
        //            return false;
        //        }
        //        index1 += 2;
        //        count = count - (index1 - offset);
        //    }
        //    var index2 = bts.FindNext0D0AIndex(index1, count);
        //    if (index2 >= 0)
        //    {
        //        var len = index2 - index1;
        //        var hbts = new byte[len];
        //        Array.Copy(bts, index1, hbts, 0, len);

        //        string hexlen;
        //        if (LastData2.Count == 0)
        //        {
        //            hexlen = ByteHelper.GBKToString(hbts);
        //        }
        //        else
        //        {
        //            LastData2.AddRange(hbts);
        //            hexlen = ByteHelper.GBKToString(LastData2.ToArray());
        //            LastData2.Clear();
        //        }
        //        datalen = Convert.ToInt32(hexlen, 16);

        //        dataIndex = index2 + 2;

        //        return true;

        //    }
        //    else
        //    {
        //        flag0D = flag0A = true;
        //        for (int i = index1; i < count; i++)
        //        {
        //            LastData2.Add(bts[i]);
        //        }
        //    }

        //    return false;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bts"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="analyze"></param>
        public void SplitChunkedData(byte[] bts, int offset, int count, AnalyzeData analyze)
        {
            if (NoHttpPackage)
            {
                analyze(bts, offset, count);
                return;
            }
            if (MustReadHeader)
            {
                //首包数据为特征数据表示无HTTP封装
                if (bts.Length > offset + 3 && bts[offset] == 0x30 && bts[offset + 1] == 0x31 && bts[offset + 2] == 0x63 && bts[offset + 3] == 0x64)
                {
                    NoHttpPackage = true;
                    analyze(bts, offset, count);
                    return;
                }
                var hlen = ReadHeader(bts, offset, count);
                if (hlen < 0)
                {
                    return;
                }
                offset += hlen - 2;
                count -= hlen - 2;
                MustReadHeader = false;
            }
            while (count > 0 && bts.Length > offset)
            {
                if (NeedLen > 0)
                {
                    if (NeedLen > count)
                    {
                        //数据未收全，缓存数据等待下一包再次触发analyze
                        analyze(bts, offset, count);
                        NeedLen -= count;
                        break;
                    }
                    analyze(bts, offset, NeedLen);

                    offset += NeedLen;
                    count -= NeedLen;
                    NeedLen = 0;
                }
                else
                {
                    if (GetChunkedDataIndex(bts, offset, count, out int dataIndex, out int datalen))
                    {
                        var blen = count - dataIndex + offset;
                        if (datalen > blen)
                        {
                            //数据未收全，等待下一包再次触发analyze
                            analyze(bts, dataIndex, blen);
                            NeedLen = datalen - blen;
                            break;
                        }
                        else
                        {
                            analyze(bts, dataIndex, datalen);
                            count = blen - datalen;
                            offset = dataIndex + datalen;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }



    }

}
