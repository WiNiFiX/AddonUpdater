using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddonUpdater
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Shown += FrmMain_Shown;
            dgExisting.CellValueChanged += DgExisting_CellValueChanged;
            dgExisting.CurrentCellDirtyStateChanged += DgExisting_CurrentCellDirtyStateChanged;
        }

        private void DgExisting_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgExisting.IsCurrentCellDirty)
            {
                dgExisting.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgExisting_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dgExisting.Rows[e.RowIndex].Cells[4];
                var addonName = dgExisting.Rows[e.RowIndex].Cells[0].Value.ToString();
                Addons.MarkForUpdate(addonName, (bool)checkCell.Value);
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            Addons.PopulateMyAddons(dgExisting);            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cmdUpdate_Click(object sender, EventArgs e)
        {
            Addons.UpdateChecked();
        }
    }
}
