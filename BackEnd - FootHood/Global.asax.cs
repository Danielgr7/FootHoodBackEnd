using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Timers;

namespace BackEnd___FootHood
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        static Timer timer = new Timer();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //code for timer
            timer.Interval = 1000 * 60 * 60;
            timer.Elapsed += tm_Tick;
            //StartTimer();
        }

        private void tm_Tick(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now > Convert.ToDateTime("08:00") && DateTime.Now < Convert.ToDateTime("09:00"))
                BackEnd___FootHood.Models.TimerServices.AlertManagerAboutMissingPlayers();

            BackEnd___FootHood.Models.TimerServices.AlertPlayersAboutTodaysGame();
            BackEnd___FootHood.Models.TimerServices.AlertToRatePlayersAfterGame();
        }

        public static void StartTimer()
        {
            timer.Enabled = true;
        }

        public static void EndTimer()
        {
            timer.Enabled = false;
        }


    }
}
