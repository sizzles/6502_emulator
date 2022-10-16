using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SDL2;
using System.Runtime.InteropServices;

namespace Chip8
{
    //References
    //http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.5

    public class Ram
    {
        //Ram is big endian - most significant byte is stored first
        public byte[] memory;

        public Ram()
        {  
            this.memory = new byte[4096];
            //Init all to zeros
            for (var i = 0x0000; i <= 0xFFF; i += 0x0001)
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

    public class Cpu
    {
        Ram ram = new Ram();
        Random random = new Random(0); 
        
        byte[] registers = new byte[16]; //defaults to 0
        ushort[] stack = new ushort[16]; //16 x 16 stack

        //v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, va, vb, vc, vd, ve, vf

        //16 8 BIT REGISTERS
        //public byte V0 = 0;
        //public byte V1 = 1;
        //public byte V2 = 2;
        //public byte V3 = 3;
        //public byte V4 = 4;
        //public byte V5 = 5;
        //public byte V6 = 6;
        //public byte V7 = 7;
        //public byte V8 = 8;
        //public byte V9 = 9;
        //public byte VA = 10;
        //public byte VB = 11;
        //public byte VC = 12;
        //public byte VD = 13;
        //public byte VE = 14;
        //public byte VF = 15;
        
        public byte vf = 0; //flag register

        public byte dt = 0; //delay timer
        public byte st = 0; //sound timer

        public ushort I = 0; //generally used to store memory addresses
        public ushort pc = 0;//program counter
        public byte sp = 0; //stack pointer

        const int displayRows = 32;
        const int displayCols = 64;
        const int spriteCols = 8; //always have 8 columns

        public bool[,] display = new bool[displayRows, displayCols]; //rows x columns
        public bool[] keyboard = new bool[16]; //1 = key down, 0 = key up

        public ushort insMask = 0b0000000000001111;

        public IntPtr wind = IntPtr.Zero;
        public IntPtr surf = IntPtr.Zero;

        public int displayScale = 10;

        public object dtLock = new object();
        public object stLock = new object();

        private SDL.SDL_Event e;

        private byte[,] tile_colours = new byte[2,3] { { 17, 16, 18 }, { 194, 179, 201 } }; //rows x cols bg/fg

        public string romPath = @"C:\tmp\tetris.ch8";
        public Cpu()
        {
   
            InitRendering();
            LoadCharSpites();
            LoadRom();
            Reset();

            // called once, to get buffer pointer
            //keysBuffer = SDL.SDL_GetKeyboardState(out _numkeys);


            //Delay Timer
            new Thread(() =>
            {
                while (true)

                {
                    lock (dtLock)
                    {
                        if (dt > 0)
                        {
                            dt -= 1;
                        }
                    }
                    Thread.Sleep(16); //sleep roughly at 60hz
                }
            }).Start();

            //Sound Timer 
            new Thread(() =>
            {
                while (true)
                {
                    bool beep = false;
                    lock (stLock)
                    {
                        if (st > 0)
                        {
                            st -= 1;
                            beep = true;
                        }
                    }

                    if (beep)
                    {
                        Console.Beep(800, 16);
                    }
                    else
                    {
                        Thread.Sleep(16); //sleep roughly at 60hz
                    }
                    
                }
            }).Start();

            Tick();
        }

        public void InitRendering()
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            wind = SDL2.SDL.SDL_CreateWindow("Chip-8 Emulator", 200, 200, displayCols * displayScale, displayRows * displayScale, 0);
            surf = SDL.SDL_GetWindowSurface(wind);
        }

        public unsafe void Render()
        {
            var rect = new SDL.SDL_Rect();
            rect.x = 0;
            rect.y = 0;
            rect.h = 1 * displayScale;
            rect.w = 1 * displayScale;

            byte color_row = 0;

            for (int r = 0; r < displayRows; r++)
            {
                for (int c =0; c < displayCols; c++)
                {
                    color_row = Convert.ToByte(display[r, c]);
                    
                    rect.x = c * displayScale;
                    rect.y = r * displayScale;

                    SDL.SDL_FillRect((System.IntPtr)surf, ref rect, SDL.SDL_MapRGB(((SDL.SDL_Surface*)surf)->format, tile_colours[color_row, 0], tile_colours[color_row, 1], tile_colours[color_row, 2]));
                }
            }

            SDL.SDL_UpdateWindowSurface(wind);
        }

        public void LoadCharSpites()
        {
            var zero = new byte[] { 0xF0, 0x90, 0x90, 0x90, 0xF0 };
            var one = new byte[] { 0x20, 0x60, 0x20, 0x20, 0x70 };
            var two = new byte[] { 0xF0, 0x10, 0xF0, 0x80, 0xF0 };
            var three = new byte[] { 0xF0, 0x10, 0xF0, 0x10, 0xF0 };
            var four = new byte[] { 0x90, 0x90, 0xF0, 0x10, 0x10 };
            var five = new byte[] { 0xF0, 0x80, 0xF0, 0x10, 0xF0 };
            var six = new byte[] { 0xF0, 0x80, 0xF0, 0x90, 0xF0 };
            var seven = new byte[] { 0xF0, 0x10, 0x20, 0x40, 0x40 };
            var eight = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0xF0 };
            var nine = new byte[] { 0xF0, 0x90, 0xF0, 0x10, 0xF0 };
            var a = new byte[] { 0xF0, 0x90, 0xF0, 0x90, 0x90 };
            var b = new byte[] { 0xE0, 0x90, 0xE0, 0x90, 0xE0 };
            var c = new byte[] { 0xF0, 0x80, 0x80, 0x80, 0xF0 };
            var d = new byte[] { 0xE0, 0x90, 0x90, 0x90, 0xE0 };
            var e = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0xF0 };
            var f = new byte[] { 0xF0, 0x80, 0xF0, 0x80, 0x80 };

