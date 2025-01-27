using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using SkiaSharp;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private int idPastaAtual;
        private int idPastaEdit;
        private List<int> idsAtalhosParaPasta = new List<int>();
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
        private void DefinirGatilhos()
        {
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
            btnCancelarConfig.Click += (s, e) => FecharCadastro();
            btnRedefinirConfig.Click += (s, e) => RedefinirConfiguracoes();

            ordAtalhosExibicao.ListarOrdem += BtnSalvarOrdAtalhos;
            ordAtalhosExibicao.ElementoClicado += SelecionarNovos;

            ordPastas.ElementoClicado += SelecionarPasta;
            ordPastas.DeleteElementoClicado += DeletarPasta;
            ordPastas.EditElementoClicado += (id, ord, lbl) => { idPastaEdit = id; NomearPasta(false); }; //False = Editar, True = Criar
            btnAddPasta.Click += (s, e) => NomearPasta(true);

            ordPastas.Loaded += (s,e) => ordPastas.Labels.Find(att => att.Id == idPastaAtual).CorBackGround = Color.FromArgb(255, 84, 84, 84); ;
        }
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
        private void CriarOrd(int idPasta = 1, List<Atalhos> atts = null)
        {
            ordAtalhosExibicao.Atts = new List<Elementos>();

            List<Elementos> elms = new List<Elementos>();

            if (atts == null) atts = Atalhos.ConsultarAtalhos(tgbtnAdicionarAtalhos.IsTrue ? Atalhos.ConsultarIDsFaltantes(idPasta): Atalhos.ConsultarIDs(idPasta), tgbtnAdicionarAtalhos.IsTrue ? 0 : idPasta);

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
        private void CarregarConfigs()
        {
            txtbxNavegadorPadrao.Texto = navegadorEmUso;
            tgbtnFecharDpsPesquisa.IsTrue = fecharNavegador;
        }
        private void SalvarConfiguracoes()
        {
            Properties.Settings.Default.NavegadorEmUso = navegadorEmUso;
            Properties.Settings.Default.FecharNavegador = fecharNavegador;
            Properties.Settings.Default.Save();

            FecharCadastro();
        }
        private void RedefinirConfiguracoes()
        {
            navegadorEmUso = GetDefaultBrowserPath();
            fecharNavegador = false;

            CarregarConfigs();
        }
        public void FecharCadastro()
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
    }
}