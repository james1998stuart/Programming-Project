using System.IO;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Media;
using System.Data;

namespace BouncingWindow
{
    public partial class ImpForm : Form
    {
        private System.Windows.Forms.Timer moveTimer;
        private Bouncer bouncer;

        private Image originalImage;
        private float currentSpinAngle = 0f;
        private bool isSpinning = false;
        private Stopwatch globalStopwatch = new Stopwatch();
        private double spinStartTime = -9999; // negative = never clicked yet
        private double spinDurationMs = 1000;
        private bool showReaction = false;

        private int rapidClickCount = 0;
        private Stopwatch rapidClickTimer = new Stopwatch();
        private const int maxClickDelay = 2000; // 2 seconds window
        private bool isInPhaseTwo = false;

        private AngryImp angryImp;
        private System.Windows.Forms.Timer chaosTimer; //for how long chaos phase lasts


        //private string imagePath = @"C:\Users\partl\OneDrive\Desktop\Dragon_impling.png"; 

        public ImpForm()//initializer
        {
            InitializeComponent();

            BalloonForm balloonForm = new BalloonForm();
            balloonForm.Show();

            
            globalStopwatch.Start();

            // Window setup
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(150, 150);
            this.TopMost = true;
            this.DoubleBuffered = true;

            //Relative pathing for image
            string baseDir1 = AppDomain.CurrentDomain.BaseDirectory;
            string imagePath = Path.Combine(baseDir1, "Assets", "Dragon_impling.png");

            //loading soundstream
            

            // Load image into memory (cloned, so no file locks)
            if (System.IO.File.Exists(imagePath))
            {
                using (var temp = Image.FromFile(imagePath))
                {
                    originalImage = new Bitmap(temp);
                }
            }
            else
            {
                originalImage = SystemIcons.Warning.ToBitmap();
            }

            // Paint hook
            this.Paint += Form1_Paint;
            this.Click += Form_Click;

            // Bounce logic
            bouncer = new Bouncer(this);
            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 20;
            moveTimer.Tick += (s, e) =>
            {
                if (!isInPhaseTwo)
                {
                    bouncer.Update(); // Only bounce if NOT in rage mode
                }
            };
            moveTimer.Start();

            // Spin loop
            Application.Idle += OnAppIdle;
        }//end of form1 initializer


        private void Form_Click(object sender, EventArgs e)//determines what happens when imp is clicked
        {
            // Count rapid clicks
            if (!rapidClickTimer.IsRunning || rapidClickTimer.ElapsedMilliseconds > maxClickDelay)
            {
                rapidClickCount = 1;
                rapidClickTimer.Restart();
            }
            else
            {
                rapidClickCount++;
            }

            // If 5 clicks within 2 seconds, activate new class
            if (rapidClickCount >= 5 && !isInPhaseTwo)
            {
                ActivatePhaseTwo();
                PlayAngerSound();
                return;
            }

            // Spin and sound as usual
            spinStartTime = globalStopwatch.Elapsed.TotalMilliseconds;
            PlayClickSound();
        }


        //plays a bite sound
        private void PlayBiteSound()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "imp_bite.wav");
            if (File.Exists(path))
            {
                var audio = new System.Media.SoundPlayer(path);
                audio.Play();
            }
        }

        
        private void PlayClickSound()//Plays the "I just got clicked" sound 
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "Assets", "imp_sound.wav");
            SoundPlayer player = new SoundPlayer(path);
            player.Play();
        }

        private void PlayAngerSound()//Plays anger sound
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "Assets", "anger.wav");
            SoundPlayer player = new SoundPlayer(path);
            player.Play();
        }


        //Phase that gets activated when clicked 5 times
        private void ActivatePhaseTwo()
        {
            isInPhaseTwo = true; // SET THIS FIRST

            angryImp = new AngryImp(this);

            this.BackColor = Color.DarkRed;

            //  This runs every 200ms while enraged
            chaosTimer = new System.Windows.Forms.Timer();
            chaosTimer.Interval = 20; // smoother than 200ms
            chaosTimer.Tick += (s, e) =>
            {
                angryImp.DoChaos(); // Should run continuously
            };
            chaosTimer.Start();

            // End rage after 8 seconds
            System.Windows.Forms.Timer phaseTimer = new System.Windows.Forms.Timer();
            phaseTimer.Interval = 8000;
            phaseTimer.Tick += (s, e) =>
            {
                phaseTimer.Stop();
                phaseTimer.Dispose();

                EndPhaseTwo();
            };
            phaseTimer.Start();
        }

        private void EndPhaseTwo()//This ends the enrage phase
        {
            isInPhaseTwo = false;

            if (chaosTimer != null)
            {
                chaosTimer.Stop();
                chaosTimer.Dispose();
                chaosTimer = null;
            }

            this.BackColor = Color.Black; // Reset color or image if needed
        }

        private void OnAppIdle(object sender, EventArgs e)//allows you to reset the imps spin when clicked
        {
            while (AppStillIdle)
            {
                double timeSinceSpin = globalStopwatch.Elapsed.TotalMilliseconds - spinStartTime;

                if (timeSinceSpin <= spinDurationMs)
                {
                    currentSpinAngle = (float)((timeSinceSpin / spinDurationMs) * 360.0);
                    this.Invalidate(); // redraw for the current frame
                }
                else
                {
                    currentSpinAngle = 0;
                    showReaction = false;
                }

                Application.DoEvents();
            }
        }

        private bool AppStillIdle//returns true if the app is idle
        {
            get
            {
                return !PeekMessage(out _, IntPtr.Zero, 0, 0, 0);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out Message lpMsg, IntPtr hWnd,
            uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        private void Form1_Paint(object sender, PaintEventArgs e)//draws the imp image, applies spin when clicked, smoothes the image
        {
            if (originalImage == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float formW = this.ClientSize.Width;
            float formH = this.ClientSize.Height;
            float imgW = originalImage.Width;
            float imgH = originalImage.Height;

            //  Calculate scale to fit (zoom-style)
            float scale = Math.Min(formW / imgW, formH / imgH);

            //  Move origin to center
            g.TranslateTransform(formW / 2f, formH / 2f);
            g.RotateTransform(currentSpinAngle);
            g.ScaleTransform(scale, scale); // 👈 scale after rotate
            g.TranslateTransform(-imgW / 2f, -imgH / 2f);

            g.DrawImage(originalImage, new PointF(0, 0));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // You can leave this empty or use it for setup stuff
        }

    }//end of form1 class

    public class AngryImp//the enrage phase after the imp is clicked 5 times
    {
        private Form form;
        private float speed = 20f;

        public AngryImp(Form target)//takes a reference of the window
        {
            form = target;
        }

        public void DoChaos()//chases the mouse
        {
            Point mousePos = Cursor.Position;

            // Get center of the imp
            PointF impCenter = new PointF(
                form.Left + form.Width / 2f,
                form.Top + form.Height / 2f
            );

            float dx = mousePos.X - impCenter.X;
            float dy = mousePos.Y - impCenter.Y;

            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1f) return; // Already close

            // Normalize direction
            dx /= dist;
            dy /= dist;

            // Move toward mouse
            form.Left += (int)(dx * speed);
            form.Top += (int)(dy * speed);
        }
    }
}

