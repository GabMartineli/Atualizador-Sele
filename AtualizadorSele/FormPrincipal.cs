using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AtualizadorSoftware.Models;
using AtualizadorSoftware.Services;



namespace AtualizadorSoftware
{
    public class FormPrincipal : Form
    {
        // ========== SERVIÇOS ==========
        private readonly ArquivoService _arquivoService = new ArquivoService();
        private readonly ScriptService _scriptService = new ScriptService();

        // ========== ESTADO ==========
        private string _pastaArquivosDetectada;
        private string _pastaScriptsDetectada;
        private List<string> _scriptsPaths = new List<string>();

        // ========== CONTROLES - Origem ==========
        private GroupBox grpOrigem;
        private TextBox txtPastaVersao;
        private Button btnSelecionarPasta;
        private Button btnSelecionarZip;
        private Label lblPastaArquivos;
        private Label lblPastaScripts;
        private Label lblVersaoDetectada;

        // ========== CONTROLES - Destino Arquivos ==========
        private GroupBox grpDestino;
        private TextBox txtPastaDestino;
        private Button btnSelecionarDestino;
        private Button btnCopiarArquivos;
        private ProgressBar progressArquivos;
        private Label lblProgressArquivos;

        // ========== CONTROLES - Banco de Dados ==========
        private GroupBox grpBanco;
        private TextBox txtInstancia;
        private ComboBox cmbBanco;
        private TextBox txtUsuario;
        private TextBox txtSenha;
        private Button btnTestarConexao;
        private Label lblStatusConexao;
        private CheckBox chkSalvarConfig;

        // ========== CONTROLES - Scripts ==========
        private GroupBox grpScripts;
        private CheckedListBox chkListScripts;
        private Button btnSelecionarTodos;
        private Button btnDesmarcarTodos;
        private Button btnExecutarScripts;
        private ProgressBar progressScripts;
        private Label lblProgressScripts;

        // ========== CONTROLES - Log ==========
        private GroupBox grpLog;
        private RichTextBox txtLog;
        private Button btnLimparLog;
        private Button btnSalvarLog;

        // ========== STATUS BAR ==========
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;

        public FormPrincipal()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            CarregarConfiguracoesSalvas();
        }

        private void InitializeComponent()
        {
            // ===== FORM =====
            this.Text = "Atualizador da Sele";
            this.Size = new Size(950, 820);
            this.MinimumSize = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);
            this.Icon = SystemIcons.Application;
            

