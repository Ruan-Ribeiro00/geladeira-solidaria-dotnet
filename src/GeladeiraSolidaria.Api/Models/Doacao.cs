namespace GeladeiraSolidaria.Api.Models;

/// <summary>
/// Alimento deixado numa geladeira. Cada doacao pertence a exatamente uma geladeira.
/// </summary>
public class Doacao
{
    public int Id { get; set; }

    /// <summary>Chave estrangeira para a geladeira (lado "1" do relacionamento).</summary>
    public int GeladeiraId { get; set; }

    public Geladeira Geladeira { get; set; } = null!;

    public string Descricao { get; set; } = string.Empty;

    public CategoriaAlimento Categoria { get; set; }

    public decimal QuantidadeKg { get; set; }

    public DateTime DataValidade { get; set; }

    /// <summary>Nome de quem doou. Opcional: muita gente doa sem se identificar.</summary>
    public string? NomeDoador { get; set; }

    public StatusDoacao Status { get; set; } = StatusDoacao.Disponivel;

    public DateTime DataDoacao { get; set; }
}
