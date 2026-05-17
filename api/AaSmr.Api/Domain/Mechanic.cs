namespace AaSmr.Api.Domain;

public class Mechanic
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public ICollection<WorkNote> WorkNotes { get; set; } = [];
}
