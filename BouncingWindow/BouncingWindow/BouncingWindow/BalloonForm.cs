using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using BouncingWindow;

namespace BouncingWindow
{
    public partial class BalloonForm : Form
    {

        private PictureBox balloon;
        private System.Windows.Forms.Timer balloonTimer;
        private float driftAngle = 0f;
        private Random random = new Random();
        private Button menuButton;
        private MenuForm menuForm;

        public BalloonForm()//Constructor
        {
            InitializeComponent();

            

            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0); // Top of screen
            this.Size = new Size(Screen.PrimaryScreen.Bounds.Width, 150);

            //this.BackColor = Color.LightBlue; //testing remove in future

            InitBalloon();
            InitMenuButton();


            this.Resize += (s, e) => PositionMenuButton();
        }

        private async void OnBalloonPopped()
        {
            var drops = await DropFetcher.GetDropsAsync(); // Get cached or fetched drop list
            var selectedDrop = DropFetcher.GetRandomDrop(drops); // Pick a drop based on rarity

            if (selectedDrop != null)
            {
                int qty = selectedDrop.GetRandomQuantity(); // Get realistic quantity from range or single value

                // ADD TO INVENTORY
                Inventory.AddItem(selectedDrop.Item, qty);

                MessageBox.Show($" You got: {selectedDrop.Item} x{qty}", "Balloon Drop!");
            }
            else
            {
                MessageBox.Show(" The balloon was empty ", "Whoops");
            }
        }

        private void InitMenuButton()//Initializes menu button
        {
            menuButton = new Button
            {
                Size = new Size(40, 40),
                BackColor = Color.SlateGray,
                FlatStyle = FlatStyle.Flat,
                Text = "≡",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            menuButton.FlatAppearance.BorderSize = 0;

            // Add it first, then position it based on the form size
            this.Controls.Add(menuButton);
            PositionMenuButton();

            menuButton.Click += MenuButton_Click;
        }

        private void PositionMenuButton()//Sets position of menu
        {
            int margin = 10;

            // Use this.ClientSize, NOT this.Size
            int x = this.ClientSize.Width - menuButton.Width - margin;
            int y = this.ClientSize.Height - menuButton.Height - margin;

            menuButton.Location = new Point(10, 10);
        }


        private void InitBalloon()//initialize balloon
        {
            try
            {
                balloon = new PictureBox
                {
                    Image = Image.FromFile("Assets/Balloon.png"),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(100, 150),
                    BackColor = Color.Transparent,
                    Location = new Point(-100, 20)
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Balloon image failed to load: " + ex.Message);
                return;
            }

            this.Controls.Add(balloon);
            balloon.BringToFront();

            balloonTimer = new System.Windows.Forms.Timer();
            balloonTimer.Interval = 30;
            balloonTimer.Tick += BalloonTimer_Tick;
            balloonTimer.Start();


            balloon.Click += Balloon_Click; 
        }

        private void BalloonTimer_Tick(object sender, EventArgs e)//makes balloon drift across screen
        {
            balloon.Left += 2;//moves balloon to the the right each tick

            //these 3 lines use a sine wave to gently  move the balloon up and down
            driftAngle += 0.1f;
            int yDrift = (int)(10 * Math.Sin(driftAngle));
            balloon.Top = 20 + yDrift;


            //when the balloon goes off-screen to the right, it resets to the left side and starts again with a new wave pattern.
            if (balloon.Left > this.Width)
            {
                balloon.Left = -balloon.Width;
                driftAngle = (float)(random.NextDouble() * Math.PI * 2);
            }
        }

        private void Balloon_Click(object sender, EventArgs e)//Series of events for when the balloon is clicked
        {
            // Stop movement
            balloonTimer.Stop();

            //play a pop sound
            System.Media.SoundPlayer player = new System.Media.SoundPlayer("Assets/pop.wav"); //needs change
            player.Play();

            // Hide the balloon (or you could remove it)
            balloon.Visible = false;

            //respawn after delay
            System.Windows.Forms.Timer respawnTimer = new System.Windows.Forms.Timer();
            respawnTimer.Interval = 2000; // 2 seconds
            respawnTimer.Tick += (s, args) =>
            {
                balloon.Left = -balloon.Width;
                balloon.Visible = true;
                balloonTimer.Start();
                respawnTimer.Stop();
            };
            respawnTimer.Start();

            OnBalloonPopped();//calls the function to get a random item from the drop table
        }

        

        private void MenuButton_Click(object sender, EventArgs e)//handles what happens when you click the menu button
        {
            if (menuForm == null || menuForm.IsDisposed)
            {
                menuForm = new MenuForm();
                menuForm.StartPosition = FormStartPosition.Manual;
                menuForm.Location = new Point(
                    Screen.PrimaryScreen.WorkingArea.Left,
                    Screen.PrimaryScreen.WorkingArea.Top
                );
                menuForm.Show();
            }
            else
            {
                menuForm.Visible = !menuForm.Visible;
            }
        }
    }
}