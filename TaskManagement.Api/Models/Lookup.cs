using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Models;

/// <summary>Lookup entity persisted in SQL Server.</summary>
public class Lookup
{
    public long Id { get; set; }

    public int MajorCode { get; set; }

    public int MinorCode { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
