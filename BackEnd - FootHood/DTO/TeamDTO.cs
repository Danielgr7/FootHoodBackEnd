using BackEnd___FootHood.DTO.TeamFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BackEnd___FootHood.DTO
{
    public class TeamDTO
    {
        public int TeamSerialNum;
        public string TeamName;
        public string TeamPicture;
        public bool IsPrivate;
        public string RulesAndLaws;
        public string EmailManager;
        public List<PlayerInTeamDTO> PlayersList;
    }
}