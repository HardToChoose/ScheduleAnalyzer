using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace UI.Helpers
{
    internal struct DWMCOLORIZATIONPARAMS 
    { 
        public UInt32 ColorizationColor; 
        public UInt32 ColorizationAfterglow; 
        public UInt32 ColorizationColorBalance; 
        public UInt32 ColorizationAfterglowBalance; 
        public UInt32 ColorizationBlurBalance; 
        public UInt32 ColorizationGlassReflectionIntensity; 
        public UInt32 ColorizationOpaqueBlend; 
    }

    public class RealColors
    {
        [DllImport("dwmapi.dll", EntryPoint="#127")] 
        private static extern void DwmGetColorizationParameters(ref DWMCOLORIZATIONPARAMS dp);

        private static byte Mix(byte channel_1, byte channel_2, double ratio)
        {
            return (byte)(channel_1 + (channel_2 - channel_1) * ratio / 100);
        }

        private static Color GetWindowBorderColor()
        {
            var param = new DWMCOLORIZATIONPARAMS();
            DwmGetColorizationParameters(ref param);
            var ratio = 100.0 - param.ColorizationColorBalance;

            Color glass = new Color
            {
                R = (byte)(param.ColorizationColor >> 16),
                G = (byte)(param.ColorizationColor >> 8),
                B = (byte) param.ColorizationColor
            };
            
            return new Color
            {
                A = 255,
                R = Mix(glass.R, 217, ratio),
                G = Mix(glass.G, 217, ratio),
                B = Mix(glass.B, 217, ratio)
            };
        }

        public static Color WindowCaption
        {
            get { return GetWindowBorderColor(); }
        }
    }
}
