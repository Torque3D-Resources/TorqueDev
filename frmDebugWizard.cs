/*

This file is part of TorqueDev.

TorqueDev is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by the 
Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

TorqueDev is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with TorqueDev.  If not, see <http://www.gnu.org/licenses>

EXCEPTIONS TO THE GPL: TorqueDev links in a number of third party libraries,
which are exempt from the license.  If you want to write closed-source
third party modules that you are going to link into TorqueDev, you may do so
without restriction.  I acknowledge that this technically allows for
one to bypass the open source provisions of the GPL, but the real goal
is to keep the core of TorqueDev free and open.  The rest is up to you.

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TSDev {
	internal partial class frmDebugWizard : Form {

		private int step = 1;

		public frmDebugWizard() {
			InitializeComponent();
		}

		private void cmdNext_Click(object sender, EventArgs e) {
			if (step == 1) {
				// Going to step 2
				if (optAutoDbg.Checked) {
					// Check to see if the file specified is filled in
					// and exists
					if (txtMainCS.Text.Trim() == "") {
						MessageBox.Show(this, "Please select a valid main.cs file to continue.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					} else if (!File.Exists(this.txtMainCS.Text)) {
						MessageBox.Show(this, "The main.cs file you specified does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
						return;
					}
				}
				pnlStep1.Visible = false;
				pnlStep2.Visible = true;

				// Set the next text and enable the previous button
				cmdNext.Text = "&Finish";
				cmdPrevious.Enabled = true;
				step = 2;
			} else if (step == 2) {
				// Finishing up; Check if the debugger they specified
				// exists
				if (txtDebugExe.Text.Trim() == "" || File.Exists(txtDebugExe.Text) == false) {
					MessageBox.Show(this, "The debugger executable you specified does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				} else if (txtDebugPasswd.Text.Trim() == "") {
					MessageBox.Show(this, "Please fill in a debugger password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				} else if (txtDebugPort.Text.Trim() == "") {
					MessageBox.Show(this, "Please fill in a debugger port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				// Write this info to the project
				if (g.Project != null) {
					g.Project.DebugExe = CProject.PathGetRelative(txtDebugExe.Text, g.Project.ProjectPath);
					g.Project.DebugPasswd = txtDebugPasswd.Text;
					g.Project.DebugPort = Convert.ToInt32(txtDebugPort.Text);
					g.Project.DebugParams = txtDebugParams.Text;

					if (optAutoDbg.Checked) {
						g.Project.DebugMainCs = CProject.PathGetRelative(txtMainCS.Text, g.Project.ProjectPath);
						g.Project.DebugAutoInsert = true;
					} else {
						g.Project.DebugMainCs = "";
						g.Project.DebugAutoInsert = false;
					}

					g.Project.DebugEnabled = true;
				}

				this.Close();
			}
		}

		private void cmdPrevious_Click(object sender, EventArgs e) {
			if (step == 1) {
				// What?!
				return;
			} else if (step == 2) {
				// Go back to step 1
				pnlStep2.Visible = false;
				pnlStep1.Visible = true;

				// Reset the "next" button text
				cmdNext.Text = "&Next ►";

				// Set the step and disable the previous button
				step = 1;
				cmdPrevious.Enabled = false;
			}
		}

		private void pnlStep2_VisibleChanged(object sender, EventArgs e) {
			if (pnlStep2.Visible) {
				lblTitle.Text = "Step 2 - Debugger Information";
			}
		}

		private void pnlStep1_VisibleChanged(object sender, EventArgs e) {
			if (pnlStep1.Visible) {
				lblTitle.Text = "Step 1 - Debug Control";
			}
		}

		private void cmdCancel_Click(object sender, EventArgs e) {
			this.Close();
		}

		private void optAutoDbg_CheckedChanged(object sender, EventArgs e) {
			lblMainCs.Enabled = cmdBrowseMainCs.Enabled = txtMainCS.Enabled = optAutoDbg.Checked;
		}

		private void cmdBrowseMainCs_Click(object sender, EventArgs e) {
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Select Main.Cs File";
			ofd.RestoreDirectory = true;
			ofd.InitialDirectory = g.Project.ProjectPath;
			ofd.CheckFileExists = true;
			ofd.Filter = "TorqueScript Files (*.cs)|*.cs|All Files (*.*)|*.*";

			DialogResult result = ofd.ShowDialog(this);

			if (result == DialogResult.Cancel)
				return;

			this.txtMainCS.Text = ofd.FileName;

			ofd.Dispose();
			ofd = null;
		}

		private void cmdBrowseEngine_Click(object sender, EventArgs e) {
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Select Engine Executable";
			ofd.RestoreDirectory = true;
			ofd.InitialDirectory = g.Project.ProjectPath;
			ofd.CheckFileExists = true;
			ofd.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";

			DialogResult result = ofd.ShowDialog(this);

			if (result == DialogResult.Cancel)
				return;

			this.txtDebugExe.Text = ofd.FileName;

			ofd.Dispose();
			ofd = null;
		}

		private void frmDebugWizard_Load(object sender, EventArgs e) {
			if (g.Project.DebugEnabled) {
				optAutoDbg.Checked = g.Project.DebugAutoInsert;
				optNonAutoDbg.Checked = !g.Project.DebugAutoInsert;

				try {
					txtMainCS.Text = Path.GetFullPath(g.Project.DebugMainCs);
				} catch { txtMainCS.Text = ""; }

				try {
					txtDebugExe.Text = Path.GetFullPath(g.Project.DebugExe);
				} catch { txtDebugExe.Text = ""; }

				txtDebugParams.Text = g.Project.DebugParams;
				txtDebugPasswd.Text = g.Project.DebugPasswd;
				txtDebugPort.Text = g.Project.DebugPort.ToString(); ;
			} else {
				txtDebugPasswd.Text = g.Project.ProjectName.ToLower().Replace(" ", "") + "_debug";
			}
		}
	}
}