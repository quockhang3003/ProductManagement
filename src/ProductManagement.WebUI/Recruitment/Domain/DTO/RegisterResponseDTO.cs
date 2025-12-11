using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class RegisterResponseDTO
    {
        public string Message { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public bool AutoLogin { get; set; }
    }
}
