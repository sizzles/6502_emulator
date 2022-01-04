using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class BVC_Instruction : Instruction
    {
        public BVC_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "tests the status of the V flag and takes the conditional branch if the flag is not set.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            bool overflowStatus = cpu.GetProcessorStatusFlag(StatusFlagName.Overflow);

            if (overflowStatus == false)
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
