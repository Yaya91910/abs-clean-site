using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AbsClean.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initiale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Demandes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreeLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Profil = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Frequence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NbVitres = table.Column<int>(type: "integer", nullable: false),
                    NbBaies = table.Column<int>(type: "integer", nullable: false),
                    NbVelux = table.Column<int>(type: "integer", nullable: false),
                    NbVitrines = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Nom = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Adresse = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CodePostal = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Ville = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Acces = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Creneaux = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConsentementLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmailClientEnvoye = table.Column<bool>(type: "boolean", nullable: false),
                    EmailGerantEnvoye = table.Column<bool>(type: "boolean", nullable: false),
                    SmsEnvoye = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Demandes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Demandes_Numero",
                table: "Demandes",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Demandes");
        }
    }
}
