using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.TeamFunctions;
using DATA.EF;

namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/team")]
    public class TeamController : ApiController
    {
        //Post api/<controller>/{object}
        [HttpPost]
        [Route("TeamDetails")]
        public IHttpActionResult TeamDetails([FromBody]PlayerInTeamDTO player)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();

                //All teams that the player is inside by email player
                List<tblPlayerInTeam> teamsList = db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == player.EmailPlayer.ToLower()).ToList();

                //creating a new list of teams in dto - empty obj
                List<TeamDTO> newList = new List<TeamDTO>();

                //loop to push on team list
                foreach (tblPlayerInTeam t in teamsList)
                {
                    //to import all details of specific team
                    tblTeam team = db.tblTeams.Where(x => x.TeamSerialNum == t.TeamSerialNum).FirstOrDefault();

                    //list of every player in each team
                    List<tblPlayerInTeam> playersInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == t.TeamSerialNum).ToList();

                    //exchange the list of all players to DTO list
                    List<PlayerInTeamDTO> playersInTeamDto = new List<PlayerInTeamDTO>();

                    //List<PlayerDTO> playersOfTeam = new List<PlayerDTO>();   not to use
                    foreach (tblPlayerInTeam p in playersInTeam)
                    {
                        PlayerInTeamDTO pl = new PlayerInTeamDTO();
                        pl.EmailPlayer = p.EmailPlayer;
                        pl.TeamSerialNum = p.TeamSerialNum;
                        playersInTeamDto.Add(pl);
                    }

                    //creating a list of teams in DTO
                    newList.Add(new TeamDTO()
                    {
                        TeamSerialNum = t.TeamSerialNum,
                        TeamName = team.TeamName,
                        IsPrivate = team.IsPrivate,
                        RulesAndLaws = team.RulesAndLaws,
                        TeamPicture = team.TeamPicture,
                        EmailManager = team.EmailManager,
                        PlayersList = playersInTeamDto,
                    });
                }
                return Content(HttpStatusCode.OK, newList);
            }
            catch (Exception ex)
            {
                // to send back error
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("GetPlayers4Team")]
        public IHttpActionResult GetPlayers4Team([FromBody]PlayerInTeamDTO teamNum)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblPlayerInTeam> playersList = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == teamNum.TeamSerialNum).ToList();


                List<PlayerInTeamDTO> playersInTeamDto = new List<PlayerInTeamDTO>();

                foreach (tblPlayerInTeam p in playersList)
                {
                    PlayerInTeamDTO pl = new PlayerInTeamDTO();
                    pl.EmailPlayer = p.EmailPlayer;
                    pl.TeamSerialNum = p.TeamSerialNum;
                    playersInTeamDto.Add(pl);
                }


                //List<PlayerDTO> players4Team = new List<PlayerDTO>();
                //foreach (tblPlayerInTeam p in playersList)
                //{
                //    var player = db.tblPlayers.Where(x => x.Email == p.EmailPlayer).FirstOrDefault();
                //    if (player != null)
                //    {
                //        PlayerDTO playerInTeam = new PlayerDTO()
                //        {
                //            FirstName = player.FirstName,
                //            LastName = player.LastName,
                //            Email = player.Email,
                //            Phone = player.Phone,
                //            Gender = player.Gender,
                //            PlayerCity = player.PlayerCity,
                //            DateOfBirth = player.DateOfBirth,
                //            PlayerPicture = player.PlayerPicture,
                //            StrongLeg = player.StrongLeg,
                //            Height = player.Height,
                //            Stamina = player.Stamina,
                //            PreferredRole = player.PreferredRole,
                //            OverallRating = player.OverallRating,
                //            LatitudeHomeCity = player.LatitudeHomeCity,
                //            LongitudeHomeCity = player.LongitudeHomeCity,
                //            TokenNotfication = player.TokenNotfication
                //        };
                //        players4Team.Add(playerInTeam);
                //        //playersDetails.Add(player);ד
                //    }
                //}
                return Content(HttpStatusCode.OK, playersInTeamDto);
            }
            catch (Exception ex)
            {
                // to send back error
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Route("CreateNewTeam")]
        // POST api/<controller>
        public IHttpActionResult CreateNewTeam([FromBody] TeamDTO team)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();

                tblTeam newTeam = new tblTeam();
                if (db.tblTeams.Where(x => x.EmailManager.ToLower() == team.EmailManager.ToLower() && x.TeamName == team.TeamName).FirstOrDefault() == null)
                {
                    newTeam.TeamName = team.TeamName;
                    newTeam.TeamPicture = team.TeamPicture;
                    newTeam.IsPrivate = team.IsPrivate;
                    newTeam.RulesAndLaws = team.RulesAndLaws;
                    newTeam.EmailManager = team.EmailManager;

                    db.tblTeams.Add(newTeam);
                    db.SaveChanges();

                    foreach (PlayerInTeamDTO p in team.PlayersList)
                    {
                        var player = new PlayerInTeamDTO()
                        {
                            TeamSerialNum = newTeam.TeamSerialNum,
                            EmailPlayer = p.EmailPlayer
                        };
                        JoinTeam(player);
                    }
                }
                else
                    return Content(HttpStatusCode.BadRequest, "You already created a team with this name. \nPlease Choose a difrrent name..");

                var theManager = new PlayerInTeamDTO()
                {
                    TeamSerialNum = newTeam.TeamSerialNum,
                    EmailPlayer = team.EmailManager
                };
                var registerd = JoinTeam(theManager);
                
                return Ok("The Team Has Been Created Succefully !");
                //return Ok("New Team has been Created but the manager has not registerd as a player");

            }

            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("JoinTeam")]
        // POST api/<controller>
        public IHttpActionResult JoinTeam([FromBody] PlayerInTeamDTO playerEntry)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();

                tblPlayerInTeam player = db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == playerEntry.EmailPlayer.ToLower() && x.TeamSerialNum == playerEntry.TeamSerialNum).FirstOrDefault();
                if (player == null)
                {
                    // insert to playerinteam
                    tblPlayerInTeam pit = new tblPlayerInTeam();

                    //insert into player In team
                    pit.TeamSerialNum = playerEntry.TeamSerialNum;
                    pit.EmailPlayer = playerEntry.EmailPlayer;
                    db.tblPlayerInTeams.Add(pit);
                    db.SaveChanges();

                    string expoToken = db.tblPlayers.Where(x => x.Email.ToLower() == playerEntry.EmailPlayer.ToLower()).Select(a => a.TokenNotfication).FirstOrDefault();
                    if (expoToken != null)
                    {
                        string title = "You Have Been Added To A New Team !";
                        string body = "Go and check your friends and games in team \"" + db.tblTeams.Where(x => x.TeamSerialNum == playerEntry.TeamSerialNum).Select(a => a.TeamName).FirstOrDefault() + "\"";
                        if (playerEntry.EmailPlayer == db.tblTeams.Where(x=>x.TeamSerialNum == playerEntry.TeamSerialNum).Select(a=>a.EmailManager).FirstOrDefault())
                        {
                            title = "The Team Has Been Created Succefully !";
                            body = "Go look for new players for your team";
                        }

                        // Create POST data and convert it to a byte array.  
                        var objectToSend = new
                        {
                            to = expoToken,
                            title = title,
                            body = body,
                            //badge = pnd.badge,
                            data = new { name = "AddedNewTeam", T_SerialNum = playerEntry.TeamSerialNum, G_SerialNum = -1, indexGame = db.tblGames.Where(x => x.TeamSerialNum == playerEntry.TeamSerialNum).Count() - 1 },
                            //new { name = "nir", grade = 100 }
                        };
                        tblPlayer.SendPushNotification(objectToSend);
                    }

                    return Ok("The Player Has Been Added Succesfully");
                }
                return Ok("This player already in the team");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("TeamEntranceCounter")]
        // Post api/<controller>
        public IHttpActionResult TeamEntranceCounter([FromBody] PlayerInTeamDTO playerEntry)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                var playerExist = db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == playerEntry.EmailPlayer.ToLower() && x.TeamSerialNum == playerEntry.TeamSerialNum).FirstOrDefault();
                if (playerExist != null)
                {
                    playerExist.NumOfTimesEnteredTeam += 1;
                    db.SaveChanges();
                    return Ok("Num Of entry team has plus 1");
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "You have player that is not exist in this team");
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("AddNewJoinRequests")]
        // Post api/<controller>
        public IHttpActionResult AddNewJoinRequests([FromBody]JoinRequestDTO joinRequest)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                int teamSerialNum = (int)db.tblGames.Where(x => x.GameSerialNum == joinRequest.GameSerialNum).Select(a => a.TeamSerialNum).FirstOrDefault();
                if (teamSerialNum != 0)
                {
                    if (db.tblPlayerInTeams.Where(x => x.EmailPlayer == joinRequest.EmailPlayer && x.TeamSerialNum == teamSerialNum).FirstOrDefault() == null)
                    {
                        //add this user email and game serial num to joinrequest table.
                        if (db.tblJoinRequests.Where(x => x.GameSerialNum == joinRequest.GameSerialNum && x.EmailPlayer == joinRequest.EmailPlayer).FirstOrDefault() == null)
                        {
                            tblJoinRequest jr = new tblJoinRequest();
                            jr.GameSerialNum = joinRequest.GameSerialNum;
                            jr.EmailPlayer = joinRequest.EmailPlayer;
                            db.tblJoinRequests.Add(jr);
                            db.SaveChanges();
                            //int teamSerialNum = (int)db.tblGames.Where(x => x.GameSerialNum == joinRequest.GameSerialNum).Select(a => a.TeamSerialNum).FirstOrDefault();
                            //if (teamSerialNum != 0)
                            //{
                            string emailManager = db.tblTeams.Where(x => x.TeamSerialNum == teamSerialNum).Select(a => a.EmailManager).FirstOrDefault();
                            if (emailManager != null)
                            {
                                string expoToken = db.tblPlayers.Where(x => x.Email == emailManager).Select(a => a.TokenNotfication).FirstOrDefault();
                                if (expoToken != null)
                                {
                                    // Create POST data and convert it to a byte array.  
                                    var objectToSend = new
                                    {
                                        to = expoToken,
                                        title = "You Have New Game Request ! ",
                                        body = "Click here and see how is asking !",
                                        //badge = pnd.badge,
                                        data = new { name = "joinRequest", T_SerialNum = teamSerialNum, G_SerialNum = joinRequest.GameSerialNum },
                                        //new { name = "nir", grade = 100 }
                                    };
                                    tblPlayer.SendPushNotification(objectToSend);
                                }
                            }

                            return Ok("The Request has been Added");
                        }
                        return Ok("The Request has already been added ");
                    }
                    return Ok("The Player is already in this team");
                }
                return Ok("Something went wrong");
            }
            catch (Exception ex)
            {
                // to send back error
                return null;
            }
        }


        [HttpPost]
        [Route("JoinRequests")]
        // Get api/<controller>
        public IHttpActionResult JoinRequests([FromBody]JoinRequestDTO joinRequest)
        {
            try
            {

                FootHoodDBContext db = new FootHoodDBContext();
                List<tblJoinRequest> playersRequests = db.tblJoinRequests.Where(x => x.GameSerialNum == joinRequest.GameSerialNum).ToList();

                List<JoinRequestDTO> joinRequests = new List<JoinRequestDTO>();
                foreach (tblJoinRequest pr in playersRequests)
                {
                    JoinRequestDTO jr = new JoinRequestDTO()
                    {
                        EmailPlayer = pr.EmailPlayer,
                        GameSerialNum = joinRequest.GameSerialNum

                    };
                    joinRequests.Add(jr);
                }

                return Content(HttpStatusCode.OK, joinRequests);
            }
            catch (Exception ex)
            {
                // to send back error
                return null; //Content(HttpStatusCode.BadRequest, ex);
            }
        }


        // DELETE api/<controller>
        [HttpPost]
        [Route("LeaveTeam")]
        public IHttpActionResult LeaveTeam([FromBody] PlayerInTeamDTO playerInTeam)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();

                tblTeam theTeam = db.tblTeams.Where(x => x.TeamSerialNum == playerInTeam.TeamSerialNum).FirstOrDefault();
                tblPlayerInTeam thePlayerInTeam = db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower() && x.TeamSerialNum == playerInTeam.TeamSerialNum).FirstOrDefault();
                if (theTeam != null && thePlayerInTeam != null)
                {
                    List<tblPlayerInTeam> numOfPlayers = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == playerInTeam.TeamSerialNum).ToList();
                    if (numOfPlayers.Count <= 1)
                    {
                        tblPlayerInTeam lastPlayer = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == theTeam.TeamSerialNum && x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower()).FirstOrDefault();
                        if (lastPlayer != null)
                        {
                            db.tblPlayerInTeams.Remove(db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower() && x.TeamSerialNum == lastPlayer.TeamSerialNum).FirstOrDefault());
                            db.SaveChanges();

                            RemoveTeamDTO teamToRemove = new RemoveTeamDTO() { TeamSerialNum = theTeam.TeamSerialNum, };
                            RemoveTeam(teamToRemove);
                        }
                    }
                    else
                    {
                        if (theTeam.EmailManager.ToLower() == playerInTeam.EmailPlayer.ToLower())
                        {
                            tblPlayerInTeam newManger = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == playerInTeam.TeamSerialNum && playerInTeam.EmailPlayer.ToLower() != x.EmailPlayer.ToLower()).FirstOrDefault();
                            if (newManger != null)
                                theTeam.EmailManager = newManger.EmailPlayer;
                        }
                        List<tblPlayerRegisteredToGame> registeredgames = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower()).ToList();
                        foreach (var g in registeredgames)
                        {
                            db.tblPlayerRegisteredToGames.Remove(g);

                        }
                        db.tblPlayerInTeams.Remove(thePlayerInTeam);
                        db.SaveChanges();

                        tblPlayer thePlayerLeft = db.tblPlayers.Where(x => x.Email.ToLower() == playerInTeam.EmailPlayer.ToLower()).FirstOrDefault();
                        if (thePlayerLeft != null)
                        {
                            List<tblPlayerInTeam> restOfPlayersInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == playerInTeam.TeamSerialNum).ToList();
                            foreach (tblPlayerInTeam p in restOfPlayersInTeam)
                            {
                                string expoToken = db.tblPlayers.Where(x => x.Email.ToLower() == p.EmailPlayer.ToLower()).Select(a => a.TokenNotfication).FirstOrDefault();
                                if (expoToken != null)
                                {
                                    // Create POST data and convert it to a byte array.  
                                    var objectToSend = new
                                    {
                                        to = expoToken,
                                        title = $"{thePlayerLeft.FirstName} {thePlayerLeft.LastName} has left the team !",
                                        body = "We hope you will find a new players soon",
                                        //badge = pnd.badge,
                                        data = new { name = "LeaveTeam", T_SerialNum = -1, G_SerialNum = -1, indexGame = - 1 },
                                        //new { name = "nir", grade = 100 }
                                    };
                                    tblPlayer.SendPushNotification(objectToSend);
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    //var teamDetails =  new TeamDetails(new PlayerInTeamDTO() { EmailPlayer = playerInTeam.EmailPlayer });
                    return Content(HttpStatusCode.OK, "The Team Has Been Removeed");
                }
                return Content(HttpStatusCode.OK, "Somethind Wrong With Team Detaile");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("RemoveTeam")]
        public IHttpActionResult RemoveTeam([FromBody] RemoveTeamDTO team)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblGame> allGamesOfTeam = db.tblGames.Where(x => x.TeamSerialNum == team.TeamSerialNum).ToList();
                if (allGamesOfTeam.Count > 0)
                {
                    foreach (tblGame game in allGamesOfTeam)
                    {

                        List<tblHotGamesMatch> matches4Game = db.tblHotGamesMatches.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                        foreach (tblHotGamesMatch hg in matches4Game)
                        {
                            db.tblHotGamesMatches.Remove(hg);
                        } 

                        //delete all equipments of Game
                        List<tblEquipmentInGame> euipmentPerGame = db.tblEquipmentInGames.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                        foreach (tblEquipmentInGame e in euipmentPerGame)
                        {
                            db.tblEquipmentInGames.Remove(db.tblEquipmentInGames.Where(x => x.EquipmentInGameId == e.EquipmentInGameId).FirstOrDefault());
                        }


                        //delete all join request per Game
                        List<tblJoinRequest> allJoinRequestes4Game = db.tblJoinRequests.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                        foreach (tblJoinRequest r in allJoinRequestes4Game)
                        {
                            db.tblJoinRequests.Remove(db.tblJoinRequests.Where(x => x.JoinRequestId == r.JoinRequestId).FirstOrDefault());
                        }
                        db.SaveChanges();//====================================================

                        //get all rating of this Game
                        //משנה את מספר המשחק בגלל שמחקנו את המשחק מהמאגר נתונים ושינינו את מספר המשחק ל0 כדי שלא ימחוק את הדירוג
                        List<tblRatingOfGame> allRatingPerGame = db.tblRatingOfGames.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                        foreach (tblRatingOfGame ratePerGame in allRatingPerGame)
                        {
                            ratePerGame.GameSerialNum = 0;
                        }
                        db.SaveChanges();//====================================================

                        //delete all players in each game
                        List<tblPlayerRegisteredToGame> allPlayersInGame = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == game.GameSerialNum).ToList();
                        foreach (tblPlayerRegisteredToGame p in allPlayersInGame)
                        {
                            db.tblPlayerRegisteredToGames.Remove(db.tblPlayerRegisteredToGames.Where(x => x.PlayerRegisteredToGameId == p.PlayerRegisteredToGameId).FirstOrDefault());
                        }
                        db.SaveChanges();//====================================================


                        //delete Game From Team
                        db.tblGames.Remove(db.tblGames.Where(x => x.GameSerialNum == game.GameSerialNum).FirstOrDefault());
                        db.SaveChanges(); //====================================================
                    }
                }
                //delete all players in this team
                List<tblPlayerInTeam> allPlayersInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == team.TeamSerialNum).ToList();
                foreach (tblPlayerInTeam p in allPlayersInTeam)
                {
                    db.tblPlayerInTeams.Remove(db.tblPlayerInTeams.Where(x => x.TeamSerialNum == p.TeamSerialNum).FirstOrDefault());
                }

                //delete team from tbl Team
                db.tblTeams.Remove(db.tblTeams.Where(x => x.TeamSerialNum == team.TeamSerialNum).FirstOrDefault());
                db.SaveChanges();

                return Ok("The team has been removed Succesfully");


            }
            catch (Exception ex)
            {
                //return null;
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("RemoveFromTeam")]
        public IHttpActionResult RemoveFromTeam([FromBody] PlayerInTeamDTO playerInTeam)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                tblTeam theTeam = db.tblTeams.Where(x => x.TeamSerialNum == playerInTeam.TeamSerialNum).FirstOrDefault();

                if (theTeam != null)
                {
                    List<tblGame> allGamesOfTeam = db.tblGames.Where(x => x.TeamSerialNum == playerInTeam.TeamSerialNum).ToList();
                    if (allGamesOfTeam.Count > 0)
                    {
                        foreach (tblGame g in allGamesOfTeam)
                        {
                            tblPlayerRegisteredToGame player = db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower() && x.GameSerialNum == g.GameSerialNum).FirstOrDefault();
                            if (player != null)
                            {
                                db.tblPlayerRegisteredToGames.Remove(db.tblPlayerRegisteredToGames.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower() && x.GameSerialNum == g.GameSerialNum).FirstOrDefault());

                            }
                        }
                    }
                    tblPlayerInTeam player2Remove = db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower()
                     && x.TeamSerialNum == playerInTeam.TeamSerialNum).FirstOrDefault();
                    if (player2Remove != null)
                    {
                        db.tblPlayerInTeams.Remove(db.tblPlayerInTeams.Where(x => x.EmailPlayer.ToLower() == playerInTeam.EmailPlayer.ToLower()
                    && x.TeamSerialNum == playerInTeam.TeamSerialNum).FirstOrDefault());
                    }

                    db.SaveChanges();

                    string expoToken = db.tblPlayers.Where(x => x.Email.ToLower() == playerInTeam.EmailPlayer.ToLower()).Select(a => a.TokenNotfication).FirstOrDefault();
                    if (expoToken != null)
                    {
                        // Create POST data and convert it to a byte array.  
                        var objectToSend = new
                        {
                            to = expoToken,
                            title = "You Have Been Kicked Out From Team !",
                            body = "Don't worry! In FootHood we have a big selection of teams to choose from! \nWe hope you will find the perfect fit for you!",
                            //badge = pnd.badge,
                            data = new { name = "RemoveFromTeam", T_SerialNum = -1, G_SerialNum = -1, indexGame = -1 },
                            //new { name = "nir", grade = 100 }
                        };
                        tblPlayer.SendPushNotification(objectToSend);
                    }

                    PlayerInTeamDTO managerTeam = new PlayerInTeamDTO()
                    {
                        EmailPlayer = theTeam.EmailManager,
                        TeamSerialNum = theTeam.TeamSerialNum
                    };
                    //var newList = TeamDetails(managerTeam);
                    //List<TeamDTO> teams2Send = new List<TeamDTO>(newList);
                    return Content(HttpStatusCode.OK, "Player has been removed !");


                    //List <tblPlayerInTeam> playersInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == theTeam.TeamSerialNum).ToList();
                    //if (playersInTeam.Count > 0)
                    //{
                    //    //exchange the list of all players to DTO list
                    //    List<PlayerInTeamDTO> playersInTeamDto = new List<PlayerInTeamDTO>();

                    //    //List<PlayerDTO> playersOfTeam = new List<PlayerDTO>();   not to use
                    //    foreach (tblPlayerInTeam p in playersInTeam)
                    //    {
                    //        PlayerInTeamDTO pl = new PlayerInTeamDTO();
                    //        pl.EmailPlayer = p.EmailPlayer;
                    //        pl.TeamSerialNum = p.TeamSerialNum;
                    //        playersInTeamDto.Add(pl);
                    //    }


                    //    TeamDTO team = new TeamDTO()
                    //    {
                    //        TeamSerialNum = theTeam.TeamSerialNum,
                    //        TeamName = theTeam.TeamName,
                    //        IsPrivate = theTeam.IsPrivate,
                    //        RulesAndLaws = theTeam.RulesAndLaws,
                    //        TeamPicture = theTeam.TeamPicture,
                    //        EmailManager = theTeam.EmailManager,
                    //        PlayersList = playersInTeamDto,
                    //    };
                    //    return Content(HttpStatusCode.OK, team);

                    //}
                }
                return Ok("There in no such Team with this serial number");
                //return Ok("The player has been removed Succesfully");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }

        }


        [HttpPost]
        [Route("SearchPlayer")]
        public IHttpActionResult SearchPlayer([FromBody] SearchPlayerDTO player)
        {
            try
            {
                if (player.FirstName == "")
                    player.FirstName = null;
                if (player.LastName == "")
                    player.LastName = null;

                FootHoodDBContext db = new FootHoodDBContext();

                List<tblPlayer> searchedPlayer = db.tblPlayers.Where(x => x.FirstName.ToLower() == player.FirstName.ToLower() && x.LastName.ToLower() == player.LastName.ToLower()).ToList();
                searchedPlayer.AddRange(db.tblPlayers.Where(x => x.FirstName.ToLower() == player.FirstName.ToLower() || x.LastName.ToLower() == player.LastName.ToLower()).ToList());

                if (searchedPlayer.Count == 0)
                    searchedPlayer = db.tblPlayers.Where(x => x.FirstName.Contains(player.FirstName) || x.LastName.Contains(player.FirstName) || x.LastName.Contains(player.LastName)).ToList();

                List<PlayerDTO> players = new List<PlayerDTO>();
                foreach (tblPlayer p in searchedPlayer)
                {
                    PlayerDTO newPlayer = new PlayerDTO()
                    {
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Email = p.Email,
                        Phone = p.Phone,
                        Gender = p.Gender,
                        PlayerCity = p.PlayerCity,
                        DateOfBirth = p.DateOfBirth,
                        PlayerPicture = p.PlayerPicture,
                        StrongLeg = p.StrongLeg,
                        Height = p.Height,
                        Stamina = p.Stamina,
                        PreferredRole = p.PreferredRole,
                        OverallRating = p.OverallRating,
                        LatitudeHomeCity = p.LatitudeHomeCity,
                        LongitudeHomeCity = p.LongitudeHomeCity,
                        TokenNotfication = p.TokenNotfication
                    };
                    players.Add(newPlayer);
                }
                return Content(HttpStatusCode.OK, players);
            }
            catch (Exception ex)
            {
                //return null;
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpPost]
        [Route("SendMessageTeamChat")]
        public IHttpActionResult SendMessageTeamChat([FromBody] PlayerSendMessageInTeamChatDTO detail)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                //זה הנכון יותר
                List<tblPlayerInTeam> playersInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == detail.TeamSerialNum && x.EmailPlayer != detail.EmailPlayer).ToList();
                //List<tblPlayerInTeam> playersInTeam = db.tblPlayerInTeams.Where(x => x.TeamSerialNum == detail.TeamSerialNum).ToList();
                foreach (tblPlayerInTeam p in playersInTeam)
                {
                    string expoToken = db.tblPlayers.Where(x => x.Email == p.EmailPlayer).Select(a => a.TokenNotfication).FirstOrDefault();
                    if (expoToken != null)
                    {
                        // Create POST data and convert it to a byte array.  
                        var objectToSend = new
                        {
                            to = expoToken,
                            title = "You Have New Message In Team - " + detail.TeamName,
                            body = detail.FirstName + ": " + detail.MessagePlayer,
                            //badge = pnd.badge,
                            data = new { name = "message", T_SerialNum = detail.TeamSerialNum, CreatedAt = detail.CreatedAt },
                            //new { name = "nir", grade = 100 }
                        };
                        tblPlayer.SendPushNotification(objectToSend);
                    }
                }
                return Ok("Messages sended succefully !");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }


        }
    }

}