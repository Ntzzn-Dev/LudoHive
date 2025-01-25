using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para Ordenar.xam
    /// </summary>
    public partial class Ordenar : UserControl
    {
        private bool _isHolding = false;
        private LabelCRUD _emMovimento;
        private int _marginAnterior;
        private Point _mousePosition;
        private string _titulo;
        private Color _corLabels = Color.FromArgb(255, 134, 134, 134);
        private Color _corBackground = Color.FromArgb(255, 39, 39, 39);
        private Color _corTitulo = Color.FromArgb(255, 21, 21, 21);
        private Color _corTextoTitulo = Color.FromArgb(255, 192, 192, 192);
        private List<Elementos> _atts;
        private List<Elementos> _ordemPadrao;
        public event Action<List<int>> ListarOrdem;
        public event Action<int, int, LabelCRUD> ElementoClicado;
        public string Titulo 
        {
            get => _titulo;
            set
            {
                _titulo = value;
                lblElementoPai.Content = _titulo;
            }
        }
        public Color CorLabels
        {
            get => _corLabels;
            set
            {
                _corLabels = value;
            }
        }
        public Color CorBackground
        {
            get => _corBackground;
            set
            {
                _corBackground = value;
                gdOrdenar.Background = new SolidColorBrush(_corBackground);
                btnSalvar.Background = new SolidColorBrush(_corBackground);
                btnRedefinir.Background = new SolidColorBrush(_corBackground);
            }
        }
        public Color CorTitulo
        {
            get => _corTitulo;
            set
            {
                _corTitulo = value;
                lblElementoPai.Background = new SolidColorBrush(_corTitulo);
            }
        }
        public Color CorTextoTitulo
        {
            get => _corTextoTitulo;
            set
            {
                _corTextoTitulo = value;
                lblElementoPai.Foreground = new SolidColorBrush(_corTextoTitulo);
                btnSalvar.Foreground = new SolidColorBrush(_corTextoTitulo);
                btnRedefinir.Foreground = new SolidColorBrush(_corTextoTitulo);
            }
        }
        public List<Elementos> Atts
        {
            get
            {
                if (_atts == null) _atts = new List<Elementos>();
                return _atts;
            }
            set
            {
                _atts = value;
                CriarLista();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Elementos> OrdemPadrao
        {
            get
            {
                if (_ordemPadrao == null) _ordemPadrao = new List<Elementos>();
                return _ordemPadrao;
            }
            set
            {
                _ordemPadrao = value;
            }
        }
        public Ordenar()
        {
            InitializeComponent();
            Loaded += (s, e) => CriarLista();
            btnSalvar.Click += SalvarOrdem;
            btnRedefinir.Click += RedefinirOrdem;
        }
        private void SalvarOrdem(object sender, EventArgs e)
        {
            OrdemPadrao = Atts.Select(e => new Elementos
            {
                Ordem = e.Ordem,
                Id = e.Id,
                Nome = e.Nome,
                Icone = e.Icone
            }).ToList();

            var elementosOrdenados = OrdemPadrao.OrderBy(att => att.Ordem);

            var listaSomenteOrdem = elementosOrdenados.Select(att => att.Id).ToList();

            ListarOrdem?.Invoke(listaSomenteOrdem);
        }
        private void RedefinirOrdem(object sender, EventArgs e)
        {
            Atts = OrdemPadrao.Select(e => new Elementos
            {
                Ordem = e.Ordem,
                Id = e.Id,
                Nome = e.Nome,
                Icone = e.Icone
            }).ToList();

            OrdenarAnimacaoLabels();
        }
        private void CriarLista()
        {
            gdOrdem.Children.Clear();
            if (Atts == null) return;
            int marginTop = 0;

            var elementosOrdenados = Atts.OrderBy(att => att.Ordem);

            OrdemPadrao = elementosOrdenados.Select(e => new Elementos
            {
                Ordem = e.Ordem,
                Id = e.Id,
                Nome = e.Nome,
                Icone = e.Icone
            }).ToList();

            foreach (Elementos att in elementosOrdenados)
            {
                LabelCRUD lblAtts = new LabelCRUD()
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    Margin = new Thickness(30, marginTop * 40, 0, 0),
                    Width = 412,
                    WithDelete = false,
                    WithEdit = false,
                    WithExpand = false,
                    Texto = att.Nome,
                    ImgPrincipal = att.Icone,
                    Id = att.Id,
                    Ordem = att.Ordem,
                    CorBackGround = CorLabels
                };

                marginTop += 1;

                lblAtts.MouseDown += Objeto_MouseDown;
                this.MouseMove += Objeto_MouseMove;
                lblAtts.MouseUp += Objeto_MouseUp;
                lblAtts.MouseEnter += MouseEnter_Objeto;
                lblAtts.MouseLeave += MouseLeave_Objeto;
                gdOrdem.Children.Add(lblAtts);
            }
        }
        private void OrdenarAnimacaoLabels()
        {
            var elementosOrdenados = Atts.OrderBy(att => att.Ordem);

            int marginTop = 0;
            foreach (Elementos att in elementosOrdenados)
            {
                att.Ordem = marginTop + 1;
                LabelCRUD lblAtt = gdOrdem.Children.OfType<LabelCRUD>().FirstOrDefault(lbl => lbl.Id == att.Id);

                if (lblAtt != null)
                {
                    lblAtt.Ordem = marginTop + 1;

                    ThicknessAnimation animation = new ThicknessAnimation
                    {
                        From = new Thickness(30, lblAtt.Margin.Top, 0, 0),
                        To = new Thickness(30, marginTop * 40, 0, 0),
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase()
                    };

                    animation.Completed += (s, e) =>
                    {
                        lblAtt.BeginAnimation(FrameworkElement.MarginProperty, null); OrdenarLabels();
                    };

                    lblAtt.BeginAnimation(FrameworkElement.MarginProperty, animation);

                    marginTop += 1;
                }
            }
        }
        private void OrdenarLabels()
        {
            var elementosOrdenados = Atts.OrderBy(att => att.Ordem);

            int marginTop = 0;
            foreach (Elementos att in elementosOrdenados)
            {
                att.Ordem = marginTop + 1;
                LabelCRUD lblAtt = gdOrdem.Children.OfType<LabelCRUD>().FirstOrDefault(lbl => lbl.Id == att.Id);

                if (lblAtt != null)
                {
                    lblAtt.Ordem = marginTop + 1;

                    lblAtt.Margin = new Thickness(30, marginTop * 40, 0, 0);
                    marginTop += 1;
                }
            }
        }
        private void Objeto_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mousePosition = e.GetPosition(this);
            _isHolding = true;
            _emMovimento = sender as LabelCRUD;
            Panel.SetZIndex(_emMovimento, 40);
            _marginAnterior = (int)_emMovimento.Margin.Top / 35;
        }
        private void Objeto_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isHolding)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _mousePosition;

                _emMovimento.Margin = new Thickness(30, _emMovimento.Margin.Top + offset.Y, 0, 0);
                _mousePosition = currentPosition;
            }
        }
        private void Objeto_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isHolding == false) return;
            _isHolding = false;
            int _marginAtual = (int)_emMovimento.Margin.Top / 35;

            _emMovimento.Margin = new Thickness(30, _marginAtual * 40, 0, 0);

            LabelCRUD lblEmQuestao = null;
            Elementos elEmQuestao = null;

            int offset = _marginAnterior > _marginAtual ? 1 : -1;
            List<int> ordens = _marginAnterior > _marginAtual ? Enumerable.Range(_marginAtual + 1, _marginAnterior - _marginAtual + 1).Reverse().ToList() : Enumerable.Range(_marginAnterior + 1, _marginAtual - _marginAnterior + 1).ToList();

            foreach (int number in ordens)
            {
                LabelCRUD lblAtt = gdOrdem.Children.OfType<LabelCRUD>().FirstOrDefault(lbl => lbl.Ordem == number);
                Elementos elemento = Atts.FirstOrDefault(att => att.Ordem == number);

                if (lblAtt != null) lblAtt.Ordem += offset;
                if (elemento != null) elemento.Ordem += offset;

                if (_emMovimento == lblAtt)
                {
                    lblEmQuestao = lblAtt;
                    elEmQuestao = elemento;
                }
            }
            if (lblEmQuestao != null) lblEmQuestao.Ordem = _marginAtual + 1;
            if (elEmQuestao != null) elEmQuestao.Ordem = _marginAtual + 1;

            OrdenarAnimacaoLabels();

            Panel.SetZIndex(_emMovimento, 0);

            //Caso não haja mudança de lugar
            if (_marginAnterior == _marginAtual)
            {
                ElementoClicado?.Invoke(_emMovimento.Id, _emMovimento.Ordem, sender as LabelCRUD);
            }
        }
        private static Color PicDarkenColor(Color color, double factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);

            byte r = (byte)Math.Clamp(color.R * factor, 0, 255);
            byte g = (byte)Math.Clamp(color.G * factor, 0, 255);
            byte b = (byte)Math.Clamp(color.B * factor, 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }
        private static Color PicLightColor(Color color, double factor)
        {
            factor = Math.Clamp(factor, 0f, 1f);

            byte r = (byte)Math.Clamp(color.R / factor, 0, 255);
            byte g = (byte)Math.Clamp(color.G / factor, 0, 255);
            byte b = (byte)Math.Clamp(color.B / factor, 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }
        private void MouseEnter_Objeto(object sender, MouseEventArgs e)
        {
            if(sender is LabelCRUD lbl)
            {
                lbl.CorBackGround = PicDarkenColor(lbl.CorBackGround, 0.8);
            }
        }
        private void MouseLeave_Objeto(object sender, MouseEventArgs e)
        {
            if (sender is LabelCRUD lbl)
            {
                lbl.CorBackGround = PicLightColor(lbl.CorBackGround, 0.8);
            } 
        }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Elementos
    {
        public string Nome { get; set; } = string.Empty;
        public int Id { get; set; } = 0;
        public int Ordem { get; set; } = 0;
        public BitmapImage Icone { get; set; } = null;
        public Elementos()
        {

        }
        public override string ToString()
        {
            return Nome;
        }
    }
}
