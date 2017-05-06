using Microsoft.VisualStudio.Shell.TableManager;

namespace SpellChecker
{
    class SpellingErrorsFactory : TableEntriesSnapshotFactoryBase
    {
        private readonly SpellChecker _spellChecker;

        public SpellingErrorsSnapshot CurrentSnapshot { get; private set; }

        public SpellingErrorsFactory(SpellChecker spellChecker, SpellingErrorsSnapshot spellingErrors)
        {
            _spellChecker = spellChecker;

            this.CurrentSnapshot = spellingErrors;
        }

        internal void UpdateErrors(SpellingErrorsSnapshot spellingErrors)
        {
            this.CurrentSnapshot.NextSnapshot = spellingErrors;
            this.CurrentSnapshot = spellingErrors;
        }

        #region ITableEntriesSnapshotFactory members
        public override int CurrentVersionNumber
        {
            get
            {
                return this.CurrentSnapshot.VersionNumber;
            }
        }

        public override void Dispose()
        {
        }

        public override ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return this.CurrentSnapshot;
        }

        public override ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            // In theory the snapshot could change in the middle of the return statement so snap the snapshot just to be safe.
            var snapshot = this.CurrentSnapshot;
            return (versionNumber == snapshot.VersionNumber) ? snapshot : null;
        }
        #endregion
    }
}
