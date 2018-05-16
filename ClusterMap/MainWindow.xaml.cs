﻿using HolyParser;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WebSupergoo.ABCpdf11;

namespace ClusterMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const bool IS_DEBUG = true;
        public const double PIXLES_PER_DISTANCE = 0.027;// RADIUS / 15000f;
        public double lat { get; set; }
        public double lng { get; set; }
        private static readonly HttpClient client = new HttpClient();

        string FILE_PATH_PDF = Path.GetTempPath() + "map.pdf";
        string FILE_PATH_TIFF = Path.GetTempPath() + "map.tif";
        string FILE_PATH_PNG = Path.GetTempPath() + "temp_map.png";
        string FILE_PATH_PNG_FINAL = Path.GetTempPath() + "map.png";
        string MAP_URL;

        public MainWindow()
        {
            InitializeComponent();
            HamQTH country = Services.getHamQth("PP5DZ");
            //lat = 32.270068;
            //lng = 35.080606;
            lat = 0;
            lng = 0;

            LoadDefaultMap();
        }

        private async void LoadDefaultMap()
        {
            if (IS_DEBUG || !File.Exists(FILE_PATH_PNG_FINAL))
            {
                string response = await PostMapRequest();
                SaveMap(response);
            }

            MapPanel.Source = new BitmapImage(new System.Uri(FILE_PATH_PNG_FINAL));

            double Lat1 = 8.229767;
            double Lng1 = 77.292732;

            ////South Argentina
            double SA_Lat = -54.601227;
            double SA_Lng = -71.524232;

            ////BearingSea
            double BS_Lat = 65.891002;
            double BS_Lng = -168.940600;

            ////BearingSea
            double India_Lat = 8.300320;
            double India_Lng = 77.472064;

            ////BearingSea
            double NP_Lat = 90;
            double NP_Lng = 0;

            ////BearingSea
            double SP_Lat = -90;
            double SP_Lng = 0;

            ////BearingSea
            double Liberia_Lat = 4.451074;
            double Liberia_Lng = -7.600905;



            PaintRelativePoint(lat,lng,Tanzania_Lat,Tanzania_Lng);
            PaintRelativePoint(lat, lng, SA_Lat, SA_Lng);
            PaintRelativePoint(lat, lng, BS_Lat, BS_Lng);
            PaintRelativePoint(lat, lng, India_Lat, India_Lng);
            PaintRelativePoint(lat, lng, NP_Lat, NP_Lng);
            PaintRelativePoint(lat, lng, SP_Lat, SP_Lng);
            PaintRelativePoint(lat, lng, Liberia_Lat, Liberia_Lng);
        }

        private void PaintRelativePoint(double slat, double slng, double dlat, double dlng)
        {
            double distance = Utils.HaversineDistanceBetweenPlaces(slat, slng, dlat, dlng);
            double factor = GetDistanceFactor(distance);
            Tuple<double, double> bearing = Utils.BearingVectors(slat, slng, dlat, dlng);
            double lat_vect = distance * bearing.Item1;
            double lng_vect = distance * bearing.Item2;

            System.Windows.Shapes.Ellipse point = new System.Windows.Shapes.Ellipse();
            point.Stroke = System.Windows.Media.Brushes.Red;
            point.Fill = System.Windows.Media.Brushes.Red;
            point.StrokeThickness = 1;
            point.Width = 4;
            point.Height = 4;
            System.Windows.Controls.Canvas.SetLeft(point, (MapPanel.Source.Width / 2) + lng_vect * factor - point.Width / 2);
            System.Windows.Controls.Canvas.SetTop(point, (MapPanel.Source.Height / 2) - lat_vect * factor - point.Height / 2);

            MapContainer.Children.Add(point);
        }

        private double GetDistanceFactor(double value)
        {
            double c = 0.000000000000000000000000740998 * Math.Pow(value, 6) - 0.000000000000000000042557060903 * Math.Pow(value, 5) + 0.000000000000000972213437376804 * Math.Pow(value, 4) - 0.0000000000112389058679665 * Math.Pow(value, 3) + 0.0000000694162742743402 * Math.Pow(value, 2) - (0.000221461581596741 * value) + 0.329424217146764;
            return c;
        }

        private async Task<string> PostMapRequest()
        {
            const string DOWNLOADER_URI = "http://ok2pbq.atesystem.cz/prog/qso2.php";
            var values = new Dictionary<string, string>
            {
               { "MapSize", "900" },
               { "range", "17000" },
               { "QRA", "4Z1KD" },
               { "QTH", "JJ00AA" },
               { "tool", "E" }
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(DOWNLOADER_URI, content);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        private void SaveMap(string response)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//html/body/img");
            var value = node.Attributes["src"].Value;
            string fileName = value.Split(new char[] { '/' }).Last();
            string url = "http://ok2pbq.atesystem.cz/image/" + fileName;
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), FILE_PATH_PNG_FINAL);
            }

            //Rectangle cropRect = new Rectangle(200, 200, 900, 900);
            //Bitmap src = Image.FromFile(FILE_PATH_PNG) as Bitmap;
            //Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            //using (Graphics g = Graphics.FromImage(target))
            //{
            //    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
            //                     cropRect,
            //                     GraphicsUnit.Pixel);
            //    target.Save(FILE_PATH_PNG_FINAL, ImageFormat.Png);
            //}
        }

    }
   
}