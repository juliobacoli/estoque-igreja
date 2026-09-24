using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueIgreja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAcessoPorModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcessoAcaoSocial",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AcessoObreiros",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Com a coluna em false, quem já existe ficaria sem nenhum módulo e não
            // conseguiria mais entrar. Todo usuário de hoje é da equipe de obreiros.
            migrationBuilder.Sql(@"UPDATE ""Usuarios"" SET ""AcessoObreiros"" = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcessoAcaoSocial",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AcessoObreiros",
                table: "Usuarios");
        }
    }
}
