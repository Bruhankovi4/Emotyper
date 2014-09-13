using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 using WaveletStudio.Blocks;
using WaveletStudio.Functions;

namespace WaveletStudio
{
    class FirstSample
    {
 
           public static void RunModel()
        {
            //Declaring the blocks
            var importFromCSVBlock = new ImportFromCSVBlock
            {
                FilePath = "D:\\Aforge\\framework-development\\Samples\\Neuro\\Resilient Backpropagation\\Sample data (time series)\\sigmoid - Copy.csv",
                ColumnSeparator = ";",
                SignalStart = 0,
                SamplingInterval = 1.0,
                IgnoreFirstRow = true,
                SignalNameInFirstColumn = false
            };
            var dWTBlock = new DWTBlock
            {
                WaveletName = "db10(db10)",
                Level = 1,
                Rescale = false,
                ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.SymmetricHalfPoint
            };
            var exportToCSVBlock = new ExportToCSVBlock
            {
                FilePath = "output.csv",
                ColumnSeparator = ";",
                DecimalPlaces = 3,
                IncludeSignalNameInFirstColumn = true
            };

            //Connecting the blocks
            importFromCSVBlock.OutputNodes[0].ConnectTo(dWTBlock.InputNodes[0]);
            dWTBlock.OutputNodes[3].ConnectTo(exportToCSVBlock.InputNodes[0]);

            //Appending the blocks to a block list and execute all
            var blockList = new BlockList();
            blockList.Add(importFromCSVBlock);
            blockList.Add(dWTBlock);
            blockList.Add(exportToCSVBlock);
            blockList.ExecuteAll();
        }
    }
}

 