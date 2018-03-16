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
            using (App app = new App())
            {
                app.Run();
            }
            Console.ReadLine();
        }

        // TODO : error reporting
        class App : IDisposable
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
            Format surfaceFormat;
            System.Drawing.Size surfaceSize;
            SwapchainKhr swapChain;
            Semaphore swapChainSemphore;
            Image[] swapchainImages;
            ImageView[] swapchainImageViews;
            Framebuffer[] frameBuffers;

            RenderPass renderPass;

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
                Console.WriteLine(" . Init Rendering");
                InitRendering();
                Console.WriteLine(" . Init FrameBuffers");
                InitFrameBuffers();
                Console.WriteLine("Init done");
            }

            public void Dispose()
            {
                Console.WriteLine("Cleanup start");
                while (cleanupStack.Count > 0)
                {
                    cleanupStack.Pop()();
                }
                Console.WriteLine("Cleanup done");
            }

            public void Run()
            {
                while (!window.IsDisposed)
                {

                    uint imageIndex = device.AcquireNextImageKHR(swapChain, uint.MaxValue, swapChainSemphore);
                    Framebuffer frameBuffer = frameBuffers[imageIndex];

                    mainCmd.Begin(new CommandBufferBeginInfo{ });
                    mainCmd.CmdBeginRenderPass(new RenderPassBeginInfo
                    {
                        RenderPass = renderPass,
                        Framebuffer = frameBuffer,
                        RenderArea = new Rect2D
                        {
                            Offset = new Offset2D { X = 0, Y = 0 },
                            Extent = new Extent2D {  Width = (uint)surfaceSize.Width, Height = (uint)surfaceSize.Height }
                        },
                        ClearValues = new ClearValue[]
                        {
                            new ClearValue
                            {
                                Color = new ClearColorValue
                                {
                                    Float32 = new [] { 0.0f, 0.2f, 0.5f, 0.0f },
                                }
                            }
                        }
                    }, SubpassContents.Inline);
                    mainCmd.CmdEndRenderPass();
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
                    mainQueue.WaitIdle();

                    System.Threading.Thread.Sleep(10);
                    Application.DoEvents();
                }

                device.WaitIdle();
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
                    Flags = CommandPoolCreateFlags.ResetCommandBuffer,
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
                surfaceFormat = Format.B8G8R8A8Unorm;
                surfaceSize = window.Size;
                if (!VulkanInitUtility.TryCreateSwapChain(instance, physicalDevice, device, surface, (uint)0, ref surfaceSize,
                    surfaceFormat, ColorSpaceKhr.SrgbNonlinear, PresentModeKhr.Fifo, ref swapChain))
                {
                    throw new Exception("Failed to create swap chain");
                }

                swapChainSemphore = device.CreateSemaphore(new SemaphoreCreateInfo { });

                // create Images and Image Views

                swapchainImages = device.GetSwapchainImagesKHR(swapChain);
                swapchainImageViews = new ImageView[swapchainImages.Length];

                int i = 0;
                foreach(Image image in swapchainImages)
                {
                    ImageView view = device.CreateImageView(new ImageViewCreateInfo
                    {
                        Image = image,
                        Format = surfaceFormat,
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

                    swapchainImageViews[i] = view;
                    i++;
                }                

                cleanupStack.Push(() => {
                    foreach (ImageView imageView in swapchainImageViews)
                    {
                        device.DestroyImageView(imageView);
                    }

                    device.DestroySemaphore(swapChainSemphore);
                    swapChainSemphore = null;

                    device.DestroySwapchainKHR(swapChain);
                    swapChain = null;

                    instance.DestroySurfaceKHR(surface);
                    surface = null;
                });
            }            

            void InitRendering()
            {
                // create a render pass for presentation

                renderPass = device.CreateRenderPass(new RenderPassCreateInfo
                {
                    Attachments = new AttachmentDescription[]
                    {
                        new AttachmentDescription
                        {
                            Format = surfaceFormat,
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

                cleanupStack.Push(() => {
                    device.DestroyRenderPass(renderPass);
                    renderPass = null;
                });
            }

            void InitFrameBuffers()
            {
                // Create the framebuffers
                
                frameBuffers = new Framebuffer[swapchainImages.Length];
                int i = 0;
                foreach (ImageView view in swapchainImageViews)
                {
                    frameBuffers[i] = device.CreateFramebuffer(new FramebufferCreateInfo
                    {
                        Attachments = new ImageView[] { view },
                        Width = (uint)surfaceSize.Width,
                        Height = (uint)surfaceSize.Height,
                        Layers = 1,
                        RenderPass = renderPass,
                    });
                    i++;
                }
                cleanupStack.Push(() => {
                    foreach(Framebuffer frameBuffer in frameBuffers)
                    {
                        device.DestroyFramebuffer(frameBuffer);
                    }
                    frameBuffers = null;
                });
            }
        }        
    }
}
