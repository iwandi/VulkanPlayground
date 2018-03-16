using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulkan;
using Vulkan.Windows;

namespace VulkanPlayground
{
    public class VulkanInitUtility
    {
        public enum InitRequestType
        {
            Layer,
            InstanceExtention,
            DeviceExtention,
        }

        public struct InitRequest
        {
            public InitRequestType Type;
            public string Name;
            public bool Required;
        }

        public static InitRequest[] InitRequestCommonDebug
        {
            get
            {
                return new InitRequest[]{
                    new InitRequest{
                        Type = InitRequestType.Layer,
                        Name = "VK_LAYER_LUNARG_standard_validation",
                        Required = false,
                    },
                    new InitRequest{
                        Type = InitRequestType.InstanceExtention,
                        Name = "VK_KHR_surface",
                        Required = true,
                    },
                    new InitRequest{
                        Type = InitRequestType.InstanceExtention,
                        Name = "VK_KHR_win32_surface",
                        Required = true,
                    },
                    new InitRequest{
                        Type = InitRequestType.DeviceExtention,
                        Name = "VK_KHR_swapchain",
                        Required = true,
                    },
                };
            }
        }

        [System.Flags]
        public enum QueuRequestTypeFlags
        {
            Graphics = 1,
            Compute = 2,
            Transfer = 4,
            Sparse = 8
        }

        public struct QueuRequest
        {
            public QueuRequestTypeFlags TypeFlags;
            public int Count;
        }

        public static ApplicationInfo CreateApplicationInfo()
        {
            return CreateApplicationInfo(System.Reflection.Assembly.GetCallingAssembly().GetName(), System.Reflection.Assembly.GetEntryAssembly().GetName());
        }

        public static ApplicationInfo CreateApplicationInfo(System.Reflection.AssemblyName Application, System.Reflection.AssemblyName Engine)
        {
            ApplicationInfo appInfo = new ApplicationInfo
            {
                ApiVersion = Vulkan.Version.Make(1, 0, 0), // TODO find a better way to define vulkan version
                ApplicationName = Application.Name,
                ApplicationVersion = Application.Version.ToVulkanVersion(),
                EngineName = Engine.Name,
                EngineVersion = Engine.Version.ToVulkanVersion(),
            };
            return appInfo;
        }

        public static InstanceCreateInfo CreateInstanceCreateInfo(IEnumerable<InitRequest> initRequest = null)
        {
            ApplicationInfo appInfo = CreateApplicationInfo();
            return CreateInstanceCreateInfo(appInfo, initRequest);
        }

        public static InstanceCreateInfo CreateInstanceCreateInfo(ApplicationInfo appInfo, IEnumerable<InitRequest> initRequest = null)
        {
            string[] layers;
            string[] extentions;
            if (initRequest != null)
            {
                layers = GetInstanceLayers(initRequest);
                extentions = GetInstanceExtention(initRequest);
            }
            else
            {
                layers = new string[0];
                extentions = new string[0];
            }

            InstanceCreateInfo instanceCreateInfo = new InstanceCreateInfo
            {
                ApplicationInfo = appInfo,
                EnabledLayerNames = layers,
                EnabledExtensionNames = extentions,
            };
            return instanceCreateInfo;
        }        

        public static string[] GetInstanceLayers(IEnumerable<InitRequest> initRequest)
        {
            return GetLayers(Vulkan.Commands.EnumerateInstanceLayerProperties(), InitRequestType.Layer, initRequest);
        }

        public static string[] GetDeviceLayers(PhysicalDevice physicalDevice, IEnumerable<InitRequest> initRequest)
        {
            return GetLayers(physicalDevice.EnumerateDeviceLayerProperties(), InitRequestType.Layer, initRequest);
        }

        public static string[] GetLayers(LayerProperties[] properties, InitRequestType targetType, IEnumerable<InitRequest> initRequest)
        {
            List<string> list = new List<string>();
            foreach (InitRequest request in initRequest)
            {
                if (request.Type == targetType)
                {
                    bool present = false;
                    foreach (var propertie in properties)
                    {
                        if (propertie.LayerName == request.Name)
                        {
                            present = true;
                            break;
                        }
                    }
                    if (request.Required && !present)
                    {
                        throw new Exception(String.Format("Required Layer {0} not available but was marked as required", request.Name));
                    }
                    if (present)
                    {
                        list.Add(request.Name);
                    }
                }
            }
            return list.ToArray();
        }

        public static string[] GetInstanceExtention(IEnumerable<InitRequest> initRequest)
        {
            return GetExtention(Vulkan.Commands.EnumerateInstanceExtensionProperties(), InitRequestType.InstanceExtention, initRequest);
        }

        public static string[] GetDeviceExtention(PhysicalDevice physicalDevice, IEnumerable<InitRequest> initRequest)
        {
            return GetExtention(physicalDevice.EnumerateDeviceExtensionProperties(), InitRequestType.DeviceExtention, initRequest);
        }

