namespace OddsCollector.Models
{
    using System.ComponentModel.DataAnnotations;

    public class SportEvent
    {
        [Key]
        public string? SportEventId { get; set; }

        [Required]
        public DateTime CommenceTime { get; set; }

        [Required]
        public string? HomeTeam { get; set; }

        [Required]
        public string? AwayTeam { get; set; }
        
        [Required]
        public string? LeagueId { get; set; }

        public string? Outcome { get; set; }

        public ICollection<Odd>? Odds { get; set; }
    }
}