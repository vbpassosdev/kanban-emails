using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanEmails.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioConfiguracaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracaoEmail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Porta = table.Column<int>(type: "int", nullable: false),
                    UsarSsl = table.Column<bool>(type: "bit", nullable: false),
                    EmailUsuario = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SenhaCriptografada = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Pasta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IntervaloMinutos = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracaoEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracaoEmail_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracaoEmail_UsuarioId",
                table: "ConfiguracaoEmail",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuario",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracaoEmail");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
