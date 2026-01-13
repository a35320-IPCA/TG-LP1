using System;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // MENU RELATÓRIOS
    // =====================================================
    public static class Relatorios
    {
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

                string input = Console.ReadLine();
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
                        case 1: RelatorioPorZona(); break;
                        case 2: RelatorioGlobal(); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro: {ex.Message}");
                    Console.ReadKey();
                }

            } while (opcao != 0);
        }

        // =====================================================
        // 2 - RELATÓRIO POR ZONA
        // =====================================================
        private static void RelatorioPorZona()
        {
            Console.Clear();
            Console.WriteLine("=== Relatório de Ocupação por Zona ===\n");

            foreach (var zona in GestaoDados.Zonas)
            {
                var unidades = GestaoDados.ObterUnidades()
                    .Where(u => string.Equals(u.Zona, zona, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                int camasTotais = unidades.Sum(u => u.ConsultarDoentes().Count() + u.CamasDisponiveis());
                int camasOcupadas = unidades.Sum(u => u.ConsultarDoentes().Count());

                Console.WriteLine($"Zona: {zona}");
                Console.WriteLine($"  Unidades: {unidades.Count}");
                Console.WriteLine($"  Camas ocupadas: {camasOcupadas} / {camasTotais}");
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        // =====================================================
        // 3 - RELATÓRIO GLOBAL
        // =====================================================
        private static void RelatorioGlobal()
        {
            Console.Clear();
            Console.WriteLine("=== Relatório Global da RNCCI ===\n");

            var unidades = GestaoDados.ObterUnidades().ToList();
            var doentes = GestaoDados.ObterDoentes().ToList();

            int camasTotais = unidades.Sum(u => u.ConsultarDoentes().Count() + u.CamasDisponiveis());
            int camasOcupadas = unidades.Sum(u => u.ConsultarDoentes().Count());

            Console.WriteLine($"Total de Unidades: {unidades.Count}");
            Console.WriteLine($"Total de Doentes: {doentes.Count}");
            Console.WriteLine($"Camas ocupadas: {camasOcupadas} / {camasTotais}\n");

            Console.WriteLine("Unidades detalhadas:");
            foreach (var u in unidades)
            {
                int camasU = u.ConsultarDoentes().Count() + u.CamasDisponiveis();
                int ocupadasU = u.ConsultarDoentes().Count();
                Console.WriteLine($"{u.Nome} | Tipologia: {u.GetTipologia()} | Zona: {u.Zona}");
                Console.WriteLine($"  Camas ocupadas: {ocupadasU} / {camasU}");
            }

            Console.ReadKey();
        }
    }
}
