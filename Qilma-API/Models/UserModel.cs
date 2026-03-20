using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qilma_API.Models;

public class UserModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    required public int UserId { get; set; }
    [Required]
    required public string Name { get; set; }
    [Required]
    required public int Age { get; set; }
    [Required]
    required public string Email { get; set; }
    [Required]
    required public string Password { get; set; }
}