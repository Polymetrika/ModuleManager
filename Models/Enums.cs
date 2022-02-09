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
        ReviewChecklist = 2,
        MediaAsset = 3,
        Content = 4,
        StoryBoard=5
    }

    public enum ReleaseStatus
    {
        Inactive = 0,
        Active=1
    }
}
