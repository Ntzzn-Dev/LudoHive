using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Security.Principal;
using WpfAnimatedGif;
using System.Windows.Interop;
using SharpDX.DirectInput;
using System.Windows.Media.Animation;
using LudoHive.Telas.Controles;
using LudoHive.Telas;
using System.Text.RegularExpressions;

namespace LudoHive;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
public partial class MainWindow : Window
{
    // Atalhos ========================
    private ArrayList ids = new ArrayList();
    private Atalhos atalhoAtual;
    private List<Atalhos> atalhosProximos = new List<Atalhos>();
    private Process jogoAberto;
    private int idAtual = Properties.Settings.Default.IdUltimoJogo;
    private bool controlesFuncionando = true;
    private bool trocandoImage = false;
    // Pastas =========================
    private Atalhos atalhoProxPasta;
    private List<Atalhos> atalhosProximosPastas = new List<Atalhos>();
    private int pastaAtual = Properties.Settings.Default.IdUltimaPasta;
    // Bandeja ========================
    private bool abrindoOJogo;
    // Painel APPS ====================
    private bool appsOcultos = true;
    private Popup pop;
    private List<FrameworkElement> controlesPermitidos = new List<FrameworkElement>();
    // Temporizadores =================
    private static System.Timers.Timer temporizadorDoRelogio, timerProcessoEstabilizar;
    private static DateTime horario;
    // Controle =======================
    private Controle controle;
    private float appAtual = 0, appCount = 0;
    private List<int> idsApps = new List<int>();
    // Hotkeys ========================
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int VK_NUMPAD5 = 0x65;
    private const int VK_T = 0x54;

    public MainWindow()
    {
        InitializeComponent();

        idAtual = idAtual == 0 ? 1 : idAtual;
        pastaAtual = pastaAtual == 0 ? 1 : pastaAtual;

        AtalhoPegarIds();

        PicImgVinheta.Source = new BitmapImage(Referencias.vinheta);
        picImgAppsOcultos.Source = new BitmapImage(Referencias.picAppsShow);

        DefinirGatilhos();

        CriarRelogio();
    }
    private void CloseWindow()
    {
        Application.Current.Shutdown();
    }
    private void CloseGame()
    {
        var resultado = MessageBox.Show("Deseja Fechar o jogo? Os dados não salvos serão perdidos", "Confirmação", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        if (resultado == MessageBoxResult.OK)
        {
            FecharAtalho();
            this.Topmost = true;
            this.Focus();
            RestaurarPlayOS(null, null);
            this.Topmost = false;
        }
    }
    private void RestaurarApp()
    {
        this.Topmost = true;
        this.Focus();
        RestaurarPlayOS(null, null);
        this.Topmost = false;
    }

    // Comandos do windows //
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        int WM_DEVICECHANGE = 0x0219;
        int DBT_DEVICEARRIVAL = 0x8000;

        if (msg == WM_DEVICECHANGE) //Controle
        {
            int wParamInt = wParam.ToInt32();
            if (wParamInt == DBT_DEVICEARRIVAL)
            {
                if(controle != null) LimparComandos();
                controle = new Controle();
                controle = new Controle(lParam);
                DefinirComandos();
            }
        }
        else if (msg == WM_HOTKEY) //Hotkeys
        {
            int id = wParam.ToInt32();
            if (id == 9000)
            {
                CloseWindow();
            }
            else if (id == 9001)
            {
                CloseGame();
            }
            else if (id == 9002)
            {
                RestaurarApp();
            }
            handled = true;
        }

        return IntPtr.Zero;
    }

