using Cpu.Instructions;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Cpu
{
    //References:
    //1) http://archive.6502.org/datasheets/rockwell_r650x_r651x.pdf
    //2) http://users.telenet.be/kim1-6502/6502/proman.html#10 --good one
    //4) http://archive.6502.org/books/mcs6500_family_hardware_manual.pdf
    //5) https://slark.me/c64-downloads/6502-addressing-modes.pdf -- addressing modes
    //6) http://www.6502.org/tutorials/vflag.html#2.4 -- overflow flag / addition
    //7) https://www.pagetable.com/c64ref/6502/?tab=2 -- opcodes 
    //8) https://www.middle-engine.com/blog/posts/2020/06/23/programming-the-nes-the-6502-in-detail
    //9) https://www.nesdev.com/6502_cpu.txt

    public class Ram
    {
        public byte[] memory;

        public Ram()
        {
            this.memory  = new 
                byte[65536];
            //Init all to zeros
            for (var i = 0x0000; i <= 0xFFFF; i += 0x0001)
            {
                byte b = 0;
                this.memory[i] = b;
            }
        }

        public byte this[ushort index]
        {
            get => this.memory[index];
            set => this.memory[index] = value;
        }

    }

    public class AddressBus
    {

        public Ram ram;

        public AddressBus()
        {
            this.ram = new Ram();
        }

        public byte ReadByte(ushort address)
        {
            return ram[address];
        }

        public void WriteByte(byte value, ushort address)
        {
            ram[address] = value;
        }

    }


    public enum StatusFlagName
    {
        Carry = 0,
        Zero = 1,
        IRQDisable =2,
        DecimalMode = 3,
        Brk = 4,
        Overflow = 6,
        Negative = 7
    }

    public record FetchResult(byte operand, int pageCross, ushort address);

    public class Cpu
    {

        public string PC_Hex => PC.ToString("X");

        //Registers
        /// <summary>
        /// Accumulator
        /// </summary>
        public Byte A; //Accumulator 8-bit
        /// <summary>
        /// Index Register Y
        /// </summary>
        public Byte Y; //Index Register 8-bit
        /// <summary>
        /// Index Register X
        /// </summary>
        public Byte X; //Index Register 8-bit
        /// <summary>
        /// Program Counter
        /// </summary>
        public ushort PC; //Program Counter 16-bit
        /// <summary>
        /// Stack Pointer 
        /// </summary>
        public Byte S; 
        /// <summary>
        /// Processor Status Flags - NV1BDIZC
        /// </summary>
        public Byte P;
        public int TimingControl = 0; //Timing Control - set to 0 on each instruction fetch
        public object _lock = new object();

        //8 bits of data transferred during each instruction cycle

        //Look at the program counter
        //Load the instruction from that address
        //Containing the addressing mod and the number of cycles
        //read any extra bytes we need
        //execute the instruction
        //count cycles and finish

        public AddressBus addressBus;
        //public Dictionary<Byte, Instruction> instructionMatrix;
        public Instruction[] instructions;

        //Interrupt Vector Locations
        public const ushort ABORT_VECTOR_LSB = 0XFFF8;
        //public const ushort ABORT_VECTOR_MSB = 0XFFF9;

        public const ushort COP_VECTOR_LSB = 0XFFF4;
        //public const ushort COP_VECTOR_MSB = 0XFFF5;

        public const ushort BRK_VECTOR_LSB = 0XFFFE;
        //public const ushort BRK_VECTOR_MSB = 0XFFFF;



        #region ProcessorStatusFlags
        public void SetZeroFlagIfRequired(byte result)
        {
            //Check for zero flag
            if (result == 0)
            {
                SetProcessorStatusFlag(true, StatusFlagName.Zero);
            }
            else
            {
                SetProcessorStatusFlag(false, StatusFlagName.Zero);
            }
        }

        public void SetNegativeFlagIfRequired(byte result)
        {            //Negative flag
            bool value = Convert.ToBoolean(result >> 7);
            SetProcessorStatusFlag(value, StatusFlagName.Negative);            
        }

        public void SetCarryFlagIfRequired(ushort result)
        {
            //check if in decmial mode
            bool decimalFlag = GetProcessorStatusFlag(StatusFlagName.DecimalMode);

            if (decimalFlag == true)
            {
                if (result > 99)
                {
                    SetProcessorStatusFlag(true, StatusFlagName.Carry);
                }
                else
                {
                    SetProcessorStatusFlag(false, StatusFlagName.Carry);
                }
            }

            else
            {
                if (result > 255)
                {
                    SetProcessorStatusFlag(true, StatusFlagName.Carry);
                }
                else
                {
                    SetProcessorStatusFlag(false, StatusFlagName.Carry);
                }
            }
        }

        //Sets the flag in the processor status word
        public void SetProcessorStatusFlag(bool flag, StatusFlagName statusflagName)
        {
            //NV1BDIZC
            //Negative Result
            //Overflow
            //Expansion
            //Break Command
            //Decimal Mode
            //Interrupt Disable
            //Zero Result
            //Carry

            byte flagNumber = (byte)statusflagName;

            if (flag == true)
            {
                byte mask = (byte)(0b00000001 << flagNumber);
                P = (byte)(P | mask);
            }

            else
            {
                byte mask = (byte)(0b00000001 << flagNumber ^ 0b11111111);
                P = (byte)(P & mask);
            }
        }

        public bool GetProcessorStatusFlag(StatusFlagName statusFlagName)
        {
            //Build mask by left shifting, apply bitwise AND then right shift to move that to 0 bit position
            byte flagNumber = (byte)statusFlagName;
            byte result = (byte)((P & (byte)(0b00000001 << flagNumber)) >> flagNumber);

            return Convert.ToBoolean(result);

        }

        #endregion
        public Cpu()
        {
            addressBus = new AddressBus();
            //instructionMatrix = new Dictionary<Byte, Instruction>();
            instructions = new Instruction[256];
            this.InitInstructionRegister();
            //this.InitProcessorStatusFlags();
            this.PowerUp();
            //this.Reset();
        }

        //private void RegisterInstruction(Instruction instruction)
        //{
        //    instructionMatrix[instruction.hexCode] = instruction;
        //}

        private void InitInstructionRegister() {

            //Assembly info = Assembly.GetExecutingAssembly();
            //var assemblyTypes = info.DefinedTypes;
            //int instructionsRegistered = 0;

            //foreach(var type in assemblyTypes)
            //{
            //    var customAttributes = type.GetCustomAttributes();

            //    foreach(var customAttribute in customAttributes)
            //    {
            //        if (customAttribute.GetType() == typeof(InstructionRegisterAttribute))
            //        {
            //            Instruction instructionInstance = (Instruction)Activator.CreateInstance(type);

            //            if (instructionInstance != null) {
            //                instructionsRegistered += 1;
            //                RegisterInstruction(instructionInstance);
            //                Console.WriteLine($"Adding to instruction register: {nameof(instructionInstance)} : {instructionInstance.hexCode}");
            //            }
            //        }
            //    }
            //}

            //Console.WriteLine($"Registered {instructionsRegistered} instructions");

         

            //Codes from: https://www.pagetable.com/c64ref/6502/?tab=2#BCS

            instructions[0x69] = new ADC_Instruction(this, "ADC", 0x69, AddressingMode.IMM, 2, 2);
            instructions[0x6D] = new ADC_Instruction(this, "ADC", 0x6D, AddressingMode.ABS, 3, 4);
            instructions[0x7D] = new ADC_Instruction(this, "ADC", 0x7D, AddressingMode.ABSX, 3, 4);
            instructions[0x79] = new ADC_Instruction(this, "ADC", 0x79, AddressingMode.ABSY, 3, 4);
            instructions[0x65] = new ADC_Instruction(this, "ADC", 0x65, AddressingMode.ZP, 2,3);
            instructions[0x75] = new ADC_Instruction(this, "ADC", 0x75, AddressingMode.ZPX, 2, 4);
            instructions[0x61] = new ADC_Instruction(this, "ADC", 0x61, AddressingMode.INDX, 2, 6);
            instructions[0x71] = new ADC_Instruction(this, "ADC", 0x71, AddressingMode.INDY, 2, 5);

            instructions[0x29] = new AND_Instruction(this, "AND", 0x29, AddressingMode.IMM, 2, 2);
            instructions[0x2D] = new AND_Instruction(this, "AND", 0x2D, AddressingMode.ABS, 3, 4);
            instructions[0x3D] = new AND_Instruction(this, "AND", 0x3D, AddressingMode.ABSX, 3, 4);
            instructions[0x39] = new AND_Instruction(this, "AND", 0x39, AddressingMode.ABSY, 3, 4);
            instructions[0x25] = new AND_Instruction(this, "AND", 0x25, AddressingMode.ZP, 2, 3);
            instructions[0x35] = new AND_Instruction(this, "AND", 0x35, AddressingMode.ZPX, 2, 4);
            instructions[0x21] = new AND_Instruction(this, "AND", 0x21, AddressingMode.INDX, 2, 6);
            instructions[0x31] = new AND_Instruction(this, "AND", 0x31, AddressingMode.INDY, 2, 5);

            instructions[0x0A] = new ASL_Instruction(this, "ASL", 0x0A, AddressingMode.Accum, 1, 2);
            instructions[0x0E] = new ASL_Instruction(this, "ASL", 0x0E, AddressingMode.ABS, 3, 6);
            instructions[0x1E] = new ASL_Instruction(this, "ASL", 0x1E, AddressingMode.ABSX, 3, 7);
            instructions[0x06] = new ASL_Instruction(this, "ASL", 0x06, AddressingMode.ZP, 2, 5);
            instructions[0x16] = new ASL_Instruction(this, "ASL", 0x16, AddressingMode.ZPX, 2, 6);

            instructions[0x90] = new BCC_Instruction(this, "BCC", 0x90, AddressingMode.Relative, 2, 2);

            instructions[0xB0] = new BCS_Instruction(this, "BCS", 0xB0, AddressingMode.Relative, 2, 2);

            instructions[0xF0] = new BEQ_Instruction(this, "BEQ", 0xF0, AddressingMode.Relative, 2, 2);

            instructions[0x2C] = new BIT_Instruction(this, "BIT", 0x2C, AddressingMode.ABS, 3, 4);
            instructions[0x24] = new BIT_Instruction(this, "BIT", 0x24, AddressingMode.ZP, 2, 3);

            instructions[0x30] = new BMI_Instruction(this, "BMI", 0x30, AddressingMode.Relative, 2, 2);

            instructions[0xD0] = new BNE_Instruction(this, "BNE", 0xD0, AddressingMode.Relative, 2, 2);

            instructions[0x10] = new BPL_Instruction(this, "BPL", 0x10, AddressingMode.Relative, 2, 2);

            instructions[0x00] = new BRK_Instruction(this, "BRK", 0x00, AddressingMode.Implied, 1, 7);

            instructions[0x50] = new BVC_Instruction(this, "BVC", 0x50, AddressingMode.Relative, 2, 2);

            instructions[0x70] = new BVS_Instruction(this, "BVS", 0x70, AddressingMode.Relative, 2, 2);

            instructions[0x18] = new CLC_Instruction(this, "CLC", 0x18, AddressingMode.Implied, 1, 2);

            instructions[0x58] = new CLI_Instruction(this, "CLI", 0x58, AddressingMode.Implied, 1, 2);

            instructions[0xB8] = new CLV_Instruction(this, "CLV", 0xB8, AddressingMode.Implied, 1, 2);

            instructions[0xC9] = new CMP_Instruction(this, "CMP", 0xC9, AddressingMode.IMM, 2, 2);
            instructions[0xCD] = new CMP_Instruction(this, "CMP", 0xCD, AddressingMode.ABS, 3, 4);
            instructions[0xDD] = new CMP_Instruction(this, "CMP", 0xDD, AddressingMode.ABSX, 3, 4);
            instructions[0xD9] = new CMP_Instruction(this, "CMP", 0xD9, AddressingMode.ABSY, 3, 4);
            instructions[0xC5] = new CMP_Instruction(this, "CMP", 0xC5, AddressingMode.ZP, 3, 4);
            instructions[0xD5] = new CMP_Instruction(this, "CMP", 0xD5, AddressingMode.ZPX, 3, 4);
            instructions[0xC1] = new CMP_Instruction(this, "CMP", 0xC1, AddressingMode.INDX, 3, 4);
            instructions[0xD1] = new CMP_Instruction(this, "CMP", 0xD1, AddressingMode.INDY, 3, 4);

            instructions[0xE0] = new CPX_Instruction(this, "CPX", 0xE0, AddressingMode.IMM, 2, 2);
            instructions[0xEC] = new CPX_Instruction(this, "CPX", 0xEC, AddressingMode.ABS, 3, 4);
            instructions[0xE4] = new CPX_Instruction(this, "CPX", 0xE4, AddressingMode.ZP, 2, 3);

            instructions[0xC0] = new CPY_Instruction(this, "CPY", 0xC0, AddressingMode.IMM, 2, 2);
            instructions[0xCC] = new CPY_Instruction(this, "CPY", 0xCC, AddressingMode.ABS, 3, 4);
            instructions[0xC4] = new CPY_Instruction(this, "CPY", 0xC4, AddressingMode.ZP, 2, 3);

            instructions[0xCE] = new DEC_Instruction(this, "DEC", 0xCE, AddressingMode.ABS, 3, 6);
            instructions[0xDE] = new DEC_Instruction(this, "DEC", 0xDE, AddressingMode.ABSX, 3, 7);
            instructions[0xC6] = new DEC_Instruction(this, "DEC", 0xC6, AddressingMode.ZP, 2, 5);
            instructions[0xD6] = new DEC_Instruction(this, "DEC", 0xD6, AddressingMode.ZPX, 2, 6);

            instructions[0xCA] = new DEX_Instruction(this, "DEX", 0xCA, AddressingMode.Implied, 1, 2);

            instructions[0x88] = new DEY_Instruction(this, "DEY", 0x88, AddressingMode.Implied, 1, 2);

            instructions[0x49] = new EOR_Instruction(this, "EOR", 0x49, AddressingMode.IMM, 2, 2);
            instructions[0x4D] = new EOR_Instruction(this, "EOR", 0x4D, AddressingMode.ABS, 3, 4);
            instructions[0x5D] = new EOR_Instruction(this, "EOR", 0x5D, AddressingMode.ABSX, 3, 4);
            instructions[0x59] = new EOR_Instruction(this, "EOR", 0x59, AddressingMode.ABSY, 3, 4);
            instructions[0x45] = new EOR_Instruction(this, "EOR", 0x45, AddressingMode.ZP, 2, 3);
            instructions[0x55] = new EOR_Instruction(this, "EOR", 0x55, AddressingMode.ZPX, 2, 4);
            instructions[0x41] = new EOR_Instruction(this, "EOR", 0x41, AddressingMode.INDX, 2,6);
            instructions[0x51] = new EOR_Instruction(this, "EOR", 0x51, AddressingMode.INDY, 2, 5);

            instructions[0xEE] = new INC_Instruction(this, "INC", 0xEE, AddressingMode.ABS, 3, 6);
            instructions[0xFE] = new INC_Instruction(this, "INC", 0xFE, AddressingMode.ABSX, 3, 7);
            instructions[0xE6] = new INC_Instruction(this, "INC", 0xE6, AddressingMode.ZP, 2, 5);
            instructions[0xF6] = new INC_Instruction(this, "INC", 0xF6, AddressingMode.ZPX, 2, 6);

            instructions[0xE8] = new INX_Instruction(this, "INX", 0xE8, AddressingMode.Implied, 1, 2);
            
            instructions[0xC8] = new INY_Instruction(this, "INY", 0xC8, AddressingMode.Implied, 1, 2);

            instructions[0x4C] = new JMP_Instruction(this, "JMP", 0x4C, AddressingMode.ABS, 3, 3);
            instructions[0x6C] = new JMP_Instruction(this, "JMP", 0x6C, AddressingMode.Indirect, 3, 5);

            instructions[0x20] = new JSR_Instruction(this, "JSR", 0x20, AddressingMode.ABS, 3, 6);

            instructions[0xA9] = new LDA_Instruction(this, "LDA", 0xA9, AddressingMode.IMM, 2, 2);
            instructions[0xAD] = new LDA_Instruction(this, "LDA", 0xAD, AddressingMode.ABS, 3, 4);
            instructions[0xBD] = new LDA_Instruction(this, "LDA", 0xBD, AddressingMode.ABSX, 3, 4);
            instructions[0xB9] = new LDA_Instruction(this, "LDA", 0xB9, AddressingMode.ABSY, 3, 4);
            instructions[0xA5] = new LDA_Instruction(this, "LDA", 0xA5, AddressingMode.ZP, 2, 3);
            instructions[0xB5] = new LDA_Instruction(this, "LDA", 0xB5, AddressingMode.ZPX, 2, 4);
            instructions[0xA1] = new LDA_Instruction(this, "LDA", 0xA1, AddressingMode.INDX, 2, 6);
            instructions[0xB1] = new LDA_Instruction(this, "LDA", 0xB1, AddressingMode.INDY, 2, 5);

            instructions[0xA2] = new LDX_Instruction(this, "LDX", 0xA2, AddressingMode.IMM, 2, 2);
            instructions[0xAE] = new LDX_Instruction(this, "LDX", 0xAE, AddressingMode.ABS, 3, 4);
            instructions[0xBE] = new LDX_Instruction(this, "LDX", 0xBE, AddressingMode.ABSY, 3, 4);
            instructions[0xA6] = new LDX_Instruction(this, "LDX", 0xA6, AddressingMode.ZP, 2, 3);
            instructions[0xB6] = new LDX_Instruction(this, "LDX", 0xB6, AddressingMode.ZPY, 2, 4);

            instructions[0xA0] = new LDY_Instruction(this, "LDY", 0xA0, AddressingMode.IMM, 2, 2);
            instructions[0xAC] = new LDY_Instruction(this, "LDY", 0xAC, AddressingMode.ABS, 3, 4);
            instructions[0xBC] = new LDY_Instruction(this, "LDY", 0xBC, AddressingMode.ABSX , 3, 4);
            instructions[0xA4] = new LDY_Instruction(this, "LDY", 0xA4, AddressingMode.ZP, 2, 3);
            instructions[0xB4] = new LDY_Instruction(this, "LDY", 0xB4, AddressingMode.ZPX, 2, 4);

            instructions[0x4A] = new LSR_Instruction(this, "LSR", 0x4A, AddressingMode.Accum, 1, 2);
            instructions[0x4E] = new LSR_Instruction(this, "LSR", 0x4E, AddressingMode.ABS, 3, 6);
            instructions[0x5E] = new LSR_Instruction(this, "LSR", 0x5E, AddressingMode.ABSX, 3, 7);
            instructions[0x46] = new LSR_Instruction(this, "LSR", 0x46, AddressingMode.ZP, 2, 5);
            instructions[0x56] = new LSR_Instruction(this, "LSR", 0x56, AddressingMode.ZPX, 2, 6);

            instructions[0xEA] = new NOP_Instruction(this, "NOP", 0xEA, AddressingMode.Implied, 1, 2);

            instructions[0x09] = new ORA_Instruction(this, "ORA", 0x09, AddressingMode.IMM, 2, 2);
            instructions[0x0D] = new ORA_Instruction(this, "ORA", 0x0D, AddressingMode.ABS, 3, 4);
            instructions[0x1D] = new ORA_Instruction(this, "ORA", 0x1D, AddressingMode.ABSX, 3, 4);
            instructions[0x19] = new ORA_Instruction(this, "ORA", 0x19, AddressingMode.ABSY, 3, 4);
            instructions[0x05] = new ORA_Instruction(this, "ORA", 0x05, AddressingMode.ZP, 2, 3);
            instructions[0x15] = new ORA_Instruction(this, "ORA", 0x15, AddressingMode.ZPX, 2, 4);
            instructions[0x01] = new ORA_Instruction(this, "ORA", 0x01, AddressingMode.INDX, 2, 6);
            instructions[0x11] = new ORA_Instruction(this, "ORA", 0x11, AddressingMode.INDY, 2, 5);

            instructions[0x48] = new PHA_Instruction(this, "PHA", 0x48, AddressingMode.Implied, 1, 3);

            instructions[0x08] = new PHP_Instruction(this, "PHP", 0x08, AddressingMode.Implied, 1, 3);

            instructions[0x68] = new PLA_Instruction(this, "PLA", 0x68, AddressingMode.Implied, 1, 4);

            instructions[0x28] = new PLP_Instruction(this, "PLP", 0x28, AddressingMode.Implied, 1, 4);

            instructions[0x2A] = new ROL_Instruction(this, "ROL", 0x2A, AddressingMode.Accum, 1, 2);
            instructions[0x2E] = new ROL_Instruction(this, "ROL", 0x2E, AddressingMode.ABS, 3, 6);
            instructions[0x3E] = new ROL_Instruction(this, "ROL", 0x3E, AddressingMode.ABSX, 3, 7);
            instructions[0x26] = new ROL_Instruction(this, "ROL", 0x26, AddressingMode.ZP, 2, 5);
            instructions[0x36] = new ROL_Instruction(this, "ROL", 0x36, AddressingMode.ZPX, 2, 6);

            instructions[0x6A] = new ROR_Instruction(this, "ROR", 0x6A, AddressingMode.Accum, 1, 2);
            instructions[0x6E] = new ROR_Instruction(this, "ROR", 0x6E, AddressingMode.ABS, 3, 6);
            instructions[0x7E] = new ROR_Instruction(this, "ROR", 0x7E, AddressingMode.ABSX, 3, 7);
            instructions[0x66] = new ROR_Instruction(this, "ROR", 0x66, AddressingMode.ZP, 2, 5);
            instructions[0x76] = new ROR_Instruction(this, "ROR", 0x76, AddressingMode.ZPX, 2, 6);

            instructions[0x40] = new RTI_Instruction(this, "RTI", 0x40, AddressingMode.Implied, 1, 6);

            instructions[0x60] = new RTS_Instruction(this, "RTS", 0x60, AddressingMode.Implied, 1, 6);

            instructions[0xE9] = new SBC_Instruction(this, "SBC", 0xE9, AddressingMode.IMM, 2, 2);
            instructions[0xED] = new SBC_Instruction(this, "SBC", 0xED, AddressingMode.ABS, 3, 4);
            instructions[0xFD] = new SBC_Instruction(this, "SBC", 0xFD, AddressingMode.ABSX, 3, 4);
            instructions[0xF9] = new SBC_Instruction(this, "SBC", 0xF9, AddressingMode.ABSY, 3, 4);
            instructions[0xE5] = new SBC_Instruction(this, "SBC", 0xE5, AddressingMode.ZP, 2, 3);
            instructions[0xF5] = new SBC_Instruction(this, "SBC", 0xF5, AddressingMode.ZPX, 2, 4);
            instructions[0xE1] = new SBC_Instruction(this, "SBC", 0xE1, AddressingMode.INDX, 2, 6);
            instructions[0xF1] = new SBC_Instruction(this, "SBC", 0xF1, AddressingMode.INDY, 2, 5);

            instructions[0x38] = new SEC_Instruction(this, "SEC", 0x38, AddressingMode.Implied, 1, 2);

            instructions[0xF8] = new SED_Instruction(this, "SED", 0xF8, AddressingMode.Implied, 1, 2);

            instructions[0x78] = new SEI_Instruction(this, "SEI", 0x78, AddressingMode.Implied, 1, 2);

            instructions[0x8D] = new STA_Instruction(this, "STA", 0x8D, AddressingMode.ABS, 3, 4);
            instructions[0x9D] = new STA_Instruction(this, "STA", 0x9D, AddressingMode.ABSX, 3, 5);
            instructions[0x99] = new STA_Instruction(this, "STA", 0x99, AddressingMode.ABSY, 3, 5);
            instructions[0x85] = new STA_Instruction(this, "STA", 0x85, AddressingMode.ZP, 2, 3);
            instructions[0x95] = new STA_Instruction(this, "STA", 0x95, AddressingMode.ZPX, 2, 4);
            instructions[0x81] = new STA_Instruction(this, "STA", 0x81, AddressingMode.INDX, 2, 6);
            instructions[0x91] = new STA_Instruction(this, "STA", 0x91, AddressingMode.INDY, 2, 6);

            instructions[0x8E] = new STX_Instruction(this, "STX", 0x8E, AddressingMode.ABS, 3, 4);
            instructions[0x86] = new STX_Instruction(this, "STX", 0x86, AddressingMode.ZP, 2, 3);
            instructions[0x96] = new STX_Instruction(this, "STX", 0x96, AddressingMode.ZPY, 2, 4);

            instructions[0x8C] = new STY_Instruction(this, "STY", 0x8C, AddressingMode.ABS, 3, 4);
            instructions[0x84] = new STY_Instruction(this, "STY", 0x84, AddressingMode.ZP, 2, 3);
            instructions[0x94] = new STY_Instruction(this, "STY", 0x94, AddressingMode.ZPX, 2, 4);

            instructions[0xAA] = new TAX_Instruction(this, "TAX", 0xAA, AddressingMode.Implied, 1, 2);

            instructions[0xA8] = new TAY_Instruction(this, "TAY", 0xA8, AddressingMode.Implied, 1, 2);

            instructions[0xBA] = new TSX_Instruction(this, "TSX", 0xBA, AddressingMode.Implied, 1, 2);

            instructions[0x8A] = new TXA_Instruction(this, "TXA", 0x8A, AddressingMode.Implied, 1, 2);

            instructions[0x9A] = new TXS_Instruction(this, "TXS", 0x9A, AddressingMode.Implied, 1, 2);

            instructions[0x98] = new TYA_Instruction(this, "TYA", 0x98, AddressingMode.Implied, 1, 2);

        }

        public ushort Read16(ushort address)
        {
            byte pcl = addressBus.ReadByte(address);
            ushort pchAddress = (ushort)(address + Convert.ToUInt16(1));
            byte pch = addressBus.ReadByte(pchAddress);

            ushort result = pch;
            result = (ushort)(result << 8);
            result += pcl;

            return result;
        }

        public ushort Read16()
        {
            //Based on Little Endian ordering
            byte pcl = addressBus.ReadByte(PC);
            IncrementPC();
            byte pch = addressBus.ReadByte(PC);

            ushort result = pch;
            result = (ushort)(result << 8);
            result += pcl;

            return result;
        }

        public void PushStack(byte value)
        {
            //Stack always pn page 1
            ushort stackAddress = (ushort)(0x0100 + S);
            addressBus.WriteByte(value, stackAddress);
            S -= 0x01; // minus 1

            //Handle the wrap around
            if (S < 0x00)
            {
                S = 0xFF; 
            }
        }

        public byte PopStack()
        {
            ushort stackAddress = (ushort)(0x0100 + S);

            //Handle the wrap around

            byte value = addressBus.ReadByte(stackAddress);

            S += 0x01; //add 1

            //Handle the wrap around
            if (S > 0xFF) { S = 0x00; }

            return value;
        }

        public void SetPC(ushort value)
        {
            PC = value;
        }

        public void IncrementPC()
        {
            //do wrap around check
            unchecked
            {
                PC += 1;
            }
        }

        public void PowerUp()
        {
            this.P = 0x34;
            this.A = 0x0;
            this.X = 0x0;
            this.Y = 0x0;
            this.S = 0x00;

            this.addressBus.WriteByte(0x00, 0x4017); //frame irq enabled
            this.addressBus.WriteByte(0x00, 0x4015); //all channels disabled

            for (ushort i = 0x4000; i <= 0x400F; i += 0x0001)
            {
                this.addressBus.WriteByte(0x00, i);
            }

            for (ushort i = 0x4010; i <= 0x4013; i += 0x0001)
            {
                this.addressBus.WriteByte(0x00, i);
            }

            //Todo do we need reset procedure - how is the stack pointer initialised??
        }

        public void Reset()
        {
            //Interupt disable flag is set
            SetProcessorStatusFlag(true, StatusFlagName.IRQDisable);
            //Load program vector locations
            ushort pc = Read16(0xFFFC);
            this.SetPC(pc);
        }

        public void Tick()
        {
            //Get next instruction from location at the program counter -- expecting this to have been set by the program counter I guess
            byte nextInstruction = this.addressBus.ReadByte(PC);

            //Reset timing control
            TimingControl = 0;

            //get the instruction
            Instruction instruction = instructions[nextInstruction];

            //Execute the instruction
            instruction.Execute(this);

            //have to split that into 2 parts - the most significant digit and the least significant digit
            //8 bits into 2 halfs 4 bits each
            //byte msd = 


            //execute the instruction

            //Interpret the instruction

            //program counter increments by 1


            //next next
        }

        internal void SetTimingControl(int machineCycles)
        {
            TimingControl = machineCycles; //2 as instruction takes 2 machine cycles
        }

        public bool IsByteNegative(byte result)
        {
            //Set negative flag
            //Set by result bit 7 --to do check this vs signed logic??
            bool isByteNegative = Convert.ToBoolean((result & 0b10000000) >> 7);
            return isByteNegative;
        }

        

        public FetchResult Fetch(AddressingMode addressingMode)
        {
            //Fetches based on the addressing mode
            //Based on having read the first operand as the opcode - so assuming the PC is set at this point still

            byte operand = 0;
            int pageCross = 0;
            ushort address = 0;
            byte lb = 0;
            byte hb = 0;


            switch (addressingMode)
            {
                case AddressingMode.Implied: //1 byte instruction
                    //IncrementPC();
                    address = PC;
                    operand = addressBus.ReadByte(PC);
                    break;
                case AddressingMode.Accum: //1 byte instruction
                    //IncrementPC();
                    operand = A;
                    break;
                case AddressingMode.IMM: //2 byte instruction
                    IncrementPC();
                    address = PC;
                    operand = addressBus.ReadByte(PC);
                    break;
                case AddressingMode.ABS: //3 byte instruction
                    IncrementPC();
                    address = Read16();
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ABSX:  //3 byte instruction
                    IncrementPC();
                    address = Read16();
                    if ((ushort)(address + (ushort)X) >> 7 > address >> 8)
                        pageCross = 1;
                    address = (ushort)(address + (ushort)X);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ABSY: //3 byte instruction
                    IncrementPC();
                    address = Read16();
                    if ((ushort)(address + (ushort)Y) >> 7 > address >> 8)
                        pageCross = 1;
                    address = (ushort)(address + (ushort)Y);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ZP: //2 byte instruction
                    IncrementPC();
                    address = (ushort)addressBus.ReadByte(PC);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ZPX: //2 byte instruction
                    IncrementPC();
                    address = (ushort)((ushort)addressBus.ReadByte(PC) + (ushort)X);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ZPY: //2 byte instruction
                    IncrementPC();
                    address = (ushort)((ushort)addressBus.ReadByte(PC) + (ushort)Y);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.INDX:
                    IncrementPC();
                    byte indx = (byte)(addressBus.ReadByte(PC) + X);
                    lb = addressBus.ReadByte(indx);
                    hb = addressBus.ReadByte((ushort)(indx+1));
                    address = (ushort)((hb << 8) + lb);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.INDY:
                    IncrementPC();
                    byte indy = (byte)(addressBus.ReadByte(PC) + Y);
                    lb = addressBus.ReadByte(indy);
                    hb = addressBus.ReadByte((ushort)(indy + 1));
                    address = (ushort)((hb << 8) + lb);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.Relative:
                    IncrementPC();
                    address = PC;
                    operand = addressBus.ReadByte(PC);
                    break;
            }

            return new FetchResult(operand, pageCross, address);
        }
    }
}

