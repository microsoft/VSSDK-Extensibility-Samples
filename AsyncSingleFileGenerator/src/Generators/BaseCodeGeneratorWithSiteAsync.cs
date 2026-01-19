namespace AsyncSingleFileGeneratorSample;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Generators;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ServiceProvider = Microsoft.VisualStudio.Shell.ServiceProvider;

public abstract class BaseCodeGeneratorWithSiteAsync : IObjectWithSite, IDisposable, IVsSingleFileGenerator, IVsSingleFileGeneratorAsync
{
    private object site;
    private ServiceProvider serviceProvider;
    private ServiceProvider globalProvider;
    private bool _disposed;

    /// <summary>
    /// Get a wrapper on the containing project system's Service provider
    /// </summary>
    /// <remarks>
    /// This is a limited service provider that can only reliably provide VxDTE::SID_SVSProjectItem
    /// SID_SVSWebReferenceDynamicProperties IID_IVsHierarchy SID_SVsApplicationSettings
    /// To get the global provider, call GetSite on IVSHierarchy or use the GlobalServiceProvider
    /// property
    /// </remarks>
    protected ServiceProvider SiteServiceProvider
    {
        get
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (serviceProvider == null)
            {
                var sp = site as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                serviceProvider = new ServiceProvider(sp);
            }

            return serviceProvider;
        }
    }

    /// <summary>
    /// Provides a wrapper on the global service provider for Visual Studio
    /// </summary>
    protected ServiceProvider GlobalServiceProvider
    {
        get
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (globalProvider == null)
            {
                var siteServiceProvider = SiteServiceProvider;
                if (siteServiceProvider != null && siteServiceProvider.GetService(typeof(IVsHierarchy)) is IVsHierarchy vsHierarchy)
                {
                    ErrorHandler.ThrowOnFailure(vsHierarchy.GetSite(out var ppSP));
                    if (ppSP != null)
                    {
                        globalProvider = new ServiceProvider(ppSP);
                    }
                }
            }

            return globalProvider;
        }
    }

    /// <summary>
    /// SetSite method of IOleObjectWithSite
    /// </summary>
    /// <param name="pUnkSite">site for this object to use</param>
    public virtual void SetSite(object pUnkSite)
    {
        site = pUnkSite;
        serviceProvider = null;
    }

    /// <summary>
    /// GetSite method of IOleObjectWithSite
    /// </summary>
    /// <param name="riid">interface to get</param>
    /// <param name="ppvSite">array in which to stuff return value</param>
    public virtual void GetSite(ref Guid riid, out IntPtr ppvSite)
    {
        if (site == null)
        {
            Marshal.ThrowExceptionForHR(-2147467259);
        }

        IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(site);
        Marshal.QueryInterface(iUnknownForObject, ref riid, out var ppv);
        if (ppv == IntPtr.Zero)
        {
            Marshal.ThrowExceptionForHR(-2147467262);
        }

        ppvSite = ppv;
    }

    protected object GetService(Guid service) => SiteServiceProvider?.GetService(service);

    protected object GetService(Type service) => SiteServiceProvider?.GetService(service);

    int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
    {
        pbstrDefaultExtension = GetDefaultExtension();
        return VSConstants.S_OK;
    }

    int IVsSingleFileGenerator.Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
    {
        try
        {
            IntPtr outputFileContents = IntPtr.Zero;
            GenerateInternal(wszInputFilePath, bstrInputFileContents, wszDefaultNamespace, out outputFileContents, out var output, pGenerateProgress);
            rgbOutputFileContents[0] = outputFileContents;
            pcbOutput = (uint)output;
        }
        catch
        {
            pcbOutput = 0u;
            rgbOutputFileContents[0] = IntPtr.Zero;
            return VSConstants.E_FAIL;
        }

        return VSConstants.S_OK;
    }

    protected abstract string GetDefaultExtension();

    private void GenerateInternal(
        string inputFilePath, string inputFileContents, string defaultNamespace,
        out IntPtr outputFileContents, out int output, IVsGeneratorProgress pGenerateProgress)
    {
        if (inputFileContents == null)
        {
            throw new ArgumentNullException(nameof(inputFileContents));
        }

        var array = ThreadHelper.JoinableTaskFactory.Run(
            async () => await GenerateCodeAsync(inputFilePath, inputFileContents, defaultNamespace, pGenerateProgress));
        if (array == null)
        {
            outputFileContents = IntPtr.Zero;
            output = 0;
        }
        else
        {
            output = array.Length;
            outputFileContents = Marshal.AllocCoTaskMem(output);
            Marshal.Copy(array, 0, outputFileContents, output);
        }
    }

    async Task<string> IVsSingleFileGeneratorAsync.GetDefaultExtensionAsync(CancellationToken cancellationToken)
        => GetDefaultExtension();

    async Task<GeneratorResult> IVsSingleFileGeneratorAsync.GenerateAsync(string inputFilePath, string inputFileContents, string defaultNamespace, Stream outputStream, IVsGeneratorProgress generatorProgress, CancellationToken cancellationToken)
    {
        var resultBytes = await GenerateCodeAsync(inputFilePath, inputFileContents, defaultNamespace, generatorProgress, cancellationToken);
        if (resultBytes != null)
        {
            await outputStream.WriteAsync(resultBytes, 0, resultBytes.Length);
        }

        return GeneratorResult.Success;
    }

    protected abstract Task<byte[]> GenerateCodeAsync(
        string inputFilePath, string inputFileContents, string defaultNamespace, 
        IVsGeneratorProgress generateProgress, CancellationToken cancellationToken = default);

    ~BaseCodeGeneratorWithSiteAsync() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// In derived classes, override Dispose(bool disposing) and make sure you call this method
    /// even in case of exceptions (in finally).
    /// </summary>
    /// <param name="disposing">True to release managed resources; false from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        try
        {
            if (disposing)
            {
                serviceProvider?.Dispose();
                serviceProvider = null;

                globalProvider?.Dispose();
                globalProvider = null;
            }
        }
        finally
        {
            _disposed = true;
        }
    }
}
