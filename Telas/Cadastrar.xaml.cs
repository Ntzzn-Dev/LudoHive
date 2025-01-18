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
    public partial class Cadastrar : UserControl
    {
        private int idDeAlteracao;
        private int imgCarregada;
        public event EventHandler atalhoCadastrado;
        public Cadastrar()
        {
            InitializeComponent();
            DefinirGatilhos();
            DefinirImgs();
            TrocarPage(1);

            btnSalvarAtalho.Click += (s, e) => SalvarAtalho();
            btnSalvarApp.Click += (s, e) => SalvarApp();
        }
        public Cadastrar(int id, int tipoDeEdicao)
        {
            InitializeComponent();
            DefinirGatilhos();

            switch (tipoDeEdicao)
            {
                case 0:
                    DadosAtalho(id);
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
        private void DefinirImgs()
        {
            BitmapImage btm = new BitmapImage(new Uri(Referencias.imgPrincipal));
            picIconAtalho.Source = btm;
            picImgAtalho.Source = btm;
            picIconApp.Source = btm;
        }
        private void SalvarAtalho()
        {
            Atalhos atl = new Atalhos();
            atl.setNomeAtalho(txtbxNomeAtalho.Texto);
            atl.setCaminhoAtalho(txtbxCaminhoAtalho.Texto);
            atl.setParametroAtalho(txtbxParamAtalho.Texto);
            if (picImgAtalho.Source is BitmapImage imgAtalho) atl.setImgAtalho(imgAtalho);
            if (picIconAtalho.Source is BitmapImage imgIcon) atl.setIconeAtalho(imgIcon);

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
            if (picImgAtalho.Source is BitmapImage imgAtalho) atl.setImgAtalho(imgAtalho);
            if (picIconAtalho.Source is BitmapImage imgIcon) atl.setIconeAtalho(imgIcon);

            Atalhos.Alterar(atl);

            atalhoCadastrado?.Invoke(this, EventArgs.Empty);

            FecharCadastro();
        }
        private void DadosAtalho(int id)
        {
            Atalhos atalhoAtual = new Atalhos(id);

            idDeAlteracao = atalhoAtual.getIdAtalho();
            txtbxNomeAtalho.Texto = atalhoAtual.getNomeAtalho();
            txtbxCaminhoAtalho.Texto = atalhoAtual.getCaminhoAtalho();
            txtbxParamAtalho.Texto = atalhoAtual.getParametroAtalho();
            picImgAtalho.Source = atalhoAtual.getImgAtalho();
            picIconAtalho.Source = atalhoAtual.getIconeAtalho();

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
        private void EnterToEndAtalhos(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtbxNomeAtalho)
                {
                    txtbxCaminhoAtalho.Focus();
                }
                else if (sender == txtbxCaminhoAtalho)
                {
                    txtbxParamAtalho.Focus();
                }
                else if (sender == txtbxParamAtalho)
                {
                    txtbxImgAtalho.Focus();
                }
                else if (sender == txtbxImgAtalho)
                {
                    txtbxIconAtalho.Focus();
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
        }

        //Segunda Aba - Aplicativos

        private void SalvarApp()
        {
            Aplicativos app = new Aplicativos();
            app.setNomeAplicativo(txtbxNomeApp.Texto);
            app.setCaminhoAplicativo(txtbxCaminhoApp.Texto);
            if (picIconApp.Source is BitmapImage imgIcon) app.setIconeAplicativo(imgIcon);

            Aplicativos.Salvar(app);

            FecharCadastro();
        }
        private void AlterarApp()
        {
            Aplicativos app = new Aplicativos();
            app.setIdAplicativo(idDeAlteracao);
            app.setNomeAplicativo(txtbxNomeApp.Texto);
            app.setCaminhoAplicativo(txtbxCaminhoApp.Texto);
            if (picIconApp.Source is BitmapImage imgIcon) app.setIconeAplicativo(imgIcon);

            Aplicativos.Alterar(app);

            FecharCadastro();
        }
        private void DadosApp(int id)
        {
            Aplicativos app = new Aplicativos(id);

            idDeAlteracao = app.getIdAplicativo();
            txtbxNomeApp.Texto = app.getNomeAplicativo();
            txtbxCaminhoApp.Texto = app.getCaminhoAplicativo();
            picIconApp.Source = app.getIconeAplicativo();

            txtbxIconApp.Placeholder = "Deixe em branco para manter o icone";
        }
        private void EnterToEndApps(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtbxNomeApp)
                {
                    txtbxCaminhoApp.Focus();
                }
                else if (sender == txtbxCaminhoApp)
                {
                    txtbxIconApp.Focus();
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

            txtbxImgAtalho.KeyDown += (s, e) => { if (e.Key == Key.Enter) { BaixarImgs(txtbxImgAtalho.Texto, picImgAtalho); } };
            txtbxIconAtalho.KeyDown += (s, e) => { if (e.Key == Key.Enter) { BaixarImgs(txtbxIconAtalho.Texto, picIconAtalho); } };
            txtbxImgAtalho.TextoChanged += (s, e) => BaixarImgs(txtbxImgAtalho.Texto, picImgAtalho);
            txtbxIconAtalho.TextoChanged += (s, e) => BaixarImgs(txtbxIconAtalho.Texto, picIconAtalho);

            txtbxNomeAtalho.KeyDown += EnterToEndAtalhos;
            txtbxCaminhoAtalho.KeyDown += EnterToEndAtalhos;
            txtbxParamAtalho.KeyDown += EnterToEndAtalhos;
            txtbxImgAtalho.KeyDown += EnterToEndAtalhos;
            txtbxIconAtalho.KeyDown += EnterToEndAtalhos;

            //Segunda Aba - Aplicativos

            btnIconAppOnline.Click += BtnProcurarImgOnline;
            btnIconAppLocal.Click += BtnProcurarImgLocal;
            btnExeAppLocal.Click += (s, e) => CriarTelaDeCola(null, "Colar URL ou URI");
            btnCancelarApp.Click += (s, e) => FecharCadastro();

            txtbxIconApp.KeyDown += (s, e) => { if (e.Key == Key.Enter) { BaixarImgs(txtbxIconApp.Texto, picIconApp); } };
            txtbxIconApp.TextoChanged += (s, e) => BaixarImgs(txtbxIconApp.Texto, picIconApp);

            txtbxNomeApp.KeyDown += EnterToEndApps;
            btnExeAppLocal.KeyDown += EnterToEndApps;
            txtbxIconApp.KeyDown += EnterToEndApps;
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
            switch (labelEspecificado)
            {
                case 0:
                    txtbxImgAtalho.Texto = urlRecebida;
                    break;
                case 1:
                    txtbxIconAtalho.Texto = urlRecebida;
                    break;
                case 2:
                    //txtbxImgIconeApp.Texto = urlRecebida;
                    break;
                case 3:
                    //txtbxURLApp.Texto = urlRecebida;
                    break;
            }
        }
        private void Reaparecer(string nomeNavegador)
        {
            /*this.Show();
            this.Owner.Show();*/

            Process[] navegatorProcesses = Process.GetProcessesByName(nomeNavegador);
            foreach (var process in navegatorProcesses)
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao encerrar o processo: {ex.Message}");
                }
            }
        }
        private void BtnProcurarImgOnline(object sender, EventArgs e)
        {
            string palavraChave = txtbxNomeAtalho.Texto;
            string urlPesquisa = $"https://www.google.com/search?hl=pt-BR&tbm=isch&q={Uri.EscapeDataString(palavraChave)}";

            //if (sender == btnIconOnlineApp) { palavraChave = txtbxNomeApp.Texto; }

            if (sender == btnIconAtalhoOnline /*|| sender == btnIconOnlineApp*/)
            {
                palavraChave += " Logo";
                urlPesquisa = $"https://www.google.com/search?as_st=y&hl=pt-BR&as_q={Uri.EscapeDataString(palavraChave)}&as_epq=&as_oq=&as_eq=&imgar=s&imgcolor=&imgtype=&cr=&as_sitesearch=&as_filetype=&tbs=&udm=2";
            }

            try
            {
                string caminhoNavegador = GetDefaultBrowserPath();

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
                /*else
                if (sender == btnIconAtalhoOnline) { acaoTelaCola = lblImgIconeApp.Text; }
                else
                if (sender == btnIconOnlineApp) { acaoTelaCola = lblImgIconeApp.Text; }*/

                CriarTelaDeCola(navegadorAberto, acaoTelaCola);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o navegador: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
        public void BtnProcurarImgLocal(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Imagens (*.jpg;*.png;*.webp;*.jpeg)|*.jpg;*.png;*.webp;*.jpeg";

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
            }
        }

        private void CriarTelaDeCola(Process navegadorAberto, string acaoTelaCola)
        {
            /*if (navegadorAberto != null && !navegadorAberto.HasExited)
            {
                string nomeDoProcessador = navegadorAberto.ProcessName;
                Form4 telaCola = new Form4(navegadorAberto, acaoTelaCola);
                telaCola.Owner = this;
                telaCola.FormClosed += (s, e) => Reaparecer(nomeDoProcessador);

                this.Owner.Hide();
                this.Hide();

                telaCola.ShowDialog();
            }
            else
            {
                Form4 telaCola = new Form4(acaoTelaCola);
                telaCola.Owner = this;

                this.Owner.Hide();
                this.Hide();

                telaCola.ShowDialog();
            }*/
        }
        private async Task BaixarImgs(string pathToImg, Image pcbxEmUso)
        {
            try
            {
                string formato = await DetectarFormatoAsync(pathToImg);
                bool eIcone = pcbxEmUso == picIconAtalho || pcbxEmUso == picIconApp;

                if (string.IsNullOrEmpty(pathToImg))
                {
                    if (txtbxCaminhoAtalho.Texto != "")
                    {
                        pcbxEmUso.Source = new BitmapImage(new Uri( Referencias.imgPrincipal));
                    }
                    return;
                }

                if (formato == "LOCAL")
                {
                    BitmapImage imgCarregada = new BitmapImage(new Uri(pathToImg, UriKind.Absolute));
                    if (imgCarregada.Width != imgCarregada.Height && eIcone)
                    {
                        txtbxIconAtalho.Texto = "";
                        MessageBox.Show("a imagem deve ser quadrada");
                    }
                    else
                    {
                        pcbxEmUso.Source = imgCarregada;
                    }
                }
                else if (formato == "BASE64")
                {
                    var base64Data = pathToImg.Split(',')[1];
                    byte[] bytesDaImg2 = Convert.FromBase64String(base64Data);

                    using (MemoryStream ms = new MemoryStream(bytesDaImg2))
                    {
                        BitmapImage imgCarregada = Referencias.memoryStreamToBitmap(ms);
                        if (imgCarregada.Width != imgCarregada.Height && eIcone)
                        {
                            txtbxIconAtalho.Texto = "";
                            MessageBox.Show("a imagem deve ser quadrada");
                        }
                        else
                        {
                            pcbxEmUso.Source = imgCarregada;
                        }
                    }
                }
                else if (formato == "OUTRO")
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] bytesDaImg = await client.GetByteArrayAsync(pathToImg);

                        using (MemoryStream ms = new MemoryStream(bytesDaImg))
                        {
                            BitmapImage imgCarregada = Referencias.memoryStreamToBitmap(ms);
                            if (imgCarregada.Width != imgCarregada.Height && eIcone)
                            {
                                txtbxIconAtalho.Texto = "";
                                MessageBox.Show("a imagem deve ser quadrada");
                            }
                            else
                            {
                                pcbxEmUso.Source = imgCarregada;
                            }
                        }
                    }
                }
                else if (formato == "WEBP")
                {
                    BitmapImage imagem = await CarregarImagemWebpAsync(pathToImg);

                    if (imagem != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BitmapImage imgCarregada = Referencias.memoryStreamToBitmap(ms);

                            pcbxEmUso.Source = imgCarregada;
                        }
                    }
                }
                else if (formato == "ICO")
                {
                    BitmapImage imagem = await BaixarEConverterIcoAsync(pathToImg);

                    if (imagem != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BitmapImage imgCarregada = Referencias.memoryStreamToBitmap(ms);

                            pcbxEmUso.Source = imgCarregada;
                        }
                    }
                }
                else { return; }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao baixar a imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<string> DetectarFormatoAsync(string url)
        {
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) { return "BASE64"; }
            else
            if (File.Exists(url)) { return "LOCAL"; }
            else
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(url);

                    if (bytes.Length >= 12)
                    {
                        string header = BitConverter.ToString(bytes.Take(12).ToArray()).Replace("-", "");

                        if (header.StartsWith("52494646") && header.Contains("57454250")) // WEBP
                            return "WEBP";

                        if (header.StartsWith("00000100") || header.StartsWith("00000200")) // ICO
                            return "ICO";
                    }
                    return "OUTRO";
                }
            }
            return "NENHUM";
        }
        private async Task<BitmapImage> CarregarImagemWebpAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes = await client.GetByteArrayAsync(url);

                using (var ms = new MemoryStream(bytes))
                {
                    SKBitmap bitmap = SKBitmap.Decode(ms);
                    using (var imgStream = new MemoryStream())
                    {
                        bitmap.Encode(imgStream, SKEncodedImageFormat.Png, 100);
                        imgStream.Seek(0, SeekOrigin.Begin);

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = imgStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        return bitmapImage;
                    }
                }
            }
        }
        private async Task<BitmapImage> BaixarEConverterIcoAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] icoBytes = await client.GetByteArrayAsync(url);

                using (var ms = new MemoryStream(icoBytes))
                {
                    SKBitmap bitmap = SKBitmap.Decode(ms);
                    if (bitmap == null)
                        throw new Exception("Não foi possível decodificar a imagem como um ícone válido.");

                    using (MemoryStream pngStream = new MemoryStream())
                    {
                        bitmap.Encode(pngStream, SKEncodedImageFormat.Png, 100);
                        pngStream.Seek(0, SeekOrigin.Begin);

                        BitmapImage imgCarregada = new BitmapImage();
                        imgCarregada.BeginInit();
                        imgCarregada.CacheOption = BitmapCacheOption.OnLoad;
                        imgCarregada.StreamSource = pngStream;
                        imgCarregada.EndInit();
                        imgCarregada.Freeze();

                        return imgCarregada;
                    }
                }
            }
        }

        //Centralizar manualmente ja que essa merda n vem automatico igual no window form pelo menos não dessa maneira
        private void picImgAtalho_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                picImgAtalho.Margin = new Thickness(recPicImgAtalho.Margin.Left + (recPicImgAtalho.ActualWidth - picImgAtalho.ActualWidth) / 2, recPicImgAtalho.Margin.Top + (recPicImgAtalho.ActualHeight - picImgAtalho.ActualHeight) / 2, 0, 0);
            }));
        }
        private void picIconAtalho_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                picIconAtalho.Margin = new Thickness(recPicIconAtalho.Margin.Left + (recPicIconAtalho.ActualWidth - picIconAtalho.ActualWidth) / 2, recPicIconAtalho.Margin.Top + (recPicIconAtalho.ActualHeight - picIconAtalho.ActualHeight) / 2, 0, 0);
            }));
        }
    }
}