using System.ComponentModel.DataAnnotations;
using GeladeiraSolidaria.Api.Models;

namespace GeladeiraSolidaria.Api.Dtos;

/// <summary>Dados para registrar uma doacao.</summary>
public record DoacaoRequest
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe o id da geladeira")]
    public int GeladeiraId { get; init; }

    /// <example>Marmitas de arroz, feijão e frango</example>
    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 120 caracteres")]
    public string Descricao { get; init; } = string.Empty;

    /// <example>MARMITAS</example>
    [Required(ErrorMessage = "A categoria é obrigatória")]
    [EnumDataType(typeof(CategoriaAlimento), ErrorMessage = "Categoria inválida")]
    public CategoriaAlimento? Categoria { get; init; }

    /// <example>5</example>
    [Range(typeof(decimal), "0.1", "200", ErrorMessage = "A quantidade deve ficar entre 0,1 e 200 kg")]
    public decimal QuantidadeKg { get; init; }

    /// <example>2026-11-20</example>
    [Required(ErrorMessage = "A data de validade é obrigatória")]
    public DateOnly? DataValidade { get; init; }

    /// <example>Restaurante Bom Prato</example>
    [StringLength(100, ErrorMessage = "O nome do doador deve ter até 100 caracteres")]
    public string? NomeDoador { get; init; }
}

/// <summary>Dados para corrigir uma doacao que ainda esta disponivel.</summary>
public record DoacaoUpdateRequest
{
    /// <example>Marmitas de arroz, feijão e frango</example>
    [Required(ErrorMessage = "A descrição é obrigatória")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 120 caracteres")]
    public string Descricao { get; init; } = string.Empty;

    /// <example>MARMITAS</example>
    [Required(ErrorMessage = "A categoria é obrigatória")]
    [EnumDataType(typeof(CategoriaAlimento), ErrorMessage = "Categoria inválida")]
    public CategoriaAlimento? Categoria { get; init; }

    /// <example>4</example>
    [Range(typeof(decimal), "0.1", "200", ErrorMessage = "A quantidade deve ficar entre 0,1 e 200 kg")]
    public decimal QuantidadeKg { get; init; }

    /// <example>2026-11-20</example>
    [Required(ErrorMessage = "A data de validade é obrigatória")]
    public DateOnly? DataValidade { get; init; }

    /// <example>Restaurante Bom Prato</example>
    [StringLength(100, ErrorMessage = "O nome do doador deve ter até 100 caracteres")]
    public string? NomeDoador { get; init; }
}

/// <summary>Muda o status de uma doacao (retirada ou descarte).</summary>
public record DoacaoStatusRequest
{
    /// <summary>RETIRADA ou DESCARTADA.</summary>
    /// <example>RETIRADA</example>
    [Required(ErrorMessage = "O status é obrigatório")]
    [EnumDataType(typeof(StatusDoacao), ErrorMessage = "Status inválido")]
    public StatusDoacao? Status { get; init; }
}

public record DoacaoResponse(
    int Id,
    int GeladeiraId,
    string Geladeira,
    string Descricao,
    CategoriaAlimento Categoria,
    decimal QuantidadeKg,
    DateOnly DataValidade,
    int DiasParaVencer,
    string? NomeDoador,
    StatusDoacao Status,
    DateTime DataDoacao);
