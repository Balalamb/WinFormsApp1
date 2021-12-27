using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;




namespace WinFormsApp1
{
    delegate void Reno(object sender, StoppedEventArgs args);
    
    public partial class Form1 : Form
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        string filePath = string.Empty;
        List<string> lstAuduoFiles= new List<string>();
        int indexSound=-1;
        PlayOntime playOntime = new PlayOntime();
        private SoundPlayer player;
        Thread thread;
        int songRandomNum = 0;
        bool randomSelect = true;
        int currIndexOfSong = 0;

        List<TimesCall> lstTimes = new List<TimesCall>();
        List<int> Hours = new List<int>();
        List<int> Minutes = new List<int>();

        TimeSpan TotalLenghtSong = new TimeSpan();
        TimeSpan TenMin = new TimeSpan(0, 0, 22);
        DateTime currTime = new DateTime();
        DateTime stopTime = new DateTime();
       
        
        
        
        // public  delegate void OnPlaybackStopped (object sender, StoppedEventArgs e) ; //StoppedEventArgs
        public Form1()
        {
            
            InitializeComponent();
            button1.Click += OpenFile;
            EnablePlaybackControls(true);
            InitialListSounds();
            lb.SelectedIndexChanged += changeFile;
            btnChangeTime.Click += ChangeTimeCall;
            btnPlay.Click += play;
            btnStop.Click += stop;
            btnPlus.Click += AddZvonok;
            btnMinus.Click += DelZvonok;
            btnPlus.Click += NotifyAboutChange;
            btnMinus.Click += NotifyAboutChange;
            btnMinSong.Click += DelSongFromPlayList;
            //btnRandom.Click += RandomSwitch;
            AddFirstZvon();
            lb.DoubleClick += play;
            playOntime.TimeOn += PlayOnTimer;
            playOntime.Check += CheckTime;
            playOntime.CheckForStop += CheckTimeForStop;
            playOntime.StopMusic += stop;


            btnChangeTime.Visible = false;
            startTimer();
            
        }

        public void startTimer()
        {

            foreach (Control ctrl in tlpZvonki.Controls)
            {

                if (ctrl is NumericUpDown & ctrl.Name.IndexOf("ZvonNumH") != -1)
                {
                    Hours.Add(Decimal.ToInt32((ctrl as NumericUpDown).Value));
                }
                if (ctrl is NumericUpDown & ctrl.Name.IndexOf("ZvonNumM") != -1)
                {
                    Minutes.Add(Decimal.ToInt32((ctrl as NumericUpDown).Value));
                }
            }

            for (int i = 0; i <= Hours.Count - 1; i++)
            {
                TimesCall tc = new TimesCall()
                {
                    Hour = Hours[i],
                    Minutes = Minutes[i]
                };
                lstTimes.Add(tc);
            }
            thread = new Thread(() => playOntime.Clock()); //lstTimes
            thread.Start();
            thread.IsBackground = true;
            

        }

        private void PlayOnTimer()
        {
            if (randomSelect)
            {
                Random random = new Random();
                int countSongs = lstAuduoFiles.Count - 1;
                int songNum = random.Next(0, countSongs);
                if (songRandomNum == songNum)
                {
                    while (songNum == songRandomNum)
                    {
                        songNum = random.Next(0, countSongs);
                    }
                }
                string strSong = lstAuduoFiles[songNum].ToString();
                if (outputDevice == null)
                {
                    outputDevice = new WaveOutEvent();

                }
                if (audioFile == null)
                {
                    audioFile = new AudioFileReader(strSong);
                    outputDevice.Init(audioFile);
                }
                outputDevice.PlaybackStopped += NextSong;
                outputDevice.Play();
                currTime = DateTime.Now;
                stopTime = currTime.AddMinutes(10);
                //stopTime = currTime.AddSeconds(25);
                thread = new Thread(() => playOntime.CheckTimes()); //lstTimes
                thread.Name = "StopThread";
                thread.Start();
                thread.IsBackground = true;
                //TimerCallback tc = new System.Threading.TimerCallback(stopPlayer);
                // System.Threading.Timer timer = new System.Threading.Timer(tc, player, 600000, 0);

            }
            else
            {
                string strSong = lstAuduoFiles[currIndexOfSong].ToString();
                player.SoundLocation = strSong;
                player.Play();
                currIndexOfSong = (currIndexOfSong==lstAuduoFiles.Count-1)?0: currIndexOfSong++;
            }

        }

