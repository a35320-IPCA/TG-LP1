// GestaoEntidades.cs - Definição das entidades do domínio (Doente, Unidade, Cama, Visitante)
// e o repositório em memória (GestaoDados) com as operações CRUD e regras básicas.
// =====================================================
using System;
using System.Collections.Generic;
using System.Linq;

namespace TG_LP1
{
    // =====================================================
    // EXCEÇÕES
    // =====================================================
    // Exceções específicas do domínio usadas para sinalizar erros claros na UI/menus.
    // Usar exceções específicas facilita distinguir erro de validação vs erro de sistema.
    public class EntidadeNaoEncontradaException : Exception
    {
        public EntidadeNaoEncontradaException(string msg) : base(msg) { }
    }

    public class DoenteInternadoException : Exception
    {
        public DoenteInternadoException(string msg) : base(msg) { }
    }

    public class UnidadeComDoentesException : Exception
    {
        public UnidadeComDoentesException(string msg) : base(msg) { }
    }

    // =====================================================
    // ABSTRAÇÃO: Pessoa
    // =====================================================
    // Pessoa: classe base simples; mantém apenas dados básicos (Nome e Idade).
    public abstract class Pessoa
    {
        public string Nome { get; protected set; }
        public int Idade { get; protected set; }

        protected Pessoa(string nome, int idade)
        {
            Nome = nome;
            Idade = idade;
        }
    }

    // =====================================================
    // ENTIDADES PRINCIPAIS
    // =====================================================
    // Visitante: dados mínimos para representar visitas autorizadas.
    public class Visitante
    {
        public string Nome { get; private set; }
        public string Relacao { get; private set; }

        public Visitante(string nome, string relacao)
        {
            Nome = nome;
            Relacao = relacao;
        }
    }

    // Representa um doente registado no sistema.
    // Contém informações pessoais e tipo de doença; visitas autorizadas são mantidas numa lista.
    public class Doente : Pessoa
    {
        // Numero: id interno sequencial usado apenas no runtime.
        // NIF: chave de negócio única — usada para identificar e evitar duplicações.
        // Identificadores 
        public int Numero { get; private set; }
        public string NIF { get; private set; }
        public string TipologiaNecessaria { get; private set; } // "UC","UMDR","ULDM","EDCCI"
        public string OrigemReferencia { get; private set; }
        public string TipoDoenca { get; private set; }
        private List<Visitante> _autorizados = new List<Visitante>();

        public IReadOnlyList<Visitante> Autorizados => _autorizados.AsReadOnly();

        public Doente(int numero, string nome, int idade, string nif, string tipologia, string origem, string tipoDoenca)
            : base(nome, idade)
        {
            Numero = numero;
            NIF = nif;
            TipologiaNecessaria = tipologia;
            OrigemReferencia = origem;
            TipoDoenca = tipoDoenca;
        }

        public void AutorizarVisitante(string nome, string relacao)
        {
            // Evita duplicação de visitantes pelo nome
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("Nome do visitante inválido.");
            if (!_autorizados.Any(v => v.Nome == nome))
            {
                _autorizados.Add(new Visitante(nome, relacao));
            }
        }

        public void RevogarVisitante(string nome)
        {
            // Remove todos os visitantes com o nome fornecido (simples correspondência por nome)
            _autorizados.RemoveAll(v => v.Nome == nome);
        }

        public void AtualizarDados(string nome, int idade, string tipoDoenca)
        {
            if (!string.IsNullOrWhiteSpace(nome)) Nome = nome;
            if (idade > 0) Idade = idade;
            if (!string.IsNullOrWhiteSpace(tipoDoenca)) TipoDoenca = tipoDoenca;
        }

        public void Mostrar()
        {
            Console.WriteLine($"Nº:{Numero} | Nome:{Nome} | Idade:{Idade} | NIF:{NIF} | Tipologia:{TipologiaNecessaria} | Doença:{TipoDoenca}");
        }
    }

    public class Cama
    {
        // Cama: pequena entidade de valor que guarda estado de ocupação e referência ao doente.
        public int Numero { get; private set; }
        public bool Ocupada { get; private set; }
        public Doente DoenteAtual { get; private set; }

