using System.Windows;

namespace UI.Helpers
{
    public static class Extensions
    {
        public static void PlaceFloatingWindow(this Window parent, Window window, PlaceInfo info)
        {
            if (info.AttachLeft)    window.Left = parent.Left;
            if (info.AttachRight)   window.Left = parent.Left + parent.ActualWidth - window.ActualWidth;

            if (info.AttachTop)     window.Top = parent.Top;
            if (info.AttachBottom)  window.Top = parent.Top + parent.ActualHeight - window.ActualHeight;

            switch (info.AttachTo)
            {
                case AttachPosition.Left:
                case AttachPosition.Right:
                    window.Top = parent.Top + (parent.ActualHeight - window.ActualHeight) / 2;
                    break;

                case AttachPosition.Top:
                case AttachPosition.Bottom:
                    window.Left = parent.Left + (parent.ActualWidth - window.ActualWidth) / 2;
                    break;
            }

            if (info.OutsideHorizontal && info.AttachLeft)    window.Left -= window.ActualWidth;
            if (info.OutsideHorizontal && info.AttachRight) window.Left += window.ActualWidth;

            if (info.OutsideVertical && info.AttachTop) window.Top -= window.ActualHeight;
            if (info.OutsideVertical && info.AttachBottom) window.Top += window.ActualHeight;

            if (info.SkipParentBorder)
            {
                if (!info.OutsideHorizontal)
                {
                    if (info.AttachLeft)    window.Left += SystemParameters.ResizeFrameVerticalBorderWidth;
                    if (info.AttachRight)   window.Left -= SystemParameters.ResizeFrameVerticalBorderWidth;
                }

                if (!info.OutsideVertical)
                {
                    if (info.AttachTop)     window.Top += SystemParameters.ResizeFrameHorizontalBorderHeight + SystemParameters.WindowCaptionHeight;
                    if (info.AttachBottom)  window.Top -= SystemParameters.ResizeFrameHorizontalBorderHeight + SystemParameters.WindowCaptionHeight;
                }
            }

            window.Left += info.OffsetX;
            window.Top += info.OffsetY;

            var screenWidth = CurrentScreen.Width;
            var screenHeight = CurrentScreen.Height;

            /* Ensure the window was placed on the visible part of the screen */
            if (window.Left < 0)
                window.Left = 0;
            if (window.Left + window.ActualWidth > screenWidth)
                window.Left = screenWidth - window.ActualWidth;

            if (window.Top < 0)
                window.Top = 0;
            if (window.Top + window.ActualHeight > screenHeight)
                window.Top = screenHeight - window.ActualHeight;
        }
    }
}
