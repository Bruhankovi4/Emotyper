﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WaveletStudio.Blocks.CustomAttributes;
using WaveletStudio.Properties;

namespace WaveletStudio.Blocks
{
    [SingleInputOutputBlock]
    [Serializable]
    class InputSeriesBlock : BlockBase
    {
      /// <summary>
        /// Constructor
        /// </summary>
        public InputSeriesBlock()
        {
            BlockBase root = this;
            CreateNodes(ref root);
            FilePath = "example.csv";
            ColumnSeparator = ",";
            SignalStart = 0;
            SamplingInterval = 1;
        }

        /// <summary>
        /// Name
        /// </summary>
        public override string Name { get { return Resources.ImportFromCSV; } }

        /// <summary>
        /// Description
        /// </summary>
        public override string Description { get { return Resources.ImportFromCSVDescription; } }

        /// <summary>
        /// Processing type
        /// </summary>
        public override BlockBase.ProcessingTypeEnum ProcessingType { get { return BlockBase.ProcessingTypeEnum.LoadSignal; } }

        /// <summary>
        /// Absolute or relative path to the file
        /// </summary>
        [Parameter]
        public string FilePath { get; set; }

        /// <summary>
        /// Column separator
        /// </summary>
        [Parameter]
        public string ColumnSeparator { get; set; }

        /// <summary>
        /// Signal start
        /// </summary>
        [Parameter]
        public int SignalStart { get; set; }

        private int _samplingRate;

        private double _samplingInterval;

        /// <summary>
        /// Sampling interval
        /// </summary>
        [Parameter]
        public double SamplingInterval 
        {
            get { return _samplingInterval; }
            set
            {
                _samplingInterval = value;
                if (Math.Abs(value - 0d) > double.Epsilon)
                {
                    _samplingRate = Convert.ToInt32(Math.Round(1 / value));   
                }
            }
        }

        /// <summary>
        /// Ignore first row when reading the file
        /// </summary>
        [Parameter]
        public bool IgnoreFirstRow { get; set; }

        /// <summary>
        /// If true, the first column contains the name of the signal
        /// </summary>
        [Parameter]
        public bool SignalNameInFirstColumn { get; set; }

        /// <summary>
        /// Executes the block
        /// </summary>
        public override void Execute()
        {
            OutputNodes[0].Object.Clear();
            var filePath = FilePath;
            if (!Path.IsPathRooted(filePath))
                filePath = Path.Combine(CurrentDirectory, FilePath);
            if(!File.Exists(filePath))
                filePath = Path.Combine(Utils.AssemblyDirectory, FilePath);
            if(!File.Exists(filePath))
                return;

            var lineNumber = 0;
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                lineNumber++;
                if (lineNumber == 1 && IgnoreFirstRow)
                    continue;
                var signal = ParseLine(line);
                if (signal == null) 
                    continue;
                if (signal.Name == "")
                    signal.Name = "Line " + lineNumber;
                OutputNodes[0].Object.Add(signal);
            }
            if (Cascade && OutputNodes[0].ConnectingNode != null)
                OutputNodes[0].ConnectingNode.Root.Execute();            
        }

        private Signal ParseLine(string line)
        {
            if(string.IsNullOrWhiteSpace(line))
                return null;

            var values = new List<double>();
            var samples = line.Split(new[] {ColumnSeparator}, StringSplitOptions.RemoveEmptyEntries);
            var columnNumber = 0;
            var signalName = "";
            foreach (var sampleString in samples)
            {
                columnNumber++;
                if (columnNumber == 1 && SignalNameInFirstColumn)
                {
                    signalName = sampleString;
                    continue;
                }
                double value;
                if (double.TryParse(sampleString, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
                    values.Add(value);
            }
            if(values.Count == 0)
                return null;

            var signal = new Signal(values.ToArray())
                             {
                                 Name = signalName,
                                 Start = SignalStart,
                                 Finish = SignalStart + SamplingInterval*values.Count - SamplingInterval,
                                 SamplingRate = _samplingRate,
                                 SamplingInterval = SamplingInterval
                             };
            return signal;
        }

        /// <summary>
        /// Creates the input and output nodes
        /// </summary>
        /// <param name="root"></param>
        protected override sealed void CreateNodes(ref BlockBase root)
        {
            root.OutputNodes = BlockOutputNode.CreateSingleOutputSignal(ref root);
        }


        /// <summary>
        /// Clone the block, including the template
        /// </summary>
        /// <returns></returns>
        public override BlockBase Clone()
        {
            var block = (ImportFromCSVBlock)MemberwiseClone();
            block.Execute();
            return block;
        }

        /// <summary>
        /// Clones this block but mantains the links
        /// </summary>
        /// <returns></returns>
        public override BlockBase CloneWithLinks()
        {
            var block = (ImportFromCSVBlock)MemberwiseCloneWithLinks();
            block.Execute();
            return block;
        }
    }
}
