using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using ShockwaveFlashObjects;
using AxShockwaveFlashObjects;
using System.Reflection;


namespace StockQuote
{
    

    public partial class Form1 : Form
    {
        static string[] companyNamefiles={"bsecompanies.txt","nsecompanies.txt"};
        static string companyNotFound = "Not Found";
        string textForBSECurrentValue = null;
        string[] daysBSEHighandLows = null;
        string[] previousBSECloseAndOpen = null;
        string textForNSECurrentValue = null;
        string[] daysNSEHighAndLows = new string[2];
        string[] previousNSECloseAndOpen = new string[2];

        public Form1()
        {
            InitializeComponent();
            //BSECompanyCode.scrapBSECompaniesCode();
            //NSECompanyCode.nseCompanyCodes();
           

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var allCompanyNames = new HashSet<string>();
            foreach (string s in companyNamefiles)
            {
                StreamReader sr = new StreamReader(@s);
                sr.ReadLine();
                string[] temp;
                while (!sr.EndOfStream)
                {
                    temp = sr.ReadLine().Split('|');
                    allCompanyNames.Add(temp[0].Trim());
                }
                sr.Close();

            }
            AutoCompleteStringCollection companyNames = new AutoCompleteStringCollection();
            string[] tempValues=new string[allCompanyNames.Count];
            allCompanyNames.CopyTo(tempValues);
            companyNames.AddRange(tempValues);

            textBox1.AutoCompleteMode = AutoCompleteMode.None;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox1.AutoCompleteCustomSource = companyNames;
            panel2.Visible = true;

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (textBox1.Text.Length == 0)
            {
                hideResults();
                return;
            }
            

            foreach (String s in textBox1.AutoCompleteCustomSource)
            {
                if (s.ToLower().Contains(textBox1.Text.ToLower()))
                {                  
                    listBox1.Items.Add(s);
                    listBox1.Visible = true;
                }
            }
       
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
            Graphics g = Graphics.FromHwnd(textBox1.Handle);
            SizeF f = g.MeasureString(textBox1.Text, textBox1.Font);
            textBox1.Width = (int)(f.Width);
            hideResults();
        }
     

        void listBox1_LostFocus(object sender, System.EventArgs e)
        {
            hideResults();
        }

        void hideResults()
        {
            listBox1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {

          this.searchInBSEFile.RunWorkerAsync(textBox1.Text);
          this.searchInNSEFile.RunWorkerAsync(textBox1.Text);


        }
        //stockexchange:0-BSE and 1-NSE
        private string searchStockCode(int stockexchange,string companyName)
        {
            StreamReader sr; 
            if (stockexchange==0)
                 sr=new StreamReader(@companyNamefiles[0]);
            else
                 sr=new StreamReader(@companyNamefiles[1]);

            sr.ReadLine();
            bool status=false;
            string[] temp=new string [2];
            
            while (!sr.EndOfStream)
            {
                temp = sr.ReadLine().Split('|');
                if (companyName.Trim() == temp[0].Trim())
                {
                    status = true;
                    break;
                }
            }
            if (status)
                return temp[1];
            else
                return companyNotFound;


        }

        public Image DownloadImage(string _URL)
        {
            Image _tmpImage = null;

            try
            {
                
                System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(_URL);

                _HttpWebRequest.AllowWriteStreamBuffering = true;

                _HttpWebRequest.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5";
                _HttpWebRequest.Accept = "*/*";

                System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse();

           
                System.IO.Stream _WebStream = _WebResponse.GetResponseStream();

                
                _tmpImage = Image.FromStream(_WebStream);

                
                _WebResponse.Close();
                _WebResponse.Close();
            }
            catch (Exception _Exception)
            {
               
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
                return null;
            }

            return _tmpImage;
        }

        void ForceCanonicalPathAndQuery(Uri uri)
        {
            string paq = uri.PathAndQuery; // need to access PathAndQuery
            FieldInfo flagsFieldInfo = typeof(Uri).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
            ulong flags = (ulong)flagsFieldInfo.GetValue(uri);
            flags &= ~((ulong)0x30); // Flags.PathNotCanonical|Flags.QueryNotCanonical
            flagsFieldInfo.SetValue(uri, flags);
        }

        private void searchInBSEFile_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = searchStockCode(0, (string)e.Argument).Trim();
            if (e.Result != companyNotFound)
            {

                Random r = new Random();

                string bseURL = ConfigurationManager.AppSettings["BSEURL1"].ToString() + e.Result.ToString() + ConfigurationManager.AppSettings["BSEURL2"].ToString() + r.NextDouble().ToString();

                HtmlWeb webpage = new HtmlWeb();
                HtmlAgilityPack.HtmlDocument doc = webpage.Load(bseURL);
                string text = doc.DocumentNode.InnerHtml;
                string[] temp = Regex.Split(text, "#@#");
                string currentValue = temp[14].Trim();
                string diffWithPrevValue = temp[15].Trim();
                if (!(diffWithPrevValue.Contains('-')))
                    diffWithPrevValue = "+" + diffWithPrevValue;

                string percentageDiff = temp[16].Trim();
                textForBSECurrentValue = "    " + currentValue + Environment.NewLine + Environment.NewLine + diffWithPrevValue + "  (" + percentageDiff + "%)";


                string[] temp1 = temp[19].Trim().Split('#');

                daysBSEHighandLows = temp1[2].Trim().Split('/');
                previousBSECloseAndOpen = temp[20].Trim().Split('/');



                //string wtdAvgPrice = temp[21].Trim();
                //string totalTradedValue = temp[23].Trim();
                //string avgCumulativeTradingVolume = temp[25].Trim();
                //string circuitLimits = temp[26].Trim();
                //string[] temp2 = temp[27].Split('#');
                //temp[27]=temp2[2].Trim();
                //string marketCapitalisationFull = temp2[0].Trim();
                //string []buyValues = new string[10];
                //string[] sellValues = new string[10];
                //int i, j = 0;
                //for (i = 0,j=27; i < 10; i+=2,j+=4)
                //{
                //    buyValues[i] = temp[j].Trim();
                //    buyValues[i + 1] = temp[j + 1].Trim();
                //    sellValues[i] = temp[j + 2].Trim();
                //    sellValues[i+1] = temp[j + 3].Trim();

                //}
                //string buyTotal = temp[47].Trim();
                //string[] temp3 = temp[48].Split('#');
                //string sellTotal = temp3[0].Trim();
                //string weeklyHighLow = temp3[2].Trim() + "/" + temp[49].Trim();
                //string monthlyHighLow = temp[50].Trim() + "/" + temp[51].Trim();
                //string yearlyHighLow = temp[52].Trim() + "/" + temp[53].Trim();
                //string yearlyHighLowDates = temp[54] + "--" + temp[55];
                //string percentageofDeliveryComparedToVolumeTraded = temp[56] + "/" + temp[57];
                //string exDate = temp[58];

                //Image bsestockImage = null;
                //string imageURL=ConfigurationManager.AppSettings["BSEImageCaptureURL1"].ToString()+e.Result.ToString().Trim()+ConfigurationManager.AppSettings["BSEImageCaptureURL2"].ToString()+r.NextDouble().ToString();
                //bsestockImage = DownloadImage(imageURL);

                //if (bsestockImage != null)
                //    pictureBox1.Image = bsestockImage;




            }


        }



