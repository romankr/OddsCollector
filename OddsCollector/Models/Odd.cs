namespace OddsCollector.Models;

using System.ComponentModel.DataAnnotations;

public class Odd
{
    [Required]
    public string? Bookmaker { get; set; }
        
    [Required]
    public DateTime LastUpdate { get; set; }

    [Required]
    public double HomeOdd { get; set; }

    [Required]
    public double DrawOdd { get; set; }

    [Required]
    public double AwayOdd { get; set; }

    [Required]
    public string? SportEventId { get; set; }
}