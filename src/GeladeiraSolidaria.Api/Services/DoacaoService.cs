using GeladeiraSolidaria.Api.Data;
using GeladeiraSolidaria.Api.Dtos;
using GeladeiraSolidaria.Api.Exceptions;
using GeladeiraSolidaria.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GeladeiraSolidaria.Api.Services;

public class DoacaoService(AppDbContext db, GeladeiraService geladeiras)
{
    public async Task<PaginaResponse<DoacaoResponse>> ListarAsync(
        int? geladeiraId, StatusDoacao? status, int? venceEmDias, int pagina, int tamanhoPagina)
    {
        var query = db.Doacoes.AsNoTracking().Include(d => d.Geladeira).AsQueryable();

        if (geladeiraId is not null)
        {
            query = query.Where(d => d.GeladeiraId == geladeiraId);
        }

        if (status is not null)
        {
            query = query.Where(d => d.Status == status);
        }

        if (venceEmDias is not null)
        {
            // Lista de distribuicao: o que vence ate a data limite, do mais urgente ao menos urgente.
            var limite = DateTime.Today.AddDays(venceEmDias.Value);
            query = query.Where(d => d.DataValidade <= limite);
        }

        var total = await query.CountAsync();

        var itens = await query
            .OrderBy(d => d.DataValidade)
            .ThenBy(d => d.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return new PaginaResponse<DoacaoResponse>(itens.Select(ParaResponse).ToList(), pagina, tamanhoPagina, total);
    }

    public async Task<DoacaoResponse> BuscarAsync(int id)
    {
        var doacao = await db.Doacoes.AsNoTracking().Include(d => d.Geladeira).FirstOrDefaultAsync(d => d.Id == id)
                     ?? throw new NaoEncontradoException($"Doação {id} não encontrada");
        return ParaResponse(doacao);
    }

    /// <summary>
    /// Regras para aceitar uma doacao:
    /// 1. a geladeira existe (senao 404);
    /// 2. a geladeira esta ATIVA;
    /// 3. o alimento nao chega vencido;
    /// 4. a doacao cabe no espaco livre. Se nao couber, a mensagem sugere outra geladeira.
    /// </summary>
    public async Task<DoacaoResponse> CriarAsync(DoacaoRequest request)
    {
        var geladeira = await db.Geladeiras.FindAsync(request.GeladeiraId)
                        ?? throw new NaoEncontradoException($"Geladeira {request.GeladeiraId} não encontrada");

        if (geladeira.Status != StatusGeladeira.Ativa)
        {
            throw new RegraNegocioException(
                $"A geladeira '{geladeira.Nome}' está {geladeira.Status.ToString().ToUpper()} e não recebe doações");
        }

        GarantirDentroDaValidade(request.DataValidade!.Value);
        await GarantirEspacoAsync(geladeira, request.QuantidadeKg, quantidadeAtualDaDoacao: 0);

        var doacao = new Doacao
        {
            GeladeiraId = geladeira.Id,
            Descricao = request.Descricao.Trim(),
            Categoria = request.Categoria!.Value,
            QuantidadeKg = request.QuantidadeKg,
            DataValidade = request.DataValidade.Value.ToDateTime(TimeOnly.MinValue),
            NomeDoador = string.IsNullOrWhiteSpace(request.NomeDoador) ? null : request.NomeDoador.Trim(),
            Status = StatusDoacao.Disponivel,
            DataDoacao = DateTime.Now
        };

        db.Doacoes.Add(doacao);
        await db.SaveChangesAsync();

        return await BuscarAsync(doacao.Id);
    }

    /// <summary>Corrige uma doacao. So vale enquanto o alimento esta na geladeira.</summary>
    public async Task AtualizarAsync(int id, DoacaoUpdateRequest request)
    {
        var doacao = await db.Doacoes.Include(d => d.Geladeira).FirstOrDefaultAsync(d => d.Id == id)
                     ?? throw new NaoEncontradoException($"Doação {id} não encontrada");

        if (doacao.Status != StatusDoacao.Disponivel)
        {
            throw new RegraNegocioException(
                $"A doação já foi {doacao.Status.ToString().ToUpper()} e não pode mais ser alterada");
        }

        GarantirDentroDaValidade(request.DataValidade!.Value);
        await GarantirEspacoAsync(doacao.Geladeira, request.QuantidadeKg, quantidadeAtualDaDoacao: doacao.QuantidadeKg);

        doacao.Descricao = request.Descricao.Trim();
        doacao.Categoria = request.Categoria!.Value;
        doacao.QuantidadeKg = request.QuantidadeKg;
        doacao.DataValidade = request.DataValidade.Value.ToDateTime(TimeOnly.MinValue);
        doacao.NomeDoador = string.IsNullOrWhiteSpace(request.NomeDoador) ? null : request.NomeDoador.Trim();

        await db.SaveChangesAsync();
    }

    /// <summary>Marca o alimento como retirado ou descartado. Libera espaco na geladeira.</summary>
    public async Task AlterarStatusAsync(int id, StatusDoacao novoStatus)
    {
        var doacao = await db.Doacoes.FindAsync(id)
                     ?? throw new NaoEncontradoException($"Doação {id} não encontrada");

        if (novoStatus == StatusDoacao.Disponivel)
        {
            throw new RegraNegocioException("Use RETIRADA ou DESCARTADA. Uma doação não volta a ficar disponível");
        }

        if (doacao.Status != StatusDoacao.Disponivel)
        {
            throw new RegraNegocioException(
                $"A doação já está {doacao.Status.ToString().ToUpper()}");
        }

        doacao.Status = novoStatus;
        await db.SaveChangesAsync();
    }

    /// <summary>Apaga uma doacao registrada por engano.</summary>
    public async Task ExcluirAsync(int id)
    {
        var doacao = await db.Doacoes.FindAsync(id)
                     ?? throw new NaoEncontradoException($"Doação {id} não encontrada");

        db.Doacoes.Remove(doacao);
        await db.SaveChangesAsync();
    }

    private static void GarantirDentroDaValidade(DateOnly validade)
    {
        if (validade < DateOnly.FromDateTime(DateTime.Today))
        {
            throw new RegraNegocioException($"O alimento venceu em {validade:dd/MM/yyyy} e não pode ser doado");
        }
    }

    private async Task GarantirEspacoAsync(Geladeira geladeira, decimal quantidadeNova, decimal quantidadeAtualDaDoacao)
    {
        var ocupacao = await geladeiras.OcupacaoKgAsync(geladeira.Id) - quantidadeAtualDaDoacao;
        var livre = geladeira.CapacidadeKg - ocupacao;

        if (quantidadeNova <= livre)
        {
            return;
        }

        var sugestao = await geladeiras.SugerirComEspacoAsync(quantidadeNova, ignorarId: geladeira.Id);
        var complemento = sugestao is null
            ? " Nenhuma outra geladeira ativa tem espaço para essa quantidade agora."
            : $" Sugestão: '{sugestao.Nome}' ({sugestao.Bairro}, id {sugestao.Id}) tem {sugestao.LivreKg:0.##} kg livres.";

        throw new RegraNegocioException(
            $"A doação de {quantidadeNova:0.##} kg não cabe na geladeira '{geladeira.Nome}' ({Math.Max(0, livre):0.##} kg livres).{complemento}");
    }

    private static DoacaoResponse ParaResponse(Doacao d) => new(
        d.Id,
        d.GeladeiraId,
        d.Geladeira.Nome,
        d.Descricao,
        d.Categoria,
        d.QuantidadeKg,
        DateOnly.FromDateTime(d.DataValidade),
        (d.DataValidade.Date - DateTime.Today).Days,
        d.NomeDoador,
        d.Status,
        d.DataDoacao);
}
