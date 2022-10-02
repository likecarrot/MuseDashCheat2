using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MuseDash修改器
{
    public partial class Form1 : Form
    {
        private string ImageName = "GameAssembly.dll";
        private string ProcessName = "MuseDash";
        private int ProcessId = 0;
        private IntPtr ImageBase;
        private Operation operate;


        public Form1()
        {
            InitializeComponent();
        }


        //初始化函数,用来找到程序pid,以及imagebase
        private bool CheckProcess()
        {
            Process[] processes = Process.GetProcessesByName(ProcessName);
            foreach (var p in processes)
            {
                if (p.ProcessName == ProcessName)
                {
                    ProcessId = p.Id;
                    richTextBox1.Text += "找到游戏进程--" + p.Id + "\r\n";
                    for (int i = 0; i < p.Modules.Count; i++)
                    {
                        if (p.Modules[i].ModuleName.Equals(ImageName))
                        {
                            ImageBase = p.Modules[i].BaseAddress;
                            richTextBox1.Text += "找到游戏模块基址--" + p.Modules[i].BaseAddress + "\r\n";
                            return true;
                        }
                    }

                }
            }
            richTextBox1.Text += "未找到游戏进程--" + ProcessName + ".exe\r\n";
            return false;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.CloseAllEffect() == true)
                {
                    richTextBox1.Text += "关闭所有效果" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "关闭所有效果失败" + "\r\n";
                }
            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.LockHP(Operation.OPEN) == false)
                {
                    richTextBox1.Text += "开启锁血:操作失败" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "开启锁血" + "\r\n";
                }
            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.IdolSkill(Operation.OPEN) == true)
                {
                    richTextBox1.Text += "替换技能:开启" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "替换技能:开启失败" + "\r\n";
                }
            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.IdolSkill(Operation.CLOSE) == true)
                {
                    richTextBox1.Text += "替换技能:关闭" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "替换技能:关闭失败" + "\r\n";
                }
            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.LockHP(Operation.CLOSE) == false)
                {
                    richTextBox1.Text += "关闭锁血:操作失败" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "关闭锁血" + "\r\n";
                }
            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.IdolSkill(Operation.OPEN) == true)
                {
                    richTextBox1.Text += "偶像布若:开启10倍经验" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "偶像布若:开启10倍经验失败" + "\r\n";
                }
            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                if (operate.IdolSkill(Operation.CLOSE))
                {
                    richTextBox1.Text += "偶像布若:关闭10倍经验" + "\r\n";
                }
                else
                {
                    richTextBox1.Text += "偶像布若:关闭10倍经验失败" + "\r\n";
                }

            }
            catch (System.NullReferenceException)
            {
                richTextBox1.Text += "请先点击开始按钮" + "\r\n";
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void button1_begin_click(object sender, EventArgs e)
        {
            if (ProcessId == 0)
            {
                CheckProcess();
            }
            //初始化操作类
            operate = new Operation(ProcessId, ImageBase);
        }
    }



    public class Operation
    {
        //宏定义
        private const int ACCESS_ALL = 0x1F0FFF;
        public const int OPEN = 1;
        public const int CLOSE = 0;
        
        //固定偏移基址
        private const   int PART_LOCKHP1 = 0x86AE60;
        private const int PART_LOCKHP2 = 0;
        private const int PART_IDOTSKILL = 0x8DF448;

        //固定操作大小
        private const int OPERAT_SIZE_LOCK = 3;
        private const int OPERAT_SIZE_IDOLSKILL = 7;

        private int ProcessId;
        private IntPtr ImageBase;
        private IntPtr ProcessHandle;

        //三个操作基址
        private IntPtr KeepComboAddr;

        //客制化功能,把梦游少女的技能加入到偶像布若中
        private IntPtr CustomReplaceSkillIdolAddr;
        private static byte[] CustomeReplaceByte1 = new byte[51]
        {
            0x48,0x85,0xc0,
            0x74,0x24,
            0xc7,0x40,0x10,0xc8,0,0,0,
            0x48,0x8b,0x0d,0x92,0xbe,0x01,0x01,
            0xe8,0xad,0x71,0x30,0xff,
            0x48,0x85,0xc0,
            0x74,0x0c,
            0xc7,0x40,0x48,0x0,0x0,0xc0,0x3f,
            0x48,0x83,0xc4,0x28,
            0xc3,
            0xe8,0xf7,0xa6,0xdf,0xfd,
            0xcc,0xcc,0xcc,0xcc,0xcc
        };
        private static byte[] CustomeReplaceByte2 = new byte[51]
        {
            0x48,0x85,0xc0,
            0x74,0x28,
            0xc7,0x40,0x10,0xc8,0,0,0,
            0x48,0x8b,0x0d,0x92,0xbe,0x01,0x01,
            0xe8,0xad,0x71,0x30,0xff,
            0x48,0x85,0xc0,
            0x74,0x10,
            0xc7,0x40,0x48,0x0,0x0,0x80,0x40,
            0x83,0x40,0x30,0x01,
            0x48,0x83,0xc4,0x28,
            0xc3,
            0xe8,0xf7,0xa6,0xdf,0xfd,
            0xcc
        };

        //数据区
        private static byte[] LockHPByte_origin = new byte[OPERAT_SIZE_LOCK] {0x8d,0x0c,0x3e };
        private static byte[] LockHPByte_patched = new byte[OPERAT_SIZE_LOCK] { 0x41,0x8b,0xc8 };
        private static byte[] IdolSkillByte_origin = new byte[OPERAT_SIZE_IDOLSKILL] { 0xc7,0x40,0x44,0x00,0x00,0xc0,0x3f };
        private static byte[] IdolSkillByte_patched = new byte[OPERAT_SIZE_IDOLSKILL] { 0xc7,0x40,0x44,0x00,0x00,0x20,0x41 };

        public Operation(int processid, IntPtr imagebase)
        {
            ImageBase = imagebase;
            ProcessId = processid;
            ProcessHandle = OpenProcess(ACCESS_ALL, false, processid);

            CustomReplaceSkillIdolAddr = ImageBase + 0x23C909B;

        }
        private bool PreCheck()
        {
            //检查pid和imagebase是否为空,为true有效,为false无效
            if (ImageBase == IntPtr.Zero || ProcessId == 0 || ProcessHandle == IntPtr.Zero)
            {
                return false;
            }
            return true;
        }
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer,
               int nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer,
            int nSize, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern void CloseHandle(IntPtr hObject);



        //主要功能区
        

        public bool LockHP(int  Mode)
        {
            bool ret = false;
            IntPtr OperatAddr = ImageBase + PART_LOCKHP1;
            if (PreCheck() == true)
            {
                byte[] nowByte = new byte[OPERAT_SIZE_LOCK];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(nowByte, 0);
                ret = ReadProcessMemory(ProcessHandle, OperatAddr, byteAddress, OPERAT_SIZE_LOCK, IntPtr.Zero);
                if (ret == true)
                {
                    if (Mode == OPEN)
                    {
                        if (CmpBytes(nowByte, LockHPByte_origin, OPERAT_SIZE_LOCK))
                        {
                            IntPtr operatBytes = Marshal.UnsafeAddrOfPinnedArrayElement(LockHPByte_patched, 0);
                            return WriteProcessMemory(ProcessHandle, OperatAddr, operatBytes, OPERAT_SIZE_LOCK, IntPtr.Zero);
                        }
                    }
                    if (Mode == CLOSE)
                    {
                        if (CmpBytes(nowByte, LockHPByte_patched, OPERAT_SIZE_LOCK))
                        {
                            IntPtr operatBytes = Marshal.UnsafeAddrOfPinnedArrayElement(LockHPByte_origin, 0);
                            return WriteProcessMemory(ProcessHandle, OperatAddr, operatBytes, OPERAT_SIZE_LOCK, IntPtr.Zero);
                        }
                    }
                }
            }
            return false;
        }

        public bool IdolSkill(int Mode)
        {
            bool ret = false;
            IntPtr OperatAddr = ImageBase + PART_IDOTSKILL;
            if (PreCheck() == true)
            {
                byte[] nowByte = new byte[OPERAT_SIZE_IDOLSKILL];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(nowByte, 0);
                ret = ReadProcessMemory(ProcessHandle, OperatAddr, byteAddress, OPERAT_SIZE_IDOLSKILL, IntPtr.Zero);
                if (ret == true)
                {
                    if (Mode == OPEN)
                    {
                        if (CmpBytes(nowByte, IdolSkillByte_origin, OPERAT_SIZE_IDOLSKILL))
                        {
                            IntPtr patchBytes = Marshal.UnsafeAddrOfPinnedArrayElement(IdolSkillByte_patched, 0);
                            return WriteProcessMemory(ProcessHandle, OperatAddr, patchBytes, OPERAT_SIZE_IDOLSKILL, IntPtr.Zero);
                        }
                    }
                    if (Mode == CLOSE)
                    {
                        if (CmpBytes(nowByte, IdolSkillByte_patched, OPERAT_SIZE_IDOLSKILL))
                        {
                            IntPtr patchBytes = Marshal.UnsafeAddrOfPinnedArrayElement(IdolSkillByte_origin, 0);
                            return WriteProcessMemory(ProcessHandle, OperatAddr, patchBytes, OPERAT_SIZE_IDOLSKILL, IntPtr.Zero);
                        }
                    }
                }
            }
            return false;
        }
       
        public bool CloseAllEffect()
        {
            return IdolSkill(CLOSE) | LockHP(CLOSE);
        }

        //cmp两个字节,返回true为一样,返回false为不一样
        private bool CmpBytes(byte[] a, byte[] b, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        //copy两个字节数据的数据,把数据b复制到数据a,长度为length
        private void CpyBytes(byte[] a, byte[] b, int length)
        {
            for (int i = 0; i < length; i++)
            {
                a[i] = b[i];
            }
        }

        public bool OpenCustomerReplaceSkill()
        {
            bool ret = false;
            if (PreCheck() == true)
            {
                byte[] nowByte = new byte[51];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(nowByte, 0);
                ret = ReadProcessMemory(ProcessHandle, CustomReplaceSkillIdolAddr, byteAddress, 51, IntPtr.Zero);
                if ((ret == true) && CmpBytes(nowByte, CustomeReplaceByte1, 51))
                {
                    CpyBytes(nowByte, CustomeReplaceByte2, 51);
                    ret = WriteProcessMemory(ProcessHandle, CustomReplaceSkillIdolAddr, byteAddress, 51, IntPtr.Zero);
                }
                else
                {
                    ret = false;
                }
            }
            return ret;
        }
        public bool CloseCustomerReplaceSkill()
        {
            bool ret = false;
            if (PreCheck() == true)
            {
                byte[] nowByte = new byte[51];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(nowByte, 0);
                ret = ReadProcessMemory(ProcessHandle, CustomReplaceSkillIdolAddr, byteAddress, 51, IntPtr.Zero);
                if ((ret == true) && CmpBytes(nowByte, CustomeReplaceByte2,51))
                {
                    CpyBytes(nowByte, CustomeReplaceByte1, 51);
                    ret = WriteProcessMemory(ProcessHandle, CustomReplaceSkillIdolAddr, byteAddress, 51, IntPtr.Zero);
                }
                else
                {
                    ret = false;
                }
            }
            return ret;
        }
    }
}
