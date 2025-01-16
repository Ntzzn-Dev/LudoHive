using Microsoft.Data.Sqlite;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LudoHive
{
    public class Aplicativos
    {
        private int id;
        private string nome;
        private string caminho;
        private BitmapImage icon;

        public Aplicativos()
        {

        }
        public Aplicativos(int idDePesquisa)
        {
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string selectCommand = "SELECT Nome, Caminho, Icon FROM AplicativosExtras WHERE Id = @id";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idDePesquisa);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nome = reader.GetString(0);
                                string caminho = reader.GetString(1);
                                long tamanhoBlobImg = reader.GetBytes(2, 0, null, 0, 0);

                                byte[] bufferImg = new byte[tamanhoBlobImg];
                                reader.GetBytes(2, 0, bufferImg, 0, (int)tamanhoBlobImg);

                                using (MemoryStream ms = new MemoryStream(bufferImg))
                                {
                                    setIconeAplicativo(Referencias.memoryStreamToBitmap(ms));
                                }

                                setIdAplicativo(idDePesquisa);
                                setNomeAplicativo(nome);
                                setCaminhoAplicativo(caminho);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar atalhos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Salvar(Aplicativos appParaSalvamento)
        {
            try
            {
                string nome = appParaSalvamento.getNomeAplicativo();
                string url = appParaSalvamento.getCaminhoAplicativo();
                byte[] icnEmBytes = Referencias.ConvertBitmapImageToByteArray(appParaSalvamento.getIconeAplicativo());

                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string insertCommand = "INSERT INTO AplicativosExtras (Nome, Caminho, Icon) VALUES (@nome, @Caminho, @icon)";
                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@Caminho", url);
                        command.Parameters.AddWithValue("@icon", icnEmBytes);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar o aplicativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void Alterar(Aplicativos appParaAlteracao)
        {
            try
            {
                int idDeAlteracao = appParaAlteracao.getIdAplicativo();
                string nome = appParaAlteracao.getNomeAplicativo();
                string caminho = appParaAlteracao.getCaminhoAplicativo();
                byte[] imgEmBytes = Referencias.ConvertBitmapImageToByteArray(appParaAlteracao.getIconeAplicativo());

                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string condicaoCommand = " WHERE id = @id";
                    string insertCommand = "UPDATE AplicativosExtras SET Nome = @nome, Caminho = @caminho";

                    if (imgEmBytes.Length != 0) { condicaoCommand = ", Icon = @icon" + condicaoCommand; }

                    insertCommand = insertCommand + condicaoCommand;

                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@caminho", caminho);
                        if (imgEmBytes.Length != 0) { command.Parameters.AddWithValue("@icon", imgEmBytes); }
                        command.Parameters.AddWithValue("@id", idDeAlteracao);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao alterar o aplicativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void Deletar(int idDeExclusao)
        {
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string deleteCommand = "DELETE FROM AplicativosExtras WHERE id = @id";

                    using (var command = new SqliteCommand(deleteCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idDeExclusao);

                        int rowsAffected = command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir o aplicativo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static ArrayList ConsultarIDs()
        {
            ArrayList idsApps = new ArrayList();
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = "SELECT Id FROM AplicativosExtras";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);

                            idsApps.Add(id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar aplicativos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return idsApps;
        }

        public int getIdAplicativo()
        {
            return this.id;
        }
        public void setIdAplicativo(int novoId)
        {
            this.id = novoId;
        }
        public string getNomeAplicativo()
        {
            return this.nome;
        }
        public void setNomeAplicativo(string novoNome)
        {
            this.nome = novoNome;
        }
        public string getCaminhoAplicativo()
        {
            return this.caminho;
        }
        public void setCaminhoAplicativo(string novoCaminho)
        {
            this.caminho = novoCaminho;
        }
        public BitmapImage getIconeAplicativo()
        {
            return this.icon;
        }
        public void setIconeAplicativo(BitmapImage novoIcon)
        {
            this.icon = novoIcon;
        }
    }

    public class Atalhos
    {
        private int id;
        private string nome;
        private string caminho;
        private string parametro;
        private BitmapImage img;
        private BitmapImage icon;
        private string dataUltimaSessao;
        private string tempoUltimaSessao;

        public Atalhos()
        {

        }
        public Atalhos(int idDePesquisa)
        {
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = "SELECT Nome, Caminho, Parametro, Imagem, Icon, DataUltimaSessao, TempoUltimaSessao FROM AtalhosdeAplicativos WHERE Id = @id";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idDePesquisa);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nome = reader.GetString(0);
                                string caminho = reader.GetString(1);
                                string parametro = reader.GetString(2);

                                long tamanhoBlobImg = reader.GetBytes(3, 0, null, 0, 0);

                                byte[] bufferImg = new byte[tamanhoBlobImg];
                                reader.GetBytes(3, 0, bufferImg, 0, (int)tamanhoBlobImg);

                                using (MemoryStream ms = new MemoryStream(bufferImg))
                                {
                                    setImgAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                long tamanhoBlobIcon = reader.GetBytes(4, 0, null, 0, 0);

                                byte[] bufferIcon = new byte[tamanhoBlobIcon];
                                reader.GetBytes(4, 0, bufferIcon, 0, (int)tamanhoBlobIcon);

                                using (MemoryStream ms = new MemoryStream(bufferIcon))
                                {
                                    setIconeAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                string dataSessao = reader.GetString(5);
                                string tempoSessao = reader.GetString(6);


                                setIdAtalho(idDePesquisa);
                                setNomeAtalho(nome);
                                setCaminhoAtalho(caminho);
                                setParametroAtalho(parametro);
                                setDataSessaoAtalho(dataSessao);
                                setTempoSessaoAtalho(tempoSessao);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar o atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Salvar(Atalhos atalhoParaSalvamento)
        {
            try
            {
                string nome = atalhoParaSalvamento.getNomeAtalho();
                string caminho = atalhoParaSalvamento.getCaminhoAtalho();
                string parametro = atalhoParaSalvamento.getParametroAtalho();
                byte[] imgEmBytes = Referencias.ConvertBitmapImageToByteArray(atalhoParaSalvamento.getImgAtalho());
                byte[] icnEmBytes = Referencias.ConvertBitmapImageToByteArray(atalhoParaSalvamento.getIconeAtalho());
                string dataSessao = "sessao não iniciada ainda";
                string tempoSessao = "0h00m";

                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string insertCommand = "INSERT INTO AtalhosdeAplicativos (Nome, Caminho, Parametro, Imagem, Icon, DataUltimaSessao, TempoUltimaSessao, DataTodasSessoes, TempoTodasSessoes) VALUES (@nome, @caminho, @parametro, @img, @icon, @datasessao, @temposessao, @datasessoes, @temposessoes)";
                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@caminho", caminho);
                        command.Parameters.AddWithValue("@parametro", parametro);
                        command.Parameters.AddWithValue("@img", imgEmBytes);
                        command.Parameters.AddWithValue("@icon", icnEmBytes);
                        command.Parameters.AddWithValue("@datasessao", dataSessao);
                        command.Parameters.AddWithValue("@temposessao", tempoSessao);
                        command.Parameters.AddWithValue("@datasessoes", dataSessao);
                        command.Parameters.AddWithValue("@temposessoes", tempoSessao);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar o atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void Alterar(Atalhos atalhoParaAlteracao)
        {
            try
            {
                int idDeAlteracao = atalhoParaAlteracao.getIdAtalho();
                string nome = atalhoParaAlteracao.getNomeAtalho();
                string caminho = atalhoParaAlteracao.getCaminhoAtalho();
                string parametro = atalhoParaAlteracao.getParametroAtalho();
                byte[] imgEmBytes = Referencias.ConvertBitmapImageToByteArray(atalhoParaAlteracao.getImgAtalho());
                byte[] icnEmBytes = Referencias.ConvertBitmapImageToByteArray(atalhoParaAlteracao.getIconeAtalho());

                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string condicaoCommand = " WHERE id = @id";
                    string insertCommand = "UPDATE AtalhosdeAplicativos SET Nome = @nome, Caminho = @caminho, Parametro = @parametro";

                    if (icnEmBytes.Length != 0) { condicaoCommand = ", Icon = @icn" + condicaoCommand; }
                    if (imgEmBytes.Length != 0) { condicaoCommand = ", Imagem = @img" + condicaoCommand; }

                    insertCommand = insertCommand + condicaoCommand;

                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@caminho", caminho);
                        command.Parameters.AddWithValue("@parametro", parametro);
                        if (imgEmBytes.Length != 0) { command.Parameters.AddWithValue("@img", imgEmBytes); }
                        if (icnEmBytes.Length != 0) { command.Parameters.AddWithValue("@icn", icnEmBytes); }
                        command.Parameters.AddWithValue("@id", idDeAlteracao);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao alterar o atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void Deletar(int idDeExclusao)
        {
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string deleteCommand = "DELETE FROM AtalhosdeAplicativos WHERE id = @id";

                    using (var command = new SqliteCommand(deleteCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idDeExclusao);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir o atalho: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static ArrayList ConsultarIDs()
        {
            ArrayList ids = new ArrayList();
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = "SELECT Id FROM AtalhosdeAplicativos";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                ids.Add(id);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar ids: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ids;
        }
        public static List<Atalhos> ConsultarAtalhos(ArrayList ids)
        {
            List<int> idsLista = ids.Cast<int>().ToList();
            List<Atalhos> atalhos = new List<Atalhos>();
            try
            {
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string idParameters = string.Join(", ", idsLista.Select((_, index) => $"@id{index}"));
                    string selectCommand = $@"SELECT Id, Nome, Caminho, Parametro, Imagem, Icon, DataUltimaSessao, TempoUltimaSessao FROM AtalhosdeAplicativos WHERE Id IN ({idParameters})";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        for (int i = 0; i < ids.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@id{i}", ids[i]);
                        }
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Atalhos atl = new Atalhos();
                                int id = reader.GetInt32(0);
                                string nome = reader.GetString(1);
                                string caminho = reader.GetString(2);
                                string parametro = reader.GetString(3);

                                long tamanhoBlobImg = reader.GetBytes(4, 0, null, 0, 0);

                                byte[] bufferImg = new byte[tamanhoBlobImg];
                                reader.GetBytes(4, 0, bufferImg, 0, (int)tamanhoBlobImg);

                                using (MemoryStream ms = new MemoryStream(bufferImg))
                                {
                                    atl.setImgAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                long tamanhoBlobIcon = reader.GetBytes(5, 0, null, 0, 0);

                                byte[] bufferIcon = new byte[tamanhoBlobIcon];
                                reader.GetBytes(5, 0, bufferIcon, 0, (int)tamanhoBlobIcon);

                                using (MemoryStream ms = new MemoryStream(bufferIcon))
                                {
                                    atl.setIconeAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                string dataSessao = reader.GetString(6);
                                string tempoSessao = reader.GetString(7);


                                atl.setIdAtalho(id);
                                atl.setNomeAtalho(nome);
                                atl.setCaminhoAtalho(caminho);
                                atl.setParametroAtalho(parametro);
                                atl.setDataSessaoAtalho(dataSessao);
                                atl.setTempoSessaoAtalho(tempoSessao);

                                atalhos.Add(atl);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar atalhos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return atalhos;
        }
        public static void SessaoIniciada(string dataInicio, int idatual)
        {
            try
            {
                string datasAcumuladas = "";
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string selectCommand = "SELECT DataTodasSessoes FROM AtalhosdeAplicativos WHERE Id = @id";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idatual);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                datasAcumuladas = reader.GetString(0);
                            }
                        }
                    }

                    if (datasAcumuladas.Equals("sessao não iniciada ainda"))
                    {
                        datasAcumuladas = "";
                    }

                    string updateCommand = "UPDATE AtalhosdeAplicativos SET DataUltimaSessao = @dataSessao, DataTodasSessoes = @dataSessoes WHERE Id = @id";
                    using (var command = new SqliteCommand(updateCommand, connection))
                    {
                        command.Parameters.AddWithValue("@dataSessao", dataInicio);
                        command.Parameters.AddWithValue("@dataSessoes", string.Join(", ", new[] { datasAcumuladas, dataInicio }.Where(s => !string.IsNullOrEmpty(s))));
                        command.Parameters.AddWithValue("@id", idatual);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao mudar data da sessao: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void SessaoFinalizada(string duracaoSessao, int idatual)
        {
            try
            {
                string horasNoBanco = "";
                using (var connection = new SqliteConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string selectCommand = "SELECT TempoTodasSessoes FROM AtalhosdeAplicativos WHERE Id = @id";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idatual);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                horasNoBanco = reader.GetString(0);
                            }
                        }
                    }

                    TimeSpan tempoTotal = Referencias.StringToHorario(horasNoBanco);
                    TimeSpan tempoAtual = Referencias.StringToHorario(duracaoSessao);

                    TimeSpan soma = tempoTotal + tempoAtual;

                    string horasAcumuladas = Referencias.HorarioToString(soma);

                    string updateCommand = "UPDATE AtalhosdeAplicativos SET TempoUltimaSessao = @tempoSessao, TempoTodasSessoes = @tempoSessoes WHERE Id = @id";
                    using (var command = new SqliteCommand(updateCommand, connection))
                    {
                        command.Parameters.AddWithValue("@tempoSessao", duracaoSessao);
                        command.Parameters.AddWithValue("@tempoSessoes", horasAcumuladas);
                        command.Parameters.AddWithValue("@id", idatual);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao mudar duracao da sessao: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int getIdAtalho()
        {
            return this.id;
        }
        public void setIdAtalho(int novoId)
        {
            this.id = novoId;
        }
        public string getNomeAtalho()
        {
            return this.nome;
        }
        public void setNomeAtalho(string novoNome)
        {
            this.nome = novoNome;
        }
        public string getCaminhoAtalho()
        {
            return this.caminho;
        }
        public void setCaminhoAtalho(string novoCaminho)
        {
            this.caminho = novoCaminho;
        }
        public string getParametroAtalho()
        {
            return this.parametro;
        }
        public void setParametroAtalho(string novoParametro)
        {
            this.parametro = novoParametro;
        }
        public BitmapImage getImgAtalho()
        {
            return this.img;
        }
        public void setImgAtalho(BitmapImage novaImg)
        {
            this.img = novaImg;
        }
        public BitmapImage getIconeAtalho()
        {
            return this.icon;
        }
        public void setIconeAtalho(BitmapImage novoIcon)
        {
            this.icon = novoIcon;
        }
        public string getDataSessaoAtalho()
        {
            return this.dataUltimaSessao;
        }
        public void setDataSessaoAtalho(string dataUltimaSessao)
        {
            this.dataUltimaSessao = dataUltimaSessao;
        }
        public string getTempoSessaoAtalho()
        {
            return this.tempoUltimaSessao;
        }
        public void setTempoSessaoAtalho(string tempoUltimaSessao)
        {
            this.tempoUltimaSessao = tempoUltimaSessao;
        }
    }

    public class Referencias
    {
        public static string connectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "applicationsShortcuts.db")}";
        public Referencias()
        {

        }

        public static BitmapImage memoryStreamToBitmap(MemoryStream ms)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }
        public static byte[] ConvertBitmapImageToByteArray(BitmapImage bitmapImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                encoder.Save(ms);

                return ms.ToArray();
            }
        }
        public static TimeSpan StringToHorario(string tempo)
        {
            string[] partes = tempo.Replace("h", ":").Replace("m", "").Split(':');
            int horas = int.Parse(partes[0]);
            int minutos = int.Parse(partes[1]);

            return new TimeSpan(horas, minutos, 0);
        }

        public static string HorarioToString(TimeSpan tempo)
        {
            int horas = (int)tempo.TotalHours;
            int minutos = tempo.Minutes;

            return $"{horas}h{minutos:D2}m";
        }
    }
}
