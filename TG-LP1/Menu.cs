using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TG_LP1
{
    public class Menu
    {
        public static void MostrarMenuPrincipal()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("  SNS - Sistema de Gestão da RNCCI");
                Console.WriteLine("========================================");
                Console.WriteLine("1 - Gestão de Entidades (CRUD)");
                Console.WriteLine("2 - Doentes e Movimentos");
                Console.WriteLine("3 - Admissões e Fila de Espera");
                Console.WriteLine("4 - Consultas e Listagens");
                Console.WriteLine("5 - Relatórios");
                Console.WriteLine("6 - Estatísticas");
                Console.WriteLine("0 - Sair");
                Console.Write("\nEscolha uma opção: ");
                
                string input = Console.ReadLine();
                if (!int.TryParse(input, out opcao))
                {
                    Console.WriteLine("Opção inválida. Prima qualquer tecla para voltar ao menu.");
                    Console.ReadKey();
                    opcao = -1; // garantir que o loop continua
                    continue;
                }

                switch (opcao)
                {
                    case 1:
                        GestaoEntidades.MostrarMenu();
                        break;
                    case 2:
                        DoentesMovimentos.MostrarMenu();
                        break;
                    case 3:
                        AdmissaoFilaEspera.MostrarMenu();
                        break;
                    case 4:
                        ConsultasListagens.MostrarMenu();
                        break;
                    case 5:
                        Relatorios.MostrarMenu();
                        break;
                    case 6:
                        Estatisticas.MostrarMenu();
                        break;
                    case 0:
                        Console.WriteLine("\nA terminar a aplicação...");
                        break;
                    default:
                        Console.WriteLine("\nOpção inválida!");
                        Console.ReadKey();
                        break;
                }

            } while (opcao != 0);
        }
    }
}
