using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace External_Buttons
{
    public partial class formCate : Form
    {
        public string[] CatNames { get; set; }
        public string Selected { get; set; }

        public formCate()
        {
            InitializeComponent();
            Shown += formCat_Shown;
        }
        private void formCat_Shown(object sender, EventArgs e)
        {
            listBox1.Items.AddRange(CatNames);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "Please select a cateogry...")
            {
                textBox1.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                Selected = textBox1.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.SelectedItem as string;
            textBox1.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
        }

        private void label6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:zach.zheng@jacobs.com");
        }
    }
}
