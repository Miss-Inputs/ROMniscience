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
 //Not that you would want to reuse this code anyway, yuck
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ROMniscience.Handlers;

namespace ROMniscience {
	//I hate GUIs and I just like I feel like I should apologise for this whole class it's just a load of fuck
	//just trying to get things to work
	//TODO: Fix current caveat of not being able to store equals signs as values
	class SettingsDialog: Form {
		private IList<FolderEditor> editors = new List<FolderEditor>();

		public SettingsDialog() {
			Text = "ROMniscience settings";
			MinimumSize = new System.Drawing.Size(500, 500);
			Size = MinimumSize;

			GroupBox editorHolderHolder = new GroupBox() {
				Left = 10,
				Top = 10,
				Size = new System.Drawing.Size(ClientSize.Width - 20, ClientSize.Height - 60),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left,
				Text = "Folder Locations"
			};
			TableLayoutPanel editorHolder = new TableLayoutPanel() {
				Dock = DockStyle.Fill,
				AutoScroll = true,
				ColumnCount = 1,
				GrowStyle = TableLayoutPanelGrowStyle.AddRows,
			};
			editorHolderHolder.Controls.Add(editorHolder);
			Controls.Add(editorHolderHolder);
			foreach(Handler h in Handler.allHandlers) {
				string existingValue = null;
				if(h.configured) {
					existingValue = h.folder.FullName;
				}

				FolderEditor fe = new FolderEditor(h.name, existingValue);
				editorHolder.Controls.Add(fe);
				editors.Add(fe);
			}
			editorHolder.Controls.Add(new Label());

			Panel buttonHolder = new Panel() {
				Top = 10 + editorHolder.Height,
				Left = 10,
				Size = new System.Drawing.Size(ClientSize.Width - 20, 50),
				Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
			};
			Controls.Add(buttonHolder);

			Button cancelButton = new Button() {
				Text = "Cancel",
				Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
				DialogResult = DialogResult.Cancel
			};
			cancelButton.Top = (buttonHolder.ClientSize.Height - cancelButton.ClientSize.Height) - cancelButton.Margin.Vertical;
			cancelButton.Left = (buttonHolder.ClientSize.Width - cancelButton.ClientSize.Width) - cancelButton.Margin.Horizontal;
			cancelButton.Click += delegate {
				Close();
			};
			CancelButton = cancelButton;
			buttonHolder.Controls.Add(cancelButton);

			Button okButton = new Button() {
				Text = "OK",
				Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
				DialogResult = DialogResult.OK
			};
			okButton.Top = cancelButton.Top;
			okButton.Left = (cancelButton.Left - cancelButton.Margin.Left) - okButton.Width;
			okButton.Click += delegate {
				saveSettings();
				Close();
			};
			AcceptButton = okButton;
			buttonHolder.Controls.Add(okButton);
		}

		class FolderEditor: Panel {
			public Label label {
				get;
			}
			public TextBox texty {
				get;
			}
			public Button browseButton {
				get;
			}

			public FolderEditor(String name, String existingValue) {
				//WELCOME TO FUCKING HELL I AM YOUR HOST WINFORMS AND I WILL TAKE A SHIT RIGHT UP YOUR BUTTHOLE
				//Don't even fucking try to have the button or text box aligned on the right or there will be
				//no end to your torment
				//Whatever you're thinking you can do to make this look less shit I'm telling you, I've been
				//wrangling with this bullshit for over 6 fucking hours straight now and it is at this point
				//that I just go fuck it, fuck WinForms, fuck everything it'll just have to be shit
				//I want to actually program fun shit and not fuck around with fucking shitty GUI libraries
				//I didn't even think it was possible for something to be worse than Swing but hey you did it Microsoft
				Name = name;
				Anchor = AnchorStyles.Left;
				AutoSize = true;
				label = new Label() {
					Text = name,
				};
				label.MinimumSize = label.Size;
				label.AutoSize = true;
				Controls.Add(label);

				texty = new TextBox() {
					Text = existingValue,
					Top = label.Top,
					Left = label.Right + label.Margin.Right
				};
				texty.Width *= 2; //fuckin I dunno
				Controls.Add(texty);

				Button butt = new Button() {
					Text = "Browse...",
					Top = label.Top,
					Left = texty.Right + texty.Margin.Right,
				};
				butt.Click += delegate {
					using(var folderBrowserThing = new FolderBrowserDialog()) {
						//FUCK C# only lets you use the fucking shitty ass bullshit folder chooser
						//Nah fuck it this is a TODO fuckin let's trick the file open dialog into selecting a folder I reckon it can be done

						if(folderBrowserThing.ShowDialog() == DialogResult.OK) {
							texty.Text = folderBrowserThing.SelectedPath;
						}
					}
					
				};
				Controls.Add(butt);
				Height = (new int[] { label.Height, texty.Height, butt.Height }).Max();
			}
		}

		private void saveSettings() {
			IDictionary<string, string> settings = new Dictionary<string, string>();
			foreach(FolderEditor f in editors) {
				string value = f.texty.Text?.Trim().Replace('=', '-');
				if(String.IsNullOrEmpty(value)) {
					settings.Add(f.Name, null);
				} else {
					settings.Add(f.Name, f.texty.Text);
				}
			}
			SettingsManager.writeSettings(settings);
		}
	}
}
