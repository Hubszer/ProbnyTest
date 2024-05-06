namespace Probny.Models;

public class AnimalDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public OwnerDTO Owner { get; set; } = null!;
    public List<ProcedureDTO> Procedures { get; set; } = null!;

}

public class OwnerDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class ProcedureDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}