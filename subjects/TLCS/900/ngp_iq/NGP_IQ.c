// NGP_IQ.c
// Generated by decompiling NGP_IQ.NGP
// using Reko decompiler version 0.7.1.0.

#include "NGP_IQ.h"

// 00200089: void fn00200089(Register word16 de)
void fn00200089(word16 de)
{
	fn0020060C();
	*(ui32 *) 0x6F86 = *(ui32 *) 0x6F86 | 0x40;
	word32 * xiy_10 = (word32 *) 28600;
	byte b_11 = 0x12;
	do
	{
		*xiy_10 = globals->dw200040;
		xiy_10 = xiy_10 + 0x01;
		b_11 = b_11 - 0x01;
	} while (b_11 != 0x00);
	*(byte *) 0x8002 = 0x00;
	*(byte *) 0x8003 = 0x00;
	*(byte *) 0x8004 = 0xA0;
	*(byte *) 0x8005 = 0x98;
	word16 * xhl_32 = (word16 *) 0xA000;
	word16 bc_34 = 0x0200;
	do
	{
		*xhl_32 = 0x00;
		xhl_32 = xhl_32 + 0x01;
		bc_34 = bc_34 - 0x01;
	} while (bc_34 != 0x00);
	word16 bc_46 = 0x0250;
	word16 * xde_49 = (word16 *) 0xA400;
	word16 * xhl_52 = globals->a20061D;
	do
	{
		*xde_49 = *xhl_52;
		xhl_52 = xhl_52 + 0x01;
		xde_49 = xde_49 + 0x01;
		bc_46 = bc_46 - 0x01;
	} while (bc_46 != 0x00);
	fn002005B8();
	__ei(0x00);
	word16 bc_70 = 0x10;
	word16 * xde_73 = (word16 *) 0x8300;
	word16 * xhl_76 = globals->a200ABD;
	do
	{
		*xde_73 = *xhl_76;
		xhl_76 = xhl_76 + 0x01;
		xde_73 = xde_73 + 0x01;
		bc_70 = bc_70 - 0x01;
	} while (bc_70 != 0x00);
	*(byte *) 0x83E0 = 0x33;
	*(byte *) 33761 = 0x07;
	*(byte *) 0x8118 = 0x80;
	word16 wa_107 = fn00200557(0x00, DPB(bc_70, 0x00, 8), 0x04, &globals->b2000FD);
	word16 wa_118 = fn00200557(wa_107, DPB(bc_70, 0x01, 8), 0x04, &globals->b200122);
	word16 wa_129 = fn00200557(wa_118, DPB(bc_70, 0x04, 8), 0x04, &globals->b200147);
	word16 wa_140 = fn00200557(wa_129, DPB(bc_70, 0x05, 8), 0x02, &globals->b200165);
	fn00200557(wa_140, DPB(bc_70, 0x06, 8), 0x02, &globals->b20017F);
	byte * xde_153 = (byte *) 0x7000;
	byte * xhl_156 = globals->a200363;
	word16 bc_158 = 303;
	do
	{
		*xde_153 = *xhl_156;
		xde_153 = xde_153 + 0x01;
		xhl_156 = xhl_156 + 0x01;
		word16 de_170 = (word16) xde_153;
		bc_158 = bc_158 - 0x01;
	} while (bc_158 != 0x00);
	fn002004F2(0x01);
	word16 wa_179 = 0x7000;
	if (true)
	{
		word32 xsp_520;
		word32 xix_521;
		word32 xiy_522;
		byte b_523;
		word32 xwa_524;
		word32 xhl_526;
		word32 xde_528;
		bool H_529;
		bool V_530;
		bool N_531;
		byte d_532;
		byte c_533;
		byte w_534;
		word16 hl_535;
		byte a_536;
		byte SZHVC_537;
		bool Z_538;
		((union Eq_245 *) 0x7000)();
	}
	word16 wa_222;
	word16 bc_221;
	fn00200532(0x04, 0x04, bc_158, fn00200532(*(byte *) 0x4003, 0x04, bc_158, de_170, 3334), 3333);
	word16 wa_204 = DPB(wa_179, 0x04, 8);
	if (*(byte *) 0x4003 == 0x04)
	{
		bc_221 = DPB(bc_158, 0x04, 8);
		wa_222 = fn00200557(wa_204, bc_221, 0x01, &globals->b2001D2);
	}
	else
	{
		bc_221 = DPB(bc_158, 0x04, 8);
		wa_222 = fn00200557(wa_204, bc_221, 0x03, &globals->b2001E8);
	}
	word16 wa_230 = fn00200557(wa_222, DPB(bc_221, 0x08, 8), 0x04, &globals->b2001FD);
	word16 wa_241 = fn00200557(wa_230, DPB(bc_221, 0x09, 8), 0x02, &globals->b200217);
	fn00200557(wa_241, DPB(bc_221, 0x0A, 8), 0x02, &globals->b200231);
	byte * xde_254 = (byte *) 0x7000;
	byte * xhl_257 = globals->a200492;
	word16 bc_259 = 0x23;
	do
	{
		*xde_254 = *xhl_257;
		xde_254 = xde_254 + 0x01;
		xhl_257 = xhl_257 + 0x01;
		word16 de_271 = (word16) xde_254;
		bc_259 = bc_259 - 0x01;
	} while (bc_259 != 0x00);
	fn002004F2(0x01);
	word16 wa_280 = 0x7000;
	if (true)
	{
		word32 xsp_475;
		word32 xix_476;
		word32 xiy_477;
		byte b_478;
		word32 xwa_479;
		word32 xhl_481;
		word32 xde_483;
		bool H_484;
		bool V_485;
		bool N_486;
		byte d_487;
		byte c_488;
		byte w_489;
		word16 hl_490;
		byte a_491;
		byte SZHVC_492;
		bool Z_493;
		((union Eq_245 *) 0x7000)();
	}
	word16 wa_323;
	word16 bc_322;
	fn00200532(0x01, 0x04, bc_259, fn00200532(*(byte *) 0x4003, 0x04, bc_259, de_271, 3338), 3337);
	word16 wa_305 = DPB(wa_280, 0x04, 8);
	if (*(byte *) 0x4003 == 0x01)
	{
		bc_322 = DPB(bc_259, 0x08, 8);
		wa_323 = fn00200557(wa_305, bc_322, 0x01, &globals->b200284);
	}
	else
	{
		bc_322 = DPB(bc_259, 0x08, 8);
		wa_323 = fn00200557(wa_305, bc_322, 0x03, &globals->b20029A);
	}
	word16 wa_331 = fn00200557(wa_323, DPB(bc_322, 0x0C, 8), 0x04, &globals->b2002AF);
	word16 wa_342 = fn00200557(wa_331, DPB(bc_322, 0x0D, 8), 0x02, &globals->b2002CB);
	fn00200557(wa_342, DPB(bc_322, 0x0E, 8), 0x02, &globals->b2002E5);
	byte * xde_355 = (byte *) 0x7000;
	byte * xhl_358 = globals->a2004B5;
	word16 bc_360 = 0x2B;
	do
	{
		*xde_355 = *xhl_358;
		xde_355 = xde_355 + 0x01;
		xhl_358 = xhl_358 + 0x01;
		word16 de_372 = (word16) xde_355;
		bc_360 = bc_360 - 0x01;
	} while (bc_360 != 0x00);
	fn002004F2(0x01);
	word16 wa_381 = 0x7000;
	if (true)
	{
		word32 xsp_432;
		word32 xix_433;
		word32 xiy_434;
		byte b_435;
		word32 xwa_436;
		word32 xhl_438;
		word32 xde_440;
		bool H_441;
		bool V_442;
		bool N_443;
		byte d_444;
		byte c_445;
		byte w_446;
		word16 hl_447;
		byte a_448;
		byte SZHVC_449;
		bool Z_450;
		((union Eq_245 *) 0x7000)();
	}
	fn00200532(0x00, 0x04, bc_360, fn00200532(*(byte *) 0x4003, 0x04, bc_360, de_372, 0x0D0E), 0x0D0D);
	word16 wa_407 = DPB(wa_381, 0x04, 8);
	if (*(byte *) 0x4003 == 0x00)
		fn00200557(wa_407, DPB(bc_360, 0x0C, 8), 0x01, &globals->b200338);
	else
		fn00200557(wa_407, DPB(bc_360, 0x0C, 8), 0x03, &globals->b20034E);
	while (true)
		;
}

