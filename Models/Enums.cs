namespace ModuleManager.Models
{

    public enum Status
    {
        Draft,
        Submitted,
        Approved,
        Rejected
    }

    public enum ContentType
    {
        Module = 1,
        BusinessProcess = 2,
        ReviewChecklist = 3,
        Experience = 4,
        MediaAsset = 5,
        LearningMaterial = 6
    }

    public enum ReleaseStatus
    {
        Inactive = 1,
        Active=2
    }
}
