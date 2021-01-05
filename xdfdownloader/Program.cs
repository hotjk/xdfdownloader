using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace xdfdownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Please input user and pwd.");
                return;
            }
            Web web = new Web();
            web.Login(args[0], args[1]);
            web.WaitUserSelect();
            web.DownloadAllVideo();
        }
    }
}
