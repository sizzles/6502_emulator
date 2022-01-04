using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class BEQ_Instruction : Instruction
    {
        public BEQ_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "takes a conditional branch whenever the Z flag is on or the previ­ ous result is equal to 0.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            bool zeroStatus = cpu.GetProcessorStatusFlag(StatusFlagName.Zero);

            if (zeroStatus == true)
            {
                sbyte signedOp = (sbyte)fr.operand;
                cpu.SetTimingControl(machineCycles + fr.pageCross + 1);
                cpu.SetPC((ushort)(cpu.PC + signedOp));
            }
            else
            {
                cpu.SetTimingControl(machineCycles + fr.pageCross);
                cpu.IncrementPC();
            }
        }
    }
}
