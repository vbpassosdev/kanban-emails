using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanEmails.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailKanban",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Remetente = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Assunto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorpoTexto = table.Column<string>(type: "varchar(max)", nullable: true),
                    CorpoHtml = table.Column<string>(type: "varchar(max)", nullable: true),
                    Resumo = table.Column<string>(type: "varchar(max)", nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailKanban", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailRemetenteMonitorado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailOuDominio = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRemetenteMonitorado", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAnexo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailKanbanId = table.Column<int>(type: "int", nullable: false),
                    NomeArquivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAnexo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAnexo_EmailKanban_EmailKanbanId",
                        column: x => x.EmailKanbanId,
                        principalTable: "EmailKanban",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailKanbanHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailKanbanId = table.Column<int>(type: "int", nullable: false),
                    StatusAnterior = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusNovo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DataMovimento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailKanbanHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailKanbanHistorico_EmailKanban_EmailKanbanId",
                        column: x => x.EmailKanbanId,
                        principalTable: "EmailKanban",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAnexo_EmailKanbanId",
                table: "EmailAnexo",
                column: "EmailKanbanId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailKanban_DataRecebimento",
                table: "EmailKanban",
                column: "DataRecebimento");

            migrationBuilder.CreateIndex(
                name: "IX_EmailKanban_MessageId",
                table: "EmailKanban",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailKanbanHistorico_EmailKanbanId",
                table: "EmailKanbanHistorico",
                column: "EmailKanbanId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRemetenteMonitorado_EmailOuDominio",
                table: "EmailRemetenteMonitorado",
                column: "EmailOuDominio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAnexo");

            migrationBuilder.DropTable(
                name: "EmailKanbanHistorico");

            migrationBuilder.DropTable(
                name: "EmailRemetenteMonitorado");

            migrationBuilder.DropTable(
                name: "EmailKanban");
        }
    }
}
