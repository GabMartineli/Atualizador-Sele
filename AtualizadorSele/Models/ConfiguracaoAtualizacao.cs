using System.Collections.Generic;

namespace AtualizadorSoftware.Models
{
    public class ConfiguracaoAtualizacao
    {
        public string Instancia { get; set; }
        public string BancoDados { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string PastaOrigem { get; set; }       // pasta zipada/extraída com a versão
        public string PastaDestinoArquivos { get; set; } // onde os arquivos vão no servidor
        public List<string> ScriptsSelecionados { get; set; } = new List<string>();
    }

    public class ResultadoScript
    {
        public string NomeScript { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public double TempoSegundos { get; set; }
    }

    public enum StatusScript
    {
        Sucesso,
        Erro,
        Pulado
    }
}