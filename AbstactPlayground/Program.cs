using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GPI;

namespace AbstactPlayground
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

        class App : IDisposable
        {
            Form window;

            IGPIProvider provider;
            IGPIInstance instance;
            IGPIDevice device;
            IGPICommandBuffer mainCmd;

            Stack<Action> cleanupStack = new Stack<Action>();

            public App()
            {
                Console.WriteLine("Init start");
                Console.WriteLine(" . Init Window");
                InitWindow();
                Console.WriteLine(" . Init GPI");
                InitGPI();
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
                    System.Threading.Thread.Sleep(10);
                    Application.DoEvents();
                    DrawFrame();
                }
            }

            void DrawFrame()
            {
                IGPIFrame frame = device.BeginFrame();
                mainCmd.Reset();
                mainCmd.Clear(frame, Color.Red);
                mainCmd.Submit();
                device.EndFrame(frame);
            }

            void InitWindow()
            {
                window = new Form();
                window.Show();
                window.Size = new System.Drawing.Size(1280, 720);

                cleanupStack.Push(() => {
                    Console.WriteLine(" . Dispose Window");
                    if (!window.IsDisposed)
                    {
                        window.Close();
                    }
                    window = null;
                });
            }

            void InitGPI()
            {
                provider = new GPI.Vulkan.VulkanGPIProvider();
                if (!provider.IsSupported)
                {
                    throw new NotSupportedException(String.Format("IGPIProvider {0} is not supported and thus faild to init GPI", provider.Name));
                }
                instance = provider.CreateInstance();
                DeviceLayout layout = DeviceLayout.SimpleDeferred;
                if (!instance.IsSupported(layout))
                {
                    throw new NotSupportedException(String.Format("IGPIInstance from IGPIProvider {0} is not able to supported required layout", provider.Name));
                }
                device = instance.CreateDevice(layout);
                mainCmd = device.CreateCommandBuffer();

                cleanupStack.Push(() => {
                    Console.WriteLine(" . Dispose GPI");
                });
            }
        }
    }
}
