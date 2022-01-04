using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class BCC_Instruction : Instruction
    {
        public BCC_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Tests the state of the carry bit and takes a conditional branch if the carry bit is reset.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);


            bool carryStatus = cpu.GetProcessorStatusFlag(StatusFlagName.Carry);

            if (carryStatus == false)
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
