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

using ROMniscience.Datfiles;
using ROMniscience.Handlers;
using ROMniscience.IO.CueSheets;
using SharpCompress.Archives;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class ROMScanner {
		public event EventHandler datfilesLoadStart;
		public event EventHandler datfilesLoadEnd;
		public event EventHandler<HaveRowEventArgs> haveRow;
		public event EventHandler<RunningWorkersUpdatedEventArgs> runningWorkersUpdated;
		public event EventHandler<ExceptionEventArgs> exceptionHappened;

		public class ExceptionEventArgs : EventArgs {
			public Exception ex { get; set; }
			public FileInfo path { get; set; }
		}

		public class RunningWorkersUpdatedEventArgs : EventArgs {
			public ConcurrentDictionary<string, bool> runningWorkers { get; set; }
		}

		public class HaveRowEventArgs : EventArgs {
			public ROMInfo info { get; set; }
		}

		protected virtual void onDatfilesLoadStart() {
			datfilesLoadStart?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void onDatfilesLoadEnd() {
			datfilesLoadEnd?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void onException(Exception exception, FileInfo fi) {
			var args = new ExceptionEventArgs {
				ex = exception,
				path = fi,
			};
			exceptionHappened?.Invoke(this, args);
		}

		protected virtual void onHaveRow(ROMInfo info) {
			HaveRowEventArgs args = new HaveRowEventArgs {
				info = info
			};
			haveRow?.Invoke(this, args);
		}

		protected virtual void onSetStatus(ConcurrentDictionary<string, bool> runningWorkers) {
			RunningWorkersUpdatedEventArgs args = new RunningWorkersUpdatedEventArgs() {
				runningWorkers = runningWorkers
			};
			runningWorkersUpdated?.Invoke(this, args);
		}

		private void processFile(FileInfo f, Handler handler, DatfileCollection datfiles) {
			if (IO.ArchiveHelpers.isArchiveExtension(f.Extension)) {
				processArchive(f, handler, datfiles);
			} else if (IO.ArchiveHelpers.isGCZ(f.Extension)) {
				processGCZ(f, handler, datfiles);
			} else if (CueSheet.isCueExtension(f.Extension)) {
				processCueSheet(f, handler, datfiles);
			} else if (handler.handlesExtension(f.Extension)) {
				processNormalFile(f, handler, datfiles);
			}
		}

		private class GenericCueHandler : Handler {
			string _name;

			public GenericCueHandler(string name) {
				_name = name;
			}

			public override IDictionary<string, string> filetypeMap => CDBasedSystem.genericCueSheetFiletypeMap;

			public override string getFiletypeName(string extension) {
				string _base = base.getFiletypeName(extension);
				if(_base == null) {
					return _name + " " + extension + " file";
				}
				return _name + " " + _base;
			}

			public override bool handlesExtension(string extension) {
				return true;
			}

			public override string name => _name;

			public override void addROMInfo(ROMInfo info, ROMFile file) {
				info.addInfo("Platform", name);
			}
		}

		private void processCueSheet(FileInfo f, Handler handler, DatfileCollection datfiles) {
			//Add the cue itself for identification purposes, although no handler would handle it correctly (since they expect to see the actual data of the thing) so use a fake stub one
			Handler fakeHandler = new GenericCueHandler(handler.name);
			ROMInfo info;
			using (ROMFile file = new NormalROMFile(f)) {
				info = ROMInfo.getROMInfo(fakeHandler, file, datfiles);
			}
			onHaveRow(info);

			using(var cueStream = f.OpenRead()) {
				var cue = CueSheet.create(cueStream, f.Extension);
				foreach(var cueFile in cue.filenames) {
					FileInfo filename = new FileInfo(Path.Combine(f.DirectoryName, cueFile.filename));

					using(ROMFile file = new NormalROMFile(filename)) {
						file.cdSectorSize = cueFile.sectorSize;
						if (cueFile.isData) {
							info = ROMInfo.getROMInfo(handler, file, datfiles);
						} else {
							info = ROMInfo.getROMInfo(fakeHandler, file, datfiles);
						}
					}
					onHaveRow(info);
				}
			}
		}

		private void processNormalFile(FileInfo f, Handler handler, DatfileCollection datfiles) {
			ROMInfo info;
			using (ROMFile file = new NormalROMFile(f)) {
				info = ROMInfo.getROMInfo(handler, file, datfiles);
			}
			onHaveRow(info);
		}

		private void processGCZ(FileInfo f, Handler handler, DatfileCollection datfiles) {
			//Refactor this later if I ever support any other kind of "custom" compressed formats like this
			ROMInfo info;
			using (GCZROMFile gcz = new GCZROMFile(f)) {
				info = ROMInfo.getROMInfo(handler, gcz, datfiles);
			}
			onHaveRow(info);
		}

		private void processArchive(FileInfo f, Handler handler, DatfileCollection datfiles) {
			try {
				using (IArchive archive = ArchiveFactory.Open(f)) {
					foreach (IArchiveEntry entry in archive.Entries) {
						if (handler.handlesExtension(Path.GetExtension(entry.Key))) {
							ROMInfo info;
							using (ROMFile file = new CompressedROMFile(entry, f)) {
								info = ROMInfo.getROMInfo(handler, file, datfiles);
							}

							onHaveRow(info);
						}
					}
				}
			} catch (Exception ex) {
				//HECK
				onException(ex, f);
			}
		}

		public void startScan() {
			onDatfilesLoadStart();
			DatfileCollection datfiles = null;
			string datFolder = SettingsManager.readSetting("datfiles");
			if (datFolder != null) {
				datfiles = DatfileCollection.loadFromFolder(new DirectoryInfo(datFolder));
			}
			onDatfilesLoadEnd();


			ConcurrentDictionary<string, bool> runningWorkers = new ConcurrentDictionary<string, bool>();

			foreach (Handler handler in Handler.allHandlers) {
				if (handler.configured && handler.enabled) {
					BackgroundWorker bw = new BackgroundWorker();
					bw.DoWork += delegate {
						if (!handler.folder.Exists) {
							System.Diagnostics.Trace.TraceWarning("{0} has folder {1} but that doesn't exist", handler.name, handler.folder);
							return;
						}
						foreach (FileInfo f in handler.folder.EnumerateFiles("*", SearchOption.AllDirectories)) {
							try {
								processFile(f, handler, datfiles);
							} catch (Exception ex) {
								onException(ex, f);
							}
						}
					};


					bw.RunWorkerCompleted += delegate {
						runningWorkers[handler.name] = false;
						onSetStatus(runningWorkers);
					};
					runningWorkers.TryAdd(handler.name, true);
					onSetStatus(runningWorkers);

					bw.RunWorkerAsync();
				}
			}
		}
	}
}
