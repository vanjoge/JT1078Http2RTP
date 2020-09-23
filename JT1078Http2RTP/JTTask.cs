using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JT1078Http2RTP
{
    public class JTTask : IDisposable
    {
        Dictionary<string, JTHClient> dit = new Dictionary<string, JTHClient>();
        object lck = new object();
        public bool StartNewHttp2RTP(string httpUrl, string Server1078, int Port1078)
        {
            var key = GetKey();
            var jtHttp = JTHClient.Start(this, key, httpUrl, Server1078, Port1078);
            if (jtHttp == null)
            {
                return false;
            }
            dit[key] = jtHttp;
            return true;
        }

        public string GetKey()
        {
            lock (lck)
            {
                return DateTime.Now.Ticks.ToString();
            }
        }

        public void Dispose()
        {
            try
            {
                var arr = dit.Keys.ToArray();
                foreach (var key in arr)
                {
                    if (dit.TryGetValue(key, out var item))
                    {
                        item.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                SQ.Base.Log.WriteLog4Ex("Dispose", ex);
            }
        }

        public void RemoveItem(string key)
        {
            try
            {
                if (dit.ContainsKey(key))
                {
                    dit.Remove(key);
                }
            }
            catch (Exception ex)
            {
                SQ.Base.Log.WriteLog4Ex("RemoveItem", ex);
            }
        }
    }
}
