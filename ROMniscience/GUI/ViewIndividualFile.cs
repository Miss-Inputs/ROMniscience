/*
 * The MIT License
 *
 * Copyright 2017 Megan Leet (Zowayix).
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ROMniscience.Datfiles;
using ROMniscience.Handlers;
using SharpCompress.Archives;

namespace ROMniscience {
	class ViewIndividualFile: Form {
		private Panel fuckinPanelIGuess;
		private TextBox notALabel;
		private Button okButton;
		private Button showImagesButton;
		private IDictionary<string, Image> images = new Dictionary<string, Image>();

		public static T chooseChoices<T>(IEnumerable<T> choices, string displayKey, string helpText, string title) where T: class {
			if(choices.Count() == 1) {
				return choices.First();
			}

			Form f = new Form() {
				DialogResult = DialogResult.Cancel,
				Size = new Size(500, 500),
				MinimumSize = new Size(500, 500),
				Text = "ROMniscience: " + title,
			};
			T choice = null;

			Label helpLabel = new Label() {
				Text = helpText,
				Width = f.ClientSize.Width - 20,
				Height = 50,
				Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
				Top = 10,
				Left = 10,
			};
			f.Controls.Add(helpLabel);

			ListBox list = new ListBox() {
				DisplayMember = displayKey,
				SelectionMode = SelectionMode.One,
				Width = f.ClientSize.Width - 20,
				Height = 350,
				Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top,
				Top = helpLabel.Bottom + 10,
				Left = 10,
			};

			f.Controls.Add(list);
			foreach(T t in choices) {
				list.Items.Add(t);
			}

			Button okButton = new Button() {
				Anchor = AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right,
				Text = "OK",
				Top = list.Bottom + 10,
				DialogResult = DialogResult.OK,
			};
			okButton.Left = (f.ClientSize.Width - 10) - okButton.Width;
			okButton.Click += delegate {
				choice = (T)list.SelectedItem;
				f.Close();
			};

			Button cancelButton = new Button() {
				Anchor = AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right,
				Text = "Cancel",
				Top = list.Bottom + 10,
				DialogResult = DialogResult.Cancel,
			};
			cancelButton.Left = (okButton.Left - 10) - cancelButton.Width;
			
			f.Controls.Add(okButton);
			f.Controls.Add(cancelButton);
			f.AcceptButton = okButton;
			f.CancelButton = cancelButton;

			DialogResult result = f.ShowDialog();
			if(result == DialogResult.OK) {
				return choice;
			}
			return null;
		}

		public static void viewFile(string path) {
			viewFile(new FileInfo(path));
		}

		public static void viewFile(FileInfo path) {
			if (IO.ArchiveHelpers.isArchiveExtension(path.Extension)) {
				try {
					using (IArchive archive = ArchiveFactory.Open(path)) {
						string helpString = "This is an archive and there's multiple files in it. Please choose which one you want to view the info for.";
						IArchiveEntry choice = chooseChoices(archive.Entries, "Key", helpString, "Choose File in Archive");
						if (choice != null) {
							using (ROMFile f = new ROMFile(choice, path)) {
								viewFile(f);
							}
						}
					}
				} catch (Exception ex) {
					MessageBox.Show(ex.ToString(), "Uh oh spaghetti-o");
				}
			} else if (IO.ArchiveHelpers.isGCZ(path.Extension)) {
				using(GCZROMFile gcz = new GCZROMFile(path)) {
					viewFile(gcz);
				}
			} else {
				using(ROMFile f = new ROMFile(path)) {
					viewFile(f);
				}
			}
		}

		public static void viewFile(ROMFile rom) {
			IList<Handler> handlers = findHandlersForExtension(rom.extension);

			Handler handler = null;
			if(handlers.Count == 0) {
				handler = chooseChoices(Handler.allHandlers.Where(h => !(h is Handlers.Stubs.StubHandler)).OrderBy(h => h.name), "name", "I don't know what this file extension means. Do you want to try and read it as something else?", "Force Handler");
			} else {
				handler = chooseChoices(handlers.OrderBy((h) => h.name), "name", 
					"This file extension could be a number of things. Which handler do you want to try and read this with?", "Choose Handler");
			}
			if(handler == null) {
				return;
			}

			string datFolder = SettingsManager.readSetting("datfiles");
			DatfileCollection datfiles = null;
			if(datFolder != null && handler.shouldCalculateHash) {
				datfiles = DatfileCollection.loadFromFolder(new DirectoryInfo(datFolder));
			}

			ROMInfo info = ROMInfo.getROMInfo(handler, rom, datfiles);

			ViewIndividualFile me = new ViewIndividualFile {
				Text = String.Format("ROMniscience: {0} ({1})", rom.path, handler.name)
			};

			foreach(var thing in info.info) {
				object value = thing.Value.value;
				if(value is Image) {
					me.images.Add(thing.Key, (Image)value);
					me.showImagesButton.Enabled = true;
					continue;
				}
				if(thing.Value.formatMode == ROMInfo.FormatMode.SIZE) {
					try {
						value = ROMInfo.formatByteSize(Convert.ToInt64(value));
					} catch(InvalidCastException) {
						//You goof
						value = String.Format("{0} cannot be casted to long, it is {1}", value, value?.GetType());
					}
				}
				if(thing.Value.formatMode == ROMInfo.FormatMode.PERCENT) {
					value = String.Format("{0:P2}", value);
				}
				if(thing.Value.formatMode == ROMInfo.FormatMode.HEX) {
					value = string.Format("0x{0:X2}", value);
				}
				if(value is byte[] bytes) {
					value = BitConverter.ToString(bytes);
				}
				if(value is string[] strings) {
					value = String.Join(", ", strings);
				}
				if(value is string str) {
					//TextBox doesn't like null chars. I dunno what the best thing to replace it with is, but that'll do
					value = str.Replace('\0', ' ');
				}

				me.notALabel.Text += String.Format("{0} => {1}{2}", thing.Key, value, Environment.NewLine);
			}

			me.ShowDialog();
		}

		public static IList<Handler> findHandlersForExtension(string ext) {
			IList<Handler> listy = new List<Handler>();
			foreach(Handler handler in Handler.allHandlers) {
				if(handler.handlesExtension(ext)) {
					listy.Add(handler);
				}
			}
			return listy;
		}

		public ViewIndividualFile() {
			InitializeComponent();
			AcceptButton = okButton;
			if(Environment.OSVersion.Platform == PlatformID.Unix && MainProgram.isMono) {
				//Some would argue I shouldn't do this, but right now Mono seems to be setting
				//the default fonts to DejaVu Sans and it doesn't support Japanese characters and
				//I'm not okay with that
				notALabel.Font = new Font("Noto Sans CJK JP", SystemFonts.DialogFont.SizeInPoints);
			} else {
				notALabel.Font = SystemFonts.DialogFont;
			}
		}


		//I give up I surrender I'm just using the GUI builder fuck it
		//Call me a coward if you will but I am not staying up until 7am again
#pragma warning disable IDE1006 // Naming Styles
		private void InitializeComponent() {
#pragma warning restore IDE1006 // Naming Styles
			this.okButton = new System.Windows.Forms.Button();
			this.fuckinPanelIGuess = new System.Windows.Forms.Panel();
			this.notALabel = new System.Windows.Forms.TextBox();
			this.showImagesButton = new System.Windows.Forms.Button();
			this.fuckinPanelIGuess.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(197, 227);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.ok);
			// 
			// fuckinPanelIGuess
			// 
			this.fuckinPanelIGuess.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.fuckinPanelIGuess.Controls.Add(this.notALabel);
			this.fuckinPanelIGuess.Location = new System.Drawing.Point(13, 13);
			this.fuckinPanelIGuess.Name = "fuckinPanelIGuess";
			this.fuckinPanelIGuess.Size = new System.Drawing.Size(259, 208);
			this.fuckinPanelIGuess.TabIndex = 0;
			// 
			// notALabel
			// 
			this.notALabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notALabel.Location = new System.Drawing.Point(0, 0);
			this.notALabel.Multiline = true;
			this.notALabel.Name = "notALabel";
			this.notALabel.ReadOnly = true;
			this.notALabel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.notALabel.Size = new System.Drawing.Size(259, 208);
			this.notALabel.TabIndex = 0;
			// 
			// showImagesButton
			// 
			this.showImagesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.showImagesButton.Enabled = false;
			this.showImagesButton.Location = new System.Drawing.Point(116, 227);
			this.showImagesButton.Name = "showImagesButton";
			this.showImagesButton.Size = new System.Drawing.Size(75, 23);
			this.showImagesButton.TabIndex = 2;
			this.showImagesButton.Text = "Images...";
			this.showImagesButton.UseVisualStyleBackColor = true;
			this.showImagesButton.Click += new System.EventHandler(this.showImagesButton_Click);
			// 
			// ViewIndividualFile
			// 
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.showImagesButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.fuckinPanelIGuess);
			this.Name = "ViewIndividualFile";
			this.Text = "ROMniscience: ";
			this.fuckinPanelIGuess.ResumeLayout(false);
			this.fuckinPanelIGuess.PerformLayout();
			this.ResumeLayout(false);

		}

		private void ok(object sender, EventArgs e) {
			Close();
		}

		private void showImagesButton_Click(object sender, EventArgs e) {
			Form f = new Form();
			FlowLayoutPanel flop = new FlowLayoutPanel() {
				FlowDirection = FlowDirection.LeftToRight,
				Dock = DockStyle.Fill,
				AutoScroll = true,
			};
			f.Controls.Add(flop);
			foreach(var image in images) {
				Label label = new Label() {
					Text = image.Key
				};
				PictureBox pic = new PictureBox() {
					Image = image.Value
				};
				flop.Controls.Add(label);
				flop.Controls.Add(pic);
				flop.SetFlowBreak(pic, true);
			}
			f.ShowDialog();
		}
	}
}
