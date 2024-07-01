using System.IO;
namespace DNPL.Core;
public static class PathMaker {
	public static string GetAbsolutePath(string _relativePath) {
		// Get the base directory (where the .exe is running from)
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		// Define your relative path
		string relativePath = _relativePath;
		// Combine the base directory with the relative path to get an absolute path
		string absolutePath = Path.Combine(baseDirectory, relativePath);
		// Normalize the path (remove any "." or ".." segments)
		absolutePath = Path.GetFullPath(absolutePath);
		return absolutePath;
	}
}
