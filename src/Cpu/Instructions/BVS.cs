using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class BVS_Instruction : Instruction
    {
        public BVS_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "tests the V flag and takes the conditional branch if V is on.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            bool overflowStatus = cpu.GetProcessorStatusFlag(StatusFlagName.Overflow);

            if (overflowStatus == true)
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
