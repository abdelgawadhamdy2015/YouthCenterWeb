using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddnewCups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CupMatch_Cups_CupId1",
                table: "CupMatch");

            migrationBuilder.DropForeignKey(
                name: "FK_CupTeams_Cups_CupId",
                table: "CupTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CupTeams_Teams_TeamId",
                table: "CupTeams");

            migrationBuilder.DropTable(
                name: "CupJoinRequests");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_CupTeams_CupId",
                table: "CupTeams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CupMatch",
                table: "CupMatch");

            migrationBuilder.DropIndex(
                name: "IX_CupMatch_CupId1",
                table: "CupMatch");

            migrationBuilder.DropColumn(
                name: "ArabicName",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "AwayTeamId",
                table: "CupMatch");

            migrationBuilder.DropColumn(
                name: "HomeTeamId",
                table: "CupMatch");

            migrationBuilder.DropColumn(
                name: "WinnerTeamId",
                table: "CupMatch");

            migrationBuilder.RenameTable(
                name: "CupMatch",
                newName: "CupMatches");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "CupTeams",
                newName: "CaptainUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CupTeams_TeamId",
                table: "CupTeams",
                newName: "IX_CupTeams_CaptainUserId");

            migrationBuilder.RenameColumn(
                name: "LatinName",
                table: "Cups",
                newName: "Format");

            migrationBuilder.RenameColumn(
                name: "CurrentTeams",
                table: "Cups",
                newName: "YouthCenterId");

            migrationBuilder.RenameColumn(
                name: "RoundNumber",
                table: "CupMatches",
                newName: "Round");

            migrationBuilder.RenameColumn(
                name: "HomeScore",
                table: "CupMatches",
                newName: "WinnerId");

            migrationBuilder.RenameColumn(
                name: "CupId1",
                table: "CupMatches",
                newName: "MatchNumber");

            migrationBuilder.RenameColumn(
                name: "AwayScore",
                table: "CupMatches",
                newName: "TeamBId");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "YouthCenters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "YouthCenters",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "YouthCenters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "YouthCenters",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "YouthCenters",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "YouthCenterActivities",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxCapacity",
                table: "YouthCenterActivities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAvailable",
                table: "YouthCenterActivities",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialRequest",
                table: "Reservations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredAt",
                table: "CupTeams",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "CupTeams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CupTeams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "CupTeams",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Cups",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Cups",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cups",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByAdminId",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentTeamCount",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EntryFee",
                table: "Cups",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Cups",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "Activities",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CupMatches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CupId",
                table: "CupMatches",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CupMatches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                table: "CupMatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreA",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreB",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamAId",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CupMatches",
                table: "CupMatches",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams",
                columns: new[] { "CupId", "CaptainUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cups_ActivityId",
                table: "Cups",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_CreatedByAdminId",
                table: "Cups",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Cups_YouthCenterId",
                table: "Cups",
                column: "YouthCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_CupId_Round_MatchNumber",
                table: "CupMatches",
                columns: new[] { "CupId", "Round", "MatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_TeamAId",
                table: "CupMatches",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_TeamBId",
                table: "CupMatches",
                column: "TeamBId");

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_WinnerId",
                table: "CupMatches",
                column: "WinnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CupMatches_CupTeams_TeamAId",
                table: "CupMatches",
                column: "TeamAId",
                principalTable: "CupTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupMatches_CupTeams_TeamBId",
                table: "CupMatches",
                column: "TeamBId",
                principalTable: "CupTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupMatches_CupTeams_WinnerId",
                table: "CupMatches",
                column: "WinnerId",
                principalTable: "CupTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupMatches_Cups_CupId",
                table: "CupMatches",
                column: "CupId",
                principalTable: "Cups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_Activities_ActivityId",
                table: "Cups",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_Users_CreatedByAdminId",
                table: "Cups",
                column: "CreatedByAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_YouthCenters_YouthCenterId",
                table: "Cups",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupTeams_Cups_CupId",
                table: "CupTeams",
                column: "CupId",
                principalTable: "Cups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CupTeams_Users_CaptainUserId",
                table: "CupTeams",
                column: "CaptainUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CupMatches_CupTeams_TeamAId",
                table: "CupMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_CupMatches_CupTeams_TeamBId",
                table: "CupMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_CupMatches_CupTeams_WinnerId",
                table: "CupMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_CupMatches_Cups_CupId",
                table: "CupMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Cups_Activities_ActivityId",
                table: "Cups");

            migrationBuilder.DropForeignKey(
                name: "FK_Cups_Users_CreatedByAdminId",
                table: "Cups");

            migrationBuilder.DropForeignKey(
                name: "FK_Cups_YouthCenters_YouthCenterId",
                table: "Cups");

            migrationBuilder.DropForeignKey(
                name: "FK_CupTeams_Cups_CupId",
                table: "CupTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_CupTeams_Users_CaptainUserId",
                table: "CupTeams");

            migrationBuilder.DropIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams");

            migrationBuilder.DropIndex(
                name: "IX_Cups_ActivityId",
                table: "Cups");

            migrationBuilder.DropIndex(
                name: "IX_Cups_CreatedByAdminId",
                table: "Cups");

            migrationBuilder.DropIndex(
                name: "IX_Cups_YouthCenterId",
                table: "Cups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CupMatches",
                table: "CupMatches");

            migrationBuilder.DropIndex(
                name: "IX_CupMatches_CupId_Round_MatchNumber",
                table: "CupMatches");

            migrationBuilder.DropIndex(
                name: "IX_CupMatches_TeamAId",
                table: "CupMatches");

            migrationBuilder.DropIndex(
                name: "IX_CupMatches_TeamBId",
                table: "CupMatches");

            migrationBuilder.DropIndex(
                name: "IX_CupMatches_WinnerId",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "RegisteredAt",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "CreatedByAdminId",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "CurrentTeamCount",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "EntryFee",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "ScoreA",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "ScoreB",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "TeamAId",
                table: "CupMatches");

            migrationBuilder.RenameTable(
                name: "CupMatches",
                newName: "CupMatch");

            migrationBuilder.RenameColumn(
                name: "CaptainUserId",
                table: "CupTeams",
                newName: "TeamId");

            migrationBuilder.RenameIndex(
                name: "IX_CupTeams_CaptainUserId",
                table: "CupTeams",
                newName: "IX_CupTeams_TeamId");

            migrationBuilder.RenameColumn(
                name: "YouthCenterId",
                table: "Cups",
                newName: "CurrentTeams");

            migrationBuilder.RenameColumn(
                name: "Format",
                table: "Cups",
                newName: "LatinName");

            migrationBuilder.RenameColumn(
                name: "WinnerId",
                table: "CupMatch",
                newName: "HomeScore");

            migrationBuilder.RenameColumn(
                name: "TeamBId",
                table: "CupMatch",
                newName: "AwayScore");

            migrationBuilder.RenameColumn(
                name: "Round",
                table: "CupMatch",
                newName: "RoundNumber");

            migrationBuilder.RenameColumn(
                name: "MatchNumber",
                table: "CupMatch",
                newName: "CupId1");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "YouthCenters",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "YouthCenters",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "YouthCenters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "YouthCenters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "YouthCenters",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "YouthCenterActivities",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "MaxCapacity",
                table: "YouthCenterActivities",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAvailable",
                table: "YouthCenterActivities",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialRequest",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Cups",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Cups",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArabicName",
                table: "Cups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CupMatch",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<long>(
                name: "CupId",
                table: "CupMatch",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "AwayTeamId",
                table: "CupMatch",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "HomeTeamId",
                table: "CupMatch",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "WinnerTeamId",
                table: "CupMatch",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CupMatch",
                table: "CupMatch",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YouthCenterId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_YouthCenters_YouthCenterId",
                        column: x => x.YouthCenterId,
                        principalTable: "YouthCenters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CupJoinRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CupId = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CupJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CupJoinRequests_Cups_CupId",
                        column: x => x.CupId,
                        principalTable: "Cups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CupJoinRequests_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId",
                table: "CupTeams",
                column: "CupId");

            migrationBuilder.CreateIndex(
                name: "IX_CupMatch_CupId1",
                table: "CupMatch",
                column: "CupId1");

            migrationBuilder.CreateIndex(
                name: "IX_CupJoinRequests_CupId",
                table: "CupJoinRequests",
                column: "CupId");

            migrationBuilder.CreateIndex(
                name: "IX_CupJoinRequests_TeamId",
                table: "CupJoinRequests",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_YouthCenterId",
                table: "Teams",
                column: "YouthCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CupMatch_Cups_CupId1",
                table: "CupMatch",
                column: "CupId1",
                principalTable: "Cups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CupTeams_Cups_CupId",
                table: "CupTeams",
                column: "CupId",
                principalTable: "Cups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupTeams_Teams_TeamId",
                table: "CupTeams",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
