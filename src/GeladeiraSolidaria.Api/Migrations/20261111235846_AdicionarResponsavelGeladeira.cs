using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeladeiraSolidaria.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarResponsavelGeladeira : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RESPONSAVEL",
                table: "T_GSNET_GELADEIRA",
                type: "NVARCHAR2(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TELEFONE_RESPONSAVEL",
                table: "T_GSNET_GELADEIRA",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RESPONSAVEL",
                table: "T_GSNET_GELADEIRA");

            migrationBuilder.DropColumn(
                name: "TELEFONE_RESPONSAVEL",
                table: "T_GSNET_GELADEIRA");
        }
    }
}
