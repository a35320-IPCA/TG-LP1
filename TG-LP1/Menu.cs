// Menu.cs - Contém o menu principal da aplicação e delega para os submenus (Gestao, Movimentos, Relatorios, etc.)
using System;

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
                    // Entrada inválida: informa e repete o loop
                    Console.WriteLine("Opção inválida. Prima qualquer tecla para voltar ao menu.");
                    Console.ReadKey();
                    opcao = -1; // garantir que o loop continua
                    continue;
                }

                switch (opcao)
                {
                    case 1:
                        // Abre o menu de gestão de entidades (CRUD)
                        GestaoEntidades.MostrarMenu();
                        break;
                    case 2:
                        // Entrar no menu de movimentos e admissões
                        DoentesMovimentos.MostrarMenu();
                        break;
                    case 3:
                        // Menu específico para fila de espera e atribuições automáticas
                        AdmissaoFilaEspera.MostrarMenu();
                        break;
                    case 4:
                        // Menu de consultas/listagens (relatórios simples)
                        ConsultasListagens.MostrarMenu();
                        break;
                    case 5:
                        // Menu de relatórios agregados
                        Relatorios.MostrarMenu();
                        break;
                    case 6:
                        // Menu de estatísticas derivadas dos dados
                        Estatisticas.MostrarMenu();
                        break;
                    case 0:
                        // Sair da aplicação
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
