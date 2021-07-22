using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO.SettingsFunctions
{
    public class ChangePersonalDetailsDTO
    {
        public string FirstName;
        public string LastName;
        public string Email;
        public int Phone;
        public bool Gender;
        public string PlayerCity;
        public DateTime DateOfBirth; //? maybe need ??
        public string PlayerPicture;
        public bool StrongLeg;
        public int Height;
        public int Stamina;
        public string PreferredRole;
        public double? LatitudeHomeCity;
        public double? LongitudeHomeCity;
        public int DistanceOfInvites;

    }
}