using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vulkan;

namespace VulkanPlayground
{
    public class VulkanInitUtility
    {
        public enum InitRequestType
        {
            Layer,
            Extention,
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
            if(initRequest != null)
            {
                layers = GetLayers(initRequest);
                extentions = GetExtentions(initRequest);
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
        
        public static string[] GetLayers(IEnumerable<InitRequest> initRequest)
        {
            return GetLayers(initRequest, Vulkan.Commands.EnumerateInstanceLayerProperties());
        }

        public static string[] GetLayers(IEnumerable<InitRequest> initRequest, LayerProperties[] properties)
        {
            List<string> list = new List<string>();
            foreach (InitRequest request in initRequest)
            {
                if (request.Type == InitRequestType.Layer)
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

        public static string[] GetExtentions(IEnumerable<InitRequest> initRequest)
        {
            return GetExtentions(initRequest, Vulkan.Commands.EnumerateInstanceExtensionProperties());
        }

        public static string[] GetExtentions(IEnumerable<InitRequest> initRequest, ExtensionProperties[] properties)
        {
            List<string> list = new List<string>();
            foreach (InitRequest request in initRequest)
            {
                if (request.Type == InitRequestType.Extention)
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
            if(includeCommonDebug)
            {
                initRequest = InitRequestCommonDebug;
            }

            InstanceCreateInfo instanceCreateInfo = CreateInstanceCreateInfo(initRequest);
            return new Instance(instanceCreateInfo);
        }

        public static DeviceCreateInfo CreateDeviceCreateInfo(IEnumerable<InitRequest> initRequest = null)
        {
            string[] layers;
            string[] extentions;
            if (initRequest != null)
            {
                layers = GetLayers(initRequest);
                extentions = GetExtentions(initRequest);
            }
            else
            {
                layers = new string[0];
                extentions = new string[0];
            }

            DeviceCreateInfo deviceCreateInfo = new DeviceCreateInfo
            {
                EnabledLayerNames = layers,
                EnabledExtensionNames = extentions,
                
            };
            return deviceCreateInfo;
        }

        /*public static Device CreateInstanceAndDevice(out Instance instance, bool includeCommonDebug = false)
        {
            instance = CreateInstance(includeCommonDebug);
            return CreateDevice(instance, includeCommonDebug);
        }*/


        // TODO, Requires PhysicalDevice Selection
        // TODO, Requires Device features Selection
        // TODO, Requires Quere Selection
        /*public static Device CreateDevice(Instance instance, bool includeCommonDebug = false)
        {
            PhysicalDevice[] physicals = instance.EnumeratePhysicalDevices();
            foreach(PhysicalDevice physical in physicals)
            {
                physical.
            }


            IEnumerable<InitRequest> initRequest = null;
            if (includeCommonDebug)
            {
                initRequest = InitRequestCommonDebug;
            }
            DeviceCreateInfo deviceCreateInfo = CreateDeviceCreateInfo(initRequest);

        }*/
    }
}
