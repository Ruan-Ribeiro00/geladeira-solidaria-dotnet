namespace GeladeiraSolidaria.Api.Models;

/// <summary>Situacao de funcionamento da geladeira.</summary>
public enum StatusGeladeira
{
    Ativa,
    Manutencao,
    Inativa
}

/// <summary>O que aconteceu com o alimento doado.</summary>
public enum StatusDoacao
{
    /// <summary>Esta na geladeira, pronto para ser retirado.</summary>
    Disponivel,

    /// <summary>Alguem retirou o alimento.</summary>
    Retirada,

    /// <summary>O voluntario jogou fora (vencido ou estragado).</summary>
    Descartada
}

/// <summary>Tipo de alimento. Define se precisa de refrigeracao.</summary>
public enum CategoriaAlimento
{
    Hortifruti,
    Laticinios,
    Carnes,
    Paes,
    Marmitas,
    Mercearia,
    Bebidas,
    Outros
}