            var charSprites = new List<byte[]>() {zero, one, two, three, four, five, six, seven, eight, nine, a, b, c, d, e, f};
            

            ushort i = 0; 
            //Load all into memory starting at - 
            foreach(var charSprite in charSprites)
            {
                foreach(var row in charSprite)
                {
                    ram[i] = row;
                    i++;
                }
            }
        }

        public void LoadRom()
        {
            ushort pgmStart = 0x200;

            using (FileStream fs = new FileStream(romPath, FileMode.Open))
            {
                fs.Seek(0, SeekOrigin.Begin);

                for (int i =0; i < fs.Length; i++)
                {
                    byte romByte = (byte)fs.ReadByte();
                    ram[(ushort)(pgmStart + i)] = romByte;

                }
            }

        }

        public void Reset()
        {
            pc = 0x200; //set program counter to 512
        }

        private void CLS()
        {
            Array.Clear(display);
        }

        private void RET() //get what is in memory at the stack pointer - decrement the stack pointer by 1
        {
            pc = stack[sp];
            sp = (byte)(sp - 1);
        }

        public ushort nnnToAdr(byte i2, byte i3, byte i4) //Converts 3 sets of bytes into a 12 bit addr in aushort
        {
            return (ushort)((i2 << 8) + (i3 << 4) + i4);
        }

