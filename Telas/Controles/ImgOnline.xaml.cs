using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    /// Interação lógica para ImgOnline.xam
    /// </summary>
    public partial class ImgOnline : UserControl
    {
        private Color _corFundo;
        private Size _tamanhoImg;
        private string _url;
        private bool _eIcone;
        private BitmapImage _imagem;
        public Action Falha; 
        public Action ImgCarregada; 
        public Color CorFundo
        {
            get => _corFundo;
            set
            {
                _corFundo = value;
                fundoImg.Background = new SolidColorBrush(_corFundo);
            }
        }
        public Size TamanhoImg
        {
            get => _tamanhoImg;
            set
            {
                _tamanhoImg = value;
                fundoImg.Width = _tamanhoImg.Width;
                fundoImg.Height = _tamanhoImg.Height;
                img.Width = _tamanhoImg.Width;
                img.Height = _tamanhoImg.Height;
                this.Width = _tamanhoImg.Width;
                this.Height = _tamanhoImg.Height;
            }
        }
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                img.Source = new BitmapImage(Referencias.imgPrincipal);
                BaixarImgs(_url);
            }
        }
        public bool EIcone
        {
            get => _eIcone;
            set
            {
                _eIcone = value;
            }
        }
        public BitmapImage Imagem
        {
            get => _imagem;
            set
            {
                _imagem = value;
                img.Source = _imagem;
            }
        }
        public ImgOnline()
        {
            InitializeComponent();
            _tamanhoImg = new Size(100, 100);
            Imagem = new BitmapImage(Referencias.imgPrincipal);
        }
        private async Task BaixarImgs(string pathToImg)
        {
            try
            {
                string formato = await DetectarFormatoAsync(pathToImg);
                if (string.IsNullOrEmpty(pathToImg))
                {
                    if (Url != "")
                    {
                        Imagem = new BitmapImage(Referencias.imgPrincipal);
                    }
                    return;
                }

                if (formato == "LOCAL")
                {
                    BitmapImage imgCarregada = new BitmapImage(new Uri(pathToImg, UriKind.Absolute));
                    if (imgCarregada.Width != imgCarregada.Height && EIcone)
                    {
                        Falha?.Invoke();
                        MessageBox.Show("a imagem deve ser quadrada");
                    }
                    else
                    {
                        Imagem = imgCarregada;
                    }
                }
                else if (formato == "BASE64")
                {
                    var base64Data = pathToImg.Split(',')[1];
                    byte[] bytesDaImg2 = Convert.FromBase64String(base64Data);

                    using (MemoryStream ms = new MemoryStream(bytesDaImg2))
                    {
                        BitmapImage imgCarregada = Referencias.memoryStreamToBitmap(ms);
                        if (imgCarregada.Width != imgCarregada.Height && EIcone)
                        {
                            Falha?.Invoke();
                            MessageBox.Show("a imagem deve ser quadrada");
                        }
                        else
                        {
                            Imagem = imgCarregada;
                        }
                    }
                }
                else if (formato == "OUTRO")
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] bytesDaImg = await client.GetByteArrayAsync(pathToImg);

                        using (MemoryStream ms = new MemoryStream(bytesDaImg))
                        {
                            BitmapImage imgCarregada = Referencias.memoryStreamToBitmap(ms);
                            if (imgCarregada.Width != imgCarregada.Height && EIcone)
                            {
                                Falha?.Invoke();
                                MessageBox.Show("a imagem deve ser quadrada");
                            }
                            else
                            {
                                Imagem = imgCarregada;
                            }
                        }
                    }
                }
                else if (formato == "WEBP")
                {
                    BitmapImage imagem = await CarregarImagemWebpAsync(pathToImg);

                    if (imagem != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Imagem = Referencias.memoryStreamToBitmap(ms);
                        }
                    }
                }
                else if (formato == "ICO")
                {
                    BitmapImage imagem = await BaixarEConverterIcoAsync(pathToImg);

                    if (imagem != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Imagem = Referencias.memoryStreamToBitmap(ms);
                        }
                    }
                }
                else { Imagem = new BitmapImage(Referencias.imgPrincipal); return; }

                ImgCarregada?.Invoke();
            }
            catch (Exception ex)
            {
                Falha?.Invoke();
                //MessageBox.Show($"Erro ao baixar a imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<string> DetectarFormatoAsync(string url)
        {
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) { return "BASE64"; }
            else
            if (File.Exists(url)) { return "LOCAL"; }
            else
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] bytes = await client.GetByteArrayAsync(url);

                    if (bytes.Length >= 12)
                    {
                        string header = BitConverter.ToString(bytes.Take(12).ToArray()).Replace("-", "");

                        if (header.StartsWith("52494646") && header.Contains("57454250")) // WEBP
                            return "WEBP";

                        if (header.StartsWith("00000100") || header.StartsWith("00000200")) // ICO
                            return "ICO";
                    }
                    return "OUTRO";
                }
            }
            return "NENHUM";
        }
        private async Task<BitmapImage> CarregarImagemWebpAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes = await client.GetByteArrayAsync(url);

                using (var ms = new MemoryStream(bytes))
                {
                    SKBitmap bitmap = SKBitmap.Decode(ms);
                    using (var imgStream = new MemoryStream())
                    {
                        bitmap.Encode(imgStream, SKEncodedImageFormat.Png, 100);
                        imgStream.Seek(0, SeekOrigin.Begin);

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = imgStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        return bitmapImage;
                    }
                }
            }
        }
        private async Task<BitmapImage> BaixarEConverterIcoAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] icoBytes = await client.GetByteArrayAsync(url);

                using (var ms = new MemoryStream(icoBytes))
                {
                    SKBitmap bitmap = SKBitmap.Decode(ms);
                    if (bitmap == null)
                        throw new Exception("Não foi possível decodificar a imagem como um ícone válido.");

                    using (MemoryStream pngStream = new MemoryStream())
                    {
                        bitmap.Encode(pngStream, SKEncodedImageFormat.Png, 100);
                        pngStream.Seek(0, SeekOrigin.Begin);

                        BitmapImage imgCarregada = new BitmapImage();
                        imgCarregada.BeginInit();
                        imgCarregada.CacheOption = BitmapCacheOption.OnLoad;
                        imgCarregada.StreamSource = pngStream;
                        imgCarregada.EndInit();
                        imgCarregada.Freeze();

                        return imgCarregada;
                    }
                }
            }
        }
    }
}
