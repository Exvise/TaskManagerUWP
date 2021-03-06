﻿using System.ComponentModel.DataAnnotations;

namespace ServerAPI.Data.Models
{
    public class Status
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
