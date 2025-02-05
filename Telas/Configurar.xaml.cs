using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using LudoHive.Telas.Controles;

namespace LudoHive.Telas
{
    /// <summary>
    /// Interação lógica para Cadastrar.xam
    /// </summary>
    public partial class Configurar : UserControl
    {
        // Valores Configuracoes ======
        private string navegadorEmUso = Properties.Settings.Default.NavegadorEmUso;
        private bool fecharNavegador = Properties.Settings.Default.FecharNavegador;
        // Valores chave ==============
        private int idPastaAtual;
        private int idPastaEdit;
        private List<int> idsAtalhosParaPasta = new List<int>();
        // Eventos ====================
        public event Action<int> MonitorAlterado; 
        public event Action OrdemAlterada;
        public event Action AtalhoAdicionado;
        public event Action PastaRenomeada;
        public event Action FimConfiguracao;
        public Configurar(int idPasta = 1, List<Atalhos> atts = null)
        {
            InitializeComponent();

            CarregarConfigs();

            DefinirGatilhos();

            idPastaAtual = idPasta;

            AlterarPastaOrd(idPastaAtual, atts);

            CriarPastaOrd();
        }
        // Geral ----------------------------------------------------------------------------------
        private void DefinirGatilhos()
        {
            this.Loaded += AoCarregar;

            txtbxNavegadorPadrao.TextoChanged += (s, e) => {
                navegadorEmUso = txtbxNavegadorPadrao.Texto;
            };
            tgbtnFecharDpsPesquisa.ValorAlternado += (fechar) => {
                fecharNavegador = fechar;
            };
            tgbtnAdicionarAtalhos.ValorAlternado += (adicionar) => {
                CriarOrd(idPastaAtual);
                idsAtalhosParaPasta.Clear();
            };

            btnSalvarConfig.Click += (s, e) => SalvarConfiguracoes();
            btnCancelarConfig.Click += (s, e) => FecharConfiguracoes();
            btnRedefinirConfig.Click += (s, e) => RedefinirConfiguracoes();
            btnTabelaJogos.Click += (s, e) => MostrarTabela();

            ordAtalhosExibicao.ListarOrdem += BtnSalvarOrdAtalhos;
            ordAtalhosExibicao.ElementoClicado += SelecionarNovos;

            ordPastas.ElementoClicado += SelecionarPasta;
            ordPastas.DeleteElementoClicado += DeletarPasta;
            ordPastas.EditElementoClicado += (id, ord, lbl) => { idPastaEdit = id; NomearPasta(false); }; //False = Editar, True = Criar
            btnAddPasta.Click += (s, e) => NomearPasta(true);

            ordPastas.Loaded += (s,e) => ordPastas.Labels.Find(att => att.Id == idPastaAtual).CorBackGround = Color.FromArgb(255, 84, 84, 84);

            sttbtnMonitor.StateAlterado += (el) => MonitorAlterado?.Invoke(el.Id);
        }
        private void AoCarregar(object sender, EventArgs e)
        {
            sttbtnMonitor.EstadoAtual = Properties.Settings.Default.MonitorEmUso - 1;

            List<MonitorInfo> monitores = NativeMethods.GetAllMonitors();
            List<Elementos> els = new List<Elementos>();
            for (int i = 0; i < monitores.Count; i++)
            {
                Elementos el = new Elementos
                {
                    Nome = "Monitor " + (i + 1),
                    Id = i + 1,
                    Icone = new BitmapImage(Referencias.ludoIcon)
                };
                els.Add(el);
            }
            sttbtnMonitor.Estados = els;
        }
        private void CarregarConfigs()
        {
            txtbxNavegadorPadrao.Texto = navegadorEmUso;
            tgbtnFecharDpsPesquisa.IsTrue = fecharNavegador;
        }
        public void FecharConfiguracoes()
        {
            if (this.Parent is Grid grid)
            {
                grid.Children.Remove(this);
                grid.UnregisterName(this.Name);
                FimConfiguracao?.Invoke();
            }
        }
        private string GetDefaultBrowserPath()
        {
            string browserPath = string.Empty;

            string userChoicePath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";

            string progId = Registry.GetValue(userChoicePath, "ProgId", null) as string;

            if (string.IsNullOrEmpty(progId))
                throw new Exception("Navegador padrão não encontrado.");

            string browserRegPath = $@"HKEY_CLASSES_ROOT\{progId}\shell\open\command";
            browserPath = Registry.GetValue(browserRegPath, null, null) as string;

            if (string.IsNullOrEmpty(browserPath))
                throw new Exception("Caminho do navegador padrão não encontrado.");

            int firstQuote = browserPath.IndexOf('"');
            if (firstQuote >= 0)
            {
                int secondQuote = browserPath.IndexOf('"', firstQuote + 1);
                if (secondQuote > firstQuote)
                {
                    browserPath = browserPath.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                }
            }

            return browserPath;
        }
        // Tabela ---------------------------------------------------------------------------------
        private void MostrarTabela()
        {
            double largura = 0;
            double altura = 0;
            Window janela = Window.GetWindow(this);
            if (janela != null)
            {
                largura = janela.ActualWidth;
                altura = janela.ActualHeight;
            }

            Table tabelada = new Table
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CorCabecalhoTbl = Color.FromArgb(255, 31, 30, 30),
                TamanhoTexto = 18,
                WithDelete = false,
                Name = "tblEveryGame",
                TamanhoMax = new Point(largura /1.5, altura /1.5)
            };

