using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AtualizadorSoftware.Models;

namespace AtualizadorSoftware.Services
{
    public class ScriptService
    {

        /// Testa a conexão com o banco de dados
        public (bool sucesso, string mensagem) TestarConexao(string instancia, string banco, string usuario, string senha)
        {
            try
            {
                string connStr = MontarConnectionString(instancia, banco, usuario, senha);
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    return (true, $"Conexão OK — SQL Server versão: {conn.ServerVersion}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Falha na conexão: {ex.Message}");
            }
        }

        /// Executa uma lista de scripts SQL no banco informado
        public List<ResultadoScript> ExecutarScripts(
    string instancia, string banco, string usuario, string senha,
    List<string> caminhosScripts,
    Action<int, string, StatusScript> progressCallback = null)
        {
            var resultados = new List<ResultadoScript>();
            string connStr = MontarConnectionString(instancia, banco, usuario, senha);

            for (int i = 0; i < caminhosScripts.Count; i++)
            {
                string caminhoScript = caminhosScripts[i];
                string nomeScript = Path.GetFileName(caminhoScript);

                // Verifica se o script já foi rodado
                if (ScriptJaExecutado(connStr, nomeScript))
                {
                    var resultadoPulado = new ResultadoScript
                    {
                        NomeScript = nomeScript,
                        Sucesso = true,
                        Mensagem = "Já executado anteriormente (pulado)",
                        TempoSegundos = 0
                    };
                    resultados.Add(resultadoPulado);
                    progressCallback?.Invoke(i + 1, nomeScript, StatusScript.Pulado);
                    continue; // Pula para o próximo script
                }

                var sw = Stopwatch.StartNew();

                try
                {
                    string conteudoSql = File.ReadAllText(caminhoScript);
                    var batches = SplitPorGO(conteudoSql);

                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        foreach (var batch in batches)
                        {
                            if (string.IsNullOrWhiteSpace(batch))
                                continue;

                            using (var cmd = new SqlCommand(batch, conn))
                            {
                                cmd.CommandTimeout = 300;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    sw.Stop();
                    var resultado = new ResultadoScript
                    {
                        NomeScript = nomeScript,
                        Sucesso = true,
                        Mensagem = $"Executado com sucesso ({batches.Count} Bloco(s))",
                        TempoSegundos = sw.Elapsed.TotalSeconds
                    };
                    resultados.Add(resultado);

                    using (var connHist = new SqlConnection(connStr))
                    {
                        connHist.Open();
                        SalvarHistorico(connHist, resultado, banco);
                    }

                    progressCallback?.Invoke(i + 1, nomeScript, StatusScript.Sucesso);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    var resultado = new ResultadoScript
                    {
                        NomeScript = nomeScript,
                        Sucesso = false,
                        Mensagem = ex.Message,
                        TempoSegundos = sw.Elapsed.TotalSeconds
                    };
                    resultados.Add(resultado);
                    progressCallback?.Invoke(i + 1, nomeScript, StatusScript.Erro);
                    break;
                }
            }

            return resultados;
        }

        //Verifica se o script já existe na tblhistoricoscripts
        private bool ScriptJaExecutado(string connStr, string nomeScript)
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string sql = "SELECT COUNT(1) FROM tblhistoricoscripts WHERE Script = @nome";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", nomeScript);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// Salva o registro do script executado no histórico
        private void SalvarHistorico(SqlConnection conn, ResultadoScript resultado, string banco)
        {
            
            if (resultado.Sucesso == true)
            {
                string sql = @"
            INSERT INTO tblhistoricoscripts 
            (Script, DataHora)
            VALUES 
            (@nome, @data)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nome", resultado.NomeScript);
                    cmd.Parameters.AddWithValue("@data", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

       
        /// Separa o script SQL por comandos GO
        private List<string> SplitPorGO(string sql)
        {
            // Regex para "GO" em linha isolada (padrão SQL Server)
            var partes = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return new List<string>(partes);
        }

        private string MontarConnectionString(string instancia, string banco, string usuario, string senha)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = instancia,
                InitialCatalog = banco,
                UserID = usuario,
                Password = senha,
                ConnectTimeout = 15,
                TrustServerCertificate = true
            };
            return builder.ConnectionString;
        }
    }
}