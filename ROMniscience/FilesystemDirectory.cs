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
