/*
  Copyright 2009-2010 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

namespace org.pdfclown.documents.contents.fonts
{
    /**
      <summary>Adobe standard Latin-text encoding [PDF:1.6:D].</summary>
    */
    internal sealed class StandardEncoding
      : Encoding
    {
        public StandardEncoding(
          )
        {
            this.Put(65, "A");
            this.Put(225, "AE");
            this.Put(66, "B");
            this.Put(67, "C");
            this.Put(68, "D");
            this.Put(69, "E");
            this.Put(70, "F");
            this.Put(71, "G");
            this.Put(72, "H");
            this.Put(73, "I");
            this.Put(74, "J");
            this.Put(75, "K");
            this.Put(76, "L");
            this.Put(232, "Lslash");
            this.Put(77, "M");
            this.Put(78, "N");
            this.Put(79, "O");
            this.Put(234, "OE");
            this.Put(233, "Oslash");
            this.Put(80, "P");
            this.Put(81, "Q");
            this.Put(82, "R");
            this.Put(83, "S");
            this.Put(84, "T");
            this.Put(85, "U");
            this.Put(86, "V");
            this.Put(87, "W");
            this.Put(88, "X");
            this.Put(89, "Y");
            this.Put(90, "Z");
            this.Put(97, "a");
            this.Put(194, "acute");
            this.Put(241, "ae");
            this.Put(38, "ampersand");
            this.Put(94, "asciicircum");
            this.Put(126, "asciitilde");
            this.Put(42, "asterisk");
            this.Put(64, "at");
            this.Put(98, "b");
            this.Put(92, "backslash");
            this.Put(124, "bar");
            this.Put(123, "braceleft");
            this.Put(125, "braceright");
            this.Put(91, "bracketleft");
            this.Put(93, "bracketright");
            this.Put(198, "breve");
            this.Put(183, "bullet");
            this.Put(99, "c");
            this.Put(207, "caron");
            this.Put(203, "cedilla");
            this.Put(162, "cent");
            this.Put(195, "circumflex");
            this.Put(58, "colon");
            this.Put(44, "comma");
            this.Put(168, "currency");
            this.Put(100, "d");
            this.Put(178, "dagger");
            this.Put(179, "daggerdbl");
            this.Put(200, "dieresis");
            this.Put(36, "dollar");
            this.Put(199, "dotaccent");
            this.Put(245, "dotlessi");
            this.Put(101, "e");
            this.Put(56, "eight");
            this.Put(188, "ellipsis");
            this.Put(208, "emdash");
            this.Put(177, "endash");
            this.Put(61, "equal");
            this.Put(33, "exclam");
            this.Put(161, "exclamdown");
            this.Put(102, "f");
            this.Put(174, "fi");
            this.Put(53, "five");
            this.Put(175, "fl");
            this.Put(166, "florin");
            this.Put(52, "four");
            this.Put(164, "fraction");
            this.Put(103, "g");
            this.Put(251, "germandbls");
            this.Put(193, "grave");
            this.Put(62, "greater");
            this.Put(171, "guillemotleft");
            this.Put(187, "guillemotright");
            this.Put(172, "guilsinglleft");
            this.Put(173, "guilsinglright");
            this.Put(104, "h");
            this.Put(205, "hungarumlaut");
            this.Put(45, "hyphen");
            this.Put(105, "i");
            this.Put(106, "j");
            this.Put(107, "k");
            this.Put(108, "l");
            this.Put(60, "less");
            this.Put(248, "lslash");
            this.Put(109, "m");
            this.Put(197, "macron");
            this.Put(110, "n");
            this.Put(57, "nine");
            this.Put(35, "numbersign");
            this.Put(111, "o");
            this.Put(250, "oe");
            this.Put(206, "ogonek");
            this.Put(49, "one");
            this.Put(227, "ordfeminine");
            this.Put(235, "ordmasculine");
            this.Put(249, "oslash");
            this.Put(112, "p");
            this.Put(182, "paragraph");
            this.Put(40, "parenleft");
            this.Put(41, "parenright");
            this.Put(37, "percent");
            this.Put(46, "period");
            this.Put(180, "periodcentered");
            this.Put(189, "perthousand");
            this.Put(43, "plus");
            this.Put(113, "q");
            this.Put(63, "question");
            this.Put(191, "questiondown");
            this.Put(34, "quotedbl");
            this.Put(185, "quotedblbase");
            this.Put(170, "quotedblleft");
            this.Put(186, "quotedblright");
            this.Put(96, "quoteleft");
            this.Put(39, "quoteright");
            this.Put(184, "quotesinglbase");
            this.Put(169, "quotesingle");
            this.Put(114, "r");
            this.Put(202, "ring");
            this.Put(115, "s");
            this.Put(167, "section");
            this.Put(59, "semicolon");
            this.Put(55, "seven");
            this.Put(54, "six");
            this.Put(47, "slash");
            this.Put(32, "space");
            this.Put(163, "sterling");
            this.Put(116, "t");
            this.Put(51, "three");
            this.Put(196, "tilde");
            this.Put(50, "two");
            this.Put(117, "u");
            this.Put(95, "underscore");
            this.Put(118, "v");
            this.Put(119, "w");
            this.Put(120, "x");
            this.Put(121, "y");
            this.Put(165, "yen");
            this.Put(122, "z");
            this.Put(48, "zero");
        }
    }
}
