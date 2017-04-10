namespace CondenserDotNet.Core
{
    public static class ServiceUtils
    {
        private const string UrlPrefix = "urlprefix-";

        public static string[] RoutesFromTags(string[] tags)
        {
            var returnCount = 0;
            for (var i = 0; i < tags.Length; i++)
            {
                if (!tags[i].StartsWith(UrlPrefix))
                {
                    continue;
                }
                returnCount++;
            }
            var returnValues = new string[returnCount];
            returnCount = 0;
            for (var i = 0; i < tags.Length; i++)
            {
                if (!tags[i].StartsWith(UrlPrefix))
                {
                    continue;
                }
                var startSubstIndex = UrlPrefix.Length;
                var endSubstIndex = tags[i].Length - UrlPrefix.Length;
                if (tags[i][tags[i].Length - 1] == '/')
                {
                    endSubstIndex--;
                }
                returnValues[returnCount] = tags[i].Substring(startSubstIndex, endSubstIndex);
                if (returnValues[returnCount][0] != '/')
                {
                    returnValues[returnCount] = "/" + returnValues[returnCount];
                }
                returnCount++;
            }
            return returnValues;
        }
    }
}
