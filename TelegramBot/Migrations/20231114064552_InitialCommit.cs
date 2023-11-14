using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "CHBTDEV");

            migrationBuilder.CreateTable(
                name: "PASS_SCHEDULE",
                schema: "CHBTDEV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Day = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Start = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    End = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASS_SCHEDULE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PASS_USER",
                schema: "CHBTDEV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Fullname = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    IIN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    Telephone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    TelegramId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AmeiUsrid = table.Column<int>(type: "NUMBER(8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASS_USER", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmeiUsrid",
                        column: x => x.AmeiUsrid,
                        principalSchema: "CHBTDEV",
                        principalTable: "AMEI",
                        principalColumn: "USRID");
                });

            migrationBuilder.CreateTable(
                name: "PASS_REQUEST",
                schema: "CHBTDEV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FromId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToUsrid = table.Column<int>(type: "NUMBER(8)", nullable: false),
                    PassScheduleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Created = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Reason = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASS_REQUEST", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AMEI_ToUsrid",
                        column: x => x.ToUsrid,
                        principalSchema: "CHBTDEV",
                        principalTable: "AMEI",
                        principalColumn: "USRID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PassScheduleId",
                        column: x => x.PassScheduleId,
                        principalSchema: "CHBTDEV",
                        principalTable: "PASS_SCHEDULE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FromId",
                        column: x => x.FromId,
                        principalSchema: "CHBTDEV",
                        principalTable: "PASS_USER",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PASS_STATUS",
                schema: "CHBTDEV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Created = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PassRequestId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASS_STATUS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassRequestId",
                        column: x => x.PassRequestId,
                        principalSchema: "CHBTDEV",
                        principalTable: "PASS_REQUEST",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PASS_REQUEST_FromId",
                schema: "CHBTDEV",
                table: "PASS_REQUEST",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_PASS_REQUEST_PassScheduleId",
                schema: "CHBTDEV",
                table: "PASS_REQUEST",
                column: "PassScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_PASS_REQUEST_ToUsrid",
                schema: "CHBTDEV",
                table: "PASS_REQUEST",
                column: "ToUsrid");

            migrationBuilder.CreateIndex(
                name: "IX_PASS_STATUS_PassRequestId",
                schema: "CHBTDEV",
                table: "PASS_STATUS",
                column: "PassRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PASS_USER_AmeiUsrid",
                schema: "CHBTDEV",
                table: "PASS_USER",
                column: "AmeiUsrid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PASS_STATUS",
                schema: "CHBTDEV");

            migrationBuilder.DropTable(
                name: "PASS_REQUEST",
                schema: "CHBTDEV");

            migrationBuilder.DropTable(
                name: "PASS_SCHEDULE",
                schema: "CHBTDEV");

            migrationBuilder.DropTable(
                name: "PASS_USER",
                schema: "CHBTDEV");
        }
    }
}
