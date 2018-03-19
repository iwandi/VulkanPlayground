using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPI;

namespace GPI.Vulkan
{
    public class VulkanGPIProvider : IGPIProvider
    {
        public string Name { get { return "Vulkan"; } }

        // TODO : do a check if vulkan is supported
        public bool IsSupported { get { return true; } }

        internal static VulkanGPIPInstance Instance;

        public IGPIInstance CreateInstance()
        {
            if(Instance == null)
            {
                Instance = new VulkanGPIPInstance();
            }
            return Instance;
        }
    }

    public class VulkanGPIPInstance : IGPIInstance
    {
        public bool IsSupported(DeviceLayout layout)
        {
            return true;
        }

        public IGPIDevice CreateDevice(DeviceLayout layout)
        {
            return new VulkanGPIDevice();
        }
    }

    public class VulkanGPIDevice : IGPIDevice
    {
        public IGPIFrame BeginFrame(bool blocking = true)
        {
            return new VulkanGPIFrame();
        }

        public IGPICommandBuffer CreateCommandBuffer()
        {
            return new VulkanGPICommandBuffer();
        }

        public void EndFrame(IGPIFrame frame, bool blocking = true)
        {

        }
    }

    public class VulkanGPIFrame : IGPIFrame
    {

    }

    public class VulkanGPICommandBuffer : IGPICommandBuffer
    {
        public void Clear(IGPIFrame frame, Color color)
        {

        }

        public void Reset()
        {

        }

        public void Submit()
        {

        }
    }
}
