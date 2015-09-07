using System.Windows.Media;

namespace UI.Visualization
{
    class LabelInfo
    {
        public string Text;
        public double Padding;

        public double FontSize;
        public FontFamily FontFamily;

        public LabelInfo(string text, double padding, double fontSize = 12, FontFamily fontFamily = null)
        {
            Text = text;
            Padding = padding;

            FontSize = fontSize;
            FontFamily = fontFamily;
        }
    }
}
