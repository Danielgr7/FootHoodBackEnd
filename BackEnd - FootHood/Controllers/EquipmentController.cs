using BackEnd___FootHood.DTO;
using BackEnd___FootHood.DTO.GameFunctions;
using DATA.EF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;



namespace BackEnd___FootHood.Controllers
{
    [RoutePrefix("api/equipment")]
    public class EquipmentController : ApiController
    {
        //    Post api/<controller>/{object

        [HttpPost]
        [Route("GetAllEquipments")]
        public IHttpActionResult GetAllEquipments([FromBody] Equipments4GameDTO eg)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblEquipmentInGame> eiG = db.tblEquipmentInGames.Where(x => x.GameSerialNum == eg.GameSerialNum).ToList();
                if (eiG.Count > 0)
                {
                    List<EquipmentDTO> allEquipments4Game = new List<EquipmentDTO>();
                    foreach (tblEquipmentInGame e in eiG)
                    {
                        var equipment = db.tblEquipments.Where(x => x.EquipmentSerialNum == e.EquipmentSerialNum).FirstOrDefault();

                        if (equipment != null)
                        {
                            allEquipments4Game.Add(new EquipmentDTO()
                            {
                                EquipmentName = equipment.EquipmentName,
                                EquipmentSerialNum = equipment.EquipmentSerialNum
                            });
                        }
                    }
                    return Content(HttpStatusCode.OK, allEquipments4Game);

                }
                return Content(HttpStatusCode.OK, "There are no Equipments for this game");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.OK, ex);
            }
        }


        [HttpPost]
        [Route("GetItemsAssignForGame")]
        public IHttpActionResult GetItemsAssignForGame([FromBody] PlayerAssignedEquipmentDTO assignedEquip)
        {
            try
            {
                FootHoodDBContext db = new FootHoodDBContext();
                List<tblPlayerRegisteredToGame> playersBringItems = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == assignedEquip.GameSerialNum && x.BringItems != null).ToList();
                if (playersBringItems.Count > 0)
                {
                    List<PlayerAssignedEquipmentDTO> assignedEquipments = new List<PlayerAssignedEquipmentDTO>();
                    foreach (tblPlayerRegisteredToGame pbi in playersBringItems)
                    {
                        PlayerAssignedEquipmentDTO ase = new PlayerAssignedEquipmentDTO()
                        {
                            BringItems = pbi.BringItems,
                            EmailPlayer = pbi.EmailPlayer,
                            GameSerialNum = pbi.GameSerialNum,
                        };
                        assignedEquipments.Add(ase);
                    }
                    return Content(HttpStatusCode.OK, assignedEquipments);
                }
                else
                {
                    return Ok("No items were assigned yet");

                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPost]
        [Route("AssignEquipment2Player")]
        public IHttpActionResult AssignEquipment2Player([FromBody] RegisterGameDTO rgame)
        {
            FootHoodDBContext db = new FootHoodDBContext();

            try
            {
                tblPlayerRegisteredToGame playerRegister2game = db.tblPlayerRegisteredToGames.Where(x => x.GameSerialNum == rgame.GameSerialNum && x.EmailPlayer.ToLower() == rgame.EmailPlayer.ToLower()).FirstOrDefault();
                if (playerRegister2game != null)
                {
                    playerRegister2game.BringItems = rgame.BringItems;
                    db.SaveChanges();
                    return Ok("The equipment was assigned successfully");
                }
                else
                {
                    return Ok("The Player does not exists");
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
                return AssignEquipment2Player(rgame);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Route("AddNewItem")]
        public IHttpActionResult AddNewItem([FromBody] Equipments4GameDTO equip)
        {
            FootHoodDBContext db = new FootHoodDBContext();

            try
            {
                var equipment = db.tblEquipments.Where(x => x.EquipmentName.ToLower() == equip.EquipmentName.ToLower()).FirstOrDefault();
                if (equipment == null)//if the equipment does not exist add to app, and to game
                {
                    tblEquipment eqp = new tblEquipment();
                    eqp.EquipmentName = equip.EquipmentName;
                    db.tblEquipments.Add(eqp);
                    db.SaveChanges();
                    var equipmentInGame = db.tblEquipments.Where(x => x.EquipmentName.ToLower() == eqp.EquipmentName.ToLower()).FirstOrDefault();
                    if (equipmentInGame != null)
                    {
                        tblEquipmentInGame equipInGame = new tblEquipmentInGame();
                        equipInGame.EquipmentSerialNum = equipmentInGame.EquipmentSerialNum;
                        equipInGame.GameSerialNum = equip.GameSerialNum;
                        db.tblEquipmentInGames.Add(equipInGame);
                        db.SaveChanges();
                        return Ok("The Equipment Has Been Added Succesfully To The Game");
                    }
                    return Ok("Somthing went wrong in add new item to the Game");

                }
                //if equipment exist is app but not in the list of equipments in game, add to game
                else if (equipment != null && db.tblEquipmentInGames.Where(x => x.EquipmentSerialNum == equipment.EquipmentSerialNum && x.GameSerialNum == equip.GameSerialNum).FirstOrDefault() == null)
                {
                    tblEquipmentInGame equipInGame = new tblEquipmentInGame();
                    equipInGame.EquipmentSerialNum = equipment.EquipmentSerialNum;
                    equipInGame.GameSerialNum = equip.GameSerialNum;
                    db.tblEquipmentInGames.Add(equipInGame);
                    db.SaveChanges();
                    return Ok("The Equipment Has Been Added Succesfully To The Game");
                }
                else
                {
                    return Ok("The Equipment already exists in the app");
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
                return AddNewItem(equip);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex);
            }
        }

    }
}
