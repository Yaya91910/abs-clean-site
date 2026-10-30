using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AbsClean.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Clients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Demandes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    CreeLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nom = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            // Demandes existantes : un client par e-mail distinct (coordonnées de sa première demande),
            // numérotés dans l'ordre de leur première demande, puis chaque demande est rattachée à son client.
            migrationBuilder.Sql("""
                INSERT INTO "Clients" ("Numero", "CreeLe", "Nom", "Prenom", "Telephone", "Email")
                SELECT 'CLI-' || lpad((row_number() OVER (ORDER BY premiere."CreeLe", premiere."Id"))::text, 5, '0'),
                       premiere."CreeLe", premiere."Nom", premiere."Prenom", premiere."Telephone", premiere.email
                FROM (
                    SELECT DISTINCT ON (lower("Email")) lower("Email") AS email, "Id", "CreeLe", "Nom", "Prenom", "Telephone"
                    FROM "Demandes"
                    ORDER BY lower("Email"), "CreeLe", "Id"
                ) AS premiere;

                UPDATE "Demandes" AS d SET "ClientId" = c."Id" FROM "Clients" AS c WHERE c."Email" = lower(d."Email");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_ClientId",
                table: "Demandes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Numero",
                table: "Clients",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Telephone",
                table: "Clients",
                column: "Telephone");

            migrationBuilder.AddForeignKey(
                name: "FK_Demandes_Clients_ClientId",
                table: "Demandes",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demandes_Clients_ClientId",
                table: "Demandes");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Demandes_ClientId",
                table: "Demandes");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Demandes");
        }
    }
}
