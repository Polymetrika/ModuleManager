using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class BusinessProcess
    {
        public int BusinessProcessId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        public string? Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? Details { get; set; }
        /// <summary>
        /// JSON array of templates required by the process
        /// </summary>
        public string? RequiredTemplates { get; set; }

        public DateTime TimeStamp { get; set; }

        public LearningContentStatus Status { get; set; }
        
    }
 
    #endregion
}
