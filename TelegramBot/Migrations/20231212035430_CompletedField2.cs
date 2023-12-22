using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class CompletedField2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                schema: "CHBTDEV",
                table: "PASS_REQUEST",
                type: "NUMBER(1)",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                schema: "CHBTDEV",
                table: "PASS_REQUEST",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "NUMBER(1)",
                oldNullable: true);
        }
    }
}