        public Cama(int numero)
        {
            Numero = numero;
            Ocupada = false;
        }

        public void Ocupar(Doente d)
        {
            if (Ocupada) throw new Exception("Cama já ocupada.");
            DoenteAtual = d;
            Ocupada = true;
        }

        public void Libertar()
        {
            DoenteAtual = null;
            Ocupada = false;
        }
    }

    public abstract class Unidade
    {
        // Unidade: representa uma unidade da rede com uma coleção de camas.
        // Zona deve ser um dos valores definidos em GestaoDados.Zonas.
        public string Nome { get; protected set; }
        public string Zona { get; protected set; } // "Norte","Centro","Sul"
        protected List<Cama> Camas;

        protected Unidade(string nome, string zona, int totalCamas)
        {
            Nome = nome;
            Zona = zona;
            Camas = new List<Cama>();
            for (int i = 1; i <= totalCamas; i++)
            {
                Camas.Add(new Cama(i));
            }
        }

        public abstract string GetTipologia();

        public bool TemCamaDisponivel()
        {
            return Camas.Any(c => !c.Ocupada);
        }

        public int AdmitirDoente(Doente d)
        {
            // Admitir primeiro doente na primeira cama livre (estratégia simples "first fit")
            Cama cama = Camas.FirstOrDefault(c => !c.Ocupada);
            if (cama == null) throw new Exception("Sem camas livres.");
            cama.Ocupar(d);
            return cama.Numero;
        }
        // Nota: este método assume que o chamador já validou que o doente não está internado.
        // Use GestaoDados.AdmitirDoenteEmUnidade para uma verificação centralizada antes de admitir.

        public int? LibertarCamaDoente(string nif)
        {
            // Procura a cama ocupada pelo NIF e a liberta, devolvendo o número (ou null se não existir)
            Cama c = Camas.FirstOrDefault(x => x.Ocupada && x.DoenteAtual.NIF == nif);
            if (c == null) return null;
            int num = c.Numero;
            c.Libertar();
            return num;
        }

        // Devolve o número da cama onde o doente com o NIF se encontra, sem alterar o estado da cama
        public int? ObterNumeroCamaDoente(string nif)
        {
            Cama c = Camas.FirstOrDefault(x => x.Ocupada && x.DoenteAtual.NIF == nif);
            if (c == null) return null;
            return c.Numero;
        }

        public bool ContemDoente(string nif)
        {
            // True se alguma cama contém o doente com o NIF
            return Camas.Any(c => c.Ocupada && c.DoenteAtual.NIF == nif);
        }

        public IEnumerable<Doente> ConsultarDoentes()
        {
            foreach (Cama c in Camas)
            {
                if (c.Ocupada) yield return c.DoenteAtual;
            }
        }

        public int CamasDisponiveis()
        {
            return Camas.Count(c => !c.Ocupada);
        }
    }

    // Subclasses de Unidade mantêm apenas a tipologia; a lógica de camas é herdada.
    public class UC : Unidade
    {
        public UC(string nome, string zona, int camas) : base(nome, zona, camas) { }
        public override string GetTipologia() { return "UC"; }
    }

    public class UMDR : Unidade
    {
        public UMDR(string nome, string zona, int camas) : base(nome, zona, camas) { }
        public override string GetTipologia() { return "UMDR"; }
    }

    public class ULDM : Unidade
    {
        public ULDM(string nome, string zona, int camas) : base(nome, zona, camas) { }
        public override string GetTipologia() { return "ULDM"; }
    }

    public class EDCCI : Unidade
    {
        public EDCCI(string nome, string zona) : base(nome, zona, 0) { }
        public override string GetTipologia() { return "EDCCI"; }

        // equipa domiciliária não usa camas; regista atendimentos internamente se necessário
    }

    // =====================================================
    // INTERFACES (IGestao genérica)
    // =====================================================
    // IGestao<T> - contrato mínimo para gestores (Inserir/Atualizar/Remover/Consultar/Obter).
    // Permite trocar implementações (ex.: usar GestaoDados ou um mock em testes).
    public interface IGestao<T>
    {
        void Inserir(T item);
        void Atualizar(Func<T, bool> predicate, Action<T> atualizador);
        void Remover(Func<T, bool> predicate);
        IEnumerable<T> Consultar();
        T Obter(Func<T, bool> predicate);
    }

