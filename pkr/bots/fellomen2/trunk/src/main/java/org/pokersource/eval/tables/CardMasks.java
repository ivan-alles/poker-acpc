/*
*  PortedHandEval.java: a fast poker hand evaluator based on eval.h
*
*  Original work Copyright (C) 1993-99 Brian Goetz, Keith Miyake, Clifford T. Matthews
*  Modifications Copyright (C) 2004 Jordan Christensen (thebigjc@gmail.com)
*
*  This program is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program; if not, write to the Free Software
*  Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
*/

package org.pokersource.eval.tables;

/**
 * @author Jordan Christensen (thebigjc@gmail.com)
 */
public class CardMasks {
	public static final long cardMasksTable[] = new long[] {
			0x0001000000000000L, 0x0002000000000000L, 0x0004000000000000L,
			0x0008000000000000L, 0x0010000000000000L, 0x0020000000000000L,
			0x0040000000000000L, 0x0080000000000000L, 0x0100000000000000L,
			0x0200000000000000L, 0x0400000000000000L, 0x0800000000000000L,
			0x1000000000000000L, 0x0000000100000000L, 0x0000000200000000L,
			0x0000000400000000L, 0x0000000800000000L, 0x0000001000000000L,
			0x0000002000000000L, 0x0000004000000000L, 0x0000008000000000L,
			0x0000010000000000L, 0x0000020000000000L, 0x0000040000000000L,
			0x0000080000000000L, 0x0000100000000000L, 0x0000000000010000L,
			0x0000000000020000L, 0x0000000000040000L, 0x0000000000080000L,
			0x0000000000100000L, 0x0000000000200000L, 0x0000000000400000L,
			0x0000000000800000L, 0x0000000001000000L, 0x0000000002000000L,
			0x0000000004000000L, 0x0000000008000000L, 0x0000000010000000L,
			0x0000000000000001L, 0x0000000000000002L, 0x0000000000000004L,
			0x0000000000000008L, 0x0000000000000010L, 0x0000000000000020L,
			0x0000000000000040L, 0x0000000000000080L, 0x0000000000000100L,
			0x0000000000000200L, 0x0000000000000400L, 0x0000000000000800L,
			0x0000000000001000L };
}
