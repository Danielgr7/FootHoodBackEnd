using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO
{
    public class PlayerDTO
    {
        public string FirstName;
        public string LastName;
        public string Email;
        public int Phone;
        public string Passcode;
        public bool Gender;
        public string PlayerCity;
        public DateTime DateOfBirth; //? maybe need ??
        public string PlayerPicture;
        public bool StrongLeg;
        public int Height;
        public int Stamina;
        public string PreferredRole;
        public string PreferredLocation;
        public int? OverallRating;
        public int? LastSearchRadius;
        public int OTP;
        public double? LatitudeHomeCity;
        public double? LongitudeHomeCity;
        public string TokenNotfication;
        public int DistanceOfInvites;
    }
}