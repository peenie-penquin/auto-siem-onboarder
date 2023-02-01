
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoSiem.Models;
using System.ComponentModel.DataAnnotations;

namespace AutoSiem
{
    public class Siem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Enter a valid ipv4")]
        public string IpAddress { get; set; }

        [Required]
        [Range(1,65535)]
        public int Port { get; set; }
    }
}