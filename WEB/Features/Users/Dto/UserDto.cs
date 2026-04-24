namespace WEB.Features.Users.Dto;

public class UserDto
{
    public string Id { get; set; } 
    public string FullName{ get; set; } 
    public string Role{ get; set; }
    public string Email{ get; set; } 
    public bool EmailConfirmed{ get; set; }
    
    public int? ProcesoId { get; set; }
    public string? ProcesoNombre { get; set; }
    
    public int? AreaId { get; set; }
    public string? AreaNombre { get; set; }
};