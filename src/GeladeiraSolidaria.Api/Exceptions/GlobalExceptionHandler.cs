using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GeladeiraSolidaria.Api.Exceptions;

/// <summary>
/// Transforma as excecoes da camada de servico em respostas ProblemDetails
/// (RFC 7807), o mesmo formato que o ASP.NET usa nos erros 400 de validacao.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetails)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            NaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ConflitoException => (StatusCodes.Status409Conflict, "Conflito"),
            RegraNegocioException => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada"),
            _ => (StatusCodes.Status500InternalServerError, "Erro inesperado")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            // O detalhe fica no log; o cliente recebe uma mensagem generica.
            logger.LogError(exception, "Erro não tratado em {Caminho}", context.Request.Path);
        }

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Ocorreu um erro inesperado. Tente novamente."
                    : exception.Message,
                Instance = context.Request.Path
            }
        });
    }
}
