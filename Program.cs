using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Options;

class MainClass
{
	public static int Main (string [] args)
	{
		System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo ("es-ES");

		var options = new OptionSet
		{
		};

		try {
			var dirs = options.Parse (args);

			if (dirs.Count != 2) {
				Console.WriteLine ("Needs to directories to compare.");
				return 1;
			}

			for (int i = 0; i < dirs.Count; i++) {
				if (!Directory.Exists (dirs [i])) {
					Console.WriteLine ($"Directory {dirs [i]} does not exist.");
					return 1;
				}

				// Remove trailing slashes
				dirs [i] = Path.GetFullPath (dirs [i]);
				if (dirs [i] [dirs [i].Length - 1] == Path.DirectorySeparatorChar)
					dirs [i] = dirs [i].Substring (0, dirs [i].Length - 1);
			}

			var files = new List<List<FileInfo>> ();
			for (int i = 0; i < dirs.Count; i++) {
				var dir = new DirectoryInfo (dirs [i]);
				var list = new List<FileInfo> ();
				list.AddRange (dir.GetFiles ("*", SearchOption.AllDirectories));
				files.Add (list);
			}

			var allFiles = new HashSet<string> ();
			for (int i = 0; i < dirs.Count; i++)
				allFiles.UnionWith (files [i].Select ((v) => v.FullName.Substring (dirs [i].Length + 1)));


			var befores = files [0];
			var afters = files [1];

			var lines = new List<string> ();
			var maxFileLength = allFiles.Max ((v) => v.Length);

			string line_format = "|{0,-" + maxFileLength + "} | {1,15} | {2,15} | {3,12} |";

			foreach (var file in allFiles.OrderBy ((v) => v)) {
				var before_name = Path.Combine (dirs [0], file);
				var after_name = Path.Combine (dirs [1], file);
				var before = befores.FirstOrDefault ((v) => v.FullName == before_name);
				var after = afters.FirstOrDefault ((v) => v.FullName == after_name);
				string line;

				if (after == null) {
					// file was deleted
					line = string.Format (line_format, file, FormatSize (before.Length), "*deleted*", FormatSize (-before.Length));
				} else if (before == null) {
					// new file
					line = string.Format (line_format, file, "*new*", FormatSize (after.Length), FormatSize (after.Length));
				} else {
					line = string.Format (line_format, file, FormatSize (before.Length), FormatSize (after.Length), FormatSize (after.Length - before.Length));
				}
				lines.Add (line);
			}

			Console.WriteLine ("## Directory size comparison");
			Console.WriteLine ();
			Console.WriteLine ("* Before: {0}", dirs [0]);
			Console.WriteLine ("* After: {0}", dirs [1]);
			Console.WriteLine ();
			Console.WriteLine (line_format, "Path", "Before", "After", "Diff");
			Console.WriteLine (line_format, ":" + new string ('-', maxFileLength - 1), "------:", "------:", "------:");
			lines.Sort ();
			for (int i = 0; i < lines.Count; i++)
				Console.WriteLine (lines [i]);

			var sumBefore = befores.Sum ((v) => v.Length);
			var sumAfter = afters.Sum ((v) => v.Length);
			Console.WriteLine (line_format, "**Total**", $"**{FormatSize (sumBefore)}**", $"**{FormatSize (sumAfter)}**", $"**{FormatSize (sumAfter - sumBefore)}**");

			return 0;
		} catch (Exception e) {
			Console.WriteLine (e);
			return 1;
		}
	}

	static string FormatSize (long length)
	{
		return length.ToString ("#,#");
	}
}
