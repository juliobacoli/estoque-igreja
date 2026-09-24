using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueIgreja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCestaBasica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MontagemCestaId",
                table: "MovimentacoesSociais",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModelosCesta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CestasProntas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosCesta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MontagensCesta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    CestasAnteriores = table.Column<int>(type: "integer", nullable: false),
                    CestasNovas = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MontagensCesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MontagensCesta_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModeloCestaItens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModeloCestaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemSocialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeloCestaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModeloCestaItens_ItensSociais_ItemSocialId",
                        column: x => x.ItemSocialId,
                        principalTable: "ItensSociais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModeloCestaItens_ModelosCesta_ModeloCestaId",
                        column: x => x.ModeloCestaId,
                        principalTable: "ModelosCesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesSociais_MontagemCestaId",
                table: "MovimentacoesSociais",
                column: "MontagemCestaId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeloCestaItens_ItemSocialId",
                table: "ModeloCestaItens",
                column: "ItemSocialId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeloCestaItens_ModeloCestaId_ItemSocialId",
                table: "ModeloCestaItens",
                columns: new[] { "ModeloCestaId", "ItemSocialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MontagensCesta_Data",
                table: "MontagensCesta",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_MontagensCesta_UsuarioId",
                table: "MontagensCesta",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesSociais_MontagensCesta_MontagemCestaId",
                table: "MovimentacoesSociais",
                column: "MontagemCestaId",
                principalTable: "MontagensCesta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesSociais_MontagensCesta_MontagemCestaId",
                table: "MovimentacoesSociais");

            migrationBuilder.DropTable(
                name: "ModeloCestaItens");

            migrationBuilder.DropTable(
                name: "MontagensCesta");

            migrationBuilder.DropTable(
                name: "ModelosCesta");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesSociais_MontagemCestaId",
                table: "MovimentacoesSociais");

            migrationBuilder.DropColumn(
                name: "MontagemCestaId",
                table: "MovimentacoesSociais");
        }
    }
}
