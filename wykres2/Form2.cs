using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace wykres2
{
    public partial class Form2 : Form
    {
        public int trawersId = 0;
        private Form1 mainForm;
        public Form2(Form1 frm1)
        {
            InitializeComponent();
            mainForm = frm1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mainForm.trawersId = trawersId;
            mainForm.lb("Wybrano trawers: \n" + mainForm.trawersId);
            ArrayList vAirTrawers = (ArrayList)mainForm.vUasCollection[trawersId];
            int i = 0;
            foreach (object vAir in vAirTrawers){
                i++;
                mainForm.lb(String.Format("vAirTrawers[{0}]: {1:0.00} ",i,(float)vAir));
            }
            mainForm.redrawChart1();
            Hide();

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            trawersId = listBox1.SelectedIndex;
        }
    }
}
