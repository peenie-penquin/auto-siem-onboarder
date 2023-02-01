
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoSiem.Models;
using System.ComponentModel.DataAnnotations;

namespace AutoSiem
{
    public class ScriptSettings
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        
        [Required]
        public OS OperatingSystem { get; set; }

        [Required]
        public bool isSystemLogs {get; set;}

        public string CustomLogPaths { get; set; } // Paths separated by : if more than one
    }

    public class ScriptUploadDTO 
    {
        [Range(0,10)]
        public int OperatingSystem { get; set; } 

        [Required]
        public string ScriptContent { get; set; } 
    }

    public enum OS
    {
        Debian,
        Ubuntu,
        CentOS,
        RHEL,
        Arch,
        Windows10,
        Windows7,
        WindowsXP,
        WindowsNT,
        Windows2000,
        Unknown,
    }
}