//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DATA.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblGame
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblGame()
        {
            this.tblEquipmentInGames = new HashSet<tblEquipmentInGame>();
            this.tblJoinRequests = new HashSet<tblJoinRequest>();
            this.tblPlayerRegisteredToGames = new HashSet<tblPlayerRegisteredToGame>();
            this.tblRatingOfGames = new HashSet<tblRatingOfGame>();
            this.tblHotGamesMatches = new HashSet<tblHotGamesMatch>();
        }
    
        public int GameSerialNum { get; set; }
        public int NumOfTeams { get; set; }
        public int NumOfPlayersInTeam { get; set; }
        public string GameLocation { get; set; }
        public System.DateTime GameDate { get; set; }
        public System.TimeSpan GameTime { get; set; }
        public System.DateTime LastRegistrationDate { get; set; }
        public System.TimeSpan LastRegistrationTime { get; set; }
        public Nullable<int> AvgPlayerAge { get; set; }
        public Nullable<int> AvgPlayerRating { get; set; }
        public Nullable<int> TeamSerialNum { get; set; }
        public Nullable<System.DateTime> CreatedTableTime { get; set; }
        public bool FindPlayersActive { get; set; }
        public Nullable<double> GameLatitude { get; set; }
        public Nullable<double> GameLongitude { get; set; }
        public byte[] RowVersion { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblEquipmentInGame> tblEquipmentInGames { get; set; }
        public virtual tblTeam tblTeam { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblJoinRequest> tblJoinRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPlayerRegisteredToGame> tblPlayerRegisteredToGames { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblRatingOfGame> tblRatingOfGames { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblHotGamesMatch> tblHotGamesMatches { get; set; }
    }
}