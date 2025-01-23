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
        public Configurar()
        {
            InitializeComponent();

            CarregarConfigs();

            DefinirGatilhos();
        }
        private void DefinirGatilhos()
        {
            txtbxNavegadorPadrao.TextoChanged += (s, e) => {
                navegadorEmUso = txtbxNavegadorPadrao.Texto;
            };
            tgbtnFecharDpsPesquisa.ValorAlternado += (fechar) => {
                fecharNavegador = fechar;
            };
            btnSalvarConfig.Click += (s, e) => SalvarConfiguracoes();
            btnCancelarConfig.Click += (s, e) => FecharCadastro();
            btnRedefinirConfig.Click += (s, e) => RedefinirConfiguracoes();
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