using System.ComponentModel.DataAnnotations;

namespace ModuleManager.Models
{
    #region snippet1
    public class LearningContent
    {
        public int LearningContentId { get; set; }

        // user ID from AspNetUser table.
        public string? OwnerID { get; set; }

        public string? Name { get; set; }
        public string? Details { get; set; }
        public string? TemplateId { get; set; }
        public DateTime TimeStamp { get; set; }
        public LearningContentType Type { get; set; }

        public LearningContentStatus Status { get; set; }
        
    }
    public enum LearningContentType
    {
        Module=1,
        Review=2,
        Assessment=3,
        Media=4,
        CommunityEngagement=5,
        StageGate=6
    }
    public enum LearningContentStatus
    {
        Submitted,
        Approved,
        Rejected
    }
    #endregion
}
