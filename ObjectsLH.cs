﻿using Microsoft.Data.Sqlite;
using SharpDX.DirectInput;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static LudoHive.NativeMethods;
using static System.Net.Mime.MediaTypeNames;

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
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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

                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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

                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
        public int id { get; set; }
        public int ordemDaPastaAtual { get; set; }
        public string nome { get; set; }
        public string caminho { get; set; }
        public string parametro { get; set; }
        public BitmapImage img { get; set; }
        public BitmapImage icon { get; set; }
        public string dataUltimaSessao { get; set; }
        public string tempoUltimaSessao { get; set; }

        public int idPasta { get; set; }
        public string nomePasta { get; set; }
        public int ordemPasta { get; set; }

        public Atalhos()
        {

        }
        public Atalhos(int idDePesquisa, int pastaAtual = 1)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = "SELECT pa.OrdemExibicao, a.Nome, a.Caminho, a.Parametro, a.Imagem, a.Icon, a.DataUltimaSessao, a.TempoUltimaSessao FROM AtalhosdeAplicativos a JOIN Pasta_Atalho pa ON a.Id = pa.Id_Atalho WHERE pa.Id_Pasta = @pasta AND a.Id = @id";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        command.Parameters.AddWithValue("@pasta", pastaAtual);
                        command.Parameters.AddWithValue("@id", idDePesquisa);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int ordem = reader.GetInt32(0);
                                string nome = reader.GetString(1);
                                string caminho = reader.GetString(2);
                                string parametro = reader.GetString(3);

                                long tamanhoBlobImg = reader.GetBytes(4, 0, null, 0, 0);

                                byte[] bufferImg = new byte[tamanhoBlobImg];
                                reader.GetBytes(4, 0, bufferImg, 0, (int)tamanhoBlobImg);

                                using (MemoryStream ms = new MemoryStream(bufferImg))
                                {
                                    setImgAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                long tamanhoBlobIcon = reader.GetBytes(5, 0, null, 0, 0);

                                byte[] bufferIcon = new byte[tamanhoBlobIcon];
                                reader.GetBytes(5, 0, bufferIcon, 0, (int)tamanhoBlobIcon);

                                using (MemoryStream ms = new MemoryStream(bufferIcon))
                                {
                                    setIconeAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                string dataSessao = reader.GetString(6);
                                string tempoSessao = reader.GetString(7);

                                setOrdemAtalho(ordem);
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

                int idPasta = atalhoParaSalvamento.getIdPasta();

                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string insertCommand = @"INSERT INTO AtalhosdeAplicativos (Nome, Caminho, Parametro, Imagem, Icon, DataUltimaSessao, TempoUltimaSessao, DataTodasSessoes, TempoTodasSessoes) VALUES (@nome, @caminho, @parametro, @img, @icon, @datasessao, @temposessao, @datasessoes, @temposessoes); SELECT last_insert_rowid();";
                    long idAtalho = 0;
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

                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            idAtalho = (long)result;
                        }
                        else
                        {
                            MessageBox.Show("Nenhum ID foi retornado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    string insertPastaAtalhoCommand = "INSERT INTO Pasta_Atalho (Id_Pasta, Id_Atalho, OrdemExibicao) SELECT @idPasta, @idAtalho, IFNULL(MAX(OrdemExibicao), 0) + 1 FROM Pasta_Atalho WHERE Id_Pasta = 1;";
                    using (var command = new SqliteCommand(insertPastaAtalhoCommand, connection))
                    {
                        command.Parameters.AddWithValue("@idPasta", idPasta);
                        command.Parameters.AddWithValue("@idAtalho", idAtalho);
                        command.ExecuteNonQuery();
                    }

                    Jogos jg = new()
                    {
                        id = (int)idAtalho,
                        nome = nome,
                        icon = atalhoParaSalvamento.getIconeAtalho(),
                        tempoUltimaSessao = tempoSessao,
                        tempoTodasSessoes = tempoSessao,
                        dataPrimeiraSessao = dataSessao,
                        dataUltimaSessao = dataSessao,
                        status = 0
                    };

                    Jogos.Salvar(jg);
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

                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
                        if (icnEmBytes.Length != 0) { command.Parameters.AddWithValue("@icn", icnEmBytes); }
                        if (imgEmBytes.Length != 0) { command.Parameters.AddWithValue("@img", imgEmBytes); }
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
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
        public static ArrayList ConsultarIDs(int pastaAtual = 1)
        {
            ArrayList ids = new ArrayList();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = "SELECT Id FROM Pasta_Atalho INNER JOIN AtalhosdeAplicativos ON Pasta_Atalho.Id_Atalho = AtalhosdeAplicativos.Id WHERE Pasta_Atalho.Id_Pasta = @idPasta ORDER BY OrdemExibicao ASC";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        command.Parameters.AddWithValue("@idPasta", pastaAtual);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ids.Add(reader.GetInt32(0));
                            }
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar ids: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ids;
        }
        public static ArrayList ConsultarIDsFaltantes(int pastaAtual = 1)
        {
            ArrayList ids = new ArrayList();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string selectCommand = $"SELECT Id FROM AtalhosdeAplicativos WHERE Id NOT IN ( SELECT Id_Atalho FROM Pasta_Atalho WHERE Id_Pasta = {pastaAtual} );";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ids.Add(reader.GetInt32(0));
                            }
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar ids: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return ids;
        }
        public static List<Atalhos> ConsultarAtalhos(ArrayList ids, int idPasta)
        {
            List<int> idsLista = ids.Cast<int>().ToList();
            List<Atalhos> atalhos = new List<Atalhos>();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string idParameters = string.Join(", ", idsLista.Select((_, index) => $"@id{index}"));
                    string selectCommand = $@"SELECT a.Id, a.Nome, a.Caminho, a.Parametro, a.Imagem, a.Icon, a.DataUltimaSessao, a.TempoUltimaSessao, pa.OrdemExibicao, pt.Nome FROM AtalhosdeAplicativos a JOIN Pasta_Atalho pa ON a.Id = pa.Id_Atalho INNER JOIN Pasta pt ON pa.Id_Pasta = pt.Id WHERE pa.Id_Pasta = {idPasta} AND a.Id IN ({idParameters}) ORDER BY pa.OrdemExibicao ASC";
                    if(idPasta == 0) { selectCommand = $@"SELECT a.Id, a.Nome, a.Caminho, a.Parametro, a.Imagem, a.Icon, a.DataUltimaSessao, a.TempoUltimaSessao FROM AtalhosdeAplicativos a WHERE a.Id IN ({idParameters})"; }
                    
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

                                if (idPasta != 0)
                                {
                                    int ordemExibicao = reader.GetInt32(8);
                                    string nomePasta = reader.GetString(9);

                                    atl.setOrdemAtalho(ordemExibicao);
                                    atl.setNomePasta(nomePasta);
                                }

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
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
                Jogos.SessaoIniciada(dataInicio, idatual);
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
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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

                    Jogos.SessaoFinalizada(duracaoSessao, horasAcumuladas, idatual);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao mudar duracao da sessao: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SalvarPasta(Atalhos pastaParaSalvamento)
        {
            string nome = pastaParaSalvamento.getNomePasta();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string insertCommand = "INSERT INTO Pasta (Nome, OrdemExibicao) SELECT @nome, IFNULL(MAX(OrdemExibicao), 0) + 1 FROM Pasta_Atalho";

                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar a pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void AlterarPasta(Atalhos pastaParaAlteracao)
        {
            string nome = pastaParaAlteracao.getNomePasta();
            int id = pastaParaAlteracao.getIdPasta();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string insertCommand = "UPDATE Pasta SET Nome = @nome WHERE Id = @id";

                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@id", id);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar a pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void DeletarPasta(int idPasta)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string deleteCommand = "DELETE FROM Pasta WHERE Id = @id";

                    using (var command = new SqliteCommand(deleteCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", idPasta);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao excluir a pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static List<Atalhos> ConsultarPasta()
        {
            List<Atalhos> atalhos = new List<Atalhos>();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = $@"SELECT Id, Nome, OrdemExibicao FROM Pasta ORDER BY OrdemExibicao ASC"; 
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Atalhos att = new Atalhos();
                                int id = reader.GetInt32(0);
                                string nome = reader.GetString(1);
                                int ordem = reader.GetInt32(2);

                                att.setIdPasta(id);
                                att.setNomePasta(nome);
                                att.setOrdemPasta(ordem);

                                atalhos.Add(att);
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

            return atalhos;
        }
        public static void AdicionarAtalhoNaPasta(List<int> idAtalhos, int idPasta)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        string insertPastaAtalhoCommand = "INSERT INTO Pasta_Atalho (Id_Pasta, Id_Atalho, OrdemExibicao) SELECT @idPasta, @idAtalho, IFNULL(MAX(OrdemExibicao), 0) + 1 FROM Pasta_Atalho WHERE Id_Pasta = @idPasta";

                        using (var command = new SqliteCommand(insertPastaAtalhoCommand, connection, transaction))
                        {
                            foreach (int idAtalho in idAtalhos)
                            {
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@idPasta", idPasta);
                                command.Parameters.AddWithValue("@idAtalho", idAtalho);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar o atalho na pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void RetirarAtalhoDaPasta(List<int> idAtalhos, int idPasta)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string idParameters = string.Join(", ", idAtalhos.Select((_, index) => $"@id{index}"));
                    string deleteCommand = $"DELETE FROM Pasta_Atalho WHERE Id_Pasta = {idPasta} AND Id_Atalho IN ({idParameters})";

                    using (var command = new SqliteCommand(deleteCommand, connection))
                    {
                        for (int i = 0; i < idAtalhos.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@id{i}", idAtalhos[i]);
                        }
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao retirar o atalho da pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static List<Atalhos> ConsultarPastas()
        {
            List<Atalhos> atalhos = new List<Atalhos>();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string selectCommand = $@"SELECT pa.Id_Pasta, pa.Id_Atalho, pa.OrdemExibicao, at.Nome, at.Caminho, at.Parametro, at.Imagem, at.Icon, at.DataUltimaSessao, at.TempoUltimaSessao, pt.Nome FROM Pasta_Atalho pa JOIN ( SELECT Id_Pasta, MIN(OrdemExibicao) AS OrdemMin FROM Pasta_Atalho GROUP BY Id_Pasta ) sub ON pa.Id_Pasta = sub.Id_Pasta AND pa.OrdemExibicao = sub.OrdemMin INNER JOIN AtalhosdeAplicativos at ON pa.Id_Atalho = at.Id INNER JOIN Pasta pt ON pa.Id_Pasta = pt.Id ORDER BY pt.OrdemExibicao ASC;";
                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Atalhos atl = new Atalhos();
                                int idPasta = reader.GetInt32(0);
                                int idAtalho = reader.GetInt32(1);
                                int ordemExibicao = reader.GetInt32(2);
                                string nome = reader.GetString(3);
                                string caminho = reader.GetString(4);
                                string parametro = reader.GetString(5);

                                long tamanhoBlobImg = reader.GetBytes(6, 0, null, 0, 0);

                                byte[] bufferImg = new byte[tamanhoBlobImg];
                                reader.GetBytes(6, 0, bufferImg, 0, (int)tamanhoBlobImg);

                                using (MemoryStream ms = new MemoryStream(bufferImg))
                                {
                                    atl.setImgAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                long tamanhoBlobIcon = reader.GetBytes(7, 0, null, 0, 0);

                                byte[] bufferIcon = new byte[tamanhoBlobIcon];
                                reader.GetBytes(7, 0, bufferIcon, 0, (int)tamanhoBlobIcon);

                                using (MemoryStream ms = new MemoryStream(bufferIcon))
                                {
                                    atl.setIconeAtalho(Referencias.memoryStreamToBitmap(ms));
                                }

                                string dataSessao = reader.GetString(8);
                                string tempoSessao = reader.GetString(9);
                                string nomePasta = reader.GetString(10);


                                atl.setIdAtalho(idAtalho);
                                atl.setIdPasta(idPasta);
                                atl.setOrdemAtalho(ordemExibicao);
                                atl.setNomeAtalho(nome);
                                atl.setCaminhoAtalho(caminho);
                                atl.setParametroAtalho(parametro);
                                atl.setDataSessaoAtalho(dataSessao);
                                atl.setTempoSessaoAtalho(tempoSessao);
                                atl.setNomePasta(nomePasta);

                                atalhos.Add(atl);
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

            return atalhos;
        }
        public static void AtualizarOrdem(List<int> idAtalhos, int idPasta)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new SqliteCommand($"UPDATE Pasta_Atalho SET OrdemExibicao = @ordem WHERE Id_Atalho = @id AND Id_Pasta = {idPasta}", connection, transaction))
                        {
                            for (int i = 0; i < idAtalhos.Count; i++)
                            {
                                command.Parameters.Clear();

                                command.Parameters.AddWithValue($"@ordem", i + 1);
                                command.Parameters.AddWithValue($"@id", idAtalhos[i]);

                                command.ExecuteNonQuery();
                            }

                        }
                        transaction.Commit();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ordenar atalhos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public int getIdAtalho()
        {
            return id;
        }
        public void setIdAtalho(int novoId)
        {
            id = novoId;
        }
        public int getOrdemAtalho()
        {
            return ordemDaPastaAtual;
        }
        public void setOrdemAtalho(int ordem)
        {
            ordemDaPastaAtual = ordem;
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
        
        public int getIdPasta()
        {
            return idPasta;
        }
        public void setIdPasta(int idPasta)
        {
            this.idPasta = idPasta;
        }
        public string getNomePasta()
        {
            return nomePasta;
        }
        public void setNomePasta(string nomePasta)
        {
            this.nomePasta = nomePasta;
        }
        public int getOrdemPasta()
        {
            return ordemPasta;
        }
        public void setOrdemPasta(int ordemPasta)
        {
            this.ordemPasta = ordemPasta;
        }
    }
    public class Jogos
    {
        public int id { get; set; }
        public string nome { get; set; }
        public BitmapImage icon { get; set; }
        public string dataUltimaSessao { get; set; }
        public string tempoUltimaSessao { get; set; }
        public string dataPrimeiraSessao { get; set; }
        public string tempoTodasSessoes { get; set; }
        public int status { get; set; }
        public Jogos()
        {

        }

        public static void Salvar(Jogos jogoParaSalvamento)
        {
            try
            {
                int id = jogoParaSalvamento.id;
                string nome = jogoParaSalvamento.nome;
                byte[] imgEmBytes = Referencias.ConvertBitmapImageToByteArray(jogoParaSalvamento.icon);
                string dataUltSessao = jogoParaSalvamento.dataUltimaSessao;
                string tempoSessao = jogoParaSalvamento.tempoUltimaSessao;
                string dataPrimSessao = jogoParaSalvamento.dataPrimeiraSessao;
                string tempoSessoes = jogoParaSalvamento.tempoTodasSessoes;
                int stats = jogoParaSalvamento.status;

                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string insertCommand = @"INSERT INTO Jogos (Id, Nome, Icon, DataUltimaSessao, TempoUltimaSessao, DataPrimeiraSessao, TempoTodasSessoes, Status) VALUES (@id, @nome, @icon, @datasessao, @temposessao, @dataprimsessao, @temposessoes, @stats);";
                    using (var command = new SqliteCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@icon", imgEmBytes);
                        command.Parameters.AddWithValue("@datasessao", dataUltSessao);
                        command.Parameters.AddWithValue("@temposessao", tempoSessao);
                        command.Parameters.AddWithValue("@dataprimsessao", dataPrimSessao);
                        command.Parameters.AddWithValue("@temposessoes", tempoSessoes);
                        command.Parameters.AddWithValue("@stats", stats);

                        command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao adicionar o jogo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /*public static void Alterar(Atalhos atalhoParaAlteracao)
        {
            try
            {
                int idDeAlteracao = atalhoParaAlteracao.getIdAtalho();
                string nome = atalhoParaAlteracao.getNomeAtalho();
                string caminho = atalhoParaAlteracao.getCaminhoAtalho();
                string parametro = atalhoParaAlteracao.getParametroAtalho();
                byte[] imgEmBytes = Referencias.ConvertBitmapImageToByteArray(atalhoParaAlteracao.getImgAtalho());
                byte[] icnEmBytes = Referencias.ConvertBitmapImageToByteArray(atalhoParaAlteracao.getIconeAtalho());

                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
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
                        if (icnEmBytes.Length != 0) { command.Parameters.AddWithValue("@icn", icnEmBytes); }
                        if (imgEmBytes.Length != 0) { command.Parameters.AddWithValue("@img", imgEmBytes); }
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
        }*/
        public static void Deletar(int idDeExclusao)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string deleteCommand = "DELETE FROM Jogos WHERE id = @id";

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
                MessageBox.Show($"Erro ao excluir o jogo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static List<Jogos> ConsultarJogos()
        {
            List<Jogos> jgs = new List<Jogos>();
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();
                    string selectCommand = $@"SELECT Id, Nome, Icon, DataUltimaSessao, TempoUltimaSessao, DataPrimeiraSessao, TempoTodasSessoes, Status FROM Jogos";

                    using (var command = new SqliteCommand(selectCommand, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Jogos jg = new Jogos();
                                jg.id = reader.GetInt32(0);
                                jg.nome = reader.GetString(1);

                                long tamanhoBlobIcon = reader.GetBytes(2, 0, null, 0, 0);

                                byte[] bufferIcon = new byte[tamanhoBlobIcon];
                                reader.GetBytes(2, 0, bufferIcon, 0, (int)tamanhoBlobIcon);

                                using (MemoryStream ms = new MemoryStream(bufferIcon))
                                {
                                    jg.icon = Referencias.memoryStreamToBitmap(ms);
                                }

                                jg.dataUltimaSessao = reader.GetString(3);
                                jg.tempoUltimaSessao = reader.GetString(4);
                                jg.dataPrimeiraSessao = reader.GetString(5);
                                jg.tempoTodasSessoes = reader.GetString(6);
                                jg.status = reader.GetInt32(7);

                                jgs.Add(jg);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar jogos: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return jgs;
        }
        public static void SessaoIniciada(string dataInicio, int idatual)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string updateCommand = "UPDATE Jogos SET DataUltimaSessao = @dataSessao WHERE Id = @id";
                    using (var command = new SqliteCommand(updateCommand, connection))
                    {
                        command.Parameters.AddWithValue("@dataSessao", dataInicio);
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
        public static void SessaoFinalizada(string duracaoSessao, string todasSessoes, int idatual)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string updateCommand = "UPDATE Jogos SET TempoUltimaSessao = @tempoSessao, TempoTodasSessoes = @tempoSessoes WHERE Id = @id";
                    using (var command = new SqliteCommand(updateCommand, connection))
                    {
                        command.Parameters.AddWithValue("@tempoSessao", duracaoSessao);
                        command.Parameters.AddWithValue("@tempoSessoes", todasSessoes);
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
        public static void AlterarStatus(int status, int idatual)
        {
            try
            {
                using (var connection = Referencias.CreateConnection(Referencias.connectionString))
                {
                    connection.Open();

                    string updateCommand = "UPDATE Jogos SET Status = @stst WHERE Id = @id";
                    using (var command = new SqliteCommand(updateCommand, connection))
                    {
                        command.Parameters.AddWithValue("@stst", status);
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
    }

    public class Referencias
    {
        public static string connectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "applicationsShortcuts.db")}";
        public static Uri imgPrincipal = new Uri("pack://application:,,,/LudoHive;component/Assets/Morgan.jpg", UriKind.Absolute);
        public static Uri ludoIcon = new Uri("pack://application:,,,/LudoHive;component/Assets/LudoHive_Logo.png", UriKind.Absolute);
        public static Uri controlOn = new Uri("pack://application:,,,/LudoHive;component/Assets/ControlON.png", UriKind.Absolute);
        public static Uri picAppsHide = new Uri("pack://application:,,,/LudoHive;component/Assets/PicAppsHide.png", UriKind.Absolute);
        public static Uri picAppsShow = new Uri("pack://application:,,,/LudoHive;component/Assets/PicAppsShow.png", UriKind.Absolute);
        public static Uri vinheta = new Uri("pack://application:,,,/LudoHive;component/Assets/Vinheta.png", UriKind.Absolute);
        public static Uri deletar = new Uri("pack://application:,,,/LudoHive;component/Assets/Deletar.png", UriKind.Absolute);
        public static Uri expandir = new Uri("pack://application:,,,/LudoHive;component/Assets/Expandir.png", UriKind.Absolute);
        public static Uri editar = new Uri("pack://application:,,,/LudoHive;component/Assets/Editar.png", UriKind.Absolute);

        public static Uri abandonado = new Uri("pack://application:,,,/LudoHive;component/Assets/Abandonado.png", UriKind.Absolute);
        public static Uri andamento = new Uri("pack://application:,,,/LudoHive;component/Assets/Em Andamento.png", UriKind.Absolute);
        public static Uri gameService = new Uri("pack://application:,,,/LudoHive;component/Assets/Game Service.png", UriKind.Absolute);
        public static Uri finalizado = new Uri("pack://application:,,,/LudoHive;component/Assets/Finalizado.png", UriKind.Absolute);
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
        public static SqliteConnection CreateConnection(string connectionString)
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            using (var command = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }
        public static void CriarClip(UIElement elemento, double height, double width, double arqueamento, bool arrLT = true, bool arrRT = true, bool arrRB = true, bool arrLB = true)
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
    public class Controle{
        private DirectInput _directInput;
        private Joystick _joystick;
        public System.Timers.Timer timerControle;
        private IntPtr _notificationHandle;
        private bool jogoEmUso = false;

        private Dictionary<string, DateTime> _cooldowns = new Dictionary<string, DateTime>();
        private TimeSpan _cooldownTime = TimeSpan.FromMilliseconds(300);

        public event Action btnQ, btnB, btnX, btnT;
        public event Action moveRight, moveLeft, moveUp, moveDown;
        public event Action controlDisconnect;

        private static readonly Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030"); // HID class GUID

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        public Controle(Joystick joy)
        {
            _joystick = joy;
            IniciarTimerMonitoramentoControle();
        }
        public Controle(IntPtr lParam)
        {
            DetectJoystick(lParam);
            IniciarTimerMonitoramentoControle();
        }

        public Controle()
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public short dbcc_name;
        }
        private void DetectJoystick(IntPtr lParam)
        {
            var hdr = Marshal.PtrToStructure<DEV_BROADCAST_HDR>(lParam);

            if (hdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
            {
                var deviceInterface = Marshal.PtrToStructure<DEV_BROADCAST_DEVICEINTERFACE>(lParam);
                if (deviceInterface.dbcc_classguid == GUID_DEVINTERFACE_HID)
                {
                    _directInput = new DirectInput();
                    foreach (var deviceInstance in _directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly))
                    {
                        var joystickGuid = deviceInstance.InstanceGuid;

                        if (joystickGuid != Guid.Empty)
                        {
                            Joystick _joystick = new Joystick(_directInput, joystickGuid);
                            _joystick.Acquire();

                            this._joystick = _joystick;
                        }
                    }
                }
            }
        }
        private void IniciarTimerMonitoramentoControle()
        {
            timerControle = new System.Timers.Timer { Interval = 16 };
            timerControle.Elapsed += ControleInputs;
            timerControle.Start();
        }
        private void ControleInputs(object sender, EventArgs e)
        {
            try
            {
                if (_joystick == null || jogoEmUso)
                {
                    timerControle.Stop();
                    controlDisconnect?.Invoke();
                    return;
                }

                _joystick.Poll();
                var state = _joystick.GetCurrentState();

                if (state == null) return;

                // Obter valores dos botoes
                var buttons = state.Buttons;

                // Obter valores das setas
                var dPad = state.PointOfViewControllers[0];

                // Obter valores dos analógicos
                var xAnalog = state.X;
                var yAnalog = state.Y;
                var zAnalog = state.Z;
                var rzAnalog = state.RotationZ;

                // Normalizar valores (-1.0 a 1.0)
                float leftX = (xAnalog - 32767f) / 32767f;
                float leftY = (yAnalog - 32767f) / 32767f;
                float rightX = (zAnalog - 32767f) / 32767f;
                float rightY = (rzAnalog - 32767f) / 32767f;

                // Executar métodos dependendo dos valores
                if ((leftX > 0.5 || dPad == 9000) && CanExecute("MoveRight")) moveRight?.Invoke();
                else if ((leftX < -0.5 || dPad == 27000) && CanExecute("MoveLeft")) moveLeft?.Invoke();

                if ((leftY > 0.5 || dPad == 18000) && CanExecute("MoveUp")) moveUp?.Invoke();
                else if ((leftY < -0.5 || dPad == 0) && CanExecute("MoveDown")) moveDown?.Invoke();

                if (buttons.Length > 1 && buttons[0] && CanExecute("BtnQ")) btnQ?.Invoke();
                if (buttons.Length > 1 && buttons[1] && CanExecute("BtnX")) btnX?.Invoke();
                if (buttons.Length > 1 && buttons[2] && CanExecute("BtnB")) btnB?.Invoke();
                if (buttons.Length > 1 && buttons[3] && CanExecute("BtnT")) btnT?.Invoke();
            }
            catch (SharpDX.SharpDXException)
            {
                timerControle.Stop();
                controlDisconnect?.Invoke();
            }
        }
        private bool CanExecute(string actionName)
        {
            if (!_cooldowns.ContainsKey(actionName) || DateTime.Now - _cooldowns[actionName] >= _cooldownTime)
            {
                _cooldowns[actionName] = DateTime.Now;
                return true;
            }
            return false;
        }
    }

    public struct MonitorInfo
    {
        public IntPtr MonitorHandle;
        public RECT MonitorArea;
    }

    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private static readonly Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030"); // HID class GUID
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnregisterDeviceNotification(IntPtr Handle);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        public const uint WS_OVERLAPPEDWINDOW = 0x00CF0000; // Estilo de janela padrão
        public const uint WS_POPUP = 0x80000000;           // Estilo de janela sem bordas
        public const uint WS_EX_TOOLWINDOW = 0x00000080;

        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;

        public const int GWL_STYLE = -16;
        public const int WS_BORDER = 0x00800000;
        public const int WS_CAPTION = 0x00C00000;
        public const int WS_THICKFRAME = 0x00040000;

        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public override string ToString()
            {
                return $"[Left = {left}, Top = {top}, Right = {right}, Bottom = {bottom}]";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public short dbcc_name;
        }

        // -=+ METODOS +=- //

        public static bool IsFullscreenWithoutBorders(Process processo)
        {
            IntPtr hwnd = FindWindow(null, processo.MainWindowTitle);
            if (hwnd != IntPtr.Zero)
            {
                RECT rect;
                GetWindowRect(hwnd, out rect);

                // Obter dimensões da tela
                int screenWidth = GetSystemMetrics(SM_CXSCREEN);
                int screenHeight = GetSystemMetrics(SM_CYSCREEN);

                if (rect.right - rect.left >= screenWidth && rect.bottom - rect.top >= screenHeight)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasBorder(Process processo)
        {
            IntPtr hwnd = FindWindow(null, processo.MainWindowTitle);
            if (hwnd != IntPtr.Zero)
            {
                int style = GetWindowLong(hwnd, GWL_STYLE);
                return (style & (WS_BORDER | WS_CAPTION | WS_THICKFRAME)) != 0;
            }
            return false;
        }

        public static List<MonitorInfo> GetAllMonitors()
        {
            List<MonitorInfo> monitors = new List<MonitorInfo>();

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
            {
                var monitorInfo = new NativeMethods.MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
                NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo);

                monitors.Add(new MonitorInfo
                {
                    MonitorHandle = hMonitor,
                    MonitorArea = monitorInfo.rcMonitor
                });
                return true;
            }, IntPtr.Zero);

            return monitors;
        }
        public static void MoveToMonitor(int numMonitor, Window wd)
        {
            List<MonitorInfo> monitors = GetAllMonitors();
            if (monitors.Count < numMonitor)
            {
                return;
            }

            MonitorInfo monitor = monitors[numMonitor - 1];

            RECT rect = new RECT
            {
                left = monitor.MonitorArea.left,
                top = monitor.MonitorArea.top,
                right = monitor.MonitorArea.right,
                bottom = monitor.MonitorArea.bottom
            };

            // Ajusta o tamanho da janela para incluir bordas e decorações
            uint style = WS_POPUP; // Use WS_OVERLAPPEDWINDOW se a janela tiver bordas
            uint exStyle = WS_EX_TOOLWINDOW; // Estilo estendido (opcional)
            AdjustWindowRectEx(ref rect, style, false, exStyle);

            IntPtr windowHandle = new WindowInteropHelper(wd).Handle;

            bool result = NativeMethods.SetWindowPos(
                windowHandle,
                IntPtr.Zero,
                monitor.MonitorArea.left - 7,
                monitor.MonitorArea.top - 7,
                monitor.MonitorArea.right - monitor.MonitorArea.left + 14,
                monitor.MonitorArea.bottom - monitor.MonitorArea.top + 14,
                NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }

        public static void RegisterForDeviceNotifications(Window wd, IntPtr _notificationHandle)
        {
            var dbi = new DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
                dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
                dbcc_classguid = GUID_DEVINTERFACE_HID
            };

            IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(dbi));
            Marshal.StructureToPtr(dbi, buffer, true);

            IntPtr windowHandle = new WindowInteropHelper(wd).Handle; // Obtém o HWND da janela WPF
            _notificationHandle = RegisterDeviceNotification(windowHandle, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);

            Marshal.FreeHGlobal(buffer);

            if (_notificationHandle == IntPtr.Zero)
            {
                throw new Exception("Falha ao registrar notificações de dispositivo.");
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
