using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeladeiraSolidaria.Api.Data;
using GeladeiraSolidaria.Api.Exceptions;
using GeladeiraSolidaria.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Oracle.EntityFrameworkCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// A connection string NAO fica no repositorio. Ela vem dos User Secrets
// (dotnet user-secrets) ou da variavel de ambiente ConnectionStrings__Oracle.
var connectionString = builder.Configuration.GetConnectionString("Oracle");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'Oracle' nao configurada. Rode na pasta do projeto: " +
        "dotnet user-secrets set \"ConnectionStrings:Oracle\" " +
        "\"User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL\"");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString, oracle =>
        // Gera SQL compativel com o Oracle 19c da FIAP.
        oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));

builder.Services.AddScoped<GeladeiraService>();
builder.Services.AddScoped<DoacaoService>();

builder.Services.AddControllers(options =>
    {
        // As regras de obrigatoriedade ficam so nos [Required] dos DTOs.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validacao (400) no formato ValidationProblemDetails, com titulo em portugues.
        options.InvalidModelStateResponseFactory = context =>
        {
            var problema = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Status = StatusCodes.Status400BadRequest,
                Title = "Dados inválidos",
                Detail = "Corrija os campos listados em 'errors' e envie de novo.",
                Instance = context.HttpContext.Request.Path
            };
            // Erros de conversao do JSON ("$.status", "$.dataValidade"...) vem em ingles
            // e com nome de tipo .NET. Troca por uma mensagem clara.
            foreach (var chave in problema.Errors.Keys.Where(k => k.StartsWith('$')).ToList())
            {
                var campo = chave.TrimStart('$', '.');
                problema.Errors[chave] = [string.IsNullOrEmpty(campo)
                    ? "JSON inválido. Confira aspas, vírgulas e chaves."
                    : $"Valor inválido para '{campo}'. Datas no formato AAAA-MM-DD e listas com um dos valores aceitos (ex.: ATIVA, MARMITAS)."];
            }

            return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
        };
    })
    .AddJsonOptions(options =>
    {
        // Enums como texto em maiusculas (ATIVA, DISPONIVEL...), igual ao banco.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        // Acentos aparecem normais no JSON ("Heliópolis" em vez de "Heli\u00f3polis").
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

// Mesmo ajuste para as respostas de erro (ProblemDetails).
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Geladeira Solidaria Inteligente - API .NET",
        Version = "v1",
        Description = "Global Solution 2026/2 (CitizenTech) - FIAP. Cadastro das geladeiras comunitarias " +
                      "e das doacoes de alimentos, com ocupacao calculada e regras que evitam desperdicio.",
        Contact = new OpenApiContact { Name = "Ruan Ribeiro - RM 567693 - 2TDSPA" }
    });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xml);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options => options.DocumentTitle = "Geladeira Solidaria - API .NET");

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