            Button btn = new()
            {
                Content = "Fechar",
                FontSize = 18,
                Name = "btnFecharTabela",
                Background = new SolidColorBrush(Color.FromArgb(255, 31, 30, 30)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)),
                Width = 100,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid rec = new()
            {
                Name = "gdFechar",
                Background = new SolidColorBrush( Color.FromArgb(255, 31, 30, 30)),
                Height = 50
            };

            btn.Click += (s, e) =>
            {
                if (tabelada.Parent is Grid grid)
                {
                    grid.Children.Remove(tabelada);
                    grid.Children.Remove(rec);
                    grid.UnregisterName(tabelada.Name);
                    grid.UnregisterName(rec.Name);
                }
            };

            List<object> listaComComando = Jogos.ConsultarJogos().Select(obj => new
            {
                icone = obj.icon,
                obj.nome,
                obj.dataPrimeiraSessao,
                obj.dataUltimaSessao,
                stt_status = obj.id + "__" + obj.status + "__Em andamento__Abandonado__Finalizado__Game Service",
                obj.tempoTodasSessoes,
                obj.tempoUltimaSessao,
            }).ToList<object>();

            tabelada.ListaDeIcons = new() { new BitmapImage(Referencias.abandonado), new BitmapImage(Referencias.andamento), new BitmapImage(Referencias.finalizado), new BitmapImage(Referencias.gameService) };

            tabelada.ListaDeItens = listaComComando;

            tabelada.TabelaAlteradaViaStateBtn += (ele) => {
                int status = 0;
                if (ele.Nome.ToLower().Contains("em andamento"))
                    status = 0;
                if (ele.Nome.ToLower().Contains("abandonado"))
                    status = 1;
                if (ele.Nome.ToLower().Contains("finalizado"))
                    status = 2;
                if (ele.Nome.ToLower().Contains("game service"))
                    status = 3;
                Jogos.AlterarStatus(status, ele.Id);
            };

            tabelada.Loaded += (s, e) =>
            {
                rec.Margin = new Thickness(0, tabelada.ActualHeight + rec.ActualHeight, 0, 0);
                rec.Width = (int)tabelada.ActualWidth;
            };

            rec.Children.Add(btn);
            Panel.SetZIndex(tabelada, 99);
            Panel.SetZIndex(rec, 98);

            if (this.Parent is Grid gd) {
                gd.RegisterName(tabelada.Name, tabelada);
                gd.RegisterName(rec.Name, rec);

                gd.Children.Add(tabelada);
                gd.Children.Add(rec);
            }

