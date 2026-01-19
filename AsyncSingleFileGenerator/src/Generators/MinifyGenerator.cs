namespace AsyncSingleFileGeneratorSample;

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using NUglify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Most of the magic is done in the BaseCodeGeneratorWithSiteAsync class, don't forget to check it out!
/// </summary>
[Guid("AE47AF68-696D-491E-B536-32F859E0B988")]
public sealed class MinifyCodeGenerator : BaseCodeGeneratorWithSiteAsync
{
    public const string Name = nameof(MinifyCodeGenerator);
    public const string Description = "Generates a minified version of JavaScript, CSS and HTML files files.";

    protected override string GetDefaultExtension()
    {
        var item = GetService(typeof(ProjectItem)) as ProjectItem;
        return ".min" + Path.GetExtension(item?.FileNames[1]);
    }

    protected override async Task<byte[]> GenerateCodeAsync(
        string inputFileName, string inputFileContent, string defaultNamespace,
        IVsGeneratorProgress generateProgress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputFileContent)) { return null; }

        var result = await MinifyAsync(inputFileName, inputFileContent);

        if (result.HasErrors)
        {
            return Encoding.UTF8.GetBytes("// Source file contains errors");
        }
        else
        {
            return Encoding.UTF8.GetBytes(result.Code);
        }
    }

    private static Task<UglifyResult> MinifyAsync(string inputFileName, string inputFileContent)
    {
        // Note! Uglify does not have async methods, so we run it in a Task to avoid blocking.

        return Task.Run(() =>
        {
            string ext = Path.GetExtension(inputFileName).ToLowerInvariant();

            switch (ext)
            {
                case ".js":
                    return Uglify.Js(inputFileContent);
                case ".css":
                    return Uglify.Css(inputFileContent);
                case ".htm":
                case ".html":
                    return Uglify.Html(inputFileContent);
            }

            return new UglifyResult(inputFileContent, new List<UglifyError>());
        });
    }
}