        public void Tick()
        {
            while (true) {

                Thread.Sleep(1);

                while(SDL.SDL_PollEvent(out e) != 0)
                {
                    //Console.WriteLine($"Event: {e.type}");
                    if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                    {
                        //Console.WriteLine($"KEY DOWN: {e.key.keysym.sym}");

                        switch(e.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_1:
                                keyboard[1] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_2:
                                keyboard[2] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_3:
                                keyboard[3] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_4:
                                keyboard[12] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_q:
                                keyboard[4] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_w:
                                keyboard[5] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_e:
                                keyboard[6] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_r:
                                keyboard[13] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_a:
                                keyboard[7] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_s:
                                keyboard[8] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_d:
                                keyboard[9] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_f:
                                keyboard[14] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_z:
                                keyboard[10] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_x:
                                keyboard[0] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_c:
                                keyboard[11] = true;
                                break;
                            case SDL.SDL_Keycode.SDLK_v:
                                keyboard[15] = true;
                                break;

                            default:
                                break;
                        }
                    }

                    else if (e.type == SDL.SDL_EventType.SDL_KEYUP)
                    {
                        //Console.WriteLine($"KEY UP: {e.key.keysym.sym}");

                        switch (e.key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_1:
                                keyboard[1] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_2:
                                keyboard[2] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_3:
                                keyboard[3] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_4:
                                keyboard[0xc] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_q:
                                keyboard[4] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_w:
                                keyboard[5] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_e:
                                keyboard[6] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_r:
                                keyboard[0xd] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_a:
                                keyboard[7] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_s:
                                keyboard[8] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_d:
                                keyboard[9] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_f:
                                keyboard[0xe] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_z:
                                keyboard[0xa] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_x:
                                keyboard[0] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_c:
                                keyboard[0xb] = false;
                                break;
                            case SDL.SDL_Keycode.SDLK_v:
                                keyboard[0xf] = false;
                                break;

                            default:
                                break;
                        }
                    }
                }

                //instructions most significant byte first -- made of two instructions
                byte msb = ram[this.pc];
                byte lsb = ram[(ushort)(this.pc + 1)];

                ushort ins = (ushort)((msb << 8) + lsb);
                pc += 2; //increment program counter

                //instructions in the format CXYN, CXNN, CNNN
                //decode by grouping up the instruction

                //c = code group -- each character is 4 bits x 4 = 16
                //x + y = register numbers
                //N, NN, NNN = literal numbers

                //have to use bytes as we dont have a nibble natively in c#


                byte c = Convert.ToByte(ins >> 12);
                byte i2 = (byte)((ins >> 8) & insMask);

                byte i3 = (byte)((ins >> 4) & insMask);
                byte i4 = (byte)(ins & insMask);

                byte x = i2;
                byte y = i3;

                //Grouping
                if (c == 0) { 
                    if (i2 == 0 && i3 == 0xe)
                    {
                        if (i4 == 0) //00e0 CLS - display clear
                        {
                            this.CLS();
                        }
                        else if(i4 == 0xe) //00ee RET - flow return
                        {
                            this.RET();
                        }
                    }

                    else
                    {
                        //0nnn SYS addr - ignored now by roms
                    }
                }

                else if (c == 1) //goto NNN
                {
                    ushort addr = nnnToAdr(i2, i3, i4);
                    pc = addr;
                }
                else if (c == 2) //call subroute at NNN
                {
                    sp += 1;
                    stack[sp] = pc;
                    ushort addr = nnnToAdr(i2, i3, i4);
                    pc = addr;
                }
                else if (c == 3) //SE Vx, byte Skip next instruction if Vx = kk.
                {
                    if (registers[x] == lsb)
                    {
                        pc += 2;
                    }
                }
                else if (c == 4) // SNE Vx, byte Skip next instruction if Vx != kk.
                {
                    if (registers[x] != lsb)
                    {
                        pc += 2;
                    }
                }
                else if (c == 5) //Skip next instruction if Vx = Vy.
                {
                    if (registers[x] == registers[y])
                    {
                        pc += 2;
                    }
                }
                else if (c == 6) //LD Vx, byte
                {
                    registers[x] = lsb;
                }
                else if (c == 7) //ADD Vx, byte  Set Vx = Vx + kk.
                {
                    registers[x] = (byte)(registers[x] + lsb);
                }
                else if (c == 8)
                {
                    if (i4 == 0) // LD Vx, Vy  - Set Vx = Vy.
                    {
                        registers[x] = registers[y];
                    }
                    else if (i4 == 1) //OR Vx, Vy
                    {
                        registers[x] = (byte)(registers[x] | registers[y]);
                    }
                    else if (i4 == 2) //AND Vx, Vy
                    {
                        registers[x] = (byte)(registers[x] & registers[y]);
                    }
                    else if (i4 == 3) // Vx XOR Vy.
                    {
                        registers[x] = (byte)(registers[x] ^ registers[y]);
                    }
                    else if (i4 == 4) //ADD Vx, Vy with carry
                    {
                        ushort res = (ushort)(registers[x] + registers[y]);
                        registers[0xf] = (byte)(res > 255 ? 1 : 0);
                        registers[x] = (byte)(res);
                    }
                    else if (i4 == 5) // SUB Vx, Vy vf = not borrow
                    {
                        registers[0xf] = (byte)(registers[x] > registers[y] ? 1 : 0);
                        registers[x] = (byte)(registers[x] - registers[y]);
                    }
                    else if (i4 == 6) //Set Vx = Vx SHR 1. lsb of reg x = 1
                    {
                        //If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0.
                        //Then Vx is divided by 2.
                        if ((registers[x] & 0b00000001) == 1)
                        {
                            registers[0xf] = 1;
                        }
                        else
                        {
                            registers[0xf] = 0;
                        }

                        registers[x] = (byte)(registers[x] >> 1); //divide by 2
                    }
                    else if (i4 == 7) //Set Vx = Vy - Vx, set VF = NOT borrow.
                    {
                        registers[0xf] = (byte)(registers[y] > registers[x] ? 1 : 0);
                        registers[x] = (byte)(registers[y] - registers[x]);
                    }
                    else if (i4 == 0xE) //SHL Vx {, Vy} - Set Vx = Vx SHL 1.
                    {
                        //if (((registers[x] & 0b10000000) >> 7) == 1)
                        if ((registers[x] >> 7) == 1)
                        {
                            registers[0xf] = 1;
                        }
                        else
                        {
                            registers[0xf] = 0;
                        }
                        registers[x] = (byte)(registers[x] << 1); //multiply by 2
                    }
                }
                else if (c == 9) //SNE Vx, Vy Skip next instruction if Vx != Vy.
                {
                    if (registers[x] != registers[y])
                    {
                        pc += 2;
                    }
                }

                else if (c == 0xA) //LD I, addr
                {
                    I = nnnToAdr(i2, i3, i4);
                }

                else if (c == 0xB) //Bnnn - JP V0, addr
                {
                    pc = (ushort)(nnnToAdr(i2, i3, i4) + registers[0]);
                }
                else if(c == 0xC) //RND Vx, byte - Set Vx = random byte AND kk.
                {
                    var rand = (byte)(random.Next(0, 256)); //upper bound is exclusive
                    byte res = (byte)(rand & lsb);
                    registers[x] = res;
                }
                else if(c == 0xD) //DRW Vx, Vy, nibble
                {
                    //read n byte sprite from memory
                    var bytesToRead = i4;

                    byte[] rows = new byte[bytesToRead];

                    for (int i =0; i < bytesToRead; i++)
                    {
                        rows[i] = ram[(ushort)(I + i)];
                    }

                    //Wrap around to both the top and side to side of the sprite
                    //check for the overlaps? - I guess cant set anything to minus 1 so only ahve to worry about 2 directions of overlap
                    //render the sprite to the string
                    //x = cols, y = rows
                    byte flippedBit = 0;

                    for (byte i = 0; i < bytesToRead; i ++)
                    {
                        //screen = rows x cols
                        //sprite = rows x cols
                        //loop through cols
                        var row = rows[i];

                        for (byte p = 0; p < 8; p++)
                        {
                            //through each byte type character
                            //check the location

                            //going through each pixel in the row

                            var colOverlap = registers[x] + p;
                            var rowOverlap = registers[y] + i;

                            //if we have gone out of bounds then just continue - nothing is done

                            if (colOverlap > 63 || rowOverlap > 31)
                            {
                                continue;
                            }

                            //63, 63 + 1, (2nd pixel) = -1 - want to draw on the first line - so mod this
                            //now xor this

                            bool currentPixel = display[rowOverlap, colOverlap];
                            bool newPixel = Convert.ToBoolean(((row >> (7 - p)) & 0b00000001));

                            bool result = newPixel ^ currentPixel;

                            if (currentPixel == true && result == false)
                            {
                                flippedBit = 1;
                            }

                            display[rowOverlap, colOverlap] = result;

                        }
                    }

                    registers[0xf] = flippedBit;
                    Render();
                }
                else if (c == 0xE)
                {
                    if (i3 == 9) // Ex9E - SKP Vx
                    {
                        if (keyboard[registers[x]]) 
                        {
                            pc += 2;
                        }
                    }
                    else if (i3 == 0xA) //ExA1 - SKNP Vx - 
                    //Skip next instruction if key with the value of Vx is not pressed.
                    {
                        if (!keyboard[registers[x]])
                        {
                            pc += 2;
                        }
                    }
                }
                else if (c == 0xF)
                {
                    if (i3 == 0 && i4 == 7) //Fx07 - LD Vx, DT
                    {
                        registers[x] = dt;
                    }
                    else if (i3 == 0 && i4 == 0xA) //Fx0A - LD Vx, K
                    {
                        //wait for a keypress ?
                        //todo
                        bool break_flag = false;
                        byte key_pressed = 0;

                        while (break_flag == false)
                        {
                            Thread.Sleep(1);
                            while (SDL.SDL_PollEvent(out e) != 0)
                            {
                                if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                                {
                                    switch (e.key.keysym.sym)
                                    {
                                        case SDL.SDL_Keycode.SDLK_1:
                                            keyboard[1] = true;
                                            break_flag = true;
                                            key_pressed = 1;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_2:
                                            keyboard[2] = true;
                                            break_flag = true;
                                            key_pressed = 2;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_3:
                                            keyboard[3] = true;
                                            break_flag = true;
                                            key_pressed = 3;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_4:
                                            keyboard[0xc] = true;
                                            break_flag = true;
                                            key_pressed = 0xc;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_q:
                                            keyboard[4] = true;
                                            break_flag = true;
                                            key_pressed = 4;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_w:
                                            keyboard[5] = true;
                                            break_flag = true;
                                            key_pressed = 5;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_e:
                                            keyboard[6] = true;
                                            break_flag = true;
                                            key_pressed = 6;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_r:
                                            keyboard[0xd] = true;
                                            break_flag = true;
                                            key_pressed = 0xd;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_a:
                                            keyboard[7] = true;
                                            break_flag = true;
                                            key_pressed = 7;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_s:
                                            keyboard[8] = true;
                                            break_flag = true;
                                            key_pressed = 8;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_d:
                                            keyboard[9] = true;
                                            break_flag = true;
                                            key_pressed = 9;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_f:
                                            keyboard[0xe] = true;
                                            break_flag = true;
                                            key_pressed = 0xe;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_z:
                                            keyboard[0xa] = true;
                                            break_flag = true;
                                            key_pressed = 0xa;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_x:
                                            keyboard[0] = true;
                                            break_flag = true;
                                            key_pressed = 0;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_c:
                                            keyboard[0xb] = true;
                                            break_flag = true;
                                            key_pressed = 0xb;
                                            break;
                                        case SDL.SDL_Keycode.SDLK_v:
                                            keyboard[0xf] = true;
                                            break_flag = true;
                                            key_pressed = 0xf;
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }


                        }

                        registers[x] = (byte)key_pressed;
                    }
                    else if (i3 == 1 && i4 == 5) //Fx15 - LD DT, Vx
                    {
                        dt = registers[x];
                    }
                    else if (i3 == 1 && i4 == 8) //Fx18 - LD ST, Vx
                    //Set sound timer = Vx.
                    {
                        st = registers[x];
                    }
                    else if (i3 == 1 && i4 == 0xE) //Fx1E - ADD I, Vx
                    {
                        I = (ushort)(I + registers[x]);
                    }
                    else if (i3 == 2 && i4 == 9)
                    {
                        //get sprite mapping todo
                        byte digit = registers[x];

                        I = (ushort)(digit * 5); //they are 5 bytes long each and we star at 0
                    }
                    else if (i3 == 3 && i4 == 3)
                    {
                        //split decimal value
                        //put them into i 

                        //1, 5, 8 
                        byte d1 = (byte)((int)((registers[x] / 100))); //1
                        byte d2 = (byte)((int)((registers[x] % 100) / 10)); //5
                        byte d3 = (byte)(registers[x] % 10); //8

                        ram[I] = d1;
                        ram[(ushort)(I + 1)] = d2;
                        ram[(ushort)(I + 2)] = d3;                        
                    }
                    else if (i3 == 5 && i4 == 5)
                    {
                        //store registers in memory - starting at I
                        for (int i = 0; i <= x; i++)
                        {
                            ram[(ushort)(I + i)] = registers[i];
                        }
                    }
                    else if (i3 == 6 && i4 == 5) //Fx65 - LD Vx, [I]
                    {
                        for (int i = 0; i <= x; i++)
                        {
                            registers[i] = ram[(ushort)(I + i)];
                        }
                    }
                }
            }
        }
    }
}