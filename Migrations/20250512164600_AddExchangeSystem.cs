using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSwap.Migrations
{
    public partial class AddExchangeSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "UserProfiles",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ExchangesCompleted",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRatings",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Exchanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestorId = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<int>(type: "int", nullable: false),
                    RequestedSkillId = table.Column<int>(type: "int", nullable: false),
                    OfferedSkillId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestorRating = table.Column<int>(type: "int", nullable: true),
                    RequestorFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderRating = table.Column<int>(type: "int", nullable: true),
                    ProviderFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exchanges_Skills_OfferedSkillId",
                        column: x => x.OfferedSkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exchanges_Skills_RequestedSkillId",
                        column: x => x.RequestedSkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exchanges_UserProfiles_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exchanges_UserProfiles_RequestorId",
                        column: x => x.RequestorId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_OfferedSkillId",
                table: "Exchanges",
                column: "OfferedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_ProviderId",
                table: "Exchanges",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_RequestedSkillId",
                table: "Exchanges",
                column: "RequestedSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_RequestorId",
                table: "Exchanges",
                column: "RequestorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exchanges");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ExchangesCompleted",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "TotalRatings",
                table: "UserProfiles");
        }
    }
}
