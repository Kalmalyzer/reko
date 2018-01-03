﻿#region License
/* 
 * Copyright (C) 1999-2017 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Reko.Core;
using Reko.Core.Expressions;
using Reko.Core.Lib;
using Reko.Core.Machine;
using Reko.Core.Operators;
using Reko.Core.Rtl;
using Reko.Core.Serialization;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Reko.Arch.M68k
{
    [Designer("Reko.Arch.M68k.Design.M68kArchitectureDesigner,Reko.Arch.M68k.Design")]
    public class M68kArchitecture : ProcessorArchitecture
    {
        private List<FlagGroupStorage> flagGroups;

        public M68kArchitecture()
        {
            InstructionBitSize = 16;
            FramePointerType = PrimitiveType.Pointer32;
            PointerType = PrimitiveType.Pointer32;
            WordWidth = PrimitiveType.Word32;
            CarryFlagMask = (uint)FlagM.CF;
            StackRegister = Registers.a7;

            flagGroups = new List<FlagGroupStorage>();
        }

        public override IEnumerable<MachineInstruction> CreateDisassembler(EndianImageReader rdr)
        {
            return M68kDisassembler.Create68020(rdr);
        }

        public IEnumerable<M68kInstruction> CreateDisassemblerImpl(EndianImageReader rdr)
        {
            return M68kDisassembler.Create68020(rdr);
        }

        public override IEqualityComparer<MachineInstruction> CreateInstructionComparer(Normalize norm)
        {
            return new M68kInstructionComparer(norm);
        }

        public override ProcessorState CreateProcessorState()
        {
            return new M68kState(this);
        }

        public override IEnumerable<Address> CreatePointerScanner(SegmentMap map, EndianImageReader rdr, IEnumerable<Address> knownAddresses, PointerScannerFlags flags)
        {
            var knownLinAddresses = knownAddresses.Select(a => a.ToUInt32()).ToHashSet();
            return new M68kPointerScanner(rdr, knownLinAddresses, flags).Select(li => Address.Ptr32(li));
        }

        public override EndianImageReader CreateImageReader(MemoryArea image, Address addr)
        {
            return new BeImageReader(image, addr);
        }

        public override EndianImageReader CreateImageReader(MemoryArea image, Address addrBegin, Address addrEnd)
        {
            return new BeImageReader(image, addrBegin, addrEnd);
        }

        public override EndianImageReader CreateImageReader(MemoryArea image, ulong offset)
        {
            return new BeImageReader(image, offset);
        }

        public override ImageWriter CreateImageWriter()
        {
            return new BeImageWriter();
        }

        public override ImageWriter CreateImageWriter(MemoryArea mem, Address addr)
        {
            return new BeImageWriter(mem, addr);
        }

        public override SortedList<string, int> GetOpcodeNames()
        {
            return Enum.GetValues(typeof(Opcode))
                .Cast<Opcode>()
                .ToSortedList(
                    v => v.ToString(),
                    v => (int)v);
        }

        public override int? GetOpcodeNumber(string name)
        {
            Opcode result;
            if (!Enum.TryParse(name, true, out result))
                return null;
            return (int)result;
        }

        public override RegisterStorage GetRegister(int i)
        {
            return Registers.GetRegister(i);
        }

        public override RegisterStorage GetRegister(string name)
        {
            var r = Registers.GetRegister(name);
            if (r == RegisterStorage.None)
                throw new ArgumentException(string.Format("'{0}' is not a register name.", name));
            return r;
        }

        public override RegisterStorage[] GetRegisters()
        {
            return Registers.regs;
        }

        public override bool TryGetRegister(string name, out RegisterStorage reg)
        {
            return Registers.regsByName.TryGetValue(name, out reg);
        }

        public override FlagGroupStorage GetFlagGroup(uint grf)
        {
            foreach (FlagGroupStorage f in flagGroups)
            {
                if (f.FlagGroupBits == grf)
                    return f;
            }

            PrimitiveType dt = Bits.IsSingleBitSet(grf) ? PrimitiveType.Bool : PrimitiveType.Byte;
            var fl = new FlagGroupStorage(Registers.ccr, grf, GrfToString(grf), dt);
            flagGroups.Add(fl);
            return fl;
        }

        public override FlagGroupStorage GetFlagGroup(string name)
        {
            FlagM grf = 0;
            for (int i = 0; i < name.Length; ++i)
            {
                switch (name[i])
                {
                    case 'C': grf |= FlagM.CF; break;
                    case 'V': grf |= FlagM.VF; break;
                    case 'Z': grf |= FlagM.ZF; break;
                    case 'N': grf |= FlagM.NF; break;
                    case 'X': grf |= FlagM.XF; break;
                    default: return null;
                }
            }
            return GetFlagGroup((uint)grf);
        }

        public override IEnumerable<RtlInstructionCluster> CreateRewriter(EndianImageReader rdr, ProcessorState state, IStorageBinder binder, IRewriterHost host)
        {
            return new Rewriter(this, rdr, (M68kState)state, binder, host);
        }

        public override Expression CreateStackAccess(IStorageBinder frame, int offset, DataType dataType)
        {
            return new MemoryAccess(new BinaryExpression(
                Operator.IAdd, FramePointerType,
                frame.EnsureRegister(StackRegister), Constant.Word32(offset)),
                dataType);
        }

        public override Address MakeAddressFromConstant(Constant c)
        {
            return Address.Ptr32(c.ToUInt32());
        }

        public override Address ReadCodeAddress(int size, EndianImageReader rdr, ProcessorState state)
        {
            return Address.Ptr32(rdr.ReadBeUInt32());
        }

        private static string[] flagRegisterNames = {
            "C",
            "V",
            "Z",
            "N",
            "X",
        };

        public override string GrfToString(uint grf)
        {
            StringBuilder s = new StringBuilder();
            for (int r = 0; grf != 0; ++r, grf >>= 1)
            {
                if ((grf & 1) != 0)
                    s.Append(flagRegisterNames[r]);
            }
            return s.ToString();
        }

        public override bool TryParseAddress(string txtAddress, out Address addr)
        {
            return Address.TryParse32(txtAddress, out addr);
        }
    }
}