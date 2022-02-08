namespace ModuleManager.Models
{
    public class Template
    {
        public string TemplateId { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public ContentType ContentType { get; set; }
        public ReleaseStatus ReleaseStatus { get; set; }
    }


}
