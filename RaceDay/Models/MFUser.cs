//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RaceDay.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class MFUser
    {
        public MFUser()
        {
            this.Attendings = new HashSet<Attending>();
            this.GroupMembers = new HashSet<GroupMember>();
        }
    
        public string UserId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public System.DateTime LastUpdate { get; set; }
    
        public virtual ICollection<Attending> Attendings { get; set; }
        public virtual ICollection<GroupMember> GroupMembers { get; set; }
    }
}
