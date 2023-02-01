
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoSiem.Models;

namespace AutoSiem
{
    public class Platform
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; } // Name of the platform
        public string Division { get; set; } // Which department the platform resides 
        public Siem Siem { get; set; } // Which siem is onboarded to
        public List<User> SMEs { get; set; } // The platform contact engineers
        public List<User> Owners { get; set; } // The platform owner
        public DateTime DateOnboarded { get; set; }
        public User ApprovedBy { get; set; } // Person who approved that the onboard is done
        public Category Category { get; set; } // Severity of platform
        public string Description { get; set; } // A short definition on what the platform is used for
        public ScriptSettings Settings { get; set; } 
        public List<Node> Nodes { get; set; } 

        [NotMapped]
        public bool isOnboarded
        {
            get { return this.DateOnboarded.Year > 1 ? true : false; }
        }
    }

    public enum Category
    {
        None,
        CAT1,       // Most Critial, Confidential Data
        CAT2,
        CAT3,
        CAT4        // Least Critical
    }
}