using System;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // MENU CONSULTAS E LISTAGENS
    // =====================================================
    // Classe estática responsável pelas consultas e listagens
    // de informação do sistema RNCCI.
    public static class ConsultasListagens
    {
        // Menu principal de consultas
        public static void MostrarMenu()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Consultas e Listagens ===");
                Console.WriteLine("1 - Listar Doentes por Unidade");
                Console.WriteLine("2 - Listar Doentes por Tipologia de Resposta");
                Console.WriteLine("3 - Listar Camas Disponíveis por Unidade");
                Console.WriteLine("4 - Listagem Geral da RNCCI");
                Console.WriteLine("0 - Voltar ao Menu Principal");
                Console.Write("\nOpção: ");

                string input = Console.ReadLine();

                // Validação da opção introduzida
                if (!int.TryParse(input, out opcao))
                {
                    Console.WriteLine("Opção inválida. Prima qualquer tecla para voltar ao menu.");
                    Console.ReadKey();
                    opcao = -1;
                    continue;
                }

                try
                {
                    switch (opcao)
                    {
                        case 1: ListarDoentesPorUnidade(); break;
                        case 2: ListarDoentesPorTipologia(); break;
                        case 3: ListarCamasDisponiveis(); break;
                        case 4: ListagemGeral(); break;
                    }
                }
                catch (Exception ex)
                {
                    // Tratamento genérico de erros
                    Console.WriteLine($"Erro: {ex.Message}");
                    Console.ReadKey();
                }

            } while (opcao != 0);
        }

        // =====================================================
        // 1 - LISTAR DOENTES POR UNIDADE
        // =====================================================
        private static void ListarDoentesPorUnidade()
        {
            Console.Clear();
            Console.WriteLine("=== Doentes por Unidade ===");

            // Obtém todas as unidades registadas
            var unidades = GestaoDados.ObterUnidades().ToList();
            if (!unidades.Any())
            {
                Console.WriteLine("Não existem unidades registadas.");
                Console.ReadKey();
                return;
            }

            // Percorre cada unidade e lista os respetivos doentes
            foreach (var u in unidades)
            {
                Console.WriteLine($"\nUnidade: {u.Nome} ({u.GetTipologia()})");

                // Agrupa por NIF para evitar listagens duplicadas
                // (garante que cada doente aparece apenas uma vez)
                var doentes = u.ConsultarDoentes()
                    .GroupBy(d => d.NIF)
                    .Select(g => g.First())
                    .ToList();

                if (!doentes.Any())
                {
                    Console.WriteLine("  Sem doentes internados.");
                }
                else
                {
                    foreach (var d in doentes)
                        Console.WriteLine($"  - {d.Nome} | NIF: {d.NIF}");
                }
            }

            Console.ReadKey();
        }

        // =====================================================
        // 2 - LISTAR DOENTES POR TIPOLOGIA
        // =====================================================
        private static void ListarDoentesPorTipologia()
        {
            Console.Clear();
            Console.WriteLine("=== Doentes por Tipologia ===");

            // Percorre todas as tipologias existentes
            foreach (var tip in GestaoDados.Tipologias)
            {
                Console.WriteLine($"\nTipologia: {tip}");

                // Filtra doentes de acordo com a tipologia necessária
                var doentes = GestaoDados.ObterDoentes()
                    .Where(d => d.TipologiaNecessaria == tip)
                    .ToList();

                if (!doentes.Any())
                {
                    Console.WriteLine("  Nenhum doente.");
                }
                else
                {
                    foreach (var d in doentes)
                        Console.WriteLine($"  - {d.Nome} | NIF: {d.NIF}");
                }
            }

            Console.ReadKey();
        }

        // =====================================================
        // 3 - LISTAR CAMAS DISPONÍVEIS POR UNIDADE
        // =====================================================
        private static void ListarCamasDisponiveis()
        {
            Console.Clear();
            Console.WriteLine("=== Camas Disponíveis por Unidade ===");

            var unidades = GestaoDados.ObterUnidades().ToList();
            if (!unidades.Any())
            {
                Console.WriteLine("Não existem unidades registadas.");
            }
            else
            {
                // Mostra, para cada unidade, o número de camas livres
                foreach (var u in unidades)
                {
                    Console.WriteLine(
                        $"Unidade: {u.Nome} | Tipologia: {u.GetTipologia()} | Camas Livres: {u.CamasDisponiveis()}");
                }
            }

            Console.ReadKey();
        }

        // =====================================================
        // 4 - LISTAGEM GERAL DA RNCCI
        // =====================================================
        private static void ListagemGeral()
        {
            Console.Clear();
            Console.WriteLine("=== Listagem Geral da RNCCI ===");

            var unidades = GestaoDados.ObterUnidades().ToList();
            var doentes = GestaoDados.ObterDoentes().ToList();

            // Listagem de unidades
            Console.WriteLine("\n--- UNIDADES ---");
            if (!unidades.Any())
            {
                Console.WriteLine("Sem unidades registadas.");
            }
            else
            {
                foreach (var u in unidades)
                {
                    Console.WriteLine(
                        $"{u.Nome} | {u.GetTipologia()} | Zona: {u.Zona} | Camas Livres: {u.CamasDisponiveis()}");
                }
            }

            // Listagem de doentes
            Console.WriteLine("\n--- DOENTES ---");
            if (!doentes.Any())
            {
                Console.WriteLine("Sem doentes registados.");
            }
            else
            {
                foreach (var d in doentes)
                {
                    Console.WriteLine(
                        $"{d.Nome} | NIF: {d.NIF} | Tipologia: {d.TipologiaNecessaria} | Doença: {d.TipoDoenca}");
                }
            }

            Console.ReadKey();
        }
    }
}
