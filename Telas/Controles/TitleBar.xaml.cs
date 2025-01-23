using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para TitleBar.xam
    /// </summary>
    public partial class TitleBar : UserControl
    {
        private bool _resize = true;
        private bool _visivelMaximizado = true;
        private int _widthFixed = 397;
        private int _heightTitle = 38;
        private int _posicao = 1;
        private string _title = "TITLE";
        private Color _colorBar = Color.FromArgb(255, 33, 33, 33);
        private bool _canClose = true;
        private bool _canMax = true;
        private bool _canMin = true;
        private bool _btnClose = true;
        private bool _btnMax = true;
        private bool _btnMin = true;
        private ImageSource _imgClose;
        private ImageSource _imgMax;
        private ImageSource _imgMin;
        private ImageSource _imgIcone;
        public Action ActClose;
        public Action ActMax;
        public Action ActMin;
        public bool Resize
        {
            get => _resize;
            set
            {
                _resize = value;
            }
        }
        public bool VisivelMaximizado
        {
            get => _visivelMaximizado;
            set
            {
                _visivelMaximizado = value;
            }
        }
        public int WidthFixed
        {
            get => _widthFixed;
            set
            {
                _widthFixed = value;
                if (_widthFixed > this.MinWidth)
                {
                    this.Width = _widthFixed;
                    CriarClip(titleGrid, HeightTitle, WidthFixed, 25, false, false);
                }
                else
                {
                    this.Width = Double.NaN;
                    titleGrid.Clip = null;
                }
            }
        }
        public int HeightTitle
        {
            get => _heightTitle;
            set
            {
                _heightTitle = value;
                this.Height = _heightTitle;
                picIconWindow.Height = _heightTitle;
                picCloseWindow.Height = _heightTitle;
                picMaximizeWindow.Height = _heightTitle;
                picMinimizeWindow.Height = _heightTitle;
                ToogleImg();
            }
        }
        public int Posicao
        {
            get => _posicao;
            set
            {
                _posicao = value;
                switch (_posicao)
                {
                    case 1:
                        lblTitleWindow.HorizontalAlignment = HorizontalAlignment.Left;
                        break;
                    case 2:
                        lblTitleWindow.HorizontalAlignment = HorizontalAlignment.Center;
                        break;
                    case 3:
                        lblTitleWindow.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                }
                ToogleImg();
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                lblTitleWindow.Content = _title;
            }
        }
        public Color ColorTitle
        {
            get => _colorBar;
            set
            {
                _colorBar = value;
                titleGrid.Background = new SolidColorBrush(_colorBar);
            }
        }
        public bool CanClose
        {
            get => _canClose;
            set => _canClose = value;
        }
        public bool CanMax
        {
            get => _canMax;
            set => _canMax = value;
        }
        public bool CanMin
        {
            get => _canMin;
            set => _canMin = value;
        }
        public bool BtnClose
        {
            get => _btnClose;
            set
            {
                _btnClose = value;
                if (_btnClose) { picCloseWindow.Visibility = Visibility.Visible; }
                else { picCloseWindow.Visibility = Visibility.Collapsed; }
                ToogleImg();
            }
        }
        public bool BtnMax
        {
            get => _btnMax;
            set
            {
                _btnMax = value;
                if (_btnMax) { picMaximizeWindow.Visibility = Visibility.Visible; }
                else { picMaximizeWindow.Visibility = Visibility.Collapsed; }
                ToogleImg();
            }
        }
        public bool BtnMin
        {
            get => _btnMin;
            set
            {
                _btnMin = value;
                if (_btnMin) { picMinimizeWindow.Visibility = Visibility.Visible; }
                else { picMinimizeWindow.Visibility = Visibility.Collapsed; }
                ToogleImg();
            }
        }
        public ImageSource ImgClose
        {
            get => _imgClose;
            set
            {
                _imgClose = value;
                picCloseWindow.Source = _imgClose;
            }
        }
        public ImageSource ImgMax
        {
            get => _imgMax;
            set
            {
                _imgMax = value;
                picMaximizeWindow.Source = _imgMax;
            }
        }
        public ImageSource ImgMin
        {
            get => _imgMin;
            set
            {
                _imgMin = value;
                picMinimizeWindow.Source = _imgMin;
            }
        }
        public ImageSource ImgIcon
        {
            get => _imgIcone;
            set
            {
                _imgIcone = value;
                picIconWindow.Source = _imgIcone;
            }
        }
        public TitleBar()
        {
            InitializeComponent();
            Loaded += TitleBar_Loaded;
            SizeChanged += TitleBar_Resize;

        }
        private void TitleBar_Resize(object sender, RoutedEventArgs e)
        {
            HeightTitle = (int)this.Height;
        }
        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            Window janela = Window.GetWindow(this);
            if (janela != null)
            {
                WindowChrome windowChrome = new WindowChrome
                {
                    CaptionHeight = HeightTitle
                };

                WindowChrome.SetWindowChrome(janela, windowChrome);

                if (Resize) { janela.ResizeMode = ResizeMode.CanResize; }
                else { janela.ResizeMode = ResizeMode.NoResize; }
            }

            picCloseWindow.MouseDown += (s, e) => CloseWindow();
            picMaximizeWindow.MouseDown += (s, e) => MaximizeWindow();
            picMinimizeWindow.MouseDown += (s, e) => MinimizeWindow();
        }
        private void CloseWindow()
        {
            ActClose?.Invoke();
            if (CanClose)
            {
                Window janela = Window.GetWindow(this);
                janela.Close();
            }
        }
        private void MaximizeWindow()
        {
            ActMax?.Invoke();
            if (CanMax)
            {
                Window janela = Window.GetWindow(this);
                if (janela.WindowState == WindowState.Maximized)
                {
                    janela.WindowState = WindowState.Normal;
                }
                else
                {
                    janela.WindowState = WindowState.Maximized;
                    if (VisivelMaximizado) { this.Visibility = Visibility.Visible; }
                    else { this.Visibility = Visibility.Collapsed; }
                }
            }
        }
        private void MinimizeWindow()
        {
            ActMin?.Invoke();
            if (CanMin)
            {
                Window janela = Window.GetWindow(this);
                janela.WindowState = WindowState.Minimized;
            }
        }
        private void ToogleImg()
        {
            int margin = 2;
            List<int> posicoes = Enumerable.Range(0, 4).Select(i => (HeightTitle + margin) * i).ToList();

            if (BtnClose) { picCloseWindow.Margin = new Thickness(0, 0, posicoes[0], 0); posicoes.RemoveAt(0); }
            if (BtnMax) { picMaximizeWindow.Margin = new Thickness(0, 0, posicoes[0], 0); posicoes.RemoveAt(0); }
            if (BtnMin) { picMinimizeWindow.Margin = new Thickness(0, 0, posicoes[0], 0); posicoes.RemoveAt(0); }

            if (Posicao == 1) { lblTitleWindow.Margin = new Thickness(61, 0, 0, 0); }
            if (Posicao == 2) { lblTitleWindow.Margin = new Thickness(0, 0, 0, 0); }
            if (Posicao == 3) { lblTitleWindow.Margin = new Thickness(0, 0, posicoes[0], 0); }
        }
        private void CriarClip(UIElement elemento, double height, double width, double arqueamento, bool arrLT = true, bool arrRT = true, bool arrRB = true, bool arrLB = true)
        {
            if (arqueamento > height / 2 || arqueamento == 0)
            {
                arqueamento = height / 2;
            }
            double distancia = arqueamento / 2;
            double primeiraMetadeWidth = arqueamento;
            double segundaMetadeWidth = width - arqueamento;
            double primeiraMetadeHeight = arqueamento;
            double segundaMetadeHeight = height - arqueamento;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(primeiraMetadeWidth, 0);

            pathFigure.Segments.Add(new LineSegment(new Point(segundaMetadeWidth, 0), true));

            if (arrRT)
                pathFigure.Segments.Add(new BezierSegment(new Point(segundaMetadeWidth + distancia, 0), new Point(width, primeiraMetadeHeight - distancia), new Point(width, primeiraMetadeHeight), true));
            else
                pathFigure.Segments.Add(new BezierSegment(new Point(width, 0), new Point(width, 0), new Point(width, 0), true));

            pathFigure.Segments.Add(new LineSegment(new Point(width, segundaMetadeHeight), true));

            if (arrRB)
                pathFigure.Segments.Add(new BezierSegment(new Point(width, segundaMetadeHeight + distancia), new Point(segundaMetadeWidth + distancia, height), new Point(segundaMetadeWidth, height), true));
            else
                pathFigure.Segments.Add(new BezierSegment(new Point(width, height), new Point(width, height), new Point(width, height), true));

            pathFigure.Segments.Add(new LineSegment(new Point(primeiraMetadeWidth, height), true));

            if (arrLB)
                pathFigure.Segments.Add(new BezierSegment(new Point(primeiraMetadeWidth - distancia, height), new Point(0, segundaMetadeHeight + distancia), new Point(0, segundaMetadeHeight), true));
            else
                pathFigure.Segments.Add(new BezierSegment(new Point(0, height), new Point(0, height), new Point(0, height), true));

            pathFigure.Segments.Add(new LineSegment(new Point(0, primeiraMetadeHeight), true));

            if (arrLT)
                pathFigure.Segments.Add(new BezierSegment(new Point(0, primeiraMetadeHeight - distancia), new Point(primeiraMetadeHeight - distancia, 0), new Point(primeiraMetadeWidth, 0), true));
            else
                pathFigure.Segments.Add(new BezierSegment(new Point(0, 0), new Point(0, 0), new Point(0, 0), true));

            pathGeometry.Figures.Add(pathFigure);

            elemento.Clip = pathGeometry;
        }
    }
}
