using System.ComponentModel.DataAnnotations;
using GeladeiraSolidaria.Api.Dtos;
using GeladeiraSolidaria.Api.Models;
using GeladeiraSolidaria.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeladeiraSolidaria.Api.Controllers;

/// <summary>Alimentos doados para as geladeiras.</summary>
[ApiController]
[Route("api/doacoes")]
[Produces("application/json")]
public class DoacoesController(DoacaoService service) : ControllerBase
{
    /// <summary>Lista as doacoes, da que vence primeiro para a que vence depois.</summary>
    /// <param name="geladeiraId">Filtra por geladeira.</param>
    /// <param name="status">DISPONIVEL, RETIRADA ou DESCARTADA.</param>
    /// <param name="venceEmDias">So o que vence ate N dias a partir de hoje (lista de distribuicao).</param>
    /// <param name="pagina">Comeca em 1.</param>
    /// <param name="tamanhoPagina">De 1 a 50.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaResponse<DoacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaResponse<DoacaoResponse>>> Listar(
        [FromQuery] int? geladeiraId,
        [FromQuery] StatusDoacao? status,
        [FromQuery, Range(0, 30, ErrorMessage = "venceEmDias deve ficar entre 0 e 30")] int? venceEmDias,
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "A página começa em 1")] int pagina = 1,
        [FromQuery, Range(1, 50, ErrorMessage = "O tamanho da página deve ficar entre 1 e 50")] int tamanhoPagina = 10)
    {
        return Ok(await service.ListarAsync(geladeiraId, status, venceEmDias, pagina, tamanhoPagina));
    }

    /// <summary>Busca uma doacao pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DoacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoacaoResponse>> Buscar(int id)
    {
        return Ok(await service.BuscarAsync(id));
    }

    /// <summary>Registra uma doacao numa geladeira.</summary>
    /// <remarks>
    /// A doacao e recusada (422) se a geladeira nao estiver ATIVA, se o alimento
    /// ja estiver vencido ou se nao couber. Quando nao cabe, a mensagem sugere a
    /// geladeira ativa com mais espaco livre.
    /// </remarks>
    /// <response code="201">Registrada. O cabecalho Location aponta para ela.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="404">Geladeira nao encontrada.</response>
    /// <response code="422">Regra de negocio (geladeira lotada, inativa ou alimento vencido).</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(DoacaoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DoacaoResponse>> Criar([FromBody] DoacaoRequest request)
    {
        var criada = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Buscar), new { id = criada.Id }, criada);
    }

    /// <summary>Corrige uma doacao que ainda esta disponivel.</summary>
    /// <response code="204">Atualizada.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="404">Doacao nao encontrada.</response>
    /// <response code="422">A doacao ja saiu da geladeira, venceu ou nao cabe.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] DoacaoUpdateRequest request)
    {
        await service.AtualizarAsync(id, request);
        return NoContent();
    }

    /// <summary>Marca o alimento como RETIRADA (alguem levou) ou DESCARTADA (estragou).</summary>
    /// <response code="204">Status alterado.</response>
    /// <response code="400">Status invalido.</response>
    /// <response code="404">Doacao nao encontrada.</response>
    /// <response code="422">A doacao ja tinha saido da geladeira.</response>
    [HttpPatch("{id:int}/status")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AlterarStatus(int id, [FromBody] DoacaoStatusRequest request)
    {
        await service.AlterarStatusAsync(id, request.Status!.Value);
        return NoContent();
    }

    /// <summary>Exclui uma doacao registrada por engano.</summary>
    /// <response code="204">Excluida.</response>
    /// <response code="404">Doacao nao encontrada.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        await service.ExcluirAsync(id);
        return NoContent();
    }
}
