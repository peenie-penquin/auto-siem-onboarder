using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoSiem.Models;
using System.ComponentModel.DataAnnotations;

namespace AutoSiem
{

    public class Node
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Hostname { get; set; }
        public string OS { get; set; }
        public string IpAddress { get; set; }
        public bool isSiemReachable { get; set; }
    }
    public class NodeDTO
    {
        public Guid Id { get; set; }
        public string Hostname { get; set; }
        public string OS { get; set; }
        [Required]
        public string IpAddress { get; set; }
    }

}