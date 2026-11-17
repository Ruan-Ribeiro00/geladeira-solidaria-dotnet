using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeladeiraSolidaria.Api.Migrations
{
    /// <summary>
    /// Carga de dados de teste: 10 geladeiras em bairros de Sao Paulo e 24 doacoes.
    /// As datas sao relativas a SYSDATE, entao o cenario e o mesmo em qualquer dia
    /// em que a migration for aplicada. Nomes e telefones sao ficticios.
    /// </summary>
    public partial class DadosIniciais : Migration
    {
        // Nome, endereco, bairro, capacidade (kg), status, instalada ha N dias, responsavel, telefone
        private static readonly (string Nome, string Endereco, string Bairro, int Capacidade, string Status, int DiasInstalada, string Responsavel, string Telefone)[] Geladeiras =
        [
            ("Geladeira Solidária Paraisópolis", "Rua Ernest Renan, 1200", "Paraisópolis", 80, "ATIVA", 420, "Associação de Moradores de Paraisópolis", "(11) 98810-1001"),
            ("Geladeira Solidária Heliópolis", "Estrada das Lágrimas, 2300", "Heliópolis", 60, "ATIVA", 380, "UNAS Heliópolis", "(11) 98810-1002"),
            ("Geladeira Solidária Capão Redondo", "Rua Comendador Antunes dos Santos, 450", "Capão Redondo", 50, "ATIVA", 300, "Coletivo Capão Solidário", "(11) 98810-1003"),
            ("Geladeira Solidária Itaquera", "Avenida Itaquera, 8900", "Itaquera", 100, "ATIVA", 260, "Paróquia São Francisco", "(11) 98810-1004"),
            ("Geladeira Solidária Jardim Ângela", "Estrada do M'Boi Mirim, 5100", "Jardim Ângela", 70, "ATIVA", 240, "Centro Comunitário Jardim Ângela", "(11) 98810-1005"),
            ("Geladeira Solidária Cidade Tiradentes", "Avenida dos Metalúrgicos, 1500", "Cidade Tiradentes", 80, "ATIVA", 200, "Instituto Tiradentes Vive", "(11) 98810-1006"),
            ("Geladeira Solidária Brasilândia", "Avenida Deputado Cantídio Sampaio, 3200", "Brasilândia", 60, "MANUTENCAO", 180, "Associação Brasilândia Unida", "(11) 98810-1007"),
            ("Geladeira Solidária Grajaú", "Avenida Belmira Marin, 2700", "Grajaú", 80, "ATIVA", 150, "Coletivo Grajaú em Ação", "(11) 98810-1008"),
            ("Geladeira Solidária Sé", "Praça da Sé, 100", "Sé", 110, "ATIVA", 120, "Rede Centro Solidário", "(11) 98810-1009"),
            ("Geladeira Solidária Jaraguá", "Estrada Turística do Jaraguá, 400", "Jaraguá", 50, "ATIVA", 5, "Coletivo Jaraguá", "(11) 98810-1010")
        ];

        // Geladeira, descricao, categoria, kg, vence em N dias, doador, status, doada ha N dias
        private static readonly (string Geladeira, string Descricao, string Categoria, decimal Kg, int VenceEm, string Doador, string Status, int DoadaHa)[] Doacoes =
        [
            ("Paraisópolis", "Marmitas de arroz, feijão e frango", "MARMITAS", 8, 1, "Restaurante Sabor da Vila", "DISPONIVEL", 0),
            ("Paraisópolis", "Bananas e maçãs", "HORTIFRUTI", 12, 5, "Sacolão Morumbi", "DISPONIVEL", 1),
            ("Paraisópolis", "Pães franceses do dia", "PAES", 4, 1, "Padaria Estrela", "DISPONIVEL", 0),
            ("Paraisópolis", "Leite integral (caixas de 1 L)", "LATICINIOS", 6, 20, null, "DISPONIVEL", 2),
            ("Heliópolis", "Arroz e feijão (pacotes de 1 kg)", "MERCEARIA", 30, 180, "Mercado Bom Preço", "DISPONIVEL", 3),
            ("Heliópolis", "Iogurtes", "LATICINIOS", 5, 3, null, "DISPONIVEL", 1),
            ("Heliópolis", "Legumes variados", "HORTIFRUTI", 20, 6, "Feira da Estrada das Lágrimas", "DISPONIVEL", 1),
            ("Capão Redondo", "Marmitas de macarrão", "MARMITAS", 7, 2, "Cozinha Solidária Capão", "DISPONIVEL", 0),
            ("Capão Redondo", "Suco de laranja natural", "BEBIDAS", 5, 4, null, "DISPONIVEL", 1),
            ("Itaquera", "Carne moída congelada", "CARNES", 10, 3, "Açougue Itaquera", "DISPONIVEL", 1),
            ("Itaquera", "Maçãs", "HORTIFRUTI", 10, 7, "Sacolão Itaquera", "DISPONIVEL", 2),
            ("Itaquera", "Queijo muçarela", "LATICINIOS", 3, 10, null, "RETIRADA", 6),
            ("Jardim Ângela", "Pão de forma", "PAES", 5, 5, "Padaria Ângela", "DISPONIVEL", 1),
            ("Jardim Ângela", "Frango assado", "CARNES", 8, 1, "Restaurante do Zé", "DISPONIVEL", 0),
            ("Jardim Ângela", "Verduras folhosas", "HORTIFRUTI", 12, 2, "Horta Comunitária M'Boi", "DISPONIVEL", 1),
            ("Cidade Tiradentes", "Marmitas de feijoada", "MARMITAS", 10, 2, "Cozinha da Dona Cida", "DISPONIVEL", 0),
            ("Brasilândia", "Leite em pó", "MERCEARIA", 4, 200, null, "RETIRADA", 20),
            ("Brasilândia", "Requeijão", "LATICINIOS", 2, -5, null, "DESCARTADA", 15),
            ("Grajaú", "Tomates e cebolas", "HORTIFRUTI", 15, 6, "Feira do Grajaú", "DISPONIVEL", 1),
            ("Sé", "Sanduíches naturais", "MARMITAS", 6, 1, "Café Central", "DISPONIVEL", 0),
            ("Sé", "Água mineral (garrafas)", "BEBIDAS", 18, 300, "Rede Centro Solidário", "DISPONIVEL", 4),
            ("Sé", "Bolos caseiros", "PAES", 4, 3, null, "DISPONIVEL", 1),
            ("Sé", "Biscoitos e bolachas", "MERCEARIA", 12, 90, "Mercado Central", "DISPONIVEL", 2),
            ("Sé", "Iogurte de morango", "LATICINIOS", 3, 8, null, "RETIRADA", 5)
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var g in Geladeiras)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO T_GSNET_GELADEIRA
                        (NOME, ENDERECO, BAIRRO, CAPACIDADE_KG, STATUS, DT_INSTALACAO, RESPONSAVEL, TELEFONE_RESPONSAVEL, DT_CADASTRO)
                    VALUES
                        ({Texto(g.Nome)}, {Texto(g.Endereco)}, {Texto(g.Bairro)}, {g.Capacidade}, '{g.Status}',
                         TRUNC(SYSDATE) - {g.DiasInstalada}, {Texto(g.Responsavel)}, {Texto(g.Telefone)}, TRUNC(SYSDATE) - {g.DiasInstalada})
                    """);
            }

            foreach (var d in Doacoes)
            {
                // A geladeira e encontrada pelo bairro, sem depender do ID gerado pelo banco.
                migrationBuilder.Sql($"""
                    INSERT INTO T_GSNET_DOACAO
                        (ID_GELADEIRA, DESCRICAO, CATEGORIA, QUANTIDADE_KG, DT_VALIDADE, NOME_DOADOR, STATUS, DT_DOACAO)
                    SELECT ID_GELADEIRA, {Texto(d.Descricao)}, '{d.Categoria}', {d.Kg.ToString(System.Globalization.CultureInfo.InvariantCulture)},
                           TRUNC(SYSDATE) + {d.VenceEm}, {Texto(d.Doador)}, '{d.Status}', SYSDATE - {d.DoadaHa}
                      FROM T_GSNET_GELADEIRA
                     WHERE BAIRRO = {Texto(d.Geladeira)}
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var nomes = string.Join(", ", Geladeiras.Select(g => Texto(g.Nome)));

            migrationBuilder.Sql($"""
                DELETE FROM T_GSNET_DOACAO
                 WHERE ID_GELADEIRA IN (SELECT ID_GELADEIRA FROM T_GSNET_GELADEIRA WHERE NOME IN ({nomes}))
                """);
            migrationBuilder.Sql($"DELETE FROM T_GSNET_GELADEIRA WHERE NOME IN ({nomes})");
        }

        /// <summary>Literal NVARCHAR2 com aspas simples escapadas, ou NULL.</summary>
        private static string Texto(string valor) =>
            valor is null ? "NULL" : $"N'{valor.Replace("'", "''")}'";
    }
}
