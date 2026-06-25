namespace AspireCRM.Domain.Contractors;

public class ContractorLegal : Contractor
{
    public long? LegalFormId { get; set; }
    public long? Staff { get; set; }
    public string? OGRN { get; set; }
    public string? KPP { get; set; }
}