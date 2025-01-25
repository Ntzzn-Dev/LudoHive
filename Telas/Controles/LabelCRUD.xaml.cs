using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
using static System.Net.Mime.MediaTypeNames;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para LabelCRUD.xam
    /// </summary>
    public partial class LabelCRUD : UserControl
    {
        private bool _withImg = true;
        private bool _withEdit = true;
        private bool _withDelete = true;
        private bool _withExpand = true;
        private string _texto = "Texto";
        private int _id = 0;
        private int _ordem = 0;
        private double _diferencaCor = 0.9;
        private ImageSource _imgPrc;
        private ImageSource _imgEdt;
        private ImageSource _imgDlt;
        private ImageSource _imgExp;
        private Color _corBackground = Color.FromArgb(255, 134, 134, 134);
        private Color _corFontBackground = Color.FromArgb(255, 0, 0, 0);

        public event EventHandler TextoChanged;
        public event EventHandler LabelClick;
        public event EventHandler ImgPrincipalClick;
        public event EventHandler ImgEditarClick;
        public event EventHandler ImgDeletarClick;
        public event EventHandler ImgExpandirClick;
        public bool WithImg
        {
            get => _withImg;
            set
            {
                _withImg = value;
                ToogleImg();
                picIconLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool WithEdit
        {
            get => _withEdit;
            set
            {
                _withEdit = value;
                ToogleImg();
                picEditLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool WithDelete
        {
            get => _withDelete;
            set
            {
                _withDelete = value;
                ToogleImg();
                picDelLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool WithExpand
        {
            get => _withExpand;
            set
            {
                _withExpand = value;
                ToogleImg();
                picExpLabel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public string Texto
        {
            get => _texto;
            set
            {
                _texto = value;
                OnTextChanged(EventArgs.Empty);
                lblNomeElemento.Content = _texto;
            }
        }
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
            }
        }
        public int Ordem
        {
            get => _ordem;
            set
            {
                _ordem = value;
            }
        }
        public double DiferencaCor
        {
            get => _diferencaCor;
            set
            {
                _diferencaCor = value;
            }
        }
        public ImageSource ImgPrincipal
        {
            get => _imgPrc;
            set
            {
                _imgPrc = value;
                picIconLabel.Imagem = _imgPrc;
            }
        }
        public ImageSource ImgEditar
        {
            get => _imgEdt;
            set
            {
                _imgEdt = value;
                picEditLabel.Imagem = _imgEdt;
            }
        }
        public ImageSource ImgDeletar
        {
            get => _imgDlt;
            set
            {
                _imgDlt = value;
                picDelLabel.Imagem = _imgDlt;
            }
        }
        public ImageSource ImgExpandir
        {
            get => _imgExp;
            set
            {
                _imgExp = value;
                picExpLabel.Imagem = _imgExp;
            }
        }
        public Color CorBackGround
        {
            get => _corBackground;
            set
            {
                _corBackground = value;
                gdLabelCRUD.Background = new SolidColorBrush(_corBackground);
                Color darkerColor = PicDarkenColor(CorBackGround, DiferencaCor);
                picDelLabel.CorFundo = darkerColor;
                picEditLabel.CorFundo = darkerColor;
                picExpLabel.CorFundo = darkerColor;
            }
        }
        public Color CorFontBackGround
        {
            get => _corFontBackground;
            set
            {
                _corFontBackground = value;
                lblNomeElemento.Foreground = new SolidColorBrush(_corFontBackground);
            }
        }

        public LabelCRUD()
        {
            InitializeComponent();
        }
        private void LabelCRUD_Load(object sender, RoutedEventArgs e)
        {
            lblNomeElemento.Content = Texto;
            lblNomeElemento.Foreground = new SolidColorBrush(_corFontBackground);
            gdLabelCRUD.Background = new SolidColorBrush(_corBackground);

            Color darkerColor = PicDarkenColor(CorBackGround, DiferencaCor);
            picDelLabel.CorFundo = darkerColor;
            picEditLabel.CorFundo = darkerColor;
            picExpLabel.CorFundo = darkerColor;

            this.SizeChanged += (s, e) => {
                picDelLabel.TamanhoImg = new Size(this.Height, this.Height);
                picEditLabel.TamanhoImg = new Size(this.Height, this.Height);
                picExpLabel.TamanhoImg = new Size(this.Height, this.Height);
                picIconLabel.TamanhoImg = new Size(this.Height, this.Height);
                ToogleImg();
            };

            picDelLabel.Imagem = new BitmapImage(Referencias.deletar);
            picEditLabel.Imagem = new BitmapImage(Referencias.editar);
            picExpLabel.Imagem = new BitmapImage(Referencias.expandir);

            picIconLabel.MouseDown += ImgPrincipal_Click;
            lblNomeElemento.MouseDown += Label_Click;
            picExpLabel.MouseDown += ImgExpandir_Click;
            picDelLabel.MouseDown += ImgDeletar_Click;
            picEditLabel.MouseDown += ImgEditar_Click;
        }
        private void ToogleImg()
        {
            int margin = 2;
            List<int> posicoes = Enumerable.Range(0, 3).Select(i => ((int)this.Height + margin) * i).ToList();

            if (WithExpand) { picExpLabel.Margin = new Thickness(0, 0, posicoes[0], 0); posicoes.RemoveAt(0); }
            if (WithDelete) { picDelLabel.Margin = new Thickness(0, 0, posicoes[0], 0); posicoes.RemoveAt(0); }
            if (WithEdit) { picEditLabel.Margin = new Thickness(0, 0, posicoes[0], 0); }

            if (WithImg) { lblNomeElemento.Margin = new Thickness(this.Height, 0, posicoes[0], 0);}
            else { lblNomeElemento.Margin = new Thickness(0, 0, posicoes[0], 0);}
        }
        private static Color PicDarkenColor(Color color, double factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);

            byte r = (byte)Math.Clamp(color.R * factor, 0, 255);
            byte g = (byte)Math.Clamp(color.G * factor, 0, 255);
            byte b = (byte)Math.Clamp(color.B * factor, 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }
        protected virtual void Label_Click(object sender, MouseEventArgs e)
        {
            LabelClick?.Invoke(this, e);
        }
        protected virtual void ImgPrincipal_Click(object sender, MouseEventArgs e)
        {
            ImgPrincipalClick?.Invoke(this, e);
        }
        protected virtual void ImgEditar_Click(object sender, MouseEventArgs e)
        {
            ImgEditarClick?.Invoke(this, e);
        }
        protected virtual void ImgDeletar_Click(object sender, MouseEventArgs e)
        {
            ImgDeletarClick?.Invoke(this, e);
        }
        protected virtual void ImgExpandir_Click(object sender, MouseEventArgs e)
        {
            ImgExpandirClick?.Invoke(this, e);
        }
        protected virtual void OnTextChanged(EventArgs e)
        {
            TextoChanged?.Invoke(this, e);
        }
    }
}
