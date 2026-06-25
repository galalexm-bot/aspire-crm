using AspireCRM.Domain.Common;
using AspireCRM.Domain.Relationships;

namespace AspireCRM.Domain.Contractors;

public class Contact : BaseEntity
{
    public string Surname { get; set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string? Middlename { get; set; }
    public string Name => $"{Firstname} {Middlename} {Surname}";
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Description { get; set; }
    public string? Site { get; set; }
    public string? Skype { get; set; }
    public long? Icq { get; set; }
    public long? Day { get; set; }
    public long? Month { get; set; }
    public long? Year { get; set; }
    public DateTime? Birthday { get; set; }

    public ContactPriority Priority { get; set; } = ContactPriority.Medium;

    public long ContractorId { get; set; }
    public Contractor Contractor { get; set; } = null!;
    public long? ClientTypeId { get; set; }
    public long? RegistrationAddressId { get; set; }
    public Address? RegistrationAddress { get; set; }
    public long? ResidenceAddressId { get; set; }
    public Address? ResidenceAddress { get; set; }
    public long? CreationAuthorId { get; set; }
    public long? ChangeAuthorId { get; set; }
    public long? NextRelationshipId { get; set; }

    public DateTime? CreationDate { get; set; }
    public DateTime? ChangeDate { get; set; }

    public ICollection<Email> Emails { get; set; } = new List<Email>();
    public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Relationship> Relationships { get; set; } = new List<Relationship>();
}