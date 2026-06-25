namespace AspireCRM.Domain.Common;

public class Address : BaseEntity
{
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? Building { get; set; }
    public string? Apartment { get; set; }
    public string? ZipCode { get; set; }
    public string? FullAddress { get; set; }
}