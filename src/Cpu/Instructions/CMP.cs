using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class CMP_Instruction : Instruction
    {
        public CMP_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Subtracts the contents of memory from the contents of the accumulator.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte result = (byte)(cpu.A - fr.operand);

            //Set carry flag
            if (fr.operand <= cpu.A)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
            }

            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }


    //[InstructionRegister]
    //public class CMP_IMM : Instruction
    //{
    //    public CMP_IMM(Cpu cpu):base(cpu)
    //    {
    //        this.mnemonic = "CMP";
    //        this.hexCode = 0xC9;
    //        this.addressingMode = AddressingMode.IMM;
    //        this.instructionBytes = 2;
    //        this.machineCycles = 2;
    //    }
    //    public override string Description => "Subtracts the contents of memory from the contents of the accumulator. Immediate.";

    //    public override void Execute(Cpu cpu)
    //    {
    //        cpu.IncrementPC(); //This is what we want to read
    //        byte m = cpu.addressBus.ReadByte(cpu.PC);

    //        byte result = (byte)(cpu.A - m);

    //        //Set carry flag
    //        if (m <= cpu.A)
    //        {
    //            cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
    //        }
    //        else
    //        {
    //            cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
    //        }

    //        //Set zero flag
    //        if (result == 0)
    //        {
    //            cpu.SetProcessorStatusFlag(true, StatusFlagName.Zero);
    //        }
    //        else
    //        {
    //            cpu.SetProcessorStatusFlag(false, StatusFlagName.Zero);
    //        }

    //        //Set negative flag
    //        //Set by result bit 7 --to do check this vs signed logic??
    //        bool isNegative = Convert.ToBoolean((result & 0b10000000) >> 7);
    //        cpu.SetProcessorStatusFlag(isNegative, StatusFlagName.Negative);

    //        cpu.SetTimingControl(machineCycles);
    //        cpu.IncrementPC();

    //    }
    //}
    //[InstructionRegister]
    //public class CMP_ZP : Instruction
    //{
    //    public CMP_ZP(Cpu cpu):base(cpu)
    //    {
    //        this.mnemonic = "CMP";
    //        this.hexCode = 0xC5;
    //        this.addressingMode = AddressingMode.ZP;
    //        this.instructionBytes = 2;
    //        this.machineCycles = 3;
    //    }

    //    public override string Description => "Subtracts the contents of memory from the contents of the accumulator. Zero Page.";

    //    public override void Execute(Cpu cpu)
    //    {
    //        cpu.IncrementPC();
    //        //gives the zero page address to find
    //        byte lb = cpu.addressBus.ReadByte(cpu.PC);
    //        byte m = cpu.addressBus.ReadByte((ushort)(lb));

    //        byte result = (byte)(cpu.A - m);

    //        //Set carry flag
    //        if (m <= cpu.A)
    //        {
    //            cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
    //        }
    //        else
    //        {
    //            cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
    //        }

    //        cpu.SetZeroFlagIfRequired(result);
    //        cpu.SetNegativeFlagIfRequired(result);

    //        cpu.SetTimingControl(machineCycles);
    //        cpu.IncrementPC();
    //    }
    //}
}
