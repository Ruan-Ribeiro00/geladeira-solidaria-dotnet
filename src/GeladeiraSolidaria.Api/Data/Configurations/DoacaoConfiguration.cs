using GeladeiraSolidaria.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GeladeiraSolidaria.Api.Data.Configurations;

/// <summary>
/// Mapeamento da tabela T_GSNET_DOACAO e do relacionamento 1:N com a geladeira.
/// </summary>
public class DoacaoConfiguration : IEntityTypeConfiguration<Doacao>
{
    public void Configure(EntityTypeBuilder<Doacao> builder)
    {
        builder.ToTable("T_GSNET_DOACAO", t =>
        {
            t.HasCheckConstraint("CK_GSNET_DOA_QUANTIDADE", "QUANTIDADE_KG > 0");
            // CHECKs de STATUS e CATEGORIA: criadas por SQL na migration CriarTabelas.
        });

        builder.HasKey(d => d.Id).HasName("PK_GSNET_DOACAO");

        builder.Property(d => d.Id)
            .HasColumnName("ID_DOACAO")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.GeladeiraId)
            .HasColumnName("ID_GELADEIRA")
            .IsRequired();

        builder.Property(d => d.Descricao)
            .HasColumnName("DESCRICAO")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(d => d.Categoria)
            .HasColumnName("CATEGORIA")
            .HasMaxLength(20)
            .HasConversion(c => c.ToString().ToUpper(), c => Enum.Parse<CategoriaAlimento>(c, true))
            .IsRequired();

        builder.Property(d => d.QuantidadeKg)
            .HasColumnName("QUANTIDADE_KG")
            .HasPrecision(7, 2)
            .IsRequired();

        builder.Property(d => d.DataValidade)
            .HasColumnName("DT_VALIDADE")
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(d => d.NomeDoador)
            .HasColumnName("NOME_DOADOR")
            .HasMaxLength(100);

        builder.Property(d => d.Status)
            .HasColumnName("STATUS")
            .HasMaxLength(15)
            .HasConversion(s => s.ToString().ToUpper(), s => Enum.Parse<StatusDoacao>(s, true))
            .IsRequired();

        builder.Property(d => d.DataDoacao)
            .HasColumnName("DT_DOACAO")
            .HasColumnType("DATE")
            .IsRequired();

        // Relacionamento 1:N: uma geladeira tem muitas doacoes.
        // Restrict: o banco nao deixa apagar uma geladeira que ainda tem
        // historico de doacoes (a API responde 409 antes de chegar nisso).
        builder.HasOne(d => d.Geladeira)
            .WithMany(g => g.Doacoes)
            .HasForeignKey(d => d.GeladeiraId)
            .HasConstraintName("FK_GSNET_DOA_GELADEIRA")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.GeladeiraId).HasDatabaseName("IX_GSNET_DOA_GELADEIRA");
        builder.HasIndex(d => new { d.Status, d.DataValidade }).HasDatabaseName("IX_GSNET_DOA_STATUS_VALIDADE");
    }
}
