using System.Net;
using CompleteEFCore.API.Application.Enums;
using CompleteEFCore.API.Domain.Entities.Northwind;
using CompleteEFCore.API.Features.Common.Models.Response;
// using CompleteEFCore.API.Models;
using CompleteEFCore.BuildingBlocks.CQRS.Query;
using CompleteEFCore.BuildingBlocks.Result;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CompleteEFCore.API.Features.Versus.AdoVersusEf;

public record AdoVersusEfQuery(QueryToolType QueryToolType = QueryToolType.EFCore) : IQuery<Result<List<NorthwindCustomerDto>>>;

public class AdoVersusEfQueryHandler(NorthwindContext context,
                                     IConfiguration configuration) : IQueryHandler<AdoVersusEfQuery, Result<List<NorthwindCustomerDto>>>
{
    private readonly string _connectionString = configuration.GetSection("DbConnection:Northwind").Value;
    public async Task<Result<List<NorthwindCustomerDto>>> Handle(AdoVersusEfQuery request, CancellationToken cancellationToken)
    {
        var customers = request.QueryToolType switch
        {
            QueryToolType.EFCore => await GetCustomersByEFCoreAsync(),
            QueryToolType.AdoNET => await GetCustomersByAdoNETAsync()
        };
        
        var result = customers.Adapt<List<NorthwindCustomerDto>>();
        
        return Result<List<NorthwindCustomerDto>>.Success(result, (int)HttpStatusCode.OK);
    }

    private async Task<List<Customer>> GetCustomersByEFCoreAsync()
        => await context.Customers
                        .ToListAsync();

    private async Task<List<Customer>> GetCustomersByAdoNETAsync()
    {
        var customers = new List<Customer>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = "SELECT * FROM customers";

        await using var command = new NpgsqlCommand(query, connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(new Customer
            {
                CustomerId = reader["customer_id"].ToString(),
                CompanyName = reader["company_name"].ToString(),
                ContactName = reader["contact_name"].ToString(),
                ContactTitle = reader["contact_title"].ToString(),
                Address = reader["address"].ToString(),
                City = reader["city"].ToString(),
                Region = reader["region"].ToString(),
                PostalCode = reader["postal_code"].ToString(),
                Country = reader["country"].ToString(),
                Phone = reader["phone"].ToString(),
                Fax = reader["fax"].ToString()
            });
        }

        return customers;
    }
}