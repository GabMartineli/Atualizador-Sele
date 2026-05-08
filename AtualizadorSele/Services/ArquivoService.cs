using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace AtualizadorSoftware.Services
{
    public class ArquivoService
    {
       
        /// Extrai ZIP, RAR, 7z para uma pasta e retorna o caminho
        public string ExtrairArquivo(string caminhoArquivo, string pastaDestino = null)
        {
            if (string.IsNullOrEmpty(pastaDestino))
            {
                pastaDestino = Path.Combine(
                    Path.GetDirectoryName(caminhoArquivo),
                    Path.GetFileNameWithoutExtension(caminhoArquivo)
                );
            }

            if (Directory.Exists(pastaDestino))
                Directory.Delete(pastaDestino, true);

            Directory.CreateDirectory(pastaDestino);

            using (var stream = File.OpenRead(caminhoArquivo))
            using (var reader = ReaderFactory.OpenReader(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(pastaDestino, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }

            return pastaDestino;
        }
       

        /// Detecta automaticamente as subpastas de Arquivos e Scripts
        public (string pastaArquivos, string pastaScripts) DetectarSubpastas(string pastaVersao)
        {
            string pastaArquivos = null;
            string pastaScripts = null;

            var subpastas = Directory.GetDirectories(pastaVersao);

            // Se o ZIP extraiu com uma pasta raiz dentro, entra nela
            if (subpastas.Length == 1)
            {
                var conteudoInterno = Directory.GetDirectories(subpastas[0]);
                if (conteudoInterno.Length >= 2)
                {
                    subpastas = conteudoInterno;
                    pastaVersao = subpastas[0];
                }
            }

            foreach (var sub in subpastas)
            {
                string nome = Path.GetFileName(sub).ToLower();

                if (nome.Contains("arquivos") || nome.Contains("arquivo"))
                    pastaArquivos = sub;
                else if (nome.Contains("scripts") || nome.Contains("script"))
                    pastaScripts = sub;
            }

            // Se não detectou por nome, tenta pela primeira e segunda pasta
            if (pastaArquivos == null && pastaScripts == null && subpastas.Length >= 2)
            {
                pastaArquivos = subpastas[0];
                pastaScripts = subpastas[1];
            }

            return (pastaArquivos, pastaScripts);
        }

        
        /// Lista todos os scripts SQL encontrados na pasta
        public List<string> ListarScripts(string pastaScripts)
        {
            if (string.IsNullOrEmpty(pastaScripts) || !Directory.Exists(pastaScripts))
                return new List<string>();

            return Directory.GetFiles(pastaScripts, "*.sql", SearchOption.AllDirectories)
                           .OrderBy(f => Path.GetFileName(f))
                           .ToList();
        }


        /// Copia os arquivos da pasta de origem para a pasta de destino
        public (int copiados, int renomeados, int erros, List<string> log) CopiarArquivos(
        string pastaOrigem, string pastaDestino, Action<int, string> progressCallback = null)
        {
            var log = new List<string>();
            int copiados = 0;
            int renomeados = 0;
            int erros = 0;

            if (!Directory.Exists(pastaOrigem))
            {
                log.Add($"[ERRO] Pasta de origem não encontrada: {pastaOrigem}");
                return (0, 0, 1, log);
            }

            if (!Directory.Exists(pastaDestino))
            {
                Directory.CreateDirectory(pastaDestino);
                log.Add($"[INFO] Pasta de destino criada: {pastaDestino}");
            }

            var arquivos = Directory.GetFiles(pastaOrigem);
            int total = arquivos.Length;

            for (int i = 0; i < arquivos.Length; i++)
            {
                string nomeArquivo = Path.GetFileName(arquivos[i]);
                string destino = Path.Combine(pastaDestino, nomeArquivo);

                try
                {
                    try
                    {
                        File.Copy(arquivos[i], destino, overwrite: true);
                        copiados++;
                        log.Add($"[OK] {nomeArquivo}");
                    }
                    catch (IOException)
                    {
                        try
                        {
                            string nome = Path.GetFileNameWithoutExtension(destino);
                            string extensao = Path.GetExtension(destino);
                            string destinoOld = Path.Combine(pastaDestino, $"{nome}-old{extensao}");

                            if (File.Exists(destinoOld))
                            {
                                try
                                {
                                    File.Delete(destinoOld);
                                }
                                catch
                                {
                                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                                    destinoOld = Path.Combine(pastaDestino, $"{nome}-old-{timestamp}{extensao}");
                                }
                            }

                            File.Move(destino, destinoOld);
                            File.Copy(arquivos[i], destino, overwrite: true);

                            renomeados++;
                            log.Add($"[RENOMEADO] {nomeArquivo} → antigo virou {Path.GetFileName(destinoOld)}");
                        }
                        catch (Exception exRename)
                        {
                            erros++;
                            log.Add($"[ERRO] {nomeArquivo}: {exRename.Message}");
                        }
                    }

                    progressCallback?.Invoke((int)((i + 1.0) / total * 100), nomeArquivo);
                }
                catch (Exception ex)
                {
                    erros++;
                    log.Add($"[ERRO] {nomeArquivo}: {ex.Message}");
                }
            }

            return (copiados, renomeados, erros, log);
        }
    }
}