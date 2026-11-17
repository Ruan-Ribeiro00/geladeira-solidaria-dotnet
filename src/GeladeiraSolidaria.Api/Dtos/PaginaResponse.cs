namespace GeladeiraSolidaria.Api.Dtos;

/// <summary>Uma pagina de resultados, com o total para o app montar a paginacao.</summary>
public record PaginaResponse<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens)
{
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}
