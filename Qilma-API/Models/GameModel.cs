using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qilma_API.Models;

public class GameModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GameId { get; set; }
    [Required]
    required public int OwnerId { get; set; }
    [Required]
    required public string OwnerType { get; set; }
    [Required]
    required public int Attempts { get; set; }
    [Required]
    required public string Word { get; set; }
    [Required]
    required public DateTime Date { get; set; }
    [Required]
    required public string Result { get; set; }
}
