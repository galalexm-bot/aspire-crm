namespace AspireCRM.Web.Models;

public record UpdateContractorRequest(
    string Name,
    string? Description,
    string? Fax,
    string? Site,
    string? INN,
    double? AnnualIncome,
    DateTime? CompanyDay,
    long? ResponsibleId,
    long? RegionId,
    long? IndustryId,
    long? ContractorTypeId,
    long? LegalFormId,
    long? Staff,
    string? OGRN,
    string? KPP,
    string? FirstName,
    string? SecondName,
    string? MiddleName,
    long? DocumentTypeId,
    string? DocumentSeries,
    string? DocumentNumber,
    string? DocumentIssued,
    DateTime? DocumentIssueDate,
    DateTime? DocumentEndDate,
    AddressDto? LegalAddress,
    AddressDto? PostalAddress
);

public record AddressDto(
    string? Country,
    string? City,
    string? Street,
    string? Building,
    string? Apartment,
    string? ZipCode,
    string? FullAddress
);