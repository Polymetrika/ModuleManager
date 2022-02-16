using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class Module
    {
        public string ModuleId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        public string? Name { get; set; }
        /// <summary>
        /// Edited with the module template
        /// </summary>
        public string? Details { get; set; }
        /// <summary>
        /// edited with the Process template
        /// </summary>

        public string? BusinessDetails { get; set; }
        public string? TemplateId { get; set; }
        public string? ProcessId { get; set; }
        public DateTime TimeStamp { get; set; }     = DateTime.UtcNow;

        public Status Status { get; set; }
        public virtual ICollection<Component> Components { get; set; } = new List<Component>();
        
    }

    #endregion
}
