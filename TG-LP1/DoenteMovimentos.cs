using System;
using System.Collections.Generic;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // ENUM TIPO DE MOVIMENTO
    // =====================================================
    public enum TipoMovimento
    {
        Admissao,
        Alta,
        Transferencia
    }

    // =====================================================
    // CLASSE MOVIMENTO
    // =====================================================
    public class MovimentoDoente
    {
        public string NIFDoente { get; }
        public string Unidade { get; }
        public int? Cama { get; }
        public TipoMovimento Tipo { get; }
        public DateTime Data { get; }

        public MovimentoDoente(string nif, string unidade, int? cama, TipoMovimento tipo)
        {
            NIFDoente = nif;
            Unidade = unidade;
            Cama = cama;
            Tipo = tipo;
            Data = DateTime.Now;
        }

        public override string ToString()
        {
            string camaTxt = Cama.HasValue ? $"Cama {Cama}" : "Sem cama";
            return $"{Data:dd/MM/yyyy HH:mm} | {Tipo} | {Unidade} | {camaTxt}";
        }
    }

    // =====================================================
    // GESTÃO DE MOVIMENTOS
    // =====================================================
    public static class GestaoMovimentos
    {
        private static readonly List<MovimentoDoente> movimentos = new List<MovimentoDoente>();


        public static void Registar(MovimentoDoente mov)
        {
            movimentos.Add(mov);
        }

        public static IEnumerable<MovimentoDoente> PorDoente(string nif)
        {
            return movimentos.Where(m => m.NIFDoente == nif);
        }

        public static IEnumerable<MovimentoDoente> PorCama(string unidade, int cama)
        {
            return movimentos.Where(m => m.Unidade == unidade && m.Cama == cama);
        }
    }

    // =====================================================
    // MENU DOENTES E MOVIMENTOS
    // =====================================================
    public static class DoentesMovimentos
    {
        public static void MostrarMenu()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Doentes e Movimentos ===");
                Console.WriteLine("1 - Registar Admissão de Doente");
                Console.WriteLine("2 - Registar Alta Médica");
                Console.WriteLine("3 - Transferir Doente entre Unidades");
                Console.WriteLine("4 - Extrato de Movimentos por Doente");
                Console.WriteLine("5 - Extrato de Movimentos por Cama");
                Console.WriteLine("0 - Voltar ao Menu Principal");
                Console.Write("\nOpção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                    continue;

                try
                {
                    switch (opcao)
                    {
                        case 1: RegistarAdmissao(); break;
                        case 2: RegistarAlta(); break;
                        case 3: TransferirDoente(); break;
                        case 4: ExtratoPorDoente(); break;
                        case 5: ExtratoPorCama(); break;
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
        // 1 - ADMISSÃO
        // =====================================================
        private static void RegistarAdmissao()
        {
            Console.Clear();
            Console.WriteLine("=== Registar Admissão ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();

            Doente doente = GestaoDados.ObterDoentePorNIF(nif);
            if (doente == null)
                throw new Exception("Doente não encontrado.");

            List<Unidade> unidadesDisponiveis = GestaoDados.ObterUnidades()
                .Where(unidade =>
                    unidade.GetTipologia() == doente.TipologiaNecessaria &&
                    unidade.TemCamaDisponivel())
                .ToList();

            if (!unidadesDisponiveis.Any())
                throw new Exception("Não existem unidades disponíveis.");

            for (int i = 0; i < unidadesDisponiveis.Count; i++)
                Console.WriteLine($"{i + 1} - {unidadesDisponiveis[i].Nome}");

            Console.Write("Escolha a unidade: ");
            if (!int.TryParse(Console.ReadLine(), out int idx) ||
                idx < 1 || idx > unidadesDisponiveis.Count)
                throw new Exception("Opção inválida.");

            Unidade unidadeEscolhida = unidadesDisponiveis[idx - 1];
            int cama = unidadeEscolhida.AdmitirDoente(doente);

            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeEscolhida.Nome, cama, TipoMovimento.Admissao));

            Console.WriteLine($"Doente admitido na unidade {unidadeEscolhida.Nome}, cama {cama}");
            Console.ReadKey();
        }

        // =====================================================
        // 2 - ALTA
        // =====================================================
        private static void RegistarAlta()
        {
            Console.Clear();
            Console.WriteLine("=== Registar Alta ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();

            Unidade unidadeInternamento = GestaoDados.ObterUnidades()
                .FirstOrDefault(u => u.ContemDoente(nif));

            if (unidadeInternamento == null)
                throw new Exception("Doente não está internado.");

            int? cama = unidadeInternamento.LibertarCamaDoente(nif);

            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeInternamento.Nome, cama, TipoMovimento.Alta));

            Console.WriteLine("Alta médica registada com sucesso.");
            Console.ReadKey();
        }

        // =====================================================
        // 3 - TRANSFERÊNCIA
        // =====================================================
        private static void TransferirDoente()
        {
            Console.Clear();
            Console.WriteLine("=== Transferir Doente ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();

            Doente doente = GestaoDados.ObterDoentePorNIF(nif);
            if (doente == null)
                throw new Exception("Doente não encontrado.");

            Unidade unidadeOrigem = GestaoDados.ObterUnidades()
                .FirstOrDefault(u => u.ContemDoente(nif));

            if (unidadeOrigem == null)
                throw new Exception("Doente não está internado.");

            List<Unidade> unidadesDestino = GestaoDados.ObterUnidades()
                .Where(u =>
                    u != unidadeOrigem &&
                    u.GetTipologia() == doente.TipologiaNecessaria &&
                    u.TemCamaDisponivel())
                .ToList();

            if (!unidadesDestino.Any())
                throw new Exception("Não existem unidades destino disponíveis.");

            for (int i = 0; i < unidadesDestino.Count; i++)
                Console.WriteLine($"{i + 1} - {unidadesDestino[i].Nome}");

            Console.Write("Escolha a unidade destino: ");
            if (!int.TryParse(Console.ReadLine(), out int idx) ||
                idx < 1 || idx > unidadesDestino.Count)
                throw new Exception("Opção inválida.");

            int? camaOrigem = unidadeOrigem.LibertarCamaDoente(nif);
            Unidade unidadeDestino = unidadesDestino[idx - 1];
            int camaDestino = unidadeDestino.AdmitirDoente(doente);

            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeOrigem.Nome, camaOrigem, TipoMovimento.Transferencia));

            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeDestino.Nome, camaDestino, TipoMovimento.Transferencia));

            Console.WriteLine("Transferência realizada com sucesso.");
            Console.ReadKey();
        }

        // =====================================================
        // 4 - EXTRATO POR DOENTE
        // =====================================================
        private static void ExtratoPorDoente()
        {
            Console.Clear();
            Console.WriteLine("=== Extrato de Movimentos por Doente ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();

            var lista = GestaoMovimentos.PorDoente(nif).ToList();

            if (!lista.Any())
                Console.WriteLine("Sem movimentos registados.");
            else
                lista.ForEach(m => Console.WriteLine(m));

            Console.ReadKey();
        }

        // =====================================================
        // 5 - EXTRATO POR CAMA
        // =====================================================
        private static void ExtratoPorCama()
        {
            Console.Clear();
            Console.WriteLine("=== Extrato de Movimentos por Cama ===");

            Console.Write("Nome da unidade: ");
            string unidade = Console.ReadLine();

            Console.Write("Número da cama: ");
            if (!int.TryParse(Console.ReadLine(), out int cama))
                throw new Exception("Número de cama inválido.");

            var lista = GestaoMovimentos.PorCama(unidade, cama).ToList();

            if (!lista.Any())
                Console.WriteLine("Sem movimentos para esta cama.");
            else
                lista.ForEach(m => Console.WriteLine(m));

            Console.ReadKey();
        }
    }
}
