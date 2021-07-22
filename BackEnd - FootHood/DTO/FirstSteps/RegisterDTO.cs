using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO
{
    public class RegisterDTO
    {
        public string FirstName;
        public string LastName;
        public string Email;
        public int Phone;   //############################
        public string Passcode;
        public bool Gender;  // 1=male, 0 female
        public string PlayerCity;
        public DateTime DateOfBirth; //maybe need     "?"
        public string PlayerPicture; //############################
        public int Height;
        public bool StrongLeg;  //1 = right,  0 = left
        public int Stamina;
        public string PreferredRole;
        public int OverallRating;
        public float LatitudeHomeCity;
        public float LongitudeHomeCity;
        public string TokenNotfication;
        public int DistanceOfInvites;
    }
}