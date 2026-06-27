using AspireCRM.Domain.Common;

namespace AspireCRM.Web.Models;

public class CrmSettingsDto
{
    public long Id { get; set; }
    public long? DefaultCurrencyId { get; set; }
    public string? DefaultCurrencyName { get; set; }
    public string? DefaultCountry { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
}

public class UpdateCrmSettingsRequest
{
    public long? DefaultCurrencyId { get; set; }
    public string? DefaultCountry { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
}