using System;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // CLASSE RELATÓRIOS
    // =====================================================
    // Classe estática responsável por mostrar o menu
    // de relatórios e gerar os vários tipos de relatórios
    public static class Relatorios
    {
        // =====================================================
        // MENU DE RELATÓRIOS
        // =====================================================
        // Apresenta o menu e gere a escolha do utilizador
        public static void MostrarMenu()
        {
            int opcao;

            do
            {
                Console.Clear();
                Console.WriteLine("=== Relatórios ===");
                Console.WriteLine("1 - Relatório de Ocupação por Zona (Norte / Centro / Sul)");
                Console.WriteLine("2 - Relatório Global da RNCCI");
                Console.WriteLine("0 - Voltar ao Menu Principal");
                Console.Write("\nOpção: ");

                // Lê a opção introduzida pelo utilizador
                string input = Console.ReadLine();

                // Validação da opção (garante que é um número inteiro)
                if (!int.TryParse(input, out opcao))
                {
                    Console.WriteLine("Opção inválida. Prima qualquer tecla para voltar ao menu.");
                    Console.ReadKey();
                    opcao = -1;
                    continue;
                }

                try
                {
                    // Encaminha para o relatório correspondente
                    switch (opcao)
                    {
                        case 1:
                            RelatorioPorZona();
                            break;

                        case 2:
                            RelatorioGlobal();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Captura erros inesperados durante a execução
                    Console.WriteLine($"Erro: {ex.Message}");
                    Console.ReadKey();
                }

            } while (opcao != 0); // Sai quando o utilizador escolhe 0
        }

        // =====================================================
        // RELATÓRIO DE OCUPAÇÃO POR ZONA
        // =====================================================
        // Apresenta um resumo da ocupação das unidades
        // agrupadas por zona (Norte, Centro, Sul)
        private static void RelatorioPorZona()
        {
            Console.Clear();
            Console.WriteLine("=== Relatório de Ocupação por Zona ===\n");

            // Percorre todas as zonas existentes no sistema
            foreach (var zona in GestaoDados.Zonas)
            {
                // Obtém as unidades pertencentes à zona atual
                var unidades = GestaoDados.ObterUnidades()
                    .Where(u => string.Equals(u.Zona, zona, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Calcula o total de camas (ocupadas + disponíveis)
                int camasTotais = unidades.Sum(u =>
                    u.ConsultarDoentes().Count() + u.CamasDisponiveis());

                // Calcula o total de camas ocupadas
                int camasOcupadas = unidades.Sum(u =>
                    u.ConsultarDoentes().Count());

                // Apresenta o resumo da zona
                Console.WriteLine($"Zona: {zona}");
                Console.WriteLine($"  Unidades: {unidades.Count}");
                Console.WriteLine($"  Camas ocupadas: {camasOcupadas} / {camasTotais}");
                Console.WriteLine();
            }

            // Pausa antes de voltar ao menu
            Console.ReadKey();
        }

        // =====================================================
        // RELATÓRIO GLOBAL DA RNCCI
        // =====================================================
        // Apresenta um resumo geral do sistema, incluindo
        // todas as unidades, doentes e ocupação de camas
        private static void RelatorioGlobal()
        {
            Console.Clear();
            Console.WriteLine("=== Relatório Global da RNCCI ===\n");

            // Obtém todas as unidades e doentes registados
            var unidades = GestaoDados.ObterUnidades().ToList();
            var doentes = GestaoDados.ObterDoentes().ToList();

            // Calcula o total de camas existentes
            int camasTotais = unidades.Sum(u =>
                u.ConsultarDoentes().Count() + u.CamasDisponiveis());

            // Calcula o total de camas ocupadas
            int camasOcupadas = unidades.Sum(u =>
                u.ConsultarDoentes().Count());

            // Apresenta o resumo global
            Console.WriteLine($"Total de Unidades: {unidades.Count}");
            Console.WriteLine($"Total de Doentes: {doentes.Count}");
            Console.WriteLine($"Camas ocupadas: {camasOcupadas} / {camasTotais}\n");

            Console.WriteLine("Unidades detalhadas:");

            // Apresenta o detalhe de cada unidade
            foreach (var u in unidades)
            {
                int camasU = u.ConsultarDoentes().Count() + u.CamasDisponiveis();
                int ocupadasU = u.ConsultarDoentes().Count();

                Console.WriteLine($"{u.Nome} | Tipologia: {u.GetTipologia()} | Zona: {u.Zona}");
                Console.WriteLine($"  Camas ocupadas: {ocupadasU} / {camasU}");
            }

            // Pausa antes de voltar ao menu
            Console.ReadKey();
        }
    }
}
