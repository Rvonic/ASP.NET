using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrviLabos.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToAgentsAndTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: DeletedAt columns already exist in all target tables.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op rollback: keep schema unchanged.
        }
    }
}
