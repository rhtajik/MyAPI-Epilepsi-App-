namespace MyAPI.Web.Models;

public class UserViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AssignedPatientId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserViewModel
{
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AssignedPatientId { get; set; }
}

public class EditUserViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AssignedPatientId { get; set; }
}