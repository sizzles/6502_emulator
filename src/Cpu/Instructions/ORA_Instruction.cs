using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{

    public class ORA_Instruction : Instruction
    {
        public ORA_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfers the memory and the accumulator to the adder which performs a binary 'OR' on a bit-by-bit basis and stores the result in the accumulator.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte result = (byte)(fr.operand | cpu.A);
            cpu.A = result;
            cpu.SetZeroFlagIfRequired(cpu.A);
            cpu.SetNegativeFlagIfRequired(cpu.A);
            cpu.SetTimingControl(this.machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }

    //[InstructionRegister] 
    //public class ORA_ABSY : Instruction
    //{
    //    public ORA_ABSY(Cpu cpu):base(cpu)
    //    {
    //        this.mnemonic = "ORA";
    //        this.hexCode = 0x19;
    //        this.addressingMode = AddressingMode.ABSY;
    //        this.instructionBytes = 3;
    //        this.machineCycles = 4; // + 1 to N if page boundary is crossed
    //    }

    //    public override string Description => "Transfers the memory and the accumulator to the adder which performs a binary 'OR' on a bit-by-bit basis and stores the result in the accumulator.";
    //    public override void Execute(Cpu cpu)
    //    {
    //        //binary or on memory and accumulator - store in the accumulator
    //        //Add y register to 2nd and 3rd bytes of the instruction
    //        //then read that for the memory and compare with the accumulator
    //        //How many extra pages are there after that?

    //        cpu.IncrementPC(); //now on first byte of operand
    //        ushort mem = (ushort)(cpu.Read16());
    //        ushort memY = (ushort)(mem + cpu.Y);

    //        byte result = (byte)(cpu.addressBus.ReadByte(memY) | cpu.A); //binary or on the accumulator
    //        cpu.A = result;

    //        //Set Zero flag if needed
    //        if (cpu.A == 0)
    //        {
    //            cpu.SetProcessorStatusFlag(true, StatusFlagName.Zero);
    //        }
    //        else
    //        {
    //            cpu.SetProcessorStatusFlag(false, StatusFlagName.Zero);
    //        }

    //        //Set Negative flag if needed
    //        if (cpu.IsByteNegative(result))
    //        {
    //            cpu.SetProcessorStatusFlag(true, StatusFlagName.Negative);
    //        }
    //        else
    //        {
    //            cpu.SetProcessorStatusFlag(false, StatusFlagName.Negative);
    //        }

    //        //Shift right to just compare the high bytes for these
    //        if (memY >> 8 > mem >> 8)
    //        {
    //            cpu.SetTimingControl(machineCycles + 1); //machine cycle for the extra page now
    //        }

    //        else
    //        {
    //            cpu.SetTimingControl(machineCycles);
    //        }

    //        cpu.IncrementPC();
    //    }
    //}
}
