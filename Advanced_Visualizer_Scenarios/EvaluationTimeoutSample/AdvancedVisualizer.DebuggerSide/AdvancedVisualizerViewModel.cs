using Microsoft.VisualStudio.DebuggerVisualizers;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvancedVisualizer.DebuggeeSide;
using System.Threading;

namespace AdvancedVisualizer.DebuggerSide
{
    internal class AdvancedVisualizerViewModel
    {
        private IAsyncVisualizerObjectProvider m_asyncObjectProvider;

        public AdvancedVisualizerViewModel(IAsyncVisualizerObjectProvider asyncObjectProvider)
        {
            m_asyncObjectProvider = asyncObjectProvider;
        }

        public async Task<string> GetDataAsync()
        {
            List<string> verySlowObjectList = new List<string>();

            // Consider the possibility that we might timeout when fetching the data.
            bool isRequestComplete;

            do
            {
                // Send the command requesting more elements from the collection and process the response.
                IDeserializableObject deserializableObject = await m_asyncObjectProvider.TransferDeserializableObjectAsync(new GetVeryLongListCommand(verySlowObjectList.Count), CancellationToken.None);
                GetVeryLongListResponse response = deserializableObject.ToObject<GetVeryLongListResponse>();

                // Check if a timeout occurred, if it did we will try fetching more data again.
                isRequestComplete = response.IsComplete;

                // If no timeout occurred and we did not get all the elements we asked for, then we reached the end
                // of the collection and we can safely exit the loop.
                verySlowObjectList.AddRange(response.Values);
            }
            while (!isRequestComplete);

            // Do some processing of the data before showing it to the user.
            string valuesToBeShown = ProcessList(verySlowObjectList);
            return valuesToBeShown;
        }

        private string ProcessList(List<string> verySlowObjectList)
        {
            return $"{verySlowObjectList.Count}";
        }
    }
}
