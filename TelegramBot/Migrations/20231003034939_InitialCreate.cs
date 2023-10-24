﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "CHBTDEV");

            migrationBuilder.CreateTable(
                name: "PassSchedule",
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
                    table.PrimaryKey("PK_PassSchedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PassUser",
                schema: "CHBTDEV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Fullname = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    IIN = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Telephone = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TelegramId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AmeiUsrid = table.Column<int>(type: "NUMBER(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmeiUsrid",
                        column: x => x.AmeiUsrid,
                        principalSchema: "CHBTDEV",
                        principalTable: "AMEI",
                        principalColumn: "USRID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PassRequest",
                schema: "CHBTDEV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FromId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ToUsrid = table.Column<int>(type: "NUMBER(8)", nullable: false),
                    PassScheduleId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Created = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToUsrid",
                        column: x => x.ToUsrid,
                        principalSchema: "CHBTDEV",
                        principalTable: "AMEI",
                        principalColumn: "USRID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PassScheduleId",
                        column: x => x.PassScheduleId,
                        principalSchema: "CHBTDEV",
                        principalTable: "PassSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FromId",
                        column: x => x.FromId,
                        principalSchema: "CHBTDEV",
                        principalTable: "PassUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PassStatus",
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
                    table.PrimaryKey("PK_PassStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassRequest_PassRequestId",
                        column: x => x.PassRequestId,
                        principalSchema: "CHBTDEV",
                        principalTable: "PassRequest",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FromId",
                schema: "CHBTDEV",
                table: "PassRequest",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_PassScheduleId",
                schema: "CHBTDEV",
                table: "PassRequest",
                column: "PassScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ToUsrid",
                schema: "CHBTDEV",
                table: "PassRequest",
                column: "ToUsrid");

            migrationBuilder.CreateIndex(
                name: "IX_PassRequestId",
                schema: "CHBTDEV",
                table: "PassStatus",
                column: "PassRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AmeiUsrid",
                schema: "CHBTDEV",
                table: "PassUser",
                column: "AmeiUsrid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PassStatus",
                schema: "CHBTDEV");

            migrationBuilder.DropTable(
                name: "PassRequest",
                schema: "CHBTDEV");

            migrationBuilder.DropTable(
                name: "PassSchedule",
                schema: "CHBTDEV");

            migrationBuilder.DropTable(
                name: "PassUser",
                schema: "CHBTDEV");
        }
    }
}