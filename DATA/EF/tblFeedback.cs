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
    
    public partial class tblFeedback
    {
        public int FeedBackSerialNum { get; set; }
        public string FeedbackContext { get; set; }
        public System.DateTime FeedbackSendDate { get; set; }
        public string EmailPlayer { get; set; }
        public string FeedBackSubject { get; set; }
    
        public virtual tblPlayer tblPlayer { get; set; }
    }
}