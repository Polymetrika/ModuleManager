using System.ComponentModel.DataAnnotations;

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

        public string? BusinessDetails { get; set; }
        public string? TemplateId { get; set; }
        public int? ProcessId { get; set; }
        public DateTime TimeStamp { get; set; }     = DateTime.UtcNow;

        public Status Status { get; set; }
        public virtual ICollection<Component> Components { get; set; } = new List<Component>();
        
    }

    #endregion
}