        private void play(Object sender, EventArgs e)
        {

            foreach (string str in lstAuduoFiles)
            {
                //индекс последнего вхождения "\"
                int indexOfChar = str.LastIndexOf('\\');

                if (str.Remove(0, indexOfChar + 1) == (lb.SelectedItem = lb.SelectedItem == null ? "1" : lb.SelectedItem).ToString())
                {
                    if (outputDevice == null)
                    {
                        outputDevice = new WaveOutEvent();

                    }
                    if (audioFile == null)
                    {
                        audioFile = new AudioFileReader(str);
                        outputDevice.Init(audioFile);
                    }
                    
                    outputDevice.Play();
                    btnPlay.Visible = false;
                    
                }
            }
        }
        public void NextSong(object sender, StoppedEventArgs args)
        {
            Random random = new Random();
            int countSongs = lstAuduoFiles.Count - 1;
            int songNum = random.Next(0, countSongs);
            if (songRandomNum == songNum)
            {
                while (songNum == songRandomNum)
                {
                    songNum = random.Next(0, countSongs);
                }
            }
            string strSong = lstAuduoFiles[songNum].ToString();
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
            }
            audioFile = new AudioFileReader(strSong);
            outputDevice.Init(audioFile);
            outputDevice.Play();

        }
        void RandomSwitch(object sender, EventArgs e)
        {
            randomSelect = randomSelect == false ? true : false;
            //btnRandom.Text= randomSelect == false ? "По порядку" : "Случайно";
        }
        
        void DelSongFromPlayList (object sender, EventArgs e)
        {
            if (lb.SelectedIndex != -1)
            {
                int ind = lb.SelectedIndex;
                lb.Items.RemoveAt(ind);
                lstAuduoFiles.RemoveAt(ind);
                try
                {
                    lb.SelectedIndex = ind;
                }
                catch (Exception w)
                {
                    lb.SelectedIndex = -1;
                }
                
            }
        }

       

        private void changeFile(Object sender, EventArgs e)
        {
            currIndexOfSong = lb.SelectedIndex;
            
        }

       
        public void stop (Object sender, EventArgs e)
        {
            //outputDevice.PlaybackStopped -= NextSong;
            outputDevice?.Stop();
            OnPlaybackStopped();
            btnPlay.Visible = true;
            
        }
        //остановка по таймеру после запуска музыки
        public void stop()
        {
            outputDevice.PlaybackStopped -= NextSong;
            outputDevice?.Stop();
            OnPlaybackStopped();
            //btnStop.Enabled = false;
        }

        public void OnPlaybackStopped()
        {
            if (outputDevice != null)
            {
                outputDevice.Dispose();
                outputDevice = null;
                audioFile.Dispose();
                audioFile = null;
            }
        }
        void AddZvonok (object sender, EventArgs e)
        {
            if (tlpZvonki.RowCount <=9)
            {
                int RowCount = tlpZvonki.RowCount;
                tlpZvonki.RowCount = tlpZvonki.RowCount + 1;
                //tlpZvonki.Size;
                tlpZvonki.Controls.Add(new Label() { Text = RowCount + 1 + " звонок", Width = 60, Name = "Zvonlbl" + tlpZvonki.RowCount }, 0, RowCount);
                tlpZvonki.Controls.Add(new NumericUpDown() { Value = 0, Width = 45, Name = "ZvonNumH" + tlpZvonki.RowCount, Maximum = 23 }, 1, RowCount);
                tlpZvonki.Controls.Add(new NumericUpDown() { Value = 0, Width = 45, Name = "ZvonNumM" + tlpZvonki.RowCount, Maximum = 59 }, 2, RowCount);
                //подписка на изменение
                Control[] contrls = tlpZvonki.Controls.Find("ZvonNumM" + tlpZvonki.RowCount, false);
                (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
                contrls = tlpZvonki.Controls.Find("ZvonNumH" + tlpZvonki.RowCount, false);
                (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            }

            
        }

        void DelZvonok (object sender, EventArgs e)
        {
            if (tlpZvonki.RowCount > 1)
            {
                Control[] contrls = tlpZvonki.Controls.Find("ZvonNumM" + tlpZvonki.RowCount, false);
                tlpZvonki.Controls.Remove(contrls[0]);
                contrls = tlpZvonki.Controls.Find("ZvonNumH" + tlpZvonki.RowCount, false);
                (contrls[0] as NumericUpDown).ValueChanged -= NotifyAboutChange;
                tlpZvonki.Controls.Remove(contrls[0]);
                contrls = tlpZvonki.Controls.Find("Zvonlbl" + tlpZvonki.RowCount, false);
                tlpZvonki.Controls.Remove(contrls[0]);
                tlpZvonki.RowCount = tlpZvonki.RowCount - 1;
            }
        }

        void ChangeTimeCall(object sender, EventArgs e)
        {
            lstTimes.Clear();
            Hours.Clear();
            Minutes.Clear();
            foreach (Control ctrl in tlpZvonki.Controls)
            {

                
                if (ctrl is NumericUpDown & ctrl.Name.IndexOf("ZvonNumH") != -1)
                {
                    Hours.Add(Decimal.ToInt32((ctrl as NumericUpDown).Value));
                }
                if (ctrl is NumericUpDown & ctrl.Name.IndexOf("ZvonNumM") != -1)
                {
                    Minutes.Add(Decimal.ToInt32((ctrl as NumericUpDown).Value));
                }
            }

            for (int i = 0; i <= Hours.Count - 1; i++)
            {
                TimesCall tc = new TimesCall()
                {
                    Hour = Hours[i],
                    Minutes = Minutes[i]
                };
                lstTimes.Add(tc);
            }
            btnChangeTime.Visible = false;
        }
        
        void OpenFile(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) 
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Audio files(*.mp3;)|*.mp3|All files(*.*)|*.*";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true;
                
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    lstAuduoFiles.Clear();
                    lb.Items.Clear();
                    foreach (string str in openFileDialog.FileNames)
                    {
                        
                        lstAuduoFiles.Add(str);
                        int indexOfChar = str.LastIndexOf('\\');
                        lb.Items.Add(str.Remove(0, indexOfChar+1));
                    }
                }

            }
                
        }

        void InitialListSounds()
        {
            
            string path = Directory.GetCurrentDirectory() + "/Music";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string[] files = Directory.GetFiles(path);
            List<string> lstSongs = new List<string>();
            foreach(string file in files)
            {
                int indexOfChar = file.LastIndexOf('\\');
                string newStr = file.Substring(file.Length - 3);
                if (newStr == "mp3")
                {
                    lstSongs.Add(file);
                    lstAuduoFiles.Add(file);
                }
            }
            lstAuduoFiles.Clear();
            foreach (string str in lstSongs)
            {
                lstAuduoFiles.Add(str);
                int indexOfChar = str.LastIndexOf('\\');
                lb.Items.Add(str.Remove(0, indexOfChar + 1));
            }
        }

         void EnablePlaybackControls(bool enable)
        {
            btnPlay.Enabled = enable;
            btnStop.Enabled = enable;
        }

        void AddFirstZvon()
        {
            Control[] contrls = new Control[2];
            tlpZvonki.Controls.Add(new Label() { Text = "1 звонок", Width=60, Name = "Zvonlbl1" }, 0, 0);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value=8, Width = 45, Name = "ZvonNumH1", Maximum=23 }, 1, 0);;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 30, Width = 45, Name = "ZvonNumM1",Maximum=59 }, 2, 0);
            contrls = tlpZvonki.Controls.Find("ZvonNumM1", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH1", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //2
            tlpZvonki.Controls.Add(new Label() { Text = "2 звонок", Width = 60, Name = "Zvonlbl2" }, 0, 1);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 9, Width = 45, Name = "ZvonNumH2", Maximum = 23 }, 1, 1); ;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 10, Width = 45, Name = "ZvonNumM2", Maximum = 59 }, 2, 1);
            contrls = tlpZvonki.Controls.Find("ZvonNumM2", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH2", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //3
            tlpZvonki.Controls.Add(new Label() { Text = "3 звонок", Width = 60, Name = "Zvonlbl3" }, 0, 2);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 10, Width = 45, Name = "ZvonNumH3", Maximum = 23 }, 1, 2); ;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 0, Width = 45, Name = "ZvonNumM3", Maximum = 59 }, 2, 2);
            contrls = tlpZvonki.Controls.Find("ZvonNumM3", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH3", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //4
            tlpZvonki.Controls.Add(new Label() { Text = "4 звонок", Width = 60, Name = "Zvonlbl4" }, 0, 3);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 10, Width = 45, Name = "ZvonNumH4", Maximum = 23 }, 1, 3); ;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 50, Width = 45, Name = "ZvonNumM4", Maximum = 59 }, 2, 3);
            contrls = tlpZvonki.Controls.Find("ZvonNumM4", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH4", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //5
            tlpZvonki.Controls.Add(new Label() { Text = "5 звонок", Width = 60, Name = "Zvonlbl5" }, 0, 4);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 11, Width = 45, Name = "ZvonNumH5", Maximum = 23 }, 1, 4); ;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 40, Width = 45, Name = "ZvonNumM5", Maximum = 59 }, 2, 4);
            contrls = tlpZvonki.Controls.Find("ZvonNumM5", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH5", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //6
            tlpZvonki.Controls.Add(new Label() { Text = "6 звонок", Width = 60, Name = "Zvonlbl6" }, 0, 5);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 12, Width = 45, Name = "ZvonNumH6", Maximum = 23 }, 1, 5); ;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 30, Width = 45, Name = "ZvonNumM6", Maximum = 59 }, 2, 5);
            contrls = tlpZvonki.Controls.Find("ZvonNumM6", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH6", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //7
            tlpZvonki.Controls.Add(new Label() { Text = "7 звонок", Width = 60, Name = "Zvonlbl7" }, 0, 6);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 13, Width = 45, Name = "ZvonNumH7", Maximum = 23 }, 1, 6); ;
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 20, Width = 45, Name = "ZvonNumM7", Maximum = 59 }, 2, 6);
            contrls = tlpZvonki.Controls.Find("ZvonNumM7", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH7", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            //8
            /*tlpZvonki.Controls.Add(new Label() { Text = "8 звонок", Width = 60, Name = "Zvonlbl8" }, 0, 7);
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 14, Width = 45, Name = "ZvonNumH8", Maximum = 23 }, 1, 7); //14
            tlpZvonki.Controls.Add(new NumericUpDown() { Value = 15, Width = 45, Name = "ZvonNumM8", Maximum = 59 }, 2, 7); //15
            contrls = tlpZvonki.Controls.Find("ZvonNumM8", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;
            contrls = tlpZvonki.Controls.Find("ZvonNumH8", false);
            (contrls[0] as NumericUpDown).ValueChanged += NotifyAboutChange;*/
            tlpZvonki.RowCount = 8;
        }

        bool CheckTime()
        {
            foreach (TimesCall tc in lstTimes)
            {
                int sec = 0;
                //если не воскресенье проигрываем
                if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                {
                    
                    if (!SaturdayNow())
                    {
                        if (tc.Hour != 0)
                        {
                            if (tc.Hour == DateTime.Now.Hour & tc.Minutes == DateTime.Now.Minute & sec == DateTime.Now.Second)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        //отсчет 10 минут
        bool CheckTimeForStop()
        {
            if (DateTime.Now >= stopTime)
            {
                return true;
            }
            return false;
        }
        //Проверка на работу в субботу
        bool SaturdayNow()
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday & cbSaturday.Checked)
            {
                return true;
            }
            else if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday & !cbSaturday.Checked)
            {
                return false;
            }
            else return false;
        }
        void NotifyAboutChange (object sender, EventArgs e)
        {
            btnChangeTime.Visible = true;
        }

        class PlayOntime
        {
            public delegate void Bam();
            public delegate bool Bum();
            

            public event Bam TimeOn;
            public event Bum Check;
            public event Bam StopMusic;

            public event Bum CheckForStop;


            public void Clock()//List<TimesCall> lstTc
            {
               //метод рабоает все время и проверяет не пора ли влючить музыку
                while (true)
                {
                    if (Check())
                    {
                        TimeOn();
                        Thread.Sleep(1500);
                    }
                    
                    Thread.Sleep(50);
                }
            }
            //метод отсчтиывает 10 минут с начала проигрывания и по истечении завершает его
            public void CheckTimes()
            {
                bool stop=true;
                while (stop)
                {
                    if (CheckForStop())
                    {
                        stop = false;
                        StopMusic();
                        Thread.Sleep(1500);
                    }

                    Thread.Sleep(50);
                }
            }
        }
        
        class TimesCall
        {
            public int Hour { get; set; }
            public int Minutes { get; set; }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {

        }
    }
 
}
