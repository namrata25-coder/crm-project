using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CrmLeadManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "RequirementEntry",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GlassId",
                table: "RequirementEntry",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "RequirementEntry",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TopologyId",
                table: "RequirementEntry",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "RequirementEntry",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MasterData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MasterType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MasterData_MasterData_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MasterData",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "MasterData",
                columns: new[] { "Id", "IsActive", "MasterType", "Name", "ParentId" },
                values: new object[,]
                {
                    { 1, true, "Category", "Window", null },
                    { 2, true, "Category", "Door", null },
                    { 3, true, "Category", "Partition", null },
                    { 401, true, "Glass", "Clear", null },
                    { 402, true, "Glass", "Tinted", null },
                    { 403, true, "Glass", "Toughened", null },
                    { 404, true, "Glass", "Frosted", null },
                    { 101, true, "Item", "Sliding Window", 1 },
                    { 102, true, "Item", "Casement Window", 1 },
                    { 103, true, "Item", "Sliding Door", 2 },
                    { 104, true, "Item", "Hinged Door", 2 },
                    { 105, true, "Item", "Glass Partition", 3 },
                    { 106, true, "Item", "Aluminium Partition", 3 },
                    { 201, true, "Type", "2 Track", 101 },
                    { 202, true, "Type", "3 Track", 101 },
                    { 203, true, "Type", "Single Sash", 102 },
                    { 204, true, "Type", "Double Sash", 102 },
                    { 205, true, "Type", "2 Track", 103 },
                    { 206, true, "Type", "3 Track", 103 },
                    { 207, true, "Type", "Single Leaf", 104 },
                    { 208, true, "Type", "Double Leaf", 104 },
                    { 209, true, "Type", "Framed", 105 },
                    { 210, true, "Type", "Frameless", 105 },
                    { 211, true, "Type", "Single Glazed", 106 },
                    { 212, true, "Type", "Double Glazed", 106 },
                    { 301, true, "Topology", "2 Panel", 101 },
                    { 302, true, "Topology", "3 Panel", 101 },
                    { 303, true, "Topology", "Left Open", 102 },
                    { 304, true, "Topology", "Right Open", 102 },
                    { 305, true, "Topology", "2 Panel", 103 },
                    { 306, true, "Topology", "3 Panel", 103 },
                    { 307, true, "Topology", "Inward", 104 },
                    { 308, true, "Topology", "Outward", 104 },
                    { 309, true, "Topology", "Straight", 105 },
                    { 310, true, "Topology", "L-Shape", 105 },
                    { 311, true, "Topology", "Straight", 106 },
                    { 312, true, "Topology", "L-Shape", 106 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequirementEntry_CategoryId",
                table: "RequirementEntry",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementEntry_GlassId",
                table: "RequirementEntry",
                column: "GlassId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementEntry_ItemId",
                table: "RequirementEntry",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementEntry_TopologyId",
                table: "RequirementEntry",
                column: "TopologyId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementEntry_TypeId",
                table: "RequirementEntry",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_MasterType_ParentId_IsActive",
                table: "MasterData",
                columns: new[] { "MasterType", "ParentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_ParentId",
                table: "MasterData",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementEntry_MasterData_CategoryId",
                table: "RequirementEntry",
                column: "CategoryId",
                principalTable: "MasterData",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementEntry_MasterData_GlassId",
                table: "RequirementEntry",
                column: "GlassId",
                principalTable: "MasterData",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementEntry_MasterData_ItemId",
                table: "RequirementEntry",
                column: "ItemId",
                principalTable: "MasterData",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementEntry_MasterData_TopologyId",
                table: "RequirementEntry",
                column: "TopologyId",
                principalTable: "MasterData",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequirementEntry_MasterData_TypeId",
                table: "RequirementEntry",
                column: "TypeId",
                principalTable: "MasterData",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequirementEntry_MasterData_CategoryId",
                table: "RequirementEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RequirementEntry_MasterData_GlassId",
                table: "RequirementEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RequirementEntry_MasterData_ItemId",
                table: "RequirementEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RequirementEntry_MasterData_TopologyId",
                table: "RequirementEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RequirementEntry_MasterData_TypeId",
                table: "RequirementEntry");

            migrationBuilder.DropTable(
                name: "MasterData");

            migrationBuilder.DropIndex(
                name: "IX_RequirementEntry_CategoryId",
                table: "RequirementEntry");

            migrationBuilder.DropIndex(
                name: "IX_RequirementEntry_GlassId",
                table: "RequirementEntry");

            migrationBuilder.DropIndex(
                name: "IX_RequirementEntry_ItemId",
                table: "RequirementEntry");

            migrationBuilder.DropIndex(
                name: "IX_RequirementEntry_TopologyId",
                table: "RequirementEntry");

            migrationBuilder.DropIndex(
                name: "IX_RequirementEntry_TypeId",
                table: "RequirementEntry");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "RequirementEntry");

            migrationBuilder.DropColumn(
                name: "GlassId",
                table: "RequirementEntry");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "RequirementEntry");

            migrationBuilder.DropColumn(
                name: "TopologyId",
                table: "RequirementEntry");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "RequirementEntry");

        }
    }
}
