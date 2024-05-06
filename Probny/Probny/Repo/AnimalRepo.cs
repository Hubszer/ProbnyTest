using System.Data.SqlClient;
using Probny.Models;

namespace Probny.Prop;

public class AnimalRepo : IAnimalRepo
{
    private readonly IConfiguration _configuration;

    public AnimalRepo(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    
    public async Task<bool> DoesAnimalExists(int id)
    {
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM Animal WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }

    public async Task<bool> DoesOwnerExists(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM Owner WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }

    public async Task<bool> DoesProcedureExists(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = "SELECT 1 FROM [Procedure] WHERE ID = @ID";
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();
        return res is not null;
    }

    public async Task<AnimalDTO> GetAnimal(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = @"SELECT Animal.ID AS AnimalID,
                               Animal.Name AS AnimalName,
                               Type,
                               AdmissionDate,
                               Owner.ID AS OwnerID,
                               FirstName,
                               LastName,
                               Date,
                               [Procedure].Name AS ProcedureName,
                               Description
                                FROM Animal
                                JOIN Owner ON Owner.ID = Animal.Owner_ID
                                JOIN Procedure_Animal ON Procedure_Animal.Animal_ID = Animal.ID
                                JOIN [Procedure] ON [Procedure].ID = Procedure_Animal,Procedure_ID
                                WHERE Animal.ID = @ID
                               ";
        command.Parameters.AddWithValue("@ID", id);
        
        
        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();
        
        var animalIdOrdinal = reader.GetOrdinal("AnimalID");
        var animalNameOrdinal = reader.GetOrdinal("AnimalName");
        var animalTypeOrdinal = reader.GetOrdinal("Type");
        var admissionDateOrdinal = reader.GetOrdinal("AdmissionDate");
        var ownerIdOrdinal = reader.GetOrdinal("OwnerID");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");
        var dateOrdinal = reader.GetOrdinal("Date");
        var procedureNameOrdinal = reader.GetOrdinal("ProcedureName");
        var procedureDescriptionOrdinal = reader.GetOrdinal("Description");


        AnimalDTO animalDto = null;

        while (await reader.ReadAsync())
        {
            if (animalDto is not null)
            {
                animalDto.Procedures.Add(new ProcedureDTO()
                {
                    Date = reader.GetDateTime(dateOrdinal),
                    Name = reader.GetString(procedureNameOrdinal),
                    Description = reader.GetString(procedureDescriptionOrdinal)
                });
            }
            else
            {
                animalDto = new AnimalDTO()
                {
                    Id = reader.GetInt32(animalIdOrdinal),
                    Name = reader.GetString(animalNameOrdinal),
                    Type = reader.GetString(animalTypeOrdinal),
                    AdmissionDate = reader.GetDateTime(admissionDateOrdinal),
                    Owner = new OwnerDTO()
                    {
                        Id = reader.GetInt32(ownerIdOrdinal),
                        FirstName = reader.GetString(firstNameOrdinal),
                        LastName = reader.GetString(lastNameOrdinal),
                    },
                    Procedures = new List<ProcedureDTO>()
                    {
                        new ProcedureDTO()
                        {
                            Date = reader.GetDateTime(dateOrdinal),
                            Name = reader.GetString(procedureNameOrdinal),
                            Description = reader.GetString(procedureDescriptionOrdinal)
                        }
                    }
                };
            }
        }

        if (animalDto is null) throw new Exception();
        
        return animalDto;
    }

    public async Task AddNewAnimalWithProcedure(NewAnimalWithProcedures newAnimalWithProcedures)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = @"INSERT INTO Animal VALUES (@Name,@Type,@AdmissionDate,@OwnerId);
                                SELECT @@IDENTITY AS ID";

        command.Parameters.AddWithValue(@"Name",newAnimalWithProcedures.Name);
        command.Parameters.AddWithValue(@"Type",newAnimalWithProcedures.Type);
        command.Parameters.AddWithValue(@"AdmissionDate",newAnimalWithProcedures.AdmissionDate);
        command.Parameters.AddWithValue(@"OwnerId",newAnimalWithProcedures.OwnerId);

        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            var id = await command.ExecuteScalarAsync();
    
            foreach (var procedure in newAnimalWithProcedures.ProcedureWithDates)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO Procedure_Animal VALUES(@ProcedureId, @AnimalId, @Date)";
                command.Parameters.AddWithValue("@ProcedureId", procedure.ProcedureId);
                command.Parameters.AddWithValue("@AnimalId", id);
                command.Parameters.AddWithValue("@Date", procedure.Date);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddAnimal(NewAnimalDTO animalDto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = @"INSERT INTO Animal VALUES (@Name,@Type,@AdmissionDate,@OwnerId);
                                SELECT @@IDENTITY AS ID";
        command.Parameters.AddWithValue("@Name", animalDto.Name);
        command.Parameters.AddWithValue("@Type", animalDto.Type);
        command.Parameters.AddWithValue("@AdmissionDate", animalDto.AdmissionDate);
        command.Parameters.AddWithValue("@OwnerId", animalDto.OwnerId);


        await connection.OpenAsync();

        var id = await command.ExecuteScalarAsync();
        if (id is null )
        {
            throw new Exception();
        }

        return Convert.ToInt32(id);
    }

    public async Task AddProcedureAnimal(int animalId, ProcedureWithDate procedureWithDate)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = $"INSERT INTO Procedure_Animal VALUES (@ProcedureID,@AnimalId,@Date)";

        command.Parameters.AddWithValue("@ProcedureID", procedureWithDate.ProcedureId);
        command.Parameters.AddWithValue("@AnimalID", animalId);
        command.Parameters.AddWithValue("@Date", procedureWithDate.Date);
        
        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }
}