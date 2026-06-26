using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Products;
using AspireCRM.Domain.Relationships;
using AspireCRM.Domain.Sales;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.DataLayer;

public class AspireCRMDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
{
    private readonly ITenantService _tenantService;

    public AspireCRMDbContext(DbContextOptions<AspireCRMDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Email> Emails => Set<Email>();
    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<ContractorLegal> ContractorLegals => Set<ContractorLegal>();
    public DbSet<ContractorIndividual> ContractorIndividuals => Set<ContractorIndividual>();
    public DbSet<ContractorRegion> ContractorRegions => Set<ContractorRegion>();
    public DbSet<ContractorIndustry> ContractorIndustries => Set<ContractorIndustry>();
    public DbSet<ContractorType> ContractorTypes => Set<ContractorType>();
    public DbSet<ClientType> ClientTypes => Set<ClientType>();
    public DbSet<ClientDocumentType> ClientDocumentTypes => Set<ClientDocumentType>();
    public DbSet<LegalForm> LegalForms => Set<LegalForm>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<PaymentCard> PaymentCards => Set<PaymentCard>();

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadSource> LeadSources => Set<LeadSource>();
    public DbSet<LeadType> LeadTypes => Set<LeadType>();
    public DbSet<LeadContact> LeadContacts => Set<LeadContact>();

    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleType> SaleTypes => Set<SaleType>();
    public DbSet<SaleStage> SaleStages => Set<SaleStage>();
    public DbSet<SaleFunnel> SaleFunnels => Set<SaleFunnel>();
    public DbSet<SaleProduct> SaleProducts => Set<SaleProduct>();
    public DbSet<Currency> Currencies => Set<Currency>();

    public DbSet<Inpayment> Inpayments => Set<Inpayment>();

    public DbSet<Relationship> Relationships => Set<Relationship>();
    public DbSet<RelationshipUser> RelationshipUsers => Set<RelationshipUser>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public override int SaveChanges()
    {
        ApplyAudit();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAudit();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAudit()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (_tenantService.TenantId.HasValue)
                    entry.Entity.TenantId = _tenantService.TenantId.Value;
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBaseEntity(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureProduct(modelBuilder);
        ConfigureContractorHierarchy(modelBuilder);
        ConfigureContact(modelBuilder);
        ConfigureLead(modelBuilder);
        ConfigureSale(modelBuilder);
        ConfigureInpayment(modelBuilder);
        ConfigureRelationshipHierarchy(modelBuilder);
        ConfigureLookups(modelBuilder);
        ConfigureTenant(modelBuilder);
        ConfigureIdentityTables(modelBuilder);
    }

    private void ConfigureBaseEntity(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType) && e.BaseType is null))
        {
            var builder = modelBuilder.Entity(entityType.ClrType);
            builder.HasKey(nameof(BaseEntity.Id));
            builder.Property(nameof(BaseEntity.Id)).ValueGeneratedOnAdd();
            builder.Property(nameof(BaseEntity.TenantId)).IsRequired();
            builder.Property(nameof(BaseEntity.CreatedAt)).IsRequired();
            builder.Property(nameof(BaseEntity.CreatedById)).IsRequired();
        }
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(256);
            e.Property(c => c.CategoryType).IsRequired().HasConversion<string>().HasMaxLength(50);
        });
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(512);
            e.Property(p => p.IsGroup).IsRequired();

            e.HasOne(p => p.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(p => p.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureContractorHierarchy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contractor>(e =>
        {
            e.ToTable("Contractors");
            e.Property(c => c.Name).IsRequired().HasMaxLength(512);
            e.Property(c => c.INN).HasMaxLength(100);

            e.HasDiscriminator<string>("ContractorType")
                .HasValue<Contractor>("Base")
                .HasValue<ContractorLegal>("Legal")
                .HasValue<ContractorIndividual>("Individual");

            e.HasOne(c => c.LegalAddress)
                .WithMany()
                .HasForeignKey(c => c.LegalAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.PostalAddress)
                .WithMany()
                .HasForeignKey(c => c.PostalAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Partner)
                .WithMany()
                .HasForeignKey(c => c.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(c => c.Emails)
                .WithMany(e => e.Contractors);

            e.HasMany(c => c.Phones)
                .WithMany(e => e.Contractors);

            e.HasMany(c => c.Comments)
                .WithMany(e => e.Contractors);

            e.HasMany(c => c.Categories)
                .WithMany(c => c.Contractors);

            e.HasMany(c => c.BankAccounts)
                .WithOne(ba => ba.Contractor)
                .HasForeignKey(ba => ba.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(c => c.PaymentCards)
                .WithOne(pc => pc.Contractor)
                .HasForeignKey(pc => pc.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContractorLegal>(e =>
        {
            e.Property(cl => cl.OGRN).HasMaxLength(100);
            e.Property(cl => cl.KPP).HasMaxLength(100);
        });

        modelBuilder.Entity<ContractorIndividual>(e =>
        {
            e.Property(ci => ci.FirstName).IsRequired().HasMaxLength(256);
            e.Property(ci => ci.DocumentSeries).HasMaxLength(50);
            e.Property(ci => ci.DocumentNumber).HasMaxLength(50);
            e.Property(ci => ci.DocumentIssued).HasMaxLength(512);
            e.Property(ci => ci.DocumentIssueDate).IsRequired();
        });

        modelBuilder.Entity<BankAccount>(e =>
        {
            e.Property(ba => ba.Number).IsRequired().HasMaxLength(100);
            e.Property(ba => ba.BIK).HasMaxLength(20);
            e.Property(ba => ba.BankName).HasMaxLength(256);
            e.Property(ba => ba.CorrespondentAccount).HasMaxLength(100);
            e.Property(ba => ba.Description).HasMaxLength(4000);
        });

        modelBuilder.Entity<PaymentCard>(e =>
        {
            e.Property(pc => pc.Number).IsRequired().HasMaxLength(50);
            e.Property(pc => pc.CardholderName).HasMaxLength(256);
            e.Property(pc => pc.Description).HasMaxLength(4000);
        });
    }

    private static void ConfigureContact(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(e =>
        {
            e.Ignore(c => c.Name);
            e.Property(c => c.Surname).IsRequired().HasMaxLength(256);
            e.Property(c => c.Firstname).IsRequired().HasMaxLength(256);
            e.Property(c => c.Middlename).HasMaxLength(256);
            e.Property(c => c.Department).HasMaxLength(256);
            e.Property(c => c.Position).HasMaxLength(256);
            e.Property(c => c.Site).HasMaxLength(512);
            e.Property(c => c.Priority).IsRequired().HasConversion<string>().HasMaxLength(50);

            e.HasOne(c => c.Contractor)
                .WithMany(c => c.Contacts)
                .HasForeignKey(c => c.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.RegistrationAddress)
                .WithMany()
                .HasForeignKey(c => c.RegistrationAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.ResidenceAddress)
                .WithMany()
                .HasForeignKey(c => c.ResidenceAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(c => c.Emails)
                .WithMany();

            e.HasMany(c => c.Phones)
                .WithMany();

            e.HasMany(c => c.Comments)
                .WithMany();

            e.HasMany(c => c.Relationships)
                .WithMany(r => r.ContractorsContacts);
        });
    }

    private static void ConfigureLead(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lead>(e =>
        {
            e.Property(l => l.Name).IsRequired().HasMaxLength(512);
            e.Property(l => l.Description).HasMaxLength(4000);
            e.Property(l => l.Site).HasMaxLength(512);
            e.Property(l => l.Status).IsRequired().HasConversion<string>().HasMaxLength(50);

            e.HasOne(l => l.Address)
                .WithMany()
                .HasForeignKey(l => l.AddressId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Contractor)
                .WithMany(c => c.Leads)
                .HasForeignKey(l => l.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(l => l.Emails)
                .WithMany(e => e.Leads);

            e.HasMany(l => l.Phones)
                .WithMany(e => e.Leads);

            e.HasMany(l => l.Comments)
                .WithMany(e => e.Leads);

            e.HasMany(l => l.Categories)
                .WithMany(c => c.Leads);
        });

        modelBuilder.Entity<LeadContact>(e =>
        {
            e.HasKey(lc => lc.Id);
            e.HasOne(lc => lc.Lead)
                .WithMany(l => l.Contacts)
                .HasForeignKey(lc => lc.LeadId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(lc => lc.Contact)
                .WithMany()
                .HasForeignKey(lc => lc.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSale(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(e =>
        {
            e.Property(s => s.Name).IsRequired().HasMaxLength(512);
            e.Property(s => s.ShortStatus).HasMaxLength(50);
            e.Property(s => s.Description).HasMaxLength(4000);
            e.Property(s => s.Priority).IsRequired().HasConversion<string>().HasMaxLength(50);
            e.Property(s => s.SaleStatus).IsRequired().HasConversion<string>().HasMaxLength(50);
            e.Property(s => s.StartDate).IsRequired();
            e.Property(s => s.CreationDate).IsRequired();

            e.HasOne(s => s.Contractor)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(s => s.Comments)
                .WithMany(c => c.Sales);

            e.HasMany(s => s.Relationships)
                .WithOne(r => r.Sale)
                .HasForeignKey(r => r.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(s => s.Inpayments)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SaleProduct>(e =>
        {
            e.HasKey(sp => sp.Id);
            e.HasOne(sp => sp.Sale)
                .WithMany(s => s.Products)
                .HasForeignKey(sp => sp.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(sp => sp.Product)
                .WithMany()
                .HasForeignKey(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureInpayment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inpayment>(e =>
        {
            e.Property(i => i.Name).IsRequired().HasMaxLength(512);
            e.Property(i => i.Comment).HasMaxLength(4000);
            e.Property(i => i.Sum).IsRequired().HasColumnType("decimal(18,2)");
            e.Property(i => i.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            e.Property(i => i.CreationDate).IsRequired();

            e.HasOne(i => i.Contractor)
                .WithMany(c => c.Inpayments)
                .HasForeignKey(i => i.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(i => i.Comments)
                .WithMany(c => c.Inpayments);
        });
    }

    private static void ConfigureRelationshipHierarchy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Relationship>(e =>
        {
            e.ToTable("Relationships");
            e.Property(r => r.Theme).IsRequired().HasMaxLength(512);
            e.Property(r => r.Description).HasMaxLength(4000);
            e.Property(r => r.Priority).IsRequired().HasConversion<string>().HasMaxLength(50);
            e.Property(r => r.StartDate).IsRequired();
            e.Property(r => r.EndDate).IsRequired();

            e.HasDiscriminator<string>("RelationshipType")
                .HasValue<Relationship>("Base")
                .HasValue<RelationshipCall>("Call")
                .HasValue<RelationshipMail>("Mail")
                .HasValue<RelationshipMeeting>("Meeting");

            e.HasOne(r => r.Contractor)
                .WithMany(c => c.Relationships)
                .HasForeignKey(r => r.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Sale)
                .WithMany(s => s.Relationships)
                .HasForeignKey(r => r.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Contact)
                .WithMany()
                .HasForeignKey(r => r.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Lead)
                .WithMany(l => l.Relationships)
                .HasForeignKey(r => r.LeadId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(r => r.Comments)
                .WithMany(c => c.Relationships);

            e.HasMany(r => r.LeadContacts)
                .WithMany();
        });

        modelBuilder.Entity<RelationshipCall>(e =>
        {
            e.Property(rc => rc.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
            e.Property(rc => rc.UniqueId).HasMaxLength(256);
        });

        modelBuilder.Entity<RelationshipUser>(e =>
        {
            e.HasKey(ru => ru.Id);
            e.Property(ru => ru.Status).IsRequired().HasConversion<string>().HasMaxLength(50);

            e.HasOne(ru => ru.Relationship)
                .WithMany(r => r.RelationshipUsers)
                .HasForeignKey(ru => ru.RelationshipId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureLookups(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LeadSource>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<LeadType>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.HasMany(x => x.Leads).WithMany();
        });
        modelBuilder.Entity<ContractorRegion>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<ContractorIndustry>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<ContractorType>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<ClientType>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<ClientDocumentType>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<LegalForm>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<Currency>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(128);
            e.Property(x => x.Code).IsRequired().HasMaxLength(16);
        });
        modelBuilder.Entity<SaleType>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
        modelBuilder.Entity<SaleStage>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.SortOrder).IsRequired();
        });
        modelBuilder.Entity<SaleFunnel>(e => e.Property(x => x.Name).IsRequired().HasMaxLength(256));
    }

    private static void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.ToTable("AspNetUsers");
            e.Property(u => u.TenantId).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(256);
            e.Property(u => u.LastName).HasMaxLength(256);
        });
    }

    private static void ConfigureTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants");
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(256);
            e.Property(t => t.Code).IsRequired().HasMaxLength(50);
            e.HasIndex(t => t.Code).IsUnique();
        });
    }
}