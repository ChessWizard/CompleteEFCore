namespace CompleteEFCore.API.Features.Common.Models.Response;

public record NorthwindCustomerDto(
    string CustomerId,
    string CompanyName,
    string ContactName,
    string ContactTitle,
    string Address,
    string City,
    string Region,
    string PostalCode,
    string Country,
    string Phone,
    string Fax
);