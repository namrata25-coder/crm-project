using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmLeadManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkProjectCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkProjects",
                type: "text",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkProjects");
        }
    }
}
