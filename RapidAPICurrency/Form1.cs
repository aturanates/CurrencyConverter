using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace RapidAPICurrency
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient;
        private Dictionary<string, string> currencyNames;
        private DateTime lastUpdateTime;
        private bool isConverting = false;

        
        private ComboBox cmbFromCurrency;
        private ComboBox cmbToCurrency;
        private TextBox txtFromAmount;
        private TextBox txtToAmount;
        private Label lblResult;
        private Label lblUpdateTime;
        private Label lblFromFlag;
        private Label lblToFlag;
        private Label lblFromName;
        private Label lblToName;
        private Button btnConvert;
        private Button btnSwap;
        private ProgressBar progressBar;
        private Panel resultPanel;

        public Form1()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            InitializeCurrencyNames();
            SetupModernUI();
        }

        private void InitializeCurrencyNames()
        {
            currencyNames = new Dictionary<string, string>
            {
                {"USD", "US Dollar"},
                {"EUR", "Euro"},
                {"GBP", "British Pound"},
                {"TRY", "Turkish Lira"},
                {"JPY", "Japanese Yen"},
                {"CHF", "Swiss Franc"},
                {"CAD", "Canadian Dollar"},
                {"AUD", "Australian Dollar"},
                {"CNY", "Chinese Yuan"},
                {"SEK", "Swedish Krona"},
                {"NOK", "Norwegian Krone"},
                {"DKK", "Danish Krone"},
                {"PLN", "Polish Zloty"},
                {"CZK", "Czech Koruna"},
                {"HUF", "Hungarian Forint"},
                {"RUB", "Russian Ruble"},
                {"INR", "Indian Rupee"},
                {"BRL", "Brazilian Real"},
                {"ZAR", "South African Rand"},
                {"KRW", "South Korean Won"}
            };
        }

        private void SetupModernUI()
        {
            
            this.Size = new Size(500, 600);
            this.MinimumSize = new Size(480, 550);
            this.Text = "Modern Döviz Çevirici";
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.StartPosition = FormStartPosition.CenterScreen;

            
            this.Controls.Clear();

            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };

            Label titleLabel = new Label
            {
                Text = "💱 Döviz Çevirici",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            lblUpdateTime = new Label
            {
                Text = "Son güncelleme: -",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(128, 128, 128),
                AutoSize = true,
                Location = new Point(20, 55)
            };

            GroupBox fromGroup = CreateFromCurrencyGroup();

            GroupBox toGroup = CreateToCurrencyGroup();

            btnSwap = new Button
            {
                Text = "⇅",
                Size = new Size(50, 30),
                Location = new Point(225, 195),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSwap.FlatAppearance.BorderSize = 0;
            btnSwap.Click += BtnSwap_Click;

            btnConvert = new Button
            {
                Text = "💰 Çevir",
                Size = new Size(200, 45),
                Location = new Point(150, 360),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConvert.FlatAppearance.BorderSize = 0;
            btnConvert.Click += BtnConvert_Click;

            resultPanel = new Panel
            {
                Size = new Size(440, 80),
                Location = new Point(20, 420),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblResult = new Label
            {
                Text = "Sonuç burada görünecek...",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(51, 51, 51),
                AutoSize = false,
                Size = new Size(420, 60),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            resultPanel.Controls.Add(lblResult);

            progressBar = new ProgressBar
            {
                Size = new Size(440, 5),
                Location = new Point(20, 505),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Visible = false
            };

            mainPanel.Controls.AddRange(new Control[] {
                titleLabel, lblUpdateTime, fromGroup, toGroup,
                btnSwap, btnConvert, resultPanel, progressBar
            });

            this.Controls.Add(mainPanel);
        }

        private GroupBox CreateFromCurrencyGroup()
        {
            GroupBox group = new GroupBox
            {
                Text = "Çevrilecek Para Birimi",
                Size = new Size(440, 100),
                Location = new Point(20, 90),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            txtFromAmount = new TextBox
            {
                Size = new Size(120, 25),
                Location = new Point(15, 30),
                Font = new Font("Segoe UI", 11),
                Text = "1"
            };
            txtFromAmount.TextChanged += TxtFromAmount_TextChanged;
            txtFromAmount.KeyPress += AmountBox_KeyPress;

            cmbFromCurrency = new ComboBox
            {
                Size = new Size(280, 25),
                Location = new Point(145, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var currency in currencyNames)
            {
                cmbFromCurrency.Items.Add($"{currency.Key} - {currency.Value}");
            }
            cmbFromCurrency.SelectedIndex = currencyNames.Keys.ToList().IndexOf("EUR");
            cmbFromCurrency.SelectedIndexChanged += CmbFromCurrency_SelectedIndexChanged;

            lblFromFlag = new Label
            {
                Size = new Size(30, 25),
                Location = new Point(15, 60),
                Font = new Font("Segoe UI", 16),
                Text = GetCurrencyFlag("EUR"),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblFromName = new Label
            {
                Size = new Size(280, 25),
                Location = new Point(55, 60),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(128, 128, 128),
                Text = currencyNames["EUR"],
                TextAlign = ContentAlignment.MiddleLeft
            };

            group.Controls.AddRange(new Control[] { txtFromAmount, cmbFromCurrency, lblFromFlag, lblFromName });
            return group;
        }

        private GroupBox CreateToCurrencyGroup()
        {
            GroupBox group = new GroupBox
            {
                Text = "Hedef Para Birimi",
                Size = new Size(440, 100),
                Location = new Point(20, 220),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 51, 51)
            };

            txtToAmount = new TextBox
            {
                Size = new Size(120, 25),
                Location = new Point(15, 30),
                Font = new Font("Segoe UI", 11),
                Text = "0",
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            cmbToCurrency = new ComboBox
            {
                Size = new Size(280, 25),
                Location = new Point(145, 30),
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var currency in currencyNames)
            {
                cmbToCurrency.Items.Add($"{currency.Key} - {currency.Value}");
            }
            cmbToCurrency.SelectedIndex = currencyNames.Keys.ToList().IndexOf("USD");
            cmbToCurrency.SelectedIndexChanged += CmbToCurrency_SelectedIndexChanged;

            lblToFlag = new Label
            {
                Size = new Size(30, 25),
                Location = new Point(15, 60),
                Font = new Font("Segoe UI", 16),
                Text = GetCurrencyFlag("USD"),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblToName = new Label
            {
                Size = new Size(280, 25),
                Location = new Point(55, 60),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(128, 128, 128),
                Text = currencyNames["USD"],
                TextAlign = ContentAlignment.MiddleLeft
            };

            group.Controls.AddRange(new Control[] { txtToAmount, cmbToCurrency, lblToFlag, lblToName });
            return group;
        }

        private string GetCurrencyFlag(string currencyCode)
        {
            switch (currencyCode)
            {
                case "USD": return "🇺🇸";
                case "EUR": return "🇪🇺";
                case "GBP": return "🇬🇧";
                case "TRY": return "🇹🇷";
                case "JPY": return "🇯🇵";
                case "CHF": return "🇨🇭";
                case "CAD": return "🇨🇦";
                case "AUD": return "🇦🇺";
                case "CNY": return "🇨🇳";
                case "SEK": return "🇸🇪";
                case "NOK": return "🇳🇴";
                case "DKK": return "🇩🇰";
                case "PLN": return "🇵🇱";
                case "CZK": return "🇨🇿";
                case "HUF": return "🇭🇺";
                case "RUB": return "🇷🇺";
                case "INR": return "🇮🇳";
                case "BRL": return "🇧🇷";
                case "ZAR": return "🇿🇦";
                case "KRW": return "🇰🇷";
                default: return "💱";
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ShowProgress(true);
                await ConvertCurrency();
                UpdateLastUpdateTime();
            }
            finally
            {
                ShowProgress(false);
            }
        }

        private void ShowProgress(bool show)
        {
            if (progressBar != null)
                progressBar.Visible = show;
        }

        private void UpdateLastUpdateTime()
        {
            lastUpdateTime = DateTime.Now;
            if (lblUpdateTime != null)
                lblUpdateTime.Text = $"Son güncelleme: {lastUpdateTime:HH:mm:ss}";
        }

        private async void BtnConvert_Click(object sender, EventArgs e)
        {
            if (isConverting) return;

            try
            {
                ShowProgress(true);
                await ConvertCurrency();
                UpdateLastUpdateTime();
            }
            finally
            {
                ShowProgress(false);
            }
        }

        private async Task ConvertCurrency()
        {
            if (isConverting) return;
            isConverting = true;

            try
            {
                if (!decimal.TryParse(txtFromAmount.Text, out decimal amount) || amount <= 0)
                {
                    lblResult.Text = "⚠️ Lütfen geçerli bir miktar girin!";
                    lblResult.ForeColor = Color.FromArgb(231, 76, 60);
                    return;
                }

                string fromCurrency = cmbFromCurrency.SelectedItem.ToString().Split('-')[0].Trim();
                string toCurrency = cmbToCurrency.SelectedItem.ToString().Split('-')[0].Trim();

                if (fromCurrency == toCurrency)
                {
                    txtToAmount.Text = amount.ToString("N4");
                    lblResult.Text = $"✅ {amount:N4} {fromCurrency} = {amount:N4} {toCurrency}";
                    lblResult.ForeColor = Color.FromArgb(46, 204, 113);
                    return;
                }

                string url = $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}";

                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                var rates = json["rates"];
                if (rates[toCurrency] != null)
                {
                    decimal rate = (decimal)rates[toCurrency];
                    decimal convertedAmount = amount * rate;

                    txtToAmount.Text = convertedAmount.ToString("N4");
                    lblResult.Text = $"✅ {amount:N4} {fromCurrency} = {convertedAmount:N4} {toCurrency}";
                    lblResult.ForeColor = Color.FromArgb(46, 204, 113);
                }
                else
                {
                    lblResult.Text = "❌ Döviz kuru bulunamadı!";
                    lblResult.ForeColor = Color.FromArgb(231, 76, 60);
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = $"❌ Hata: {ex.Message}";
                lblResult.ForeColor = Color.FromArgb(231, 76, 60);
            }
            finally
            {
                isConverting = false;
            }
        }

        private void BtnSwap_Click(object sender, EventArgs e)
        {
            int tempIndex = cmbFromCurrency.SelectedIndex;
            cmbFromCurrency.SelectedIndex = cmbToCurrency.SelectedIndex;
            cmbToCurrency.SelectedIndex = tempIndex;
        }

        private async void TxtFromAmount_TextChanged(object sender, EventArgs e)
        {
            if (!isConverting && !string.IsNullOrEmpty(txtFromAmount.Text))
            {
                await Task.Delay(500); 
                if (!isConverting)
                    await ConvertCurrency();
            }
        }

        private void AmountBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                e.KeyChar != '.' && e.KeyChar != ',')
            {
                e.Handled = true;
            }

            TextBox textBox = sender as TextBox;
            if ((e.KeyChar == '.' || e.KeyChar == ',') &&
                (textBox.Text.Contains('.') || textBox.Text.Contains(',')))
            {
                e.Handled = true;
            }
        }

        private async void CmbFromCurrency_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCurrency = cmbFromCurrency.SelectedItem.ToString().Split('-')[0].Trim();

            lblFromFlag.Text = GetCurrencyFlag(selectedCurrency);
            lblFromName.Text = currencyNames[selectedCurrency];

            if (!isConverting)
                await ConvertCurrency();
        }

        private async void CmbToCurrency_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCurrency = cmbToCurrency.SelectedItem.ToString().Split('-')[0].Trim();

            lblToFlag.Text = GetCurrencyFlag(selectedCurrency);
            lblToName.Text = currencyNames[selectedCurrency];

            if (!isConverting)
                await ConvertCurrency();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            httpClient?.Dispose();
            base.OnFormClosed(e);
        }
    }
}