using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
    private int idAtual = 2;
    private bool jogoEmUso = false;
    private bool trocandoImage = false;
    // Bandeja ========================
    //private NotifyIcon notifyIcon;
    private bool abrindoOJogo;
    // Painel APPS ====================
    private bool appsOcultos = true;
    int heightPnlApps;
    // Temporizadores =================
    private static System.Timers.Timer temporizadorDoRelogio, timerProcessoEstabilizar;
    private static DateTime horario;
    private SynchronizationContext _syncContext;
    // Controle =======================
    private DirectInput _directInput;
    private Joystick _joystick;
    private System.Timers.Timer timerControle;
    private IntPtr _notificationHandle;
    private float appAtual = 0, appCount = 0;
    private List<int> idsApps = new List<int>();
    private Dictionary<string, DateTime> _cooldowns = new Dictionary<string, DateTime>();
    private TimeSpan _cooldownTime = TimeSpan.FromMilliseconds(300);
    // Hotkeys e DLL ==================
    private static readonly Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030"); // HID class GUID
    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnregisterDeviceNotification(IntPtr Handle);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    const int GWL_STYLE = -16;
    const int WS_BORDER = 0x00800000;
    const int WS_CAPTION = 0x00C00000;
    const int WS_THICKFRAME = 0x00040000;

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private const int WM_DEVICECHANGE = 0x0219;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
    private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
    public MainWindow()
    {
        InitializeComponent();

        _syncContext = SynchronizationContext.Current;

        AtalhoPegarIds();

        BitmapImage vinhetaImage = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Vinheta.png")));
        PicImgVinheta.Source = vinhetaImage;

        DefinirGatilhos();

        CriarRelogio();

        var closeWindowCommand = new RelayCommand(CloseWindow);
        var closeGameCommand = new RelayCommand(CloseGame);
        var RestaurarCommand = new RelayCommand(RestaurarApp);

        this.InputBindings.Add(new KeyBinding(closeWindowCommand, new KeyGesture(System.Windows.Input.Key.Q, ModifierKeys.Control | ModifierKeys.Alt)));
        this.InputBindings.Add(new KeyBinding(closeGameCommand, new KeyGesture(System.Windows.Input.Key.NumPad5, ModifierKeys.Alt)));
        this.InputBindings.Add(new KeyBinding(RestaurarCommand, new KeyGesture(System.Windows.Input.Key.NumPad5, ModifierKeys.Control)));
    }
    private void CloseWindow()
    {
        this.Close();
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
        this.Topmost = true; // Garante que fique sobre todas as janelas
        this.Focus();
        RestaurarPlayOS(null, null);
        this.Topmost = false;
    }

    // Verificar jogo Maximizado //

    [StructLayout(LayoutKind.Sequential)]
    private struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;
    public static bool IsFullscreenWithoutBorders(Process processo)
    {
        IntPtr hwnd = FindWindow(null, processo.MainWindowTitle);
        if (hwnd != IntPtr.Zero)
        {
            RECT rect;
            GetWindowRect(hwnd, out rect);

            // Obter dimensões da tela
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            if (rect.right - rect.left >= screenWidth && rect.bottom - rect.top >= screenHeight)
            {
                return true;
            }
        }
        return false;
    }
    public static bool HasBorder(Process processo)
    {
        IntPtr hwnd = FindWindow(null, processo.MainWindowTitle);
        if (hwnd != IntPtr.Zero)
        {
            int style = GetWindowLong(hwnd, GWL_STYLE);
            return (style & (WS_BORDER | WS_CAPTION | WS_THICKFRAME)) != 0;
        }
        return false;
    }

    // Verificar Conexão de algum controle //

    [StructLayout(LayoutKind.Sequential)]
    private struct DEV_BROADCAST_HDR
    {
        public int dbch_size;
        public int dbch_devicetype;
        public int dbch_reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        public Guid dbcc_classguid;
        public short dbcc_name;
    }
    private void RegisterForDeviceNotifications()
    {
        var dbi = new DEV_BROADCAST_DEVICEINTERFACE
        {
            dbcc_size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
            dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
            dbcc_classguid = GUID_DEVINTERFACE_HID
        };

        IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(dbi));
        Marshal.StructureToPtr(dbi, buffer, true);

        IntPtr windowHandle = new WindowInteropHelper(this).Handle; // Obtém o HWND da janela WPF
        _notificationHandle = RegisterDeviceNotification(windowHandle, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);

        Marshal.FreeHGlobal(buffer);

        if (_notificationHandle == IntPtr.Zero)
        {
            throw new Exception("Falha ao registrar notificações de dispositivo.");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_DEVICECHANGE)
        {
            int wParamInt = wParam.ToInt32();
            if (wParamInt == DBT_DEVICEARRIVAL)
            {
                DetectJoystick(lParam);
            }
            else if (wParamInt == DBT_DEVICEREMOVECOMPLETE)
            {
                PicControlON.Source = null;
                timerControle.Stop();
            }
        }

        return IntPtr.Zero;
    }

    // Substituir acao das teclas //
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (jogoEmUso == false) {
            if (e.Key == System.Windows.Input.Key.Left)
            {
                GamePrev();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Right)
            {
                GameNext();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                /*appsOcultos = false;
                pnlAppTransition.Start();
                heightPnlApps = pnlApps.Size.Height;
                e.Handled = true;  */
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                /*appsOcultos = true;
                pnlAppTransition.Start();
                heightPnlApps = pnlApps.Size.Height;
                e.Handled = true; */
            }
            else if (e.Key == System.Windows.Input.Key.Space)
            {
                BtnAbrirAtalho(null, null);
                e.Handled = true;
            }
        }
    }
    private void DefinirGatilhos()
    {
        this.Loaded += AoCarregar;

        btnAbrir.Click += BtnAbrirAtalho;
        btnFechar.Click += (s, e) => this.Close();
        //btnEditarAtalho.Click += BtnEditarAtalho;
        /*btnAdicionar.Click += (s, e) => Cadastrar();*/
        btnDeletarAtalho.Click += BtnDeletarAtalho;

        btnNextAtalho.Click += (s, e) => GameNext();
        btnPrevAtalho.Click += (s, e) => GamePrev();
    
        /*picPuxarApps.Click += BtnAlternarAppsOcultos;

        pnlAppTransition.Tick += TransicaoAppsOcultos;*/
    }
    private void BtnAbrirAtalho(object sender, EventArgs e)
    {
        try
        {
            abrindoOJogo = true;
            string caminho = atalhoAtual.getCaminhoAtalho();
            string permissao = "runas";
            string argumentacao = atalhoAtual.getParametroAtalho();
            string diretorioTrabalho = System.IO.Path.GetDirectoryName(caminho);
            if (caminho.Contains("steam:"))
            {
                permissao = "";
            }
            if (caminho.Contains("epicgames"))
            {
                argumentacao = caminho;
                caminho = GetEpicGames();
                diretorioTrabalho = System.IO.Path.GetDirectoryName(caminho);
                AbrirEpicGames(caminho, diretorioTrabalho, argumentacao, permissao);
            }
            else
            {
                AbrirAtalho(caminho, diretorioTrabalho, argumentacao, permissao);
            }
            PicGIFAbrindoJogo();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro no botao de abrir atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void RestaurarPlayOS(object sender, EventArgs e)
    {
        this.Dispatcher.Invoke(() => {
            this.Show();
        });
        this.WindowState = WindowState.Maximized;
    }
    private void MonitoramentoDeProcesos()
    {
        timerProcessoEstabilizar = new System.Timers.Timer(5000); //Tempo maximo de monitoramento de processos
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

                if (IsFullscreenWithoutBorders(processo) || HasBorder(processo))
                {
                    if (jogoAberto != null)
                    {
                        jogoAberto.Exited -= AoFecharAtalho;
                    }
                    jogoAberto = processo;
                    jogoAberto.EnableRaisingEvents = true;
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
        try {
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
                //RemoverNotificacao();

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
    private void MandarPraBandeja()
    {
        this.Dispatcher.Invoke(() => {
            this.Hide();
        });
        /*CriarNotificacao();
        notifyIcon.ShowBalloonTip(1000, "Aplicativo Minimizado", "Clique para restaurar", ToolTipIcon.Info);*/
        timerProcessoEstabilizar.Enabled = false;
        abrindoOJogo = false;

        PararPrograma();

        btnAbrir.Content = "Voltar ao jogo";
        btnAbrir.Click -= BtnAbrirAtalho;
        btnAbrir.Click += BtnMandarPraBandeja;
    }
    private void PararPrograma()
    {
        timerControle.Stop();
        PicControlON.Source = null;
        jogoEmUso = true;
    }
    private void VoltarPrograma()
    {
        timerControle.Start();
        PicControlON.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ControlON.png")));
        jogoEmUso = false;
    }
    private void BtnMandarPraBandeja(object sender, EventArgs e)
    {
        MandarPraBandeja();
    }

    private bool IgnorarProcesso(Process processo)
    {
        try
        {
            string[] processosIgnorados = { "dev", "chrome", "firefox", "opera", "spotify", "edge", "steam", "textinput", "code", "xbox", "dwm", "taskmgr", "protected", "ludoh", "discord", "settings", "explorer", "svchost", "dllhost", "taskhost", "service", "application", "explorer" };

            if (processosIgnorados.Any(nome => processo.ProcessName.ToLower().Contains(nome)))
                return true;

            return processo.MainWindowHandle == IntPtr.Zero;
        }
        catch
        {
            return true;
        }
    }
    private string GetEpicGames()
    {
        string registryKey = @"HKEY_CLASSES_ROOT\com.epicgames.launcher\shell\open\command";

        string command = (string)Registry.GetValue(registryKey, null, string.Empty);

        if (!string.IsNullOrEmpty(command))
        {
            string[] parts = command.Split(" %");

            string commandToRun = parts[0].Trim();

            return commandToRun;
        }

        return null;
    }
    private void AbrirEpicGames(string caminho, string diretorioTrabalho, string argumentacao, string permissao)
    {
        this.Topmost = true;
        Process.Start(caminho);

        System.Timers.Timer temporizadorEpicAberta = new System.Timers.Timer(7000);
        temporizadorEpicAberta.Elapsed += (s, e) =>
        {
            var processosAtuais = Process.GetProcesses();
            Process epic = null;
            foreach (var processo in processosAtuais)
            {
                if (processo.ProcessName.ToLower().Contains("epicgameslauncher"))
                {
                    epic = processo;
                    break;
                }
            }
            if (epic != null)
            {
                temporizadorEpicAberta.Enabled = false;
                this.Topmost = false;
                epic.CloseMainWindow();
                AbrirAtalho(caminho, diretorioTrabalho, argumentacao, permissao);
            }
            else
            {
                MessageBox.Show("Epic Games Launcher não foi encontrado.");
            }
        };

        temporizadorEpicAberta.Enabled = true;

    }
    private void AbrirAtalho(string caminho, string diretorioTrabalho, string argumentacao, string permissao)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = caminho,
                WorkingDirectory = diretorioTrabalho,
                Arguments = argumentacao,
                Verb = permissao,
                UseShellExecute = true
            };
            jogoAberto = Process.Start(psi);

            string inicioSessao = DateTime.Now.ToString("dd/MM/yy - HH:mm");
            Atalhos.SessaoIniciada(inicioSessao, idAtual);
            lblDataSessao.Content = "Data: " + inicioSessao;

            MonitoramentoDeProcesos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao abrir o atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void AoCarregar(object sender, EventArgs e)
    {
        if (atalhoAtual != null)
        {
            PicImgAtalhoAtual.Source = atalhoAtual.getImgAtalho();
            PicDefinirCorDeFundo(atalhoAtual.getImgAtalho());
            PicTransitionsCriar();

            TransicaoImageAtalho(false);
            TransicaoImageAtalho(true);
        }
        InicializarControle();
        RegisterForDeviceNotifications();
        IniciarTimerMonitoramentoControle();

        var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(WndProc);

        /*picPuxarApps.Image = Properties.Resources.PicAppsShow;
        PicArredondarBordas(picPuxarApps, 0, 0, 30, 30);
        PegarApps();*/
    }
    private void PicTransitionsCriar()
    {
        int tamanhopic = (int)PicImgAtalhoAtual.ActualWidth;
        Image imgL = new Image()
        {
            Width = PicImgAtalhoAtual.Width,
            Height = PicImgAtalhoAtual.Height,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(tamanhopic, 0, -tamanhopic, 0),
            Name = "picImgLeft"
        };
        Image imgR = new Image()
        {
            Width = PicImgAtalhoAtual.Width,
            Height = PicImgAtalhoAtual.Height,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(-tamanhopic, 0, tamanhopic, 0),
            Name = "picImgRight"
        };

        mainGrid.Children.Add(imgL);
        mainGrid.Children.Add(imgR);
        Panel.SetZIndex(imgL, 0);
        Panel.SetZIndex(imgR, 0);
    }
    private void TransicaoImageAtalho(bool dir)
    {
        this.Dispatcher.Invoke(new Action(() =>
        {
            Image img = null;
            int num = 1;
            if (dir) { img = mainGrid.Children.OfType<Image>().FirstOrDefault(img => img.Name == "picImgRight"); num = num * -1; }
            if (dir == false) { img = mainGrid.Children.OfType<Image>().FirstOrDefault(img => img.Name == "picImgLeft"); }

            Atalhos atalhoAtualL = atalhosProximos.Find(atalho => atalho.getIdAtalho() == idAtual + num);
            if (atalhoAtualL != null) { img.Source = atalhoAtualL.getImgAtalho(); }

            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = new Thickness(img.Margin.Left, 0, img.Margin.Right, 0), // Margem inicial
                To = new Thickness(0, 0, 0, 0),         // Margem final
                Duration = TimeSpan.FromSeconds(0.2),     // Duração da animação
                EasingFunction = new QuadraticEase()    // Adiciona um efeito de suavização
            };

            animation.Completed += (s, e) => RecarregarTransicaoImageAtalho(img, num);

            img.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }));
    }
    private void RecarregarTransicaoImageAtalho(Image img, int num)
    {
        img.BeginAnimation(FrameworkElement.MarginProperty, null);

        int tamanhopic = (int)PicImgAtalhoAtual.ActualWidth;
        if (img.Name.Equals("picImgRight")) { tamanhopic = tamanhopic * -1; }

        PicImgAtalhoAtual.Source = atalhoAtual.getImgAtalho();

        img.Margin = new Thickness(tamanhopic, 0, -tamanhopic, 0);

        Atalhos atalhoAtualL = atalhosProximos.Find(atalho => atalho.getIdAtalho() == idAtual + num);
        if (atalhoAtualL != null) { img.Source = atalhoAtualL.getImgAtalho(); }
        trocandoImage = false;
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
    private bool GameNext()
    {
        if (trocandoImage) return false;

        int indiceAtual = ids.IndexOf(idAtual);
        if (indiceAtual < ids.Count - 1)
        {
            trocandoImage = true;
            TransicaoImageAtalho(false);
            idAtual = (int)ids[indiceAtual + 1];
            AtalhoListar(idAtual);
            return true;
        }

        return false;
    }
    private bool GamePrev()
    {
        if (trocandoImage) return false;

        int indiceAtual = ids.IndexOf(idAtual);
        if (indiceAtual > 0)
        {
            trocandoImage = true;
            TransicaoImageAtalho(true);
            idAtual = (int)ids[indiceAtual - 1];
            AtalhoListar(idAtual);
            return true;
        }
        return false;
    }
    private void AtalhoPegarIds()
    {
        ids.Clear();
        try
        {
            ids = Atalhos.ConsultarIDs();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao listar ids: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        if (ids.Count > 0)
        {
            if (!ids.Contains(idAtual)) { idAtual = (int)ids[0]; }
            atalhosProximos = Atalhos.ConsultarAtalhos(ids);
            AtalhoListar(idAtual);
        }
        else
        {
            //Cadastrar();
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

            var clip = new RectangleGeometry(new Rect(0, 0, 200, 200), 20, 20);
            picIconAtalho.Clip = clip;

            picIconAtalho.Source = atalhoAtual.getIconeAtalho();
        });
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

        this.Dispatcher.Invoke(() => {
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
    private void SetUAC(int enable)
    {
        if (!IsAdministrator())
        {
            MessageBox.Show("Execute o programa como administrador para alterar o UAC.", "Permissão Necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    // Controles ----------------------------------------------------------------------------------
    private void IniciarTimerMonitoramentoControle()
    {
        timerControle = new System.Timers.Timer { Interval = 16 };
        timerControle.Elapsed += ControleInputs;
        timerControle.Start();

        PicControlON.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ControlON.png")));
    }
    private void InicializarControle()
    {
        _directInput = new DirectInput();
        var joystickGuid = Guid.Empty;

        foreach (var deviceInstance in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly))
        {
            joystickGuid = deviceInstance.InstanceGuid;
        }

        if (joystickGuid == Guid.Empty)
        {
            return;
        }

        _joystick = new Joystick(_directInput, joystickGuid);
        _joystick.Acquire();
    }
    private void DetectJoystick(IntPtr lParam)
    {
        var hdr = Marshal.PtrToStructure<DEV_BROADCAST_HDR>(lParam);

        if (hdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
        {
            var deviceInterface = Marshal.PtrToStructure<DEV_BROADCAST_DEVICEINTERFACE>(lParam);
            if (deviceInterface.dbcc_classguid == GUID_DEVINTERFACE_HID)
            {
                foreach (var deviceInstance in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly))
                {
                    var joystickGuid = deviceInstance.InstanceGuid;

                    if (joystickGuid != Guid.Empty)
                    {
                        _joystick = new Joystick(_directInput, joystickGuid);
                        _joystick.Acquire();

                        IniciarTimerMonitoramentoControle();
                    }
                }
            }
        }
    }
    private void ControleInputs(object sender, EventArgs e)
    {
        try
        {
            if (_joystick == null) {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        PicControlON.Source = null;
                    });
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("A operação foi cancelada.");
                }
                return;
            }

            _joystick.Poll();
            var state = _joystick.GetCurrentState();

            if (state == null) return;

            // Obter valores dos botoes
            var buttons = state.Buttons;

            // Obter valores das setas
            var dPad = state.PointOfViewControllers[0];

            // Obter valores dos analógicos
            var xAnalog = state.X;
            var yAnalog = state.Y;
            var zAnalog = state.Z;
            var rzAnalog = state.RotationZ;

            // Normalizar valores (-1.0 a 1.0)
            float leftX = (xAnalog - 32767f) / 32767f;
            float leftY = (yAnalog - 32767f) / 32767f;
            float rightX = (zAnalog - 32767f) / 32767f;
            float rightY = (rzAnalog - 32767f) / 32767f;

            // Executar métodos dependendo dos valores
            if ((leftX > 0.5 || dPad == 9000) && CanExecute("MoveRight")) MoveRight();
            else if ((leftX < -0.5 || dPad == 27000) && CanExecute("MoveLeft")) MoveLeft();

            if ((leftY > 0.5 || dPad == 18000) && CanExecute("MoveUp")) MoveUp();
            else if ((leftY < -0.5 || dPad == 0) && CanExecute("MoveDown")) MoveDown();

            if (buttons.Length > 1 && buttons[1] && CanExecute("BtnX")) BtnX();
            if (buttons.Length > 1 && buttons[2] && CanExecute("BtnO")) BtnO();
        }
        catch (SharpDX.SharpDXException)
        {
            timerControle.Stop();
            this.Dispatcher.Invoke(() => {
                PicControlON.Source = null;
            });
        }
    }
    private bool CanExecute(string actionName)
    {
        if (!_cooldowns.ContainsKey(actionName) || DateTime.Now - _cooldowns[actionName] >= _cooldownTime)
        {
            _cooldowns[actionName] = DateTime.Now;
            return true;
        }
        return false;
    }
    private void MoveRight() { if (appsOcultos) { GameNext(); } else { appAtual += 0.2f; if (appAtual >= appCount) { appAtual = appCount - 1; } /*VisualizarEscolhaViaControle();*/ } }
    private void MoveLeft() { if (appsOcultos) { GamePrev(); } else { appAtual -= 0.2f; if (appAtual <= 0) { appAtual = 0; } /*VisualizarEscolhaViaControle();*/ } }
    private void MoveUp() { appsOcultos = true; /*pnlAppTransition.Start(); heightPnlApps = pnlApps.Size.Height;*/ }
    private void MoveDown() { appsOcultos = false; /*pnlAppTransition.Start(); heightPnlApps = pnlApps.Size.Height;*/ }
    private void BtnX() { if (appsOcultos) { if (abrindoOJogo == false) { BtnAbrirAtalho(null, null); } } else { /*BtnAbrirAplicativos(null, null);*/ } }
    private void BtnO() { this.Dispatcher.Invoke(() => { this.Close(); }); }
}
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object parameter)
    {
        _execute();
    }
}