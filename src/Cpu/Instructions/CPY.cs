using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class CPY_Instruction : Instruction
    {
        public CPY_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Compare Index register Y to memory. Zero Page.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            var result = (byte)(cpu.Y - fr.operand);

            if (cpu.Y >= fr.operand)
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
    //public class CPY_ZP_Instruction : Instruction
    //{
    //    public CPY_ZP_Instruction(Cpu cpu) : base(cpu)
    //    {
    //        this.mnemonic = "CPY";
    //        this.hexCode = 0xC4;
    //        this.addressingMode = AddressingMode.ZP;
    //        this.instructionBytes = 2;
    //        this.machineCycles = 3;
    //    }

    //    public override string Description => "Compare Index register Y to memory. Zero Page.";

    //    public override void Execute(Cpu cpu)
    //    {
    //        cpu.IncrementPC();
    //        byte lb = cpu.addressBus.ReadByte(cpu.PC); //Read the operand
    //        byte m = cpu.addressBus.ReadByte((ushort)(lb));

    //        var result = (byte)(cpu.Y - m);

    //        if (cpu.Y >= m)
    //        {
    //            cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
    //        }
    //        else
    //        {
    //            cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
    //        }

    //        HandleZeroFlag(result);
    //        HandleNegativeFlag(result);

    //        cpu.SetTimingControl(machineCycles);
    //        cpu.IncrementPC();
    //    }
    //}
}
