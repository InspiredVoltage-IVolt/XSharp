﻿using System;

namespace XSharp.Assembler.x86.X86
{
    [XSharp.Assembler.OpCode("cmps")]
    public class Cmps: InstructionWithSize, IInstructionWithPrefix {

        public InstructionPrefixes Prefixes {
            get;
            set;
        }

        public override void WriteText(XSharp.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            if ((Prefixes & InstructionPrefixes.RepeatTillEqual) != 0)
            {
                aOutput.Write("repne ");
            }
            switch (Size) {
                case 32:
                    aOutput.Write("cmpsd");
                    return;
                case 16:
                    aOutput.Write("cmpsw");
                    return;
                case 8:
                    aOutput.Write("smpsb");
                    return;
                default: throw new Exception("Size not supported!");
            }
        }
    }
}
