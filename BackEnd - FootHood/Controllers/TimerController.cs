using DATA.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/Timer")]
    public class TimerController : ApiController
    {
        //code for timer
        [HttpPost]
        [Route("StartTimer")]
        public void StartTimer()
        {
            WebApiApplication.StartTimer();
        }

        //code for timer
        [HttpGet]
        [Route("StopTimer")]
        public void StopTimer()
        {
            WebApiApplication.EndTimer();
        }
    }
}
