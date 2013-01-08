using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;

namespace StockQuote
{
    class BSECompanyCode
    {


        public static void scrapBSECompaniesCode()
        {
            try
            {
                DataTable bsedata=new DataTable();
                DataColumn companyName = new DataColumn("CompanyName");
                bsedata.Columns.Add(companyName);
                DataColumn bsecode = new DataColumn("BSE Code");
                bsedata.Columns.Add(bsecode);
                DataRow row;
                string url = ConfigurationManager.AppSettings["BSECompanyCodeURL"].ToString();
                HtmlWeb webpage = new HtmlWeb();
                
                HtmlAgilityPack.HtmlDocument document = webpage.Load(url);
                HtmlNode node = document.DocumentNode.SelectSingleNode("//div[@id='content']");
                
                string[] temp;
                foreach (HtmlNode link in node.SelectNodes("//a[@href]"))
                {
                    if (Regex.IsMatch(link.InnerText, "BSE code:"))
                    {
                        temp = Regex.Split(link.InnerText, "BSE code:");
                        row=bsedata.NewRow();
                        row[companyName] = temp[0].Remove(temp[0].Length-1).Trim();
                        row[bsecode] = temp[1].Replace(')',' ').Trim();
                        bsedata.Rows.Add(row);

                        

                    }
                }
                CommonFunctions.Write(bsedata, "bsecompanies.txt");
                companyName.Dispose();
                bsecode.Dispose();
                bsedata.Dispose();
                
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
   
    }
}
