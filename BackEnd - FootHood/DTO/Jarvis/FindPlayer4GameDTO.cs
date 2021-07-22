using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO.Jarvis
{
    public class FindPlayer4GameDTO
    {
        public int TeamSerialNum;
        public int GameSerialNum;
        public int AvgPlayerAge;
        public int AvgPlayerRating;
        public double? LatitudeGameLoc;
        public double? LongitudeGameLoc;
    }
}