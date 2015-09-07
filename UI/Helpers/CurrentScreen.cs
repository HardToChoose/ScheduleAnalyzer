using System.Windows;
using System.Windows.Interop;

using Forms = System.Windows.Forms;

namespace UI.Helpers
{
    public static class CurrentScreen
    {
        private static Forms.Screen WindowsFormsScreen
        {
            get
            {
                return Forms.Screen.FromHandle(new WindowInteropHelper(Application.Current.MainWindow).Handle);
            }
        }

        public static double Width
        {
            get { return WindowsFormsScreen.Bounds.Width; }
        }

        public static double Height
        {
            get { return WindowsFormsScreen.Bounds.Height; }
        }
    }
}
