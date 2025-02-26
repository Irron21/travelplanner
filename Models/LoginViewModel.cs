using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public required string email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string password { get; set; }
}
