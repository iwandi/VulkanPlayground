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
            Semaphore swapChainSemphore;

            Stack<Action> cleanupStack = new Stack<Action>();

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
                Console.WriteLine("Cleanup start");
                while(cleanupStack.Count > 0)
                {
                    cleanupStack.Pop()();
                }
                Console.WriteLine("Cleanup done");
                Console.ReadLine();
            }

            public void Run()
            {
                while (!window.IsDisposed)
                {
                    Application.DoEvents();

                    uint imageIndex = device.AcquireNextImageKHR(swapChain, uint.MaxValue, swapChainSemphore);
                    
                    mainCmd.Begin(new CommandBufferBeginInfo { });
                    /*mainCmd.CmdClearColorImage(null, 
                        Vulkan.ImageLayout.ColorAttachmentOptimal, 
                        new ClearColorValue(new float[] { 1.0f, 0.5f, 0.0f, 1.0f }), 
                        new ImageSubresourceRange[] {});*/
                    mainCmd.End();
                    mainQueue.Submit(new SubmitInfo
                    {
                        CommandBuffers = new CommandBuffer[] { mainCmd },
                    });
                    mainQueue.PresentKHR(new PresentInfoKhr
                    {
                        Swapchains = new SwapchainKhr[] { swapChain },
                        ImageIndices = new uint[] { imageIndex },
                        WaitSemaphores =  new Semaphore[] { swapChainSemphore },
                    });

                    System.Threading.Thread.Sleep(10);
                }
            }

            void InitVulkanInstance()
            {
                instance = VulkanInitUtility.CreateInstance(true);

                cleanupStack.Push(() => {
                    instance.Destroy();
                    instance = null;
                });
            }

            void InitVulkanDevice()
            {
                physicalDevice = VulkanInitUtility.SelectPhysicalDevice(instance);
                device = VulkanInitUtility.CreateDevice(physicalDevice, true, true);

                mainQueue = device.GetQueue(0, 0);

                cleanupStack.Push(() => {
                    mainQueue = null;

                    device.Destroy();
                    device = null;

                    physicalDevice = null;
                });
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

                cleanupStack.Push(() => {
                    device.FreeCommandBuffer(mainCmdPool, mainCmd);
                    mainCmd = null;
                    device.DestroyCommandPool(mainCmdPool);
                    mainCmdPool = null;
                });
            }
            
            void InitWindow()
            {
                window = new Form();
                window.Show();
                window.Size = new System.Drawing.Size(1280, 720);

                cleanupStack.Push(() => {
                    if (!window.IsDisposed)
                    {
                        window.Close();
                    }
                    window = null;
                });
            }
            
            void InitSwapChain()
            {
                surface = VulkanInitUtility.CreateSurface(instance, window);

                // TODO : try to use a linear format
                // TODO : dount just guest the queue
                Format format = Format.B8G8R8A8Unorm;
                System.Drawing.Size size = window.Size;
                if (!VulkanInitUtility.TryCreateSwapChain(instance, physicalDevice, device, surface, (uint)0, ref size,
                    format, ColorSpaceKhr.SrgbNonlinear, PresentModeKhr.Fifo, ref swapChain))
                {
                    throw new Exception("Failed to create swap chain");
                }

                swapChainSemphore = device.CreateSemaphore(new SemaphoreCreateInfo { });

                // create Images and Image Views

                Image[] images = device.GetSwapchainImagesKHR(swapChain);
                ImageView[] views = new ImageView[images.Length];

                int i = 0;
                foreach(Image image in images)
                {
                    ImageView view = device.CreateImageView(new ImageViewCreateInfo
                    {
                        Image = image,
                        Format = format,
                        ViewType = ImageViewType.View2D,
                        Components = new ComponentMapping
                        {
                            R = ComponentSwizzle.Identity,
                            G = ComponentSwizzle.Identity,
                            B = ComponentSwizzle.Identity,
                            A = ComponentSwizzle.Identity,
                        },
                        SubresourceRange = new ImageSubresourceRange
                        {
                            AspectMask = ImageAspectFlags.Color,
                            LayerCount = 1,
                            LevelCount = 1,
                            BaseMipLevel = 0,
                            BaseArrayLayer = 0,
                        },
                    });

                    views[i] = view;
                    i++;
                }

                // create a render pass for presentation

                RenderPass renderPass = device.CreateRenderPass(new RenderPassCreateInfo
                {
                    Attachments = new AttachmentDescription[]
                    {
                        new AttachmentDescription
                        {
                            Format = format,
                            Samples = SampleCountFlags.Count1,
                            LoadOp = AttachmentLoadOp.Clear,
                            StoreOp = AttachmentStoreOp.Store,
                            StencilLoadOp = AttachmentLoadOp.DontCare,
                            StencilStoreOp = AttachmentStoreOp.DontCare,
                            InitialLayout = Vulkan.ImageLayout.Undefined,
                            FinalLayout = Vulkan.ImageLayout.PresentSrcKhr,
                        }
                    },
                    Subpasses = new SubpassDescription[]
                    {
                        new SubpassDescription
                        {
                            PipelineBindPoint = PipelineBindPoint.Graphics,
                            ColorAttachments = new AttachmentReference[]
                            {
                                new AttachmentReference
                                {
                                    Attachment = 0u,
                                    Layout = Vulkan.ImageLayout.ColorAttachmentOptimal,
                                }
                            },                            
                        }
                    }                
                });

                /*GraphicsPipelineCreateInfo gpCreateInfo = new GraphicsPipelineCreateInfo
                {
                    Stages = new PipelineShaderStageCreateInfo[]
                    {
                        new PipelineShaderStageCreateInfo
                        {
                            
                        },
                        new PipelineShaderStageCreateInfo
                        {

                        }
                    }
                };

                PipelineLayout pipelineLayout = device.CreatePipelineLayout(new PipelineLayoutCreateInfo
                {
                    
                });*/

                // Create the framebuffers

                foreach(ImageView view in views)
                {
                    device.CreateFramebuffer(new FramebufferCreateInfo
                    {
                        Attachments = new ImageView[] { view },
                        Width = (uint)size.Width,
                        Height = (uint)size.Height,
                        Layers = 1,
                        RenderPass = renderPass,
                    });
                }

                //uint imageIndex = device.AcquireNextImageKHR(swapChain, uint.MaxValue, swapChainSemphore);

                // TODO : change image layout 

                cleanupStack.Push(() => {
                    foreach(ImageView imageView in views)
                    {
                        device.DestroyImageView(imageView);
                    }

                    //device.DestroyPipelineLayout(pipelineLayout);
                    //pipelineLayout = null;

                    device.DestroyRenderPass(renderPass);
                    renderPass = null;

                    device.DestroySemaphore(swapChainSemphore);
                    swapChainSemphore = null;

                    device.DestroySwapchainKHR(swapChain);
                    swapChain = null;

                    instance.DestroySurfaceKHR(surface);
                    surface = null;
                });
            }            
        }        
    }
}
