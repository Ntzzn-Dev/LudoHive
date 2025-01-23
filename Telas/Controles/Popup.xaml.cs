using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interação lógica para Popup.xam
    /// </summary>
    public partial class Popup : UserControl
    {
        private Size _sizePopup;
        private Color _colorPopup;
        private Color _colorElementoPopup;
        private Color _colorTextPopup;
        private List<Boxes> _elementosPopup;
        public Action<int, int> BoxClicadoEvent;
        public bool sla;
        public Size SizePopup
        {
            get => _sizePopup;
            set
            {
                _sizePopup = value;
            }
        }
        public Color ColorPopup
        {
            get => _colorPopup;
            set
            {
                _colorPopup = value;
                PopupLoad(null, null);
            }
        }
        public Color ColorElementoPopup
        {
            get => _colorElementoPopup;
            set
            {
                _colorElementoPopup = value;
                PopupLoad(null, null);
            }
        }
        public Color ColorTextPopup
        {
            get => _colorTextPopup;
            set
            {
                _colorTextPopup = value;
                PopupLoad(null, null);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Boxes> ElementosPopup
        {
            get
            {
                if (_elementosPopup == null) _elementosPopup = new List<Boxes>();
                return _elementosPopup;
            }
            set
            {
                _elementosPopup = value;
                PopupLoad(null, null);
            }
        }
        public Popup()
        {
            InitializeComponent();
            SizePopup = new Size(278, 0);
        }
        private void PopupLoad(object sender, RoutedEventArgs e)
        {
            flwPopup.Background = new SolidColorBrush(ColorPopup);
            flwPopup.Children.Clear();
            SizePopup = new Size(SizePopup.Width, 0);
            foreach (Boxes box in ElementosPopup)
            {
                SizePopup = new Size(SizePopup.Width, SizePopup.Height + 54);
                Grid gdOpcao1 = new Grid()
                {
                    Name = "pnl_ID_" + box.IdBox + "_ID_" + box.IdRepassar,
                    Height = 50,
                    Background = new SolidColorBrush(ColorElementoPopup),
                    Margin = new Thickness(0, 2, 0, 2)
                };
                Image pic = new Image()
                {
                    Name = "pic_" + box.IdBox + "_ID_" + box.IdRepassar,
                    Height = 50,
                    Width = 50,
                    Source = box.Imagem,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Label lbl = new Label()
                {
                    FontFamily = new FontFamily("Arial"),
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(50, 0, 0, 0),
                    Name = "lbl_" + box.IdBox + "_ID_" + box.IdRepassar,
                    Content = box.Nome,
                    Foreground = new SolidColorBrush(ColorTextPopup)
                };
                gdOpcao1.MouseDown += BoxClicado;
                gdOpcao1.MouseEnter += BoxEnter;
                gdOpcao1.MouseLeave += BoxLeave;
                gdOpcao1.Children.Add(lbl);
                gdOpcao1.Children.Add(pic);
                flwPopup.Children.Add(gdOpcao1);
            }
            this.Height = SizePopup.Height;
            this.Width = SizePopup.Width;
        }
        private void BoxClicado(object sender, EventArgs e)
        {
            if (sender is Grid gd)
            {
                int botaoClicado = int.Parse(gd.Name.Split("_ID_")[1]);
                int valorRepassado = int.Parse(gd.Name.Split("_ID_")[2]);
                BoxClicadoEvent?.Invoke(botaoClicado, valorRepassado);
            }
        }
        private void BoxEnter(object sender, EventArgs e)
        {
            if (sender is Grid gd)
            {
                gd.Background = PicDarkenColor(new SolidColorBrush(ColorElementoPopup));
            }
        }
        private void BoxLeave(object sender, EventArgs e)
        {
            if (sender is Grid gd)
            {
                gd.Background = new SolidColorBrush(ColorElementoPopup);
            }
        }
        private static Brush PicDarkenColor(Brush brush, double factor = 0.8)
        {
            factor = Math.Clamp(factor, 0, 1);

            if (brush is SolidColorBrush solidColorBrush)
            {
                Color color = solidColorBrush.Color;

                byte r = (byte)(color.R * factor);
                byte g = (byte)(color.G * factor);
                byte b = (byte)(color.B * factor);

                return new SolidColorBrush(Color.FromArgb(color.A, r, g, b));
            }

            return brush;
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Boxes
    {
        public string Nome { get; set; } = string.Empty;
        public int IdBox { get; set; } = 0;
        public int IdRepassar { get; set; } = 0;
        public BitmapImage Imagem { get; set; } = null;
        public Boxes()
        {

        }
        public override string ToString()
        {
            return Nome;
        }
    }
}
