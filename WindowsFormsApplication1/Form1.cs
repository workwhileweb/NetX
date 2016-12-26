using Extreme.Net;
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

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var socksProxy = new Socks5ProxyClient("77.109.184.55", 62810);

            var handler = new ProxyHandler(socksProxy);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage();
            //request.RequestUri = new Uri("http://httpbin.org/get");
            request.Method = HttpMethod.Post;

            var parameters = new Dictionary<string, string> { { "param1", "1" }, { "param2", "2" } };
            var encodedContent = new System.Net.Http.FormUrlEncodedContent(parameters);

            var response = await client.PostAsync("http://httpbin.org/post", encodedContent);
            var content  = await response.Content.ReadAsStringAsync();
        }
    }
}
