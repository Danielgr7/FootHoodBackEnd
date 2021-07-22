using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web.Http;
using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.SettingsFunctions;
using DATA.EF;

namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/settings")]
    public class SettingsController : ApiController
    {

        [HttpPost]
        [Route("AddFeedback")]
        // POST api/<controller>
        public IHttpActionResult AddFeedback([FromBody] FeedbackDTO fb)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                string firstName = db.tblPlayers.Where(x => x.Email.ToLower() == fb.EmailPlayer.ToLower()).Select(a => a.FirstName).FirstOrDefault();
                if (firstName != null)
                {
                    tblFeedback newFeedback = new tblFeedback();
                    DateTime createdTime = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day, DateTime.Now.Hour,
                    DateTime.Now.Minute, DateTime.Now.Second);

                    newFeedback.FeedbackContext = fb.FeedbackContext;
                    newFeedback.EmailPlayer = fb.EmailPlayer;
                    newFeedback.FeedbackSendDate = createdTime;
                    newFeedback.FeedBackSubject = fb.FeedBackSubject;

                    db.tblFeedbacks.Add(newFeedback);
                    db.SaveChanges();

                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("FootHood.App@gmail.com", "foothood2021"),
                        EnableSsl = true
                    };

                    //client.Send(from,recipients,subject,body)
                    client.Send("FootHood.App@gmail.com",
                        //$"maayanstudent@gmail.com",
                        $"FootHood.App@gmail.com",
                        $"Costumer Feedback - {fb.FeedBackSubject} - {fb.EmailPlayer}",
                        $"Hello my name is {firstName}\nMy email: {fb.EmailPlayer}\n\n" +
                        $"\n{fb.FeedbackContext}");
                    return Ok("The Feedback Has Been sent Succesfully");
                }
                return Ok("Something went wrong, the email is not exsit !");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }



        [HttpPost]
        [Route("ChangePersonalDetails")]
        public IHttpActionResult ChangePersonalDetails([FromBody] ChangePersonalDetailsDTO details)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblPlayer thePlayer = db.tblPlayers.Where(x => x.Email.ToLower() == details.Email.ToLower()).FirstOrDefault();
                if (thePlayer != null)
                {
                    if (details.FirstName != null && thePlayer.FirstName.ToLower() != details.FirstName.ToLower())
                        thePlayer.FirstName = details.FirstName;

                    if (details.LastName != null && thePlayer.LastName.ToLower() != details.LastName.ToLower())
                        thePlayer.LastName = details.LastName;

                    if (details.Phone != 0 && thePlayer.Phone != details.Phone)
                        thePlayer.Phone = details.Phone;

                    if (thePlayer.Gender != details.Gender)
                        thePlayer.Gender = details.Gender;

                    if (details.PlayerCity != null && thePlayer.PlayerCity != details.PlayerCity)
                        thePlayer.PlayerCity = details.PlayerCity;

                    if (details.LatitudeHomeCity != null && thePlayer.LatitudeHomeCity != details.LatitudeHomeCity)
                        thePlayer.LatitudeHomeCity = details.LatitudeHomeCity;

                    if (details.LongitudeHomeCity != null && thePlayer.LongitudeHomeCity != details.LongitudeHomeCity)
                        thePlayer.LongitudeHomeCity = details.LongitudeHomeCity;

                    if (details.DateOfBirth != null && thePlayer.DateOfBirth != details.DateOfBirth)
                        thePlayer.DateOfBirth = details.DateOfBirth;

                    if (details.PlayerPicture != null && thePlayer.PlayerPicture != details.PlayerPicture)
                        thePlayer.PlayerPicture = details.PlayerPicture;

                    if (thePlayer.StrongLeg != details.StrongLeg)
                        thePlayer.StrongLeg = details.StrongLeg;

                    if (details.Height >= 100 && thePlayer.Height != details.Height && details.Height <= 300)
                        thePlayer.Height = details.Height;

                    if (details.Stamina != 0 && thePlayer.Stamina != details.Stamina)
                        thePlayer.Stamina = details.Stamina;

                    if (details.PreferredRole != null && thePlayer.PreferredRole != details.PreferredRole)
                        thePlayer.PreferredRole = details.PreferredRole;

                    if (details.DistanceOfInvites != 0 && thePlayer.DistanceOfInvites != details.DistanceOfInvites)
                        thePlayer.DistanceOfInvites = details.DistanceOfInvites;

                    db.SaveChanges();

                    PlayerDTO newPlayer = new PlayerDTO()
                    {
                        FirstName = thePlayer.FirstName,
                        LastName = thePlayer.LastName,
                        Email = thePlayer.Email,
                        Phone = thePlayer.Phone,
                        Gender = thePlayer.Gender,
                        PlayerCity = thePlayer.PlayerCity,
                        DateOfBirth = thePlayer.DateOfBirth,
                        PlayerPicture = thePlayer.PlayerPicture,
                        StrongLeg = thePlayer.StrongLeg,
                        Height = thePlayer.Height,
                        Stamina = thePlayer.Stamina,
                        PreferredRole = thePlayer.PreferredRole,
                        OverallRating = thePlayer.OverallRating,
                        LatitudeHomeCity = thePlayer.LatitudeHomeCity,
                        LongitudeHomeCity = thePlayer.LongitudeHomeCity,
                        DistanceOfInvites = thePlayer.DistanceOfInvites,
                        PreferredLocation = thePlayer.PreferredLocation,
                    };
                    return Ok(newPlayer);
                }
                return Content(HttpStatusCode.OK, "Something went wrong with the details, \nPlease try again ! ");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }



        [HttpPost]
        [Route("ChangePasscode")]
        public IHttpActionResult ChangePasscode([FromBody] ChangePasswordDTO details)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblPlayer thePlayer = db.tblPlayers.Where(x => x.Email.ToLower() == details.Email.ToLower()).FirstOrDefault();
                if (thePlayer != null)
                {
                    if (thePlayer.Passcode == details.Passcode)
                    {
                        thePlayer.Passcode = details.NewPasscode;
                        db.SaveChanges();
                        return Ok("Password has changed succefully !");
                    }
                    return Ok("Something wrong with your details\nPleases try again !");

                }
                return Content(HttpStatusCode.OK, "Email user is incorrect !");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


    }


}