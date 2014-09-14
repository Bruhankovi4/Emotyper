using System;
using System.Collections.Generic;
using WaveletStudio.Blocks.CustomAttributes;
using WaveletStudio.Properties;

namespace WaveletStudio.Blocks
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EmotivRawDataBlock : BlockBase
    {
       
        /// <summary>
        /// 
        /// </summary>
        public EmotivRawDataBlock()
        {
            BlockBase root = this;
            CreateNodes(ref root);
            this.Height = 120;
            this.Width = 150;
        }

        /// <summary>
        /// Name of the block
        /// </summary>
        public override string Name
        {
            get { return "Emotiv Data"; }
        }

        public override string Description
        {
            get { return "Emotiv block that gives raw data from emotiv headset"; }
        }

        public override ProcessingTypeEnum ProcessingType
        {
            get { return ProcessingTypeEnum.LoadSignal;  }
        }

        public override void Execute()
        {
            return;
        }
        protected override sealed void CreateNodes(ref BlockBase root)
        {
                            
            root.OutputNodes = new List<BlockOutputNode>
                                   {
                                       new BlockOutputNode(ref root, "AF3", "AF3"),
                                       new BlockOutputNode(ref root, "F7", "F7"),
                                       new BlockOutputNode(ref root, "F3", "F3"),
                                       new BlockOutputNode(ref root, "FC5", "FC5"),
                                       new BlockOutputNode(ref root, "T7", "T7"),
                                       new BlockOutputNode(ref root, "P7", "P7"),
                                       new BlockOutputNode(ref root, "O1", "O1"),
                                       new BlockOutputNode(ref root, "O2", "O2"),
                                       new BlockOutputNode(ref root, "P8", "P8"),
                                       new BlockOutputNode(ref root, "T8", "T8"),
                                       new BlockOutputNode(ref root, "FC6", "FC6"),
                                       new BlockOutputNode(ref root, "F4", "F4"),
                                       new BlockOutputNode(ref root, "F8", "F8"),
                                       new BlockOutputNode(ref root, "AF4", "AF4")
                                   };
        }

        public override BlockBase Clone()
        {
            var block = (EmotivRawDataBlock)MemberwiseClone();
            block.Execute();
            return block;
        }

        public override BlockBase CloneWithLinks()
        {
            var block = (EmotivRawDataBlock)MemberwiseCloneWithLinks();
            block.Execute();
            return block;
        }
    }
}
