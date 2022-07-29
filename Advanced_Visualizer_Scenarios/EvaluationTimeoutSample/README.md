# Evaluation Timeout Sample

This folder contains the different projects that were used in the [Advanced Visualizer Scenarios](https://review.docs.microsoft.com/en-us/visualstudio/debugger/visualizer-advanced-scenarios?view=vs-2022#handling-long-serialization-time) tutorial that focused on how to handle long serialization time.

To follow the tutorial, open the AdvancedVisualizerExample.sln solution file in VS 2022 and then follow the instructions described below:

1. Build the solution.
2. Copy the **AdvancedVisualizer.DebuggeeSide.dll** file to the *\<VS Instance\>*\Common7\Packages\Debugger\Visualizers\net4.6.2 directory.
3. Copy the **AdvancedVisualizer.DebuggerSide.dll** and **VerySlowObject.dll** files to the *\<VS Instance\>*\Common7\Packages\Debugger\Visualizers directory.
4. Create a .NET Framework 4.6.2 or above console application and replace its main method with the following code snippet:

    ```csharp
    static void Main(string[] args)
    {
        VerySlowObject obj = new VerySlowObject();
        obj.VeryLongList = new List<SomeRandomObject>();

        for (int i = 0; i < 200; i++)
        {
            obj.VeryLongList.Add(new SomeRandomObject());
        }

        Console.WriteLine(); // Add a breakpoint here.
    }
    ```
5. Set a breakpoint on the specified line.
6. Debug the console appliation that was created in step 4.
7. When the debugger enters break state in the locals window you should see a magnifying glass icon next to the `obj` instance. Click it and the visualizer window for this sample should open.
8. The visualizer window will show a progress bar and after a few minutes it should show the text '200' which indicates that all the data was fetched from the *debuggee-side*. 