            FecharConfiguracoes();
        }
        // Edicao Atalhos -------------------------------------------------------------------------
        private void BtnSalvarOrdAtalhos(List<int> listaOrdem)
        {
            if (tgbtnAdicionarAtalhos.IsTrue) 
            {
                if(idsAtalhosParaPasta.Count > 0)
                {
                    Atalhos.AdicionarAtalhoNaPasta(idsAtalhosParaPasta, idPastaAtual);
                    CriarOrd(idPastaAtual);
                    idsAtalhosParaPasta.Clear();
                    OrdemAlterada?.Invoke();

                    AtalhoAdicionado?.Invoke();
                }
            }
            else
            {
                if (idsAtalhosParaPasta.Count > 0)
                {
                    Atalhos.RetirarAtalhoDaPasta(idsAtalhosParaPasta, idPastaAtual);
                    CriarOrd(idPastaAtual);

                    //impede que a ordem seja redefinida contando itens que serão retirados da pasta
                    listaOrdem = listaOrdem.Except(idsAtalhosParaPasta).ToList();

                    idsAtalhosParaPasta.Clear();
                }
                Atalhos.AtualizarOrdem(listaOrdem, idPastaAtual);

                OrdemAlterada?.Invoke();
            }
        }
        private void SelecionarNovos(int idAtalho, int ordemAtalho, LabelCRUD lbl)
        {
            if (tgbtnAdicionarAtalhos.IsTrue)
            {
                //Retirar
                if (idsAtalhosParaPasta.Contains(idAtalho))
                {
                    idsAtalhosParaPasta.Remove(idAtalho);
                    lbl.CorBackGround = Color.FromArgb(255, 39, 39, 39);
                }
                //Adicionar
                else
                {
                    idsAtalhosParaPasta.Add(idAtalho);
                    lbl.CorBackGround = Color.FromArgb(255, 84, 86, 80);
                }
            } 
            else
            {
                //Retirar
                if (idsAtalhosParaPasta.Contains(idAtalho))
                {
                    idsAtalhosParaPasta.Remove(idAtalho);
                    lbl.CorBackGround = Color.FromArgb(255, 39, 39, 39);
                }
                //Adicionar
                else
                {
                    idsAtalhosParaPasta.Add(idAtalho);
                    lbl.CorBackGround = Color.FromArgb(255, 86, 80, 80);
                }
            }
        }
        private void CriarOrd(int idPasta = 1, List<Atalhos> atts = null)
        {
            ordAtalhosExibicao.Atts = new List<Elementos>();

            List<Elementos> elms = new List<Elementos>();

            if (atts == null) atts = Atalhos.ConsultarAtalhos(tgbtnAdicionarAtalhos.IsTrue ? Atalhos.ConsultarIDsFaltantes(idPasta) : Atalhos.ConsultarIDs(idPasta), tgbtnAdicionarAtalhos.IsTrue ? 0 : idPasta);

            foreach (Atalhos att in atts)
            {
                Elementos elm = new Elementos()
                {
                    Nome = att.getNomeAtalho(),
                    Id = att.getIdAtalho(),
                    Icone = att.getIconeAtalho(),
                    Ordem = att.getOrdemAtalho()
                };
                elms.Add(elm);
            }
            ordAtalhosExibicao.Atts = elms;
        }
        // Edicao Pasta ---------------------------------------------------------------------------
        private void SelecionarPasta(int idPasta, int ordemPasta, LabelCRUD lbl)
        {
            if (idPastaAtual != idPasta)
            {
                foreach (LabelCRUD lb in ordPastas.Labels)
                {
                    lb.CorFundo = Color.FromArgb(255, 39, 39, 39);
                }
                lbl.CorBackGround = Color.FromArgb(255, 84, 84, 84);

                AlterarPastaOrd(idPasta);
            }
        }
        private void DeletarPasta(int idPasta, int ordemPasta, LabelCRUD lbl)
        {
            ordPastas.LabelRetirar(lbl);
            Atalhos.DeletarPasta(idPasta);
        }
        private void EditarPasta(object sender, EventArgs e)
        {
            txtbxNomePasta.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(txtbxNomePasta.Texto)) 
            {
                Atalhos pasta = new Atalhos();
                pasta.setNomePasta(txtbxNomePasta.Texto);
                pasta.setIdPasta(idPastaEdit);

                Atalhos.AlterarPasta(pasta);

                txtbxNomePasta.Texto = "";

                txtbxNomePasta.EnterPressed -= CriarPasta;
                txtbxNomePasta.EnterPressed -= EditarPasta;

                PastaRenomeada?.Invoke();

                CriarPastaOrd();
            }
        }
        private void CriarPasta(object sender, EventArgs e)
        {
            txtbxNomePasta.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(txtbxNomePasta.Texto))
            {
                Atalhos pasta = new Atalhos();
                pasta.setNomePasta(txtbxNomePasta.Texto);

                Atalhos.SalvarPasta(pasta);

                txtbxNomePasta.Texto = "";

                txtbxNomePasta.EnterPressed -= CriarPasta;
                txtbxNomePasta.EnterPressed -= EditarPasta;

                CriarPastaOrd();
            }
        }
        private void NomearPasta(bool criar)
        {
            if (txtbxNomePasta.Visibility == Visibility.Collapsed)
            {
                txtbxNomePasta.Visibility = Visibility.Visible;
                txtbxNomePasta.SetFocus();

                txtbxNomePasta.EnterPressed += criar ? CriarPasta : EditarPasta;
            } 
            else
            {
                txtbxNomePasta.Visibility = Visibility.Collapsed;

                txtbxNomePasta.EnterPressed -= criar ? CriarPasta : EditarPasta;
            }
        }
        private void AlterarPastaOrd(int idPasta, List<Atalhos> atts = null)
        {
            idPastaAtual = idPasta;
            CriarOrd(idPasta, atts);
        }
        private void CriarPastaOrd()
        {
            List<Elementos> elms = new List<Elementos>();

            List<Atalhos> pastas = Atalhos.ConsultarPasta();

            foreach (Atalhos pst in pastas)
            {
                Elementos elm = new Elementos()
                {
                    Nome = pst.getNomePasta(),
                    Id = pst.getIdPasta(),
                    Ordem = pst.getOrdemPasta()
                };
                elms.Add(elm);
            }
            ordPastas.Atts = elms;
        }
        // Configuracoes --------------------------------------------------------------------------
        private void SalvarConfiguracoes()
        {
            Properties.Settings.Default.NavegadorEmUso = navegadorEmUso;
            Properties.Settings.Default.FecharNavegador = fecharNavegador;
            Properties.Settings.Default.Save();

            FecharConfiguracoes();
        }
        private void RedefinirConfiguracoes()
        {
            navegadorEmUso = GetDefaultBrowserPath();
            fecharNavegador = false;

            CarregarConfigs();
        }
    }
}