using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GPI
{
    //Graphics Processing Interface
    public interface IGPIProvider
    {
        string Name { get; }
        bool IsSupported { get; }
        IGPIInstance CreateInstance();
    }

    public interface IGPIInstance
    {
        bool IsSupported(DeviceLayout layout);
        IGPIDevice CreateDevice(DeviceLayout layout);
    }

    public struct DeviceLayout
    {
        public static DeviceLayout SimpleFormawad = new DeviceLayout
        {

        };

        public static DeviceLayout SimpleDeferred = new DeviceLayout
        {

        };
    }

    public struct SwapChainLayout
    {
        public FullscreenMode FullscreenMode;
        public int Screen;
        public VSyncMode VSyncMode;
        public float VSyncRate;

        public Lazy<SwapChainLayout> Fallback;
    }

    public enum FullscreenMode
    {
        Fullscreen,
        WindowedFullscreen,
        Windowed,
    }

    public enum VSyncMode
    {
        Off,
        DoubleBuffer,
        TrippleBuffer,
    }

    public interface IGPIDevice
    {
        IGPICommandBuffer CreateCommandBuffer();

        IGPIFrame BeginFrame(bool blocking = true);

        void EndFrame(IGPIFrame frame, bool blocking = true);
    }

    public interface IGPICommandBuffer
    {
        void Reset();

        void Clear(IGPIFrame frame, Color color);

        void Submit();
    }

    public interface IGPIFrame
    {

    }

    public struct Color
    {
        public static Color White = new Color();
        public static Color Black = new Color();
        public static Color Green = new Color();
        public static Color Blue = new Color();
        public static Color Red = new Color();

        public float R;
        public float G;
        public float B;
        public float A;
    }
}
