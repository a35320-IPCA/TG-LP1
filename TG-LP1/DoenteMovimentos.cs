using System;
using System.Collections.Generic;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // ENUM TIPO DE MOVIMENTO
    // =====================================================
    // Enumeração que representa os tipos possíveis
    // de movimentos de um doente no sistema.
    public enum TipoMovimento
    {
        Admissao,
        Alta,
        Transferencia
    }

    // =====================================================
    // CLASSE MOVIMENTO DOENTE
    // =====================================================
    // Representa um único registo de movimento de um doente,
    // incluindo a unidade, a cama, o tipo de movimento e a data.
    public class MovimentoDoente
    {
        public string NIFDoente { get; }
        public string Unidade { get; }
        public int? Cama { get; }
        public TipoMovimento Tipo { get; }
        public DateTime Data { get; }

        // Construtor do movimento
        public MovimentoDoente(string nif, string unidade, int? cama, TipoMovimento tipo)
        {
            NIFDoente = nif;
            Unidade = unidade;
            Cama = cama;
            Tipo = tipo;
            Data = DateTime.Now; // Data/hora automática do registo
        }

        // Representação textual do movimento (para listagens)
        public override string ToString()
        {
            // Se não existir cama (ex.: EDCCI ou situações especiais)
            string camaTxt = Cama.HasValue ? $"Cama {Cama}" : "Sem cama";
            return $"{Data:dd/MM/yyyy HH:mm} | {Tipo} | {Unidade} | {camaTxt}";
        }
    }

    // =====================================================
    // GESTÃO DE MOVIMENTOS
    // =====================================================
    // Classe estática responsável por manter o histórico
    // de todos os movimentos registados no sistema.
    public static class GestaoMovimentos
    {
        // Lista interna de movimentos (append-only)
        private static readonly List<MovimentoDoente> movimentos = new List<MovimentoDoente>();

        // Regista um novo movimento no histórico
        public static void Registar(MovimentoDoente mov)
        {
            movimentos.Add(mov);
        }

        // Obtém o histórico completo de movimentos de um doente
        public static IEnumerable<MovimentoDoente> PorDoente(string nif)
        {
            return movimentos.Where(m => m.NIFDoente == nif);
        }

        // Obtém o histórico de movimentos associados a uma cama específica
        public static IEnumerable<MovimentoDoente> PorCama(string unidade, int cama)
        {
            return movimentos.Where(m => m.Unidade == unidade && m.Cama == cama);
        }
    }

    // =====================================================
    // MENU DOENTES E MOVIMENTOS
    // =====================================================
    // Classe responsável pela interação com o utilizador
    // relativamente aos movimentos dos doentes.
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

                string input = Console.ReadLine();

                // Validação da opção
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
                        case 1: RegistarAdmissao(); break;
                        case 2: RegistarAlta(); break;
                        case 3: TransferirDoente(); break;
                        case 4: ExtratoPorDoente(); break;
                        case 5: ExtratoPorCama(); break;
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
        // 1 - REGISTAR ADMISSÃO
        // =====================================================
        private static void RegistarAdmissao()
        {
            Console.Clear();
            Console.WriteLine("=== Registar Admissão ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            // Obter doente pelo NIF
            Doente doente = GestaoDados.ObterDoentePorNIF(nif);
            if (doente == null)
                throw new Exception("Doente não encontrado.");

            // Verificação de internamento duplicado
            if (GestaoDados.ObterUnidades().Any(u => u.ContemDoente(nif)))
            {
                Console.WriteLine("Doente já se encontra internado.");
                Console.ReadKey();
                return;
            }

            // Filtra unidades compatíveis com a tipologia do doente
            // e que tenham camas disponíveis
            List<Unidade> unidadesDisponiveis = GestaoDados.ObterUnidades()
                .Where(u =>
                    u.GetTipologia() == doente.TipologiaNecessaria &&
                    u.TemCamaDisponivel())
                .ToList();

            if (!unidadesDisponiveis.Any())
                throw new Exception("Não existem unidades disponíveis.");

            // Listagem das unidades disponíveis
            for (int i = 0; i < unidadesDisponiveis.Count; i++)
                Console.WriteLine($"{i + 1} - {unidadesDisponiveis[i].Nome}");

            Console.Write("Escolha a unidade: ");
            string choice = Console.ReadLine();
            if (!int.TryParse(choice, out int idx) ||
                idx < 1 || idx > unidadesDisponiveis.Count)
                throw new Exception("Opção inválida.");

            Unidade unidadeEscolhida = unidadesDisponiveis[idx - 1];

            // Admissão centralizada garante que um doente só é internado uma vez
            int cama = GestaoDados.AdmitirDoenteEmUnidade(unidadeEscolhida, doente);

            // Registo do movimento de admissão
            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeEscolhida.Nome, cama, TipoMovimento.Admissao));

            Console.WriteLine($"Doente admitido na unidade {unidadeEscolhida.Nome}, cama {cama}");
            Console.ReadKey();
        }

        // =====================================================
        // 2 - REGISTAR ALTA
        // =====================================================
        private static void RegistarAlta()
        {
            Console.Clear();
            Console.WriteLine("=== Registar Alta ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            // Identifica a unidade onde o doente está internado
            Unidade unidadeInternamento = GestaoDados.ObterUnidades()
                .FirstOrDefault(u => u.ContemDoente(nif));

            if (unidadeInternamento == null)
                throw new Exception("Doente não está internado.");

            // Libertação da cama ocupada
            int? cama = unidadeInternamento.LibertarCamaDoente(nif);

            // Registo do movimento de alta
            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeInternamento.Nome, cama, TipoMovimento.Alta));

            Console.WriteLine("Alta médica registada com sucesso.");
            Console.ReadKey();
        }

        // =====================================================
        // 3 - TRANSFERÊNCIA DE DOENTE
        // =====================================================
        private static void TransferirDoente()
        {
            Console.Clear();
            Console.WriteLine("=== Transferir Doente ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            Doente doente = GestaoDados.ObterDoentePorNIF(nif);
            if (doente == null)
                throw new Exception("Doente não encontrado.");

            // Unidade atual do doente
            Unidade unidadeOrigem = GestaoDados.ObterUnidades()
                .FirstOrDefault(u => u.ContemDoente(nif));

            if (unidadeOrigem == null)
                throw new Exception("Doente não está internado.");

            // Unidades destino compatíveis
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
            string choice = Console.ReadLine();
            if (!int.TryParse(choice, out int idx) ||
                idx < 1 || idx > unidadesDestino.Count)
                throw new Exception("Opção inválida.");

            // Libertação da cama de origem
            int? camaOrigem = unidadeOrigem.LibertarCamaDoente(nif);

            Unidade unidadeDestino = unidadesDestino[idx - 1];
            int camaDestino = GestaoDados.AdmitirDoenteEmUnidade(unidadeDestino, doente);

            // Registos de transferência (origem e destino)
            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeOrigem.Nome, camaOrigem, TipoMovimento.Transferencia));

            GestaoMovimentos.Registar(
                new MovimentoDoente(nif, unidadeDestino.Nome, camaDestino, TipoMovimento.Transferencia));

            Console.WriteLine("Transferência realizada com sucesso.");
            Console.ReadKey();
        }

        // =====================================================
        // 4 - EXTRATO DE MOVIMENTOS POR DOENTE
        // =====================================================
        private static void ExtratoPorDoente()
        {
            Console.Clear();
            Console.WriteLine("=== Extrato de Movimentos por Doente ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            var lista = GestaoMovimentos.PorDoente(nif).ToList();

            if (!lista.Any())
                Console.WriteLine("Sem movimentos registados.");
            else
                lista.ForEach(m => Console.WriteLine(m));

            Console.ReadKey();
        }

        // =====================================================
        // 5 - EXTRATO DE MOVIMENTOS POR CAMA
        // =====================================================
        private static void ExtratoPorCama()
        {
            Console.Clear();
            Console.WriteLine("=== Extrato de Movimentos por Cama ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            // Determina automaticamente a unidade do doente
            Unidade unidadeInternamento = GestaoDados.ObterUnidades()
                .FirstOrDefault(u => u.ContemDoente(nif));

            if (unidadeInternamento == null)
            {
                Console.WriteLine("Doente não está internado.");
                Console.ReadKey();
                return;
            }

            // Obtém o número da cama ocupada pelo doente
            int? camaNum = unidadeInternamento.ObterNumeroCamaDoente(nif);
            if (!camaNum.HasValue)
            {
                Console.WriteLine("Não foi possível determinar a cama.");
                Console.ReadKey();
                return;
            }

            var lista = GestaoMovimentos
                .PorCama(unidadeInternamento.Nome, camaNum.Value)
                .ToList();

            if (!lista.Any())
                Console.WriteLine("Sem movimentos para esta cama.");
            else
                lista.ForEach(m => Console.WriteLine(m));

            Console.ReadKey();
        }
    }
}
