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
    
    public partial class tblMessageSender
    {
        public int MessageSenderId { get; set; }
        public int TeamSerialNum { get; set; }
        public string EmailPlayer { get; set; }
        public int MessageSerialNum { get; set; }
    
        public virtual tblChat tblChat { get; set; }
        public virtual tblPlayerInTeam tblPlayerInTeam { get; set; }
    }
}
