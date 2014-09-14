using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaveletStudio.Blocks
{
    [Serializable]
  public class OutputSeriesBlock:BlockBase
    {
       
        private List<double> _series = new List<double>(); 
      /// <summary>
        /// Constructor
        /// </summary>
        public OutputSeriesBlock()
        {
            BlockBase root = this;
            CreateNodes(ref root);
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
        public override string Name { get { return "OutputSeries"; } }

        /// <summary>
        /// Description
        /// </summary>
        public override string Description { get { return "OutputSeries"; } }

        /// <summary>
        /// Processing type
        /// </summary>
        public override ProcessingTypeEnum ProcessingType
        {
            get { return ProcessingTypeEnum.Export; }
        }

   
        /// <summary>
        /// Executes the block
        /// </summary>
        public override void Execute()
        {
            var inputNode = InputNodes[0].ConnectingNode as BlockOutputNode;
            if (inputNode == null || inputNode.Object == null)
                return;
            if (inputNode.Object.Count()>0)
             _series= new List<double>(inputNode.Object[0].Samples);
            //foreach (var inputSignal in inputNode.Object)
            //{

            //} 
            //todo Fire event with processed data here  istead of foreach loop
            
        }

        /// <summary>
        /// Creates the input and output nodes
        /// </summary>
        /// <param name="root"></param>
        protected override sealed void CreateNodes(ref BlockBase root)
        {
            root.InputNodes = BlockInputNode.CreateSingleInputSignal(ref root);
        }


        /// <summary>
        /// Clone the block, including the template
        /// </summary>
        /// <returns></returns>
        public override BlockBase Clone()
        {
            var block = (OutputSeriesBlock)MemberwiseClone();
            block.Execute();
            return block;
        }

        /// <summary>
        /// Clones this block but mantains the links
        /// </summary>
        /// <returns></returns>
        public override BlockBase CloneWithLinks()
        {
            var block = (OutputSeriesBlock)MemberwiseCloneWithLinks();
            block.Execute();
            return block;
        }
    }
}
       