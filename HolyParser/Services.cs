﻿using DXCCManager;
using HolyParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HolyParser
{
    public static class Services
    {        
        public static string getHamQth(string callsign)
        {
            if (!string.IsNullOrWhiteSpace(callsign))
            {
                try
                {
                    string baseRequest = "http://www.hamqth.com/dxcc.php?callsign=";
                    WebRequest request = WebRequest.Create(baseRequest + callsign);
                    WebResponse response = request.GetResponse();
                    string status = ((HttpWebResponse)response).StatusDescription;
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    XDocument xDoc = XDocument.Parse(responseFromServer);
                    IEnumerable<XElement> country = xDoc.Root.Descendants(xDoc.Root.GetDefaultNamespace‌​() + "name");
                    if (country.Count() > 0)
                        return country.FirstOrDefault().Value;
                    else
                        return "";

                }
                catch (Exception)
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public static string GenerateAdif(IEnumerable<QSO> qso_list)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            StringBuilder adif = new StringBuilder(200);
            adif.AppendLine("<ADIF_VERS:3>2.2 ");
            adif.AppendLine("<PROGRAMID:10>HolyLogger ");
            //adif.AppendLine("<PROGRAMVERSION:15>Version 1.0.0.0 ");
            adif.AppendFormat("<PROGRAMVERSION:{0}>{1} ", version.Length, version);
            adif.AppendLine();
            adif.AppendLine("<EOH>");
            adif.AppendLine();

            foreach (QSO qso in qso_list)
            {
                if (qso.DXCall != null) adif.AppendFormat("<call:{0}>{1} ", qso.DXCall.Length, qso.DXCall);
                if (qso.SRX != null) adif.AppendFormat("<srx_string:{0}>{1} ", qso.SRX.Length, qso.SRX);
                if (qso.Freq != null) adif.AppendFormat("<freq:{0}>{1} ", qso.Freq.Length, qso.Freq);
                if (qso.Mode != null) adif.AppendFormat("<mode:{0}>{1} ", qso.Mode.Length, qso.Mode);
                if (qso.Band != null) adif.AppendFormat("<band:{0}>{1} ", qso.Band.Length, qso.Band);
                if (qso.MyCall != null) adif.AppendFormat("<station_callsign:{0}>{1} ", qso.MyCall.Length, qso.MyCall);
                if (qso.MyCall != null) adif.AppendFormat("<operator:{0}>{1} ", qso.MyCall.Length, qso.MyCall);
                if (qso.Name != null) adif.AppendFormat("<name:{0}>{1} ", qso.Name.Length, qso.Name);
                if (qso.Country != null) adif.AppendFormat("<country:{0}>{1} ", qso.Country.Length, qso.Country);
                if (qso.STX != null) adif.AppendFormat("<stx_string:{0}>{1} ", qso.STX.Length, qso.STX);
                if (qso.RST_RCVD != null) adif.AppendFormat("<rst_rcvd:{0}>{1} ", qso.RST_RCVD.Length, qso.RST_RCVD);
                if (qso.RST_SENT != null) adif.AppendFormat("<rst_sent:{0}>{1} ", qso.RST_SENT.Length, qso.RST_SENT);
                if (qso.Date != null) adif.AppendFormat("<qso_date:{0}>{1} ", qso.Date.Length, qso.Date);
                if (qso.Time != null) adif.AppendFormat("<time_on:{0}>{1} ", qso.Time.Length, qso.Time);
                if (qso.Time != null) adif.AppendFormat("<time_off:{0}>{1} ", qso.Time.Length, qso.Time);
                if (qso.Comment != null) adif.AppendFormat("<comment:{0}>{1} ", qso.Comment.Length, qso.Comment);
                adif.AppendLine("<EOR>");
            }

            return adif.ToString();
        }

        public static string GenerateCSV(IEnumerable<QSO> qso_list)
        {
            StringBuilder csv = new StringBuilder(200);
            RadioEntityResolver rem = new RadioEntityResolver();

            int index = 1;

            csv.AppendFormat("{0},", "No");
            csv.AppendFormat("{0},", "Date");
            csv.AppendFormat("{0},", "UTC Start");
            csv.AppendFormat("{0},", "Callsign");
            csv.AppendFormat("{0},", "Country");
            csv.AppendFormat("{0},", "Name");
            csv.AppendFormat("{0},", "QTH");
            csv.AppendFormat("{0},", "Band");
            csv.AppendFormat("{0},", "Mode");
            csv.AppendFormat("{0},", "Rcvd");
            csv.AppendFormat("{0},", "Sent");
            csv.AppendFormat("{0},", "UTC End");
            csv.AppendFormat("{0}\r\n", "Exchange");

            foreach (QSO qso in qso_list)
            {
                string date = qso.Date;
                string time = qso.Time;

                csv.AppendFormat("{0},", index++);
                csv.AppendFormat("{0},", date);
                csv.AppendFormat("{0},", time);
                csv.AppendFormat("{0},", qso.DXCall);
                csv.AppendFormat("{0},", rem.GetEntity(qso.DXCall));
                csv.AppendFormat("{0},", qso.Name);
                csv.AppendFormat("{0},", "");
                csv.AppendFormat("{0},", qso.Band);
                csv.AppendFormat("{0},", qso.Mode);
                csv.AppendFormat("{0},", qso.RST_RCVD);
                csv.AppendFormat("{0},", qso.RST_SENT);
                csv.AppendFormat("{0},", time);
                csv.AppendFormat("{0}\r\n", qso.SRX);
            }
            return csv.ToString();
        }

        public static string getBareCallsign(string callsign)
        {
            return callsign;
            //string[] callParts = callsign.Split('/');
            //if (callParts.Length == 1) return callsign;
            //if (callParts.Length > 2) return callParts[1];
            //if (callParts.Length == 2)
            //{
            //    if (callParts[0].Length > callParts[1].Length) return callParts[0];
            //    return callParts[1];
            //}
            //return callsign;
        }

        public static async Task<string> SendMail(string from, string to, string subject, string body)
        {
            MailMessage mail = new MailMessage(from, to);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "host406.hostmonster.com";
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("iarcorg", "Rw6Ach!@");

            mail.IsBodyHtml = true;
            mail.Subject = subject;
            mail.Body = body;

            try
            {
                await client.SendMailAsync(mail);
                return "email successfully sent";
            }
            catch (Exception)
            {
                return "Connection with server failed! Check your internet connection";
            }            
        }
    }
}
