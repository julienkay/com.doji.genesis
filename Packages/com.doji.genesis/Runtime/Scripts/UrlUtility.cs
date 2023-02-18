namespace Genesis.Utilities {
    /// <summary>
    /// Small helper functions when implementing a framework in Unity that
    /// helps the developer access a web API using HTTP Requests and such.
    /// </summary>
    public static class UrlUtility {

        /// <summary>
        /// Appends <paramref name="appendix"/> to <paramref name="url"/>.
        /// </summary>
        public static void AppendQuerytoUrl(ref string url, string parameterName, string query) {
            if (query == null) {
                return;
            }
            url += parameterName + "=" + query + "&";
        }

        /// <summary>
        /// Appends <paramref name="appendix"/> to <paramref name="url"/>.
        /// If the value that is passed in is null, it will not be appended.
        /// </summary>
        public static void AppendQuerytoUrl<T>(ref string url, string parameterName, T? query) where T : struct {
            if (!query.HasValue || query == null) {
                return;
            }
            url += parameterName + "=" + query.Value.ToString() + "&";
        }

        public static void AppendQuerytoUrl<T>(ref string url, string parameterName, T query) where T : struct {
            url += parameterName + "=" + query.ToString() + "&";
        }

        /*
        public static void AppendQuerytoUrl(ref string url, string parameterName, int? query) {
            if (!query.HasValue || query == null) {
                return;
            }
            url += parameterName + "=" + query.Value.ToString() + "&";
        }

        public static void AppendQuerytoUrl(ref string url, string parameterName, long? query) {
            if (!query.HasValue || query == null) {
                return;
            }
            url += parameterName + "=" + query.Value.ToString() + "&";
        }*/
    }
}