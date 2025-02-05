using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                Referencias.CriarClip(tgbtnBackgroundBorder, tgbtnBackgroundBorder.ActualHeight, tgbtnBackgroundBorder.ActualWidth, Arredondamento);
                Referencias.CriarClip(tgbtnBackground, tgbtnBackground.ActualHeight, tgbtnBackground.ActualWidth, Arredondamento);
                Referencias.CriarClip(tgbtnCircle, tgbtnCircle.ActualHeight, tgbtnCircle.ActualWidth, Arredondamento);
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

                Referencias.CriarClip(tgbtnBackgroundBorder, tgbtnBackgroundBorder.ActualHeight, tgbtnBackgroundBorder.ActualWidth, Arredondamento);
                Referencias.CriarClip(tgbtnBackground, tgbtnBackground.ActualHeight, tgbtnBackground.ActualWidth, Arredondamento);
                Referencias.CriarClip(tgbtnCircle, tgbtnCircle.ActualHeight, tgbtnCircle.ActualWidth, Arredondamento);

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
                Referencias.CriarClip(tgbtnBackgroundBorder, tgbtnBackgroundBorder.ActualHeight, tgbtnBackgroundBorder.ActualWidth, Arredondamento);
            };
            tgbtnBackground.SizeChanged += (s, e) =>
            {
                Referencias.CriarClip(tgbtnBackground, tgbtnBackground.ActualHeight, tgbtnBackground.ActualWidth, Arredondamento);
            };
            tgbtnCircle.SizeChanged += (s, e) =>
            {
                Referencias.CriarClip(tgbtnCircle, tgbtnCircle.ActualHeight, tgbtnCircle.ActualWidth, Arredondamento);
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
    }
}
