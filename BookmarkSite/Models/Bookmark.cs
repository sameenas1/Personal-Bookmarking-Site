using System;
using System.ComponentModel.DataAnnotations;

namespace BookmarkSite.Models
{
    public class Bookmark
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required, Url]
        public string Url { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}
