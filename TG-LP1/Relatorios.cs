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
                Console.WriteLine("1 - Relatório de Ocupação por Distrito");
                Console.WriteLine("2 - Relatório de Ocupação por Zona (Norte / Centro / Sul)");
                Console.WriteLine("3 - Relatório Global da RNCCI");
                Console.WriteLine("0 - Voltar ao Menu Principal");
                Console.Write("\nOpção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                    continue;

                try
                {
                    switch (opcao)
                    {
                        case 1: RelatorioPorDistrito(); break;
                        case 2: RelatorioPorZona(); break;
                        case 3: RelatorioGlobal(); break;
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
        // 1 - RELATÓRIO POR DISTRITO
        // =====================================================
        private static void RelatorioPorDistrito()
        {
            Console.Clear();
            Console.WriteLine("=== Relatório de Ocupação por Distrito ===\n");

            var distritos = GestaoDados.ObterUnidades()
                .Select(u => u.Distrito)
                .Distinct();

            foreach (var d in distritos)
            {
                var unidades = GestaoDados.ObterUnidades()
                    .Where(u => u.Distrito == d)
                    .ToList();

                int camasTotais = unidades.Sum(u => u.ConsultarDoentes().Count() + u.CamasDisponiveis());
                int camasOcupadas = unidades.Sum(u => u.ConsultarDoentes().Count());

                Console.WriteLine($"Distrito: {d}");
                Console.WriteLine($"  Unidades: {unidades.Count}");
                Console.WriteLine($"  Camas ocupadas: {camasOcupadas} / {camasTotais}");
                Console.WriteLine();
            }

            Console.ReadKey();
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
                    .Where(u => u.Zona == zona)
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
                Console.WriteLine($"{u.Nome} | Tipologia: {u.GetTipologia()} | Distrito: {u.Distrito} | Zona: {u.Zona}");
                Console.WriteLine($"  Camas ocupadas: {ocupadasU} / {camasU}");
            }

            Console.ReadKey();
        }
    }
}
