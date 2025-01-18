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
using LudoHive.Telas.Controles;
using LudoHive.Telas;

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
    private Controle controle;
    private IntPtr _notificationHandle;
    private float appAtual = 0, appCount = 0;
    private List<int> idsApps = new List<int>();
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
    private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
    private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
    public MainWindow()
    {
        InitializeComponent();

        _syncContext = SynchronizationContext.Current;

        AtalhoPegarIds();

        PicImgVinheta.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Vinheta.png")));
        picImgAppsOcultos.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PicAppsShow.png")));

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
                controle = new Controle(lParam);
                DefinirComandos();
            }
        }

        return IntPtr.Zero;
    }
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

        controle = new Controle(_joystick);
        DefinirComandos();
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
                appsOcultos = false;
                ToggleApps();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                appsOcultos = true;
                ToggleApps();
                e.Handled = true; 
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
        btnEditarAtalho.Click += BtnEditarAtalho;
        btnAdicionarAtalho.Click += (s, e) => Cadastrar();
        btnDeletarAtalho.Click += BtnDeletarAtalho;

        btnNextAtalho.Click += (s, e) => GameNext();
        btnPrevAtalho.Click += (s, e) => GamePrev();

        gdPnlApps.MouseDown += (s, e) => ToggleApps();
    }
    private void PegarApps()
    {
        gdApps.Children.Clear();
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
        int tamanhoApp = 75;
        int margemApps = 48;
        int meiaTela = (int)gdApps.ActualWidth / 2;
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
    private Panel CriacaoApp(Aplicativos appEmUso, Point localDoAplicativo)
    {
        Image picAppIcon = new Image
        {
            Source = appEmUso.getIconeAplicativo(),
            Location = new Point(0, 0),
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Name = "picIconApp_" + appEmUso.getNomeAplicativo() + ">" + appEmUso.getIdAplicativo(),
            Size = new Size(75, 75),
            SizeMode = PictureBoxSizeMode.Zoom,
            Clip = new RectangleGeometry(new Rect(0, 0, 200, 200), 20, 20)
        };
        //
        Label lblAppNome = new Label
        {
            Font = new Font("Arial", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
            Anchor = AnchorStyles.Top,
            BackColor = Color.Transparent,
            Location = new Point(0, 75),
            Margin = new Padding(0),
            Name = "lblNomeApp_" + appEmUso.getNomeAplicativo() + ">" + appEmUso.getIdAplicativo(),
            Size = new Size(75, 48),
            TabIndex = 22,
            Text = appEmUso.getNomeAplicativo(),
            TextAlign = ContentAlignment.MiddleCenter
        };
        //
        Panel pnlBackground = new Panel
        {
            Location = localDoAplicativo,
            Margin = new Padding(0),
            Name = "pnl_" + appEmUso.getNomeAplicativo() + ">" + appEmUso.getIdAplicativo(),
            Size = new Size(75, 123),
            TabIndex = 21
        };
        pnlBackground.Controls.Add(picAppIcon);
        pnlBackground.Controls.Add(lblAppNome);

        Color cor = Color.FromArgb(67, 0, 0, 0);

        pnlBackground.MouseEnter += (s, e) => pnlBackground.BackColor = cor;
        lblAppNome.MouseEnter += (s, e) => pnlBackground.BackColor = cor;
        picAppIcon.MouseEnter += (s, e) => pnlBackground.BackColor = cor;
        pnlBackground.MouseLeave += (s, e) => pnlBackground.BackColor = Color.Transparent;
        lblAppNome.MouseLeave += (s, e) => pnlBackground.BackColor = Color.Transparent;
        picAppIcon.MouseLeave += (s, e) => pnlBackground.BackColor = Color.Transparent;

        picAppIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                CriarPopup(appEmUso.getIdAplicativo());
            }
            if (e.Button == MouseButtons.Left)
            {
                BtnAbrirAplicativos(s, e);
            }
        };
        lblAppNome.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                CriarPopup(appEmUso.getIdAplicativo());
            }
            if (e.Button == MouseButtons.Left)
            {
                BtnAbrirAplicativos(s, e);
            }
        };

        return pnlBackground;
    }
    private void ToggleApps()
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (appsOcultos == false)
            {
                ThicknessAnimation animation = new ThicknessAnimation
                {
                    From = new Thickness(6, gdPnlApps.Margin.Top, 0, 0),
                    To = new Thickness(0, -215, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new QuadraticEase()
                };

                animation.Completed += (s, e) =>
                {
                    picImgAppsOcultos.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PicAppsShow.png")));
                    appsOcultos = true;
                };

                gdPnlApps.BeginAnimation(FrameworkElement.MarginProperty, animation);
            }
            else
            {
                ThicknessAnimation animation = new ThicknessAnimation
                {
                    From = new Thickness(6, gdPnlApps.Margin.Top, 0, 0),
                    To = new Thickness(0, 0, 0, 0),
                    Duration = TimeSpan.FromSeconds(0.2),
                    EasingFunction = new QuadraticEase()
                };

                animation.Completed += (s, e) =>
                {
                    picImgAppsOcultos.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "PicAppsHide.png")));
                    appsOcultos = false;
                };

                gdPnlApps.BeginAnimation(FrameworkElement.MarginProperty, animation);
            }
        });
    }
    private void Cadastrar()
    {
        Cadastrar cad = new Cadastrar()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        cad.atalhoCadastrado += (s, e) => AtalhoPegarIds();

        mainGrid.Children.Add(cad);
        Panel.SetZIndex(cad, 10);
    }
    private void BtnEditarAtalho(object sender, EventArgs e)
    {
        Cadastrar cadEdicao = new Cadastrar(idAtual, 0)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        cadEdicao.atalhoCadastrado += (s, e) => AtalhoPegarIds();

        mainGrid.Children.Add(cadEdicao);
        Panel.SetZIndex(cadEdicao, 10);
    }
    private void BtnAbrirAtalho(object sender, EventArgs e)
    {
        try
        {
            this.Dispatcher.Invoke(new Action(() =>
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
            }));
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

            if (timerProcessoEstabilizar != null)
            {
                timerProcessoEstabilizar.Enabled = false;
                abrindoOJogo = false;
            }
            
            PararPrograma();

            btnAbrir.Content = "Voltar ao jogo";
            btnAbrir.Click -= BtnAbrirAtalho;
            btnAbrir.Click += BtnMandarPraBandeja;
        });
        /*CriarNotificacao();
        notifyIcon.ShowBalloonTip(1000, "Aplicativo Minimizado", "Clique para restaurar", ToolTipIcon.Info);*/
    }
    private void PararPrograma()
    {
        controle?.timerControle.Stop();
        PicControlON.Source = null;
        jogoEmUso = true;
    }
    private void VoltarPrograma()
    {
        controle?.timerControle.Start();
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
            string[] processosIgnorados = { "dev", "notepad", "chrome", "firefox", "opera", "spotify", "edge", "steam", "textinput", "code", "xbox", "dwm", "taskmgr", "protected", "ludoh", "discord", "settings", "explorer", "svchost", "dllhost", "taskhost", "service", "application", "explorer" };

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
            this.Dispatcher.Invoke(new Action(() =>
            {
                lblDataSessao.Content = "Data: " + inicioSessao;
            }));

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

        var rectGeometry = new RectangleGeometry
        {
            Rect = new Rect(0, -30, mainGrid.ActualWidth, 245),
            RadiusX = 20,
            RadiusY = 20
        };
        recPnlApp.Clip = rectGeometry;

        var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        source.AddHook(WndProc);

        /*picPuxarApps.Image = Properties.Resources.PicAppsShow;
        PicArredondarBordas(picPuxarApps, 0, 0, 30, 30);
        PegarApps();*/
    }
    private void PicTransitionsCriar()
    {
        int tamanhopic = (int)PicImgAtalhoAtual.ActualWidth;
        PicImgAtalhoProx.Margin = new Thickness(-tamanhopic, 0, tamanhopic, 0);
        PicImgAtalhoPrev.Margin = new Thickness(tamanhopic, 0, -tamanhopic, 0);
    }
    private void TransicaoImageAtalho(bool dir)
    {
        this.Dispatcher.Invoke(new Action(() =>
        {
            Image img = null;
            int num = 1;
            if (dir) { img = PicImgAtalhoProx; num = num * -1; }
            if (dir == false) { img = PicImgAtalhoPrev; }

            Atalhos atalhoAtualL = atalhosProximos.Find(atalho => atalho.getIdAtalho() == idAtual + num);
            if (atalhoAtualL != null) { img.Source = atalhoAtualL.getImgAtalho(); }

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
    private void RecarregarTransicaoImageAtalho(Image img, int num)
    {
        img.BeginAnimation(FrameworkElement.MarginProperty, null);

        int tamanhopic = (int)PicImgAtalhoAtual.ActualWidth;
        if (img == PicImgAtalhoProx) { tamanhopic = tamanhopic * -1; }

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
            this.Dispatcher.Invoke(() => {
                trocandoImage = true;
                TransicaoImageAtalho(false);
                idAtual = (int)ids[indiceAtual + 1];
                AtalhoListar(idAtual);
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
                return true;
            });
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
    private void DefinirComandos()
    {
        this.Dispatcher.Invoke(() => { PicControlON.Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ControlON.png"))); });
        controle.controlDisconnect += (s, e) => this.Dispatcher.Invoke(() => { PicControlON.Source = null; });
        controle.btnX += (s, e) => BtnX();
        controle.btnQ += (s, e) => BtnO();
        controle.moveLeft += (s, e) => MoveLeft();
        controle.moveRight += (s, e) => MoveRight();
        controle.moveDown += (s, e) => MoveDown();
        controle.moveUp += (s, e) => MoveUp();
    }
    private void MoveRight() { if (appsOcultos) { GameNext(); } else { appAtual += 0.2f; if (appAtual >= appCount) { appAtual = appCount - 1; } /*VisualizarEscolhaViaControle();*/ } }
    private void MoveLeft() { if (appsOcultos) { GamePrev(); } else { appAtual -= 0.2f; if (appAtual <= 0) { appAtual = 0; } /*VisualizarEscolhaViaControle();*/ } }
    private void MoveUp() { appsOcultos = true; /*pnlAppTransition.Start(); heightPnlApps = pnlApps.Size.Height;*/ }
    private void MoveDown() { appsOcultos = false; /*pnlAppTransition.Start(); heightPnlApps = pnlApps.Size.Height;*/ }
    private void BtnX() { if (appsOcultos) { if (abrindoOJogo == false) { BtnAbrirAtalho(null, null); } } else { /*BtnAbrirAplicativos(null, null);*/ } }
    private void BtnO() { this.Dispatcher.Invoke(() => { this.Close(); }); }
}