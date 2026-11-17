using System.ComponentModel.DataAnnotations;
using GeladeiraSolidaria.Api.Dtos;
using GeladeiraSolidaria.Api.Models;
using GeladeiraSolidaria.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeladeiraSolidaria.Api.Controllers;

/// <summary>Cadastro das geladeiras da rede.</summary>
[ApiController]
[Route("api/geladeiras")]
[Produces("application/json")]
public class GeladeirasController(GeladeiraService service, DoacaoService doacoes) : ControllerBase
{
    /// <summary>Lista as geladeiras, com filtro por bairro e status.</summary>
    /// <param name="bairro">Parte do nome do bairro, sem diferenciar acentos (ex.: "heliopolis").</param>
    /// <param name="status">ATIVA, MANUTENCAO ou INATIVA.</param>
    /// <param name="pagina">Comeca em 1.</param>
    /// <param name="tamanhoPagina">De 1 a 50.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaResponse<GeladeiraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResponse<GeladeiraResponse>>> Listar(
        [FromQuery] string? bairro,
        [FromQuery] StatusGeladeira? status,
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "A página começa em 1")] int pagina = 1,
        [FromQuery, Range(1, 50, ErrorMessage = "O tamanho da página deve ficar entre 1 e 50")] int tamanhoPagina = 10)
    {
        return Ok(await service.ListarAsync(bairro, status, pagina, tamanhoPagina));
    }

    /// <summary>Busca uma geladeira pelo id, com a ocupacao atual.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GeladeiraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GeladeiraResponse>> Buscar(int id)
    {
        return Ok(await service.BuscarAsync(id));
    }

    /// <summary>Lista as doacoes de uma geladeira (lado N do relacionamento).</summary>
    /// <param name="id">Id da geladeira.</param>
    /// <param name="status">DISPONIVEL, RETIRADA ou DESCARTADA.</param>
    /// <param name="pagina">Comeca em 1.</param>
    /// <param name="tamanhoPagina">De 1 a 50.</param>
    [HttpGet("{id:int}/doacoes")]
    [ProducesResponseType(typeof(PaginaResponse<DoacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginaResponse<DoacaoResponse>>> ListarDoacoes(
        int id,
        [FromQuery] StatusDoacao? status,
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "A página começa em 1")] int pagina = 1,
        [FromQuery, Range(1, 50, ErrorMessage = "O tamanho da página deve ficar entre 1 e 50")] int tamanhoPagina = 10)
    {
        await service.BuscarAsync(id); // 404 se a geladeira nao existir
        return Ok(await doacoes.ListarAsync(id, status, null, pagina, tamanhoPagina));
    }

    /// <summary>Cadastra uma geladeira.</summary>
    /// <response code="201">Criada. O cabecalho Location aponta para ela.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="409">Ja existe uma geladeira com esse nome.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(GeladeiraResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GeladeiraResponse>> Criar([FromBody] GeladeiraRequest request)
    {
        var criada = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Buscar), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza todos os dados de uma geladeira.</summary>
    /// <response code="204">Atualizada.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="404">Geladeira nao encontrada.</response>
    /// <response code="409">Nome repetido.</response>
    /// <response code="422">Capacidade menor do que o que ja esta dentro.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] GeladeiraRequest request)
    {
        await service.AtualizarAsync(id, request);
        return NoContent();
    }

    /// <summary>Exclui uma geladeira sem historico de doacoes.</summary>
    /// <response code="204">Excluida.</response>
    /// <response code="404">Geladeira nao encontrada.</response>
    /// <response code="409">A geladeira tem doacoes. Use o status INATIVA.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        await service.ExcluirAsync(id);
        return NoContent();
    }
}
