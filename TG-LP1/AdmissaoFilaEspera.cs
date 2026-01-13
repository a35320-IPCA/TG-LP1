using System;
using System.Collections.Generic;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // PEDIDO DE ADMISSÃO (FILA DE ESPERA)
    // =====================================================
    public class PedidoAdmissao
    {
        public Doente Doente { get; }
        public DateTime DataPedido { get; }
        public bool EntrouNaPortaria { get;  set; }

        public PedidoAdmissao(Doente doente)
        {
            Doente = doente;
            DataPedido = DateTime.Now;
            EntrouNaPortaria = false;
        }

        public override string ToString()
        {
            return $"{DataPedido:dd/MM/yyyy HH:mm} | {Doente.Nome} | NIF: {Doente.NIF} | Tipologia: {Doente.TipologiaNecessaria}";
        }
    }

    // =====================================================
    // GESTÃO DA FILA DE ESPERA
    // =====================================================
    public static class GestaoFilaEspera
    {
        private static readonly Queue<PedidoAdmissao> fila = new Queue<PedidoAdmissao>();

        public static void AdicionarPedido(Doente d)
        {
            if (fila.Any(p => p.Doente.NIF == d.NIF))
                throw new Exception("Doente já se encontra em fila de espera.");

            fila.Enqueue(new PedidoAdmissao(d));
        }

        public static PedidoAdmissao ProximoPedido()
        {
            return fila.Any() ? fila.Peek() : null;
        }

        public static void RemoverPedido()
        {
            if (fila.Any())
                fila.Dequeue();
        }

        // devolve snapshot para evitar exposição da coleção interna
        public static IEnumerable<PedidoAdmissao> Consultar()
        {
            return fila.ToList();
        }
    }

    // =====================================================
    // MENU ADMISSÃO E FILA DE ESPERA
    // =====================================================
    public static class AdmissaoFilaEspera
    {
        public static void MostrarMenu()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Admissões e Fila de Espera ===");
                Console.WriteLine("1 - Registar Pedido de Admissão (Fila de Espera)");
                Console.WriteLine("2 - Atribuição Automática de Unidade");
                Console.WriteLine("3 - Registar Entrada na Portaria");
                Console.WriteLine("4 - Gerir Lista de Visitantes Autorizados");
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
                        case 1: RegistarPedido(); break;
                        case 2: AtribuirUnidadeAutomaticamente(); break;
                        case 3: RegistarEntradaPortaria(); break;
                        case 4: GerirVisitantes(); break;
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
        // 1 - PEDIDO DE ADMISSÃO
        // =====================================================
        private static void RegistarPedido()
        {
            Console.Clear();
            Console.WriteLine("=== Pedido de Admissão ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            Doente d = GestaoDados.ObterDoentePorNIF(nif);
            if (d == null)
                throw new Exception("Doente não encontrado.");

            GestaoFilaEspera.AdicionarPedido(d);

            Console.WriteLine("Pedido registado em fila de espera.");
            Console.ReadKey();
        }

        // =====================================================
        // 2 - ATRIBUIÇÃO AUTOMÁTICA
        // =====================================================
        private static void AtribuirUnidadeAutomaticamente()
        {
            Console.Clear();
            Console.WriteLine("=== Atribuição Automática de Unidade ===");

            PedidoAdmissao pedido = GestaoFilaEspera.ProximoPedido();
            if (pedido == null)
                throw new Exception("Fila de espera vazia.");

            Doente d = pedido.Doente;

            Unidade unidade = GestaoDados.ObterUnidades()
                .FirstOrDefault(u =>
                    u.GetTipologia() == d.TipologiaNecessaria &&
                    u.GetTipologia() != "EDCCI" &&
                    u.TemCamaDisponivel());

            if (unidade == null)
                throw new Exception("Ainda não existem unidades disponíveis.");

            int cama = unidade.AdmitirDoente(d);

            GestaoMovimentos.Registar(
                new MovimentoDoente(d.NIF, unidade.Nome, cama, TipoMovimento.Admissao));

            GestaoFilaEspera.RemoverPedido();

            Console.WriteLine($"Doente admitido automaticamente em {unidade.Nome}, cama {cama}");
            Console.ReadKey();
        }

        // =====================================================
        // 3 - ENTRADA NA PORTARIA
        // =====================================================
        private static void RegistarEntradaPortaria()
        {
            Console.Clear();
            Console.WriteLine("=== Entrada na Portaria ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            PedidoAdmissao pedido = GestaoFilaEspera.Consultar()
                .FirstOrDefault(p => p.Doente.NIF == nif);

            if (pedido == null)
                throw new Exception("Doente não se encontra em fila de espera.");

            pedido.EntrouNaPortaria = true;

            Console.WriteLine("Entrada na portaria registada.");
            Console.ReadKey();
        }

        // =====================================================
        // 4 - VISITANTES AUTORIZADOS
        // =====================================================
        private static void GerirVisitantes()
        {
            Console.Clear();
            Console.WriteLine("=== Visitantes Autorizados ===");

            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nif))
                throw new ArgumentException("NIF inválido.");

            Doente d = GestaoDados.ObterDoentePorNIF(nif);
            if (d == null)
                throw new Exception("Doente não encontrado.");

            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine($"Doente: {d.Nome}");
                Console.WriteLine("1 - Adicionar visitante");
                Console.WriteLine("2 - Remover visitante");
                Console.WriteLine("3 - Consultar visitantes");
                Console.WriteLine("0 - Voltar");
                Console.Write("\nOpção: ");

                string in2 = Console.ReadLine();
                if (!int.TryParse(in2, out opcao))
                {
                    Console.WriteLine("Opção inválida. Prima qualquer tecla para voltar.");
                    Console.ReadKey();
                    opcao = -1;
                    continue;
                }

                switch (opcao)
                {
                    case 1:
                        Console.Write("Nome do visitante: ");
                        string nome = Console.ReadLine();
                        Console.Write("Relação: ");
                        string rel = Console.ReadLine();
                        d.AutorizarVisitante(nome, rel);
                        Console.WriteLine("Visitante autorizado.");
                        Console.ReadKey();
                        break;

                    case 2:
                        Console.Write("Nome do visitante: ");
                        string rem = Console.ReadLine();
                        d.RevogarVisitante(rem);
                        Console.WriteLine("Visitante removido.");
                        Console.ReadKey();
                        break;

                    case 3:
                        Console.WriteLine("Visitantes:");
                        foreach (var v in d.Autorizados)
                            Console.WriteLine($"- {v.Nome} ({v.Relacao})");
                        Console.ReadKey();
                        break;
                }

            } while (opcao != 0);
        }
    }
}
