using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class Review
    {
        public int ReviewId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? TemplateId { get; set; }
        public DateTime TimeStamp { get; set; }

        public ReviewStatus Status { get; set; }
        
    }

    public enum ReviewStatus
    {
        Submitted,
        Approved,
        Rejected
    }
    #endregion
}
