using GeladeiraSolidaria.Api.Data;
using GeladeiraSolidaria.Api.Dtos;
using GeladeiraSolidaria.Api.Exceptions;
using GeladeiraSolidaria.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GeladeiraSolidaria.Api.Services;

public class GeladeiraService(AppDbContext db)
{
    public async Task<PaginaResponse<GeladeiraResponse>> ListarAsync(
        string? bairro, StatusGeladeira? status, int pagina, int tamanhoPagina)
    {
        var query = db.Geladeiras.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(bairro))
        {
            // Sem diferenciar maiusculas nem acentos: "heliopolis" acha "Heliópolis".
            var termo = AppDbContext.NormalizarTermo(bairro);
            query = query.Where(g => AppDbContext.Normalizar(g.Bairro).Contains(termo));
        }

        if (status is not null)
        {
            query = query.Where(g => g.Status == status);
        }

        var total = await query.CountAsync();

        var itens = await ComOcupacao(query.OrderBy(g => g.Nome)
                .Skip((pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina))
            .ToListAsync();

        return new PaginaResponse<GeladeiraResponse>(itens.Select(ParaResponse).ToList(), pagina, tamanhoPagina, total);
    }

    public async Task<GeladeiraResponse> BuscarAsync(int id)
    {
        var item = await ComOcupacao(db.Geladeiras.AsNoTracking().Where(g => g.Id == id)).FirstOrDefaultAsync()
                   ?? throw new NaoEncontradoException($"Geladeira {id} não encontrada");
        return ParaResponse(item);
    }

    public async Task<GeladeiraResponse> CriarAsync(GeladeiraRequest request)
    {
        await GarantirNomeUnicoAsync(request.Nome, idAtual: null);

        var geladeira = new Geladeira { DataCadastro = DateTime.Now };
        Aplicar(geladeira, request);

        db.Geladeiras.Add(geladeira);
        await db.SaveChangesAsync();

        return await BuscarAsync(geladeira.Id);
    }

    public async Task AtualizarAsync(int id, GeladeiraRequest request)
    {
        var geladeira = await db.Geladeiras.FindAsync(id)
                        ?? throw new NaoEncontradoException($"Geladeira {id} não encontrada");

        await GarantirNomeUnicoAsync(request.Nome, idAtual: id);

        var ocupacao = await OcupacaoKgAsync(id);
        if (request.CapacidadeKg < ocupacao)
        {
            throw new RegraNegocioException(
                $"A geladeira tem {ocupacao:0.##} kg de alimentos disponíveis. A capacidade não pode ficar abaixo disso");
        }

        Aplicar(geladeira, request);
        await db.SaveChangesAsync();
    }

    public async Task ExcluirAsync(int id)
    {
        var geladeira = await db.Geladeiras.FindAsync(id)
                        ?? throw new NaoEncontradoException($"Geladeira {id} não encontrada");

        if (await db.Doacoes.AnyAsync(d => d.GeladeiraId == id))
        {
            throw new ConflitoException(
                "A geladeira tem histórico de doações e não pode ser excluída. Mude o status para INATIVA");
        }

        db.Geladeiras.Remove(geladeira);
        await db.SaveChangesAsync();
    }

    /// <summary>Soma dos kg das doacoes que ainda estao na geladeira.</summary>
    public async Task<decimal> OcupacaoKgAsync(int geladeiraId) =>
        await db.Doacoes
            .Where(d => d.GeladeiraId == geladeiraId && d.Status == StatusDoacao.Disponivel)
            .SumAsync(d => (decimal?)d.QuantidadeKg) ?? 0m;

    /// <summary>Geladeira ativa com mais espaco livre que comporta a quantidade (para sugerir ao doador).</summary>
    public async Task<GeladeiraResponse?> SugerirComEspacoAsync(decimal quantidadeKg, int ignorarId)
    {
        var candidatas = await ComOcupacao(db.Geladeiras.AsNoTracking()
                .Where(g => g.Status == StatusGeladeira.Ativa && g.Id != ignorarId))
            .ToListAsync();

        return candidatas
            .Select(ParaResponse)
            .Where(g => g.LivreKg >= quantidadeKg)
            .OrderByDescending(g => g.LivreKg)
            .FirstOrDefault();
    }

    private async Task GarantirNomeUnicoAsync(string nome, int? idAtual)
    {
        var nomeNormalizado = AppDbContext.NormalizarTermo(nome);
        var existe = await db.Geladeiras.AnyAsync(g => AppDbContext.Normalizar(g.Nome) == nomeNormalizado && g.Id != idAtual);
        if (existe)
        {
            throw new ConflitoException($"Já existe uma geladeira com o nome '{nome.Trim()}'");
        }
    }

    private static void Aplicar(Geladeira geladeira, GeladeiraRequest request)
    {
        geladeira.Nome = request.Nome.Trim();
        geladeira.Endereco = request.Endereco.Trim();
        geladeira.Bairro = request.Bairro.Trim();
        geladeira.CapacidadeKg = request.CapacidadeKg;
        geladeira.Status = request.Status;
        geladeira.DataInstalacao = request.DataInstalacao!.Value.ToDateTime(TimeOnly.MinValue);
        geladeira.Responsavel = string.IsNullOrWhiteSpace(request.Responsavel) ? null : request.Responsavel.Trim();
        geladeira.TelefoneResponsavel = string.IsNullOrWhiteSpace(request.TelefoneResponsavel) ? null : request.TelefoneResponsavel.Trim();
    }

    /// <summary>
    /// Traz a geladeira junto com a soma e a contagem das doacoes disponiveis,
    /// numa consulta so (subconsultas correlacionadas no SQL gerado).
    /// </summary>
    private static IQueryable<GeladeiraComOcupacao> ComOcupacao(IQueryable<Geladeira> query) =>
        query.Select(g => new GeladeiraComOcupacao(
            g,
            g.Doacoes.Where(d => d.Status == StatusDoacao.Disponivel).Sum(d => (decimal?)d.QuantidadeKg) ?? 0m,
            g.Doacoes.Count(d => d.Status == StatusDoacao.Disponivel)));

    private static GeladeiraResponse ParaResponse(GeladeiraComOcupacao item)
    {
        var g = item.Geladeira;
        var livre = Math.Max(0, g.CapacidadeKg - item.OcupacaoKg);
        var percentual = g.CapacidadeKg == 0 ? 0 : Math.Round(item.OcupacaoKg / g.CapacidadeKg * 100, 1);

        return new GeladeiraResponse(
            g.Id, g.Nome, g.Endereco, g.Bairro, g.CapacidadeKg, g.Status,
            DateOnly.FromDateTime(g.DataInstalacao), g.Responsavel, g.TelefoneResponsavel,
            item.OcupacaoKg, livre, percentual, item.DoacoesDisponiveis);
    }

    private sealed record GeladeiraComOcupacao(Geladeira Geladeira, decimal OcupacaoKg, int DoacoesDisponiveis);
}
