/*
 * The MIT License
 *
 * Copyright 2018 Megan Leet (Zowayix).
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ROMniscience.GUI {
	partial class ViewFilesystems : Form {
		private Button okButton;
		private Panel treeViewHolder;
		private Button infoButton;
		private TreeView treeView;

		public static void viewFilesystems(ROMInfo info) {
			if (info.filesystems.Count == 0) {
				MessageBox.Show("There are no filesystems");
				return;
			}

			var me = new ViewFilesystems();
			foreach (var fs in info.filesystems) {
				me.treeView.Nodes.Add(treeNodeFromDir(fs));
			}
			me.ShowDialog();
		}

		private static TreeNode treeNodeFromDir(FilesystemDirectory d) {
			TreeNode root = new TreeNode(d.name) {
				Tag = d
			};
			foreach (var c in d.children) {
				if (c is FilesystemDirectory childDir) {
					root.Nodes.Add(treeNodeFromDir(childDir));
				} else {
					TreeNode child = new TreeNode(c.name) {
						Tag = c
					};
					root.Nodes.Add(child);
				}
			}
			root.Expand();
			return root;
		}

		public ViewFilesystems() {
			InitializeComponent();
			AcceptButton = okButton;
		}

		private void ok(object sender, EventArgs e) {
			Close();
		}


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.okButton = new System.Windows.Forms.Button();
			this.treeViewHolder = new System.Windows.Forms.Panel();
			this.treeView = new System.Windows.Forms.TreeView();
			this.infoButton = new System.Windows.Forms.Button();
			this.treeViewHolder.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(197, 227);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.ok);
			// 
			// treeViewHolder
			// 
			this.treeViewHolder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.treeViewHolder.Controls.Add(this.treeView);
			this.treeViewHolder.Location = new System.Drawing.Point(13, 13);
			this.treeViewHolder.Name = "treeViewHolder";
			this.treeViewHolder.Size = new System.Drawing.Size(259, 208);
			this.treeViewHolder.TabIndex = 1;
			// 
			// treeView
			// 
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Name = "treeView";
			this.treeView.Size = new System.Drawing.Size(259, 208);
			this.treeView.TabIndex = 0;
			// 
			// infoButton
			// 
			this.infoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.infoButton.Location = new System.Drawing.Point(116, 228);
			this.infoButton.Name = "infoButton";
			this.infoButton.Size = new System.Drawing.Size(75, 23);
			this.infoButton.TabIndex = 2;
			this.infoButton.Text = "Show info";
			this.infoButton.UseVisualStyleBackColor = true;
			this.infoButton.Click += new System.EventHandler(this.infoButton_Click);
			// 
			// ViewFilesystems
			// 
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.infoButton);
			this.Controls.Add(this.treeViewHolder);
			this.Controls.Add(this.okButton);
			this.Name = "ViewFilesystems";
			this.treeViewHolder.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private void infoButton_Click(object sender, EventArgs e) {
			var selectedNode = treeView.SelectedNode;
			if (selectedNode == null) {
				//Should I display a dialog box? Ehh.... nah (until someone convinces me otherwise)
				return;
			}
			if (!(selectedNode.Tag is FilesystemNode)) {
				MessageBox.Show("This shouldn't happen and is a sign of programmer error! Tag is not FilesystemNode, it is " + (selectedNode.Tag == null ? "null" : selectedNode.Tag.GetType().FullName));
				return;
			}
			var selectedFSNode = (FilesystemNode)selectedNode.Tag;
			bool isFile = selectedFSNode is FilesystemFile;

			var text = new StringBuilder();
			text.AppendFormat("Name: {0}", selectedFSNode.name).AppendLine();
			text.AppendFormat("Type: {0}", isFile ? "File" : "Folder").AppendLine();
			if (isFile) {
				var selectedFile = (FilesystemFile)selectedFSNode;
				text.AppendFormat("Offset: 0x{0:X2}", selectedFile.offset).AppendLine();
				text.AppendFormat("Size: {0}", ROMInfo.formatByteSize(selectedFile.size)).AppendLine();
			}
			MessageBox.Show(text.ToString());
		}
	}
}
