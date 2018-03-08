using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Vulkan;
using Vulkan.Windows;

namespace VulkanPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            App app = new App();
            app.Run();
        }

        // TODO : error reporting
        class App
        {
            Instance instance;
            PhysicalDevice physicalDevice;
            Device device;

            Queue mainQueue;
            CommandPool mainCmdPool;
            // TODO use a main CommandBuffer for each Frame 
            CommandBuffer mainCmd;

            Form window;

            SurfaceKhr surface;
            SwapchainKhr swapChain;

            public App()
            {
                Console.WriteLine("Init start");
                Console.WriteLine(" . Init Instance");
                InitVulkanInstance();
                Console.WriteLine(" . Init Device");
                InitVulkanDevice();
                Console.WriteLine(" . Init Command Buffers");
                InitCommandBuffers();
                Console.WriteLine(" . Init Window");
                InitWindow();
                Console.WriteLine(" . Init SwapChain");
                InitSwapChain();
                Console.WriteLine("Init done");
            }

            ~App()
            {
                // TODO : create a eay system that alows cleanup in reverse order
                Console.WriteLine("Cleanup start");
                if (swapChain != null)
                {
                    device.DestroySwapchainKHR(swapChain);
                    swapChain = null;
                }
                if(surface != null)
                {
                    instance.DestroySurfaceKHR(surface);
                    surface = null;
                }
                if(window != null)
                {
                    if (!window.IsDisposed)
                    {
                        window.Close();
                    }
                    window = null;
                }
                if(mainCmd != null)
                {
                    device.FreeCommandBuffer(mainCmdPool, mainCmd);
                    mainCmd = null;
                }
                if(mainCmdPool != null)
                {
                    device.DestroyCommandPool(mainCmdPool);
                    mainCmdPool = null;
                }
                if(mainQueue != null)
                {
                    mainQueue = null;
                }
                if (device != null)
                {
                    device.Destroy();
                    device = null;
                }
                physicalDevice = null;
                if (instance != null)
                {
                    instance.Destroy();
                    instance = null;
                }
                Console.WriteLine("Cleanup done");
                Console.ReadLine();
            }

            public void Run()
            {
                while (!window.IsDisposed)
                {
                    Application.DoEvents();

                    uint imageIndex = device.AcquireNextImageKHR(swapChain, uint.MaxValue); // TODO : add locking system
                    
                    mainCmd.Begin(new CommandBufferBeginInfo { });
                    mainCmd.CmdClearColorImage(null, 
                        Vulkan.ImageLayout.ColorAttachmentOptimal, 
                        new ClearColorValue(new float[] { 1.0f, 0.5f, 0.0f, 1.0f }), 
                        new ImageSubresourceRange[] {});
                    mainCmd.End();
                    mainQueue.Submit(new SubmitInfo
                    {
                        CommandBuffers = new CommandBuffer[] { mainCmd },                        
                    });
                    mainQueue.PresentKHR(new PresentInfoKhr
                    {
                        // TODO : locking
                        Swapchains = new SwapchainKhr[] { swapChain },
                        ImageIndices = new uint[] { imageIndex },
                    });

                    System.Threading.Thread.Sleep(10);
                }
            }

            void InitVulkanInstance()
            {
                instance = VulkanInitUtility.CreateInstance(true);
            }

            void InitVulkanDevice()
            {
                physicalDevice = VulkanInitUtility.SelectPhysicalDevice(instance);
                device = VulkanInitUtility.CreateDevice(physicalDevice, true, true);

                mainQueue = device.GetQueue(0, 0);
            }   

            void InitCommandBuffers()
            {
                CommandPoolCreateInfo poolCreateInfo = new CommandPoolCreateInfo
                {
                    QueueFamilyIndex = 0,                    
                };

                mainCmdPool = device.CreateCommandPool(poolCreateInfo);

                CommandBufferAllocateInfo bufferAllocInfo = new CommandBufferAllocateInfo
                {
                    CommandBufferCount = 1,
                    CommandPool = mainCmdPool,
                    Level = CommandBufferLevel.Primary,
                };

                mainCmd = device.AllocateCommandBuffers(bufferAllocInfo)[0];
            }
            
            void InitWindow()
            {
                window = new Form();
                window.Show();
                window.Size = new System.Drawing.Size(1280, 720);
            }
            
            void InitSwapChain()
            {
                surface = VulkanInitUtility.CreateSurface(instance, window);

                // TODO : try to use a linear format
                // TODO : dount just guest the queue
                if(!VulkanInitUtility.TryCreateSwapChain(instance, physicalDevice, device, surface, (uint)0, window.Size,
                    Format.B8G8R8A8Unorm, ColorSpaceKhr.SrgbNonlinear, PresentModeKhr.Fifo, ref swapChain))
                {
                    throw new Exception("Failed to create swap chain");
                }
                                
                // TODO : create Images and Image Views
            }            
        }        
    }
}
