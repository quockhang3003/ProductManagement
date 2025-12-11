using System.ComponentModel.DataAnnotations;
namespace Domain.Entities;
public class Admin 
{ 
    public int Id { get; set; } 
    [Required] public string Username { get; set; } = string.Empty; 
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [EmailAddress] public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true; 
}