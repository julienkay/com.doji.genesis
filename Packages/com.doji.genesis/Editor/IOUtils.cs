using System.IO;

namespace Genesis.Editor {
    internal class IOUtils {

        private static readonly string StagingAreaFolder = Path.Combine("Assets", "Genesis Assets");
        public static string StagingAreaPath {
            get {
                if (!Directory.Exists(StagingAreaFolder)) {
                    Directory.CreateDirectory(StagingAreaFolder);
                }
                return StagingAreaFolder;
            }
        }
    }
}