using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BouncingWindow
{
    public partial class InventoryForm : Form
    {
        public InventoryForm()
        {
            InitializeComponent();
            this.Load += InventoryForm_Load;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Leave empty for now
        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            foreach (var item in InventoryManager.GetItems())
            {
                listBox1.Items.Add($"{item.Key} x{item.Value}");
            }
        }
    }
}