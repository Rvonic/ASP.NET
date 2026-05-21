using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrviLabos.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToCustomerAndBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Bookings");
        }
    }
}
