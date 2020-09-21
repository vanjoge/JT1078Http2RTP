using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JT1078Http2RTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JTH2R_Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public bool Get(string httpUrl, string Server1078, int Port1078)
        {
            try
            {
                return Program.task.StartNewHttp2RTP(httpUrl, Server1078, Port1078);
            }
            catch (Exception ex)
            {
                SQ.Base.Log.WriteLog4Ex("ApiController.Get", ex);
                return false;
            }
        }
    }
}
