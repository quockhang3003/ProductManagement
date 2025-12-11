using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class SessionInfoDTO
    {
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsUser {  get; set; }
        public string UserEmail {  get; set; } = string.Empty;
        public string AdminUsername {  get; set; }  = string.Empty;
        public int? UserId { get; set; }
        public int? AdminId { get; set; }
    }
}
