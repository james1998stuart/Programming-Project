using System;
using System.Drawing;
using System.Windows.Forms;

namespace BouncingWindow
{
    public partial class MenuForm : Form
    {
        private RichTextBox inventoryBox;
        private Button inventoryButton;
        private bool isInventoryVisible = false;

        public MenuForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(200, 300);
            this.BackColor = Color.White;
            this.TopMost = true;

            InitMenuButtons();

            // When form is shown, refresh the inventory view
            this.Shown += (s, e) =>
            {
                inventoryBox.Text = Inventory.GetInventoryText();
            };
        }

        private void InitMenuButtons()
        {
            // Inventory display box
            inventoryBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true
            };

            // Inventory toggle button
            Button inventoryButton = new Button
            {
                Text = "Open Inventory",
                Dock = DockStyle.Top,
                Height = 40
            };

            inventoryButton.Click += (s, e) =>
            {
                inventoryBox.Text = Inventory.GetInventoryText();
            };

            // Sort button
            Button sortButton = new Button
            {
                Text = "Sort Inventory",
                Dock = DockStyle.Top,
                Height = 40
            };

            sortButton.Click += (s, e) =>
            {
                inventoryBox.Text = Inventory.GetSortedInventoryText();
            };

            // Add controls to the menu form in order
            this.Controls.Add(inventoryBox);     // bottom
            this.Controls.Add(sortButton);       // middle
            this.Controls.Add(inventoryButton);  // top
        }
    }
}