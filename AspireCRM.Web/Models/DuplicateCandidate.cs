using AspireCRM.Domain.Leads;

namespace AspireCRM.Web.Models;

public record DuplicateCandidate(long Id, string Name, string? Description, LeadStatus Status, double Similarity);
public record LeadConversionPreview(long LeadId, string LeadName, bool CanConvert, string[] AvailableTypes);
public record LeadConversionRequest(string? Comment, string? SaleName, double? SaleVolume, string? RelationshipTheme, string? RelationshipDescription, DateTime? RelationshipStartDate, DateTime? RelationshipEndDate);
public record LeadConversionResult(long LeadId, long ContractorId, long? SaleId, long? RelationshipId);