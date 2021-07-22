using BackEnd___FootHood.DTO.Jarvis;
using DATA.EF;
using System;
using System.Net;
using System.Web.Http;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.GameFunctions;
using System.Data.Entity;

namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/jarvis")]

    public class JarvisController : ApiController
    {


        // Get api/<controller>
        [HttpPost]
        [Route("Jarvis_GetHotGames")]
        public IHttpActionResult Jarvis_GetHotGames([FromBody] UserHotGameDTO user)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                if (db.tblPlayers.Where(x => x.Email.ToLower() == user.EmailPlayer.ToLower()).FirstOrDefault() != null)
                {
                    List<tblHotGamesMatch> hotGamesMatches = db.tblHotGamesMatches.Where(x => x.EmailPlayer.ToLower() == user.EmailPlayer.ToLower()).ToList();
                    List<GameDTO> hotGames = new List<GameDTO>();

                    foreach (tblHotGamesMatch hg in hotGamesMatches)
                    {
                        tblGame g2Push = db.tblGames.Where(x => x.GameSerialNum == hg.GameSerialNum && x.GameDate >= DateTime.Today).FirstOrDefault();
                        if (g2Push != null)
                        {
                            hotGames.Add(new GameDTO()
                            {
                                GameSerialNum = g2Push.GameSerialNum,
                                NumOfTeams = g2Push.NumOfTeams,
                                NumOfPlayersInTeam = g2Push.NumOfPlayersInTeam,
                                GameLocation = g2Push.GameLocation,
                                GameDate = g2Push.GameDate,
                                GameTime = g2Push.GameTime,
                                LastRegistrationDate = g2Push.LastRegistrationDate,
                                LastRegistrationTime = g2Push.LastRegistrationTime,
                                AvgPlayerAge = tblGames.calcAvgAgeInGame(db, g2Push.GameSerialNum),
                                AvgPlayerRating = tblGames.calcAvgRankInGame(db, g2Push.GameSerialNum),
                                TeamSerialNum = g2Push.TeamSerialNum,
                            });
                        }
                    }
                    return Content(HttpStatusCode.OK, hotGames);
                }
                return Content(HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // Get api/<controller>
        [HttpPost]
        [Route("Jarvis_FindPlayers4Game")]
        public IHttpActionResult Jarvis_FindPlayers4Game([FromBody] FindPlayer4GameDTO gameDetailes)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblGame theGame = db.tblGames.Where(x => x.GameSerialNum == gameDetailes.GameSerialNum).FirstOrDefault();
                if (theGame != null)
                {
                    if (theGame.FindPlayersActive == false)
                    {
                        int numOfMatches = 0;
                        double oneKM = 0.0941;
                        double rankScore = 0;
                        double ageScore = 0;
                        double totalMatchScore = 0;
                        //double distanceScore = 0;

                        List<tblPlayerInTeam> playerInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == gameDetailes.TeamSerialNum).ToList();

                        DateTime today = DateTime.Today;
                        List<tblPlayer> players = db.tblPlayers.Where(x =>

                        //Age -
                        today.Year - x.DateOfBirth.Year >= gameDetailes.AvgPlayerAge - (int)(x.DateOfBirth.Year / 10) - 1
                        && today.Year - x.DateOfBirth.Year <= gameDetailes.AvgPlayerAge + (int)(x.DateOfBirth.Year / 10) + 1

                          //Rating
                          && (x.OverallRating >= gameDetailes.AvgPlayerRating - 5 - theGame.NumOfPlayersInTeam
                          && x.OverallRating <= gameDetailes.AvgPlayerRating + 5 + theGame.NumOfPlayersInTeam)

                          //Distance - Latitude
                          && (x.LatitudeHomeCity >= gameDetailes.LatitudeGameLoc - oneKM * x.DistanceOfInvites
                          && x.LatitudeHomeCity <= gameDetailes.LatitudeGameLoc + oneKM * x.DistanceOfInvites)

                          //Distance - LongTitude
                          && (x.LongitudeHomeCity >= gameDetailes.LongitudeGameLoc - oneKM * x.DistanceOfInvites
                          && x.LongitudeHomeCity <= gameDetailes.LongitudeGameLoc + oneKM * x.DistanceOfInvites)).ToList();

                        List<PlayerDTO> matchingPlayers = new List<PlayerDTO>();
                        foreach (tblPlayer p in players)
                        {
                            if (!playerInTeam.Any(x => x.EmailPlayer.ToLower() == p.Email.ToLower()))
                            {
                                if ((double)p.OverallRating < (double)theGame.AvgPlayerRating)
                                    rankScore += (double)p.OverallRating / (double)theGame.AvgPlayerRating;
                                else
                                    rankScore += 1 - (((double)p.OverallRating / (double)theGame.AvgPlayerRating) - 1);

                                if ((double)today.Year - p.DateOfBirth.Year < (double)theGame.AvgPlayerAge)
                                    ageScore += (today.Year - p.DateOfBirth.Year) / (double)theGame.AvgPlayerAge;
                                else
                                    ageScore += 1 - (((today.Year - p.DateOfBirth.Year) / (double)theGame.AvgPlayerAge) - 1);

                                string expoToken = p.TokenNotfication;
                                if (expoToken != null)
                                {
                                    // Create POST data and convert it to a byte array.  
                                    var objectToSend = new
                                    {
                                        to = expoToken,
                                        title = "Hot Game Need You!",
                                        body = "There is a game around you that is looking for new players !\nCheck Your Hot Games And Join Now !",
                                        //badge = pnd.badge,
                                        data = new { name = "hotGames" },
                                        //new { name = "nir", grade = 100 }
                                    };
                                    tblPlayer.SendPushNotification(objectToSend);
                                }
                                numOfMatches++;


                                if (db.tblHotGamesMatches.Where(x => x.EmailPlayer.ToLower() == p.Email.ToLower() && x.GameSerialNum == gameDetailes.GameSerialNum).FirstOrDefault() == null)
                                {
                                    tblHotGamesMatch saveMatch = new tblHotGamesMatch()
                                    {
                                        CreatedMatchTime = DateTime.Today,
                                        EmailPlayer = p.Email,
                                        GameSerialNum = gameDetailes.GameSerialNum
                                    };
                                    db.tblHotGamesMatches.Add(saveMatch);
                                    db.SaveChanges();
                                }
                            }
                        }
                        if (numOfMatches > 0)
                        {
                            rankScore /= numOfMatches;
                            ageScore /= numOfMatches;
                            totalMatchScore = rankScore * ageScore;

                            theGame.FindPlayersActive = true;
                            db.SaveChanges();
                        }
                        return Content(HttpStatusCode.OK, new JarvisResultsOfSearchDTO() { AmountOfResults = numOfMatches, MatchPrecent = (int)(totalMatchScore * 100) });
                    }
                    return Content(HttpStatusCode.OK, "You already activated Jarvis, Please wait for your join requestes");
                }
                return Content(HttpStatusCode.OK, "Someting went wrong with the details of the game");


            }
            catch (DivideByZeroException ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }



        // Get api/<controller>
        [HttpPost]
        [Route("AlertManagerAboutMissingPlayers")]
        public void AlertManagerAboutMissingPlayers()
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



        // Get api/<controller>
        [HttpPost]
        [Route("AlertToRatePlayersAfterGame")]
        public void AlertToRatePlayersAfterGame()
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
                var oneHourAfterAlertHour = g.GameTime + new TimeSpan(02, 00, 00); // צריך להיות (04,00,00
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

    }
}