    // Substituir acao das teclas //
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (controlesFuncionando == true) {
            if (e.Key == System.Windows.Input.Key.Left)
            {
                MoveLeft();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                MoveRight();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                MoveDown();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                MoveUp();
                e.Handled = true; 
            }
            else if (e.Key == System.Windows.Input.Key.Space)
            {
                BtnAbrirAtalho(null, null);
                e.Handled = true;
            }
        }
    }

    // Geral --------------------------------------------------------------------------------------
    private void DefinirGatilhos()
    {
        this.Loaded += AoCarregar;

        btnAbrir.Click += BtnAbrirAtalho;
        btnFechar.Click += (s, e) => this.Close();
        btnEditarAtalho.Click += BtnEditarAtalho;
        btnAdicionarAtalho.Click += (s, e) => Cadastrar();
        btnConfigurar.Click += (s, e) => Configurar();
        btnDeletarAtalho.Click += BtnDeletarAtalho;

        btnNextAtalho.Click += (s, e) => GameNext();
        btnPrevAtalho.Click += (s, e) => GamePrev();

        gdPnlApps.MouseDown += (s, e) => ToggleApps(e);
    }
    private void AoCarregar(object sender, EventArgs e)
    {
        NativeMethods.MoveToMonitor(Properties.Settings.Default.MonitorEmUso, this);
        if (atalhoAtual != null)
        {
            PicImgAtalhoAtual.Source = atalhoAtual.getImgAtalho();
            PicDefinirCorDeFundo(atalhoAtual.getImgAtalho());
            PicTransitionsCriar();

            CarregarAtalhosPastas();
        }
        PegarApps();
        InicializarControle();
        NativeMethods.RegisterForDeviceNotifications(this, IntPtr.Zero);

        var rectGeometry = new RectangleGeometry
        {
            Rect = new Rect(0, -30, mainGrid.ActualWidth, 245),
            RadiusX = 20,
            RadiusY = 20
        };
        recPnlApp.Clip = rectGeometry;

        var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(WndProc);

        NativeMethods.RegisterHotKey(new WindowInteropHelper(this).Handle, 9000, MOD_CONTROL | MOD_ALT, VK_T);
        NativeMethods.RegisterHotKey(new WindowInteropHelper(this).Handle, 9001, MOD_CONTROL, VK_NUMPAD5);
        NativeMethods.RegisterHotKey(new WindowInteropHelper(this).Handle, 9002, MOD_ALT, VK_NUMPAD5);
    }
    private void AtalhoPegarIds()
    {
        ids.Clear();
        try
        {
            ids = Atalhos.ConsultarIDs(pastaAtual);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao listar ids: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        if (ids.Count > 0)
        {
            if (!ids.Contains(idAtual)) { idAtual = (int)ids[0]; }
            atalhosProximos = Atalhos.ConsultarAtalhos(ids, pastaAtual);
            AtalhoListar(idAtual);
        }
        else
        {
            Cadastrar();
        }
    }
    private int AumentarIndice(int i, int[] a)
    {
        if (Array.IndexOf(a, i) == a.Length - 1)
        {
            i = a[0];
        }
        else
        {
            i += 1;
        }
        return i;
    }
    private void PicDefinirCorDeFundo(BitmapImage bitmapImage)
    {
        WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);

        long totalR = 0, totalG = 0, totalB = 0, totalA = 0;
        int totalPixels = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight;

        int stride = writeableBitmap.PixelWidth * 4;
        byte[] pixelData = new byte[writeableBitmap.PixelHeight * stride];
        writeableBitmap.CopyPixels(pixelData, stride, 0);

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            byte a = pixelData[i + 3];
            byte r = pixelData[i + 2];
            byte g = pixelData[i + 1];
            byte b = pixelData[i];

            totalA += a;
            totalR += r;
            totalG += g;
            totalB += b;
        }

        byte avgA = (byte)(totalA / totalPixels);
        byte avgR = (byte)(totalR / totalPixels);
        byte avgG = (byte)(totalG / totalPixels);
        byte avgB = (byte)(totalB / totalPixels);

        this.Background = new SolidColorBrush(Color.FromArgb(avgA, avgR, avgG, avgB));
    }
    // Aplicativos --------------------------------------------------------------------------------
    private void PegarApps()
    {
        gdApps.Children.Clear();
        gdApps.Width = gdPnlApps.ActualWidth;
        ArrayList idsApps = Aplicativos.ConsultarIDs();
        List<Aplicativos> appsContext = new List<Aplicativos>();

        foreach (int id in idsApps)
        {
            Aplicativos appAtual = new Aplicativos(id);
            appsContext.Add(appAtual);
        }

        PnlPnlAddApp(appsContext);
    }
    private void PnlPnlAddApp(List<Aplicativos> appsContext)
    {
        idsApps.Clear();
        for (int i = 0; i < appsContext.Count; i++)
        {
            gdApps.Children.Add(CriacaoApp(appsContext[i], OrganizacaoApps(appsContext.Count, i + 1)));
            idsApps.Add(appsContext[i].getIdAplicativo());
        }
        appCount = appsContext.Count;
        appsContext.Clear();
    }
    private Point OrganizacaoApps(int quantidadeDeApps, int posicaoDoApp)
    {
        int tamanhoApp = 100;
        int margemApps = 33;
        int meiaTela = (int)this.ActualWidth / 2;
        int posicao = meiaTela - tamanhoApp / 2;

        if (quantidadeDeApps % 2 == 0)
        {
            posicao += 62; //Centraliza para quantidades pares
        }

        int metadeDosApps = quantidadeDeApps / 2 + 1;

        //posicao diminui o tamanho do panel * posicao em relacao a metade - margem de distancia entre os apps
        posicao += -tamanhoApp * (metadeDosApps - posicaoDoApp) - margemApps * (metadeDosApps - posicaoDoApp);
        return new Point(posicao, 12);
    }
    private Grid CriacaoApp(Aplicativos appEmUso, Point localDoAplicativo)
    {
        Image picAppIcon = new Image
        {
            Source = appEmUso.getIconeAplicativo(),
            Width = 100,
            Height = 100,
            Margin = new Thickness(0, 0, 0, 0),
            Name = "picIconApp_" + Regex.Replace(appEmUso.getNomeAplicativo(), @"[^a-zA-Z0-9_]", "_") + "_ID_" + appEmUso.getIdAplicativo(),
            Stretch = Stretch.Uniform,
            Clip = new RectangleGeometry(new Rect(0, 0, 100, 100), 20, 20),
            VerticalAlignment = VerticalAlignment.Top
        };
        //
        TextBlock textBlock = new TextBlock
        {
            Text = appEmUso.getNomeAplicativo(),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = System.Windows.TextAlignment.Center
        };
        Label lblAppNome = new Label
        {
            FontFamily = new FontFamily("Arial"),
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 100, 0, 0),
            Name = "lblNomeApp_" + Regex.Replace(appEmUso.getNomeAplicativo(), @"[^a-zA-Z0-9_]", "_") + "_ID_" + appEmUso.getIdAplicativo(),
            Width = 100,
            Height = 100,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Content = textBlock
        };
        //
        Grid pnlBackground = new Grid
        {
            Margin = new Thickness(localDoAplicativo.X, localDoAplicativo.Y, 0, 0),
            Name = "pnl_" + Regex.Replace(appEmUso.getNomeAplicativo(), @"[^a-zA-Z0-9_]", "_") + "_ID_" + appEmUso.getIdAplicativo(),
            Width = 100,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        pnlBackground.Children.Add(picAppIcon);
        pnlBackground.Children.Add(lblAppNome);

        Brush cor = new SolidColorBrush(Color.FromArgb(67, 0, 0, 0));
        Brush semCor = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        pnlBackground.MouseEnter += (s, e) => pnlBackground.Background = cor;
        pnlBackground.MouseLeave += (s, e) => pnlBackground.Background = semCor;

        pnlBackground.MouseDown += (s, e) =>
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                CriarOpcoes(appEmUso.getIdAplicativo());
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                BtnAbrirAplicativos(s, e);
            }
        };

        return pnlBackground;
    }
    private void CriarOpcoes(int id)
    {
        Boxes boxAbrirApp = new Boxes()
        {
            IdBox = 0,
            IdRepassar = id,
            //Imagem = Properties.Resources.AdicionarAFila,
            Nome = "Abrir"
        };
        Boxes boxEditarApp = new Boxes()
        {
            IdBox = 1,
            IdRepassar = id,
            //Imagem = Properties.Resources.AdicionarAFila,
            Nome = "Editar"
        };
        Boxes boxDeletarApp = new Boxes()
        {
            IdBox = 2,
            IdRepassar = id,
            //Imagem = Properties.Resources.AdicionarAFila,
            Nome = "Deletar"
        };
        List<Boxes> bxs = new List<Boxes>() { boxAbrirApp, boxEditarApp, boxDeletarApp };
        
        CriarPopup(bxs);
    }
    private void ReconhecerEscolhaPopup(int botaoClicado, int valorRepassado)
    {
        if (botaoClicado == 0) { BtnAbrirAplicativos(null, EventArgs.Empty, valorRepassado); }
        if (botaoClicado == 1) { BtnEditarAplicativos(valorRepassado); }
        if (botaoClicado == 2) { BtnDeletarAplicativos(valorRepassado); }
    }
    private void BtnDeletarAplicativos(int id)
    {
        try
        {
            Aplicativos.Deletar(id);
            PegarApps();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao deletar o aplicativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void BtnEditarAplicativos(int id)
    {
        Cadastrar cad = new Cadastrar(id, pastaAtual, 1)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Name = "cadastro"
        };

        mainGrid.RegisterName(cad.Name, cad);

        cad.appCadastrado += (s, e) => PegarApps();
        cad.FimCadastro += VoltarPrograma;

        mainGrid.Children.Add(cad);
        Panel.SetZIndex(cad, 10);

        PararPrograma();
    }
    private void CriarPopup(List<Boxes> boxes)
    {
        Point relativePos = PointFromScreen(System.Windows.Input.Mouse.GetPosition(this));

        if(pop != null)
        {
            mainGrid.Children.Remove(pop);
            CriarDetectarCliqueForaPopup(false);
        }

        pop = new Popup()
        {
            ColorElementoPopup = Color.FromArgb(255, 44, 44, 44),
            ColorPopup = Color.FromArgb(255, 21, 22, 23),
            ColorTextPopup = Color.FromArgb(255, 192, 192, 192),
            Margin = new Thickness(relativePos.X, relativePos.Y, 0, 0),
            Name = "popup1",
            SizePopup = new Size(278, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        foreach (Boxes box in boxes)
        {
            pop.ElementosPopup.Add(box);
        }

        mainGrid.Children.Add(pop);
        Panel.SetZIndex(pop, 11);

        pop.BoxClicadoEvent += ReconhecerEscolhaPopup;

        CriarDetectarCliqueForaPopup(true);
    }
    private void CriarDetectarCliqueForaPopup(bool detectar)
    {
        if (detectar)
        {
            mainGrid.MouseDown += ClicarForaDoPopup;
        }
        else
        {
            mainGrid.MouseDown -= ClicarForaDoPopup;
        }
    }
    private void ClicarForaDoPopup(object sender, MouseEventArgs e)
    {
        mainGrid.Children.Remove(pop);
        pop = null;
        CriarDetectarCliqueForaPopup(false);
    }
    private void BtnAbrirAplicativos(object sender, EventArgs e, int id = 0)
    {
        Grid gd = sender as Grid;
        if (gd != null && gd.Name.Contains("_ID_"))
        {
            id = int.Parse(gd.Name.Split("_ID_")[1]);
        }
        else if(id == 0) { id = idsApps[(int)appAtual]; }

        try
        {
            Aplicativos appPraAbrir = new Aplicativos(id);

            string url = appPraAbrir.getCaminhoAplicativo();

            string diretorioTrabalho = System.IO.Path.GetDirectoryName(url);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = url,
                WorkingDirectory = diretorioTrabalho,
                UseShellExecute = true
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir o aplicativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void ToggleApps(MouseEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (e != null) { 
                var originalSource = e.OriginalSource as UIElement;
                var gdpnl = VisualTreeHelper.GetParent(originalSource);
                if (originalSource is Border)
                {
                    gdpnl = VisualTreeHelper.GetParent(gdpnl);
                }
                if ((gdpnl is Grid && gdpnl != gdPnlApps) || (originalSource is Grid && originalSource != gdPnlApps))
                {
                    return; //Se o mouse clicou em um dos aplicativos, não fechar
                }
            }

            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = new Thickness(6, gdPnlApps.Margin.Top, 0, 0),
                To = appsOcultos ? new Thickness(0, 0, 0, 0) : new Thickness(0, -215, 0, 0),
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new QuadraticEase()
            };

            animation.Completed += (s, e) =>
            {
                picImgAppsOcultos.Source = new BitmapImage(Referencias.picAppsHide);
                appsOcultos = appsOcultos ? false : true;
            };

            gdPnlApps.BeginAnimation(FrameworkElement.MarginProperty, animation);
        });
    }
    private void VisualizarEscolhaViaControle()
    {
        this.Dispatcher.Invoke(() =>
        {
            List<Grid> ctrls = new List<Grid>();
            foreach (Grid ctrl in gdApps.Children)
            {
                ctrls.Add(ctrl);
                ctrl.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            ctrls[(int)appAtual].Background = new SolidColorBrush(Color.FromArgb(67, 0, 0, 0));
        });
    }
    // Atalhos ------------------------------------------------------------------------------------
    private void Cadastrar()
    {
        Cadastrar cad = new Cadastrar()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Name = "cadastro"
        };

        cad.atalhoCadastrado += (s, e) => AtalhoPegarIds();
        cad.FimCadastro += VoltarPrograma;

        mainGrid.RegisterName(cad.Name, cad);

        mainGrid.Children.Add(cad);
        Panel.SetZIndex(cad, 10);

        PararPrograma();
    }
    private void BtnAbrirAtalho(object sender, EventArgs e)
    {
        try
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (controlesFuncionando == true)
                {
                    abrindoOJogo = true;
                    string caminho = atalhoAtual.getCaminhoAtalho();
                    string argumentacao = atalhoAtual.getParametroAtalho();

                    AbrirAtalho(caminho, argumentacao);
                    PicGIFAbrindoJogo();
                }
            }));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro no botao de abrir atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void BtnEditarAtalho(object sender, EventArgs e)
    {
        Cadastrar cadEdicao = new Cadastrar(idAtual, pastaAtual, 0)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Name = "cadastro"
        };

        cadEdicao.atalhoCadastrado += (s, e) => AtalhoPegarIds();
        cadEdicao.FimCadastro += VoltarPrograma;

        mainGrid.RegisterName(cadEdicao.Name, cadEdicao);

        mainGrid.Children.Add(cadEdicao);
        Panel.SetZIndex(cadEdicao, 10);

        PararPrograma();
    }
    private void BtnDeletarAtalho(object sender, EventArgs e)
    {
        Atalhos.Deletar(idAtual);

        if (!GameNext())
        {
            GamePrev();
        }

        AtalhoPegarIds();
    }
    private void AbrirAtalho(string caminho, string argumentacao)
    {
        try
        {
            int pid = -1;
            if (caminho.Contains("epicgames") || caminho.Contains("steam"))
            {
                pid = NativeMethods.abrirEpicLauncher(caminho);
            }
            else 
            { 
                pid = NativeMethods.initPrg(caminho, argumentacao); 
            }

            if (pid > 0)
            {
                jogoAberto = Process.GetProcessById(pid);
            }

             string inicioSessao = DateTime.Now.ToString("dd/MM/yy - HH:mm");
             Atalhos.SessaoIniciada(inicioSessao, idAtual);
             this.Dispatcher.Invoke(new Action(() =>
             {
                 lblDataSessao.Content = "Data: " + inicioSessao;
             }));

             PararPrograma();

             MonitoramentoDeProcesos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir o atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void AtalhoListar(int idatual)
    {
        this.Dispatcher.Invoke(() => {
            atalhoAtual = atalhosProximos.Find(atalho => atalho.getIdAtalho() == idAtual);

            lblNomeAtalho.Content = atalhoAtual.getNomeAtalho();
            lblCaminhoAtalho.Content = atalhoAtual.getCaminhoAtalho();

            lblDataSessao.Content = "Data: " + atalhoAtual.getDataSessaoAtalho();
            lblTempoSessao.Content = "Durou: " + atalhoAtual.getTempoSessaoAtalho();

            PicDefinirCorDeFundo(atalhoAtual.getImgAtalho());

            picIconAtalho.Source = atalhoAtual.getIconeAtalho();
        });
    }
    private bool GameNext()
    {
        if (trocandoImage) return false;
        int indiceAtual = ids.IndexOf(idAtual);
        if (indiceAtual < ids.Count - 1)
        {
            this.Dispatcher.Invoke(() => {
                trocandoImage = true;
                TransicaoImageAtalho(false);
                idAtual = (int)ids[indiceAtual + 1];
                AtalhoListar(idAtual);

                Properties.Settings.Default.IdUltimoJogo = idAtual;
                Properties.Settings.Default.Save();

                return true;
            });
        }

        return false;
    }
    private bool GamePrev()
    {
        if (trocandoImage) return false;

        int indiceAtual = ids.IndexOf(idAtual);
        if (indiceAtual > 0)
        {
            this.Dispatcher.Invoke(() => {
                trocandoImage = true;
                TransicaoImageAtalho(true);
                idAtual = (int)ids[indiceAtual - 1];
                AtalhoListar(idAtual);

                Properties.Settings.Default.IdUltimoJogo = idAtual;
                Properties.Settings.Default.Save();

                return true;
            });
        }
        return false;
    }
    // Jogo ---------------------------------------------------------------------------------------
    private void RestaurarPlayOS(object sender, EventArgs e)
    {
        this.Dispatcher.Invoke(() => {
            this.Show();
        });
        this.WindowState = WindowState.Maximized;
    }
    private void MonitoramentoDeProcesos()
    {
        timerProcessoEstabilizar = new System.Timers.Timer(5000);
        timerProcessoEstabilizar.Elapsed += VerificacaoProcessoEstabilizado;
        timerProcessoEstabilizar.AutoReset = true;
        timerProcessoEstabilizar.Enabled = true;
    }
    private void VerificacaoProcessoEstabilizado(object sender, ElapsedEventArgs e)
    {
        var processosAtuais = Process.GetProcesses();
        foreach (var processo in processosAtuais)
        {
            try
            {
                if (IgnorarProcesso(processo))
                    continue;
                if (NativeMethods.IsFullscreenWithoutBorders(processo) || NativeMethods.HasBorder(processo))
                {
                    if (jogoAberto != null)
                    {
                        jogoAberto.Exited -= AoFecharAtalho;
                    }
                    jogoAberto = processo;

                    if (!jogoAberto.HasExited)
                    {
                        try
                        {
                            jogoAberto.EnableRaisingEvents = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao definir EnableRaisingEvents: {ex.Message}");
                        }
                    }
                    jogoAberto.Exited += AoFecharAtalho;

                    PicGIFRemover();
                    MandarPraBandeja();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao acessar processo {processo.Id}: {ex.Message}");
            }
        }
    }
    private bool IgnorarProcesso(Process processo)
    {
        try
        {
            string[] processosIgnorados = { "devenv", "riot", "notepad", "chrome", "firefox", "opera", "spotify", "edge", "steam", "textinput", "code", "xbox", "dwm", "taskmgr", "protected", "ludoh", "discord", "settings", "explorer", "svchost", "dllhost", "taskhost", "service", "application", "explorer", "window", "DS4" };

            if (processosIgnorados.Any(nome => processo.ProcessName.ToLower().Contains(nome)))
                return true;

            return processo.MainWindowHandle == IntPtr.Zero;
        }
        catch
        {
            return true;
        }
    }
    private void PicGIFAbrindoJogo()
    {
        var image = new Image()
        {
            Width = 400,
            Height = 200,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Name = "GifImage"
        };

        var gifUri = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Carregando.gif"));
        var imagegif = new BitmapImage(gifUri);
        ImageBehavior.SetAnimatedSource(image, imagegif);

        mainGrid.Children.Add(image);
        Panel.SetZIndex(image, 99);
    }
    private void PicGIFRemover()
    {
        try
        {
            mainGrid.Dispatcher.Invoke(() =>
            {
                Image specificImage = mainGrid.Children.OfType<Image>().FirstOrDefault(img => img.Name == "GifImage");

                if (specificImage != null)
                {
                    mainGrid.Children.Remove(specificImage);
                }
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao excluir gif: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void FecharAtalho()
    {
        if (jogoAberto != null && !jogoAberto.HasExited)
        {
            jogoAberto.CloseMainWindow();
            if (!jogoAberto.HasExited)
            {
                jogoAberto.Kill();
            }
        }
    }
    private void AoFecharAtalho(object sender, EventArgs e)
    {
        var processo = sender as Process;
        if (processo != null)
        {
            this.Dispatcher.Invoke(() => {
                jogoAberto = null;
                this.Show();
                this.WindowState = WindowState.Maximized;

                VoltarPrograma();

                btnAbrir.Content = "Abrir";
                btnAbrir.Click -= BtnMandarPraBandeja;
                btnAbrir.Click += BtnAbrirAtalho;

                TimeSpan tempoDecorrido = DateTime.Now - processo.StartTime;
                string tempoDaSessao = tempoDecorrido.ToString(@"h'h'mm'm'");
                Atalhos.SessaoFinalizada(tempoDaSessao, idAtual);

                lblTempoSessao.Content = "Durou: " + tempoDaSessao;
            });
        }
    }
    private void PararPrograma()
    {
        PicControlON.Source = null;
        controle?.timerControle.Stop();
        controlesFuncionando = false;

        TrayIcon.Visibility = Visibility.Visible;
    }
    private void VoltarPrograma()
    {
        PicControlON.Source = new BitmapImage(Referencias.controlOn);
        controle?.timerControle.Start();
        if (controle == null) { PicControlON.Source = null; }
        controlesFuncionando = true;

        TrayIcon.Visibility = Visibility.Collapsed;
    }
    // Bandeja ------------------------------------------------------------------------------------
    private void BtnMandarPraBandeja(object sender, EventArgs e)
    {
        MandarPraBandeja();
    }
    private void MandarPraBandeja()
    {
        this.Dispatcher.Invoke(() => {
            this.Hide();

            if (timerProcessoEstabilizar != null)
            {
                timerProcessoEstabilizar.Enabled = false;
                abrindoOJogo = false;
            }

            btnAbrir.Content = "Voltar ao jogo";
            btnAbrir.Click -= BtnAbrirAtalho;
            btnAbrir.Click += BtnMandarPraBandeja;
        });
    }
    private void RestaurarClick(object sender, RoutedEventArgs e)
    {
        this.Show();
        this.WindowState = WindowState.Maximized;
    }
    private void SairClick(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }
    // Pastas -------------------------------------------------------------------------------------
    private void CarregarAtalhosPastas()
    {
        atalhosProximosPastas = Atalhos.ConsultarPastas();

        int proxPasta = pastaAtual;

        int[] atalhosProximosPastasArray = atalhosProximosPastas.Select(a => a.getIdPasta()).ToArray();
        proxPasta = AumentarIndice(proxPasta, atalhosProximosPastasArray);

        atalhoProxPasta = atalhosProximosPastas.Find(atalho => atalho.getIdPasta() == proxPasta);

        lblPastaAtual.Content = "Pasta: " + atalhosProximosPastas.Find(atalho => atalho.getIdPasta() == pastaAtual).getNomePasta();
    }
    private void Apresentacao(string nome = "", int indice = -1)
    {
        lblApresentacao.Content = nome;
        Panel.SetZIndex(recApresentacao, indice);
        Panel.SetZIndex(lblApresentacao, indice);
    }
    // Configuracoes ------------------------------------------------------------------------------
    private void TrocarMonitor(int idMonitor)
    {
        PicTransitionsCriar();
        NativeMethods.MoveToMonitor(idMonitor, this);
        Properties.Settings.Default.MonitorEmUso = idMonitor;
        Properties.Settings.Default.Save();
    }
    private void Configurar()
    {
        Configurar config = new Configurar(pastaAtual, atalhosProximos)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Name = "config"
        };

        config.OrdemAlterada += AtalhoPegarIds;
        config.AtalhoAdicionado += CarregarAtalhosPastas;
        config.PastaRenomeada += CarregarAtalhosPastas;
        config.FimConfiguracao += VoltarPrograma;
        config.MonitorAlterado += TrocarMonitor;

        mainGrid.RegisterName(config.Name, config);

        mainGrid.Children.Add(config);
        Panel.SetZIndex(config, 10);

        PararPrograma();
    }
    private void SetUAC(int enable)
    {
        if (!IsAdministrator())
        {
            MessageBox.Show("Execute o programa como administrador rra alterar o UAC.", "Permissão Necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            const string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
            {
                if (key != null)
                {
                    key.SetValue("EnableLUA", enable, RegistryValueKind.DWord);
                    MessageBox.Show($"UAC {(enable == 1 ? "ativado" : "desativado")}. Reinicie o computador para aplicar as mudanças.", "Alteração Bem-sucedida", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Não foi possível acessar o registro.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao alterar o UAC: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private bool IsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
    // Imagens ------------------------------------------------------------------------------------
    private void PicTransitionsCriar()
    {
        int widthpic = (int)mainGrid.ActualWidth;
        int heightpic = (int)mainGrid.ActualHeight;
        PicImgAtalhoProx.Margin = new Thickness(-widthpic, 0, widthpic, 0);
        PicImgAtalhoPrev.Margin = new Thickness(widthpic, 0, -widthpic, 0);
        PicImgAtalhoNovaPasta.Margin = new Thickness(0, heightpic, 0, -heightpic);
    }
    private void TransicaoImageAtalho(bool dir)
    {
        this.Dispatcher.Invoke(new Action(() =>
        {
            Image img = null;
            int num = 1;
            if (dir) { img = PicImgAtalhoProx; num = num * -1; }
            if (dir == false) { img = PicImgAtalhoPrev; }

            Atalhos atalhoProximo = atalhosProximos.Find(atalho => atalho.getOrdemAtalho() == atalhoAtual.getOrdemAtalho() + num);
            if (atalhoProximo != null) { img.Source = atalhoProximo.getImgAtalho(); }

            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = new Thickness(img.Margin.Left, 0, img.Margin.Right, 0),
                To = new Thickness(0, 0, 0, 0),
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new QuadraticEase()
            };

            animation.Completed += (s, e) => RecarregarTransicaoImageAtalho(img, num);

            img.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }));
    }
    private void TransicaoImagePasta()
    {
        this.Dispatcher.Invoke(new Action(() =>
        {
            if (atalhosProximos.Count > 1)
            {
                Apresentacao(atalhoProxPasta.getNomePasta(), 99);
                lblPastaAtual.Content = "Pasta: " + atalhoProxPasta.getNomePasta();

                if (atalhoProxPasta != null) { PicImgAtalhoNovaPasta.Source = atalhoProxPasta.getImgAtalho(); }

                ThicknessAnimation animation = new ThicknessAnimation
                {
                    From = new Thickness(0, PicImgAtalhoNovaPasta.Margin.Top, 0, PicImgAtalhoNovaPasta.Margin.Bottom),
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase()
                };

                animation.Completed += (s, e) => RecarregarTransicaoImagePasta(PicImgAtalhoNovaPasta);

                PicImgAtalhoNovaPasta.BeginAnimation(FrameworkElement.MarginProperty, animation);
            }
        }));
    }
    private void RecarregarTransicaoImageAtalho(Image img, int num)
    {
        img.BeginAnimation(FrameworkElement.MarginProperty, null);

        int tamanhopic = (int)mainGrid.ActualWidth;
        if (img == PicImgAtalhoProx) { tamanhopic = tamanhopic * -1; }

        PicImgAtalhoAtual.Source = atalhoAtual.getImgAtalho();

        img.Margin = new Thickness(tamanhopic, 0, -tamanhopic, 0);

        Atalhos atalhoAtualL = atalhosProximos.Find(atalho => atalho.getIdAtalho() == idAtual + num);
        if (atalhoAtualL != null) { img.Source = atalhoAtualL.getImgAtalho(); }

        trocandoImage = false;
    }
    private void RecarregarTransicaoImagePasta(Image img)
    {
        atalhoAtual = atalhoProxPasta;

        idAtual = atalhoAtual.getIdAtalho();

        Properties.Settings.Default.IdUltimoJogo = idAtual;
        Properties.Settings.Default.Save();

        int[] atalhosProximosPastasArray = atalhosProximosPastas.Select(a => a.getIdPasta()).ToArray();

        pastaAtual = AumentarIndice(pastaAtual, atalhosProximosPastasArray); 
        int proxPasta = pastaAtual;
        proxPasta = AumentarIndice(proxPasta, atalhosProximosPastasArray);

        Properties.Settings.Default.IdUltimaPasta = pastaAtual;
        Properties.Settings.Default.Save();

        atalhoProxPasta = atalhosProximosPastas.Find(atalho => atalho.getIdPasta() == proxPasta);

        AtalhoPegarIds();

        trocandoImage = false;

        img.BeginAnimation(FrameworkElement.MarginProperty, null);

        int tamanhopic = (int)mainGrid.ActualHeight;

        PicImgAtalhoAtual.Source = atalhoAtual.getImgAtalho();

        img.Margin = new Thickness(0, tamanhopic, 0, -tamanhopic);

        Apresentacao();
    }
    // Data e hora --------------------------------------------------------------------------------
    private void CriarRelogio()
    {
        horario = DateTime.Now;
        int segundosParaProximoMinuto = 60 - horario.Second;
        lblHoraAtual.Content = horario.ToString("HH:mm") + " h";
        lblDataAtual.Content = horario.ToString("D");

        temporizadorDoRelogio = new System.Timers.Timer(segundosParaProximoMinuto * 1000);
        temporizadorDoRelogio.Elapsed += AtualizarHorario;
        temporizadorDoRelogio.AutoReset = true;
        temporizadorDoRelogio.Enabled = true;
    }
    private void AtualizarHorario(object sender, ElapsedEventArgs e)
    {
        DateTime horarioAtual = DateTime.Now;

        if (temporizadorDoRelogio.Interval != 60000)
        {
            AtualizarTemporizador(horarioAtual);
        }

        horario = horarioAtual;

        this.Dispatcher.InvokeAsync(() => {
            lblHoraAtual.Content = horario.ToString("HH:mm") + " h";
        });
    }
    private void AtualizarTemporizador(DateTime horarioAtual)
    {
        if (!horario.ToString("HH:mm").Equals(horarioAtual.ToString("HH:mm")))
        {
            temporizadorDoRelogio.Enabled = false;

            temporizadorDoRelogio = new System.Timers.Timer(60000);
            temporizadorDoRelogio.Elapsed += AtualizarHorario;
            temporizadorDoRelogio.AutoReset = true;
            temporizadorDoRelogio.Enabled = true;
        }
    }
    // Controles ----------------------------------------------------------------------------------
    private void InicializarControle()
    {
        DirectInput _directInput = new DirectInput();
        var joystickGuid = Guid.Empty;

        foreach (var deviceInstance in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly))
        {
            joystickGuid = deviceInstance.InstanceGuid;
        }

        if (joystickGuid == Guid.Empty)
        {
            return;
        }

        Joystick _joystick = new Joystick(_directInput, joystickGuid);
        _joystick.Acquire();

        if (controle != null) LimparComandos();
        controle = new Controle();
        controle = new Controle(_joystick);
        DefinirComandos();
    }
    private void LimparComandos()
    {
        controle.controlDisconnect -= TirarIconControle;
        controle.btnX -= BtnX;
        controle.btnB -= BtnO;
        controle.moveLeft -= MoveLeft;
        controle.moveRight -= MoveRight;
        controle.moveDown -= MoveDown;
        controle.moveUp -= MoveUp;
    }
    private void DefinirComandos()
    {
        this.Dispatcher.Invoke(() => { PicControlON.Source = new BitmapImage(Referencias.controlOn); });
        controle.controlDisconnect += TirarIconControle;
        controle.btnX += BtnX;
        controle.btnB += BtnO;
        controle.moveLeft += MoveLeft;
        controle.moveRight += MoveRight;
        controle.moveDown += MoveDown;
        controle.moveUp += MoveUp;
        VisualizarEscolhaViaControle();

        if (controlesFuncionando == false)
        { controle?.timerControle.Stop(); PicControlON.Source = null; }
    }
    private void TirarIconControle() { this.Dispatcher.Invoke(() => { PicControlON.Source = null; }); }
    private void MoveRight() { if (appsOcultos) { GameNext(); } else { appAtual += 1; if (appAtual >= appCount) { appAtual = appCount - 1; } VisualizarEscolhaViaControle(); } }
    private void MoveLeft() { if (appsOcultos) { GamePrev(); } else { appAtual -= 1; if (appAtual <= 0) { appAtual = 0; } VisualizarEscolhaViaControle(); } }
    private void MoveDown() { if (appsOcultos) { TransicaoImagePasta(); } else { appsOcultos = false; ToggleApps(null); } }
    private void MoveUp() { appsOcultos = true; ToggleApps(null); }
    private void BtnX() { if (appsOcultos) { if (abrindoOJogo == false) { BtnAbrirAtalho(null, null); } } else { BtnAbrirAplicativos(null, EventArgs.Empty); } }
    private void BtnO() { this.Dispatcher.Invoke(() => { this.Close(); }); }
}