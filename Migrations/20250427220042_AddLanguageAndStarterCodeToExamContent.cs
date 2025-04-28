using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteHotrohoctap.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageAndStarterCodeToExamContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "LessonContents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "ExamContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StarterCode",
                table: "ExamContents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "ExamContents");

            migrationBuilder.DropColumn(
                name: "StarterCode",
                table: "ExamContents");

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "LessonContents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
