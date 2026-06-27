using AspireCRM.Domain.Sales;

namespace AspireCRM.Domain.Common;

public class CrmSettings : BaseEntity
{
    public long? DefaultCurrencyId { get; set; }
    public Currency? DefaultCurrency { get; set; }
    public string? DefaultCountry { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
}