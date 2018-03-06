using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VulkanPlayground
{
    public static class Helper
    {
        public static uint ToVulkanVersion(this System.Version self)
        {
            uint major = SaveConvertToUint32(self.Major);
            uint minor = SaveConvertToUint32(self.Minor);
            uint patch = SaveConvertToUint32(self.Revision);

            return Vulkan.Version.Make(major, minor, patch);
        }

        public static uint SaveConvertToUint32(int value, uint defaultValue = 0)
        {
            if (value >= 0)
            {
                return (uint)value;
            }
            return defaultValue;
        }

        public static bool TryConvertToUint32(int inValue, out uint outValue)
        {
            if (inValue >= 0)
            {
                outValue = (uint)inValue;
                return true;
            }
            outValue = 0;
            return false;
        }
    }
}
