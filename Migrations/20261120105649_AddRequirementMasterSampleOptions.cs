using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CrmLeadManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementMasterSampleOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 201,
                column: "Name",
                value: "2-Track");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 202,
                column: "Name",
                value: "3-Track");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 203,
                column: "Name",
                value: "2-Track");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 204,
                column: "Name",
                value: "3-Track");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Fixed", 102 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 206,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Openable", 102 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 207,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "2-Track", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 208,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "3-Track", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 209,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Fixed", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 210,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Openable", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 211,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Fixed", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 212,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Openable", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 303,
                column: "Name",
                value: "2 Panel");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 304,
                column: "Name",
                value: "3 Panel");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "4 Panel", 102 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Corner", 102 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "2 Panel", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "3 Panel", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 309,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "4 Panel", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 310,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Corner", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 311,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "2 Panel", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 312,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "4 Panel", 104 });

            migrationBuilder.InsertData(
                table: "MasterData",
                columns: new[] { "Id", "IsActive", "MasterType", "Name", "ParentId" },
                values: new object[,]
                {
                    { 4, true, "Category", "Facade", null },
                    { 107, true, "Item", "Fixed Window", 1 },
                    { 108, true, "Item", "Folding Door", 2 },
                    { 213, true, "Type", "2-Track", 105 },
                    { 214, true, "Type", "3-Track", 105 },
                    { 215, true, "Type", "Fixed", 105 },
                    { 216, true, "Type", "Openable", 105 },
                    { 217, true, "Type", "2-Track", 106 },
                    { 218, true, "Type", "3-Track", 106 },
                    { 219, true, "Type", "Fixed", 106 },
                    { 220, true, "Type", "Openable", 106 },
                    { 313, true, "Topology", "2 Panel", 105 },
                    { 314, true, "Topology", "3 Panel", 105 },
                    { 315, true, "Topology", "4 Panel", 105 },
                    { 316, true, "Topology", "Corner", 105 },
                    { 317, true, "Topology", "2 Panel", 106 },
                    { 318, true, "Topology", "3 Panel", 106 },
                    { 319, true, "Topology", "4 Panel", 106 },
                    { 320, true, "Topology", "Corner", 106 },
                    { 109, true, "Item", "Curtain Wall", 4 },
                    { 110, true, "Item", "Structural Glazing", 4 },
                    { 221, true, "Type", "Fixed", 107 },
                    { 222, true, "Type", "Openable", 107 },
                    { 223, true, "Type", "2-Track", 107 },
                    { 224, true, "Type", "3-Track", 107 },
                    { 225, true, "Type", "2-Track", 108 },
                    { 226, true, "Type", "3-Track", 108 },
                    { 227, true, "Type", "Fixed", 108 },
                    { 228, true, "Type", "Openable", 108 },
                    { 321, true, "Topology", "2 Panel", 107 },
                    { 322, true, "Topology", "3 Panel", 107 },
                    { 323, true, "Topology", "4 Panel", 107 },
                    { 324, true, "Topology", "Corner", 107 },
                    { 325, true, "Topology", "2 Panel", 108 },
                    { 326, true, "Topology", "3 Panel", 108 },
                    { 327, true, "Topology", "4 Panel", 108 },
                    { 328, true, "Topology", "Corner", 108 },
                    { 229, true, "Type", "2-Track", 109 },
                    { 230, true, "Type", "3-Track", 109 },
                    { 231, true, "Type", "Fixed", 109 },
                    { 232, true, "Type", "Openable", 109 },
                    { 233, true, "Type", "2-Track", 110 },
                    { 234, true, "Type", "3-Track", 110 },
                    { 235, true, "Type", "Fixed", 110 },
                    { 236, true, "Type", "Openable", 110 },
                    { 329, true, "Topology", "2 Panel", 109 },
                    { 330, true, "Topology", "3 Panel", 109 },
                    { 331, true, "Topology", "4 Panel", 109 },
                    { 332, true, "Topology", "Corner", 109 },
                    { 333, true, "Topology", "2 Panel", 110 },
                    { 334, true, "Topology", "3 Panel", 110 },
                    { 335, true, "Topology", "4 Panel", 110 },
                    { 336, true, "Topology", "Corner", 110 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 213);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 214);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 215);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 216);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 217);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 218);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 219);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 220);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 221);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 222);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 223);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 224);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 225);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 226);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 227);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 228);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 229);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 230);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 231);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 232);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 233);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 234);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 235);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 236);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 313);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 314);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 315);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 316);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 317);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 318);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 319);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 320);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 321);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 322);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 323);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 324);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 325);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 326);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 327);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 328);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 329);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 330);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 331);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 332);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 333);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 334);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 335);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 336);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 201,
                column: "Name",
                value: "2 Track");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 202,
                column: "Name",
                value: "3 Track");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 203,
                column: "Name",
                value: "Single Sash");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 204,
                column: "Name",
                value: "Double Sash");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 205,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "2 Track", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 206,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "3 Track", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 207,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Single Leaf", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 208,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Double Leaf", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 209,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Framed", 105 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 210,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Frameless", 105 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 211,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Single Glazed", 106 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 212,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Double Glazed", 106 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 303,
                column: "Name",
                value: "Left Open");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 304,
                column: "Name",
                value: "Right Open");

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 305,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "2 Panel", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 306,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "3 Panel", 103 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 307,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Inward", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 308,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Outward", 104 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 309,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Straight", 105 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 310,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "L-Shape", 105 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 311,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "Straight", 106 });

            migrationBuilder.UpdateData(
                table: "MasterData",
                keyColumn: "Id",
                keyValue: 312,
                columns: new[] { "Name", "ParentId" },
                values: new object[] { "L-Shape", 106 });
        }
    }
}
