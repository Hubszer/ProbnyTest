using Probny.Models;

namespace Probny.Prop;

public interface IAnimalRepo
{
    Task<bool> DoesAnimalExists(int id);
    Task<bool> DoesOwnerExists(int id);
    Task<bool> DoesProcedureExists(int id);
    Task<AnimalDTO> GetAnimal(int id);

    Task AddNewAnimalWithProcedure(NewAnimalWithProcedures newAnimalWithProcedures);

    Task<int> AddAnimal(NewAnimalDTO animalDto);
    Task AddProcedureAnimal(int animalId, ProcedureWithDate procedureWithDate);
}