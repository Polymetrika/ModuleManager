using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class Process
    {
        public string ProcessId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        
        public string? Name { get; set; }
        /// <summary>
        /// Manual data fields describing appropriate production schedules (time and resource requirements) for each of the templated items
        /// </summary>
        public string? Details { get; set; }
        /// <summary>
        /// JSON array of template required by the process. Each template definition
        /// </summary>
        public string? RequiredModuleTemplates { get; set; }

        public DateTime TimeStamp { get; set; }

        public ReleaseStatus ReleaseStatus { get; set; }

    }
 
    #endregion
}
