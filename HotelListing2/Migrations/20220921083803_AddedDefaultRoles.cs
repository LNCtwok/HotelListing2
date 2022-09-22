using Microsoft.EntityFrameworkCore.Migrations;

namespace HotelListing2.Migrations
{
    public partial class AddedDefaultRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "ef48d867-91e3-4a29-b265-b131d7d07026", "dac14073-83cb-4b8c-baff-97a2e2124c0f", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "6a552803-2f28-47f9-b59b-a810ff7fc437", "12f5bf2e-d3c8-47f4-8cc0-49e1dd4dbb96", "Administrator", "ADMINISTRATOR" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6a552803-2f28-47f9-b59b-a810ff7fc437");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ef48d867-91e3-4a29-b265-b131d7d07026");
        }
    }
}
