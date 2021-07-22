using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.GameFunctions;
using BackEnd___FootHood.DTO.TeamFunctions;
using DATA.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;


namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/game")]
    public class GameController : ApiController
    {
        // GET api/<controller>/{object}
        [HttpPost]
        [Route("GamesList")]
        public List<GameDTO> GamesList([FromBody]RequestGamesListDTO request)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();

                //All Games that the Team have , by team serial number
                List<tblGame> gamesList = db.tblGames.Where(x => x.TeamSerialNum == request.TeamSerialNum && DbFunctions.DiffDays(DateTime.Now, x.GameDate) >= 0).ToList();

                //passing 2 dto List
                List<GameDTO> newGameList = new List<GameDTO>();

                //loop each game to get details
                foreach (var g in gamesList)
                {
                    //get all equipments serial number each game
                    List<tblEquipmentInGame> equipmentsPerGame = db.tblEquipmentInGames.Where(x => x.GameSerialNum == g.GameSerialNum).ToList();

                    //passing 2 dto List
                    List<EquipmentsInGameDTO> newListEquipmentsInGame = new List<EquipmentsInGameDTO>();
                    foreach (tblEquipmentInGame e in equipmentsPerGame)
                    {
                        //all round add new obj of EquipmentsInGameDTO
                        newListEquipmentsInGame.Add(new EquipmentsInGameDTO()
                        {
                            EquipmentSerialNum = e.EquipmentSerialNum,
                            GameSerialNum = e.GameSerialNum
                        });
                    }

                    //all round add new obj of GameDTO
                    newGameList.Add(new GameDTO()
                    {
                        GameSerialNum = g.GameSerialNum,
                        NumOfTeams = g.NumOfTeams,
                        NumOfPlayersInTeam = g.NumOfPlayersInTeam,
                        GameLocation = g.GameLocation,
                        GameDate = g.GameDate,
                        GameTime = g.GameTime,
                        LastRegistrationDate = g.LastRegistrationDate,
                        LastRegistrationTime = g.LastRegistrationTime,
                        AvgPlayerAge = tblGames.calcAvgAgeInGame(db, g.GameSerialNum),
                        AvgPlayerRating = tblGames.calcAvgRankInGame(db, g.GameSerialNum),
                        //AvgPlayerRating = g.AvgPlayerRating,
                        TeamSerialNum = request.TeamSerialNum,
                        equipmentsInGame = newListEquipmentsInGame, // this is the List of EquipmentsInGameDTO
                        FindPlayersActive = g.FindPlayersActive,
                        GameLatitude = g.GameLatitude,
                        GameLongitude = g.GameLongitude
                    });
                }
                return newGameList;
            }
            catch (Exception ex)
            {
                // to send back error
                return null;
            }
        }


        // POST api/<controller>
        [HttpPost]
        [Route("CreateNewGame")]
        public IHttpActionResult CreateNewGame([FromBody] CreateNewGameDTO CNG)
        {
            //GameDTO game = CNG.game;
            //return Content(HttpStatusCode.OK, game);
            try
            {
                GameDTO game = CNG.game;
                List<EquipmentDTO> equipments = CNG.equipments;
                bool checkIfExist = false;

                FootHoodDBContext db = new FootHoodDBContext();
                List<tblEquipment> allEquipments = db.tblEquipments.ToList();

                //maybe in function
                //tblEquipment equipmentsList = importAllEquipmentsList(db, new tblEquipment());

                //ריצה על כל הציוד שנשלח מהמשתמש 
                foreach (EquipmentDTO e in equipments)
                {
                    checkIfExist = false;
                    checkIfExist = allEquipments.Any(x => x.EquipmentName.ToLower() == e.EquipmentName.ToLower());

                    //דואג לייצר רשימה ייחודית לציוד - כל שם יופיע רק פעם אחת עם מספר מזהה שהמאגר ייצר
                    if (!checkIfExist)
                    {
                        tblEquipment equipment = new tblEquipment();
                        equipment.EquipmentName = e.EquipmentName;
                        db.tblEquipments.Add(equipment);
                    }
                }

                DateTime createdTime = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day, DateTime.Now.Hour,
                    DateTime.Now.Minute, DateTime.Now.Second);

                tblGame newGame = new tblGame();
                newGame.NumOfTeams = game.NumOfTeams;
                newGame.NumOfPlayersInTeam = game.NumOfPlayersInTeam;
                newGame.GameLocation = game.GameLocation;
                newGame.GameDate = game.GameDate;
                newGame.GameTime = game.GameTime;
                newGame.LastRegistrationDate = game.LastRegistrationDate;
                newGame.LastRegistrationTime = game.LastRegistrationTime;
                newGame.AvgPlayerAge = game.AvgPlayerAge;
                newGame.AvgPlayerRating = game.AvgPlayerRating;
                newGame.CreatedTableTime = createdTime;
                newGame.TeamSerialNum = game.TeamSerialNum;
                newGame.GameLatitude = game.GameLatitude;
                newGame.GameLongitude = game.GameLongitude;
                db.tblGames.Add(newGame);
                //שומר את הטבלאות החדשות כדי שיווצרו מספרים מזהים עבור כל טבלה ואוכל להשתמש בהם בהמשך
                db.SaveChanges();

                // hh:mm:ss
                //משיכת מאגר חדש לאחר שינויים
                FootHoodDBContext db_AfterUpdate = new FootHoodDBContext();

                //get the serial number of the game after created on db
                int gameSerialNum = db_AfterUpdate.tblGames.Where(x => x.CreatedTableTime == createdTime).Select(g => g.GameSerialNum).FirstOrDefault();

                //New list of all equipments becuase maybe there is new equipments
                List<tblEquipment> allEquipments_AfterUpdate = db_AfterUpdate.tblEquipments.ToList();

                if (gameSerialNum != 0)
                {
                    foreach (EquipmentDTO e in equipments)
                    {
                        //create new tbl equipment in game of all the requierd quipment for the specific game
                        //בודק שאכן נוצר משחק חדש במאגר נתונים ואנחנו מחזיקים את המספר משחק 
                        //בסוף מייצר טבלה חדשה של ציוד במשחק ומוסיף לו את המספר ציוד ומספר משחק
                        //בטבלה מתקיים מפתח ראשי בספר רץ עבור כל טבלה שמיוצרת
                        int e_serialNum = allEquipments_AfterUpdate.Find(x => x.EquipmentName.ToLower() == e.EquipmentName.ToLower()).EquipmentSerialNum;
                        if (e_serialNum != 0)
                        {
                            tblEquipmentInGame equipmentOfGame = new tblEquipmentInGame();
                            equipmentOfGame.EquipmentSerialNum = e_serialNum;
                            equipmentOfGame.GameSerialNum = gameSerialNum;
                            db_AfterUpdate.tblEquipmentInGames.Add(equipmentOfGame);
                        }
                    }
                    db_AfterUpdate.SaveChanges();
                    //RequestGamesListDTO teamSerialNum = new RequestGamesListDTO(){ TeamSerialNum = game.TeamSerialNum };
                    tblTeam theTeam = db.tblTeams.Where(x => x.TeamSerialNum == game.TeamSerialNum).FirstOrDefault();
                    if (theTeam != null)
                    {
                        List<tblPlayerInTeam> players = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == game.TeamSerialNum).ToList();
                        foreach (tblPlayerInTeam p in players)
                        {
                            string expoToken = db.tblPlayers.Where(x => x.Email == p.EmailPlayer).Select(a => a.TokenNotfication).FirstOrDefault();
                            if (expoToken != null)
                            {
                                // Create POST data and convert it to a byte array.  
                                var objectToSend = new
                                {
                                    to = expoToken,
                                    title = "New Game Available !",
                                    body = "There is a new game available in Team \"" + theTeam.TeamName + " \"",
                                    //badge = pnd.badge,
                                    data = new { name = "newTeamAdded", T_SerialNum = theTeam.TeamSerialNum, G_SerialNum = gameSerialNum, indexGame = db.tblGames.Where(x => x.TeamSerialNum == theTeam.TeamSerialNum).Count() - 1 },
                                    //new { name = "nir", grade = 100 }
                                };
                                tblPlayer.SendPushNotification(objectToSend);
                            }
                        }
                    }

                    //return Content(HttpStatusCode.OK, GamesList(teamSerialNum));
                    return Ok("The Game Has Been Created Succesfully");
                }
                return BadRequest("Somthing went wrong, the serial number are not exsit ");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // POST api/<controller>
        [HttpPost]
        [Route("RegisterGame")]
        public IHttpActionResult RegisterGame([FromBody] RegisterGameDTO rgame)
        {
            FootHoodDBContext db = new FootHoodDBContext();

            try
            {//צריך לבנות פונקציה שתבצע בדיקה שהשחקן לא נרשם ב24 אחרונות לפני המשחק כי אם כן אז צריך להשתמש בלוגיקה שונה

                if (db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == rgame.GameSerialNum && x.EmailPlayer.ToLower() == rgame.EmailPlayer.ToLower()).FirstOrDefault() == null)
                {
                    tblPlayerRegisteredToGame playerRegister2game = new tblPlayerRegisteredToGame();

                    //bool waitOrNotStatus = tblGames.CheckIf_InWaitingList(db, rgame.GameSerialNum); //check if in waiting list or not with game serial number
                    playerRegister2game.GameSerialNum = rgame.GameSerialNum;
                    playerRegister2game.EmailPlayer = rgame.EmailPlayer;
                    playerRegister2game.WaitOrNot = tblGames.CheckIf_InWaitingList(db, rgame.GameSerialNum); //check if in waiting list or not with game serial number
                    playerRegister2game.GroupNumber = 0;
                    db.tblPlayerRegisteredToGames.Add(playerRegister2game);
                    db.SaveChanges();

                    tblPlayerRegisteredToGame.DividePlayers2Groups(db, rgame.GameSerialNum);

                    int avgPlayersInGame = tblGames.calcAvgAgeInGame(db, rgame.GameSerialNum);
                    db.tblGames.Where(x => x.GameSerialNum == rgame.GameSerialNum).FirstOrDefault().AvgPlayerAge = avgPlayersInGame;
                    int avgRankPlayersInGame = tblGames.calcAvgRankInGame(db, rgame.GameSerialNum);
                    db.tblGames.Where(x => x.GameSerialNum == rgame.GameSerialNum).FirstOrDefault().AvgPlayerRating = avgRankPlayersInGame;
                    db.SaveChanges();
                    return Ok("The Player Has Been Added Succesfully To The Game");
                }
                else
                {
                    return Ok("The Player Has already Registered The Game");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var ctx = ((IObjectContextAdapter)db).ObjectContext;
                foreach (var et in ex.Entries)
                {
                    //client win
                    //ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, et.Entity);

                    //Store Win
                    ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, et.Entity);
                }
                db.SaveChanges();
                return RegisterGame(rgame);
            }
            catch (DivideByZeroException ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Route("AcceptRequest")]
        public IHttpActionResult AcceptRequest([FromBody] JoinRequestDTO joinrequest)
        {
            FootHoodDBContext db = new FootHoodDBContext();
            try
            {
                int teamSerialNum = (int)db.tblGames.Where(x => x.GameSerialNum == joinrequest.GameSerialNum).Select(a => a.TeamSerialNum).FirstOrDefault();
                if (teamSerialNum != 0)
                {
                    var status = tblGames.CheckIf_InWaitingList(db, joinrequest.GameSerialNum);

                    //delete data from join request table
                    db.tblJoinRequests.Remove(db.tblJoinRequests.Where(x => x.EmailPlayer == joinrequest.EmailPlayer
                   && x.GameSerialNum == joinrequest.GameSerialNum).FirstOrDefault());

                    // insert into playerregisterdtogame
                    tblPlayerRegisteredToGame prg = new tblPlayerRegisteredToGame();
                    prg.GameSerialNum = joinrequest.GameSerialNum;
                    prg.EmailPlayer = joinrequest.EmailPlayer;
                    prg.WaitOrNot = status;
                    db.tblPlayerRegisteredToGames.Add(prg);
                    db.SaveChanges();

                    if (db.tblPlayerInTeams.Where(x => x.EmailPlayer == joinrequest.EmailPlayer && x.TeamSerialNum == teamSerialNum).FirstOrDefault() == null)
                    {
                        // insert to playerinteam
                        tblPlayerInTeam pit = new tblPlayerInTeam();
                        //get data from game 
                        pit.TeamSerialNum = db.tblGames.Where(x => x.GameSerialNum == joinrequest.GameSerialNum).
                            Select(x => x.TeamSerialNum.Value).FirstOrDefault();
                        //insert into player registered to game
                        pit.EmailPlayer = joinrequest.EmailPlayer;
                        db.tblPlayerInTeams.Add(pit);
                        db.SaveChanges();
                    }

                    string expoToken = db.tblPlayers.Where(x => x.Email.ToLower() == joinrequest.EmailPlayer.ToLower()).Select(a => a.TokenNotfication).FirstOrDefault();
                    if (expoToken != null)
                    {
                        // Create POST data and convert it to a byte array.  
                        var objectToSend = new
                        {
                            to = expoToken,
                            title = "You Have Joined The Game!",
                            body = "Your request to join the game of team \"" + db.tblTeams.Where(x => x.TeamSerialNum == teamSerialNum).Select(a => a.TeamName).FirstOrDefault() + " \" was accepted",
                            //badge = pnd.badge,
                            data = new { name = "AcceptRequest", T_SerialNum = teamSerialNum, G_SerialNum = prg.GameSerialNum, indexGame = db.tblGames.Where(x => x.TeamSerialNum == teamSerialNum).Count() - 1 },
                            //new { name = "nir", grade = 100 }
                        };
                        tblPlayer.SendPushNotification(objectToSend);
                    }

                    return Ok("The Player Has Been Added Succesfully");
                }
                return Ok("Something went wrong with accepting join request");

            }
            catch (DbUpdateConcurrencyException ex)
            {
                var ctx = ((IObjectContextAdapter)db).ObjectContext;
                foreach (var et in ex.Entries)
                {
                    //client win
                    //ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, et.Entity);

                    //Store Win
                    ctx.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, et.Entity);
                }
                db.SaveChanges();
                return AcceptRequest(joinrequest);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // DELETE api/<controller>
        [HttpPost]
        [Route("DeleteRequest")]
        public IHttpActionResult DeleteRequest([FromBody] JoinRequestDTO joinrequest)
        {
            try
            {
                // delete data from join request table
                FootHoodDBContext db = new FootHoodDBContext();
                db.tblJoinRequests.Remove(db.tblJoinRequests.Where(x => x.EmailPlayer == joinrequest.EmailPlayer
               && x.GameSerialNum == joinrequest.GameSerialNum).FirstOrDefault());
                db.SaveChanges();

                return Ok("The Request Has Been deleted Succesfully");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // DELETE api/<controller>
        [HttpPost]
        [Route("LeaveGame")]
        public IHttpActionResult LeaveGame([FromBody] PlayerInGameDTO playerInGame)
        {
            FootHoodDBContext db = new FootHoodDBContext();
            try
            {
                var player2Remove = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == playerInGame.EmailPlayer.ToLower()
               && x.GameSerialNum == playerInGame.GameSerialNum).FirstOrDefault();
                if (player2Remove != null)
                {
                    db.tblPlayerRegisteredToGames.Remove(db.tblPlayerRegisteredToGames.Where(x => x.PlayerRegisteredToGameId == player2Remove.PlayerRegisteredToGameId).FirstOrDefault());
                    db.SaveChanges();

                    var status = tblGames.CheckIf_InWaitingList(db, playerInGame.GameSerialNum);
                    // set next person = false
                    if (status == false)
                    {
                        var nextwaitinfperson = db.tblPlayerRegisteredToGames.Where(x => x.WaitOrNot == true).FirstOrDefault();
                        if (nextwaitinfperson != null)
                        {
                            nextwaitinfperson.WaitOrNot = false;
                            db.SaveChanges();
                        }
                    }
                    tblPlayerRegisteredToGame.DividePlayers2Groups(db, playerInGame.GameSerialNum);

                    return Ok("You Have Left the Game Succesfully");
                }
                return Ok("You Have already Left the Game ");


            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // Get api/<controller>
        [HttpPost]
        [Route("GetPlayers4Game")]
        public IHttpActionResult GetPlayers4Game([FromBody] ask4PlayersDTO gameSerialNum)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblPlayerRegisteredToGame> players = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == gameSerialNum.GameSerialNum && x.WaitOrNot == false).ToList();

                List<RegisterGameDTO> playersAreReg = new List<RegisterGameDTO>();
                foreach (tblPlayerRegisteredToGame p in players)
                {
                    RegisterGameDTO pr = new RegisterGameDTO() { EmailPlayer = p.EmailPlayer };
                    playersAreReg.Add(pr);
                }
                return Content(HttpStatusCode.OK, playersAreReg);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // Get api/<controller>
        [HttpPost]
        [Route("EditGameDetailes")]
        public IHttpActionResult EditGameDetailes([FromBody] CreateNewGameDTO CNG)
        {
            try
            {
                GameDTO game = CNG.game;
                FootHoodDBContext db = new FootHoodDBContext();
                tblGame theGame = db.tblGames.Where(x => x.GameSerialNum == game.GameSerialNum).FirstOrDefault();
                if (theGame != null)
                {
                    if (game.GameDate != theGame.GameDate)
                        theGame.GameDate = game.GameDate;

                    if (game.GameLocation != theGame.GameLocation)
                    {
                        theGame.GameLocation = game.GameLocation;
                        theGame.FindPlayersActive = false;
                        theGame.GameLatitude = game.GameLatitude;
                        theGame.GameLongitude = game.GameLongitude;
                    }

                    if (game.GameTime != theGame.GameTime)
                        theGame.GameTime = game.GameTime;

                    if (game.LastRegistrationDate != theGame.LastRegistrationDate)
                        theGame.LastRegistrationDate = game.LastRegistrationDate;

                    if (game.LastRegistrationTime != theGame.LastRegistrationTime)
                        theGame.LastRegistrationTime = game.LastRegistrationTime;

                    if (game.NumOfPlayersInTeam != theGame.NumOfPlayersInTeam)
                        theGame.NumOfPlayersInTeam = game.NumOfPlayersInTeam;

                    if (game.NumOfTeams != theGame.NumOfTeams)
                        theGame.NumOfTeams = game.NumOfTeams;

                    RequestGamesListDTO rgl = new RequestGamesListDTO()
                    {
                        TeamSerialNum = (int)theGame.TeamSerialNum
                    };
                    db.SaveChanges();
                    tblPlayerRegisteredToGame.DividePlayers2Groups(db, game.GameSerialNum);


                    return Content(HttpStatusCode.OK, GamesList(rgl));
                }
                return Content(HttpStatusCode.OK, "Somting went wrong with the gameSerialNum");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);

            }

        }

        // Get api/<controller>
        [HttpPost]
        [Route("GetPlayersDivied2Groups")]
        public IHttpActionResult GetPlayersDivied2Groups([FromBody] ask4PlayersDTO game)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblPlayerRegisteredToGame> playersThetRegisterd = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                if (playersThetRegisterd.Count > 0)
                {
                    List<DividePlayerDTO> players = new List<DividePlayerDTO>();
                    foreach (tblPlayerRegisteredToGame p in playersThetRegisterd)
                    {
                        DividePlayerDTO pr = new DividePlayerDTO()
                        {
                            EmailPlayer = p.EmailPlayer,
                            GroupNumber = p.GroupNumber
                        };
                        players.Add(pr);
                    }
                    return Content(HttpStatusCode.OK, players);
                }
                return Content(HttpStatusCode.OK, "No one has registered yet for this game");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }

        // Get api/<controller>
        [HttpPost]
        [Route("GetAmountRegisteredPlayersEachGame")]
        public IHttpActionResult GetAmountRegisteredPlayersEachGame([FromBody] AmountOfPlayersEachGameDTO team)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                var games = db.tblGames.Where(x => x.TeamSerialNum == team.TeamSerialNum && DbFunctions.DiffDays(DateTime.Now, x.GameDate) >= 0).ToList();
                if (games.Count > 0)
                {
                    List<AmountOfPlayersEachGameDTO> arr = new List<AmountOfPlayersEachGameDTO>();
                    foreach (tblGame g in games)
                    {
                        AmountOfPlayersEachGameDTO game = new AmountOfPlayersEachGameDTO()
                        {
                            TeamSerialNum = team.TeamSerialNum,
                            GameSerialNum = g.GameSerialNum,
                            NumOfPlayers = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == g.GameSerialNum).Count()
                        };
                        arr.Add(game);
                    }
                    return Content(HttpStatusCode.OK, arr);
                }
                return Content(HttpStatusCode.OK, "There are no games for this Team");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }


        // DELETE api/<controller>
        [HttpPost]
        [Route("RemoveGameFromList")]
        public IHttpActionResult RemoveGameFromList([FromBody] RemoveGameFromListDTO game2Remove)
        {
            FootHoodDBContext db = new FootHoodDBContext();
            try
            {
                if (db.tblTeams.Where(x => x.TeamSerialNum == game2Remove.TeamSerialNum && x.EmailManager.ToLower() == game2Remove.EmailManager.ToLower()).FirstOrDefault() != null)
                {
                    tblGame theGame = db.tblGames.Where(x => x.GameSerialNum == game2Remove.GameSerialNum).FirstOrDefault();
                    if (theGame != null)
                    {
                        List<tblHotGamesMatch> matches4Game = db.tblHotGamesMatches.Where(x => x.GameSerialNum == theGame.GameSerialNum).ToList();
                        foreach (tblHotGamesMatch hg in matches4Game)
                        {
                            db.tblHotGamesMatches.Remove(hg);
                        }

                        List<tblEquipmentInGame> euipmentsInGame = db.tblEquipmentInGames.Where(x => x.GameSerialNum == theGame.GameSerialNum).ToList();
                        if (euipmentsInGame.Count > 0)
                        {
                            foreach (tblEquipmentInGame e in euipmentsInGame)
                            {
                                db.tblEquipmentInGames.Remove(db.tblEquipmentInGames.Where(x => x.GameSerialNum == e.GameSerialNum && x.EquipmentSerialNum == e.EquipmentSerialNum).FirstOrDefault());
                            }
                        }

                        List<tblJoinRequest> gameRequsts = db.tblJoinRequests.Where(x => x.GameSerialNum == theGame.GameSerialNum).ToList();
                        if (gameRequsts.Count > 0)
                        {
                            foreach (tblJoinRequest jr in gameRequsts)
                            {
                                db.tblJoinRequests.Remove(db.tblJoinRequests.Where(x => x.GameSerialNum == jr.GameSerialNum && x.EmailPlayer.ToLower() == jr.EmailPlayer.ToLower()).FirstOrDefault());
                            }
                        }

                        List<tblPlayerRegisteredToGame> playersInGame = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == theGame.GameSerialNum).ToList();

                        if (playersInGame.Count > 0)
                        {
                            foreach (tblPlayerRegisteredToGame p in playersInGame)
                            {
                                db.tblPlayerRegisteredToGames.Remove(db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == theGame.GameSerialNum && x.EmailPlayer.ToLower() == p.EmailPlayer.ToLower()).FirstOrDefault());
                            }
                        }

                        db.tblGames.Remove(db.tblGames.Where(x => x.GameSerialNum == theGame.GameSerialNum).FirstOrDefault());

                        db.SaveChanges();

                        return Content(HttpStatusCode.OK, GamesList(new RequestGamesListDTO() { TeamSerialNum = game2Remove.TeamSerialNum }));
                    }
                }
                return Content(HttpStatusCode.OK, "Team Serial Number or Email Manager not exist ! ");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("GamesThatUserNotRegisterd")]
        public IHttpActionResult GamesThatUserNotRegisterd([FromBody] GamesNotRegisteredDTO gnr)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                //List of all team
                List<tblTeam> allTeams = db.tblTeams.Where(x => x.TeamSerialNum > 0 && x.IsPrivate == false).ToList();

                //List of all team, will remove teams, will be the list of teams that user not registered to
                List<tblTeam> allTeams2 = new List<tblTeam>(allTeams);

                foreach (tblTeam team in allTeams)
                {
                    var playerRegisterd = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == team.TeamSerialNum && x.EmailPlayer.ToLower() == gnr.EmailPlayer.ToLower()).FirstOrDefault();
                    if (playerRegisterd != null)
                    {
                        allTeams2.Remove(db.tblTeams.Where(x => x.TeamSerialNum == team.TeamSerialNum).FirstOrDefault());
                    }
                }
                if (allTeams2.Count > 0)
                {
                    List<GameDTO> gameNotIn = new List<GameDTO>();
                    foreach (tblTeam t in allTeams2)
                    {
                        List<tblGame> gameUserNotIn = db.tblGames.Where(x => x.TeamSerialNum == t.TeamSerialNum && DbFunctions.DiffDays(DateTime.Now, x.GameDate) >= 0).ToList();
                        if (gameUserNotIn.Count > 0)
                        {
                            foreach (tblGame g in gameUserNotIn)
                            {
                                var checkIfSendRequest = db.tblJoinRequests.Where(x => x.GameSerialNum == g.GameSerialNum && x.EmailPlayer == gnr.EmailPlayer).FirstOrDefault();
                                if (checkIfSendRequest == null)
                                {
                                    int numOfPlayersInTeam = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == g.GameSerialNum && x.WaitOrNot == false).Count();
                                    GameDTO game = new GameDTO()
                                    {
                                        GameSerialNum = g.GameSerialNum,
                                        NumOfTeams = g.NumOfTeams,
                                        NumOfPlayersInTeam = numOfPlayersInTeam,
                                        GameLocation = g.GameLocation,
                                        GameDate = g.GameDate,
                                        GameTime = g.GameTime,
                                        LastRegistrationDate = g.LastRegistrationDate,
                                        LastRegistrationTime = g.LastRegistrationTime,
                                        AvgPlayerAge = g.AvgPlayerAge,
                                        AvgPlayerRating = g.AvgPlayerRating,
                                        TeamSerialNum = t.TeamSerialNum,
                                        GameLatitude = g.GameLatitude,
                                        GameLongitude = g.GameLongitude,
                                    };
                                    gameNotIn.Add(game);
                                }
                            }
                        }
                    }
                    return Content(HttpStatusCode.OK, gameNotIn);
                }
                return Content(HttpStatusCode.OK, "No Games has been found");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }


        // Get api/<controller>
        [HttpPost]
        [Route("GetPlayerWaiting")]
        public IHttpActionResult GetPlayerWaiting([FromBody] ask4PlayersDTO game)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblPlayerRegisteredToGame> playersThetRegisterd = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                if (playersThetRegisterd.Count > 0)
                {
                    List<WaitingPlayerDTO> players = new List<WaitingPlayerDTO>();
                    foreach (tblPlayerRegisteredToGame p in playersThetRegisterd)
                    {
                        if (p.WaitOrNot == true)
                        {
                            WaitingPlayerDTO pr = new WaitingPlayerDTO()
                            {
                                EmailPlayer = p.EmailPlayer,
                                WaitOrNot = p.WaitOrNot,
                            };
                            players.Add(pr);
                        }
                    }
                    return Content(HttpStatusCode.OK, players);
                }
                return Content(HttpStatusCode.OK, "There are no players");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }


        // Get api/<controller>
        [HttpPost]
        [Route("TodaysGame")]
        public IHttpActionResult TodaysGame([FromBody] AskTodaysGameDTO player)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblPlayer thePlayer = db.tblPlayers.Where(x => x.Email.ToLower() == player.EmailPlayer.ToLower()).FirstOrDefault();
                if (thePlayer != null)
                {
                    List<tblPlayerRegisteredToGame> gamesPlayerRegistered = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == player.EmailPlayer.ToLower() && x.WaitOrNot==false).ToList();
                    List<tblGame> allGames = new List<tblGame>();
                    foreach (tblPlayerRegisteredToGame g in gamesPlayerRegistered)
                    {
                        tblGame game = db.tblGames.Where(x => x.GameSerialNum == g.GameSerialNum && x.GameDate>= DateTime.Today).FirstOrDefault();
                        if (game != null)
                            allGames.Add(game);
                    }
                    //Check if registered to any game
                    if (gamesPlayerRegistered.Count == 0 || allGames.Count==0)
                        return Content(HttpStatusCode.OK, -1);
                    
                    //allGames.OrderByDescending(x => x.GameDate);
                    allGames.OrderBy(x => x.GameDate);
                    tblGame todaysGame = allGames.First();
                    List<PlayerDTO> groupPlayers = new List<PlayerDTO>();

                    tblPlayerRegisteredToGame detaileOfReg = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == thePlayer.Email.ToLower() && x.GameSerialNum == todaysGame.GameSerialNum).FirstOrDefault();
                    int groupNumber = 0;
                    string bring = "";
                    if (detaileOfReg!=null)
                    {
                        groupNumber = detaileOfReg.GroupNumber;
                        bring = detaileOfReg.BringItems;
                    }
                     groupNumber = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == thePlayer.Email.ToLower() && x.GameSerialNum == todaysGame.GameSerialNum).Select(a => a.GroupNumber).FirstOrDefault();
                    if (groupNumber != 0)
                    {
                        List<tblPlayerRegisteredToGame> emailsOfPlayerGroup = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == todaysGame.GameSerialNum && x.GroupNumber == groupNumber).ToList();

                        foreach (tblPlayerRegisteredToGame p in emailsOfPlayerGroup)
                        {
                            tblPlayer pg = db.tblPlayers.Where(x => x.Email.ToLower() == p.EmailPlayer.ToLower()).FirstOrDefault();
                            if (pg != null)
                            {
                                groupPlayers.Add(new PlayerDTO()
                                {
                                    FirstName = pg.FirstName,
                                    LastName = pg.LastName,
                                    Email = pg.Email,
                                    Phone = pg.Phone,
                                    Gender = pg.Gender,
                                    PlayerCity = pg.PlayerCity,
                                    DateOfBirth = pg.DateOfBirth,
                                    PlayerPicture = pg.PlayerPicture,
                                    StrongLeg = pg.StrongLeg,
                                    Height = pg.Height,
                                    Stamina = pg.Stamina,
                                    PreferredRole = pg.PreferredRole,
                                    OverallRating = pg.OverallRating,
                                    LatitudeHomeCity = pg.LatitudeHomeCity,
                                    LongitudeHomeCity = pg.LongitudeHomeCity
                                });
                            }
                        }

                    }
                    List<tblEquipmentInGame> equipmentsPerGame = db.tblEquipmentInGames.Where(x => x.GameSerialNum == todaysGame.GameSerialNum).ToList();

                    //passing 2 dto List
                    List<EquipmentsInGameDTO> newListEquipmentsInGame = new List<EquipmentsInGameDTO>();
                    foreach (tblEquipmentInGame e in equipmentsPerGame)
                    {
                        //all round add new obj of EquipmentsInGameDTO
                        newListEquipmentsInGame.Add(new EquipmentsInGameDTO()
                        {
                            EquipmentSerialNum = e.EquipmentSerialNum,
                            GameSerialNum = e.GameSerialNum
                        });
                    }
                    
                    GameDTO theGame = new GameDTO()
                    {
                        GameSerialNum = todaysGame.GameSerialNum,
                        NumOfTeams = todaysGame.NumOfTeams,
                        NumOfPlayersInTeam = todaysGame.NumOfPlayersInTeam,
                        GameLocation = todaysGame.GameLocation,
                        GameDate = todaysGame.GameDate,
                        GameTime = todaysGame.GameTime,
                        LastRegistrationDate = todaysGame.LastRegistrationDate,
                        LastRegistrationTime = todaysGame.LastRegistrationTime,
                        AvgPlayerAge = tblGames.calcAvgAgeInGame(db, todaysGame.GameSerialNum),
                        AvgPlayerRating = tblGames.calcAvgRankInGame(db, todaysGame.GameSerialNum),
                        TeamSerialNum = todaysGame.TeamSerialNum,
                        equipmentsInGame = newListEquipmentsInGame, // this is the List of EquipmentsInGameDTO
                        FindPlayersActive = todaysGame.FindPlayersActive,
                        GameLatitude = todaysGame.GameLatitude,
                        GameLongitude = todaysGame.GameLongitude
                    };
                    TodaysGameDTO allDetails = new TodaysGameDTO()
                    {
                        Game = theGame,
                        Players = groupPlayers,
                        Bring = bring
                    };
                    return Content(HttpStatusCode.OK, allDetails);
                }
                return Content(HttpStatusCode.OK, "Something wrong with user details");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }



        // Get api/<controller>
        [HttpPost]
        [Route("CheckIfRegisterd2AnyGame")]
        public IHttpActionResult CheckIfRegisterd2AnyGame([FromBody] CheckIfRegisterd2AnyGameDTO player)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblPlayer thePlayer = db.tblPlayers.Where(x => x.Email.ToLower() == player.EmailPlayer.ToLower()).FirstOrDefault();
                if (thePlayer != null)
                {
                    if (db.tblPlayerRegisteredToGames.Where(x=>x.EmailPlayer.ToLower()== thePlayer.Email.ToLower()).FirstOrDefault()!= null)
                    {
                        return Content(HttpStatusCode.OK, true);
                    }
                    return Content(HttpStatusCode.OK, false);
                }
                return Content(HttpStatusCode.OK, "Something wrong with user details");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }
        


        //no use yet
        public IHttpActionResult GamesThatUserRegisterd([FromBody] RegisterGameDTO rg)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();

                List<tblGame> gamesOfTeam = db.tblGames.Where(x => x.GameSerialNum == rg.GameSerialNum && DbFunctions.DiffDays(DateTime.Now, x.GameDate) >= 0).ToList();
                List<RegisterGameDTO> userHasRegisterd2 = new List<RegisterGameDTO>();
                foreach (tblGame game in gamesOfTeam)
                {
                    var hasRegi = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == game.GameSerialNum && x.EmailPlayer == rg.EmailPlayer).FirstOrDefault();
                    if (hasRegi != null)
                    {
                        RegisterGameDTO r = new RegisterGameDTO()
                        {
                            EmailPlayer = hasRegi.EmailPlayer,
                            GameSerialNum = hasRegi.GameSerialNum,
                            PlayerRegisteredToGameId = hasRegi.PlayerRegisteredToGameId,
                            TeamSerialNum = rg.TeamSerialNum
                        };
                        userHasRegisterd2.Add(r);
                    }

                }
                return Content(HttpStatusCode.OK, userHasRegisterd2);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Route("MyCheckes")]
        public IHttpActionResult MyCheckes([FromBody] PlayerDTO p)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                //tblPlayer playersDetails = db.tblPlayers.Where(x => x.Email.ToLower() == p.Email.ToLower()).FirstOrDefault();

                List<tblPlayer> playersDetails = db.tblPlayers.Where(x => x.Email.ToLower() == p.Email.ToLower()).OrderBy(x => x.OverallRating).ToList();
                foreach (tblPlayer item in playersDetails)
                {
                    if (playersDetails != null)
                    {
                        PlayerDTO newPlayer = new PlayerDTO()
                        {
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Email = item.Email,
                            Phone = item.Phone,
                            //Passcode = player.Passcode,
                            Gender = item.Gender,
                            PlayerCity = item.PlayerCity,
                            DateOfBirth = item.DateOfBirth,
                            PlayerPicture = item.PlayerPicture,
                            StrongLeg = item.StrongLeg,
                            Height = item.Height,
                            Stamina = item.Stamina,
                            PreferredRole = item.PreferredRole,
                            OverallRating = item.OverallRating,
                            TokenNotfication = item.TokenNotfication
                        };
                        return Ok(newPlayer);
                    }
                }

                return Content(HttpStatusCode.BadRequest, "The user you are trying to login does not exist");


                // return Content(HttpStatusCode.OK, playersDetails);
            }
            catch (Exception ex)
            {

                return Content(HttpStatusCode.BadRequest, ex);

            }

        }

    }
}