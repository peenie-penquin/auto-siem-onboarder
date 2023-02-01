
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoSiem.Models;

namespace AutoSiem
{
    public class OnboardTicket
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Title { get; set; } // Title of the case
        public string Description { get; set; } // A text explaining what needs to be done
        public Platform Platform { get; set; } // The platform information
        public User Creator { get; set; } // Ticket creator 
        public DateTime Created { get; set; } // Date created
        public DateTime Closed { get; set; }  // Date closed / finish / unfinish
        public List<User> Assignees { get; set; } // People who are assigned the task
        public List<Comment> Comments { get; set; } // Chat session under the ticket 
    }

}