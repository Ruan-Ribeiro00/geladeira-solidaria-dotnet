namespace GeladeiraSolidaria.Api.Models;

/// <summary>
/// Geladeira comunitaria instalada num espaco publico. Recebe doacoes de
/// alimentos e qualquer pessoa pode retirar o que precisa.
/// </summary>
public class Geladeira
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    /// <summary>Quanto de alimento (em kg) a geladeira comporta.</summary>
    public decimal CapacidadeKg { get; set; }

    public StatusGeladeira Status { get; set; } = StatusGeladeira.Ativa;

    public DateTime DataInstalacao { get; set; }

    /// <summary>Pessoa ou entidade que cuida da geladeira (incluido na migration 2).</summary>
    public string? Responsavel { get; set; }

    public string? TelefoneResponsavel { get; set; }

    public DateTime DataCadastro { get; set; }

    /// <summary>Lado "N" do relacionamento 1:N.</summary>
    public ICollection<Doacao> Doacoes { get; set; } = new List<Doacao>();
}