        public static string[] GetExtention(ExtensionProperties[] properties, InitRequestType targetType, IEnumerable<InitRequest> initRequest)
        {
            List<string> list = new List<string>();
            foreach (InitRequest request in initRequest)
            {
                if (request.Type == targetType)
                {
                    bool present = false;
                    foreach (var propertie in properties)
                    {
                        if (propertie.ExtensionName == request.Name)
                        {
                            present = true;
                            break;
                        }
                    }
                    if (request.Required && !present)
                    {
                        throw new Exception(String.Format("Required Extention {0} not available but was marked as required", request.Name));
                    }
                    if (present)
                    {
                        list.Add(request.Name);
                    }
                }
            }
            return list.ToArray();
        }

        public static Instance CreateInstance(bool includeCommonDebug = false)
        {
            IEnumerable<InitRequest> initRequest = null;
            if (includeCommonDebug)
            {
                initRequest = InitRequestCommonDebug;
            }

            InstanceCreateInfo instanceCreateInfo = CreateInstanceCreateInfo(initRequest);
            return new Instance(instanceCreateInfo);
        }

        public static DeviceCreateInfo CreateDeviceCreateInfo(PhysicalDevice physicalDevice, bool createDefaultQueue, IEnumerable<InitRequest> initRequest = null)
        {
            string[] layers;
            string[] extentions;
            if (initRequest != null)
            {
                layers = GetDeviceExtention(physicalDevice, initRequest);
                extentions = GetDeviceExtention(physicalDevice, initRequest);
            }
            else
            {
                layers = new string[0];
                extentions = new string[0];
            }

            List<DeviceQueueCreateInfo> queus = new List<DeviceQueueCreateInfo>();
            if (createDefaultQueue)
            {
                uint selectedQueue;
                // TODO : this needs to be a single step in order to correctly partition
                // TODO : on default we should look for a present queue
                if (VulkanInitUtility.TrySelectQueue(physicalDevice, QueueFlags.Compute | QueueFlags.Graphics | QueueFlags.Transfer, true, out selectedQueue))
                {
                    queus.Add(new DeviceQueueCreateInfo
                    {
                        QueueCount = 1,
                        QueueFamilyIndex = selectedQueue,
                        QueuePriorities = new float[] { 1.0f },
                    });
                };
            }

            DeviceCreateInfo deviceCreateInfo = new DeviceCreateInfo
            {
                EnabledLayerNames = layers,
                EnabledExtensionNames = extentions,
                QueueCreateInfos = queus.ToArray(),
            };
            return deviceCreateInfo;
        }

        public static PhysicalDevice SelectPhysicalDevice(Instance instance)
        {
            foreach (PhysicalDevice physicalDevice in instance.EnumeratePhysicalDevices())
            {
                // TODO : Select Best GPU via VkPhysicalDeviceType 
                return physicalDevice;
            }
            return null;
        }

        public static Device CreateDevice(PhysicalDevice physicalDevice, bool createDefaultQueue = true, bool includeCommonDebug = false)
        {
            IEnumerable<InitRequest> initRequest = null;
            if (includeCommonDebug)
            {
                initRequest = InitRequestCommonDebug;
            }

            DeviceCreateInfo deviceCreateInfo = VulkanInitUtility.CreateDeviceCreateInfo(physicalDevice, createDefaultQueue, initRequest);
            return physicalDevice.CreateDevice(deviceCreateInfo);
        }

        static PhysicalDevice QueueFamilyPropertiesCachePhysicalDevice;
        static QueueFamilyProperties[] QueueFamilyPropertiesCache;

        public static bool TrySelectQueue(PhysicalDevice physicalDevice, QueueFlags flags, bool present, out uint selectedIndex)
        {
            if (QueueFamilyPropertiesCachePhysicalDevice != physicalDevice)
            {
                QueueFamilyPropertiesCachePhysicalDevice = physicalDevice;
                QueueFamilyPropertiesCache = physicalDevice.GetQueueFamilyProperties();
            }

            selectedIndex = 0;
            int score = 0;
            uint i = 0;
            foreach (QueueFamilyProperties test in QueueFamilyPropertiesCache)
            {
                /*foreach(SurfaceFormatKhr format in physicalDevice.GetSurfaceFormatsKHR())
                {

                }
                physicalDevice.GetSurfaceSupportKHR(i);*/

                int checkScore = ScoreQueue(test, flags);
                if(checkScore > score)
                {
                    selectedIndex = i;
                    score = checkScore;
                }
                i++;
            }
            return score > 0;
        }

        static int ScoreQueue(QueueFamilyProperties queueFamilyProperties, QueueFlags flags)
        {
            // Check if all flags are pressent
            if ((queueFamilyProperties.QueueFlags & flags) == flags)
            {
                int score = 256; // base score this was we can remove a lot and are still over 0

                int targetFlagCount = CountFlags((int)flags);
                int flagsCount = CountFlags((int)queueFamilyProperties.QueueFlags);

                score -= flagsCount - targetFlagCount * 16; // remove 16 for for overmaching the falgs
                score += (int)queueFamilyProperties.QueueCount; // add score for QueueCount;
                
                if(score <= 0)
                {
                    // always return minimum of 1 if it maches the flags
                    return 1;
                }
                return score;
            }
            return 0;
        }

