using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.FirstSteps;
using BackEnd___FootHood.DTO.RankPlayers;
using BackEnd___FootHood.DTO.SettingsFunctions;
using DATA.EF;

namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/player")]
    public class PlayerController : ApiController
    {
        // Post api/<controller>/{object}
        [HttpPost]
        [Route("LoginUser")]
        public IHttpActionResult LoginUser([FromBody] LoginUserDTO user)
        {
            try
            {
                tblPlayer player = tblPlayer.CheckIfExist(user.Email, user.Passcode);
                if (player != null)
                {
                    PlayerDTO newPlayer = new PlayerDTO()
                    {
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        Email = player.Email,
                        Phone = player.Phone,
                        //Passcode = player.Passcode,
                        Gender = player.Gender,
                        PlayerCity = player.PlayerCity,
                        DateOfBirth = player.DateOfBirth,
                        PlayerPicture = player.PlayerPicture,
                        StrongLeg = player.StrongLeg,
                        Height = player.Height,
                        Stamina = player.Stamina,
                        PreferredRole = player.PreferredRole,
                        OverallRating = player.OverallRating,
                        LatitudeHomeCity = player.LatitudeHomeCity,
                        LongitudeHomeCity = player.LongitudeHomeCity
                    };
                    return Ok(newPlayer);
                }
                return Content(HttpStatusCode.BadRequest, "The user you are trying to login does not exist");
                //return BadRequest("The user you are trying to login does not exist");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // PUT api/<controller>/5
        [HttpPut]
        [Route("ChangePassCode")]
        public IHttpActionResult ChangePassCode([FromBody] NewPassCodeDTO player)
        {
            try
            {
                bool verify = tblPlayer.ChangePassCode(player.OTP, player.Passcode);
                if (verify)
                    return Ok("The PassCode has Changed");
                else
                    return BadRequest("The otp isn't correct");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // POST api/values
        [HttpPost]
        [Route("RequestOTP")]
        public IHttpActionResult RequestOTP([FromBody] OTPRequestDTO player)
        {
            try
            {
                Random rnd = new Random();
                bool verified = false;
                int otp = rnd.Next(100000, 999999);
                string name = tblPlayer.Insert_OTP_2DB(player.Email, otp);
                if (name != null & name != "not exist")
                {
                    // email to clint 
                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("FootHood.App@gmail.com", "foothood2021"),
                        EnableSsl = true
                    };
                    client.Send("FootHood.App@gmail.com",
                        $"{player.Email}",
                        $"FootHood - Reset Password",
                        $"Hello {name} ," +
                        $"\nHope you take care of your passwords better 💚😎 " +
                        $"\nHere is the code to reset your password, switch back to FootHood App and enter: {otp}");
                    verified = true;
                }
                if (name == "not exist")
                {
                    return Content(HttpStatusCode.BadRequest, "The Email Does not exist in DataBase");
                    //return BadRequest("The Email Does not exist in DataBase");
                }

                return Ok(verified);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("Register")]
        public IHttpActionResult Register([FromBody] RegisterDTO player)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                if (!tblPlayer.CheckIfRegisterd(db, player.Email))
                {
                    tblPlayer newPlayer = new tblPlayer()
                    {
                        FirstName = player.FirstName,
                        LastName = player.LastName,
                        Email = player.Email,
                        Phone = player.Phone,
                        Passcode = player.Passcode,
                        Gender = player.Gender,
                        PlayerCity = player.PlayerCity,
                        DateOfBirth = player.DateOfBirth,
                        PlayerPicture = player.PlayerPicture,
                        StrongLeg = player.StrongLeg,
                        Height = player.Height,
                        Stamina = player.Stamina,
                        PreferredRole = player.PreferredRole,
                        OverallRating = 75,
                        LatitudeHomeCity = player.LatitudeHomeCity,
                        LongitudeHomeCity = player.LongitudeHomeCity,
                        TokenNotfication = player.TokenNotfication,
                        DistanceOfInvites = player.DistanceOfInvites
                    };
                    db.tblPlayers.Add(newPlayer);
                    db.SaveChanges();
                    return Created(new Uri(Request.RequestUri.AbsoluteUri + player.FirstName), player);
                }
                else
                    return BadRequest("You have already registered with this email.");

            }
            catch (Exception ex) { return Content(HttpStatusCode.BadRequest, ex); }
        }


        [HttpGet]
        [Route("GetPlayers")]
        public IHttpActionResult GetPlayers()
        {
            try
            {
                //WebApiApplication.StartTimer();
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblPlayer> players = db.tblPlayers.ToList();
                List<PlayerDTO> newListOfPlayers = new List<PlayerDTO>();
                foreach (tblPlayer p in players)
                {
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
                        OverallRating = p.OverallRating,
                        PreferredLocation = p.PreferredLocation,
                        LastSearchRadius = p.LastSearchRadius,
                        LatitudeHomeCity = p.LatitudeHomeCity,
                        LongitudeHomeCity = p.LongitudeHomeCity,
                        TokenNotfication = p.TokenNotfication
                    });
                }

                return Created(new Uri(Request.RequestUri.AbsoluteUri), newListOfPlayers);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("GetWeightedRate")]
        public IHttpActionResult GetWeightedRate([FromBody] WeightedRateDTO player)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblRating> rates = db.tblRatings.Where(x => x.EmailofRatingPlayer == player.EmailofRatingPlayer).ToList();

                int numOfRates = rates.Count;
                int attack = 0;
                int defense = 0;
                int power = 0;

                if (rates.Count == 0)
                {
                    numOfRates = 1;
                    attack += 75;
                    defense += 75;
                    power += 75;
                }
                else
                {
                    foreach (tblRating r in rates)
                    {
                        attack += r.AttackRating;
                        defense += r.DefenseRating;
                        power += r.PowerRating;
                    }
                }


                WeightedRateDTO weightedRate = new WeightedRateDTO()
                {
                    EmailofRatingPlayer = player.EmailofRatingPlayer,
                    AttackRating = attack / numOfRates,
                    DefenseRating = defense / numOfRates,
                    PowerRating = power / numOfRates
                };


                return Created(new Uri(Request.RequestUri.AbsoluteUri), weightedRate);
            }
            catch (DivideByZeroException ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Route("PushNotificationToken")]
        public IHttpActionResult PushNotificationToken([FromBody] UpdateExpoTokenPlayerDTO player)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                //tblPlayer thePlayer = db.tblPlayers.Where(x => x.Email == player.Email).FirstOrDefault().TokenNotfication= player.TokenNotfication;
                tblPlayer p = db.tblPlayers.Where(x => x.Email == player.Email).FirstOrDefault();
                if (p != null)
                {
                    p.TokenNotfication = player.TokenNotfication;
                    db.SaveChanges();
                    PlayerDTO playerDetailes = new PlayerDTO()
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
                        OverallRating = p.OverallRating,
                        PreferredLocation = p.PreferredLocation,
                        LastSearchRadius = p.LastSearchRadius,
                        LatitudeHomeCity = p.LatitudeHomeCity,
                        LongitudeHomeCity = p.LongitudeHomeCity,
                        TokenNotfication = p.TokenNotfication
                    };

                    return Created(new Uri(Request.RequestUri.AbsoluteUri), playerDetailes);

                }
                return Created(new Uri(Request.RequestUri.AbsoluteUri), "Something went wrong, the email doesn't exist in the db");

            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("CheckIfExist")]
        public IHttpActionResult CheckIfExist([FromBody] LoginUserDTO user)
        {
            try
            {
                tblPlayer player = tblPlayer.CheckIfExist(user.Email, user.Passcode);
                if (player != null)
                    return Content(HttpStatusCode.OK, true);
                return Content(HttpStatusCode.OK, false);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("LogOut")]
        public IHttpActionResult LogOut([FromBody] LoginUserDTO user)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                db.tblPlayers.Where(x => x.Email.ToLower() == user.Email.ToLower()).FirstOrDefault().TokenNotfication = null;
                db.SaveChanges();
                return Content(HttpStatusCode.OK, "Token has been removed");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        
    }
}