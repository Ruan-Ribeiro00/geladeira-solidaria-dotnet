using System.ComponentModel.DataAnnotations;
using GeladeiraSolidaria.Api.Models;

namespace GeladeiraSolidaria.Api.Dtos;

/// <summary>Dados para cadastrar ou atualizar uma geladeira.</summary>
public record GeladeiraRequest : IValidatableObject
{
    /// <example>Geladeira Solidária Vila Prudente</example>
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; init; } = string.Empty;

    /// <example>Rua Ibitirama, 900</example>
    [Required(ErrorMessage = "O endereço é obrigatório")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "O endereço deve ter entre 5 e 150 caracteres")]
    public string Endereco { get; init; } = string.Empty;

    /// <example>Vila Prudente</example>
    [Required(ErrorMessage = "O bairro é obrigatório")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "O bairro deve ter entre 2 e 60 caracteres")]
    public string Bairro { get; init; } = string.Empty;

    /// <example>80</example>
    [Range(typeof(decimal), "1", "1000", ErrorMessage = "A capacidade deve ficar entre 1 e 1000 kg")]
    public decimal CapacidadeKg { get; init; }

    /// <summary>ATIVA, MANUTENCAO ou INATIVA. Se nao vier, a geladeira entra como ATIVA.</summary>
    /// <example>ATIVA</example>
    [EnumDataType(typeof(StatusGeladeira), ErrorMessage = "Status inválido")]
    public StatusGeladeira Status { get; init; } = StatusGeladeira.Ativa;

    /// <example>2026-03-15</example>
    [Required(ErrorMessage = "A data de instalação é obrigatória")]
    public DateOnly? DataInstalacao { get; init; }

    /// <example>Coletivo Vila Prudente Solidária</example>
    [StringLength(100, ErrorMessage = "O responsável deve ter até 100 caracteres")]
    public string? Responsavel { get; init; }

    /// <example>(11) 98765-4321</example>
    [RegularExpression(@"^\(?\d{2}\)?\s?9?\d{4}-?\d{4}$", ErrorMessage = "Telefone inválido. Use o formato (11) 98765-4321")]
    public string? TelefoneResponsavel { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataInstalacao > DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult(
                "A data de instalação não pode estar no futuro",
                [nameof(DataInstalacao)]);
        }
    }
}

/// <summary>Geladeira com a ocupacao calculada a partir das doacoes disponiveis.</summary>
public record GeladeiraResponse(
    int Id,
    string Nome,
    string Endereco,
    string Bairro,
    decimal CapacidadeKg,
    StatusGeladeira Status,
    DateOnly DataInstalacao,
    string? Responsavel,
    string? TelefoneResponsavel,
    decimal OcupacaoKg,
    decimal LivreKg,
    decimal OcupacaoPercentual,
    int DoacoesDisponiveis);
