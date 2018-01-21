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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ROMniscience.Handlers;
using System.IO;
using ROMniscience.Datfiles;
using SharpCompress.Archives;
using System.Collections.Concurrent;

namespace ROMniscience {
	class MainWindow: Form {
		DataGridView table = new DoubleBufferedDataGridView();
		StatusStrip statusBar = new StatusStrip() {
			//So the thing is Mono is just gonna be weird about this and put stuff in the middle, which looks even
			//uglier compared to cutting off stuff
			LayoutStyle = MainProgram.isMono ? ToolStripLayoutStyle.Table : ToolStripLayoutStyle.Flow,
			ShowItemToolTips = true,
		};
		ToolStripStatusLabel statusText = new ToolStripStatusLabel() {
			Spring = !MainProgram.isMono,
		};

		readonly string[] DEFAULT_COLUMNS = {
			"Filename",
			"Folder",
			"Uncompressed filename",
			"Datfile",
			"Datfile game name",
			"Datfile ROM name",
			"Datfile ROM status",
			"Size",
			"Compressed size",
			"Compression ratio",
			"Platform",
			"File type",
			"Icon",
			"Internal name",
			"Product code",
			"Version",
			"Region",
			"Type",
			"Manufacturer"};

		class DoubleBufferedDataGridView: DataGridView {
			//Microsoft why? Why'd you make me do this? Come on Microsoft you
			//almost made me think you were actually cool for a bit and that you
			//made the perfect language and a GUI toolkit that just actually works and I
			//just thought aaaaaaaaa
			//(For those of you playing at home, I need to set this table to double buffered in order for it to
			//actually redraw when you scroll around and stuff, but it's protected access in DataGridView for
			//no discernible reason)
			public DoubleBufferedDataGridView() {
				DoubleBuffered = true;
			}
		}

		public MainWindow() {
			Text = "ROMniscience";
			MinimumSize = new System.Drawing.Size(500, 500);

			//table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; //performance fucking DIES
			//table.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			table.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			table.AllowUserToAddRows = false;
			table.AutoGenerateColumns = false;
			table.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			table.MultiSelect = false;
			table.CellFormatting += formatCell;
			table.SortCompare += noCrashySort;
			table.DataError += tableErrorHandler;
			foreach(string columnName in DEFAULT_COLUMNS) {
				int index = table.Columns.Add(columnName, columnName);
				table.Columns[index].FillWeight = 1;
			}
			table.Dock = DockStyle.Fill;
			Controls.Add(table);

			statusBar.Items.Add(statusText);
			Controls.Add(statusBar);

			setupMenu();
		}

		private void tableErrorHandler(object sender, DataGridViewDataErrorEventArgs e) {
			DataGridViewCell cell = table[e.ColumnIndex, e.RowIndex];
			if(cell is DataGridViewImageCell && e.Context.HasFlag(DataGridViewDataErrorContexts.Formatting)) {
				//Apparently using DataGridViewImageCell the way you're supposed to use it (by putting an image in it) generates errors? Go figure
				e.Cancel = true;
			}
		}

		private void noCrashySort(object sender, DataGridViewSortCompareEventArgs args) {
			if(args.CellValue1 is string && !(args.CellValue2 is string)) {
				args.SortResult = 1;
				args.Handled = true;
				return;
			} else if(args.CellValue2 is string && !(args.CellValue1 is string)) {
				args.SortResult = -1;
				args.Handled = true;
				return;
			}

			if(args.CellValue1 is int && args.CellValue2 is long) {
				//Are you fuckin serious
				args.SortResult = ((long)(int)args.CellValue1).CompareTo((long)args.CellValue2);
				args.Handled = true;
			} else if(args.CellValue1 is long && args.CellValue2 is int) {
				args.SortResult = ((long)args.CellValue1).CompareTo((int)args.CellValue2);
				args.Handled = true;
			}
		}