        static int CountFlags(int v)
        {
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            int c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        public static void ClearCaches()
        {
            QueueFamilyPropertiesCachePhysicalDevice = null;
            QueueFamilyPropertiesCache = null;
        }

        public static SurfaceKhr CreateSurface(Instance instance, System.Windows.Forms.Form window)
        {
            Win32SurfaceCreateInfoKhr surfaceCreateInfo = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = System.Diagnostics.Process.GetCurrentProcess().Handle,
                Hwnd = window.Handle,
            };
            return instance.CreateWin32SurfaceKHR(surfaceCreateInfo);
        }

        // TODO : move to MathF class
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static uint Clamp(uint value, uint min, uint max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static bool CheckSwapchainCreateInfo(PhysicalDevice physicalDevice, SwapchainCreateInfoKhr createInfo, bool fixExtend)
        {
            SurfaceKhr surface = createInfo.Surface;
            foreach (uint index in createInfo.QueueFamilyIndices)
            {
                if (!physicalDevice.GetSurfaceSupportKHR(index, surface))
                {
                    return false;
                }
            }

            bool supportImageFormat = false;
            foreach (SurfaceFormatKhr suportedFormat in physicalDevice.GetSurfaceFormatsKHR(surface))
            {
                if (suportedFormat.Format == createInfo.ImageFormat &&
                    suportedFormat.ColorSpace == createInfo.ImageColorSpace)
                {
                    supportImageFormat = true;
                    break;
                }
            }
            if (!supportImageFormat)
            {
                return false;
            }

            SurfaceCapabilitiesKhr capabilities = physicalDevice.GetSurfaceCapabilitiesKHR(surface);
            if(fixExtend)
            {
                var extend = createInfo.ImageExtent;
                extend.Width = Clamp(extend.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
                extend.Height = Clamp(extend.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);
                createInfo.ImageExtent = extend;
            }
            if(createInfo.PreTransform == SurfaceTransformFlagsKhr.Inherit)
            {
                createInfo.PreTransform = capabilities.CurrentTransform;
            }
            // TODO: Fix up CompositeAlpha if Inherit is set

            if (capabilities.MinImageCount <= createInfo.MinImageCount &&
                capabilities.MaxImageCount >= createInfo.MinImageCount &&
                capabilities.MaxImageArrayLayers <= createInfo.ImageArrayLayers &&
                ((capabilities.SupportedTransforms & createInfo.PreTransform ) == createInfo.PreTransform) &&
                ((capabilities.SupportedCompositeAlpha & createInfo.CompositeAlpha) == createInfo.CompositeAlpha) &&
                ((capabilities.SupportedUsageFlags & createInfo.ImageUsage) == createInfo.ImageUsage) &&
                createInfo.ImageExtent.Width >= capabilities.MinImageExtent.Width &&
                createInfo.ImageExtent.Width <= capabilities.MaxImageExtent.Width &&
                createInfo.ImageExtent.Height >= capabilities.MinImageExtent.Height &&
                createInfo.ImageExtent.Height <= capabilities.MaxImageExtent.Height)
            {
                return true;
            }
            return false;
        }

        public static bool TryCreateSwapChain(Instance instance, PhysicalDevice physicalDevice, Device device,
            SurfaceKhr surface, uint queue, ref System.Drawing.Size size,
            Format format, ColorSpaceKhr colorSpace, PresentModeKhr presentMode,
            ref SwapchainKhr swapchain)
        {   
            SwapchainCreateInfoKhr swapChainCreateInfo = new SwapchainCreateInfoKhr
            {
                Surface = surface,
                MinImageCount = 2,
                ImageFormat = format,
                ImageColorSpace = colorSpace,
                ImageExtent = ToExtent2D(size),
                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                ImageSharingMode = SharingMode.Exclusive,
                QueueFamilyIndices = new uint[] { queue },
                PreTransform = SurfaceTransformFlagsKhr.Inherit,
                CompositeAlpha = CompositeAlphaFlagsKhr.Opaque, // TODO : set this to Ingerit if it can be fixed in Check
                PresentMode = presentMode,
                Clipped = false,
                OldSwapchain = swapchain,
            };

            if(!CheckSwapchainCreateInfo(physicalDevice, swapChainCreateInfo, true))
            {
                return false;
            }

            size.Width = (int)swapChainCreateInfo.ImageExtent.Width;
            size.Height = (int)swapChainCreateInfo.ImageExtent.Height;

            SwapchainKhr newSwapchain = device.CreateSwapchainKHR(swapChainCreateInfo);
            if (newSwapchain != null)
            {
                swapchain = newSwapchain;
                return true;
            }
            return false;
        }

        public static Extent2D ToExtent2D(System.Drawing.Size size)
        {
            Extent2D extent = new Extent2D();
            extent.Width = (uint)size.Width;
            extent.Height = (uint)size.Height;
            return extent;
        }
    }
}
