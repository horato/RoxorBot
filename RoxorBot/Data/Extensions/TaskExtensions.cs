using System.Threading.Tasks;

namespace RoxorBot.Data.Extensions
{
    public static class TaskExtensions
    {
        public static T WaitAndReturn<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
