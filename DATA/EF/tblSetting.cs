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
    
    public partial class tblSetting
    {
        public int SettingSerialNum { get; set; }
        public string SettingName { get; set; }
        public string IsActivated { get; set; }
        public string EmailPlayer { get; set; }
    
        public virtual tblPlayer tblPlayer { get; set; }
    }
}
