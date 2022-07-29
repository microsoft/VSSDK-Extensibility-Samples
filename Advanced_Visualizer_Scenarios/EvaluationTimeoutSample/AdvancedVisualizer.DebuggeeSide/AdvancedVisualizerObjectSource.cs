using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.DebuggerVisualizers;
using CustomObjects;

namespace AdvancedVisualizer.DebuggeeSide
{
    public class CustomVisualizerObjectSource : VisualizerObjectSource
    {
        public override void TransferData(object obj, Stream fromVisualizer, Stream toVisualizer)
        {
            // Serialize `obj` into the `toVisualizer` stream...

            // Start the timer so that we can stop processing the request if it is are taking too long.
            long startTime = Environment.TickCount;

            var slowObject = obj as VerySlowObject;

            bool isComplete = true;

            // Read the supplied command
            fromVisualizer.Seek(0, SeekOrigin.Begin);
            IDeserializableObject deserializableObject = GetDeserializableObject(fromVisualizer);
            GetVeryLongListCommand command = (GetVeryLongListCommand)deserializableObject.ToObject(null);

            List<string> returnValues = new List<string>();

            for (int i = (int)command.StartOffset; i < slowObject.VeryLongList.Count; i++)
            {
                // If the call takes more than 3 seconds, just return what we have received so far and fetch the remaining data on a posterior call.
                if ((Environment.TickCount - startTime) > 3_000)
                {
                    isComplete = false;
                    break;
                }

                // This call takes a considerable amount of time...
                returnValues.Add(slowObject.VeryLongList[i].ToString());
            }

            GetVeryLongListResponse response = new GetVeryLongListResponse(returnValues.ToArray(), isComplete);
            Serialize(toVisualizer, response);
        }
    }
}
