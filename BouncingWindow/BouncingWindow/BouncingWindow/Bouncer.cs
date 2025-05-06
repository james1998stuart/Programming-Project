using System.Drawing;
using System.Windows.Forms;

namespace BouncingWindow
{
    public class Bouncer//gives any form sent to it physics, making it move around the screen.
    {
        private float x, y;
        private float vx, vy;
        private float gravity = 0.25f;
        private float maxSpeed = 5f;
        private Form form;
        private Random rand = new Random();

        private int flapCooldown = 0;

        public Bouncer(Form targetForm)//constructor
        {
            form = targetForm;
            x = form.Left;
            y = form.Top;

            // Start with a small launch
            vx = rand.Next(-3, 4);
            vy = -5; // shoot upward a bit
        }

        public void Update()//core physics engine. runs every frame via a timer to animate the form like its a bird flapping around.
        {
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;

            // Apply gravity
            vy += gravity;

            //  Randomly flap to stay afloat
            flapCooldown--;
            if (flapCooldown <= 0 && rand.NextDouble() < 0.1)
            {
                vy -= (float)(rand.NextDouble() * 45 + 3); // strong flap up
                vx += (float)(rand.NextDouble() * 2 - 1); // wobble left/right
                flapCooldown = rand.Next(15, 40);
            }

            // Clamps the speed. If it goes too fast or too slow, it remains at the maxspeed.
            vx = Math.Clamp(vx, -maxSpeed, maxSpeed);
            vy = Math.Clamp(vy, -maxSpeed, maxSpeed);

            // Update position
            x += vx;
            y += vy;

            // Bounce off screen edges
            if (x < screen.Left)
            {
                x = screen.Left;
                vx = -vx * 0.8f; // bounce + lose energy
            }
            if (x + form.Width > screen.Right)
            {
                x = screen.Right - form.Width;
                vx = -vx * 0.8f;
            }

            if (y < screen.Top)
            {
                y = screen.Top;
                vy = -vy * 0.8f;
            }
            if (y + form.Height > screen.Bottom)
            {
                y = screen.Bottom - form.Height;
                vy = -vy * 0.8f;
            }

            // Apply to form
            form.Left = (int)x;
            form.Top = (int)y;
        }
    }

}