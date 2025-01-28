using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PueblaApi.Database.Migrations
{
    /// <inheritdoc />
    public partial class NoNationalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_AspNetUsers_NationalId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UK_AspNetUsers_NationalId",
                table: "AspNetUsers",
                column: "NationalId",
                unique: true);
        }
    }
}
