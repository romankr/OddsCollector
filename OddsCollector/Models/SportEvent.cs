namespace OddsCollector.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Stores general information about a sport event.
/// </summary>
public class SportEvent
{
    /// <summary>
    /// Sport event ID, a primary key.
    /// </summary>
    [Key]
    public string? SportEventId { get; set; }

    /// <summary>
    /// Time when the event commences.
    /// </summary>
    [Required]
    public DateTime CommenceTime { get; set; }

    /// <summary>
    /// Name of the home team.
    /// </summary>
    [Required]
    public string? HomeTeam { get; set; }

    /// <summary>
    /// Name of the away team.
    /// </summary>
    [Required]
    public string? AwayTeam { get; set; }
    
    /// <summary>
    /// League Id.
    /// </summary>
    [Required]
    public string? LeagueId { get; set; }

    /// <summary>
    /// Winner team or "Draw".
    /// </summary>
    public string? Outcome { get; set; }

    /// <summary>
    /// A list of corresponding odds.
    /// </summary>
    public ICollection<Odd>? Odds { get; set; }
}