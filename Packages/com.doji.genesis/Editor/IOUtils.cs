using System.IO;

namespace Genesis.Editor {
    internal class IOUtils {

        private static readonly string StagingAreaFolder = @"Assets\Genesis Assets\";
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