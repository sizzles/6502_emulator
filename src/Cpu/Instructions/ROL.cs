using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class ROL_Instruction : Instruction
    {
        public ROL_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "rotate left instruction shifts either the accumulator or addressed memory left 1 bit,";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte operand = fr.operand;

            byte result = (byte)(operand << 1);
            byte inputCarry = (byte)(operand & 0b00000001);

            result += (byte)inputCarry; // input carry being stored in bit 0

            //input bit 7 stored in the carry flags.
            bool carryFlag = Convert.ToBoolean((byte)(operand & 0b10000000) >> 7);
            cpu.SetProcessorStatusFlag(carryFlag, StatusFlagName.Carry);
            cpu.SetZeroFlagIfRequired(result);

            //Negative equal to input bit 6
            bool negativeFlag = Convert.ToBoolean((byte)(operand & 0b01000000) >> 6);
            cpu.SetProcessorStatusFlag(negativeFlag, StatusFlagName.Negative);

            if (this.addressingMode == AddressingMode.Accum)
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
