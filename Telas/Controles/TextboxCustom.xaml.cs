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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace LudoHive.Telas.Controles
{
    /// <summary>
    /// Interação lógica para TextboxCustom.xam
    /// </summary>
    public partial class TextboxCustom : UserControl
    {
        private bool isFocused = false;
        private string _placeholder = "";
        private bool _password = false;
        private Color _corBackground;
        private Color _corForeground;
        private Color _corPlaceholder;
        private int _fontSize;
        private string _text = "";
        public event EventHandler TextoChanged;
        public event EventHandler EnterPressed;
        public bool Password
        {
            get => _password;
            set
            {
                _password = value;
                txtbxTexto.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                pwdBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Color CorBackground
        {
            get => _corBackground;
            set
            {
                _corBackground = value;
                Brush pincel = new SolidColorBrush(value);
                txtbxBackground.Stroke = pincel;
                txtbxBackground.Fill = pincel;
                txtbxTexto.Background = pincel;
                txtbxTexto.BorderBrush = pincel;
                txtbxTexto.SelectionBrush = pincel;
                pwdBox.Background = pincel;
                pwdBox.BorderBrush = pincel;
                pwdBox.SelectionBrush = pincel;
            }
        }
        public Color CorForeground
        {
            get => _corForeground;
            set
            {
                _corForeground = value;
                Brush pincel = new SolidColorBrush(value);
                txtbxTexto.Foreground = pincel;
                pwdBox.Foreground = pincel;
            }
        }
        public Color CorPlaceholder
        {
            get => _corPlaceholder;
            set
            {
                _corPlaceholder = value;
                Brush pincel = new SolidColorBrush(value);
                lblPlaceholder.Foreground = pincel;
            }
        }
        public int Fontsize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                lblPlaceholder.FontSize = value;

                txtbxTexto.FontSize = value;
                txtbxTexto.Margin = new Thickness(txtbxTexto.Margin.Left, 7 + value, txtbxTexto.Margin.Right, txtbxTexto.Margin.Bottom);

                pwdBox.FontSize = value;
                pwdBox.Margin = txtbxTexto.Margin; 
                this.Height = 30 + (19 * value / 12);
                if (txtbxTexto.Text != "")
                {
                    isFocused = false;
                    TransicaoLabel();
                }
            }
        }
        public string Texto
        {
            get => _text;
            set
            {
                _text = value;
                txtbxTexto.Text = value;
                TextoChanged?.Invoke(this, EventArgs.Empty);
                if (txtbxTexto.Text != "")
                {
                    isFocused = false;
                    TransicaoLabel();
                }
            }
        }
        public string Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                lblPlaceholder.Content = value;
            }
        }
        public TextboxCustom()
        {
            InitializeComponent();
            _corBackground = Color.FromArgb(255, 125, 125, 125);
            _corPlaceholder = Color.FromArgb(255, 189, 189, 189);
            _corForeground = Color.FromArgb(255, 0, 0, 0);
            this.Loaded += AoCarregar;
        }
        private void AoCarregar(object sender, EventArgs e)
        {
            txtbxTexto.BorderThickness = new Thickness(0);
            txtbxTexto.FocusVisualStyle = null;
            pwdBox.BorderThickness = new Thickness(0);
            pwdBox.FocusVisualStyle = null;
            if (txtbxTexto.Text != "")
            {
                isFocused = false;
                TransicaoLabel();
            }
        }
        private void TransicaoLabel()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (isFocused == false)
                {
                    ThicknessAnimation animation = new ThicknessAnimation
                    {
                        From = new Thickness(6, lblPlaceholder.Margin.Top, 0, 0),
                        To = new Thickness(6, 0, 0, 0),
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase()
                    };

                    animation.Completed += (s, e) =>
                    {
                        isFocused = true;
                        Brush pincel = new SolidColorBrush(Color.FromArgb(255, 138, 138, 138));
                        lblPlaceholder.Foreground = pincel;
                    };

                    lblPlaceholder.BeginAnimation(FrameworkElement.MarginProperty, animation);
                }
                else
                {
                    ThicknessAnimation animation = new ThicknessAnimation
                    {
                        From = new Thickness(6, lblPlaceholder.Margin.Top, 0, 0),
                        To = new Thickness(6, Fontsize * 0.83f, 0, 0),
                        Duration = TimeSpan.FromSeconds(0.2),
                        EasingFunction = new QuadraticEase()
                    };

                    animation.Completed += (s, e) =>
                    {
                        isFocused = false;
                        Brush pincel = new SolidColorBrush(CorPlaceholder);
                        lblPlaceholder.Foreground = pincel;
                    };

                    lblPlaceholder.BeginAnimation(FrameworkElement.MarginProperty, animation);
                }
            });
        }
        private void label_Click(object sender, MouseButtonEventArgs e)
        {
            txtbxTexto.Focus();
        }
        protected virtual void OnTextChanged(EventArgs e)
        {
            TextoChanged?.Invoke(this, e);
        }
        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnEnterPressed(EventArgs.Empty);
            }
        }
        protected virtual void OnEnterPressed(EventArgs e)
        {
            EnterPressed?.Invoke(this, e);
        }
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Texto = txtbxTexto.Text;
            if (Password) { Texto = pwdBox.Password; }
        }
        private void label1_Click(object sender, MouseButtonEventArgs e)
        {
            txtbxTexto.Focus();
        }
        private void textBox1_EnterOrLeave(object sender, RoutedEventArgs e)
        {
            if (txtbxTexto.Text == "")
            {
                TransicaoLabel();
            }
        }
        public void SetFocus()
        {
            txtbxTexto.Focus();
        }
    }
}