		private void setupMenu() {
			Menu = new MainMenu();

			MenuItem fileMenu = new MenuItem("File");
			Menu.MenuItems.Add(fileMenu);

			MenuItem scanItem = new MenuItem("Scan!") {
				Shortcut = Shortcut.CtrlS
			};
			scanItem.Click += delegate {
				startScan();
			};
			fileMenu.MenuItems.Add(scanItem);

			MenuItem autosizeColumnsItem = new MenuItem("Autosize columns");
			autosizeColumnsItem.Click += delegate {
				table.AutoResizeColumns();
			};
			Menu.MenuItems.Add(autosizeColumnsItem);

			MenuItem autosizeRowsItem = new MenuItem("Autosize rows");
			autosizeRowsItem.Click += delegate {
				table.AutoResizeRows();
			};
			Menu.MenuItems.Add(autosizeRowsItem);

			MenuItem exportItem = new MenuItem("Export to CSV");
			exportItem.Click += delegate {
				SaveFileDialog fileDialog = new SaveFileDialog() {
					DefaultExt = ".csv"
				};
				if(fileDialog.ShowDialog() == DialogResult.OK) {
					CSVWriter.writeCSV(table, new FileInfo(fileDialog.FileName));
					MessageBox.Show("Done!");
				}
			};
			fileMenu.MenuItems.Add(exportItem);

			MenuItem settingsItem = new MenuItem("Settings");
			settingsItem.Click += delegate {
				(new SettingsDialog()).ShowDialog();
			};
			fileMenu.MenuItems.Add(settingsItem);

			MenuItem quitItem = new MenuItem("Quit") {
				Shortcut = Shortcut.CtrlQ
			};
			quitItem.Click += delegate {
				this.Close();
			};
			fileMenu.MenuItems.Add(quitItem);

		}

		private void formatCell(object sender, DataGridViewCellFormattingEventArgs args) {
			//Welcome to the most called function in the program! So this needs to be hecking fast
			if(args.Value is string[]) {
				args.Value = String.Join(", ", (string[])args.Value);
				args.FormattingApplied = true;
				return;
			}
			if(args.Value is byte[]) {
				args.Value = BitConverter.ToString((byte[])args.Value);
				args.FormattingApplied = true;
				return;
			}
			if(ROMInfo.FormatMode.SIZE.Equals(args.CellStyle.Tag)) {
				try {
					//Okay so apparently I have to double cast to avoid weird boxing shit that's weird
					args.Value = ROMInfo.formatByteSize(args.Value is long ? (long)args.Value : (int)args.Value);
					args.FormattingApplied = true;
				} catch(InvalidCastException) {
					args.FormattingApplied = false;
				}
				return;
			}
			if(ROMInfo.FormatMode.PERCENT.Equals(args.CellStyle.Tag)) {
				args.Value = String.Format("{0:P2}", args.Value);
				args.FormattingApplied = true;
				return;
			}
		}

		private void startScan() {
			table.Rows.Clear();
			//TODO Separate this logic from the GUI, and notify when all workers are finished

			statusText.Text = "Loading datfiles";
			statusBar.Refresh();
			DatfileCollection datfiles = null;
			string datFolder = SettingsManager.readSetting("datfiles");
			if(datFolder != null) {
				datfiles = DatfileCollection.loadFromFolder(new DirectoryInfo(datFolder));
			}
			statusText.Text = "Datfiles loaded";
			statusBar.Refresh();

			ConcurrentDictionary<string, bool> runningWorkers = new ConcurrentDictionary<string, bool>();

			foreach(Handler handler in Handler.allHandlers) {
				if(handler.configured) {
					BackgroundWorker bw = new BackgroundWorker();
					bw.DoWork += delegate {
						if(!handler.folder.Exists) {
							System.Diagnostics.Trace.TraceWarning("{0} has folder {1} but that doesn't exist", handler.name, handler.folder);
							return;
						}
						foreach(FileInfo f in handler.folder.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)) {
							if(IO.ArchiveHelpers.isArchiveExtension(f.Extension)) {
								try {
									using(IArchive archive = ArchiveFactory.Open(f)) {
										foreach(IArchiveEntry entry in archive.Entries) {
											if(handler.handlesExtension(Path.GetExtension(entry.Key))) {
												ROMInfo info;
												using(ROMFile file = new ROMFile(entry, f)) {
													info = ROMInfo.getROMInfo(handler, file, datfiles);
												}

												table.Invoke(new addRowDelegate(addRow), info.info);
											}
										}
									}
								} catch (Exception ex) {
									//HECK
									//TODO Add the archive and this exception as a row
									Console.WriteLine(ex);
								}
							}

							if(handler.handlesExtension(f.Extension)) {
								ROMInfo info;
								using(ROMFile file = new ROMFile(f)) {
									info = ROMInfo.getROMInfo(handler, file, datfiles);
								}

								table.Invoke(new addRowDelegate(addRow), info.info);
							}
						}
					};


					bw.RunWorkerCompleted += delegate {
						runningWorkers[handler.name] = false;
						//Console.WriteLine("Currently running: {0}", String.Join(", ", runningWorkers.Where(kv => kv.Value).Select(kv => kv.Key)));
						statusBar.Invoke(new setStatusDelegate(setStatus), runningWorkers);
					};
					runningWorkers.TryAdd(handler.name, true);
					setStatus(runningWorkers);


					bw.RunWorkerAsync();
				}
			}
		}

