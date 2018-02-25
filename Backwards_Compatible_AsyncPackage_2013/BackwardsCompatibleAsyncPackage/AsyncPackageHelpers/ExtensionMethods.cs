using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.AsyncPackageHelpers
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Helper method to use async/await with IAsyncServiceProvider implementation
        /// </summary>
        /// <param name="asyncServiceProvider"></param>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static async Task<T> GetServiceAsync<T>(this IAsyncServiceProvider asyncServiceProvider, Type serviceType) where T : class
        {
            T returnValue = null;

            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                object serviceInstance = null;
                Guid serviceTypeGuid = serviceType.GUID;
                IVsTask task = asyncServiceProvider.QueryServiceAsync(ref serviceTypeGuid);
                await task;
                serviceInstance = task.GetResult();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                returnValue = serviceInstance as T;
            });

            return returnValue;
        }

        /// <summary>
        /// Gets if async package is supported in the current instance of Visual Studio
        /// </summary>
        /// <param name="serviceProvider">an IServiceProvider instance, usually a Package instance</param>
        /// <returns>true if async packages are supported</returns>
        public static bool IsAsyncPackageSupported(this IServiceProvider serviceProvider)
        {
            IAsyncServiceProvider asyncServiceProvider = serviceProvider.GetService(typeof(SAsyncServiceProvider)) as IAsyncServiceProvider;
            return asyncServiceProvider != null;
        }
    }
}