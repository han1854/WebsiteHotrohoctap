using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteHotrohoctap.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeInputOuputToExamResults2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CorrectAnswer",
                table: "ExamContents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ExpectedOutput",
                table: "ExamContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SampleInput",
                table: "ExamContents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedOutput",
                table: "ExamContents");

            migrationBuilder.DropColumn(
                name: "SampleInput",
                table: "ExamContents");

            migrationBuilder.AlterColumn<string>(
                name: "CorrectAnswer",
                table: "ExamContents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