		private delegate void setStatusDelegate(ConcurrentDictionary<string, bool> runningWorkers);

		private void setStatus(ConcurrentDictionary<string, bool> runningWorkers) {
			var currentlyRunning = runningWorkers.Where(kv => kv.Value);
			if(currentlyRunning.Count() == 0) {
				setStatus("Done!");
			} else {
				setStatus(String.Format("Currently running: {0}", String.Join(", ", currentlyRunning.Select(kv => kv.Key))));
			}
			statusBar.Refresh();
		}

		private void setStatus(string text) {
			statusText.Text = text;
			statusBar.Refresh();
		}

		private delegate void addRowDelegate(IDictionary<string, Tuple<object, ROMInfo.FormatMode>> data);

		private void addRow(IDictionary<string, Tuple<object, ROMInfo.FormatMode>> data) {
			int newRow = table.Rows.Add();
			foreach(var kv in data) {

				object value = kv.Value.Item1;

				if(!table.Columns.Contains(kv.Key)) {
					int index = table.Columns.Add(kv.Key, kv.Key);
					table.Columns[index].FillWeight = 1;
				}

				if(value is System.Drawing.Image image && !MainProgram.isMono) {
					//FIXME: Mono doesn't like displaying images, it offsets all cells after that by one cell to the right, and I have no idea why
					var cell = new DataGridViewImageCell();
					table[kv.Key, newRow] = cell;
					cell.ReadOnly = true;
					cell.Value = image;
				} else {
					//This reminds me too much of programming in Excel/VBA and I'm slightly uncomfortable
					//DataGridViewCell cell = table[columnIndex, newRow];
					try {
						DataGridViewCell cell = table[kv.Key, newRow];
						cell.Value = value;
						//cell.Value = String.Format("{0} (R{1}C{2}) => {3}", kv.Key, cell.RowIndex, cell.ColumnIndex, kv.Value.Item1);
						cell.Style.Tag = kv.Value.Item2;
						if(value is string str && str.Contains(Environment.NewLine)) {
							cell.Style.WrapMode = DataGridViewTriState.True;
						}
						if(Environment.OSVersion.Platform == PlatformID.Unix && MainProgram.isMono) {
							//Some would argue I shouldn't do this, but right now Mono seems to be setting
							//the default fonts to DejaVu Sans and it doesn't support Japanese characters and
							//I'm not okay with that
							cell.Style.Font = new System.Drawing.Font("Noto Sans CJK JP", System.Drawing.SystemFonts.DefaultFont.SizeInPoints);
						}
					} catch(Exception e) {
						//This is gonna kaboom the whole thing otherwise because threads I guess
						System.Diagnostics.Trace.TraceError(e.ToString());
					}
				}
				table.AutoResizeRow(newRow);
			}
		}

	}
}
