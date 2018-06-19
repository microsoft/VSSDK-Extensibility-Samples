using LibGit2Sharp;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CodeLensOopProvider
{
	public static class GitUtil
    {
        /// <summary>
        /// Given a directory path, probes a Git repo and root path to the repo.
        /// </summary>
        public static Repository ProbeGitRepository(string path, out string repoRoot)
        {
            repoRoot = null;

            // if path is neither a directory nor a file path
            if (!Directory.Exists(path) && !File.Exists(path))
                return null;

            try
            {
                repoRoot = Repository.Discover(path);
                if (repoRoot != null)
                {
                    var repo = new Repository(repoRoot);
                    return repo;
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                return null;
            }

            return null;
        }

        public static Branch GetCurrentBranch(Repository repo)
        {
            return repo.Head;
        }

        public static Branch GetTrackedBranch(Repository repo)
        {
            return repo.Head.TrackedBranch;
        }

        public static Commit GetLastCommit(Repository repo)
        {
            return repo.Commits.FirstOrDefault();
        }

        public static ImmutableArray<Commit> GetCommits(Repository repo, string filePath, int count)
        {
            var workingDir = repo.Info.WorkingDirectory;
            var relativePath = filePath.Replace(workingDir, string.Empty);

            var filter = new Func<Commit, bool>(
                commit =>
                {
                    var commitId = commit.Tree[relativePath]?.Target?.Sha;
                    var parentId = commit.Parents?.FirstOrDefault()?[relativePath]?.Target?.Sha;
                    return commitId != parentId;
                });

            var commits = repo.Commits.Where(filter);

            var last5 = commits.OrderByDescending(c => c.Author.When).Take(count);

            return ImmutableArray.ToImmutableArray(last5);
        }
    }
}
