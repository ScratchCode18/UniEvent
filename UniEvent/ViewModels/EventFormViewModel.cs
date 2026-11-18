using System.ComponentModel.DataAnnotations;

namespace UniEvent.ViewModels;

public class EventFormViewModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(150)]
    public string Venue { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Category { get; set; }
}
