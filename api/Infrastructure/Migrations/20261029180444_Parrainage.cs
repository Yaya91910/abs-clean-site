using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AbsClean.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Parrainage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeParrainPersonnel",
                table: "Demandes",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodeParrainUtilise",
                table: "Demandes",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterventionRealiseeLe",
                table: "Demandes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JetonInterventionHash",
                table: "Demandes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParrainId",
                table: "Demandes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Parrainages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParrainId = table.Column<int>(type: "integer", nullable: false),
                    FilleulId = table.Column<int>(type: "integer", nullable: false),
                    CreeLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ValideLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemiseUtiliseeLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parrainages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parrainages_Demandes_FilleulId",
                        column: x => x.FilleulId,
                        principalTable: "Demandes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parrainages_Demandes_ParrainId",
                        column: x => x.ParrainId,
                        principalTable: "Demandes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Demandes enregistrées avant le parrainage : un code unique chacune, avant l'index unique.
            migrationBuilder.Sql("""UPDATE "Demandes" SET "CodeParrainPersonnel" = 'ABS-ANCIEN' || "Id" WHERE "CodeParrainPersonnel" = '';""");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_CodeParrainPersonnel",
                table: "Demandes",
                column: "CodeParrainPersonnel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_Email",
                table: "Demandes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_ParrainId",
                table: "Demandes",
                column: "ParrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_Telephone",
                table: "Demandes",
                column: "Telephone");

            migrationBuilder.CreateIndex(
                name: "IX_Parrainages_FilleulId",
                table: "Parrainages",
                column: "FilleulId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parrainages_ParrainId",
                table: "Parrainages",
                column: "ParrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Demandes_Demandes_ParrainId",
                table: "Demandes",
                column: "ParrainId",
                principalTable: "Demandes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demandes_Demandes_ParrainId",
                table: "Demandes");

            migrationBuilder.DropTable(
                name: "Parrainages");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_CodeParrainPersonnel",
                table: "Demandes");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_Email",
                table: "Demandes");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_ParrainId",
                table: "Demandes");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_Telephone",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "CodeParrainPersonnel",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "CodeParrainUtilise",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "InterventionRealiseeLe",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "JetonInterventionHash",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "ParrainId",
                table: "Demandes");
        }
    }
}
