﻿#region License
/* 
 * Copyright (C) 1999-2016 John Källén.
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

using NUnit.Framework;
using Reko.Arch.Tlcs;
using Reko.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reko.UnitTests.Arch.Tlcs
{
    [TestFixture]
    public class Tlcs900DisassemblerTests : DisassemblerTestBase<Tlcs900Instruction>
    {
        private Tlcs900Architecture arch;

        public Tlcs900DisassemblerTests()
        {
            this.arch = new Tlcs900Architecture();
        }

        public override IProcessorArchitecture Architecture
        {
            get { return arch; }
        }

        public override Address LoadAddress { get { return Address.Ptr32(0x00010000); } }

        protected override ImageWriter CreateImageWriter(byte[] bytes)
        {
            return new LeImageWriter(bytes);
        }

        private void AssertCode(string sExp, string hexBytes)
        {
            var i = DisassembleHexBytes(hexBytes);
            Assert.AreEqual(sExp, i.ToString());
        }

        [Test]
        public void Tlcs900_dis_nop()
        {
            AssertCode("nop", "00");
        }

        [Test]
        public void Tlcs900_dis_push_RR()
        {
            AssertCode("push\twa", "28");
        }

        [Test]
        public void Tlcs900_dis_ld_reg_byte()
        {
            AssertCode("ld\ta,b", "CA89");
        }

        [Test]
        public void Tlcs900_dis_add_reg_indirect_word()
        {
            AssertCode("add\tbc,(xhl)", "9381");
        }

        [Test]
        public void Tlcs900_dis_sub_reg_indexed_8()
        {
            AssertCode("sub\tde,(xsp-0x04)", "9FFCA2");
        }

        [Test]
        public void Tlcs900_dis_xor_reg_indexed_16()
        {
            AssertCode("xor\tde,(xsp-0x04)", "D31DFCFFD2");
        }

        [Test]
        public void Tlcs900_dis_inc_reg()
        {
            AssertCode("inc\t00000004,xbc", "E964");
        }

        [Test]
        public void Tlcs900_dis_inc_predec()
        {
            AssertCode("inc\t00000001,(-xde)", "E40961");
        }

        [Test]
        public void Tlcs900_dis_ld_absolute()
        {
            AssertCode("ld\txbc,(0000069C)", "E29C060021");
        }

        [Test]
        public void Tlcs900_dis_store_to_mem()
        {
            AssertCode("ld\t(xbc+0x26),xwa", "B92660");
        }

        [Test]
        public void Tlcs900_dis_lda()
        {
            AssertCode("lda\tix,(xbc+0x26)",  "B92624");
            AssertCode("lda\txiz,(xbc+0x26)", "B92636");
        }

        [Test]
        public void Tlcs900_dis_ext()
        {
            AssertCode("extz\thl", "DB12");
            AssertCode("exts\txde", "EA13");
        }

        [Test]
        public void Tlcs900_dis_jr()
        {
            AssertCode("jr\tZ,00010030", "662E");
            AssertCode("jr\t0000FF00", "78FDFE");
        }

        [Test]
        public void Tlcs900_dis_push_pop_sr()
        {
            AssertCode("push\tsr", "02");
            AssertCode("pop\tsr", "03");
        }

        [Test]
        public void Tlcs900_dis_push_ei_nn()
        {
            AssertCode("ei\t05", "0605");
        }

        [Test]
        public void Tlcs900_dis_jp_abs()
        {
            AssertCode("jp\t00001234", "1A3412");
            AssertCode("jp\t00123456", "1B563412");
        }

        [Test]
        public void Tlcs900_dis_call_abs()
        {
            AssertCode("call\t00001234", "1C3412");
            AssertCode("call\t00123456", "1D563412");
        }

        [Test]
        public void Tlcs900_dis_calr()
        {
            AssertCode("calr\t00010200", "1EFD01");
        }

        [Test]
        public void Tlcs900_dis_swi()
        {
            AssertCode("swi\t05", "FD");
        }

        [Test]
        public void Tlcs900_dis_xor_imm()
        {
            AssertCode("xor\tiz,1234", "DECD3412");
        }

        [Test]
        public void Tlcs900_dis_cp_imm()
        {
            AssertCode("cp\tiz,0005", "DEDD");
        }

        [Test]
        public void Tlcs900_dis_cp()
        {
            AssertCode("cp\te,(xsp-0x3E)", "8FC2F5");
        }

        [Test]
        public void Tlcs900_dis_rlc()
        {
            AssertCode("rlc\t04,e", "CDE804");
            AssertCode("rlc\ta,e", "CDF8");
        }
    }
}