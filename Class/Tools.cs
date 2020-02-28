using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Web;

namespace WSUnionbank
{
    public class Tools
    {
        public static void LogData(string mainFolderName, string subFolderName, string userId, string message, bool insertNewLine)
        {

            int counter = 1;
            string fileName = DateTime.Now.ToString("MM-dd-yy") + "File no " + counter.ToString();
            while (counter < 11)
            {
                try
                {
                    string dirName = (System.Configuration.ConfigurationManager.AppSettings["Logs"].ToString()
                                   + mainFolderName + "\\"
                                   + subFolderName + "\\"
                                   + (DateTime.Now.Year.ToString()) + "\\"
                                   + (DateTime.Now.ToString("MMMM")) + "\\"
                                   + (DateTime.Now.ToString("dddd, dd")) + "\\"
                                   + userId + "\\");

                    if (Directory.Exists(dirName))
                    {

                        using (FileStream fs = new FileStream(dirName + fileName + ".txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                message = (insertNewLine) ? Environment.NewLine + DateTime.Now.ToString() + ": " + message
                                                : DateTime.Now.ToString() + ": " + message;
                                sw.WriteLine(message);

                            }
                        }

                    }
                    else
                    {
                        Directory.CreateDirectory(dirName);
                        using (FileStream fs = new FileStream(dirName + fileName + ".txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                message = (insertNewLine) ? Environment.NewLine + DateTime.Now.ToString() + ": " + message
                                                : DateTime.Now.ToString() + ": " + message;
                                sw.WriteLine(message);
                            }
                        }

                    }
                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(250);
                    counter++;
                    if (counter == 11)
                        return;
                }
            }
        }

        public static void TLS()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => { return true; };
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            const SslProtocols _Tls11 = (SslProtocols)0x00000300;
            const SecurityProtocolType Tls11 = (SecurityProtocolType)_Tls11;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | Tls11
                | Tls12
                | SecurityProtocolType.Ssl3;

        }
        public sealed class SmartReader1
        {
            private DateTime defaultDate;
            public SmartReader1(SqlDataReader reader)
            {
                this.defaultDate = DateTime.MinValue;
                this.reader = reader;
            }

            public Int64 GetInt64(String column)
            {
                Int64 data = (reader.IsDBNull(reader.GetOrdinal(column))) ? (Int64)0 : (Int64)reader[column];
                return data;
            }

            public int GetInt32(String column)
            {

                int data = (reader.IsDBNull(reader.GetOrdinal(column))) ? (int)0 : (int)reader[column];
                return data;
            }

            public short GetInt16(String column)
            {
                short data = (reader.IsDBNull(reader.GetOrdinal(column))) ? (short)0 : (short)reader[column];
                return data;
            }

            public float GetFloat(String column)
            {
                float data = (reader.IsDBNull(reader.GetOrdinal(column))) ? 0 : float.Parse(reader[column].ToString());
                return data;
            }

            public bool GetBoolean(String column)
            {
                bool data = (reader.IsDBNull(reader.GetOrdinal(column))) ? false : (bool)reader[column];
                return data;
            }

            public String GetString(String column)
            {
                String data = (reader.IsDBNull(reader.GetOrdinal(column))) ? null : reader[column].ToString();
                return data;
            }

            public DateTime GetDateTime(String column)
            {
                DateTime data = (reader.IsDBNull(reader.GetOrdinal(column))) ? defaultDate : (DateTime)reader[column];
                return data;
            }
            public Decimal GetDecimal(String column)
            {
                Decimal data = (reader.IsDBNull(reader.GetOrdinal(column))) ? (decimal)0.00 : (Decimal)reader[column];
                return data;
            }

            public bool Read()
            {
                return this.reader.Read();
            }
            private SqlDataReader reader;
        }

    }
}