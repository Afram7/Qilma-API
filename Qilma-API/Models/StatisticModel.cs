using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qilma_API.Models;

public class StatisticModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StatisticId { get; set; }
    [Required]
    required public int OwnerId { get; set; }
    [Required]
    required public string OwnerType { get; set; }
    [Required]
    required public int GamesPlayed { get; set; }
    [Required]
    required public int GamesWon { get; set; }
}
