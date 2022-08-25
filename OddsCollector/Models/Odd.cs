namespace OddsCollector.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Stores data about bookmakers and odds for a particular event.
/// </summary>
public class Odd
{
    /// <summary>
    /// Bookmaker ID.
    /// </summary>
    [Required]
    public string? Bookmaker { get; set; }
    
    /// <summary>
    /// Date when information was received.
    /// </summary>
    [Required]
    public DateTime LastUpdate { get; set; }

    /// <summary>
    /// Home team odd.
    /// </summary>
    [Required]
    public double HomeOdd { get; set; }

    /// <summary>
    /// Draw odd.
    /// </summary>
    [Required]
    public double DrawOdd { get; set; }

    /// <summary>
    /// Away team odd.
    /// </summary>
    [Required]
    public double AwayOdd { get; set; }

    /// <summary>
    /// ID of the sport event.
    /// </summary>
    [Required]
    public string? SportEventId { get; set; }
}