        private void searchInBSEFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void searchInBSEFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != companyNotFound)
            {
                richTextBox1.Text = textForBSECurrentValue;

                textBox2.Text = daysBSEHighandLows[0];
                textBox3.Text = daysBSEHighandLows[1];
                textBox4.Text = previousBSECloseAndOpen[0];
                textBox5.Text = previousBSECloseAndOpen[1];
            }
        }

        private void searchInNSEFile_DoWork(object sender, DoWorkEventArgs e)
        {
           e.Result= searchStockCode(1, (string)e.Argument).Trim();
           if (e.Result != companyNotFound)
           {
               string nseurl = ConfigurationManager.AppSettings["NSEURL"].ToString() + e.Result.ToString();
               HttpWebRequest request = (HttpWebRequest)WebRequest.Create(nseurl);
               request.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5";
               request.Accept = "*/*";
               HttpWebResponse response = (HttpWebResponse)request.GetResponse();
               Stream s = response.GetResponseStream();

               HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
               doc.Load(s);
               HtmlNode divContentNode = doc.DocumentNode.SelectSingleNode("//div[@id='responseDiv']");
               //HtmlNode intraDayChart = doc.DocumentNode.SelectSingleNode("//div[@id='tab10Content']//a[@href]");
               //string href ="http://www.nseindia.com/"+intraDayChart.Attributes["href"].Value.ToString().Trim();

               //HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(href);
               //request1.UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.1.5) Gecko/20091102 Firefox/3.5.5";
               //request1.Accept = "*/*";

               //HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse();
               //Stream s1 = response1.GetResponseStream();

               //HtmlAgilityPack.HtmlDocument doc1 = new HtmlAgilityPack.HtmlDocument();
               //doc1.Load(s1);



               string text = divContentNode.InnerText.Trim();
               string[] values = Regex.Split(text, "data");

               string[] values1 = values[1].Split(',');




               string lastkey = null;
               System.Collections.Hashtable ht = new System.Collections.Hashtable();
               foreach (string temp in values1)
               {
                   if (temp.Contains(':'))
                   {
                       string[] tempValues = temp.Split(':');
                       if (tempValues.Length == 3)
                       {
                           lastkey = tempValues[1];
                           ht.Add(tempValues[1], tempValues[2]);
                       }
                       else
                       {
                           lastkey = tempValues[0];
                           ht.Add(tempValues[0], tempValues[1]);

                       }
                   }
                   else
                   {

                       ht[lastkey] = ht[lastkey] + "," + temp;
                   }


               }
               string temp1 = ht["\"change\""].ToString();
               if (!(temp1.Contains('-')))
                   temp1 = "+" + temp1;

               textForNSECurrentValue = "    " + ht["\"lastPrice\""].ToString() + Environment.NewLine + Environment.NewLine + temp1 + "  (" + ht["\"pChange\""].ToString() + "%)";

               daysNSEHighAndLows[0] = ht["\"dayHigh\""].ToString();
               daysNSEHighAndLows[1] = ht["\"dayLow\""].ToString();
               previousNSECloseAndOpen[0] = ht["\"previousClose\""].ToString();
               previousNSECloseAndOpen[1] = ht["\"open\""].ToString();

           }

 

                      
        }

        private void searchInNSEFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void searchInNSEFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != companyNotFound)
            {
                richTextBox2.Text = textForNSECurrentValue.Replace("\"", "");
                textBox6.Text = daysNSEHighAndLows[0].Replace("\"", "");
                textBox7.Text = daysNSEHighAndLows[1].Replace("\"", "");
                textBox8.Text = previousNSECloseAndOpen[0].Replace("\"", "");
                textBox9.Text = previousNSECloseAndOpen[1].Replace("\"", "");
            }
        }

     
 






        
    }
}
