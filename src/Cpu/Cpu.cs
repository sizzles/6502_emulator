using Cpu.Instructionns;
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
        public Dictionary<Byte, Instruction> instructionMatrix;

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
            instructionMatrix = new Dictionary<Byte, Instruction>();

            this.InitInstructionRegister();
            //this.InitProcessorStatusFlags();
            this.PowerUp();
        }

        private void RegisterInstruction(Instruction instruction)
        {
            instructionMatrix[instruction.hexCode] = instruction;
        }

        private void InitInstructionRegister() {

            Assembly info = Assembly.GetExecutingAssembly();
            var assemblyTypes = info.DefinedTypes;
            int instructionsRegistered = 0;

            foreach(var type in assemblyTypes)
            {
                var customAttributes = type.GetCustomAttributes();

                foreach(var customAttribute in customAttributes)
                {
                    if (customAttribute.GetType() == typeof(InstructionRegisterAttribute))
                    {
                        Instruction instructionInstance = (Instruction)Activator.CreateInstance(type);

                        if (instructionInstance != null) {
                            instructionsRegistered += 1;
                            RegisterInstruction(instructionInstance);
                            Console.WriteLine($"Adding to instruction register: {nameof(instructionInstance)} : {instructionInstance.hexCode}");
                        }
                    }
                }
            }

            Console.WriteLine($"Registered {instructionsRegistered} instructions");

            ////Basic instruction set to start with - LDY
            //SetInstruction(new LDY_IMM_Instruction()); //Finish cycles?
            //SetInstruction(new JMP_ABS_Instruction()); //Finish cycles?
            //SetInstruction(new LDA_INDX_Instruction());//Finish cycles?
            //SetInstruction(new LDY_IMM_Instruction());//Finish cycles?
            //SetInstruction(new AND_ZP_Instruction());//Finish cycles?
            //SetInstruction(new BRK_Implied_Instruction()); //Todo Finish this!
            //SetInstruction(new ADC_ZP_Instruction()); //
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
            Instruction instruction = instructionMatrix[nextInstruction];

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

            IncrementPC();

            switch (addressingMode)
            {
                case AddressingMode.Implied: //1 byte instruction
                    address = PC;
                    operand = addressBus.ReadByte(PC);
                    break;
                case AddressingMode.Accum: //1 byte instruction
                    operand = A;
                    break;
                case AddressingMode.IMM: //2 byte instruction
                    address = PC;
                    operand = addressBus.ReadByte(PC);
                    break;
                case AddressingMode.ABS: //3 byte instruction
                    address = Read16();
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ABSX:  //3 byte instruction
                    address = Read16();
                    if ((ushort)(address + (ushort)X) >> 7 > address >> 8)
                        pageCross = 1;
                    address = (ushort)(address + (ushort)X);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ABSY: //3 byte instruction
                    address = Read16();
                    if ((ushort)(address + (ushort)Y) >> 7 > address >> 8)
                        pageCross = 1;
                    address = (ushort)(address + (ushort)Y);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ZP: //2 byte instruction
                    address = (ushort)addressBus.ReadByte(PC);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ZPX: //2 byte instruction
                    address = (ushort)((ushort)addressBus.ReadByte(PC) + (ushort)X);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.ZPY: //2 byte instruction
                    address = (ushort)((ushort)addressBus.ReadByte(PC) + (ushort)Y);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.INDX:
                    byte indx = (byte)(addressBus.ReadByte(PC) + X);
                    lb = addressBus.ReadByte(indx);
                    hb = addressBus.ReadByte((ushort)(indx+1));
                    address = (ushort)((hb << 8) + lb);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.INDY:
                    byte indy = (byte)(addressBus.ReadByte(PC) + Y);
                    lb = addressBus.ReadByte(indy);
                    hb = addressBus.ReadByte((ushort)(indy + 1));
                    address = (ushort)((hb << 8) + lb);
                    operand = addressBus.ReadByte(address);
                    break;
                case AddressingMode.Relative:
                    address = PC;
                    operand = addressBus.ReadByte(PC);
                    break;
            }

            return new FetchResult(operand, pageCross, address);
        }
    }
}

