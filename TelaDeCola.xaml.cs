using LudoHive.Telas;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LudoHive
{
    /// <summary>
    /// Lógica interna para TelaDeCola.xaml
    /// </summary>
    public partial class TelaDeCola : Window
    {
        Process navegadorAberto;
        int imgCarregada = 0;
        public TelaDeCola(Process navegador, string acaoAtual)
        {
            InitializeComponent();
            DefinirGatilhos();
            lblNomeDaPesquisa.Content = acaoAtual;
            navegadorAberto = navegador;
        }
        private void DefinirGatilhos()
        {
            btnSendTelaCola.Click += (s, e) => RetornarURL();
            btnCloseTelaCola.Click += (s, e) => FecharBuscaWeb();

            txtbxURLReturn.EnterPressed += (s, e) => picOnImgPesquisa.Url = txtbxURLReturn.Texto;
            txtbxURLReturn.TextoChanged += (s, e) => picOnImgPesquisa.Url = txtbxURLReturn.Texto;

            txtbxURLReturn.EnterPressed += EnterToEndCola;

            picOnImgPesquisa.ImgCarregada += () => imgCarregada = 1;
        }
        private void EnterToEndCola(object sender, EventArgs e)
        {
            if (sender == txtbxURLReturn)
            {
                if (imgCarregada == 1)
                {
                    RetornarURL();
                    imgCarregada = 0;
                }
            }
        }
        private void RetornarURL()
        {
            if (this.Owner is MainWindow tela)
            {
                int labelEspecificado = 0;
                if (lblNomeDaPesquisa.Content.Equals("Imagem do Atalho")) { labelEspecificado = 1; }
                else
                if (lblNomeDaPesquisa.Content.Equals("Icone do Atalho")) { labelEspecificado = 2; }
                else
                if (lblNomeDaPesquisa.Content.Equals("Icone do Aplicativo")) { labelEspecificado = 3; }
                else
                if (lblNomeDaPesquisa.Content.Equals("Caminho do Aplicativo")) { labelEspecificado = 4; }

                Cadastrar telaCadastro = (Cadastrar)tela.mainGrid.FindName("cadastro");
                telaCadastro.DadoRecebidoOnline(txtbxURLReturn.Texto, labelEspecificado);

                FecharBuscaWeb();
            }
        }
        private void FecharBuscaWeb()
        {
            if (Properties.Settings.Default.FecharNavegador && navegadorAberto != null && !navegadorAberto.HasExited)
            {
                navegadorAberto.Kill();
            }

            if (this.Owner != null)
            {
                this.Owner.Show();
            }

            this.Close();
        }
    }
}