using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PueblaApi.Database.Migrations
{
    /// <inheritdoc />
    public partial class StudiosNoImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Studios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Studios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
