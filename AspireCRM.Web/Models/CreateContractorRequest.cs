namespace AspireCRM.Web.Models;

public class CreateContractorRequest
{
    public string Name { get; set; } = "";
    public string SubType { get; set; } = "Legal";
    public string? Description { get; set; }
    public string? Fax { get; set; }
    public string? Site { get; set; }
    public string? INN { get; set; }
    public double? AnnualIncome { get; set; }
    public DateTime? CompanyDay { get; set; }
    public long? ResponsibleId { get; set; }
    public long? RegionId { get; set; }
    public long? IndustryId { get; set; }
    public long? ContractorTypeId { get; set; }
    public long? LegalFormId { get; set; }
    public long? Staff { get; set; }
    public string? OGRN { get; set; }
    public string? KPP { get; set; }
    public string? FirstName { get; set; }
    public string? SecondName { get; set; }
    public string? MiddleName { get; set; }
    public long? DocumentTypeId { get; set; }
    public string? DocumentSeries { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentIssued { get; set; }
    public DateTime? DocumentIssueDate { get; set; }
    public DateTime? DocumentEndDate { get; set; }
}