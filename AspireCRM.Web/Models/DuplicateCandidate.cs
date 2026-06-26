using AspireCRM.Domain.Leads;

namespace AspireCRM.Web.Models;

public record DuplicateCandidate(long Id, string Name, string? Description, LeadStatus Status, double Similarity);