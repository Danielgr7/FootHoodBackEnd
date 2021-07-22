using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.RankPlayers;
using DATA.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/RankPlayers")]
    public class RankPlayersController : ApiController
    {

        // GET api/<controller>/{object}
        [HttpGet]
        [Route("Filters")]
        public List<PlayerDTO> Filters([FromBody]FilterPlayersDTO filterDetails)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                //All Players After Filter that the user choosen 
                List<tblPlayer> playersFiltered = db.tblPlayers.Where(x => x.PlayerCity == filterDetails.PlayerCity &&
                 x.Gender == filterDetails.Gender &&
                    (x.DateOfBirth.Year - DateTime.Now.Date.Year) >= filterDetails.maxAge && filterDetails.minAge <=
                    (x.DateOfBirth.Year - DateTime.Now.Date.Year)
                    && x.OverallRating >= filterDetails.maxOverallRating &&
                    filterDetails.minOverallRating <= x.OverallRating).ToList();
                
                // passing 2 dto List 
                List<PlayerDTO> newListOfPlayers = new List<PlayerDTO>();
                foreach (tblPlayer p in playersFiltered)
                {
                    //Every loop creating a new obj off player DTO and Push to ListDTO
                    newListOfPlayers.Add(new PlayerDTO()
                    {
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Email = p.Email,
                        Phone = p.Phone,
                        Gender = p.Gender,
                        PlayerCity = p.PlayerCity,
                        DateOfBirth = p.DateOfBirth,
                        PlayerPicture = p.PlayerPicture,
                        StrongLeg = p.StrongLeg,
                        Height = p.Height,
                        Stamina = p.Stamina,
                        PreferredRole = p.PreferredRole,
                        PreferredLocation = p.PreferredLocation,
                        OverallRating = p.OverallRating,
                        LastSearchRadius = p.LastSearchRadius,
                        LatitudeHomeCity = p.LatitudeHomeCity,
                        LongitudeHomeCity = p.LongitudeHomeCity,
                        TokenNotfication = p.TokenNotfication
                    });
                }
                return newListOfPlayers;
            }
            catch (Exception ex)
            {
                //Add error 
                return null;
            }

        }

        // Post api/<controller>/{object}
        [HttpPost]
        [Route("RankPlayer")]
        public IHttpActionResult RankPlayer([FromBody]RankPlayerDTO rate)
        {
            FootHoodDBContext db = new FootHoodDBContext();

            try
            {
                tblPlayer playerRated = db.tblPlayers.Where(x => x.Email.ToLower() == rate.EmailofRatedPlayer.ToLower()).FirstOrDefault();
                if (playerRated!=null && tblPlayer.CheckIfRegisterd(db,rate.EmailofRatingPlayer))
                {
                    tblRating checkIfRatedByThisPlayer = db.tblRatings.Where(x => x.EmailofRatedPlayer.ToLower() == rate.EmailofRatedPlayer.ToLower() && x.EmailofRatingPlayer.ToLower() == rate.EmailofRatingPlayer.ToLower()).FirstOrDefault();
                    if (checkIfRatedByThisPlayer !=null && checkIfRatedByThisPlayer.GameOrGenrel==false && (DateTime.Today - checkIfRatedByThisPlayer.RatingDate).Days <90)
                    {
                        int daysLeft = (DateTime.Today - checkIfRatedByThisPlayer.RatingDate).Days;

                        return Content(HttpStatusCode.OK, $"You have rate this player last time less than 3 month.\nNext time you will able to rate {playerRated.FirstName} in {90-daysLeft} days ");
                    }

                    int attack = rate.AttackRating;
                    int defense = rate.DefenseRating;
                    int power = rate.PowerRating;
                    if (attack<=30)
                        attack = 50;
                    if (defense <= 30)
                        defense = 50;
                    if (power <= 30)
                        power = 50;
                   
                    db.tblRatings.Add(new tblRating()
                    {
                        EmailofRatedPlayer = rate.EmailofRatedPlayer,
                        AttackRating = attack,
                        DefenseRating = defense,
                        PowerRating = power,
                        EmailofRatingPlayer = rate.EmailofRatingPlayer,
                        GameOrGenrel = false,
                        RatingDate = DateTime.Today,
                    });
                    db.SaveChanges();

                    tblPlayer.CalcTotalRank4Player(db, playerRated);

                }
                else
                {
                    return Content(HttpStatusCode.OK, "One Of The Emails incorrect !");

                }
                return Content(HttpStatusCode.OK, "The rank added succefully !");

            }
            catch (DbUpdateConcurrencyException ex)
            {
                var ctx = ((IObjectContextAdapter)db).ObjectContext;
                foreach (var et in ex.Entries)
                {
                    //client win
                    //ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, et.Entity);

                    //Store Win
                    ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, et.Entity);
                }
                db.SaveChanges();
                return RankPlayer(rate);
            }
            catch (Exception ex)
            {
                //Add error 
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }


        // Post api/<controller>/{object}
        [HttpPost]
        [Route("RankPlayerAfterGame")]
        public IHttpActionResult RankPlayerAfterGame([FromBody]RankPlayerDTO rate)
        {
            FootHoodDBContext db = new FootHoodDBContext();
            try
            {
                tblPlayer playerRated = db.tblPlayers.Where(x => x.Email.ToLower() == rate.EmailofRatedPlayer.ToLower()).FirstOrDefault();
                
                //בודק אם השחקן שמדרג אכן שיחק במשחק
                var playerRating = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == rate.EmailofRatingPlayer.ToLower() && x.GameSerialNum == rate.GameSerialNum && x.WaitOrNot == false).FirstOrDefault();
                
                //בודק אם השחקן קיים במערכת/ ורשום למשחק כשהוא לא ברשימת ההמתנה
                if (playerRated != null && tblPlayer.CheckIfRegisterd(db, rate.EmailofRatingPlayer) && rate.GameSerialNum>0 && db.tblPlayerRegisteredToGames.Where(x=>x.EmailPlayer.ToLower()==playerRated.Email.ToLower()&& x.GameSerialNum== rate.GameSerialNum && x.WaitOrNot==false).FirstOrDefault()!=null && playerRating!=null)
                {
                    bool isAlReadyReatedByThisPlayer = false;
                    int[] listOfRatesSerialNums = db.tblRatings.Where(x => x.EmailofRatedPlayer.ToLower() == rate.EmailofRatedPlayer.ToLower() && x.EmailofRatingPlayer.ToLower() == rate.EmailofRatingPlayer.ToLower()&& x.GameOrGenrel==true).Select(a => a.RatingSerialNum).ToArray();
                    foreach (int  r in listOfRatesSerialNums)
                    {
                        if (db.tblRatingOfGames.Where(x=>x.RatingSerialNum == r&& x.GameSerialNum== rate.GameSerialNum).SingleOrDefault() != null )
                        {
                            isAlReadyReatedByThisPlayer = true;
                        }
                    }
                    if (!isAlReadyReatedByThisPlayer)
                    {
                        int attack = rate.AttackRating;
                        int defense = rate.DefenseRating;
                        int power = rate.PowerRating;
                        if (attack <= 30)
                            attack = 50;
                        if (defense <= 30)
                            defense = 50;
                        if (power <= 30)
                            power = 50;

                        tblRating newRate = new tblRating()
                        {
                            EmailofRatedPlayer = rate.EmailofRatedPlayer.ToLower(),
                            AttackRating = attack,
                            DefenseRating = defense,
                            PowerRating = power,
                            EmailofRatingPlayer = rate.EmailofRatingPlayer.ToLower(),
                            GameOrGenrel = true,
                            RatingDate = DateTime.Today,
                        };
                        db.tblRatings.Add(newRate);
                        db.SaveChanges();

                        db.tblRatingOfGames.Add(new tblRatingOfGame()
                        {
                            RatingSerialNum = newRate.RatingSerialNum,
                            GameSerialNum = rate.GameSerialNum
                        });
                        db.SaveChanges();
                        tblPlayer.CalcTotalRank4Player(db, playerRated);
                    }
                    else
                    {
                        return Content(HttpStatusCode.OK, "You Have already rated this player for this game !");
                    }
                }
                else
                {
                    return Content(HttpStatusCode.OK, "One Of The detailes incorrect !");
                }
                return Content(HttpStatusCode.OK, "The rank added succefully !");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var ctx = ((IObjectContextAdapter)db).ObjectContext;
                foreach (var et in ex.Entries)
                {
                    //client win
                    //ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, et.Entity);

                    //Store Win
                    ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, et.Entity);
                }
                db.SaveChanges();
                return RankPlayer(rate);
            }
            catch (Exception ex)
            {
                //Add error 
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }

    }
}