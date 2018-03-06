using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulkan;

namespace VulkanPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            App app = new App();
            app.Run();
        }

        class App
        {
            Instance instance;
            PhysicalDevice physicalDevice;
            Device device;

            public App()
            {
                InitVulkanInstance();


            }

            public void Run()
            {

            }

            void InitVulkanInstance()
            {
                instance = VulkanInitUtility.CreateInstance(true);
            }

            void InitVulkanDevice()
            {
                List<DeviceQueueCreateInfo> queus = new List<DeviceQueueCreateInfo>();
                

                DeviceCreateInfo deviceCreateInfo = VulkanInitUtility.CreateDeviceCreateInfo(VulkanInitUtility.InitRequestCommonDebug);
                deviceCreateInfo.QueueCreateInfos = queus.ToArray();
                
                foreach(PhysicalDevice avialablePhysicalDevice in instance.EnumeratePhysicalDevices())
                {

                }

                device = physicalDevice.CreateDevice(deviceCreateInfo);
            }
        }        
    }
}
