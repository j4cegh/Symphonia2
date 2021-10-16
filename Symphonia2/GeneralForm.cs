using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;
using DiscordRPC;
using System.Runtime.InteropServices;

namespace Symphonia2
{
    

    public partial class GeneralForm : Form
    {
        bool isPlayingSong;
        bool isPlaylist;
        Label songLab;
        ThreadingRPC threadingRPC = new ThreadingRPC();
        bool onLoop;
        int prevX = 0;
        int prevY = 100;
        int lastSongIndex = 0;
        bool playlistSelected;
        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(long uDeviceID, uint dwVolume);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        DiscordRPC drpc = new DiscordRPC();
        Constants constants = new Constants();
        List<Label> labels = new List<Label>();
        

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public GeneralForm()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

            flowLayoutPanel1.AutoScroll = true;
            //settingsBtn.Text = "\u2699";
            constants.initVersion();
            label2.Text += " (build " + constants.build + ")";
            drpc.Init();
        }



        public static bool IsOutofBounds(Form form, Control control)
        {
            int controlEnd_X = control.Location.X + control.ClientSize.Width;
            int controlEnd_Y = control.Location.Y + control.ClientSize.Height;
            if (form.ClientSize.Width < controlEnd_X || form.ClientSize.Height < controlEnd_Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        public event EventHandler MyEvent;
        public bool hasEnded = true;
        double cPos = 0;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            panel1.BackColor = ColorTranslator.FromHtml("#171717");
        }

        private void button1_Paint(object sender, PaintEventArgs e)
        {

        }
        public delegate void StopEventHandler(object sender, EventArgs args);
        public event StopEventHandler StopEvent;

        public delegate void LoopAudioHandler(object sender, EventArgs args);
        public event LoopAudioHandler LoopAudioEvent;

        public delegate void UnLoopAudioHandler(object sender, EventArgs args);
        public event UnLoopAudioHandler UnLoopAudioEvent;

        public delegate void PauseAudioHandler(object sender, EventArgs args);
        public event PauseAudioHandler PauseAudioEvent;

        public delegate void ResumeAudioHandler(object sender, EventArgs args);
        public event ResumeAudioHandler ResumeAudioEvent;

        public delegate void DelUselessAudioHandler(object sender, EventArgs args);
        public event DelUselessAudioHandler DelUselessAudioEvent;

        public event EventHandler<ChangeVolAddEventArgs> ChangeVolAddEvent;
        
        private void OpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                StopEvent?.Invoke(this, new EventArgs() { });
                foreach (var label in labels.ToList())
                {
                    prevX = 0;
                    prevY = 100;
                    flowLayoutPanel1.Controls.Remove(label);
                    labels.Remove(label);
                }



                string root = folderDlg.SelectedPath;


                List<string> files = new List<string>();

                List<string> filesinDir = Directory.GetFiles(root).ToList();

                foreach (string possSong in filesinDir.ToList())
                {
                    filesinDir.Remove(possSong);
                }
                foreach(string b in Directory.GetFiles(root).ToList())
                {
                    filesinDir.Add(b);
                }
                foreach (string i in filesinDir.ToList())
                {
                    if (!i.EndsWith(".mp3") && !i.EndsWith(".wav"))
                    {
                        filesinDir.Remove(i);
                    }
                }

                foreach (string possSong in filesinDir.ToList())
                {
                    var filename = Path.GetFileName(possSong);

                    if (filename.EndsWith(".mp3") || filename.EndsWith(".wav"))
                    {
                        files.Add(filename);
                    }
                    else
                    {

                    }
                }
               

                WindowsMediaPlayer Player = new WindowsMediaPlayer();
                WindowsMediaPlayer Playernew;
                
                button5.Enabled = true;
                button5.Click += (oe, eo) =>
                {
                    
                    IWMPPlaylist playlist = Player.playlistCollection.newPlaylist(new DirectoryInfo(folderDlg.SelectedPath).Name);
                    IWMPMedia media;
                    
                    foreach (var song in files.Select((value, index) => new { value, index }))
                    {
                        media = Player.newMedia(filesinDir[song.index]);
                        playlist.appendItem(media);
                    }
                    Player.currentPlaylist = playlist;
                    Player.controls.play();
                    
                    string songName = Path.GetFileNameWithoutExtension(Player.currentMedia.sourceURL);
                    playingSong.Text = songName;                
                    threadingRPC.SetRPC(drpc, "Listening to \"" + songName + "\"", "Listening" + " (playlist" + (onLoop ? ", on loop)" : ")") + "...", constants);
                    StopEvent += (o, e4) =>
                    {
                        Player.controls.stop();
                        threadingRPC.SetRPC(drpc, "Nothing is playing.", "About to play some more tunes?", constants);
                    };
                    
                    Player.CurrentItemChange += (NewItem) =>
                    {
                        songName = Path.GetFileNameWithoutExtension(Player.currentMedia.sourceURL);
                        threadingRPC.SetRPC(drpc, "Listening to \"" + songName + "\"", "Listening" + " (playlist" + (onLoop ? ", on loop)" : ")") + "...", constants);
                        playingSong.Text = songName;
                    };
                    LoopAudioEvent += (oeae, e4) =>
                    {
                        Player.settings.setMode("loop", true);
                        onLoop = true;
                        threadingRPC.SetRPC(drpc, "Listening to \"" + songName + "\"", "Listening (on loop)...", constants);
                    };

                };
                foreach (var song in files.Select((value, index) => new { value, index }))
                {
                    



                    int fileExtPos = song.value.LastIndexOf(".");
                    var songFixd = song.value.Substring(0, fileExtPos);
                    Label songLab = new Label();
                    songLab.Padding = new Padding(6);
                    songLab.AutoSize = true;
                    
                    StopEvent += (o, e4) =>
                    {
                        Player.controls.stop();
                        threadingRPC.SetRPC(drpc, "Nothing is playing.", "About to play some more tunes?", constants);
                    };
                    songLab.Location = new Point(prevX + 10, prevY + 20);
                    songLab.Click += (o, e2) =>
                    {
                        button2.BackColor = Color.White;
                        onLoop = false;


                        hasEnded = false;
                        playingSong.Text = songFixd;
                       
                        Player.PlayStateChange += (NewState) =>
                        {
                            
                            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
                            {
                                hasEnded = true;
                            }
                            else if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded && !onLoop && isPlaylist)
                            {
                                //threadingRPC.SetRPC(drpc, "Listening to \"" + songFixd + "\"", "Listening (on loop)...", constants);
                            }
                            else if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded && !onLoop && !isPlaylist)
                            {
                                playingSong.Text = "";
                                threadingRPC.SetRPC(drpc, "Nothing is playing.", "About to play some more tunes?", constants);
                                StopEvent?.Invoke(this, new EventArgs() { });

                            }
                            

                        };

                        Player.URL = filesinDir[song.index];
                        Player.controls.play();

                        threadingRPC.SetRPC(drpc, "Listening to \"" + songFixd + "\"", "Listening...", constants);
                        
                        LoopAudioEvent += (oe, e4) =>
                        {
                            Player.settings.setMode("loop", true);
                            onLoop = true;
                            threadingRPC.SetRPC(drpc, "Listening to \"" + songFixd + "\"", "Listening (on loop)...", constants);
                        };
                        UnLoopAudioEvent += (oe, e4) =>
                        {
                            onLoop = false;
                            Player.settings.setMode("loop", false);
                            threadingRPC.SetRPC(drpc, "Listening to \"" + songFixd + "\"", "Listening...", constants);
                        };




                    };
                    songLab.MouseEnter += (o, e3) =>
                    {
                        Cursor = Cursors.Hand;
                        songLab.ForeColor = Color.Red;
                    };

