using System;

namespace AdvancedVisualizer.DebuggeeSide
{
    [Serializable]
    public class GetVeryLongListCommand
    {
        public int StartOffset { get; }

        public GetVeryLongListCommand(int startOffset)
        {
            StartOffset = startOffset;
        }
    }

    [Serializable]
    public class GetVeryLongListResponse
    {
        public string[] Values { get; }
        public bool IsComplete { get; }

        public GetVeryLongListResponse(string[] values, bool isComplete)
        {
            Values = values;
            IsComplete = isComplete;
        }
    }
}
