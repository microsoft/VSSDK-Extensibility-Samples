using CodeLensOopProviderShared;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Windows;

namespace CodeLensOopProviderVsix
{
    [Export(typeof(IViewElementFactory))]
    [Name("Git commit details UI factory")]
    [TypeConversion(from: typeof(GitCommitCustomDetailsData), to: typeof(FrameworkElement))]
    [Order]
    internal class ViewElementFactory : IViewElementFactory
    {
        public TView CreateViewElement<TView>(ITextView textView, object model) where TView : class
        {
            // Should never happen if the service's code is correct, but it's good to be paranoid.
            if (typeof(FrameworkElement) != typeof(TView))
            {
                throw new ArgumentException($"Invalid type conversion. Unsupported {nameof(model)} or {nameof(TView)} type");
            }

            if (model is GitCommitCustomDetailsData detailsData)
            {
                var detailsUI = new GitCommitDetails();
                detailsUI.DataContext = detailsData;
                return detailsUI as TView;
            }

            return null;
        }
    }
}
