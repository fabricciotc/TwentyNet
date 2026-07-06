using Microsoft.EntityFrameworkCore;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;
using TaskItemEntity = TwentyNet.Domain.Entities.TaskItem;

namespace TwentyNet.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserWorkspaceMembership> UserWorkspaceMemberships => Set<UserWorkspaceMembership>();
    public DbSet<WorkspaceInvite> WorkspaceInvites => Set<WorkspaceInvite>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<FileEntity> Files => Set<FileEntity>();
    public DbSet<Webhook> Webhooks => Set<Webhook>();
    public DbSet<ConnectedAccount> ConnectedAccounts => Set<ConnectedAccount>();
    public DbSet<MessageChannel> MessageChannels => Set<MessageChannel>();
    public DbSet<CalendarChannel> CalendarChannels => Set<CalendarChannel>();
    public DbSet<View> Views => Set<View>();
    public DbSet<ViewFilter> ViewFilters => Set<ViewFilter>();
    public DbSet<ViewSort> ViewSorts => Set<ViewSort>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<TaskItemEntity> TaskItems => Set<TaskItemEntity>();
    public DbSet<TimelineActivity> TimelineActivities => Set<TimelineActivity>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<RecordRelation> RecordRelations => Set<RecordRelation>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<SsoProvider> SsoProviders => Set<SsoProvider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
