using CodeLensOopProvider;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;

namespace CodeLensOopProviderVsix
{
    [Export(typeof(ICodeLensDetailsTemplateProvider))]
    [CodeLensDataPointViewModel(typeof(GitCommitDataPointViewModel))]
    internal class GitCommitCodeLensDetailsTemplateProvider : AbstractCodeLensDetailsTemplateProvider
    {
        public GitCommitCodeLensDetailsTemplateProvider()
        {

        }
    }

    [Export(typeof(IAsyncCodeLensDataPointViewModelProvider))]
    [Name("GitCommit")]
    //[CodeLensDataPoint(typeof(GitCommitDataPoint))]
    internal sealed class GitCommitDataPointViewModelProvider : IAsyncCodeLensDataPointViewModelProvider
    {
        public IAsyncCodeLensDataPointViewModel GetViewModel(IAsyncCodeLensDataPoint dataPoint)
        {
            return new GitCommitDataPointViewModel(dataPoint);
        }
    }

    internal class GitCommitDataPointViewModel : IAsyncCodeLensDataPointViewModel
    {
        public GitCommitDataPointViewModel(IAsyncCodeLensDataPoint dataPoint)
        {
            this.DataPoint = dataPoint;
        }

        public IAsyncCodeLensDataPoint DataPoint { get; }

        public bool IsLoadingDetails => throw new NotImplementedException();

        public string DetailsFailureInfo => throw new NotImplementedException();

        public ICommand RefreshCommand => throw new NotImplementedException();

        public string AdditionalInformation => throw new NotImplementedException();

        public string Descriptor => throw new NotImplementedException();

        public bool HasDetails => throw new NotImplementedException();

        public bool? HasData => throw new NotImplementedException();

        public bool IsVisible => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
