using System.Runtime.InteropServices.JavaScript;

namespace Probny.Models;

public class NewAnimalWithProcedures
{
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTime AdmissionDate { get; set; }
    public int OwnerId { get; set; }
    public IEnumerable<ProcedureWithDate> ProcedureWithDates { get; set; } = new List<ProcedureWithDate>();
}

public class ProcedureWithDate
{
    public int ProcedureId { get; set; }
    public DateTime Date { get; set; }
}