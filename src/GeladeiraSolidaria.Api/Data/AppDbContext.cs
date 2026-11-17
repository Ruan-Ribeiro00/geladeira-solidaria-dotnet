using System.Linq.Expressions;
using GeladeiraSolidaria.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace GeladeiraSolidaria.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private const string ComAcento = "ÁÀÂÃÄÉÈÊËÍÌÎÏÓÒÔÕÖÚÙÛÜÇ";
    private const string SemAcentoChars = "AAAAAEEEEIIIIOOOOOUUUUC";

    public DbSet<Geladeira> Geladeiras => Set<Geladeira>();

    public DbSet<Doacao> Doacoes => Set<Doacao>();

    /// <summary>
    /// Texto em maiusculas e sem acento, calculado no Oracle com
    /// TRANSLATE(UPPER(texto), ...). Assim a busca por "heliopolis" encontra
    /// "Heliópolis". So pode ser usada dentro de consultas LINQ.
    /// </summary>
    public static string Normalizar(string texto) =>
        throw new NotSupportedException("Use apenas dentro de consultas do Entity Framework");

    /// <summary>Mesma normalizacao do banco, feita em C# no termo digitado pelo usuario.</summary>
    public static string NormalizarTermo(string texto)
    {
        var maiusculo = texto.Trim().ToUpperInvariant().ToCharArray();
        for (var i = 0; i < maiusculo.Length; i++)
        {
            var pos = ComAcento.IndexOf(maiusculo[i]);
            if (pos >= 0) maiusculo[i] = SemAcentoChars[pos];
        }
        return new string(maiusculo);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Le as classes de Data/Configurations (uma por entidade).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.HasDbFunction(typeof(AppDbContext).GetMethod(nameof(Normalizar))!)
            .HasTranslation(args =>
            {
                var texto = args.First();
                var tipo = texto.TypeMapping; // mesmo tipo da coluna (NVARCHAR2)

                var upper = new SqlFunctionExpression("UPPER", [texto], nullable: true,
                    argumentsPropagateNullability: [true], typeof(string), tipo);

                return new SqlFunctionExpression(
                    "TRANSLATE",
                    [
                        upper,
                        new SqlConstantExpression(Expression.Constant(ComAcento), tipo),
                        new SqlConstantExpression(Expression.Constant(SemAcentoChars), tipo)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, false, false],
                    typeof(string),
                    tipo);
            });
    }
}
