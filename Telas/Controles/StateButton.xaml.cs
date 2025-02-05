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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para StateButton.xam
    /// </summary>
    public partial class StateButton : UserControl
    {
        private List<Elementos> _estados;
        private int _state = 0;
        private int _arredondamento = -1;
        private bool _wImage = true;
        private Color _bkFundo = Color.FromArgb(255, 39, 39, 39);
        private Color _colorFont = Color.FromArgb(255, 0, 0, 0);
        public event Action<Elementos> StateAlterado;
        private static double[] valoresRGBPrecisos = new double[3];
        public List<Elementos> Estados
        {
            get
            {
                if (_estados == null) _estados = new List<Elementos>();
                return _estados;
            }
            set
            {
                _estados = value;

                lblStateAtual.Content = Estados[EstadoAtual].Nome;
                imgAtual.Imagem = Estados[EstadoAtual].Icone;
            }
        }
        public int EstadoAtual
        {
            get => _state;
            set => _state = value;
        }
        public int Arredondamento
        {
            get => _arredondamento;
            set {
                _arredondamento = value;

                Referencias.CriarClip(gdFundo, gdFundo.ActualHeight, gdFundo.ActualWidth, Arredondamento);
            }
        }
        public bool ComImagem
        {
            get => _wImage;
            set
            {
                _wImage = value;
                if (_wImage) { 
                    imgAtual.Visibility = Visibility.Visible;
                } else {
                    imgAtual.Visibility = Visibility.Collapsed;
                }
            }
        }
        public Color CorBtn
        {
            get => _bkFundo;
            set
            {
                _bkFundo = value;
                gdFundo.Background = new SolidColorBrush(_bkFundo);
            }
        }
        public Color CorTexto
        {
            get => _colorFont;
            set
            {
                _colorFont = value;
                lblStateAtual.Foreground = new SolidColorBrush(_colorFont);
            }
        }
        public StateButton()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                Referencias.CriarClip(gdFundo, gdFundo.ActualHeight, gdFundo.ActualWidth, Arredondamento);
                if (imgAtual.Imagem == null)
                {
                    imgAtual.Visibility = Visibility.Collapsed;
                }
                else
                {
                    imgAtual.Visibility = Visibility.Visible;
                }
            };
            this.MouseDown += TrocarEstado;
            this.MouseEnter += MouseEnter_Btn;
            this.MouseLeave += MouseLeave_Btn;
        }
        private void TrocarEstado(object sender, MouseButtonEventArgs e)
        {
            EstadoAtual = AumentarIndice(EstadoAtual, Estados.Count);

            lblStateAtual.Content = Estados[EstadoAtual].Nome;
            imgAtual.Imagem = Estados[EstadoAtual].Icone;

            if (imgAtual.Imagem == null)
            {
                imgAtual.Visibility = Visibility.Collapsed;
            }
            else
            {
                imgAtual.Visibility = Visibility.Visible;
            }

            StateAlterado?.Invoke(Estados[EstadoAtual]);
        }
        private int AumentarIndice(int i, int a)
        {
            return i == a - 1? i = 0 : i += 1;
        }
        private static Color PicDarkenColor(Color color, double factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);

            valoresRGBPrecisos[0] = (color.R * factor);
            valoresRGBPrecisos[1] = (color.G * factor);
            valoresRGBPrecisos[2] = (color.B * factor);

            byte r = (byte)valoresRGBPrecisos[0];
            byte g = (byte)valoresRGBPrecisos[1];
            byte b = (byte)valoresRGBPrecisos[2];

            return Color.FromArgb(color.A, r, g, b);
        }
        private static Color PicLightColor(Color color, double factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);

            valoresRGBPrecisos[0] = (valoresRGBPrecisos[0] / factor);
            valoresRGBPrecisos[1] = (valoresRGBPrecisos[1] / factor);
            valoresRGBPrecisos[2] = (valoresRGBPrecisos[2] / factor);

            byte r = (byte)valoresRGBPrecisos[0];
            byte g = (byte)valoresRGBPrecisos[1];
            byte b = (byte)valoresRGBPrecisos[2];

            return Color.FromArgb(color.A, r, g, b);
        }
        private void MouseEnter_Btn(object sender, MouseEventArgs e)
        {
            CorBtn = PicDarkenColor(CorBtn, 0.8);
        }
        private void MouseLeave_Btn(object sender, MouseEventArgs e)
        {
            CorBtn = PicLightColor(CorBtn, 0.8);
        }
    }
}
