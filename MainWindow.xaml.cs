using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;

namespace MacroscopPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Properties

        string projectPath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\"; 
        string configURL = "http://demo.macroscop.com:8080/configex?login=root";

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            ChannelsToComboBox(configURL); // Now this text only is displayed

            if (!Directory.Exists(projectPath + @"frames"))
            {
                DirectoryInfo di = Directory.CreateDirectory(projectPath + @"frames");
            }
            
        }
        
        private void DrawBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in GetFilesInDirectory(projectPath + @"frames\"))
            {
                string framePath = i.FullName; // How to setup a delay between frames (40 milliseconds)? Thread.Sleep(40) just freezes application :(
                ShowImageAsync(framePath);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e) // To divide part of stream into sequence of frames. Files are saved in the "frames" folder
        {
            Frames.SaveFrames(projectPath);
        }

        private async void ShowImageAsync(string source)
        {
            VideoPlayer.Source = await LoadImageSourceAsync(source);
        }

        private async Task<ImageSource> LoadImageSourceAsync(string address)
        {
            ImageSource imgSource = null;

            try
            {
                byte[] result;

                using (FileStream sourceStream = File.Open(address, FileMode.Open))
                {
                    result = new byte[sourceStream.Length];
                    await sourceStream.ReadAsync(result, 0, (int)sourceStream.Length);

                    ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                    imgSource = (ImageSource)imageSourceConverter.ConvertFrom(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return imgSource;
        }

        private void ChannelsToComboBox(string xmlAddress) 
        {
            XmlDocument doc = MakeConfigRequest(configURL);
            XmlElement root = doc.DocumentElement;
            XmlNodeList childnodes = root.SelectNodes("//Channels/ChannelInfo");

            foreach (XmlNode node in childnodes)
            {
                XmlNode id = node.Attributes.GetNamedItem("Id");
                XmlNode name = node.Attributes.GetNamedItem("Name");
                if (id != null)
                {
                    camList.Items.Add(name.Value + " — " + id.Value);
                }
            }

            camList.SelectedIndex = 0;
        }

        private XmlDocument MakeConfigRequest(string query)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(query);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(new StreamReader(response.GetResponseStream(), Encoding.UTF8));
                return doc;
            }
        }

        private FileInfo[] GetFilesInDirectory(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            DirectoryInfo[] dirs = info.GetDirectories();
            return info.GetFiles();
        }
    }


}
