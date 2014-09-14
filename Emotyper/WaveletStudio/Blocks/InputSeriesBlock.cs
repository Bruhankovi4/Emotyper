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
      /// <summary>
        /// Constructor
        /// </summary>
        public InputSeriesBlock()
        {
            BlockBase root = this;
            CreateNodes(ref root);
           
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
                  return;
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
