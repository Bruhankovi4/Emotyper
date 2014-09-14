using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WaveletStudio.Blocks.CustomAttributes;
using WaveletStudio.Properties;

namespace WaveletStudio.Blocks
{
    [Serializable]
    public class InputSeriesBlock : BlockBase
    {
        private List<double> _series = new List<double>(){1,2,3,4,5}; 
      /// <summary>
        /// Constructor
        /// </summary>
        public InputSeriesBlock()
        {
            BlockBase root = this;
            CreateNodes(ref root);
          Start = 0;
          Finish = 100;
          SamplingInterval = 1;
          SamplingRate = 1;

        }

        public List<double> GetSeries()
        {
            return _series;
        }
        public void SetSeries(List<double> series )
        {
            this._series = series;
        }
        public void SetSeriesAndExecute(List<double> series)
        {
            this._series = series;
            Execute();
        }
        /// <summary>
        /// Name
        /// </summary>
        public override string Name { get { return "InputSeries"; } }

        /// <summary>
        /// Description
        /// </summary>
        public override string Description { get { return "InputSeries"; } }

        /// <summary>
        /// Processing type
        /// </summary>
        public override ProcessingTypeEnum ProcessingType
        {
            get { return ProcessingTypeEnum.LoadSignal; }
        }

   
        /// <summary>
        /// Executes the block
        /// </summary>
        public override void Execute()
        {
                  if (_series == null || !_series.Any())   //series is empty
                  {
                      return;
                  }
                  OutputNodes[0].Object = new List<Signal> { new Signal(_series.ToArray())
                        {
                            SamplingRate = SamplingRate,
                            Start = Start,
                            Finish = Finish,
                            SamplingInterval = SamplingInterval
                        }};
                  if (Cascade && OutputNodes[0].ConnectingNode != null)
                      OutputNodes[0].ConnectingNode.Root.Execute();    
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

        public int SamplingRate { get; set; }

        public double Start { get; set; }

        public double Finish { get; set; }

        public double SamplingInterval { get; set; }
    }
}
