using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueIgreja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAcaoSocialItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItensSociais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NomeNormalizado = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Unidade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EstoqueAtual = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensSociais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesSociais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemSocialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuantidadeAnterior = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeNova = table.Column<int>(type: "integer", nullable: false),
                    Doador = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesSociais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesSociais_ItensSociais_ItemSocialId",
                        column: x => x.ItemSocialId,
                        principalTable: "ItensSociais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesSociais_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItensSociais_NomeNormalizado",
                table: "ItensSociais",
                column: "NomeNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesSociais_Data",
                table: "MovimentacoesSociais",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesSociais_ItemSocialId_Data",
                table: "MovimentacoesSociais",
                columns: new[] { "ItemSocialId", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesSociais_UsuarioId",
                table: "MovimentacoesSociais",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacoesSociais");

            migrationBuilder.DropTable(
                name: "ItensSociais");
        }
    }
}
