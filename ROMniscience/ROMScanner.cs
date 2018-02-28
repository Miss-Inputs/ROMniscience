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
using SharpCompress.Archives;
using System;
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
                try {
                    using (IArchive archive = ArchiveFactory.Open(f)) {
                        foreach (IArchiveEntry entry in archive.Entries) {
                            if (handler.handlesExtension(Path.GetExtension(entry.Key))) {
                                ROMInfo info;
                                using (ROMFile file = new ROMFile(entry, f)) {
                                    info = ROMInfo.getROMInfo(handler, file, datfiles);
                                }

                                onHaveRow(info);
                            }
                        }
                    }
                } catch (Exception ex) {
                    //HECK
                    //TODO Add event for this happening (so MainWindow can add the archive and exception as a row)
                    Console.WriteLine(ex);
                }
            }

            if (handler.handlesExtension(f.Extension)) {
                ROMInfo info;
                using (ROMFile file = new ROMFile(f)) {
                    info = ROMInfo.getROMInfo(handler, file, datfiles);
                }

                onHaveRow(info);
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
                            processFile(f, handler, datfiles);
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
