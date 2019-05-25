using LibGit2Sharp;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeLensOopProvider
{
    [Export(typeof(IAsyncCodeLensDataPointProvider))]
    [Name(Id)]
    [ContentType("code")]
    [LocalizedName(typeof(Resources), "GitCommitCodeLensProvider")]
    [Priority(200)]
    internal class GitCommitDataPointProvider : IAsyncCodeLensDataPointProvider
    {
        internal const string Id = "GitCommit";

        public Task<bool> CanCreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext context, CancellationToken token)
        {
            Debug.Assert(descriptor != null);
            var gitRepo = GitUtil.ProbeGitRepository(descriptor.FilePath, out string repoRoot);
            return Task.FromResult<bool>(gitRepo != null);
        }

        public Task<IAsyncCodeLensDataPoint> CreateDataPointAsync(CodeLensDescriptor descriptor, CodeLensDescriptorContext context, CancellationToken token)
        {
            return Task.FromResult<IAsyncCodeLensDataPoint>(new GitCommitDataPoint(descriptor));
        }

        private class GitCommitDataPoint : IAsyncCodeLensDataPoint
        {
            private readonly CodeLensDescriptor descriptor;
            private readonly Repository gitRepo;
            private readonly string gitRepoRootPath;

            public GitCommitDataPoint(CodeLensDescriptor descriptor)
            {
                this.descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
                this.gitRepo = GitUtil.ProbeGitRepository(descriptor.FilePath, out this.gitRepoRootPath);
            }

            public event AsyncEventHandler InvalidatedAsync;

            public CodeLensDescriptor Descriptor => this.descriptor;

            public Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext context, CancellationToken token)
            {
                // get the most recent commit
                Commit commit = GitUtil.GetCommits(this.gitRepo, this.descriptor.FilePath, 1).FirstOrDefault();
                if (commit == null)
                {
                    return Task.FromResult<CodeLensDataPointDescriptor>(null);
                }

                CodeLensDataPointDescriptor response = new CodeLensDataPointDescriptor()
                {
                    Description = commit.Author.Name,
                    TooltipText = $"Last change committed by {commit.Author.Name} at {commit.Author.When.ToString(CultureInfo.CurrentCulture)}",
                    IntValue = null,    // no int value
                    ImageId = GetCommitTypeIcon(commit),
                };

                return Task.FromResult(response);
            }

            public Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext context, CancellationToken token)
            {
                // get the most recent 5 commits
                var commits = GitUtil.GetCommits(this.gitRepo, this.descriptor.FilePath, 5).AsEnumerable();
                if (commits == null || commits.Count() == 0)
                {
                    return Task.FromResult<CodeLensDetailsDescriptor>(null);
                }

                var headers = new List<CodeLensDetailHeaderDescriptor>()
                {
                    new CodeLensDetailHeaderDescriptor()
                    {
                        UniqueName = "CommitType",
                        Width = 22,
                    },
                    new CodeLensDetailHeaderDescriptor()
                    {
                        UniqueName = "CommitId",
                        DisplayName = "Commit Id",
                        Width = 100, // fixed width
                    },
                    new CodeLensDetailHeaderDescriptor()
                    {
                        UniqueName = "CommitDescription",
                        DisplayName = "Description",
                        Width = 0.66666, // use 2/3 of the remaining width
                    },
                    new CodeLensDetailHeaderDescriptor()
                    {
                        UniqueName = "CommitAuthor",
                        DisplayName = "Author",
                        Width = 0.33333, // use 1/3 of the remaining width
                    },
                    new CodeLensDetailHeaderDescriptor()
                    {
                        UniqueName = "CommitDate",
                        DisplayName = "Date",
                        Width = 85, // fixed width
                    }
                };

                var entries = commits.Select(
                    commit => new CodeLensDetailEntryDescriptor()
                    {
                        Fields = new List<CodeLensDetailEntryField>()
                        {
                            new CodeLensDetailEntryField()
                            {
                                ImageId = GetCommitTypeIcon(commit),
                            },
                            new CodeLensDetailEntryField()
                            {
                                Text = commit.Id.Sha.Substring(0, 8),
                            },
                            new CodeLensDetailEntryField()
                            {
                                Text = commit.MessageShort,
                            },
                            new CodeLensDetailEntryField()
                            {
                                Text = commit.Author.Name,
                            },
                            new CodeLensDetailEntryField()
                            {
                                Text = commit.Author.When.ToString(@"MM\/dd\/yyyy", CultureInfo.CurrentCulture),
                            },
                        },
                        Tooltip = commit.Message,
                        NavigationCommand = new CodeLensDetailEntryCommand()
                        {
                            CommandSet = new Guid("f3cb9f10-281b-444f-a14e-de5de36177cd"),
                            CommandId = 0x0100,
                            CommandName = "Git.NavigateToCommit",
                        },
                        NavigationCommandArgs = new List<object>() {commit.Id.Sha },
                    });

                var result = new CodeLensDetailsDescriptor()
                {
                    Headers = headers,
                    Entries = entries,
                    PaneNavigationCommands = new List<CodeLensDetailPaneCommand>()
                    {
                        new CodeLensDetailPaneCommand()
                        {
                            CommandId = new CodeLensDetailEntryCommand()
                            {
                                CommandSet = new Guid("57735D06-C920-4415-A2E0-7D6E6FBDFA99"),
                                CommandId = 0x1005,
                                CommandName = "Git.ShowHistory",
                            },
                            CommandDisplayName = "Show History"
                        }
                    },
                };

                return Task.FromResult(result);
            }

            /// <summary>
            /// Raises <see cref="IAsyncCodeLensDataPoint.Invalidated"/> event.
            /// </summary>
            /// <remarks>
            ///  This is not part of the IAsyncCodeLensDataPoint interface.
            ///  The data point source can call this method to notify the client proxy that data for this data point has changed.
            /// </remarks>
            public void Invalidate()
            {
                this.InvalidatedAsync?.Invoke(this, EventArgs.Empty).ConfigureAwait(false);
            }

            private static ImageId GetCommitTypeIcon(Commit commit)
            {
                var imageCatalog = new Guid("{ae27a6b0-e345-4288-96df-5eaf394ee369}");
                int merge = 1855;
                int changeset = 425;
                int imageId = commit.Parents.Count() > 1 ? merge : changeset;

                return new ImageId(imageCatalog, imageId);
            }
        }
    }
}
