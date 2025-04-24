using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteHotrohoctap.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeInputOuputToExamResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Input",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Output",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ExamResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Input",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Output",
                table: "ExamResults");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExamResults");
        }
    }
}
