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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMniscience {
	class FilesystemDirectory: FilesystemNode {
		private List<FilesystemNode> _children = new List<FilesystemNode>();
		

		public void addChild(FilesystemNode node) {
			children.Add(node);
		}

		public void addChild(string name, long offset, long size) {
			var file = new FilesystemFile {
				name = name,
				offset = offset,
				size = size
			};
			addChild(file);
		}

		public bool contains(string name) {
			foreach(var child in children) {
				if (name.Equals(child.name)) {
					return true;
				}
			}
			return false;
		}

		public bool containsDeepSearch(string name) {
			foreach (var child in children) {
				if (name.Equals(child.name)) {
					return true;
				}
				if (child is FilesystemDirectory dir) {
					return dir.containsDeepSearch(name);
				}
			}
			return false;
		}

		public FilesystemNode getChild(string name) {
			foreach (var child in children) {
				if (name.Equals(child.name)) {
					return child;
				}
			}
			return null;
		}

		public FilesystemNode getChildDeepSearch(string name) {
			foreach (var child in children) {
				if (name.Equals(child.name)) {
					return child;
				}
				if (child is FilesystemDirectory dir) {
					var c = dir.getChildDeepSearch(name);
					if(c != null) {
						return c;
					}
				}
			}
			return null;
		}

		public IList<FilesystemNode> children => _children;
	}
}