            // Painel principal com scroll
            var panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            // GRUPO 1 - PASTA DA VERSÃO (Origem)
            grpOrigem = new GroupBox
            {
                Text = "Pasta da Atualização (Origem)",
                Location = new Point(10, 10),
                Size = new Size(910, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            var lblCaminho = new Label { Text = "Caminho:", Location = new Point(15, 28), AutoSize = true };
            txtPastaVersao = new TextBox
            {
                Location = new Point(85, 25),
                Size = new Size(530, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                PlaceholderText = @"S:\Sele\Sistemas\Atualização"
            };
            btnSelecionarPasta = new Button
            {
                Text = "📂 Pasta",
                Location = new Point(620, 23),
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSelecionarZip = new Button
            {
                Text = "📦 ZIP",
                Location = new Point(720, 23),
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            lblVersaoDetectada = new Label
            {
                Text = "",
                Location = new Point(15, 58),
                AutoSize = true,
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            lblPastaArquivos = new Label
            {
                Text = "Arquivos: (não detectado)",
                Location = new Point(15, 80),
                AutoSize = true,
                ForeColor = Color.Gray
            };
            lblPastaScripts = new Label
            {
                Text = "Scripts: (não detectado)",
                Location = new Point(15, 98),
                AutoSize = true,
                ForeColor = Color.Gray
            };

            btnSelecionarPasta.Click += BtnSelecionarPasta_Click;
            btnSelecionarZip.Click += BtnSelecionarZip_Click;

            grpOrigem.Controls.AddRange(new Control[] {
        lblCaminho, txtPastaVersao, btnSelecionarPasta, btnSelecionarZip,
        lblVersaoDetectada, lblPastaArquivos, lblPastaScripts
    });


            // GRUPO 2 - DESTINO DOS ARQUIVOS
            grpDestino = new GroupBox
            {
                Text = "Pasta do sistema (Destino)",
                Location = new Point(10, 140),
                Size = new Size(910, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left 
            };

            var lblDestino = new Label { Text = "Destino:", Location = new Point(15, 28), AutoSize = true };
            txtPastaDestino = new TextBox
            {
                Location = new Point(85, 25),
                Size = new Size(530, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                PlaceholderText = @"S:\Sele\Sistemas\Rieletronico"
            };
            btnSelecionarDestino = new Button
            {
                Text = "📂 Destino",
                Location = new Point(620, 23),
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                
            };
            btnCopiarArquivos = new Button
            {
                Text = "▶ Copiar",
                Location = new Point(720, 23),
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            progressArquivos = new ProgressBar
            {
                Location = new Point(85, 55),
                Size = new Size(580, 8),
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            lblProgressArquivos = new Label
            {
                Text = "",
                Location = new Point(675, 52),
                AutoSize = true,
                ForeColor = Color.DarkGreen
            };

            btnSelecionarDestino.Click += BtnSelecionarDestino_Click;
            btnCopiarArquivos.Click += BtnCopiarArquivos_Click;

            grpDestino.Controls.AddRange(new Control[] {
        lblDestino, txtPastaDestino, btnSelecionarDestino, btnCopiarArquivos,
        progressArquivos, lblProgressArquivos
    });


            // GRUPO 3 - CONEXÃO BANCO DE DADOS
            grpBanco = new GroupBox
            {
                Text = "Conexão com Banco de Dados",
                Location = new Point(10, 220),
                Size = new Size(910, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            var lblInst = new Label { Text = "Instância:", Location = new Point(15, 28), AutoSize = true };
            txtInstancia = new TextBox
            {
                Location = new Point(85, 25),
                Size = new Size(200, 25),
                PlaceholderText = @"SERVIDOR\SQLEXPRESS"
            };

            var lblBanco = new Label { Text = "Banco:", Location = new Point(300, 28), AutoSize = true };
            cmbBanco = new ComboBox
            {
                Location = new Point(355, 25),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDown
            };

            var btnCarregarBancos = new Button
            {
                Text = "🔄",
                Location = new Point(540, 23),
                Size = new Size(35, 28)
            };
            btnCarregarBancos.Click += BtnCarregarBancos_Click;

            btnTestarConexao = new Button
            {
                Text = "🔌 Testar",
                Location = new Point(585, 23),
                Size = new Size(100, 28)
            };

            chkSalvarConfig = new CheckBox
            {
                Text = "Lembrar config",
                Location = new Point(700, 28),
                AutoSize = true,
                Checked = true
            };

            var lblUser = new Label { Text = "Usuário:", Location = new Point(15, 62), AutoSize = true };
            txtUsuario = new TextBox
            {
                Location = new Point(85, 59),
                Size = new Size(200, 25),
                PlaceholderText = "usrrie"
            };

            var lblSenha = new Label { Text = "Senha:", Location = new Point(300, 62), AutoSize = true };
            txtSenha = new TextBox
            {
                Location = new Point(355, 59),
                Size = new Size(180, 25),
                UseSystemPasswordChar = true
            };

            lblStatusConexao = new Label
            {
                Text = "",
                Location = new Point(540, 62),
                AutoSize = true,
                MaximumSize = new Size(350, 0)
            };

            btnTestarConexao.Click += BtnTestarConexao_Click;

            grpBanco.Controls.AddRange(new Control[] {
        lblInst, txtInstancia, lblBanco, cmbBanco, btnCarregarBancos,
        lblUser, txtUsuario, lblSenha, txtSenha,
        btnTestarConexao, lblStatusConexao, chkSalvarConfig
    });


            // GRUPO 4 - SCRIPTS SQL
            grpScripts = new GroupBox
            {
                Text = "Scripts SQL",
                Location = new Point(10, 330),
                Size = new Size(910, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            chkListScripts = new CheckedListBox
            {
                Location = new Point(15, 25),
                Size = new Size(730, 150),
                CheckOnClick = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            btnSelecionarTodos = new Button
            {
                Text = "✅ Todos",
                Location = new Point(753, 25),
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnDesmarcarTodos = new Button
            {
                Text = "⬜ Nenhum",
                Location = new Point(753, 58),
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnExecutarScripts = new Button
            {
                Text = "▶ Executar",
                Location = new Point(753, 100),
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            progressScripts = new ProgressBar
            {
                Location = new Point(15, 185),
                Size = new Size(630, 12),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lblProgressScripts = new Label
            {
                Text = "",
                Location = new Point(653, 182),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            btnSelecionarTodos.Click += (s, e) =>
            {
                for (int i = 0; i < chkListScripts.Items.Count; i++)
                    chkListScripts.SetItemChecked(i, true);
            };
            btnDesmarcarTodos.Click += (s, e) =>
            {
                for (int i = 0; i < chkListScripts.Items.Count; i++)
                    chkListScripts.SetItemChecked(i, false);
            };
            btnExecutarScripts.Click += BtnExecutarScripts_Click;

            grpScripts.Controls.AddRange(new Control[] {
        chkListScripts, btnSelecionarTodos, btnDesmarcarTodos, btnExecutarScripts,
        progressScripts, lblProgressScripts
    });


            // GRUPO 5 - LOG
            grpLog = new GroupBox
            {
                Text = "Log de Execução",
                Location = new Point(10, 560),
                Size = new Size(910, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            txtLog = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(730, 150),
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                WordWrap = false
            };

            btnLimparLog = new Button
            {
                Text = "🗑 Limpar",
                Location = new Point(753, 22),
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSalvarLog = new Button
            {
                Text = "💾 Salvar",
                Location = new Point(753, 55),
                Size = new Size(80, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnLimparLog.Click += (s, e) => txtLog.Clear();
            btnSalvarLog.Click += BtnSalvarLog_Click;

            grpLog.Controls.AddRange(new Control[] { txtLog, btnLimparLog, btnSalvarLog });


            // STATUS BAR
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel { Text = "Pronto" };
            statusStrip.Items.Add(lblStatus);


            // MONTAGEM FINAL
            panelMain.Controls.AddRange(new Control[] {
        grpOrigem, grpDestino, grpBanco, grpScripts, grpLog
    });

            this.Controls.Add(panelMain);
            this.Controls.Add(statusStrip);
        }

        
        // EVENTOS - SELEÇÃO DE PASTA/ZIP
        private void BtnSelecionarPasta_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecione a pasta da versão";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtPastaVersao.Text = dialog.SelectedPath;
                    CarregarPastaVersao(dialog.SelectedPath);
                }
            }
        }

        private void BtnSelecionarZip_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Arquivos compactados|*.zip;*.rar;*.7z|Todos os arquivos|*.*";
                dialog.Title = "Selecione o ZIP da versão";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        LogInfo("Extraindo ZIP...");
                        lblStatus.Text = "Extraindo ZIP...";
                        Application.DoEvents();

                        string pastaExtraida = _arquivoService.ExtrairArquivo(dialog.FileName);
                        txtPastaVersao.Text = pastaExtraida;
                        CarregarPastaVersao(pastaExtraida);

                        LogSucesso($"ZIP extraído em: {pastaExtraida}");
                    }
                    catch (Exception ex)
                    {
                        LogErro($"Erro ao extrair ZIP: {ex.Message}");
                        MessageBox.Show($"Erro ao extrair ZIP:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CarregarPastaVersao(string caminho)
        {
            try
            {
                // Detecta nome da versão
                string nomeVersao = Path.GetFileName(caminho);
                lblVersaoDetectada.Text = $"Versão detectada: {nomeVersao}";

                // Detecta subpastas
                var (pastaArq, pastaScr) = _arquivoService.DetectarSubpastas(caminho);
                _pastaArquivosDetectada = pastaArq;
                _pastaScriptsDetectada = pastaScr;

                // Atualiza labels
                if (pastaArq != null)
                {
                    int qtdArq = Directory.GetFiles(pastaArq, "*.*", SearchOption.AllDirectories).Length;
                    lblPastaArquivos.Text = $"✅ Arquivos: {Path.GetFileName(pastaArq)} ({qtdArq} arquivos)";
                    lblPastaArquivos.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblPastaArquivos.Text = "⚠ Arquivos: não detectado";
                    lblPastaArquivos.ForeColor = Color.DarkOrange;
                }

                if (pastaScr != null)
                {
                    // Carrega scripts na lista
                    _scriptsPaths = _arquivoService.ListarScripts(pastaScr);
                    chkListScripts.Items.Clear();
                    foreach (var script in _scriptsPaths)
                    {
                        chkListScripts.Items.Add(Path.GetFileName(script), isChecked: true);
                    }

                    lblPastaScripts.Text = $"✅ Scripts: {Path.GetFileName(pastaScr)} ({_scriptsPaths.Count} scripts)";
                    lblPastaScripts.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblPastaScripts.Text = "⚠ Scripts: não detectado";
                    lblPastaScripts.ForeColor = Color.DarkOrange;
                    chkListScripts.Items.Clear();
                    _scriptsPaths.Clear();
                }

                lblStatus.Text = $"Versão carregada: {nomeVersao}";
                LogInfo($"Pasta carregada: {caminho}");
                LogInfo($"  → Arquivos: {pastaArq ?? "N/A"}");
                LogInfo($"  → Scripts: {pastaScr ?? "N/A"} ({_scriptsPaths.Count} encontrados)");
            }
            catch (Exception ex)
            {
                LogErro($"Erro ao carregar pasta: {ex.Message}");
            }
        }

      
        // EVENTOS - DESTINO E CÓPIA DE ARQUIVOS
        private void BtnSelecionarDestino_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Selecione a pasta de destino no servidor";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtPastaDestino.Text = dialog.SelectedPath;
                }
            }
        }

        private async void BtnCopiarArquivos_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_pastaArquivosDetectada))
            {
                MessageBox.Show("Nenhuma pasta de arquivos detectada. Carregue a pasta da versão primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtPastaDestino.Text))
            {
                MessageBox.Show("Informe a pasta de destino.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacao = MessageBox.Show(
                $"Copiar arquivos de:\n{_pastaArquivosDetectada}\n\nPara:\n{txtPastaDestino.Text}\n\nArquivos existentes serão sobrescritos. Continuar?",
                "Confirmar Cópia",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacao != DialogResult.Yes) return;

            btnCopiarArquivos.Enabled = false;
            progressArquivos.Visible = true;
            progressArquivos.Value = 0;
            lblStatus.Text = "Copiando arquivos...";
            LogInfo("Iniciando cópia de arquivos...");

            string origem = _pastaArquivosDetectada;
            string destino = txtPastaDestino.Text;

            var resultado = await Task.Run(() =>
                _arquivoService.CopiarArquivos(origem, destino, (percent, nome) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        progressArquivos.Value = Math.Min(percent, 100);
                        lblProgressArquivos.Text = $"{percent}%";
                    }));
                })
            );

            foreach (var linha in resultado.log)
            {
                if (linha.StartsWith("[OK]"))
                    LogSucesso(linha);
                else if (linha.StartsWith("[RENOMEADO]"))
                    LogInfo(linha);
                else if (linha.StartsWith("[ERRO]"))
                    LogErro(linha);
                else
                    LogInfo(linha);
            }

            progressArquivos.Value = 100;
            lblProgressArquivos.Text = $"✅ {resultado.copiados} copiados, {resultado.renomeados} renomeados, {resultado.erros} erros";
            lblStatus.Text = $"Cópia concluída: {resultado.copiados} copiados, {resultado.renomeados} renomeados";
            btnCopiarArquivos.Enabled = true;

            LogInfo($"=== Cópia finalizada: {resultado.copiados} copiados, {resultado.renomeados} renomeados, {resultado.erros} erros ===");
        }


        // EVENTOS - BANCO DE DADOS
        private async void BtnTestarConexao_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInstancia.Text) || string.IsNullOrEmpty(cmbBanco.Text))
            {
                MessageBox.Show("Preencha a instância e o banco de dados.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTestarConexao.Enabled = false;
            lblStatusConexao.Text = "Testando...";
            lblStatusConexao.ForeColor = Color.DarkBlue;

            string instancia = txtInstancia.Text;
            string banco = cmbBanco.Text;
            string usuario = txtUsuario.Text;
            string senha = txtSenha.Text;

            var resultado = await Task.Run(() =>
                _scriptService.TestarConexao(instancia, banco, usuario, senha)
            );

            if (resultado.sucesso)
            {
                lblStatusConexao.Text = "✅ Conectado!";
                lblStatusConexao.ForeColor = Color.DarkGreen;
                LogSucesso($"Conexão OK: {txtInstancia.Text} / {cmbBanco.Text}");
            }
            else
            {
                lblStatusConexao.Text = "❌ Falha";
                lblStatusConexao.ForeColor = Color.Red;
                LogErro($"Falha na conexão: {resultado.mensagem}");
            }

            btnTestarConexao.Enabled = true;
        }

        private async void BtnCarregarBancos_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInstancia.Text) || string.IsNullOrEmpty(txtUsuario.Text))
            {
                MessageBox.Show("Preencha a instância, usuário e senha primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblStatusConexao.Text = "Carregando bancos...";
                var bancos = await Task.Run(() =>
                {
                    var lista = new List<string>();
                    string connStr = $"Data Source={txtInstancia.Text};User ID={txtUsuario.Text};Password={txtSenha.Text};Connect Timeout=10;TrustServerCertificate=True;";
                    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", conn))
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                lista.Add(reader.GetString(0));
                        }
                    }
                    return lista;
                });

                cmbBanco.Items.Clear();
                foreach (var banco in bancos)
                    cmbBanco.Items.Add(banco);

                lblStatusConexao.Text = $"✅ {bancos.Count} bancos encontrados";
                lblStatusConexao.ForeColor = Color.DarkGreen;
                LogInfo($"Bancos carregados: {string.Join(", ", bancos)}");

                if (bancos.Count > 0)
                    cmbBanco.DroppedDown = true;
            }
            catch (Exception ex)
            {
                lblStatusConexao.Text = "❌ Erro ao listar bancos";
                lblStatusConexao.ForeColor = Color.Red;
                LogErro($"Erro ao listar bancos: {ex.Message}");
            }
        }

        
        // EVENTOS - EXECUÇÃO DE SCRIPTS
        private async void BtnExecutarScripts_Click(object sender, EventArgs e)
        {
            // Coleta scripts selecionados
            var scriptsSelecionados = new List<string>();
            for (int i = 0; i < chkListScripts.Items.Count; i++)
            {
                if (chkListScripts.GetItemChecked(i))
                    scriptsSelecionados.Add(_scriptsPaths[i]);
            }

            if (scriptsSelecionados.Count == 0)
            {
                MessageBox.Show("Selecione pelo menos um script para executar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtInstancia.Text) || string.IsNullOrEmpty(txtUsuario.Text) || string.IsNullOrEmpty(txtSenha.Text) || string.IsNullOrEmpty(cmbBanco.Text))
            {
                MessageBox.Show("Preencha os dados de conexão com o banco.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacao = MessageBox.Show(
                $"Executar {scriptsSelecionados.Count} script(s) no banco:\n{txtInstancia.Text} / {cmbBanco.Text}\n\nContinuar?",
                "Confirmar Execução",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmacao != DialogResult.Yes) return;

            btnExecutarScripts.Enabled = false;
            progressScripts.Maximum = scriptsSelecionados.Count;
            progressScripts.Value = 0;
            lblStatus.Text = "Executando scripts...";

            LogInfo($"=== Iniciando execução de {scriptsSelecionados.Count} script(s) ===");
            LogInfo($"Banco: {txtInstancia.Text} / {cmbBanco.Text}");

            string inst = txtInstancia.Text;
            string banco = cmbBanco.Text;
            string user = txtUsuario.Text;
            string pass = txtSenha.Text;

            var resultados = await Task.Run(() =>
                _scriptService.ExecutarScripts(inst, banco, user, pass, scriptsSelecionados,
                    (index, nome, sucesso) =>
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            progressScripts.Value = index;
                            lblProgressScripts.Text = $"{index}/{scriptsSelecionados.Count}";
                        }));
                    })
            );

            // Exibe resultados
            int ok = 0, falhas = 0, pulados = 0;
            double tempoTotal = 0;

            foreach (var res in resultados)
            {
                if (res.Sucesso == true && res.Mensagem != "Já executado anteriormente (pulado)")
                {
                    LogSucesso($"  ✅ {res.NomeScript} — {res.Mensagem} ({res.TempoSegundos:F1}s)");
                    ok++;
                }
                else if (res.Sucesso == true && res.Mensagem == "Já executado anteriormente (pulado)")
                {
                    LogPulado($"  ⚠️ {res.NomeScript} — {res.Mensagem}");
                    pulados++;
                }
                else
                {
                    LogErro($"  ❌ {res.NomeScript} — {res.Mensagem}");
                    falhas++;
                    progressScripts.Value = 0;
                }

                tempoTotal += res.TempoSegundos;
            }

            string resumo = $"=== Concluído: {ok} OK, {falhas} falha(s), {pulados} pulado(s) — tempo total: {tempoTotal:F1}s ===";

            if (falhas > 0)
                LogErro(resumo);
            else if (pulados > 0)
                LogPulado(resumo);
            else
                LogSucesso(resumo);

            lblStatus.Text = $"Scripts: {ok} OK, {falhas} falha(s), {pulados} pulado(s)";
            lblProgressScripts.Text = falhas > 0 ? $"⚠ {falhas} erro(s)" : "✅ Concluído";
            btnExecutarScripts.Enabled = true;

            // Salvar config se marcado
            if (chkSalvarConfig.Checked)
                SalvarConfiguracoes();
        }

        // LOG
      
        private void LogInfo(string msg)
        {
            AppendLog(msg, Color.LightGray);
        }

        private void LogSucesso(string msg)
        {
            AppendLog(msg, Color.LightGreen);
        }

        private void LogErro(string msg)
        {
            AppendLog(msg, Color.Salmon);
        }

        private void LogPulado(string msg)
        {
            AppendLog(msg, Color.Yellow);
        }

        private void AppendLog(string msg, Color cor)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => AppendLog(msg, cor)));
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionColor = Color.DarkGray;
            txtLog.AppendText($"[{timestamp}] ");
            txtLog.SelectionColor = cor;
            txtLog.AppendText(msg + Environment.NewLine);
            txtLog.ScrollToCaret();
        }

        private void BtnSalvarLog_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Arquivo de texto|*.txt|Todos os arquivos|*.*";
                dialog.FileName = $"log_atualizacao_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, txtLog.Text);
                    LogInfo($"Log salvo em: {dialog.FileName}");
                }
            }
        }

        // ============================================================
        // PERSISTÊNCIA DE CONFIGURAÇÕES
        // ============================================================

        private string ConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AtualizadorSoftware", "config.ini"
        );

        private void SalvarConfiguracoes()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var linhas = new[]
                {
                    $"instancia={txtInstancia.Text}",
                    $"banco={cmbBanco.Text}",
                    $"usuario={txtUsuario.Text}",
                    $"destino={txtPastaDestino.Text}"
                    // Senha NÃO é salva por segurança
                };

                File.WriteAllLines(ConfigPath, linhas);
            }
            catch { /* silencioso */ }
        }

        private void CarregarConfiguracoesSalvas()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return;

                var config = new Dictionary<string, string>();
                foreach (var linha in File.ReadAllLines(ConfigPath))
                {
                    var partes = linha.Split(new[] { '=' }, 2);
                    if (partes.Length == 2)
                        config[partes[0].Trim()] = partes[1].Trim();
                }

                if (config.ContainsKey("instancia")) txtInstancia.Text = config["instancia"];
                if (config.ContainsKey("banco")) cmbBanco.Text = config["banco"];
                if (config.ContainsKey("usuario")) txtUsuario.Text = config["usuario"];
                if (config.ContainsKey("destino")) txtPastaDestino.Text = config["destino"];

                LogInfo("Configurações anteriores carregadas.");
            }
            catch { /* silencioso */ }
        }
    }
}