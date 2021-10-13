using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symphonia2
{
    public class Constants
    {
        public string build;

        public void initVersion()
        {
#if DEBUG
            build = DateTime.Now.ToString("yyyyMMdd") + "dev";
#else
            build = "20211013a";
#endif
        }
    }
}