// 002004F2: void fn002004F2(Register byte w)
void fn002004F2(byte w)
{
	*(byte *) 0x4004 = 0x00;
	do
		;
	while (*(byte *) 0x4004 != w);
	return;
}

// 0020050A: Register byte fn0020050A(Register byte w, Register byte a, Register word16 bc, Register word16 de, Register word16 hl, Register out Eq_478 deOut, Register out Eq_479 hOut)
byte fn0020050A(byte w, byte a, word16 bc, word16 de, word16 hl, Eq_478 & deOut, Eq_479 & hOut)
{
	struct Eq_480 * xde_27 = DPB(0x9800, (word16) (DPB(bc, 0x00, 8) * 0x02) + 0x9800 + (word16) (DPB(hl, 0x00, 8) * 0x40), 0);
	xde_27->b0000 = a;
	xde_27->b0001 = w;
	word16 de_43;
	*deOut = (word16) xde;
	byte h_46;
	*hOut = SLICE(xhl, byte, 8);
	return (byte) xbc;
}

// 00200532: Register word16 fn00200532(Register byte a, Register byte w, Register word16 bc, Register word16 de, Register word16 hl)
word16 fn00200532(byte a, byte w, word16 bc, word16 de, word16 hl)
{
	word16 de_23;
	byte h_24;
	byte c_25 = fn0020050A(w, ((a & 0xF0) >> 0x04) + 0x40, DPB(bc, a, 0), de, hl, out de_23, out h_24);
	word16 de_34;
	byte h_35;
	fn0020050A(w, (c_25 & 0x0F) + 0x40, DPB(bc, c_25, 0), de_23, DPB(hl, h_24 + 0x01, 8), out de_34, out h_35);
	return de_34;
}

