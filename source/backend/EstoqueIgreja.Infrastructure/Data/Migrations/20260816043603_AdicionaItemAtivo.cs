using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueIgreja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaItemAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Itens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // O EF preenche a coluna nova com false, o que deixaria todo item já
            // cadastrado como removido e esvaziaria a Dashboard no primeiro deploy.
            // Quem existe hoje está ativo.
            migrationBuilder.Sql(@"UPDATE ""Itens"" SET ""Ativo"" = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Itens");
        }
    }
}
