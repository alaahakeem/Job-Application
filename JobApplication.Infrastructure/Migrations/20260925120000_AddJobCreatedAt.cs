using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Jobs");
        }
    }
}
