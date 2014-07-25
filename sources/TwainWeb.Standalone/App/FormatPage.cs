using System;
using System.Collections.Generic;
using System.Text;

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
                this.Height = height;
                this.Width = width;
            }
        }

        public float Width { get; set; }
        public float Height { get; set; }
        public string Name { get; set; }
        public bool EqualsWithBackslash(float width, float height, float backslash)
        {
            return Math.Abs(this.Width - width) <= backslash && Math.Abs(this.Height - height) <= backslash;
        }
    }
}
