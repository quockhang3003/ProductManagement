using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class University
    {
        public int Id { get; set; }
        [Required][Display(Name = "University")] 
        public string UniversityName { get; set; }

    }
}