                    songLab.MouseLeave += (o, e4) =>
                    {
                        Cursor = Cursors.Default;
                        songLab.ForeColor = Color.Black;
                    };
                    
/*
                    ResumeAudioEvent += (oe, e4) =>
                    {
                        Player.controls.play();
                    };
                    PauseAudioEvent += (o, e69) =>
                    {
                        cPos = Player.controls.currentPosition;
                        Player.controls.pause();
                    };
                    DelUselessAudioEvent += (o, ed) =>
                    {

                    };
                    ChangeVolAddEvent += (oe, e69) =>
                    {
                        
                    };
*/

                    labels.Add(songLab);
                    flowLayoutPanel1.Controls.Add(songLab);




                    prevY += 20;
                    songLab.Text += songFixd + "\n";
                }

            }

        }

        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
            {
                hasEnded = true;
            }
            else if ((WMPPlayState)NewState == WMPPlayState.wmppsMediaEnded && !onLoop)
            {
                playingSong.Text = "";
                //threadingRPC.SetRPC(drpc, "Nothing is playing.", "About to play some more tunes?", constants);
                
            }

        }

        private void Form1_MyEvent(object sender, EventArgs e)
        {

        }

        

        

        private void loop_Click(object sender, EventArgs e)
        {
            var sndBt = (System.Windows.Forms.Button)sender;
            if (sndBt.BackColor != Color.Green)
            {
                sndBt.BackColor = Color.Green;

                LoopAudioEvent?.Invoke(this, new EventArgs() { });
            }
            else
            {
                sndBt.BackColor = Color.White;
                UnLoopAudioEvent?.Invoke(this, new EventArgs() { });
            }
        }



        

        
        private void settingsBtn_Click(object sender, EventArgs e)
        {
            //new Settings().Show();
        }

        private void playingSong_TextChanged(object sender, EventArgs e)
        {
            if (playingSong.Text == "")
            {
                isPlayingSong = false;
            }
            else
            {
                isPlayingSong = true;
            }
        }

        private void stopButt_Click(object sender, EventArgs e)
        {

        }

        private void panel1_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void flowLayoutPanel1_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ChangeVolAddEvent?.Invoke(this, new ChangeVolAddEventArgs(100) { });
            
        }

        private void GeneralForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox senderChk = (CheckBox)sender;
            if(senderChk.Checked)
            {
                isPlaylist = true;
            }
            else
            {
                isPlaylist = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void stopButt_Click_1(object sender, EventArgs e)
        {
            StopEvent?.Invoke(this, new EventArgs() { });
        }
    }
    public class ChangeVolAddEventArgs : EventArgs
    {
        public ChangeVolAddEventArgs(int plusVol)
        {
            this.PlusVolNew = plusVol;
        }
        public int PlusVolNew { get; private set; }
    }
}
