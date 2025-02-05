using System.Collections;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para Table.xam
    /// </summary>
    public partial class Table : UserControl
    {
        private int _numRows = 0;
        private int _numCols = 0;
        private Point _tamanhoMax = new Point(2560, 1440);
        private double _linhasCols = 0.2;
        private double _linhasRows = 0.2;
        private double _tamanhoTexto = 12;
        private bool _withDelete = false;
        private Color _corFonteTbl = Color.FromArgb(255, 192, 192, 192);
        private Color _corFundoTbl = Color.FromArgb(255, 39, 39, 39);
        private Color _corCabecalhoTbl = Color.FromArgb(255, 39, 39, 39);
        private List<object> _listaDeItens;
        private List<BitmapImage> _listaDeIcons;
        public event Action<Elementos> TabelaAlteradaViaStateBtn;
        private int NumRows
        {
            get => _numRows;
            set
            {
                if (_numRows != value)
                {
                    _numRows = value;
                    gdFundo.RowDefinitions.Clear();
                    for (int i = 0; i < _numRows; i++)
                    {
                        gdFundo.RowDefinitions.Add(new RowDefinition
                        {
                            Height = new GridLength(50, GridUnitType.Pixel),
                        });
                    }
                }
            }
        }
        private int NumCols
        {
            get => _numCols;
            set
            {
                if (_numCols != value)
                {
                    _numCols = value;
                    gdFundo.ColumnDefinitions.Clear(); // Limpa antes de adicionar novas
                    for (int i = 0; i < _numCols; i++)
                    {
                        gdFundo.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(1, GridUnitType.Star)
                        });
                    }
                }
            }
        }
        public Point TamanhoMax
        {
            get => _tamanhoMax;
            set
            {
                _tamanhoMax = value;
                this.MaxWidth = _tamanhoMax.X;
                this.MaxHeight = _tamanhoMax.Y;
            }
        }
        public double LinhasCols
        {
            get => _linhasCols;
            set => _linhasCols = value;
        }
        public double LinhasRows
        {
            get => _linhasRows;
            set => _linhasRows = value;
        }
        public double TamanhoTexto
        {
            get => _tamanhoTexto;
            set => _tamanhoTexto = value;
        }
        public bool WithDelete
        {
            get => _withDelete;
            set => _withDelete = value;
        }
        public Color CorFonteTbl
        {
            get => _corFonteTbl;
            set => _corFonteTbl = value;
        }
        public Color CorFundoTbl
        {
            get => _corFundoTbl;
            set
            {
                _corFundoTbl = value;
                gdFundo.Background = new SolidColorBrush(_corFundoTbl);
                gdScroll.Background = new SolidColorBrush(_corFundoTbl);
            }
        }
        public Color CorCabecalhoTbl
        {
            get => _corCabecalhoTbl;
            set => _corCabecalhoTbl = value;
        }
        public List<object> ListaDeItens
        {
            get
            {
                if (_listaDeItens == null) _listaDeItens = new List<object>();
                return _listaDeItens;
            }
            set
            {
                _listaDeItens = value ?? new List<object>();

                NumRows = _listaDeItens.Count + 1;

                Listar();
            }
        }
        public List<BitmapImage> ListaDeIcons
        {
            get
            {
                if (_listaDeIcons == null) _listaDeIcons = new List<BitmapImage>();
                return _listaDeIcons;
            }
            set
            {
                _listaDeIcons = value ?? new List<BitmapImage>();
            }
        }
        public Table()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                scrElementos.Maximum = gdFundo.ActualHeight - this.ActualHeight;
                gdScroll.Height = this.ActualHeight;
                
            };
            gdScroll.SizeChanged += (s, e) => {
                if (gdScroll.ActualHeight < gdFundo.ActualHeight)
                    scrElementos.Visibility = Visibility.Visible;
                else
                    scrElementos.Visibility = Visibility.Collapsed;
            };
            scrElementos.ValueChanged += (s, e) =>
            {
                gdFundo.Margin = new Thickness(0, -e.NewValue, 0, 0);
            };
        }
        private void Listar()
        {
            if (ListaDeItens.Count == 0) return;

            Type tipo = ListaDeItens[0].GetType();
            PropertyInfo[] propriedades = tipo.GetProperties();

            NumCols = propriedades.Length;
            if (WithDelete) NumCols++;

            int coluna = 0;

            foreach (PropertyInfo propriedade in propriedades)
            {
                ColorirLinha(0);

                string nomeCelula = propriedade.Name;

                if (nomeCelula.Contains("stt_")) { nomeCelula = nomeCelula.Replace("stt_", ""); }

                Label lbl = CriarLabel(FormatarNome(nomeCelula), true);

                Border border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(LinhasCols, LinhasRows, LinhasCols, LinhasRows)
                };

                border.Child = lbl;

                gdFundo.Children.Add(border);

                Grid.SetRow(border, 0);
                Grid.SetColumn(border, coluna);
                coluna++;
            }
            if (WithDelete) {
                Label lbl = CriarLabel("Deletar", true);

                Border border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(LinhasCols, LinhasRows, LinhasCols, LinhasRows)
                };

                border.Child = lbl;

                gdFundo.Children.Add(border);

                Grid.SetRow(border, 0);
                Grid.SetColumn(border, coluna);
                coluna++;
            }

            int linha = 1;
            foreach (object obj in ListaDeItens)
            {
                coluna = 0;
                foreach (PropertyInfo propriedade in propriedades)
                {
                    object valor = propriedade.GetValue(obj);

                    UIElement elem = null;

                    if (propriedade.Name.ToString().StartsWith("stt_"))
                    {
                        elem = CriarStateButton(valor);
                    }
                    else
                    {
                        elem = CriarElemento(valor);
                    }

                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(LinhasCols, LinhasRows, LinhasCols, LinhasRows)
                    };

                    border.Child = elem;

                    gdFundo.Children.Add(border);

                    Grid.SetRow(border, linha);
                    Grid.SetColumn(border, coluna);
                    coluna++;
                }
                if (WithDelete)
                {
                    UIElement elem = new Button()
                    {
                        Content = "apagar"
                    };

                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(LinhasCols, LinhasRows, LinhasCols, LinhasRows)
                    };

                    border.Child = elem;

                    gdFundo.Children.Add(border);

                    Grid.SetRow(border, linha);
                    Grid.SetColumn(border, coluna);
                    coluna++;
                }
                linha++;
            }
        }
        private string FormatarNome(string nome)
        {
            string resultado = Regex.Replace(nome, "([A-Z])", " $1").Trim();

            resultado = char.ToUpper(resultado[0]) + resultado.Substring(1);

            return char.ToUpper(resultado[0]) + resultado.Substring(1);
        }
        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta;

            if (delta > 0)
            {
                scrElementos.Value = Math.Max(scrElementos.Minimum, scrElementos.Value - 30);
            }
            else
            {
                scrElementos.Value = Math.Min(scrElementos.Maximum, scrElementos.Value + 30);
            }

            e.Handled = true;
        }
        private void ColorirLinha(int linha)
        {
            for (int coluna = 0; coluna < NumCols; coluna++)
            {
                var celula = new Rectangle
                {
                    Fill = new SolidColorBrush(CorCabecalhoTbl) // A cor de fundo que você quer
                };

                Grid.SetRow(celula, linha);
                Grid.SetColumn(celula, coluna);
                gdFundo.Children.Add(celula);
                Panel.SetZIndex(celula, -1);
            }
        }
        private UIElement CriarElemento(object valor)
        {
            if (valor is BitmapImage icon)
            {
                return new ImgOnline()
                {
                    Imagem = icon,
                    TamanhoImg = new Size(50, 50),
                    Arredondado = 12,
                    CorFundo = Color.FromArgb(255, 55, 55, 55)
                };
            }
            else
            {
                return CriarLabel(valor?.ToString());
            }
        }
        private Label CriarLabel(string conteudo, bool bold = false)
        {
            Label lbl = new()
            {
                Content = conteudo,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = bold? FontWeights.Bold : FontWeights.Normal,
                FontSize = TamanhoTexto,
                Foreground = new SolidColorBrush(CorFonteTbl)
            };

            return lbl;
        }
        private StateButton CriarStateButton(object valor)
        {
            if (!valor.ToString().Contains("__")) return new();

            string[] partes = valor.ToString().Split("__");

            int idAtual = int.Parse(partes[0]);

            int estadoAtual = int.Parse(partes[1]);

            List<string> lista = partes.Skip(2).ToList();

            List<Elementos> estados = new List<Elementos>();

            int ordem = 0;
            foreach (string state in lista)
            {
                BitmapImage imagemEncontrada = ListaDeIcons.Find(img => img.UriSource.ToString().ToLower().Contains(state.ToLower()));
                estados.Add(new Elementos() { Id = idAtual, Nome = state, Ordem = ordem, Icone = imagemEncontrada });
                ordem++;
            }

            StateButton stt = new()
            {
                Width = 200,
                CorTexto = CorFonteTbl,
                Arredondamento = 1,
                EstadoAtual = estadoAtual,
                Estados = estados,
                CorBtn = CorFundoTbl
            };

            stt.StateAlterado += (ele) => TabelaAlteradaViaStateBtn?.Invoke(ele);

            return stt;
        }
    }
}
