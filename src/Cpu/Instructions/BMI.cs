using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class BMI_Instruction : Instruction
    {
        public BMI_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "takes the conditional branch if the N bit is set.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            bool negativeStatus = cpu.GetProcessorStatusFlag(StatusFlagName.Negative);

            if (negativeStatus == true)
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
