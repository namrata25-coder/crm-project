using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CrmLeadManagement.Models;

/// <summary>
/// EF Core database context for PostgreSQL.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<UserRoleEntry> Users => Set<UserRoleEntry>();
    public DbSet<LeadStatusOption> StatusOptions => Set<LeadStatusOption>();
    public DbSet<LeadSourceOption> SourceOptions => Set<LeadSourceOption>();
    public DbSet<RequirementTypeOption> RequirementTypeOptions => Set<RequirementTypeOption>();
    public DbSet<MasterData> MasterData => Set<MasterData>();
    public DbSet<FollowUpSettings> FollowUpSettings => Set<FollowUpSettings>();
    public DbSet<TaxMaster> TaxMasters => Set<TaxMaster>();
    public DbSet<RoleMaster> RoleMasters => Set<RoleMaster>();
    public DbSet<AccessToken> AccessTokens => Set<AccessToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<EstimationEntry> Estimations => Set<EstimationEntry>();
    public DbSet<QuotationEntry> Quotations => Set<QuotationEntry>();
    public DbSet<ProposalEntry> Proposals => Set<ProposalEntry>();
    public DbSet<FinanceLineItem> FinanceLineItems => Set<FinanceLineItem>();

    // Work Management
    public DbSet<WorkProject>          WorkProjects          => Set<WorkProject>();
    public DbSet<WorkMilestone>        WorkMilestones        => Set<WorkMilestone>();
    public DbSet<WorkTask>             WorkTasks             => Set<WorkTask>();
    public DbSet<WorkInvoice>          WorkInvoices          => Set<WorkInvoice>();
    public DbSet<WorkPayment>          WorkPayments          => Set<WorkPayment>();
    public DbSet<WorkRecurringInvoice> WorkRecurringInvoices => Set<WorkRecurringInvoice>();
    public DbSet<WorkFile>             WorkFiles             => Set<WorkFile>();
    public DbSet<WorkTimesheet>        WorkTimesheets        => Set<WorkTimesheet>();
    public DbSet<WorkExpense>          WorkExpenses          => Set<WorkExpense>();
    public DbSet<WorkNote>             WorkNotes             => Set<WorkNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var lead = modelBuilder.Entity<Lead>();
        lead.HasKey(l => l.Id);
        lead.HasIndex(l => l.LeadId).IsUnique();

        lead.OwnsMany(l => l.CallHistory,       b => b.WithOwner().HasForeignKey("LeadId_FK"));
        lead.OwnsMany(l => l.FollowUps,         b => b.WithOwner().HasForeignKey("LeadId_FK"));
        lead.OwnsMany(l => l.Requirements, b =>
        {
            b.WithOwner().HasForeignKey("LeadId_FK");
            b.HasOne<MasterData>().WithMany().HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne<MasterData>().WithMany().HasForeignKey(r => r.ItemId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne<MasterData>().WithMany().HasForeignKey(r => r.TypeId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne<MasterData>().WithMany().HasForeignKey(r => r.TopologyId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne<MasterData>().WithMany().HasForeignKey(r => r.GlassId).OnDelete(DeleteBehavior.NoAction);
        });
        lead.OwnsMany(l => l.Visits,            b => b.WithOwner().HasForeignKey("LeadId_FK"));
        lead.OwnsMany(l => l.AssignmentHistory, b => b.WithOwner().HasForeignKey("LeadId_FK"));

        lead.HasMany(l => l.Estimations).WithOne().HasForeignKey("LeadId_FK").OnDelete(DeleteBehavior.Cascade);
        lead.Navigation(l => l.Estimations).AutoInclude();

        lead.HasMany(l => l.Quotations).WithOne().HasForeignKey("LeadId_FK").OnDelete(DeleteBehavior.Cascade);
        lead.Navigation(l => l.Quotations).AutoInclude();

        lead.HasMany(l => l.Proposals).WithOne().HasForeignKey("LeadId_FK").OnDelete(DeleteBehavior.Cascade);
        lead.Navigation(l => l.Proposals).AutoInclude();

        modelBuilder.Entity<EstimationEntry>(est =>
        {
            est.ToTable("Estimations");
            est.HasKey(e => e.Id);
            est.HasMany(e => e.Items)
               .WithOne(i => i.Estimation)
               .HasForeignKey(i => i.EstimationId_FK)
               .HasPrincipalKey(e => e.Id)
               .OnDelete(DeleteBehavior.Restrict);
            est.Navigation(e => e.Items).AutoInclude();
        });

        modelBuilder.Entity<QuotationEntry>(quo =>
        {
            quo.ToTable("Quotations");
            quo.HasKey(q => q.Id);
            quo.HasMany(q => q.Items)
               .WithOne(i => i.Quotation)
               .HasForeignKey(i => i.QuotationId_FK)
               .HasPrincipalKey(q => q.Id)
               .OnDelete(DeleteBehavior.Restrict);
            quo.Navigation(q => q.Items).AutoInclude();
        });

        modelBuilder.Entity<ProposalEntry>(pro =>
        {
            pro.ToTable("Proposals");
            pro.HasKey(p => p.Id);
        });

        modelBuilder.Entity<FinanceLineItem>(item =>
        {
            item.ToTable("FinanceLineItems");
            item.HasKey(i => i.Id);
        });

        modelBuilder.Entity<UserRoleEntry>().HasKey(u => u.Id);
        modelBuilder.Entity<UserRoleEntry>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<AuditLog>().HasKey(a => a.Id);

        modelBuilder.Entity<LeadStatusOption>().HasKey(s => s.Id);
        modelBuilder.Entity<LeadStatusOption>().HasIndex(s => s.Value).IsUnique();

        modelBuilder.Entity<LeadSourceOption>().HasKey(s => s.Id);
        modelBuilder.Entity<LeadSourceOption>().HasIndex(s => s.Value).IsUnique();

        modelBuilder.Entity<RequirementTypeOption>().HasKey(r => r.Id);
        modelBuilder.Entity<RequirementTypeOption>().HasIndex(r => r.Value).IsUnique();

        modelBuilder.Entity<MasterData>(master =>
        {
            master.ToTable("MasterData");
            master.HasKey(m => m.Id);
            master.Property(m => m.MasterType).IsRequired().HasMaxLength(20);
            master.Property(m => m.Name).IsRequired().HasMaxLength(150);
            master.HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.NoAction);
            master.HasIndex(m => new { m.MasterType, m.ParentId, m.IsActive });
            master.HasData(
                new MasterData { Id = 1, MasterType = "Category", Name = "Window", IsActive = true },
                new MasterData { Id = 2, MasterType = "Category", Name = "Door", IsActive = true },
                new MasterData { Id = 3, MasterType = "Category", Name = "Partition", IsActive = true },
                new MasterData { Id = 4, MasterType = "Category", Name = "Facade", IsActive = true },
                new MasterData { Id = 101, MasterType = "Item", Name = "Sliding Window", ParentId = 1, IsActive = true },
                new MasterData { Id = 102, MasterType = "Item", Name = "Casement Window", ParentId = 1, IsActive = true },
                new MasterData { Id = 107, MasterType = "Item", Name = "Fixed Window", ParentId = 1, IsActive = true },
                new MasterData { Id = 103, MasterType = "Item", Name = "Sliding Door", ParentId = 2, IsActive = true },
                new MasterData { Id = 104, MasterType = "Item", Name = "Hinged Door", ParentId = 2, IsActive = true },
                new MasterData { Id = 108, MasterType = "Item", Name = "Folding Door", ParentId = 2, IsActive = true },
                new MasterData { Id = 105, MasterType = "Item", Name = "Glass Partition", ParentId = 3, IsActive = true },
                new MasterData { Id = 106, MasterType = "Item", Name = "Aluminium Partition", ParentId = 3, IsActive = true },
                new MasterData { Id = 109, MasterType = "Item", Name = "Curtain Wall", ParentId = 4, IsActive = true },
                new MasterData { Id = 110, MasterType = "Item", Name = "Structural Glazing", ParentId = 4, IsActive = true },
                new MasterData { Id = 201, MasterType = "Type", Name = "2-Track", ParentId = 101, IsActive = true },
                new MasterData { Id = 202, MasterType = "Type", Name = "3-Track", ParentId = 101, IsActive = true },
                new MasterData { Id = 203, MasterType = "Type", Name = "2-Track", ParentId = 102, IsActive = true },
                new MasterData { Id = 204, MasterType = "Type", Name = "3-Track", ParentId = 102, IsActive = true },
                new MasterData { Id = 205, MasterType = "Type", Name = "Fixed", ParentId = 102, IsActive = true },
                new MasterData { Id = 206, MasterType = "Type", Name = "Openable", ParentId = 102, IsActive = true },
                new MasterData { Id = 207, MasterType = "Type", Name = "2-Track", ParentId = 103, IsActive = true },
                new MasterData { Id = 208, MasterType = "Type", Name = "3-Track", ParentId = 103, IsActive = true },
                new MasterData { Id = 209, MasterType = "Type", Name = "Fixed", ParentId = 103, IsActive = true },
                new MasterData { Id = 210, MasterType = "Type", Name = "Openable", ParentId = 103, IsActive = true },
                new MasterData { Id = 211, MasterType = "Type", Name = "Fixed", ParentId = 104, IsActive = true },
                new MasterData { Id = 212, MasterType = "Type", Name = "Openable", ParentId = 104, IsActive = true },
                new MasterData { Id = 213, MasterType = "Type", Name = "2-Track", ParentId = 105, IsActive = true },
                new MasterData { Id = 214, MasterType = "Type", Name = "3-Track", ParentId = 105, IsActive = true },
                new MasterData { Id = 215, MasterType = "Type", Name = "Fixed", ParentId = 105, IsActive = true },
                new MasterData { Id = 216, MasterType = "Type", Name = "Openable", ParentId = 105, IsActive = true },
                new MasterData { Id = 217, MasterType = "Type", Name = "2-Track", ParentId = 106, IsActive = true },
                new MasterData { Id = 218, MasterType = "Type", Name = "3-Track", ParentId = 106, IsActive = true },
                new MasterData { Id = 219, MasterType = "Type", Name = "Fixed", ParentId = 106, IsActive = true },
                new MasterData { Id = 220, MasterType = "Type", Name = "Openable", ParentId = 106, IsActive = true },
                new MasterData { Id = 221, MasterType = "Type", Name = "Fixed", ParentId = 107, IsActive = true },
                new MasterData { Id = 222, MasterType = "Type", Name = "Openable", ParentId = 107, IsActive = true },
                new MasterData { Id = 223, MasterType = "Type", Name = "2-Track", ParentId = 107, IsActive = true },
                new MasterData { Id = 224, MasterType = "Type", Name = "3-Track", ParentId = 107, IsActive = true },
                new MasterData { Id = 225, MasterType = "Type", Name = "2-Track", ParentId = 108, IsActive = true },
                new MasterData { Id = 226, MasterType = "Type", Name = "3-Track", ParentId = 108, IsActive = true },
                new MasterData { Id = 227, MasterType = "Type", Name = "Fixed", ParentId = 108, IsActive = true },
                new MasterData { Id = 228, MasterType = "Type", Name = "Openable", ParentId = 108, IsActive = true },
                new MasterData { Id = 229, MasterType = "Type", Name = "2-Track", ParentId = 109, IsActive = true },
                new MasterData { Id = 230, MasterType = "Type", Name = "3-Track", ParentId = 109, IsActive = true },
                new MasterData { Id = 231, MasterType = "Type", Name = "Fixed", ParentId = 109, IsActive = true },
                new MasterData { Id = 232, MasterType = "Type", Name = "Openable", ParentId = 109, IsActive = true },
                new MasterData { Id = 233, MasterType = "Type", Name = "2-Track", ParentId = 110, IsActive = true },
                new MasterData { Id = 234, MasterType = "Type", Name = "3-Track", ParentId = 110, IsActive = true },
                new MasterData { Id = 235, MasterType = "Type", Name = "Fixed", ParentId = 110, IsActive = true },
                new MasterData { Id = 236, MasterType = "Type", Name = "Openable", ParentId = 110, IsActive = true },
                new MasterData { Id = 301, MasterType = "Topology", Name = "2 Panel", ParentId = 101, IsActive = true },
                new MasterData { Id = 302, MasterType = "Topology", Name = "3 Panel", ParentId = 101, IsActive = true },
                new MasterData { Id = 303, MasterType = "Topology", Name = "2 Panel", ParentId = 102, IsActive = true },
                new MasterData { Id = 304, MasterType = "Topology", Name = "3 Panel", ParentId = 102, IsActive = true },
                new MasterData { Id = 305, MasterType = "Topology", Name = "4 Panel", ParentId = 102, IsActive = true },
                new MasterData { Id = 306, MasterType = "Topology", Name = "Corner", ParentId = 102, IsActive = true },
                new MasterData { Id = 307, MasterType = "Topology", Name = "2 Panel", ParentId = 103, IsActive = true },
                new MasterData { Id = 308, MasterType = "Topology", Name = "3 Panel", ParentId = 103, IsActive = true },
                new MasterData { Id = 309, MasterType = "Topology", Name = "4 Panel", ParentId = 103, IsActive = true },
                new MasterData { Id = 310, MasterType = "Topology", Name = "Corner", ParentId = 103, IsActive = true },
                new MasterData { Id = 311, MasterType = "Topology", Name = "2 Panel", ParentId = 104, IsActive = true },
                new MasterData { Id = 312, MasterType = "Topology", Name = "4 Panel", ParentId = 104, IsActive = true },
                new MasterData { Id = 313, MasterType = "Topology", Name = "2 Panel", ParentId = 105, IsActive = true },
                new MasterData { Id = 314, MasterType = "Topology", Name = "3 Panel", ParentId = 105, IsActive = true },
                new MasterData { Id = 315, MasterType = "Topology", Name = "4 Panel", ParentId = 105, IsActive = true },
                new MasterData { Id = 316, MasterType = "Topology", Name = "Corner", ParentId = 105, IsActive = true },
                new MasterData { Id = 317, MasterType = "Topology", Name = "2 Panel", ParentId = 106, IsActive = true },
                new MasterData { Id = 318, MasterType = "Topology", Name = "3 Panel", ParentId = 106, IsActive = true },
                new MasterData { Id = 319, MasterType = "Topology", Name = "4 Panel", ParentId = 106, IsActive = true },
                new MasterData { Id = 320, MasterType = "Topology", Name = "Corner", ParentId = 106, IsActive = true },
                new MasterData { Id = 321, MasterType = "Topology", Name = "2 Panel", ParentId = 107, IsActive = true },
                new MasterData { Id = 322, MasterType = "Topology", Name = "3 Panel", ParentId = 107, IsActive = true },
                new MasterData { Id = 323, MasterType = "Topology", Name = "4 Panel", ParentId = 107, IsActive = true },
                new MasterData { Id = 324, MasterType = "Topology", Name = "Corner", ParentId = 107, IsActive = true },
                new MasterData { Id = 325, MasterType = "Topology", Name = "2 Panel", ParentId = 108, IsActive = true },
                new MasterData { Id = 326, MasterType = "Topology", Name = "3 Panel", ParentId = 108, IsActive = true },
                new MasterData { Id = 327, MasterType = "Topology", Name = "4 Panel", ParentId = 108, IsActive = true },
                new MasterData { Id = 328, MasterType = "Topology", Name = "Corner", ParentId = 108, IsActive = true },
                new MasterData { Id = 329, MasterType = "Topology", Name = "2 Panel", ParentId = 109, IsActive = true },
                new MasterData { Id = 330, MasterType = "Topology", Name = "3 Panel", ParentId = 109, IsActive = true },
                new MasterData { Id = 331, MasterType = "Topology", Name = "4 Panel", ParentId = 109, IsActive = true },
                new MasterData { Id = 332, MasterType = "Topology", Name = "Corner", ParentId = 109, IsActive = true },
                new MasterData { Id = 333, MasterType = "Topology", Name = "2 Panel", ParentId = 110, IsActive = true },
                new MasterData { Id = 334, MasterType = "Topology", Name = "3 Panel", ParentId = 110, IsActive = true },
                new MasterData { Id = 335, MasterType = "Topology", Name = "4 Panel", ParentId = 110, IsActive = true },
                new MasterData { Id = 336, MasterType = "Topology", Name = "Corner", ParentId = 110, IsActive = true },
                new MasterData { Id = 401, MasterType = "Glass", Name = "Clear", IsActive = true },
                new MasterData { Id = 402, MasterType = "Glass", Name = "Tinted", IsActive = true },
                new MasterData { Id = 403, MasterType = "Glass", Name = "Toughened", IsActive = true },
                new MasterData { Id = 404, MasterType = "Glass", Name = "Frosted", IsActive = true });
        });

        modelBuilder.Entity<FollowUpSettings>().HasKey(f => f.Id);
        modelBuilder.Entity<FollowUpSettings>().Property(f => f.Id).ValueGeneratedNever();

        modelBuilder.Entity<TaxMaster>(t =>
        {
            t.ToTable("TaxMasters");
            t.HasKey(x => x.Id);
            t.Property(x => x.TaxRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<RoleMaster>(r =>
        {
            r.ToTable("RoleMasters");
            r.HasKey(x => x.Id);
            r.HasIndex(x => x.RoleName).IsUnique();
        });

        modelBuilder.Entity<AccessToken>(token =>
        {
            token.ToTable("AccessTokens");
            token.HasKey(t => t.Id);
            token.HasIndex(t => t.TokenHash).IsUnique();
            token.Property(t => t.TokenHash).IsRequired().HasMaxLength(64);
            token.HasOne(t => t.User)
                 .WithMany()
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Work Management ──────────────────────────────────────────────────

        // Project -> Milestones  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.Milestones)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> Tasks  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Milestone -> Tasks  — NoAction prevents multiple-cascade-paths SQL Server error
        modelBuilder.Entity<WorkMilestone>()
            .HasMany(m => m.Tasks)
            .WithOne(t => t.Milestone)
            .HasForeignKey(t => t.MilestoneId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // Project -> Invoices  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.Invoices)
            .WithOne(i => i.Project)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Invoice -> Milestone (NoAction)
        modelBuilder.Entity<WorkInvoice>()
            .HasOne(i => i.Milestone)
            .WithMany()
            .HasForeignKey(i => i.MilestoneId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<WorkInvoice>()
            .HasIndex(i => i.MilestoneId)
            .IsUnique()
            .HasFilter("\"MilestoneId\" IS NOT NULL");

        // Invoice -> Payments (Cascade delete)
        modelBuilder.Entity<WorkInvoice>()
            .HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> Recurring Invoices (Cascade delete)
        modelBuilder.Entity<WorkRecurringInvoice>()
            .HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Recurring Invoice -> Milestone (NoAction)
        modelBuilder.Entity<WorkRecurringInvoice>()
            .HasOne(r => r.Milestone)
            .WithMany()
            .HasForeignKey(r => r.MilestoneId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // Project -> Files  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.Files)
            .WithOne(f => f.Project)
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> Timesheets  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.Timesheets)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> Timesheets (NoAction to prevent cascade cycle)
        modelBuilder.Entity<WorkTimesheet>()
            .HasOne(t => t.Task)
            .WithMany()
            .HasForeignKey(t => t.TaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // Project -> Expenses  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.Expenses)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> WorkNotes  (Cascade delete)
        modelBuilder.Entity<WorkProject>()
            .HasMany(p => p.WorkNotes)
            .WithOne(n => n.Project)
            .HasForeignKey(n => n.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Decimal precisions
        modelBuilder.Entity<WorkProject>().Property(p => p.Budget).HasPrecision(18, 2);
        modelBuilder.Entity<WorkMilestone>().Property(m => m.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkMilestone>().Property(m => m.CompletedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkTask>().Property(t => t.TaskAmount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkTask>().Property(t => t.CompletedAmount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkInvoice>().Property(i => i.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkInvoice>().Property(i => i.TaxRate).HasPrecision(18, 2);
        modelBuilder.Entity<WorkInvoice>().Property(i => i.Tax).HasPrecision(18, 2);
        modelBuilder.Entity<WorkInvoice>().Property(i => i.Discount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkInvoice>().Property(i => i.GrandTotal).HasPrecision(18, 2);
        modelBuilder.Entity<WorkInvoice>().Property(i => i.PaidAmount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkPayment>().Property(p => p.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkRecurringInvoice>().Property(r => r.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkRecurringInvoice>().Property(r => r.TaxRate).HasPrecision(18, 2);
        modelBuilder.Entity<WorkRecurringInvoice>().Property(r => r.Tax).HasPrecision(18, 2);
        modelBuilder.Entity<WorkRecurringInvoice>().Property(r => r.Discount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkRecurringInvoice>().Property(r => r.GrandTotal).HasPrecision(18, 2);
        modelBuilder.Entity<WorkTimesheet>().Property(t => t.HoursWorked).HasPrecision(18, 2);
        modelBuilder.Entity<WorkTimesheet>().Property(t => t.HourlyRate).HasPrecision(18, 2);
        modelBuilder.Entity<WorkTimesheet>().Property(t => t.TotalCost).HasPrecision(18, 2);
        modelBuilder.Entity<WorkExpense>().Property(e => e.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkExpense>().Property(e => e.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<WorkExpense>().Property(e => e.TotalAmount).HasPrecision(18, 2);

        // Auto-include child collections
        modelBuilder.Entity<WorkProject>().Navigation(p => p.Milestones).AutoInclude();
        modelBuilder.Entity<WorkProject>().Navigation(p => p.Tasks).AutoInclude();
        modelBuilder.Entity<WorkProject>().Navigation(p => p.Invoices).AutoInclude();
        modelBuilder.Entity<WorkProject>().Navigation(p => p.Files).AutoInclude();
        modelBuilder.Entity<WorkProject>().Navigation(p => p.Timesheets).AutoInclude();
        modelBuilder.Entity<WorkProject>().Navigation(p => p.Expenses).AutoInclude();
        modelBuilder.Entity<WorkProject>().Navigation(p => p.WorkNotes).AutoInclude();
        modelBuilder.Entity<WorkMilestone>().Navigation(m => m.Tasks).AutoInclude();
        modelBuilder.Entity<WorkInvoice>().Navigation(i => i.Payments).AutoInclude();

        // Enforce UTC DateTime for all PostgreSQL timestamptz columns
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
                }
            }
        }
    }
}

/// <summary>One selectable Lead Status value (Settings > Lead Status).</summary>
public class LeadStatusOption
{
    public int Id { get; set; }
    public string Value { get; set; } = "";
}

/// <summary>One selectable Lead Source value (Settings > Lead Sources).</summary>
public class LeadSourceOption
{
    public int Id { get; set; }
    public string Value { get; set; } = "";
}

/// <summary>One selectable Requirement Type value (Settings > Requirement Types).</summary>
public class RequirementTypeOption
{
    public int Id { get; set; }
    public string Value { get; set; } = "";
}
