﻿using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class Module
    {
        public int ModuleId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? TemplateId { get; set; }
        public DateTime TimeStamp { get; set; }

        public ModuleStatus Status { get; set; }
        
    }

    public enum ModuleStatus
    {
        Submitted,
        Approved,
        Rejected
    }
    #endregion
}
