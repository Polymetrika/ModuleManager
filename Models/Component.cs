using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class Component
    {
        public int ComponentId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? TemplateId { get; set; }
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// inherited from template to simplify lookup
        /// </summary>
        public ContentType ContentType { get; set; }

        public Status Status { get; set; }
        public int ModuleId { get; set; }
        
    }
    #endregion
}
