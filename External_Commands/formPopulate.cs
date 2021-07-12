using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace External_Buttons
{
    public partial class formPopulate : Form
    {
        public List<Populate_Coordinates_Cat.SetOutElement> ElementData { get; set; }
        public int DecimalPlaces { get; set; }
        public string ParaNS { get; set; }
        public string ParaEW { get; set; }
        public string ParaEL { get; set; }
        public formPopulate()
        {
            InitializeComponent();
            Shown += formCat_Shown;
        }
        private void formCat_Shown(object sender, EventArgs e)
        {
            DecimalPlaces = 2;
            UpdateDataGrid();
        }
        private void UpdateDataGrid()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.AllowDrop = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = false;
            for (int i = 0; i < ElementData.Count; i++)
            {
                dataGridView1.Rows.Add(new string[5]
                {
                    ElementData[i].element.Name,
                    ElementData[i].id.ToString(),
                    Math.Round(ElementData[i].NS, DecimalPlaces).ToString(),
                    Math.Round(ElementData[i].EW, DecimalPlaces).ToString(),
                    Math.Round(ElementData[i].EL, DecimalPlaces).ToString()
                });
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParaNS = textBox1.Text;
            ParaEW = textBox2.Text;
            ParaEL = textBox3.Text;
            DecimalPlaces = Convert.ToInt32(numericUpDown1.Value);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            DecimalPlaces = Convert.ToInt32(numericUpDown1.Value);
            UpdateDataGrid();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:zach.zheng@jacobs.com");
        }
    }
}
