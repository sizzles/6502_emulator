using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class AND_Instruction : Instruction
    {
        public AND_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Transfer the accumulator and memory to the adder which performs a bit-by-bit AND operation and stores result back in accumulator";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte result = (byte)(cpu.A & fr.operand);
            cpu.A = result;
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }


    //[InstructionRegister]
    //public class AND_ZP_Instruction : Instruction
    //{
    //    public AND_ZP_Instruction(Cpu cpu) : base(cpu)
    //    {
    //        this.mnemonic = "AND";
    //        this.hexCode = 0x25;
    //        this.addressingMode = AddressingMode.ZP; //Zero Page Mode
    //        this.instructionBytes = 2;
    //        this.machineCycles = 3;
    //    }

    //    public override string Description => "Transfer the accumulator and memory to the adder which performs a bit-by-bit AND operation and stores result back in accumulator. Zero Page.";

    //    public override void Execute(Cpu cpu)
    //    {
    //        //increment the PC - read the page
    //        cpu.IncrementPC();
    //        byte lb = cpu.addressBus.ReadByte(cpu.PC);
    //        byte m = cpu.addressBus.ReadByte((ushort)(lb));

    //        //do a bitwise and with what is in the Accumulator
    //        byte result = (byte)(cpu.A & m);

    //        //store this on the accumulator
    //        cpu.A = result;

    //        cpu.IncrementPC();
    //        cpu.SetZeroFlagIfRequired(result);
    //        cpu.SetNegativeFlagIfRequired(result);
    //        cpu.SetTimingControl(machineCycles);
    //    }
    //}
}