    // =====================================================
    // GESTAO CENTRAL (GestaoDados) - encapsula listas e operações CRUD
    // =====================================================
    // GestaoDados: repositório em memória. Se precisares de persistência, este é o local para ligar I/O.
    // Observação: não é thread-safe; em ambiente concorrente adicionar locking (ex.: lock por NIF).
    public static class GestaoDados
    {
        // arrays / listas
        public static readonly string[] Zonas = new string[] { "Norte", "Centro", "Sul" };

        private static readonly List<Doente> _doentes = new List<Doente>();
        private static readonly List<Unidade> _unidades = new List<Unidade>();
        private static readonly List<string> _tipologias = new List<string>() { "UC", "UMDR", "ULDM", "EDCCI" };
        private static int _proximoNumeroDoente = 1;

        // Exposição controlada (somente leitura)
        public static IReadOnlyList<string> Tipologias => _tipologias.AsReadOnly();
        public static IEnumerable<Doente> ObterDoentes() { return _doentes.AsReadOnly(); }
        public static IEnumerable<Unidade> ObterUnidades() { return _unidades.AsReadOnly(); }

        // =========================
        // DOENTES CRUD
        // =========================
        // Cria um novo doente e adiciona ao repositório em memória.
        // Validações importantes:
        // - NIF é obrigatório
        // - NIF deve ser único (lança exceção se já existir)
        public static Doente CriarDoente(string nome, int idade, string nif, string tipologia, string origem, string tipoDoenca)
        {
            if (string.IsNullOrWhiteSpace(nif)) throw new ArgumentException("NIF obrigatório.");
            if (_doentes.Any(x => x.NIF == nif)) throw new Exception("NIF já registado.");

            Doente d = new Doente(_proximoNumeroDoente, nome, idade, nif, tipologia, origem, tipoDoenca);
            _proximoNumeroDoente++;
            _doentes.Add(d);
            return d;
        }

        // ObterDoentePorNIF: procura linear em memória — suficiente para protótipo.
        // Para grandes volumes considerar dicionário indexado por NIF.
        public static Doente ObterDoentePorNIF(string nif)
        {
            return _doentes.FirstOrDefault(d => d.NIF == nif);
        }

        // Atualiza um doente encontrado por NIF aplicando a função 'atualizador'.
        // Lança EntidadeNaoEncontradaException se não existir.
        public static void AtualizarDoente(string nif, Action<Doente> atualizador)
        {
            Doente d = ObterDoentePorNIF(nif);
            if (d == null) throw new EntidadeNaoEncontradaException("Doente não encontrado.");
            atualizador(d);
        }

        // Remove doente por NIF, mas apenas se não estiver internado (invariante de integridade).
        public static void RemoverDoente(string nif)
        {
            Doente d = ObterDoentePorNIF(nif);
            if (d == null) throw new EntidadeNaoEncontradaException("Doente não encontrado.");

            Unidade u = _unidades.FirstOrDefault(x => x.ContemDoente(nif));
            if (u != null) throw new DoenteInternadoException("Doente internado. Fazer alta antes de remover.");

            _doentes.RemoveAll(x => x.NIF == nif);
        }

        // =========================
        // UNIDADES CRUD
        // =========================
        // Insere uma nova unidade (valida nome único)
        public static void InserirUnidade(Unidade u)
        {
            if (u == null) throw new ArgumentNullException(nameof(u));
            if (_unidades.Any(x => x.Nome == u.Nome)) throw new Exception("Unidade já existente.");
            _unidades.Add(u);
        }

        // ObterUnidadePorNome: procura exata por nome. Retorna null se não encontrada.
        public static Unidade ObterUnidadePorNome(string nome)
        {
            return _unidades.FirstOrDefault(u => u.Nome == nome);
        }

        // AtualizarUnidade: aplica atualizador na unidade encontrada.
        // Atenção: as propriedades de Unidade têm set protected, a UI recria a unidade ao atualizar.
        public static void AtualizarUnidade(string nome, Action<Unidade> atualizador)
        {
            Unidade u = ObterUnidadePorNome(nome);
            if (u == null) throw new EntidadeNaoEncontradaException("Unidade não encontrada.");
            atualizador(u);
        }

