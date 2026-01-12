using System;
using System.Collections.Generic;
using System.Linq;

namespace TG_LP1
{
    public static class Estatisticas
    {
        public static void MostrarMenu()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Estatísticas de Visitas ===");
                Console.WriteLine("1 - Percentagem de Visitas por Tipo de Doença");
                Console.WriteLine("2 - Número de Visitas por Unidade da Rede");
                Console.WriteLine("0 - Voltar ao Menu Principal");
                Console.Write("\nEscolha uma opção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                    continue;

                try
                {
                    switch (opcao)
                    {
                        case 1:
                            PercentagemVisitasPorDoenca();
                            break;
                        case 2:
                            NumeroVisitasPorUnidade();
                            break;
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
        // 1 - Percentagem de visitas por tipo de doença
        // =====================================================
        private static void PercentagemVisitasPorDoenca()
        {
            Console.Clear();
            Console.WriteLine("=== Percentagem de Visitas por Tipo de Doença ===");

            var doentes = GestaoDados.ObterDoentes().ToList();

            if (!doentes.Any())
            {
                Console.WriteLine("Nenhum doente registado.");
                Console.ReadKey();
                return;
            }

            // criar um dicionário tipoDoenca -> nº visitas
            var visitasPorDoenca = new Dictionary<string, int>();

            foreach (var d in doentes)
            {
                int nVisitas = d.Autorizados.Count; // número de visitantes autorizados
                if (!visitasPorDoenca.ContainsKey(d.TipoDoenca))
                    visitasPorDoenca[d.TipoDoenca] = 0;
                visitasPorDoenca[d.TipoDoenca] += nVisitas;
            }

            int totalVisitas = visitasPorDoenca.Values.Sum();

            if (totalVisitas == 0)
            {
                Console.WriteLine("Não há visitas registadas.");
            }
            else
            {
                foreach (var kv in visitasPorDoenca)
                {
                    double percent = (kv.Value / (double)totalVisitas) * 100;
                    Console.WriteLine($"{kv.Key}: {kv.Value} visitas ({percent:F2}%)");
                }
            }

            Console.ReadKey();
        }

        // =====================================================
        // 2 - Número de visitas por unidade da rede
        // =====================================================
        private static void NumeroVisitasPorUnidade()
        {
            Console.Clear();
            Console.WriteLine("=== Número de Visitas por Unidade da Rede ===");

            var unidades = GestaoDados.ObterUnidades().ToList();

            if (!unidades.Any())
            {
                Console.WriteLine("Nenhuma unidade registada.");
                Console.ReadKey();
                return;
            }

            foreach (var u in unidades)
            {
                int nVisitas = 0;

                // percorrer doentes da unidade
                foreach (var doente in u.ConsultarDoentes())
                {
                    nVisitas += doente.Autorizados.Count;
                }

                Console.WriteLine($"Unidade {u.Nome} ({u.GetTipologia()}): {nVisitas} visitas");
            }

            Console.ReadKey();
        }
    }
}
