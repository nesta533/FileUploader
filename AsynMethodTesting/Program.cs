using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsynMethodTesting
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        public async Task MyMethod()
        {
            //throw new InvalidOperationException();
            await Task.Delay(1000);
        }

        public async Task CallMethod()
        {
            await MyMethod();
        }

        public async void ThrowExceptionAsync()
        {
            throw new InvalidOperationException();
        }

        public void AsyncVoidException_CannotBeCaughtByCatch()
        {
            try
            {
                ThrowExceptionAsync();
            }
            catch(Exception)
            {
                throw;
            }
        }


    }
}
