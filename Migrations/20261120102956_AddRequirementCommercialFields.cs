using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmLeadManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementCommercialFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Amount",
                table: "RequirementEntry",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaCharge",
                table: "RequirementEntry",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "RequirementEntry");

            migrationBuilder.DropColumn(
                name: "AreaCharge",
                table: "RequirementEntry");
        }
    }
}
