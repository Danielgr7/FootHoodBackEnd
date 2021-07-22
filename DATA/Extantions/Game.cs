using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DATA.EF
{
    public partial class tblGames
    {
        public static bool CheckIf_InWaitingList(FootHoodDBContext db, int gameserialno)
        {
            var gamedetail = db.tblGames.Where(x => x.GameSerialNum == gameserialno).FirstOrDefault();
            //var totalPlayers = gamedetail.NumOfPlayersInTeam * gamedetail.NumOfTeams;
            //var availableplayers = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == gameserialno).Count();
            var totalPlayers = gamedetail.NumOfPlayersInTeam * gamedetail.NumOfTeams;
            var availableplayers = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == gameserialno && x.WaitOrNot == false).Count();
            if (availableplayers < totalPlayers)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static int calcAvgAgeInGame(FootHoodDBContext db, int gameSerialNum)
        {
            int calcAvgAge = 0;

            DateTime today = DateTime.Today;
            List<tblPlayerRegisteredToGame> playersInGame = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == gameSerialNum).ToList();

            foreach (tblPlayerRegisteredToGame p in playersInGame)
            {
                if (p.WaitOrNot == false)
                {
                    DateTime birthDatePlayer = db.tblPlayers.Where(x => x.Email.ToLower() == p.EmailPlayer.ToLower()).Select(a => a.DateOfBirth).FirstOrDefault();
                    int age = today.Year - birthDatePlayer.Year;
                    calcAvgAge += age;
                }

            }
            if (playersInGame.Count > 0)
                calcAvgAge = calcAvgAge / playersInGame.Count;

            return calcAvgAge;
        }

        public static int calcAvgRankInGame(FootHoodDBContext db, int gameSerialNum)
        {
            int calcAvgRank = 0;

            List<tblPlayerRegisteredToGame> playersInGameRank = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == gameSerialNum && x.WaitOrNot != true).ToList();

            foreach (tblPlayerRegisteredToGame p in playersInGameRank)
            {
                int playerRank = (int)db.tblPlayers.Where(x => x.Email.ToLower() == p.EmailPlayer.ToLower()).Select(a => a.OverallRating).FirstOrDefault();
                calcAvgRank += playerRank;
            }
            if (playersInGameRank.Count > 0)
                calcAvgRank /= playersInGameRank.Count;

            return calcAvgRank;
        }
    }
}