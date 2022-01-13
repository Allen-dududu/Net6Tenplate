using System.ComponentModel.DataAnnotations;

namespace Net6TemplateWebApi
{
    public class Net6TemplateWebApiSettings
    {
        public const string Net6TemplateWebApiSettingsName = "Connections";
        [Required]
        public string ConnectionString { get; set; }
    }
}
