using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Probny.Models;
using Probny.Prop;

namespace Probny;

[Route("api/[controller]")]
[ApiController]
public class AnimalController : ControllerBase
{
    private readonly IAnimalRepo _animalRepo;

    public AnimalController(IAnimalRepo animalRepo)
    {
        _animalRepo = animalRepo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAnimal(int id)
    {
        if (!await _animalRepo.DoesOwnerExists(id))
        {
            return NotFound($"Animal with given ID - {id} doesnt exist");
        }

        var animal = await _animalRepo.GetAnimal(id);
        return Ok(animal);
    }

    [HttpPost]
    public async Task<IActionResult> AddAnimal(NewAnimalWithProcedures newAnimalWithProcedures)
    {
        if (!await _animalRepo.DoesOwnerExists(newAnimalWithProcedures.OwnerId))
        {
            return NotFound($"Owner with given ID - {newAnimalWithProcedures.OwnerId} doesn't exist");
        }

        await _animalRepo.AddNewAnimalWithProcedure(newAnimalWithProcedures);

        return Created(Request.Path.Value ?? "api/animals", newAnimalWithProcedures);
    }

    [HttpPost]
    public async Task<IActionResult> AddAnimalv2(NewAnimalWithProcedures newAnimalWithProcedures)
    {
        if (!await _animalRepo.DoesOwnerExists(newAnimalWithProcedures.OwnerId))
            return NotFound($"Owner with given ID - {newAnimalWithProcedures.OwnerId} doesn't exist");

        foreach (var procedure in newAnimalWithProcedures.ProcedureWithDates)
        {
            if (!await _animalRepo.DoesProcedureExists(procedure.ProcedureId))
                return NotFound($"Procedure with given ID - {procedure.ProcedureId} doesn't exist");
        }

        using(TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var id = await (_animalRepo).AddAnimal(new NewAnimalDTO()
            {
                Name = newAnimalWithProcedures.Name,
                Type = newAnimalWithProcedures.Type,
                AdmissionDate = newAnimalWithProcedures.AdmissionDate,
                OwnerId = newAnimalWithProcedures.OwnerId
            });

            foreach (var procedure in newAnimalWithProcedures.ProcedureWithDates)
            {
                await _animalRepo.AddProcedureAnimal(id, procedure);
            }

            scope.Complete();
        }

        return Created(Request.Path.Value ?? "api/animals", newAnimalWithProcedures);
    }

}
