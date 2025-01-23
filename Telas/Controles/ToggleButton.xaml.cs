using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Windows.Media.Animation;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para ToggleButton.xam
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        private bool _isTrue = false;
        private Color _corFundoTrue = Color.FromArgb(255, 84, 86, 80);
        private Color _corFundoFalse = Color.FromArgb(255, 86, 80, 80);
        private Color _corBorda = Color.FromArgb(255, 55, 55, 55);
        private Color _corCirculo = Color.FromArgb(255, 123, 123, 123);
        private int _arredondamento = 0;
        private int _bordas = 1;
        public event Action<bool> ValorAlternado;
        public bool IsTrue
        {
            get => _isTrue;
            set
            {
                _isTrue = value;
                AnimacaoToggle(_isTrue);
                TrocarCorFundo(_isTrue);
                ValorAlternado?.Invoke(IsTrue);
            }
        }
        public Color CorFundoFalse
        {
            get => _corFundoFalse;
            set
            {
                _corFundoFalse = value;
            }
        }
        public Color CorFundoTrue
        {
            get => _corFundoTrue;
            set
            {
                _corFundoTrue = value;
            }
        }
        public Color CorBorda
        {
            get => _corBorda;
            set
            {
                _corBorda = value;
                tgbtnBackgroundBorder.Fill = new SolidColorBrush(_corBorda);
            }
        }
        public Color CorCirculo
        {
            get => _corCirculo;
            set
            {
                _corCirculo = value;
                tgbtnCircle.Fill = new SolidColorBrush(_corCirculo);
            }
        }
        public int Arredondamento
        {
            get => _arredondamento;
            set
            {
                _arredondamento = value;
                CriarClip(tgbtnBackgroundBorder, tgbtnBackgroundBorder.ActualHeight, tgbtnBackgroundBorder.ActualWidth, Arredondamento);
                CriarClip(tgbtnBackground, tgbtnBackground.ActualHeight, tgbtnBackground.ActualWidth, Arredondamento);
                CriarClip(tgbtnCircle, tgbtnCircle.ActualHeight, tgbtnCircle.ActualWidth, Arredondamento);
            }
        }
        public int Bordas
        {
            get => _bordas;
            set
            {
                if (value > this.ActualHeight / 2) _bordas = (int)this.ActualHeight / 2;
                else _bordas = value;

                tgbtnCircle.Height = tgbtnBackgroundBorder.ActualHeight - (_bordas * 2);
                tgbtnCircle.Width = tgbtnBackgroundBorder.ActualHeight - (_bordas * 2);

                CriarClip(tgbtnBackgroundBorder, tgbtnBackgroundBorder.ActualHeight, tgbtnBackgroundBorder.ActualWidth, Arredondamento);
                CriarClip(tgbtnBackground, tgbtnBackground.ActualHeight, tgbtnBackground.ActualWidth, Arredondamento);
                CriarClip(tgbtnCircle, tgbtnCircle.ActualHeight, tgbtnCircle.ActualWidth, Arredondamento);

                tgbtnBackground.Margin = new Thickness(_bordas);

                AnimacaoToggle(IsTrue);
            }
        }
        public ToggleButton()
        {
            InitializeComponent();
            tgbtnBackgroundBorder.SizeChanged += (s, e) =>
            {
                tgbtnBackground.MaxHeight = this.ActualHeight;
                tgbtnBackground.MaxWidth = this.ActualWidth;
                tgbtnCircle.Height = tgbtnBackgroundBorder.ActualHeight - (Bordas * 2);
                tgbtnCircle.Width = tgbtnBackgroundBorder.ActualHeight - (Bordas * 2);
                CriarClip(tgbtnBackgroundBorder, tgbtnBackgroundBorder.ActualHeight, tgbtnBackgroundBorder.ActualWidth, Arredondamento);
            };
            tgbtnBackground.SizeChanged += (s, e) =>
            {
                CriarClip(tgbtnBackground, tgbtnBackground.ActualHeight, tgbtnBackground.ActualWidth, Arredondamento);
            };
            tgbtnCircle.SizeChanged += (s, e) =>
            {
                CriarClip(tgbtnCircle, tgbtnCircle.ActualHeight, tgbtnCircle.ActualWidth, Arredondamento);
            };

            this.MouseDown += (s, e) => IsTrue = !IsTrue;
            this.Loaded += (s, e) => IsTrue = IsTrue;
        }
        private void TrocarCorFundo(bool value)
        {
            if (value) { tgbtnBackground.Fill = new SolidColorBrush(CorFundoTrue); }
            else { tgbtnBackground.Fill = new SolidColorBrush(CorFundoFalse); }
        }
        private void AnimacaoToggle(bool acao)
        {
            ThicknessAnimation animation = new ThicknessAnimation
            {
                From = acao
                    ? new Thickness(Bordas, Bordas, 0, Bordas)
                    : new Thickness(tgbtnBackgroundBorder.ActualWidth - tgbtnCircle.ActualWidth - Bordas, Bordas, -tgbtnBackgroundBorder.ActualWidth - tgbtnCircle.ActualWidth, Bordas),
                To = acao
                    ? new Thickness(tgbtnBackgroundBorder.ActualWidth - tgbtnCircle.ActualWidth - Bordas, Bordas, -tgbtnBackgroundBorder.ActualWidth - tgbtnCircle.ActualWidth, Bordas)
                    : new Thickness(Bordas, Bordas, 0, Bordas),
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new QuadraticEase()
            };

            tgbtnCircle.BeginAnimation(FrameworkElement.MarginProperty, animation);
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
