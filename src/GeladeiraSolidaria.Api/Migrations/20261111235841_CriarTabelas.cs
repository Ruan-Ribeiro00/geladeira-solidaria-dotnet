using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeladeiraSolidaria.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_GSNET_GELADEIRA",
                columns: table => new
                {
                    ID_GELADEIRA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ENDERECO = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    BAIRRO = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false),
                    CAPACIDADE_KG = table.Column<decimal>(type: "DECIMAL(7,2)", precision: 7, scale: 2, nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    DT_INSTALACAO = table.Column<DateTime>(type: "DATE", nullable: false),
                    DT_CADASTRO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GSNET_GELADEIRA", x => x.ID_GELADEIRA);
                    table.CheckConstraint("CK_GSNET_GEL_CAPACIDADE", "CAPACIDADE_KG > 0");
                });

            migrationBuilder.CreateTable(
                name: "T_GSNET_DOACAO",
                columns: table => new
                {
                    ID_DOACAO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_GELADEIRA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    CATEGORIA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    QUANTIDADE_KG = table.Column<decimal>(type: "DECIMAL(7,2)", precision: 7, scale: 2, nullable: false),
                    DT_VALIDADE = table.Column<DateTime>(type: "DATE", nullable: false),
                    NOME_DOADOR = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    DT_DOACAO = table.Column<DateTime>(type: "DATE", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GSNET_DOACAO", x => x.ID_DOACAO);
                    table.CheckConstraint("CK_GSNET_DOA_QUANTIDADE", "QUANTIDADE_KG > 0");
                    table.ForeignKey(
                        name: "FK_GSNET_DOA_GELADEIRA",
                        column: x => x.ID_GELADEIRA,
                        principalTable: "T_GSNET_GELADEIRA",
                        principalColumn: "ID_GELADEIRA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GSNET_DOA_GELADEIRA",
                table: "T_GSNET_DOACAO",
                column: "ID_GELADEIRA");

            migrationBuilder.CreateIndex(
                name: "IX_GSNET_DOA_STATUS_VALIDADE",
                table: "T_GSNET_DOACAO",
                columns: new[] { "STATUS", "DT_VALIDADE" });

            migrationBuilder.CreateIndex(
                name: "UK_GSNET_GEL_NOME",
                table: "T_GSNET_GELADEIRA",
                column: "NOME",
                unique: true);

            // Listas de valores aceitos. Ficam fora do CREATE TABLE porque o provider
            // Oracle nao escapa as aspas simples no EXECUTE IMMEDIATE que ele gera.
            migrationBuilder.Sql(
                "ALTER TABLE T_GSNET_GELADEIRA ADD CONSTRAINT CK_GSNET_GEL_STATUS " +
                "CHECK (STATUS IN ('ATIVA', 'MANUTENCAO', 'INATIVA'))");

            migrationBuilder.Sql(
                "ALTER TABLE T_GSNET_DOACAO ADD CONSTRAINT CK_GSNET_DOA_STATUS " +
                "CHECK (STATUS IN ('DISPONIVEL', 'RETIRADA', 'DESCARTADA'))");

            migrationBuilder.Sql(
                "ALTER TABLE T_GSNET_DOACAO ADD CONSTRAINT CK_GSNET_DOA_CATEGORIA " +
                "CHECK (CATEGORIA IN ('HORTIFRUTI', 'LATICINIOS', 'CARNES', 'PAES', 'MARMITAS', 'MERCEARIA', 'BEBIDAS', 'OUTROS'))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_GSNET_DOACAO");

            migrationBuilder.DropTable(
                name: "T_GSNET_GELADEIRA");
        }
    }
}
