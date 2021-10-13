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

namespace Symphonia2
{
    

    public partial class GeneralForm : Form
    {
        bool isPlayingSong;
        Label songLab;
        
        bool onLoop;
        int prevX = 0;
        int prevY = 100;
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
                foreach (string i in filesinDir.ToList())
                {
                    if (!i.EndsWith(".mp3") && !i.EndsWith(".wav"))
                    {
                        filesinDir.Remove(i);
                    }
                }

                foreach (string possSong in filesinDir)
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


                foreach (var song in files.Select((value, index) => new { value, index }))
                {
                    WMPLib.WindowsMediaPlayer Player = new WMPLib.WindowsMediaPlayer();


                    int fileExtPos = song.value.LastIndexOf(".");
                    var songFixd = song.value.Substring(0, fileExtPos);
                    Label songLab = new Label();
                    songLab.Padding = new Padding(6);
                    songLab.AutoSize = true;


                    songLab.Location = new Point(prevX + 10, prevY + 20);
                    songLab.Click += (o, e2) =>
                    {
                        button2.BackColor = Color.White;
                        UnLoopAudioEvent?.Invoke(this, new EventArgs() { });
                        onLoop = false;
                        StopEvent?.Invoke(this, new EventArgs() { });
                        DelUselessAudioEvent?.Invoke(this, new EventArgs() { });

                        hasEnded = false;
                        playingSong.Text = songFixd;
                        Player.PlayStateChange += Player_PlayStateChange;

                        Player.URL = filesinDir[song.index];
                        Player.controls.play();

                        drpc.client.SetPresence(new RichPresence()
                        {
                            Details = "Listening to \"" + songFixd + "\"",
                            State = "Listening...",
                            Assets = new Assets()
                            {
                                LargeImageKey = "symphony",
                                LargeImageText = "Symphonia",
                                SmallImageKey = "symphony",
                                SmallImageText = "Build " + constants.build
                            }
                        });
                        stopButt.Click += (oe, ea) =>
                        {
                            Player.controls.stop();
                            playingSong.Text = "";
                            drpc.client.SetPresence(new RichPresence()
                            {
                                Details = "Nothing is playing.",
                                State = "About to play some more tunes?",
                                Assets = new Assets()
                                {
                                    LargeImageKey = "symphony",
                                    LargeImageText = "Symphonia",
                                    SmallImageKey = "symphony",
                                    SmallImageText = "Build " + constants.build
                                }
                            });
                        };
                        LoopAudioEvent += (oe, e4) =>
                        {
                            Player.settings.setMode("loop", true);
                            onLoop = true;
                            drpc.client.SetPresence(new RichPresence()
                            {
                                Details = "Listening to \"" + songFixd + "\"",
                                State = "Listening (on loop)...",
                                Assets = new Assets()
                                {
                                    LargeImageKey = "symphony",
                                    LargeImageText = "Symphonia",
                                    SmallImageKey = "symphony",
                                    SmallImageText = "Build " + constants.build
                                }
                            });

                        };
                        UnLoopAudioEvent += (oe, e4) =>
                        {
                            onLoop = false;
                            Player.settings.setMode("loop", false);
                            drpc.client.SetPresence(new RichPresence()
                            {
                                Details = "Listening to \"" + songFixd + "\"",
                                State = "Listening...",
                                Assets = new Assets()
                                {
                                    LargeImageKey = "symphony",
                                    LargeImageText = "Symphonia",
                                    SmallImageKey = "symphony",
                                    SmallImageText = "Build " + constants.build
                                }
                            });
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
                    StopEvent += (o, e4) =>
                    {
                        Player.controls.stop();
                        drpc.client.SetPresence(new RichPresence()
                        {
                            Details = "Nothing is playing.",
                            State = "About to play some tunes?",
                            Assets = new Assets()
                            {
                                LargeImageKey = "symphony",
                                LargeImageText = "Symphonia",
                                SmallImageKey = "symphony",
                                SmallImageText = "Build " + constants.build
                            }
                        });
                    };

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
                        Player.settings.volume += e69.PlusVolNew;
                    };

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
            ChangeVolAddEvent?.Invoke(this, new ChangeVolAddEventArgs(5) { });
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
