﻿using Microsoft.EntityFrameworkCore;

namespace GiantTeam.Organizations.Directory.Data;

public abstract class DirectoryDbContext<T> : DbContext where T : DirectoryDbContext<T>
{
    public const string DefaultSchema = "directory";

    public DirectoryDbContext(DbContextOptions<T> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);

        var userPasswords = modelBuilder.Entity<UserPassword>();
        userPasswords.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationRole> OrganizationRoles => Set<OrganizationRole>();
    public DbSet<User> Users => Set<User>();
    internal DbSet<UserPassword> UserPasswords => Set<UserPassword>();
}
