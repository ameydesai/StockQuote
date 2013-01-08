using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace StockQuote
{
    class NSECompanyCode
    {

        public static void nseCompanyCodes()
        {
            StreamReader sr = new StreamReader(@"C:\\Documents and Settings\\Amey\\Desktop\\EQUITY_L.csv");
            DataTable nsedata = new DataTable();
            DataColumn companyName = new DataColumn("CompanyName");
            nsedata.Columns.Add(companyName);
            DataColumn nsecode = new DataColumn("NSE Code");
            nsedata.Columns.Add(nsecode);
            DataRow row;
            string[] values = null;
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                values = sr.ReadLine().Split(',');
                row = nsedata.NewRow();
                row[companyName] = values[1];
                row[nsecode] = values[0];
                nsedata.Rows.Add(row);

            }
            CommonFunctions.Write(nsedata, "nsecompanies.txt");
            sr.Close();
            nsedata.Dispose();
            companyName.Dispose();
            nsecode.Dispose();
            

        }
    }
}
