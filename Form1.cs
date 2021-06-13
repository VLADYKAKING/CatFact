using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace CatFact
{
    public partial class Form1 : Form
    {
        DateTime today = DateTime.Today;
        string[] facts = new string[5];
        int current = 0;
        private SqlConnection connection = null;
        private SqlCommand command = null;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\CatFact-main\Database1.mdf;Integrated Security=True");
            connection.Open();
            if (connection.State != ConnectionState.Open)
                MessageBox.Show("БД не подключена!");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://catfact.ninja/facts?limit=5&max_length=255");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream stream = resp.GetResponseStream();
            StreamReader strRdr = new StreamReader(stream);

            string result = strRdr.ReadToEnd();
            dynamic json = JsonConvert.DeserializeObject(result);
            for (int i = 0; i < 5; i++)
                facts[i] = $"{i+1}. " + json.data[i].fact;

            command = new SqlCommand("SELECT * FROM CatFact where Date=@Date", connection);
            command.Parameters.AddWithValue("Date", today.Date);
            if (Convert.ToString(command.ExecuteScalar()) != Convert.ToString(today.Date)) 
            {
                for (int i = 0; i < 5; i++)
                {
                    command = new SqlCommand($"INSERT INTO CatFact (Date, Fact) VALUES (@Date, @Fact)", connection);
                    command.Parameters.AddWithValue("Date", today.Date);
                    command.Parameters.AddWithValue("Fact", facts[i]);
                    command.ExecuteNonQuery();
                }
            }
            richTextBox1.Text = facts[0];
        }
        private void back_Click(object sender, EventArgs e)
        {
            current--;
            if (current < 0)
                current = 4;
            richTextBox1.Text = facts[current];
        }
        private void forward_Click(object sender, EventArgs e)
        {
            current++;
            if (current > 4)
                current = 0;
            richTextBox1.Text =  facts[current];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetFactsByDate(-1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GetFactsByDate(-2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GetFactsByDate(-3);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            GetFactsByDate(0);
        }

        void GetFactsByDate(int n)
        {
            DateTime date = DateTime.Today.AddDays(n);
            SqlDataAdapter adapter = new SqlDataAdapter($"SELECT Date, Fact FROM CatFact where Date='{date.Month}/{date.Day}/{date.Year}'", connection);
            DataSet set = new DataSet();
            adapter.Fill(set);
            dataGridView1.DataSource = set.Tables[0];
        }
    }
}
