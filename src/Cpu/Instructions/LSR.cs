using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class LSR_Instruction : Instruction
    {
        public LSR_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "shifts either the accumulator or a specified memory location 1 bit to the righ";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);

            byte operand = fr.operand;

            byte result = (byte)(operand >> 1);

            bool carryFlag = Convert.ToBoolean((byte)(operand & 0b00000001));
            cpu.SetProcessorStatusFlag(carryFlag, StatusFlagName.Carry);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetZeroFlagIfRequired(result);

            if(this.addressingMode == AddressingMode.Accum)
            {
                cpu.A = result;
            }
            else
            {
                cpu.addressBus.WriteByte(result, fr.address);
            }

            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();

        }
    }
}