// 00200557: Register word16 fn00200557(Register word16 wa, Register word16 bc, Register byte d, Register (ptr byte) xhl)
word16 fn00200557(word16 wa, word16 bc, byte d, byte * xhl)
{
	*(ui8 *) 0x4002 = d << 0x01;
	word16 wa_100 = DPB(wa, c * 0x02, 0);
	byte * xde_29 = (word16) (DPB(bc, 0x00, 8) * 0x40) + ((word32) (c * 0x02) + 0x9800);
	byte b_33 = 0x13;
l00200579:
	cu8 v17_47 = *xhl;
	wa_100 = DPB(wa_100, v17_47, 0);
	if (v17_47 != 0x00)
	{
		xhl = xhl + 0x01;
		if (v17_47 >= 0x5B)
			wa_100 = DPB(wa_100, v17_47 + 0xE0, 0);
		word16 wa_77 = DPB(wa_100, 0x00, 8);
		byte w_82 = SLICE(wa_77 + 0x09, byte, 8) + *((byte *) 0x4002);
		*xde_29 = (byte) (wa_77 + 0x09);
		byte * xde_88 = xde_29 + 0x01;
		*xde_88 = w_82;
		wa_100 = DPB(wa_77 + 0x09, w_82, 8);
		xde_29 = xde_88 + 0x01;
		b_33 = b_33 - 0x01;
		if (b_33 != 0x00)
			goto l00200579;
	}
	return wa_100;
}

// 002005B8: void fn002005B8()
void fn002005B8()
{
	byte * xbc_2 = (byte *) 0x9000;
	word16 hl_3 = 0x04C0;
	do
	{
		*xbc_2 = 0x00;
		xbc_2 = xbc_2 + 0x01;
		hl_3 = hl_3 - 0x01;
	} while (hl_3 != 0x00);
	byte * xbc_13 = (byte *) 0x9800;
	word16 hl_14 = 0x04C0;
	do
	{
		*xbc_13 = 0x00;
		xbc_13 = xbc_13 + 0x01;
		hl_14 = hl_14 - 0x01;
	} while (hl_14 != 0x00);
	return;
}

// 0020060C: void fn0020060C()
void fn0020060C()
{
	if (*(byte *) 0x6F91 == 0x00)
	{
		*(ui32 *) 0x6F83 = *(ui32 *) 0x6F83 & ~0x08;
		*(byte *) 0x6DA0 = 0x00;
	}
	return;
}
