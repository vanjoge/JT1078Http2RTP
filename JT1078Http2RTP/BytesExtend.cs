using System;
using System.Collections.Generic;
using System.Text;

namespace JT1078Http2RTP
{
    public static class Bytes0D0AExtend
    {
        /// <summary>
        /// 查找下一个0D0A索引位置
        /// </summary>
        /// <param name="bts"></param>
        /// <param name="offset"></param>
        /// <param name="maxCount"></param>
        /// <returns>大于等于0下一个0D0A的索引 小于0表示未找到 </returns>
        public static int FindNext0D0AIndex(this byte[] bts, int offset, int maxCount = 0)
        {
            if (maxCount <= 0)
            {
                maxCount = bts.Length - offset;
            }
            maxCount -= 1;
            for (int i = 0; i < maxCount; i++)
            {
                if (bts[offset + i] == 0xd && bts[offset + i + 1] == 0xa)
                {
                    return offset + i;
                }
            }

            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bts"></param>
        /// <param name="offset"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static byte[] FindNext0D0ABytes(this byte[] bts, int offset, int maxCount = 0)
        {
            var index = FindNext0D0AIndex(bts, offset, maxCount);
            if (index > 0)
            {
                var len = index - offset;
                var dest = new byte[len];
                if (len > 0)
                {
                    Array.Copy(bts, offset, dest, 0, len);
                }
                return dest;
            }
            return null;
        }
    }
}
