namespace ModuleManager.Models
{
    public class Template
    {
        public string TemplateID { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public TemplateType TemplateType { get; set; }
    }
    public enum TemplateType
    {
        Module=1,
        Review=2
    }
}
