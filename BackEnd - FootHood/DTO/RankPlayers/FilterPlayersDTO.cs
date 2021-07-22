using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO.RankPlayers
{
    public class FilterPlayersDTO
    {
        public string PlayerCity;
        //public int Radios; 
        public bool Gender;
        public int maxAge;
        public int minAge;
        public int maxOverallRating;
        public int minOverallRating;

        //to use feauter
    //    List<tblPlayers> playersSort db.tblPlayers.Where(x=>x.PlayerCity = sortDetails.PlayerCity && x.Gender==sortDetails.Gender   &&
    //        x./*AGE*/>=sortDetails.maxAge    &&   sortDetails.minAge <=x./*AGE*/  &&     x.OverallRating>=sortDetails.maxOverallRating    &&   sortDetails.minOverallRating <=x.OverallRating).ToList()
    }
}