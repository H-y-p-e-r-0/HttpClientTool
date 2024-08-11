using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HttpClientTool
{
    public partial class Form1 : Form
    {
        private event Action<string> DataTextEvent;
        private event Action<string> EncryptPathChanged;
        private Dictionary<int, encryptType> EncryptPath = new Dictionary<int, encryptType>();

        public Form1()
        {
            InitializeComponent();
            richTextBox1.DragDrop += richTextBoxDragDrop;
            richTextBox1.DragEnter += RichTextBox1_DragEnter;
            DataTextEvent += WriteResultDataView;
            EncryptPathChanged += WriteResultDataView;
            WriteResultDataView(richTextBox1.Text);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(1, encryptType.aes);
        private void radioButton2_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(1, encryptType.base64);
        private void radioButton3_CheckedChanged_1(object sender, EventArgs e) => AddEncryptPath(1, encryptType.sha256);
        private void radioButton4_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(1, encryptType.sha512);
        private void radioButton8_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(2, encryptType.aes);
        private void radioButton7_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(2, encryptType.base64);
        private void radioButton6_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(2, encryptType.sha256);
        private void radioButton5_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(2, encryptType.sha512);
        private void radioButton12_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(3, encryptType.aes);
        private void radioButton11_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(3, encryptType.base64);
        private void radioButton10_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(3, encryptType.sha256);
        private void radioButton9_CheckedChanged(object sender, EventArgs e) => AddEncryptPath(3, encryptType.sha512);
        private void richTextBox1_TextChanged(object sender, EventArgs e) => DataTextEvent?.Invoke(richTextBox1.Text);
        private void button2_Click(object sender, EventArgs e) => ResetSecurityButtons();
        private void button3_Click(object sender, EventArgs e) => GenerateRandomAesKey();

        private void Form1_Load(object sender, EventArgs e) { }

        private void label7_Click(object sender, EventArgs e) { }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }

        private void textBox4_TextChanged(object sender, EventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void textBox2_TextChanged(object sender, EventArgs e) { }

        private void textBox3_TextChanged(object sender, EventArgs e) { }

        // Check the dragged file.
        private void RichTextBox1_DragEnter(object sender, DragEventArgs e)
        {
            // Check the dragged files.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }


        // Receive and write the contents of the file.
        private void richTextBoxDragDrop(object sender, DragEventArgs e)
        {
            // Get the dragged files.
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Read the files.
            foreach (string file in files)
            {
                // Check the file.
                if (File.Exists(file))
                {
                    try
                    {
                        string fileContent = File.ReadAllText(file);
                        richTextBox1.Text += fileContent + Environment.NewLine;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"File read error: {ex.Message}", "[Hyper] HttpClientTool say:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Sender button.
        private void button1_Click(object sender, EventArgs e)
        {

            // Check the ComboBox.
            if (comboBox1.SelectedItem == null || string.IsNullOrEmpty(comboBox1.SelectedItem.ToString()))
            {
                MessageBox.Show("\"Type\" cannot be null or empty, please select it.", "[Hyper] HttpClientTool say:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check the TextBox (URL).
            if (string.IsNullOrEmpty(textBox4.Text))
            {
                MessageBox.Show("\"Url\" cannot be null or empty, please fill it.", "[Hyper] HttpClientTool say:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the selected ComboBox item/text.
            string selectedText = comboBox1.SelectedItem.ToString();

            // Parse the selected ComboBox item/text to sendType enum.
            sendType sType = (sendType)Enum.Parse(typeof(sendType), selectedText, true);

            // Send data to web.
            SendData(sType, richTextBox3.Text, textBox4.Text);
        }

        #region Encryption

        // Encrypt data using Base64 algorithm.
        string EncryptBase64(string text)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(text);
            string base64String = Convert.ToBase64String(plainTextBytes);
            return base64String;
        }

        // Encrypt data using AES algorithm.
        string EncryptAES(string text, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = new byte[16]; // AES için genellikle 16 byte'lık IV kullanılır.
            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes(text);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        var cipherTextBytes = ms.ToArray();
                        var cipherText = Convert.ToBase64String(cipherTextBytes);
                        return cipherText;
                    }
                }
            }
        }

        // Encrypt data using SHA512 algorithm.
        string EncryptSha512(string text)
        {
            using (var sha512 = SHA512.Create())
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                byte[] hashBytes = sha512.ComputeHash(textBytes);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashString;
            }
        }

        // Encrypt data using SHA256 algorithm.
        string EncryptSha256(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                byte[] hashBytes = sha256.ComputeHash(textBytes);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashString;
            }
        }

        // Create random AES key. 16 byte 128 bit key.
        string RandomCreateAesKey()
        {
            byte[] key = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return Convert.ToBase64String(key);
        }
        #endregion

        // Reset all security buttons.
        void ResetSecurityButtons()
        {
            // clear radio button checks

            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            radioButton5.Checked = false;
            radioButton6.Checked = false;
            radioButton7.Checked = false;
            radioButton8.Checked = false;
            radioButton9.Checked = false;
            radioButton10.Checked = false;
            radioButton11.Checked = false;
            radioButton12.Checked = false;
            ClearEncryptPath();
        }

        // Write result data view.
        void WriteResultDataView(string userText)
        {
            string encryptedText = "";
            WriteLenghtCount();

            if (EncryptPath.Count > 0)
            {
                // Encryption type list for encryption layer.
                List<int> closeToZero = new List<int>();

                foreach (var item in EncryptPath)
                {
                    closeToZero.Add(item.Key);
                }

                // Sort list for encryption layer. (1-2-3)
                closeToZero.Sort();

                // Select encryption type for each key.
                foreach (int item in closeToZero)
                {
                    encryptType eType = EncryptPath[item];

                    // If key is not valid, generate new key.
                    try
                    {
                        byte[] checkKey = Convert.FromBase64String(textBox1.Text);
                        if (checkKey.Length != 16) GenerateRandomAesKey();
                    }
                    catch
                    {
                        GenerateRandomAesKey();
                    }

                    // Select encryption type.
                    switch (eType)
                    {
                        // Encrypt data using AES algorithm.
                        case encryptType.aes:
                            if (string.IsNullOrEmpty(encryptedText))
                            {
                                encryptedText = EncryptAES(userText, textBox1.Text);
                            }
                            else
                            {
                                encryptedText = EncryptAES(encryptedText, textBox1.Text);
                            }
                            break;

                        // Encrypt data using SHA512 algorithm.
                        case encryptType.sha512:
                            if (string.IsNullOrEmpty(encryptedText))
                            {
                                encryptedText = EncryptSha512(userText);
                            }
                            else
                            {
                                encryptedText = EncryptSha512(encryptedText);
                            }
                            break;

                        // Encrypt data using SHA256 algorithm.
                        case encryptType.sha256:
                            if (string.IsNullOrEmpty(encryptedText))
                            {
                                encryptedText = EncryptSha256(userText);
                            }
                            else
                            {
                                encryptedText = EncryptSha256(encryptedText);
                            }
                            break;

                        // Encrypt data using Base64 algorithm.
                        case encryptType.base64:
                            if (string.IsNullOrEmpty(encryptedText))
                            {
                                encryptedText = EncryptBase64(userText);
                            }
                            else
                            {
                                encryptedText = EncryptBase64(encryptedText);
                            }
                            break;
                    }
                }

                richTextBox3.Text = encryptedText;
            }
            else
            {
                richTextBox3.Text = userText;
            }
        }

        // Generate random AES key.
        void GenerateRandomAesKey() => textBox1.Text = RandomCreateAesKey();

        // Add encrypt layer to list for encryption.
        void AddEncryptPath(int index, encryptType type)
        {
            // If index is already in list, return.
            if (EncryptPath.Any(i => i.Key == index && i.Value == type)) return;

            // If index key same but value is not same , change the value.
            if (EncryptPath.Any(i => i.Key == index && i.Value != type))
            {
                EncryptPath[index] = type;
            }
            else
            {
                // If index is not in list, add it.
                EncryptPath.Add(index, type);
            }

            EncryptPathChanged?.Invoke(richTextBox1.Text);
        }

        // Clear encrypt path.
        void ClearEncryptPath()
        {
            WriteLenghtCount();
            EncryptPath.Clear();
            WriteResultDataView(richTextBox1.Text);

        }

        // Encrypt type enum.
        enum encryptType : byte
        {
            aes,
            sha512,
            sha256,
            base64
        }

        // Send type enum.
        enum sendType : byte
        {
            post,
            put,
            get,
            delete
        }

        // Send data to web.
        private async void SendData(sendType type, string data, string url)
        {
            richTextBox2.Text = "";
            // Show the sent info to user.
            MessageBox.Show("Request sent.", "[Hyper] HttpClientTool say:", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Send data to web.
            switch (type)
            {
                // use Post method
                case sendType.post:
                    try
                    {
                        // Proccess in Garbage method for prevent memory leak.
                        using (HttpClient client = new HttpClient())
                        {
                            // Create content.
                            HttpContent content = new StringContent(data, Encoding.UTF8);

                            // Send Post request.
                            HttpResponseMessage response = await client.PostAsync(url, content);

                            // Check the result.
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                richTextBox2.Text = responseContent;
                            }
                            else
                            {
                                richTextBox2.Text = "ERROR";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        richTextBox2.Text = "URL ERROR" + e;
                    }

                    break;

                // use Put method
                case sendType.put:
                    try
                    {
                        // Proccess in Garbage method for prevent memory leak.
                        using (HttpClient client = new HttpClient())
                        {

                            // Create content.
                            HttpContent content = new StringContent(data, Encoding.UTF8);

                            // Send Put request.
                            HttpResponseMessage response = await client.PutAsync(url, content);

                            // Check the result.
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                richTextBox2.Text = responseContent;
                            }
                            else
                            {
                                richTextBox2.Text = "ERROR";
                            }
                        }
                    }
                    catch
                    {
                        richTextBox2.Text = "URL ERROR";
                    }
                    break;

                // use Get method
                case sendType.get:
                    try
                    {
                        // Proccess in Garbage method for prevent memory leak.
                        using (HttpClient client = new HttpClient())
                        {
                            // Create custom URL with data.
                            string _url = url + Uri.EscapeDataString(data);

                            // Send Get request.
                            HttpResponseMessage response = await client.GetAsync(_url);

                            // Check the result.
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                richTextBox2.Text = responseContent;
                            }
                            else
                            {
                                richTextBox2.Text = "ERROR";
                            }
                        }
                    }
                    catch
                    {
                        richTextBox2.Text = "URL ERROR";
                    }

                    break;

                // use Delete method
                case sendType.delete:
                    try
                    {
                        // Proccess in Garbage method for prevent memory leak.
                        using (HttpClient client = new HttpClient())
                        {

                            // Create custom URL with data.
                            string _url = url + Uri.EscapeDataString(data);

                            // Send Delete request.
                            HttpResponseMessage response = await client.DeleteAsync(_url);

                            // Check the result.
                            if (response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                richTextBox2.Text = responseContent;
                            }
                            else
                            {
                                richTextBox2.Text = "ERROR";
                            }
                        }
                    }
                    catch
                    {
                        richTextBox2.Text = "URL ERROR";
                    }

                    break;
            }


        }

        // Write length count.
        private void WriteLenghtCount()
        {
            // If lenght count is more than 1000, write K.
            if (richTextBox1.Text.Length / 1000 > 1)
            {
                label10.Text = (richTextBox1.Text.Length / 1000).ToString() + "K";
            }
            else
            {
                label10.Text = richTextBox1.Text.Length.ToString();
            }

            // If lenght count is more than 1000, write K.
            if (richTextBox3.Text.Length / 1000 > 1)
            {
                label11.Text = (richTextBox3.Text.Length / 1000).ToString() + "K";
            }
            else
            {
                label11.Text = richTextBox3.Text.Length.ToString();
            }
        }
    }
}
