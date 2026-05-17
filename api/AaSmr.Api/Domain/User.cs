namespace AaSmr.Api.Domain;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? MechanicId { get; set; }
    public Mechanic? Mechanic { get; set; }
}
