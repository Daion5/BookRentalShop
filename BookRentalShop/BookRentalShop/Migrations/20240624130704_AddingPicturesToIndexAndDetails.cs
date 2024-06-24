using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookRentalShop.Migrations
{
    /// <inheritdoc />
    public partial class AddingPicturesToIndexAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Books",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Books");
        }
    }
}
