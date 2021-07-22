using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DATA.EF
{
    public partial class tblPlayer
    {
        public static tblPlayer CheckIfExist(string email, string passCode)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblPlayer player = db.tblPlayers.Where(x => x.Email.ToLower() == email).SingleOrDefault();
                //tblPlayer playerPassCode = db.tblPlayers.Where(x => x.Passcode == passCode).SingleOrDefault(); 
                if (player != null)
                {
                    if (player.Passcode == passCode)
                        return player;
                }
                return null;
            }
            catch (Exception ex)
            {
                HttpStatusCode.BadRequest.ToString(ex.Message);
                return null;
            }
        }

        public static bool ChangePassCode(int otp, string passcode)
        {
            //check if otp is like in db
            bool verify = false;
            FootHoodDBContext db = new FootHoodDBContext();
            tblPlayer player = db.tblPlayers.Where(x => x.OTP == otp).SingleOrDefault();
            if (otp == player.OTP)
            {
                //if get here than change passCode
                player.Passcode = passcode;

                //remove the otp after changing passCode
                player.OTP = -1;
                db.SaveChanges();
                verify = true;
            }
            return verify;
        }

        public static string Insert_OTP_2DB(string email, int otp)
        {
            string playerName = null;
            FootHoodDBContext db = new FootHoodDBContext();
            tblPlayer player = db.tblPlayers.Where(x => x.Email.ToLower() == email).SingleOrDefault();
            if (email.ToLower() == player.Email)
            {
                playerName = player.FirstName + " " + player.LastName;
                player.OTP = otp;
                db.SaveChanges();
            }
            else
            {
                playerName = "not exist";
                return playerName;
            }
            return playerName;
        }

        public static bool CheckIfRegisterd(FootHoodDBContext db, string email)
        {
            tblPlayer exist = db.tblPlayers.Where(x => x.Email.ToLower() == email).FirstOrDefault();
            if (exist == null)
                return false;
            else
                return true;
        }

        public static void SendPushNotification(object objectToSend)
        {
            // Create a request using a URL that can receive a post.   
            WebRequest request = WebRequest.Create("https://exp.host/--/api/v2/push/send");

            // Set the Method property of the request to POST.  
            request.Method = "POST";



            string postData = new JavaScriptSerializer().Serialize(objectToSend);

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.  
            request.ContentType = "application/json";

            // Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length;

            // Get the request stream.  
            Stream dataStream = request.GetRequestStream();

            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.  
            dataStream.Close();

            // Get the response.  
            WebResponse response = request.GetResponse();

            // Display the status.  
            string returnStatus = ((HttpWebResponse)response).StatusDescription;

            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            dataStream = response.GetResponseStream();

            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);

            // Read the content.  
            string responseFromServer = reader.ReadToEnd();

            // Display the content.  
            //Console.WriteLine(responseFromServer);

            // Clean up the streams.  
            reader.Close();
            dataStream.Close();
            response.Close();

            //return "success:) --- " + responseFromServer + ", " + returnStatus;
        }

        public static void CalcTotalRank4Player(FootHoodDBContext db, tblPlayer playerRated)
        {
            try
            {
                int numOfRates = 0;
                int attack = 0;
                int defense = 0;
                int power = 0;

                List<tblRating> allRates = db.tblRatings.Where(x => x.EmailofRatedPlayer.ToLower() == playerRated.Email.ToLower() && x.GameOrGenrel == false).ToList();
                if (allRates.Count > 0)
                {
                    numOfRates = allRates.Count;
                    foreach (tblRating r in allRates)
                    {
                        attack += r.AttackRating;
                        defense += r.DefenseRating;
                        power += r.PowerRating;
                    }
                    playerRated.OverallRating = (attack + defense + power) / 3;
                    db.SaveChanges();
                }
                int last3RatedGames = Last3RatedGames(db, playerRated);
                if (last3RatedGames > 0)
                {
                    playerRated.OverallRating = (playerRated.OverallRating + last3RatedGames) / 2;
                }
                db.SaveChanges();
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }

        public static int Last3RatedGames(FootHoodDBContext db, tblPlayer playerRated)
        {
            try
            {
                int[] gamesPlayerRegistered = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == playerRated.Email.ToLower() && x.WaitOrNot == false).Select(a => a.GameSerialNum).ToArray();
                if (gamesPlayerRegistered.Length >= 3)
                {
                    int numOfRates = 0;
                    int attack = 0;
                    int defense = 0;
                    int power = 0;
                    bool breakLoop = false;
                    int numOfTotalRates = 0;
                    int total = 0;

                    int[] last3GamesPlayed = new int[3];
                    int ctrGamesRated = 0;

                    Array.Sort(gamesPlayerRegistered);
                    Array.Reverse(gamesPlayerRegistered);

                    for (int i = 0; i < gamesPlayerRegistered.Length; i++)
                    {
                        attack = 0;
                        defense = 0;
                        power = 0;
                        var currentgamesPlayerRegistered = gamesPlayerRegistered[i];
                        if (ctrGamesRated >= 3)
                            break;
                        //if (ctrGamesRated >= 3 || breakLoop == true)
                        //break

                        int gamePlayed = db.tblGames.Where(x => x.GameSerialNum == currentgamesPlayerRegistered && x.GameDate <= DateTime.Today).Select(a => a.GameSerialNum).FirstOrDefault();
                        if (gamePlayed > 0)
                        {
                            numOfRates = 0;
                            int[] ratesOfGame = db.tblRatingOfGames.Where(x => x.GameSerialNum == currentgamesPlayerRegistered).Select(a => a.RatingSerialNum).ToArray();

                            for (int j = 0; j < ratesOfGame.Length; j++)
                            {
                                var currentRateOfGame = ratesOfGame[j];
                                ctrGamesRated++;

                                tblRating r = db.tblRatings.Where(x => x.RatingSerialNum == currentRateOfGame && x.EmailofRatedPlayer.ToLower() == playerRated.Email.ToLower()).FirstOrDefault();
                                if (r != null)
                                {
                                    attack += r.AttackRating;
                                    defense += r.DefenseRating;
                                    power += r.PowerRating;
                                    numOfRates++;
                                    numOfTotalRates++;
                                }
                                //else
                                //{
                                //    breakLoop = true;
                                //}
                            }
                            attack /= numOfRates;
                            defense /= numOfRates;
                            power /= numOfRates;
                            total += (attack + defense + power) / 3;
                        }

                    }
                    total /= numOfTotalRates;
                    return total;
                }
                return 0;
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
        }
    }
}
