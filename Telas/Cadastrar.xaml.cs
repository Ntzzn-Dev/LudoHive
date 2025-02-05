using Microsoft.Win32;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LudoHive.Telas
{
    /// <summary>
    /// Interação lógica para Cadastrar.xam
    /// </summary>
    public partial class Cadastrar : UserControl
    {
        private int idDeAlteracao;
        private int imgCarregada;
        public event EventHandler atalhoCadastrado;
        public event EventHandler appCadastrado;
        public event Action FimCadastro;
        public Cadastrar()
        {
            InitializeComponent();
            DefinirGatilhos();
            TrocarPage(1);

            btnSalvarAtalho.Click += (s, e) => SalvarAtalho();
            btnSalvarApp.Click += (s, e) => SalvarApp();
        }
        public Cadastrar(int id, int idPasta, int tipoDeEdicao)
        {
            InitializeComponent();
            DefinirGatilhos();

            switch (tipoDeEdicao)
            {
                case 0:
                    DadosAtalho(id, idPasta);
                    TrocarPage(1);
                    if (idDeAlteracao != 0) { btnSalvarAtalho.Click += (s, e) => AlterarAtalho(); }
                    break;
                case 1:
                    DadosApp(id);
                    TrocarPage(2);
                    if (idDeAlteracao != 0) { btnSalvarApp.Click += (s, e) => AlterarApp(); }
                    break;
            }
        }
        //Primeira aba - Atalhos
        private void SalvarAtalho()
        {
            Atalhos atl = new Atalhos();
            atl.setNomeAtalho(txtbxNomeAtalho.Texto);
            atl.setCaminhoAtalho(txtbxCaminhoAtalho.Texto);
            atl.setParametroAtalho(txtbxParamAtalho.Texto);
            atl.setImgAtalho((BitmapImage)picOnImgAtalho.Imagem);
            atl.setIconeAtalho((BitmapImage)picOnIconAtalho.Imagem);
            atl.setIdPasta(1);

            Atalhos.Salvar(atl);

            atalhoCadastrado?.Invoke(this, EventArgs.Empty);

            FecharCadastro();
        }
        private void AlterarAtalho()
        {
            Atalhos atl = new Atalhos();
            atl.setIdAtalho(idDeAlteracao);
            atl.setNomeAtalho(txtbxNomeAtalho.Texto);
            atl.setCaminhoAtalho(txtbxCaminhoAtalho.Texto);
            atl.setParametroAtalho(txtbxParamAtalho.Texto);
            atl.setImgAtalho((BitmapImage)picOnImgAtalho.Imagem);
            atl.setIconeAtalho((BitmapImage)picOnIconAtalho.Imagem);

            Atalhos.Alterar(atl);

            atalhoCadastrado?.Invoke(this, EventArgs.Empty);

            FecharCadastro();
        }
        private void DadosAtalho(int id, int idPasta)
        {
            Atalhos atalhoAtual = new Atalhos(id, idPasta);

            idDeAlteracao = atalhoAtual.getIdAtalho();
            txtbxNomeAtalho.Texto = atalhoAtual.getNomeAtalho();
            txtbxCaminhoAtalho.Texto = atalhoAtual.getCaminhoAtalho();
            txtbxParamAtalho.Texto = atalhoAtual.getParametroAtalho();
            picOnImgAtalho.Imagem = atalhoAtual.getImgAtalho();
            picOnIconAtalho.Imagem = atalhoAtual.getIconeAtalho();

            txtbxImgAtalho.Placeholder = "Deixe em branco para manter a imagem";
            txtbxIconAtalho.Placeholder = "Deixe em branco para manter o icone";
        }
        private void ObterDestinoAtalho(string atalho)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic wshShell = Activator.CreateInstance(shellType);

            dynamic atalhos = wshShell.CreateShortcut(atalho);

            string destino = atalhos.TargetPath;

            string extensao = Path.GetExtension(destino);

            string argumentos = "";

            string nomeAtalho = Path.GetFileNameWithoutExtension(atalho);

            if (destino == "")
            {
                destino = $@"{ObterPastaXbox()}\{nomeAtalho}\Content\gamelaunchhelper.exe";
            }
            if (extensao != "")
            {
                argumentos = atalhos.Arguments;
            }

            txtbxCaminhoAtalho.Texto = destino;
            txtbxParamAtalho.Texto = argumentos;
            txtbxNomeAtalho.Texto = nomeAtalho;
        }
        private string ObterPastaXbox()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    string xboxGamesPath = Path.Combine(drive.RootDirectory.FullName, "XboxGames");

                    if (Directory.Exists(xboxGamesPath))
                    {
                        return xboxGamesPath;
                    }
                }
            }
            return "";
        }
        private bool PegarIds()
        {
            ArrayList ids = Atalhos.ConsultarIDs();

            if (ids.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void BtnProcurarExecutavel(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Arquivos Executáveis (*.exe)|*.exe";

            if (ofd.ShowDialog() == true)
            {
                txtbxCaminhoAtalho.Texto = ofd.FileName;
            }
        }
        private void BtnImportarAtalho(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Selecione um arquivo de atalho",
                Filter = "Todos os Arquivos (*.url;*.lnk)|*.url;*.lnk"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ObterDestinoAtalho(openFileDialog.FileName);
            }
        }
        private void EnterToEndAtalhos(object sender, EventArgs e)
        {
            if (sender == txtbxNomeAtalho)
            {
                txtbxCaminhoAtalho.SetFocus();
            }
            else if (sender == txtbxCaminhoAtalho)
            {
                txtbxParamAtalho.SetFocus();
            }
            else if (sender == txtbxParamAtalho)
            {
                txtbxImgAtalho.SetFocus();
            }
            else if (sender == txtbxImgAtalho)
            {
                txtbxIconAtalho.SetFocus();
            }
            else if (sender == txtbxIconAtalho)
            {
                if (imgCarregada == 1)
                {
                    if (idDeAlteracao == 0) { SalvarAtalho(); }
                    else { AlterarAtalho(); }
                    imgCarregada = 0;
                }
            }
        }

        //Segunda Aba - Aplicativos

        private void SalvarApp()
        {
            Aplicativos app = new Aplicativos();
            app.setNomeAplicativo(txtbxNomeApp.Texto);
            app.setCaminhoAplicativo(txtbxCaminhoApp.Texto);
            app.setIconeAplicativo((BitmapImage)picOnIconApp.Imagem);

            Aplicativos.Salvar(app);

            appCadastrado?.Invoke(this, EventArgs.Empty);

            FecharCadastro();
        }
        private void AlterarApp()
        {
            Aplicativos app = new Aplicativos();
            app.setIdAplicativo(idDeAlteracao);
            app.setNomeAplicativo(txtbxNomeApp.Texto);
            app.setCaminhoAplicativo(txtbxCaminhoApp.Texto);
            app.setIconeAplicativo((BitmapImage)picOnIconApp.Imagem);

            Aplicativos.Alterar(app);

            appCadastrado?.Invoke(this, EventArgs.Empty);

            FecharCadastro();
        }
        private void DadosApp(int id)
        {
            Aplicativos app = new Aplicativos(id);

            idDeAlteracao = app.getIdAplicativo();
            txtbxNomeApp.Texto = app.getNomeAplicativo();
            txtbxCaminhoApp.Texto = app.getCaminhoAplicativo();
            picOnIconApp.Imagem = app.getIconeAplicativo();

            txtbxIconApp.Placeholder = "Deixe em branco para manter o icone";
        }
        private void EnterToEndApps(object sender, EventArgs e)
        {
            if (sender == txtbxNomeApp)
            {
                txtbxCaminhoApp.SetFocus();
            }
            else if (sender == txtbxCaminhoApp)
            {
                txtbxIconApp.SetFocus();
            }
            else if (sender == txtbxIconApp)
            {
                if (imgCarregada == 1)
                {
                    if (idDeAlteracao == 0) { SalvarApp(); }
                    else { AlterarAtalho(); }
                    imgCarregada = 0;
                }
            }
        }

        //Meio Termo - Atalhos e Aplicativos

        public void DefinirGatilhos()
        {
            btnCancelarAtalho.Click += (s, e) => FecharCadastro();

            btnExeAtalhoLocal.Click += BtnProcurarExecutavel;
            btnImgAtalhoLocal.Click += BtnProcurarImgLocal;
            btnImgAtalhoOnline.Click += BtnProcurarImgOnline;
            btnIconAtalhoLocal.Click += BtnProcurarImgLocal;
            btnIconAtalhoOnline.Click += BtnProcurarImgOnline;
            btnImportarAtalho.Click += BtnImportarAtalho;

            btnPageAtalho.Click += (s, e) => TrocarPage(1);
            btnPageApp.Click += (s, e) => TrocarPage(2);

            txtbxImgAtalho.EnterPressed += (s, e) => picOnImgAtalho.Url = txtbxImgAtalho.Texto;
            txtbxIconAtalho.EnterPressed += (s, e) => picOnIconAtalho.Url = txtbxIconAtalho.Texto;
            txtbxImgAtalho.TextoChanged += (s, e) => picOnImgAtalho.Url = txtbxImgAtalho.Texto;
            txtbxIconAtalho.TextoChanged += (s, e) => picOnIconAtalho.Url = txtbxIconAtalho.Texto;

            txtbxNomeAtalho.EnterPressed += EnterToEndAtalhos;
            txtbxCaminhoAtalho.EnterPressed += EnterToEndAtalhos;
            txtbxParamAtalho.EnterPressed += EnterToEndAtalhos;
            txtbxImgAtalho.EnterPressed += EnterToEndAtalhos;
            txtbxIconAtalho.EnterPressed += EnterToEndAtalhos;

            picOnIconAtalho.ImgCarregada += () => imgCarregada = 1;

            //Segunda Aba - Aplicativos

            btnIconAppOnline.Click += BtnProcurarImgOnline;
            btnIconAppLocal.Click += BtnProcurarImgLocal;
            btnExeAppLocal.Click += (s, e) => CriarTelaDeCola(null, "Colar URL ou URI");
            btnCancelarApp.Click += (s, e) => FecharCadastro();

            txtbxIconApp.KeyDown += (s, e) => { if (e.Key == Key.Enter) { picOnIconApp.Url = txtbxIconApp.Texto; } };
            txtbxIconApp.TextoChanged += (s, e) => picOnIconApp.Url = txtbxIconApp.Texto;

            txtbxNomeApp.EnterPressed += EnterToEndApps;
            txtbxCaminhoApp.EnterPressed += EnterToEndApps;
            txtbxIconApp.EnterPressed += EnterToEndApps;

            picOnIconApp.ImgCarregada += () => imgCarregada = 1;
        }
        private void TrocarPage(int indice)
        {
            Brush bsSelect = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
            Brush bsUnselect = new SolidColorBrush(Color.FromArgb(255, 44, 44, 44));

            btnPageAtalho.Background = bsUnselect;
            btnPageApp.Background = bsUnselect;
            btnPageAtalho.BorderBrush = bsUnselect;
            btnPageApp.BorderBrush = bsUnselect;

            gdPage1.Visibility = Visibility.Collapsed;
            gdPage2.Visibility = Visibility.Collapsed;
            switch (indice)
            {
                case 1:
                    btnPageAtalho.Background = bsSelect;
                    btnPageAtalho.BorderBrush = bsSelect;
                    gdPage1.Visibility = Visibility.Visible;
                    break;
                case 2:
                    btnPageApp.Background = bsSelect;
                    btnPageApp.BorderBrush = bsSelect;
                    gdPage2.Visibility = Visibility.Visible;
                    break;
            }
        }
        public void DadoRecebidoOnline(string urlRecebida, int labelEspecificado)
        {
            MessageBox.Show(labelEspecificado.ToString());
            switch (labelEspecificado)
            {
                case 1:
                    txtbxImgAtalho.Texto = urlRecebida;
                    break;
                case 2:
                    txtbxIconAtalho.Texto = urlRecebida;
                    break;
                case 3:
                    txtbxIconApp.Texto = urlRecebida;
                    break;
                case 4:
                    txtbxCaminhoApp.Texto = urlRecebida;
                    break;
            }
        }
        private void BtnProcurarImgOnline(object sender, EventArgs e)
        {
            string palavraChave = txtbxNomeAtalho.Texto;
            string urlPesquisa = $"https://www.google.com/search?hl=pt-BR&tbm=isch&q={Uri.EscapeDataString(palavraChave)}";

            if (sender == btnIconAppOnline) { palavraChave = txtbxNomeApp.Texto; }

            if (sender == btnIconAtalhoOnline || sender == btnIconAppOnline)
            {
                palavraChave += " Logo";
                urlPesquisa = $"https://www.google.com/search?as_st=y&hl=pt-BR&as_q={Uri.EscapeDataString(palavraChave)}&as_epq=&as_oq=&as_eq=&imgar=s&imgcolor=&imgtype=&cr=&as_sitesearch=&as_filetype=&tbs=&udm=2";
            }

            try
            {
                string caminhoNavegador = Properties.Settings.Default.NavegadorEmUso;

                Process navegadorAberto;

                if (File.Exists(caminhoNavegador))
                {
                    navegadorAberto = Process.Start(caminhoNavegador, urlPesquisa);
                }
                else
                {
                    navegadorAberto = Process.Start(urlPesquisa);
                }

                string acaoTelaCola = "";
                if (sender == btnImgAtalhoOnline) { acaoTelaCola = lblImgAtalho.Content?.ToString(); }
                else
                if (sender == btnIconAtalhoOnline) { acaoTelaCola = lblIconAtalho.Content?.ToString(); }
                else
                if (sender == btnIconAppOnline) { acaoTelaCola = lblIconApp.Content?.ToString(); }

                CriarTelaDeCola(navegadorAberto, acaoTelaCola);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o navegador: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void BtnProcurarImgLocal(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Imagens (*.jpg;*.png;*.webp;*.ico;*.jpeg)|*.jpg;*.png;*.webp;*.ico;*.jpeg";

            if (sender == btnImgAtalhoLocal)
            {
                if (ofd.ShowDialog() == true)
                {
                    txtbxImgAtalho.Texto = ofd.FileName;
                }
            }
            if (sender == btnIconAtalhoLocal)
            {
                if (ofd.ShowDialog() == true)
                {
                    txtbxIconAtalho.Texto = ofd.FileName;
                }
            }
            if (sender == btnIconAppLocal)
            {
                if (ofd.ShowDialog() == true)
                {
                    txtbxIconApp.Texto = ofd.FileName;
                }
            }
        }
        public void FecharCadastro()
        {
            if (this.Parent is Grid grid)
            {
                grid.Children.Remove(this);
                grid.UnregisterName(this.Name);
                FimCadastro?.Invoke();
            }
        }
        private void CriarTelaDeCola(Process navegadorAberto, string acaoTelaCola)
        {
            TelaDeCola tela = new TelaDeCola(navegadorAberto, acaoTelaCola);
            tela.Owner = Window.GetWindow(this);
            tela.Owner.Hide();
            tela.Show();
        }
    }
}