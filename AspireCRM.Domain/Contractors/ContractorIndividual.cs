namespace AspireCRM.Domain.Contractors;

public class ContractorIndividual : Contractor
{
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string? MiddleName { get; set; }
    public long? DocumentTypeId { get; set; }
    public string? DocumentSeries { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentIssued { get; set; }
    public DateTime DocumentIssueDate { get; set; }
    public DateTime? DocumentEndDate { get; set; }
}