        // RemoverUnidade: só remove se não tiver doentes internados para manter integridade dos dados.
        public static void RemoverUnidade(string nome)
        {
            Unidade u = ObterUnidadePorNome(nome);
            if (u == null) throw new EntidadeNaoEncontradaException("Unidade não encontrada.");
            if (u.ConsultarDoentes().Any()) throw new UnidadeComDoentesException("Unidade tem doentes internados.");
            _unidades.RemoveAll(x => x.Nome == nome);
        }

        // Admitir doente numa unidade verificando primeiro se já está internado
        // Garante que um NIF só esteja internado numa unidade de cada vez.
        public static int AdmitirDoenteEmUnidade(Unidade u, Doente d)
        {
            if (u == null) throw new ArgumentNullException(nameof(u));
            if (d == null) throw new ArgumentNullException(nameof(d));

            // Verifica se o doente já está internado em qualquer unidade
            if (_unidades.Any(x => x.ContemDoente(d.NIF)))
                throw new Exception("Doente já internado. Não é possível admitir novamente.");

            return u.AdmitirDoente(d);
        }

        // =========================
        // TIPOLOGIAS
        // =========================
        // Insere uma nova tipologia (ex.: UC, UMDR). Valida que não esteja duplicada.
        public static void InserirTipologia(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) throw new ArgumentException("Tipologia inválida.");
            if (_tipologias.Contains(t)) throw new Exception("Tipologia já existe.");
            _tipologias.Add(t);
        }

