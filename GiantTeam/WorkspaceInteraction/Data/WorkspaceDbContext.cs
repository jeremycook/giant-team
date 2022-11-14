﻿using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using GiantTeam.Services;
using GiantTeam.WorkspaceInteraction.Data.InformationSchema;
using GiantTeam.WorkspaceInteraction.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GiantTeam.WorkspaceInteraction.Data
{
    public class WorkspaceDbContext : DbContext
    {
        private readonly DatabaseConnectionService databaseConnectionService;
        private readonly SessionService sessionService;
        private readonly string databaseName;

        public WorkspaceDbContext(
            DatabaseConnectionService databaseConnectionService,
            SessionService sessionService,
            string databaseName)
        {
            this.databaseConnectionService = databaseConnectionService;
            this.sessionService = sessionService;
            this.databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            SessionUser sessionUser = sessionService.User;

            string userRole = DatabaseHelper.QueryUser(sessionUser.DatabaseUsername);

            NpgsqlConnection connection = databaseConnectionService
                .CreateQueryConnection(databaseName);

            optionsBuilder
                .AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(userRole)};"))
                .UseNpgsql(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Tables> InformationSchemaTables => Set<Tables>();
    }
}