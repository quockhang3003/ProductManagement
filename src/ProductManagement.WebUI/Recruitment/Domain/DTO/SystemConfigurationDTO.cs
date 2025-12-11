using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class SystemConfigurationDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSystemConfigurationDTO
    {
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Display name is required")]
        public string DisplayName { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateSystemConfigurationDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Display name is required")]
        public string DisplayName { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }

    public class SystemConfigurationGroupDTO
    {
        public string Type { get; set; }
        public List<SystemConfigurationDTO> Items { get; set; }
    }
}
