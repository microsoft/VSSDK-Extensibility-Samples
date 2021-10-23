using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using NUglify;

namespace SingleFileGeneratorSample
{
    [Guid("cffb7601-6a1b-4f28-a2d0-a435e6686a2e")]
    public sealed class MinifyCodeGenerator : BaseCodeGeneratorWithSite
    {
        public const string Name = nameof(MinifyCodeGenerator);
        public const string Description = "Generates a minified version of JavaScript, CSS and HTML files files.";

        public override string GetDefaultExtension()
        {
            var item = GetService(typeof(ProjectItem)) as ProjectItem;
            return ".min" + Path.GetExtension(item?.FileNames[1]);
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            string ext = Path.GetExtension(inputFileName).ToLowerInvariant();
            UglifyResult result = Minify(inputFileName, inputFileContent);

            if (result.HasErrors)
            {
                return Encoding.UTF8.GetBytes("// Source file contains errors");
            }
            else
            {
                return Encoding.UTF8.GetBytes(result.Code);
            }
        }

        private static UglifyResult Minify(string inputFileName, string inputFileContent)
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
        }
    }
}