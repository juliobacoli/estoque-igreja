using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueIgreja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDoadorMovimentacaoSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Doador",
                table: "MovimentacoesSociais");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Doador",
                table: "MovimentacoesSociais",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }
    }
}
