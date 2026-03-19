namespace Qilma_API.Models;

public class UserModel
{
    required public int UserId { get; set; }
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
    required public string Password { get; set; }
}