        // Remove uma tipologia existente.
        public static void RemoverTipologia(string t)
        {
            if (!_tipologias.Contains(t)) throw new EntidadeNaoEncontradaException("Tipologia não encontrada.");
            _tipologias.RemoveAll(x => x == t);
        }
    }

    // =====================================================
    // MANAGERS (implementam IGestao<T>) - demonstram interfaces e polimorfismo
    // =====================================================
    // Classes adaptadoras que usam GestaoDados (simples wrappers) — útil para testes ou injeção futura.
    public class GestorDoentes : IGestao<Doente>
    {
        public void Inserir(Doente item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            // usa GestaoDados para persistir
            GestaoDados.CriarDoente(item.Nome, item.Idade, item.NIF, item.TipologiaNecessaria, item.OrigemReferencia, item.TipoDoenca);
        }

        public void Atualizar(Func<Doente, bool> predicate, Action<Doente> atualizador)
        {
            Doente d = GestaoDados.ObterDoentes().FirstOrDefault(predicate);
            if (d == null) throw new EntidadeNaoEncontradaException("Doente não encontrado.");
            atualizador(d);
        }

        public void Remover(Func<Doente, bool> predicate)
        {
            Doente d = GestaoDados.ObterDoentes().FirstOrDefault(predicate);
            if (d == null) throw new EntidadeNaoEncontradaException("Doente não encontrado.");
            GestaoDados.RemoverDoente(d.NIF);
        }

        public IEnumerable<Doente> Consultar()
        {
            return GestaoDados.ObterDoentes();
        }

        public Doente Obter(Func<Doente, bool> predicate)
        {
            return GestaoDados.ObterDoentes().FirstOrDefault(predicate);
        }
    }

    // Gestor para operações com unidades (wrapper sobre GestaoDados). Comentários evitam duplicar a lógica aqui.
    public class GestorUnidades : IGestao<Unidade>
    {
        public void Inserir(Unidade item)
        {
            GestaoDados.InserirUnidade(item);
        }

        public void Atualizar(Func<Unidade, bool> predicate, Action<Unidade> atualizador)
        {
            Unidade u = GestaoDados.ObterUnidades().FirstOrDefault(predicate);
            if (u == null) throw new EntidadeNaoEncontradaException("Unidade não encontrada.");
            atualizador(u);
        }

        public void Remover(Func<Unidade, bool> predicate)
        {
            Unidade u = GestaoDados.ObterUnidades().FirstOrDefault(predicate);
            if (u == null) throw new EntidadeNaoEncontradaException("Unidade não encontrada.");
            GestaoDados.RemoverUnidade(u.Nome);
        }

        public IEnumerable<Unidade> Consultar()
        {
            return GestaoDados.ObterUnidades();
        }

        public Unidade Obter(Func<Unidade, bool> predicate)
        {
            return GestaoDados.ObterUnidades().FirstOrDefault(predicate);
        }
    }

    // =====================================================
    // UI: GestaoEntidades — menus e prompts que usam GestaoDados
    // Nota: a UI faz validações básicas; regras de integridade mais fortes estão em GestaoDados.
    // =====================================================
    public static class GestaoEntidades
    {
        // UI/Console: menus e prompts. A lógica de domínio fica em GestaoDados.
        public static void MostrarMenu()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Gestão de Entidades (CRUD) ===");
                Console.WriteLine("1 - Doentes");
                Console.WriteLine("2 - Unidades");
                Console.WriteLine("3 - Tipologias");
                Console.WriteLine("0 - Voltar");
                Console.Write("\nEscolha uma opção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                {
                    Console.WriteLine("Opção inválida.");
                    Console.ReadKey();
                    continue;
                }

                switch (opcao)
                {
                    case 1:
                        MenuDoentes();
                        break;
                    case 2:
                        MenuUnidades();
                        break;
                    case 3:
                        MenuTipologias();
                        break;
                    case 0:
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        Console.ReadKey();
                        break;
                }

            } while (opcao != 0);
        }

        // ---------------------------
        // Menu Doentes
        // ---------------------------
        private static void MenuDoentes()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Gestão de Doentes ===");
                Console.WriteLine("1 - Inserir");
                Console.WriteLine("2 - Atualizar");
                Console.WriteLine("3 - Consultar");
                Console.WriteLine("4 - Eliminar");
                Console.WriteLine("0 - Sair");
                Console.Write("\nEscolha uma opção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                {
                    Console.WriteLine("Opção inválida.");
                    Console.ReadKey();
                    continue;
                }

                try
                {
                    switch (opcao)
                    {
                        case 1:
                            InserirDoente();
                            break;
                        case 2:
                            AtualizarDoente();
                            break;
                        case 3:
                            ConsultarDoentes();
                            break;
                        case 4:
                            EliminarDoente();
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Opção inválida.");
                            Console.ReadKey();
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

        private static void InserirDoente()
        {
            // Menu de inserção de doente: recolhe dados básicos, valida NIF/tipologia/zona e envia para GestaoDados
            Console.Clear();
            Console.WriteLine("=== Inserir Doente ===");
            // Nome e tipo de doença usam LerLetras para garantir apenas letras/espaços
            string nome = LerLetras("Nome: ");
            Console.Write("Idade: ");
            int idade = LerIntPositivo();
            // NIF: leitura manual com validação de vazio e duplicado, porque é a chave principal dos doentes
            string nif;
            while (true)
            {
                Console.Write("NIF: ");
                nif = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(nif))
                {
                    Console.WriteLine("NIF inválido. Tente novamente.");
                    continue;
                }
                if (GestaoDados.ObterDoentePorNIF(nif) != null)
                {
                    Console.WriteLine("NIF já registado. Introduza outro NIF.");
                    continue;
                }
                break;
            }

            // Escolha da tipologia a partir da lista centralizada em GestaoDados
            Console.WriteLine("Tipologias disponíveis:");
            for (int i = 0; i < GestaoDados.Tipologias.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {GestaoDados.Tipologias[i]}");
            }
            Console.Write("Escolha tipologia (número): ");
            int index = LerInt();
            string tipologia = (index >= 1 && index <= GestaoDados.Tipologias.Count) ? GestaoDados.Tipologias[index - 1] : GestaoDados.Tipologias.First();

            // Origem/zona usa array GestaoDados.Zonas para evitar strings mágicas dispersas
            string origem;
            while (true)
            {
                Console.WriteLine("Zonas:");
                for (int i = 0; i < GestaoDados.Zonas.Length; i++)
                {
                    Console.WriteLine($"{i + 1} - {GestaoDados.Zonas[i]}");
                }
                Console.Write("Escolha zona (número): ");
                int zonaindex = LerInt();
                if (zonaindex >= 1 && zonaindex <= GestaoDados.Zonas.Length)
                {
                    origem = GestaoDados.Zonas[zonaindex - 1];
                    break;
                }
                Console.WriteLine("Opção inválida. Escolha 1, 2 ou 3.");
            }

             string tipoDoenca = LerLetras("Tipo de doença: ");

            GestaoDados.CriarDoente(nome, idade, nif, tipologia, origem, tipoDoenca);
            Console.WriteLine("Doente inserido com sucesso.");
            Console.ReadKey();
        }

        private static void AtualizarDoente()
        {
            Console.Clear();
            Console.WriteLine("=== Atualizar Doente ===");
            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            // A procura é feita na lista em memória, usando o NIF como chave de negócio
            Doente d = GestaoDados.ObterDoentes().FirstOrDefault(x => x.NIF == nif);
            if (d == null)
            {
                Console.WriteLine("Doente não encontrado.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Deixe em branco para manter o valor atual.");
            string nome = LerLetras("Nome (atual: " + d.Nome + "): ", allowEmpty: true);
            Console.Write($"Idade (atual: {d.Idade}): ");
            int idade = LerIntAllowEmpty(d.Idade);
             string tipoDoenca = LerLetras($"Tipo de doença (atual: {d.TipoDoenca}): ", allowEmpty: true);

            GestaoDados.AtualizarDoente(nif, doente => doente.AtualizarDados(nome, idade, tipoDoenca));
            Console.WriteLine("Doente atualizado.");
            Console.ReadKey();
        }

        private static void ConsultarDoentes()
        {
            Console.Clear();
            Console.WriteLine("=== Lista de Doentes ===");
            var lista = GestaoDados.ObterDoentes().ToList();
            if (!lista.Any())
            {
                Console.WriteLine("Nenhum doente registado.");
            }
            else
            {
                // Usa o método Mostrar do próprio Doente para centralizar o formato de apresentação
                foreach (var d in lista)
                {
                    d.Mostrar();
                }
            }
            Console.ReadKey();
        }

        private static void EliminarDoente()
        {
            Console.Clear();
            Console.WriteLine("=== Eliminar Doente ===");
            Console.Write("NIF do doente: ");
            string nif = Console.ReadLine();
            try
            {
                // GestaoDados garante regra: não remover se estiver internado
                GestaoDados.RemoverDoente(nif);
                Console.WriteLine("Doente removido.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            Console.ReadKey();
        }

        // ---------------------------
        // Menu Unidades
        // ---------------------------
        private static void MenuUnidades()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Gestão de Unidades ===");
                Console.WriteLine("1 - Inserir");
                Console.WriteLine("2 - Atualizar");
                Console.WriteLine("3 - Consultar");
                Console.WriteLine("4 - Eliminar");
                Console.WriteLine("0 - Sair");
                Console.Write("\nEscolha uma opção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                {
                    Console.WriteLine("Opção inválida.");
                    Console.ReadKey();
                    continue;
                }

                try
                {
                    switch (opcao)
                    {
                        case 1:
                            InserirUnidade();
                            break;
                        case 2:
                            AtualizarUnidade();
                            break;
                        case 3:
                            ConsultarUnidades();
                            break;
                        case 4:
                            EliminarUnidade();
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Opção inválida.");
                            Console.ReadKey();
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

        private static void InserirUnidade()
        {
            Console.Clear();
            Console.WriteLine("=== Inserir Unidade ===");
            Console.Write("Nome: ");
            string nome = Console.ReadLine();

            // Escolha zona: obrigatória, apenas 1..N
            string zona;
            while (true)
            {
                Console.WriteLine("Zonas:");
                for (int i = 0; i < GestaoDados.Zonas.Length; i++)
                {
                    Console.WriteLine($"{i + 1} - {GestaoDados.Zonas[i]}");
                }
                Console.Write("Escolha zona (número): ");
                int zidx = LerInt();
                if (zidx >= 1 && zidx <= GestaoDados.Zonas.Length)
                {
                    zona = GestaoDados.Zonas[zidx - 1];
                    break;
                }
                Console.WriteLine("Opção inválida. Escolha 1, 2 ou 3.");
            }

            Console.WriteLine("Tipologias disponíveis:");
            for (int i = 0; i < GestaoDados.Tipologias.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {GestaoDados.Tipologias[i]}");
            }
            Console.Write("Escolha tipologia (número): ");
            int tidx = LerInt();
            string tip = (tidx >= 1 && tidx <= GestaoDados.Tipologias.Count) ? GestaoDados.Tipologias[tidx - 1] : GestaoDados.Tipologias.First();

            Unidade u;
            if (tip == "EDCCI")
            {
                u = new EDCCI(nome, zona);
            }
            else
            {
                Console.Write("Número de camas: ");
                int camas = LerInt();
                if (tip == "UC") u = new UC(nome, zona, camas);
                else if (tip == "UMDR") u = new UMDR(nome, zona, camas);
                else if (tip == "ULDM") u = new ULDM(nome, zona, camas);
                else u = new UC(nome, zona, camas); // fallback
            }

            GestaoDados.InserirUnidade(u);
            Console.WriteLine("Unidade inserida.");
            Console.ReadKey();
        }

        private static void AtualizarUnidade()
        {
            Console.Clear();
            Console.WriteLine("=== Atualizar Unidade ===");
            Console.Write("Nome da unidade: ");
            string nome = Console.ReadLine();
            Unidade u = GestaoDados.ObterUnidades().FirstOrDefault(x => x.Nome == nome);
            if (u == null)
            {
                Console.WriteLine("Unidade não encontrada.");
                Console.ReadKey();
                return;
            }

            if (u.ConsultarDoentes().Any())
            {
                Console.WriteLine("Não é possível atualizar unidade com doentes internados. Faça alta antes.");
                Console.ReadKey();
                return;
            }

            // Recriar unidade com novos dados (porque propriedades têm set protected)
            Console.WriteLine("Deixe em branco para manter o valor atual.");
            Console.Write($"Novo nome (atual: {u.Nome}): ");
            string novoNome = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(novoNome)) novoNome = u.Nome;

            Console.WriteLine("Zonas:");
            for (int i = 0; i < GestaoDados.Zonas.Length; i++)
                Console.WriteLine($"{i + 1} - {GestaoDados.Zonas[i]}");
            Console.Write("Escolha nova zona (número, 0 para manter): ");
            int zidx;
            if (!int.TryParse(Console.ReadLine(), out zidx) || zidx < 1 || zidx > GestaoDados.Zonas.Length)
                zidx = -1;
            string novaZona = zidx == -1 ? u.Zona : GestaoDados.Zonas[zidx - 1];

            string tip = u.GetTipologia();
            Unidade novaUnidade;
            if (tip == "EDCCI")
            {
                novaUnidade = new EDCCI(novoNome, novaZona);
            }
            else
            {
                Console.Write("Número de camas: ");
                int camas = LerInt();
                if (tip == "UC") novaUnidade = new UC(novoNome, novaZona, camas);
                else if (tip == "UMDR") novaUnidade = new UMDR(novoNome, novaZona, camas);
                else if (tip == "ULDM") novaUnidade = new ULDM(novoNome, novaZona, camas);
                else novaUnidade = new UC(novoNome, novaZona, camas);
            }

            // substituir: remover e inserir
            GestaoDados.RemoverUnidade(u.Nome);
            GestaoDados.InserirUnidade(novaUnidade);
            Console.WriteLine("Unidade atualizada.");
            Console.ReadKey();
        }

        private static void ConsultarUnidades()
        {
            Console.Clear();
            Console.WriteLine("=== Lista de Unidades ===");
            var lista = GestaoDados.ObterUnidades().ToList();
            if (!lista.Any())
            {
                Console.WriteLine("Nenhuma unidade registada.");
            }
            else
            {
                foreach (var u in lista)
                {
                    Console.WriteLine($"Nome:{u.Nome} | Tipologia:{u.GetTipologia()} | Zona:{u.Zona} | Camas Livres:{u.CamasDisponiveis()}");
                }
            }
            Console.ReadKey();
        }

        private static void EliminarUnidade()
        {
            Console.Clear();
            Console.WriteLine("=== Eliminar Unidade ===");
            Console.Write("Nome da unidade: ");
            string nome = Console.ReadLine();
            try
            {
                GestaoDados.RemoverUnidade(nome);
                Console.WriteLine("Unidade removida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            Console.ReadKey();
        }

        // ---------------------------
        // Menu Tipologias
        // ---------------------------
        private static void MenuTipologias()
        {
            int opcao;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Gestão de Tipologias ===");
                Console.WriteLine("1 - Inserir");
                Console.WriteLine("2 - Atualizar");
                Console.WriteLine("3 - Consultar");
                Console.WriteLine("4 - Eliminar");
                Console.WriteLine("0 - Sair");
                Console.Write("\nEscolha uma opção: ");

                if (!int.TryParse(Console.ReadLine(), out opcao))
                {
                    Console.WriteLine("Opção inválida.");
                    Console.ReadKey();
                    continue;
                }

                try
                {
                    switch (opcao)
                    {
                        case 1:
                            InserirTipologia();
                            break;
                        case 2:
                            AtualizarTipologia();
                            break;
                        case 3:
                            ConsultarTipologias();
                            break;
                        case 4:
                            EliminarTipologia();
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("Opção inválida.");
                            Console.ReadKey();
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

        private static void InserirTipologia()
        {
            Console.Clear();
            Console.WriteLine("=== Inserir Tipologia ===");
            Console.Write("Nome da tipologia: ");
            string t = Console.ReadLine();
            try
            {
                GestaoDados.InserirTipologia(t);
                Console.WriteLine("Tipologia inserida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static void AtualizarTipologia()
        {
            Console.Clear();
            Console.WriteLine("=== Atualizar Tipologia ===");
            Console.WriteLine("Tipologias atuais:");
            ConsultarTipologias(false);
            Console.Write("Tipologia a alterar: ");
            string antiga = Console.ReadLine();
            Console.Write("Nova tipologia: ");
            string nova = Console.ReadLine();

            try
            {
                // simples estratégia: remover antiga e inserir nova
                GestaoDados.RemoverTipologia(antiga);
                GestaoDados.InserirTipologia(nova);
                Console.WriteLine("Tipologia atualizada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            Console.ReadKey();
        }

        private static void ConsultarTipologias(bool wait = true)
        {
            Console.Clear();
            Console.WriteLine("=== Tipologias ===");
            var lista = GestaoDados.Tipologias.ToList();
            if (!lista.Any())
            {
                Console.WriteLine("Nenhuma tipologia definida.");
            }
            else
            {
                foreach (var t in lista)
                {
                    Console.WriteLine($"- {t}");
                }
            }
            if (wait) Console.ReadKey();
        }

        private static void EliminarTipologia()
        {
            Console.Clear();
            Console.WriteLine("=== Eliminar Tipologia ===");
            Console.Write("Tipologia: ");
            string t = Console.ReadLine();
            try
            {
                GestaoDados.RemoverTipologia(t);
                Console.WriteLine("Tipologia removida.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            Console.ReadKey();
        }

        // ---------------------------
        // Helpers
        // ---------------------------
        private static int LerInt()
        {
            int val;
            while (!int.TryParse(Console.ReadLine(), out val))
            {
                Console.Write("Valor inválido. Tente novamente: ");
            }
            return val;
        }

        private static int LerIntPositivo()
        {
            // Lê um inteiro e força ser positivo; usado tipicamente para idades/camas
            int val = LerInt();
            while (val <= 0)
            {
                Console.Write("Idade inválida. Introduza um número inteiro positivo: ");
                val = LerInt();
            }
            return val;
        }

        private static string LerLetras(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s))
                {
                    if (allowEmpty) return string.Empty;
                    Console.WriteLine("Entrada inválida. Utilize apenas letras e espaços.");
                    continue;
                }
                s = s.Trim();
                bool ok = s.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-');
                if (ok) return s;
                Console.WriteLine("Entrada inválida. Utilize apenas letras e espaços.");
            }
        }

        private static int LerIntAllowEmpty(int valorAtual)
        {
            // Permite ao utilizador deixar em branco para manter valor atual (usado em updates)
            string s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return valorAtual;
            int v;
            if (int.TryParse(s, out v)) return v;
            return valorAtual;
        }
    }
 }
