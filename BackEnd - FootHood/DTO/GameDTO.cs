using BackEnd___FootHood.DTO.GameFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO
{
    public class GameDTO
    {
        public int GameSerialNum;
        public int NumOfTeams;
        public int NumOfPlayersInTeam;
        public string GameLocation;
        public DateTime GameDate;
        public TimeSpan GameTime;

        public DateTime LastRegistrationDate;
        public TimeSpan LastRegistrationTime;
        public int? AvgPlayerAge;
        public int? AvgPlayerRating;
        public DateTime CreatedTimeTable;

        public List<EquipmentsInGameDTO> equipmentsInGame;
        public int ?TeamSerialNum;
        public bool FindPlayersActive;
        public double? GameLatitude;
        public double? GameLongitude;
    }
}