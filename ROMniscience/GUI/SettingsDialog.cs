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
		TextBox datFolderBox;
		CheckBox showExtra;

		public SettingsDialog() {
			Text = "ROMniscience settings";
			MinimumSize = new System.Drawing.Size(500, 500);
			Size = MinimumSize;

			Label datFolderLabel = new Label() {
				Text = "Datfile folder",
				Left = 10,
				Top = 10,
				Size = new System.Drawing.Size(100, 30), //I hope that'll be enough
				Anchor = AnchorStyles.Top | AnchorStyles.Left,
			};
			Controls.Add(datFolderLabel);
			datFolderBox = new TextBox() {
				Left = datFolderLabel.Left + datFolderLabel.Width,
				Top = 10,
				Size = new System.Drawing.Size(ClientSize.Width - (datFolderLabel.Width + 20 + 80), 30),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Text = SettingsManager.readSetting("datfiles"),
			};

			Controls.Add(datFolderBox);
			Button datFolderButt = new Button() {
				Text = "Browse...",
				Top = 10,
				Left = datFolderBox.Left + datFolderBox.Width + 10,
				Size = new System.Drawing.Size(ClientSize.Width - (datFolderLabel.Width + datFolderBox.Width + 30), 30),
				Anchor = AnchorStyles.Top | AnchorStyles.Right,
			};
			Controls.Add(datFolderButt);
			datFolderButt.Click += delegate {
				browseForFolder(datFolderBox);
			};

			showExtra = new CheckBox() {
				Text = "Show extra information in table view",
				Top = 40,
				Left = 10,
				Size = new System.Drawing.Size(ClientSize.Width - 20, 30),
			};
			if(bool.TryParse(SettingsManager.readSetting("show_extra"), out bool result)) {
				showExtra.Checked = result;
			}
			Controls.Add(showExtra);


			GroupBox editorHolder = new GroupBox() {
				Left = 10,
				Top = 70,
				Size = new System.Drawing.Size(ClientSize.Width - 20, ClientSize.Height - 120),
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left,
				Text = "Folder Locations", //FIXME Why the fuck is this covered up by some shit and isn't visible... You know what I don't even care anymore
			};
			ScrollableControl scrollArea = new ScrollableControl() {
				Dock = DockStyle.Fill,
				AutoScroll = true,
				Text = String.Empty,
			};
			editorHolder.Controls.Add(scrollArea);
			Controls.Add(editorHolder);

			int last = scrollArea.ClientRectangle.Top + scrollArea.Padding.Vertical;
			foreach(Handler h in Handler.allHandlers.OrderBy((Handler h) => h.name)) {
				FolderEditor fe = new FolderEditor(last, h.name);

				fe.Width = scrollArea.Width;
				last = fe.Bottom;
				scrollArea.Controls.Add(fe);
				editors.Add(fe);
			}

			Panel buttonHolder = new Panel() {
				Top = editorHolder.Top + editorHolder.Height,
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
			public CheckBox enabledChecky {
				get;
			}
			public IDictionary<string, string> settingsToSave {
				get {
					return new Dictionary<string, string>(){
						{Name, texty.Text},
						{(string)enabledChecky.Tag, enabledChecky.Checked.ToString()}
					};
				}
			}
			Label divider {
				get;
			}

			public FolderEditor(int top, String name) {
				//WELCOME TO FUCKING HELL I AM YOUR HOST WINFORMS AND I WILL TAKE A SHIT RIGHT UP YOUR BUTTHOLE
				//Don't even fucking try to have the button or text box aligned on the right or there will be
				//no end to your torment
				//Whatever you're thinking you can do to make this look less shit I'm telling you, I've been
				//wrangling with this bullshit for over 6 fucking hours straight now and it is at this point
				//that I just go fuck it, fuck WinForms, fuck everything it'll just have to be shit
				//I want to actually program fun shit and not fuck around with fucking shitty GUI libraries
				//I didn't even think it was possible for something to be worse than Swing but hey you did it Microsoft
				//Update: Yeah I made it look nicer but I still hate it
				Name = name;
				Top = top;
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

				label = new Label() {
					Text = name,
					Top = 0,
				};
				label.AutoSize = true;
				Controls.Add(label);

				enabledChecky = new CheckBox() {
					Text = "Enabled",
					Top = label.Bottom + label.Margin.Vertical,
					Tag = name + "_enabled",
				};
				Controls.Add(enabledChecky);
				if (SettingsManager.doesKeyExist((string)enabledChecky.Tag)) {
					if (bool.TryParse(SettingsManager.readSetting((string)enabledChecky.Tag), out bool result)) {
						enabledChecky.Checked = result;
					}
				} else {
					enabledChecky.Checked = true;
				}

				texty = new TextBox() {
					Top = enabledChecky.Bottom + enabledChecky.Margin.Vertical,
				};
				if (SettingsManager.doesKeyExist(name)) {
					texty.Text = SettingsManager.readSetting(name);
				}
				Controls.Add(texty);

				Button butt = new Button() {
					Text = "Browse...",
					Top = texty.Top,
				};
				butt.Click += delegate {
					browseForFolder(texty);
				};
				texty.Size = new System.Drawing.Size(Width - texty.Left - (butt.Width + butt.Margin.Left), texty.Height);
				butt.Left = texty.Left + texty.Width + butt.Margin.Left;
				texty.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				butt.Anchor = AnchorStyles.Top | AnchorStyles.Right;
				Controls.Add(butt);

				divider = new Label() {
					Text = String.Empty,
					BorderStyle = BorderStyle.Fixed3D,
					AutoSize = false,
					Size = new System.Drawing.Size(Width - Padding.Horizontal, 2),
					Top = butt.Bottom + butt.Margin.Vertical, //Haha butt
					Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				};
				Controls.Add(divider);
				Height = divider.Bottom;
			}
		}

		private static void browseForFolder(Control result) {
			using(var folderBrowserThing = new FolderBrowserDialog()) {
				//FUCK C# only lets you use the fucking shitty ass bullshit folder chooser
				//Nah fuck it this is a TODO fuckin let's trick the file open dialog into selecting a folder I reckon it can be done

				if(folderBrowserThing.ShowDialog() == DialogResult.OK) {
					result.Text = folderBrowserThing.SelectedPath;
				}
			}
		}

		private void saveSettings() {
			IDictionary<string, string> settings = new Dictionary<string, string>();
			foreach(FolderEditor f in editors) {
				foreach(var setting in f.settingsToSave) {
					saveSetting(settings, setting.Key, setting.Value);
				}
			}
			saveSetting(settings, "datfiles", datFolderBox.Text);
			saveSetting(settings, "show_extra", showExtra.Checked.ToString());
			SettingsManager.writeSettings(settings);
		}

		private void saveSetting(IDictionary<string, string> settings, string name, string value) {
			value = value?.Trim().Replace('=', '-');
			if(String.IsNullOrEmpty(value)) {
				settings.Add(name, null);
			} else {
				settings.Add(name, value);
			}
		}
	}
}
