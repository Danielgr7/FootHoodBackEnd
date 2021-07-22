using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.EF
{
    public partial class tblPlayerRegisteredToGame
    {

        public static void DividePlayers2Groups(FootHoodDBContext db, int gameSerialNum)
        {
            //מוצא את כל השחקנים שנרשמו למשחק ואינם נמצאים ברשימת המתנה
            List<tblPlayerRegisteredToGame> playersThatRegistred = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == gameSerialNum && x.WaitOrNot == false).ToList();

            //מייצר רשימת שתאסוף את כל הפרטים של השחקנים שנרשמו למשחק
            List<tblPlayer> playersWithDetails = new List<tblPlayer>();
            foreach (tblPlayerRegisteredToGame pr in playersThatRegistred)
            {
                //אם המשתמש אכן קיים במאגר - הכנסה לרשימה של השחקנים שנרשמו יחד עם כל הפרטים שלהם
                tblPlayer p = db.tblPlayers.Where(x => x.Email.ToLower() == pr.EmailPlayer.ToLower()).FirstOrDefault();
                if (p != null)
                    playersWithDetails.Add(p);
            }
            //מביא את השחקן שמנסה להצטרף לקבוצה
            //tblPlayer thePlayer = db.tblPlayers.Where(x => x.Email.ToLower() == emailPlayer.ToLower()).FirstOrDefault();
            //playersWithDetails.Add(thePlayer);

            //ממיין את כל השחקנים לפי הדירוג שלהם
            playersWithDetails = playersWithDetails.OrderByDescending(x => x.OverallRating).ToList();

            tblGame theGame = db.tblGames.Where(x => x.GameSerialNum == gameSerialNum).FirstOrDefault();
            if (theGame != null)
            {
                bool breakTheLoop = false;
                int ctr = 0;
                for (int p = 0; p < playersWithDetails.Count; ctr++) //רץ כל כמות השחקנים הנוכחית
                {
                    for (int i = 0; i < theGame.NumOfTeams; i++)//מתחיל לרוץ קדימה במערך הקבוצות
                    {
                        if (p >= playersWithDetails.Count)//להוסיף משתנה בוליאני
                        {
                            breakTheLoop = true;
                            break;
                        }
                        //קוד להוספת שחקן לקבוצה מסויימת==================================
                        playersThatRegistred.Where(x => x.EmailPlayer.ToLower() == playersWithDetails[p].Email.ToLower() && x.GameSerialNum == gameSerialNum).FirstOrDefault().GroupNumber = i + 1;

                        //db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == playersWithDetails[p].Email.ToLower() && x.GameSerialNum == gameSerialNum).FirstOrDefault().GroupNumber = i + 1;

                         p++;
                        if (i == theGame.NumOfTeams - 1)// בדיקה אם הגענו לסוף המערך הקבוצות במהלך הריצה קדימה
                        {
                            for (int j = i; j >= 0; --j)//הכנסה ברוורס
                            {
                                if (p >= playersWithDetails.Count)//להוסיף משתנה בוליאני
                                {
                                    breakTheLoop = true;
                                    break;
                                }
                                //קוד להוספת שחקן לקבוצה מסויימת=========================
                                playersThatRegistred.Where(x => x.EmailPlayer.ToLower() == playersWithDetails[p].Email.ToLower() && x.GameSerialNum == gameSerialNum).FirstOrDefault().GroupNumber = j + 1;

                                p++;
                            }
                        }
                    }
                    if (breakTheLoop == true)
                        break;
                }
            }
            db.SaveChanges();
        }
    }
}
