using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emergency.Migrations
{
    public partial class mobile_message_shown_prop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Shown",
                table: "MobileMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Shown",
                table: "MobileMessages");
        }
    }
}
