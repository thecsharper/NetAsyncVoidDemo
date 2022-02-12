// From: https://www.meziantou.net/awaiting-an-async-void-method-in-dotnet.htm

namespace AsyncVoidDemo
{
    public class Program
    {
        public async static Task Main()
        {
            Console.WriteLine("before");
            
            await Run(() => Test());
            
            Console.WriteLine("after");
        }

       public static async void Test()
        {
            Console.WriteLine("begin");

            await Task.Delay(1000);
            
            Console.WriteLine("end");
        }

        public static async Task Run(Action action)
        {
            var currentContext = SynchronizationContext.Current;
            var synchronizationContext = new AsyncVoidSynchronizationContext(currentContext);
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            try
            {
                action();

                // Wait for the async void method to call OperationCompleted or to report an exception
                await synchronizationContext.Completed;
            }
            finally
            {
                // Reset the original SynchronizationContext
                SynchronizationContext.SetSynchronizationContext(currentContext);
            }
        }
    }
}