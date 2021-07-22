using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO.RankPlayers
{
    public class RankPlayerDTO
    {
        public string EmailofRatingPlayer;
        public string EmailofRatedPlayer;
        public int PowerRating;
        public int AttackRating;
        public int DefenseRating;
        public bool GameOrGenrel;
        public int GameSerialNum;
    }
}