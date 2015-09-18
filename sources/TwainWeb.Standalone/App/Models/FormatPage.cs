using System;

namespace TwainWeb.Standalone.App
{
    public class FormatPage
    {

        public FormatPage()
        {
        }
        public FormatPage(string query)
        {
            if (query != null)
            {
                float width;
                float height;
                var indexSeparator = query.IndexOf("*");
                if (indexSeparator == -1)
                    return;
                float.TryParse(query.Substring(0, indexSeparator), out width);
                float.TryParse(query.Substring(indexSeparator+1), out height);
                Height = height;
                Width = width;
            }
        }

        public float Width { get; set; }
        public float Height { get; set; }
        public string Name { get; set; }
        public bool EqualsWithBackslash(float width, float height, float backslash)
        {
            return Math.Abs(Width - width) <= backslash && Math.Abs(Height - height) <= backslash;
        }
    }
}
