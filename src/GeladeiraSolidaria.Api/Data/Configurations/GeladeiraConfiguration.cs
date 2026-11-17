using GeladeiraSolidaria.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeladeiraSolidaria.Api.Data.Configurations;

/// <summary>
/// Mapeamento da tabela T_GSNET_GELADEIRA. Nomes em maiusculas para o
/// SELECT no SQL Developer funcionar sem aspas.
/// </summary>
public class GeladeiraConfiguration : IEntityTypeConfiguration<Geladeira>
{
    public void Configure(EntityTypeBuilder<Geladeira> builder)
    {
        builder.ToTable("T_GSNET_GELADEIRA", t =>
        {
            t.HasCheckConstraint("CK_GSNET_GEL_CAPACIDADE", "CAPACIDADE_KG > 0");
            // A CHECK do STATUS (lista de textos) e criada por SQL na migration
            // CriarTabelas: o provider Oracle nao escapa as aspas dentro do
            // EXECUTE IMMEDIATE que ele gera para o CREATE TABLE.
        });

        builder.HasKey(g => g.Id).HasName("PK_GSNET_GELADEIRA");

        builder.Property(g => g.Id)
            .HasColumnName("ID_GELADEIRA")
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Nome)
            .HasColumnName("NOME")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(g => g.Nome)
            .IsUnique()
            .HasDatabaseName("UK_GSNET_GEL_NOME");

        builder.Property(g => g.Endereco)
            .HasColumnName("ENDERECO")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(g => g.Bairro)
            .HasColumnName("BAIRRO")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(g => g.CapacidadeKg)
            .HasColumnName("CAPACIDADE_KG")
            .HasPrecision(7, 2)
            .IsRequired();

        // Enum gravado como texto (ATIVA, MANUTENCAO...) em vez de numero,
        // para a tabela ser legivel direto no banco.
        builder.Property(g => g.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(15)
            .HasConversion(s => s.ToString().ToUpper(), s => Enum.Parse<StatusGeladeira>(s, true))
            .IsRequired();

        builder.Property(g => g.DataInstalacao)
            .HasColumnName("DT_INSTALACAO")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(g => g.Responsavel)
            .HasColumnName("RESPONSAVEL")
            .HasMaxLength(100);

        builder.Property(g => g.TelefoneResponsavel)
            .HasColumnName("TELEFONE_RESPONSAVEL")
            .HasMaxLength(20);

        builder.Property(g => g.DataCadastro)
            .HasColumnName("DT_CADASTRO")
            .HasColumnType("DATE")
            .IsRequired();
    }
}
