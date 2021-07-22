using DATA.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.Models
{
    public class TimerServices
    {
        // צריך לשנות את כל התוקן לאקספו תוקן הרגיל !!!!!!!

        public static void AlertToRatePlayersAfterGame()
        {
            //כמה זמן לאחר משחק נרצה להוציא את ההתראה
            TimeSpan time2Alert = new TimeSpan(00, 05, 00); // צריך להיות (03,00,00
            FootHoodDBContext db = new FootHoodDBContext();
            //מביא את כל המשחקים שמתרחשים היום או שעבר יממה בגלל שהמשחק היה בשעה מאוחרת בלילה
            List<tblGame> games = db.tblGames.Where(x => DbFunctions.DiffDays(DateTime.Now, x.GameDate) == 0 || DbFunctions.DiffDays(DateTime.Now, x.GameDate) == -1).ToList();
            foreach (tblGame g in games)
            {
                //מביא את הזמן עכשיו
                var now = DateTime.Now.ToLongTimeString();
                var timeAfterAlert = TimeSpan.Parse(now) - time2Alert;
                var oneHourAfterAlertHour = g.GameTime + new TimeSpan(01, 00, 00); // צריך להיות (04,00,00
                //בודק שהשעה הנוכחית בזמן הריצה פחות הזמן שרצינו אכן לאחר הטווח של המשחק אך גם לא
                if (timeAfterAlert >= g.GameTime && timeAfterAlert < oneHourAfterAlertHour)
                {
                    List<tblPlayerRegisteredToGame> playersRegistered2Game = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == g.GameSerialNum && x.WaitOrNot == false).ToList();

                    foreach (tblPlayerRegisteredToGame p in playersRegistered2Game)
                    {
                        tblPlayer player2Send = db.tblPlayers.Where(x => x.Email.ToLower() == p.EmailPlayer.ToLower()).FirstOrDefault();
                        if (player2Send != null)
                        {
                            string expoToken = player2Send.TokenNotfication;
                            if (expoToken != null)
                            {
                                string title = "Hi " + player2Send.FirstName + " !";
                                string body = "Come and Rank your friends from the last game ! \nClick here to rank..";

                                // Create POST data and convert it to a byte array.  
                                var objectToSend = new
                                {
                                    //to = "ExponentPushToken[kSndkCPGAaWJIMuMZ-YFHa]",
                                    to = expoToken,
                                    title = title,
                                    body = body,
                                    //badge = pnd.badge,
                                    data = new { name = "RankPlayerFromGame", T_SerialNum = g.TeamSerialNum, G_SerialNum = g.GameSerialNum, indexGame = -1 }
                                    //new { name = "nir", grade = 100 }
                                };
                                tblPlayer.SendPushNotification(objectToSend);
                            }
                        }
                    }
                }
            }
        }

        public static void AlertPlayersAboutTodaysGame()
        {
            try
            {
                TimeSpan hours2Alert = new TimeSpan(08, 00, 00);
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblGame> games = db.tblGames.Where(x => DbFunctions.DiffDays(DateTime.Now, x.GameDate) == 0).ToList();
                foreach (tblGame g in games)
                {
                    var tiem = DateTime.Now.ToLongTimeString();
                    if (g.GameTime - TimeSpan.Parse(tiem) <= hours2Alert)
                    {
                        List<tblPlayerRegisteredToGame> playersRegistered2Game = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == g.GameSerialNum && x.WaitOrNot == false).ToList();

                        foreach (tblPlayerRegisteredToGame p in playersRegistered2Game)
                        {
                            tblPlayer player2Send = db.tblPlayers.Where(x => x.Email.ToLower() == p.EmailPlayer.ToLower()).FirstOrDefault();
                            if (player2Send != null)
                            {
                                string expoToken = player2Send.TokenNotfication;
                                if (expoToken != null)
                                {
                                    string title = "Hi " + player2Send.FirstName + " !";
                                    string body = "Dont forget your game today !\nEnjoy !";
                                    if (p.BringItems != null)
                                        body = "Dont forget your game today!\n You have to bring " + p.BringItems + " for the game ! \nEnjoy !";

                                    // Create POST data and convert it to a byte array.  
                                    var objectToSend = new
                                    {
                                        to = "ExponentPushToken[TWILcbNbWaI3-tH5kUiwUw]",
                                        title = title,
                                        body = body,
                                        //badge = pnd.badge,
                                        data = new { name = "TodaysGame", T_SerialNum = g.TeamSerialNum, G_SerialNum = g.GameSerialNum, indexGame = -1 }
                                        //new { name = "nir", grade = 100 }
                                    };
                                    tblPlayer.SendPushNotification(objectToSend);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void AlertManagerAboutMissingPlayers()
        {

            FootHoodDBContext db = new FootHoodDBContext();

            List<tblGame> games = db.tblGames.Where(x => DbFunctions.DiffDays(DateTime.Now, x.GameDate) <= 2).ToList();
            foreach (tblGame g in games)
            {
                int totalPlayers = g.NumOfPlayersInTeam * g.NumOfTeams;
                int numberOfRegistered = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == g.GameSerialNum && x.WaitOrNot == false).Count();
                if (totalPlayers > numberOfRegistered)
                {
                    tblTeam theTeam = db.tblTeams.Where(x => x.TeamSerialNum == g.TeamSerialNum).FirstOrDefault();
                    if (theTeam.EmailManager != null)
                    {
                        string expoToken = db.tblPlayers.Where(x => x.Email.ToLower() == theTeam.EmailManager.ToLower()).Select(a => a.TokenNotfication).FirstOrDefault();
                        if (expoToken != null)
                        {
                            string title = "Please Notice !";
                            string body = "Your game in team \"" + theTeam.TeamName + "\" needs " + (totalPlayers - numberOfRegistered) + " more players! \nPlease go and add more players.";
                            if (DbFunctions.DiffDays(DateTime.Now, g.GameDate) <= 1)
                            {
                                title = "Less than 24 for your game !";
                                body = "We recommand you to active the Smart Search to complete your players";
                            }

                            // Create POST data and convert it to a byte array.  
                            var objectToSend = new
                            {
                                to = "ExponentPushToken[TWILcbNbWaI3-tH5kUiwUw]",
                                title = title,
                                body = body,
                                //badge = pnd.badge,
                                data = new { name = "ManagerAlert", T_SerialNum = theTeam.TeamSerialNum, G_SerialNum = g.GameSerialNum, indexGame = -1 }
                                //new { name = "nir", grade = 100 }
                            };
                            tblPlayer.SendPushNotification(objectToSend);
                        }
                    }
                }
            }

        }


    }
}