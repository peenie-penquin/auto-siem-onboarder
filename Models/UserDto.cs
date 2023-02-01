using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSiem
{
    public class UserDto
    {
        [Required(ErrorMessage = "Name required")]
        public string Name { get; set; }

        [Required]
        [Range(1,65535)]
        public int Age { get; set; }
    }
}
