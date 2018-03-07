using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanPlayground.GraphicsHardware
{
    /*public struct DeviceLayout
    {
        public DeviceRequirment[] Requirments;

        public SurfaceLayout Surface;

        public QueuLayout[] Queu;
        public MemoryLayout[] Memory;
    }

    public enum RequirmentType
    {
        Layer,
        InstanceExtentsion,
        DeviceExtentsion,
        DeviceInfo,
        DeviceLimit,
        DeviceSparsePropertie,
        DeviceFeature,
        TextureFormat,
    }

    [Flags]
    public enum CheckFlags
    {
        Exists = 1,
        CheckMin = 2,
        CheckMax = 4,
        FlagsMatchAll = 8,
        FlagsMatchAny = 16,
    }

    public struct DeviceRequirment
    {
        public RequirmentType Type;
        public string Name;

        public CheckFlags CheckFlags;
        public float Min;
        public float Max;
        public int FlagsMatch;
    }

    [Flags]
    public enum QueuTypeFlags
    {
        Graphics = 1,
        Compute = 2,
        Transfer = 4,
        Sparse = 8
    }

    public struct QueuLayout
    {
        public QueuTypeFlags TypeFlags;
        public int MinCount;
        public int MaxCount;
    }

    [Flags]
    public enum MemoryTypeFlags
    {
        DeviceLocal = 1,
        HostVisible = 2,
        HostCoherent = 4,
        HostCached = 8,
        LazilyAllocated = 16,
    }

    public struct MemoryLayout
    {
        public MemoryTypeFlags TypeFlags;
        public long Size;
    }

    public enum PresentMode
    {
        Immediate,
        Fifo,
        FifoRelaxed,
        Mailbox,
    }

    public enum SurfaceFormat
    {
        B8G8R8A8Unorm,
        B8G8R8A8SRGB,
    }

    public enum SurfaceColorspace
    {
        SRGBNonLinear,
    }

    public struct SurfaceLayout
    {
        public PresentMode PresentMode;
        public SurfaceFormat SurfaceFormat;
        public int ImageCount;
        public int Width;
        public int Height;

    }

    public class GraphicsInitUtility
    {
        
    }

    public interface ITexture
    {

    }

    public interface IRenderTarget
    {

    }

    public interface IFence
    {

    }

    public interface ICommandBuffer
    {
        IFence CreateFence();

        void Copy(ITexture source, ITexture dest); // TODO add source Loca and Dest Loc

        void ClearRendertarget(IRenderTarget renderTarget, bool clearColor, Color color, bool clearDepth, double depth);

        void RunCompute();

        void Draw(); // TODO Params

        void ClearCommands();
    }*/
}
