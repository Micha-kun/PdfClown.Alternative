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
      <summary>Windows ANSI encoding (Windows Code Page 1252) [PDF:1.6:D].</summary>
    */
    internal sealed class WinAnsiEncoding
      : Encoding
    {
        public WinAnsiEncoding(
          )
        {
            this.Put(65, "A");
            this.Put(198, "AE");
            this.Put(193, "Aacute");
            this.Put(194, "Acircumflex");
            this.Put(196, "Adieresis");
            this.Put(192, "Agrave");
            this.Put(197, "Aring");
            this.Put(195, "Atilde");
            this.Put(66, "B");
            this.Put(67, "C");
            this.Put(199, "Ccedilla");
            this.Put(68, "D");
            this.Put(69, "E");
            this.Put(201, "Eacute");
            this.Put(202, "Ecircumflex");
            this.Put(203, "Edieresis");
            this.Put(200, "Egrave");
            this.Put(208, "Eth");
            this.Put(128, "Euro");
            this.Put(70, "F");
            this.Put(71, "G");
            this.Put(72, "H");
            this.Put(73, "I");
            this.Put(205, "Iacute");
            this.Put(206, "Icircumflex");
            this.Put(207, "Idieresis");
            this.Put(204, "Igrave");
            this.Put(74, "J");
            this.Put(75, "K");
            this.Put(76, "L");
            this.Put(77, "M");
            this.Put(78, "N");
            this.Put(209, "Ntilde");
            this.Put(79, "O");
            this.Put(140, "OE");
            this.Put(211, "Oacute");
            this.Put(212, "Ocircumflex");
            this.Put(214, "Odieresis");
            this.Put(210, "Ograve");
            this.Put(216, "Oslash");
            this.Put(213, "Otilde");
            this.Put(80, "P");
            this.Put(81, "Q");
            this.Put(82, "R");
            this.Put(83, "S");
            this.Put(138, "Scaron");
            this.Put(84, "T");
            this.Put(222, "Thorn");
            this.Put(85, "U");
            this.Put(218, "Uacute");
            this.Put(219, "Ucircumflex");
            this.Put(220, "Udieresis");
            this.Put(217, "Ugrave");
            this.Put(86, "V");
            this.Put(87, "W");
            this.Put(88, "X");
            this.Put(89, "Y");
            this.Put(221, "Yacute");
            this.Put(159, "Ydieresis");
            this.Put(90, "Z");
            this.Put(142, "Zcaron");
            this.Put(97, "a");
            this.Put(225, "aacute");
            this.Put(226, "acircumflex");
            this.Put(180, "acute");
            this.Put(228, "adieresis");
            this.Put(230, "ae");
            this.Put(224, "agrave");
            this.Put(38, "ampersand");
            this.Put(229, "aring");
            this.Put(94, "asciicircum");
            this.Put(126, "asciitilde");
            this.Put(42, "asterisk");
            this.Put(64, "at");
            this.Put(227, "atilde");
            this.Put(98, "b");
            this.Put(92, "backslash");
            this.Put(124, "bar");
            this.Put(123, "braceleft");
            this.Put(125, "braceright");
            this.Put(91, "bracketleft");
            this.Put(93, "bracketright");
            this.Put(166, "brokenbar");
            this.Put(149, "bullet");
            this.Put(99, "c");
            this.Put(231, "ccedilla");
            this.Put(184, "cedilla");
            this.Put(162, "cent");
            this.Put(136, "circumflex");
            this.Put(58, "colon");
            this.Put(44, "comma");
            this.Put(169, "copyright");
            this.Put(164, "currency");
            this.Put(100, "d");
            this.Put(134, "dagger");
            this.Put(135, "daggerdbl");
            this.Put(176, "degree");
            this.Put(168, "dieresis");
            this.Put(247, "divide");
            this.Put(36, "dollar");
            this.Put(101, "e");
            this.Put(233, "eacute");
            this.Put(234, "ecircumflex");
            this.Put(235, "edieresis");
            this.Put(232, "egrave");
            this.Put(56, "eight");
            this.Put(133, "ellipsis");
            this.Put(151, "emdash");
            this.Put(150, "endash");
            this.Put(61, "equal");
            this.Put(240, "eth");
            this.Put(33, "exclam");
            this.Put(161, "exclamdown");
            this.Put(102, "f");
            this.Put(53, "five");
            this.Put(131, "florin");
            this.Put(52, "four");
            this.Put(103, "g");
            this.Put(223, "germandbls");
            this.Put(96, "grave");
            this.Put(62, "greater");
            this.Put(171, "guillemotleft");
            this.Put(187, "guillemotright");
            this.Put(139, "guilsinglleft");
            this.Put(155, "guilsinglright");
            this.Put(104, "h");
            this.Put(45, "hyphen");
            this.Put(105, "i");
            this.Put(237, "iacute");
            this.Put(238, "icircumflex");
            this.Put(239, "idieresis");
            this.Put(236, "igrave");
            this.Put(106, "j");
            this.Put(107, "k");
            this.Put(108, "l");
            this.Put(60, "less");
            this.Put(172, "logicalnot");
            this.Put(109, "m");
            this.Put(175, "macron");
            this.Put(181, "mu");
            this.Put(215, "multiply");
            this.Put(110, "n");
            this.Put(57, "nine");
            this.Put(241, "ntilde");
            this.Put(35, "numbersign");
            this.Put(111, "o");
            this.Put(243, "oacute");
            this.Put(244, "ocircumflex");
            this.Put(246, "odieresis");
            this.Put(156, "oe");
            this.Put(242, "ograve");
            this.Put(49, "one");
            this.Put(189, "onehalf");
            this.Put(188, "onequarter");
            this.Put(185, "onesuperior");
            this.Put(170, "ordfeminine");
            this.Put(186, "ordmasculine");
            this.Put(248, "oslash");
            this.Put(245, "otilde");
            this.Put(112, "p");
            this.Put(182, "paragraph");
            this.Put(40, "parenleft");
            this.Put(41, "parenright");
            this.Put(37, "percent");
            this.Put(46, "period");
            this.Put(183, "periodcentered");
            this.Put(137, "perthousand");
            this.Put(43, "plus");
            this.Put(177, "plusminus");
            this.Put(113, "q");
            this.Put(63, "question");
            this.Put(191, "questiondown");
            this.Put(34, "quotedbl");
            this.Put(132, "quotedblbase");
            this.Put(147, "quotedblleft");
            this.Put(148, "quotedblright");
            this.Put(145, "quoteleft");
            this.Put(146, "quoteright");
            this.Put(130, "quotesinglbase");
            this.Put(39, "quotesingle");
            this.Put(114, "r");
            this.Put(174, "registered");
            this.Put(115, "s");
            this.Put(154, "scaron");
            this.Put(167, "section");
            this.Put(59, "semicolon");
            this.Put(55, "seven");
            this.Put(54, "six");
            this.Put(47, "slash");
            this.Put(32, "space");
            this.Put(163, "sterling");
            this.Put(116, "t");
            this.Put(254, "thorn");
            this.Put(51, "three");
            this.Put(190, "threequarters");
            this.Put(179, "threesuperior");
            this.Put(152, "tilde");
            this.Put(153, "trademark");
            this.Put(50, "two");
            this.Put(178, "twosuperior");
            this.Put(117, "u");
            this.Put(250, "uacute");
            this.Put(251, "ucircumflex");
            this.Put(252, "udieresis");
            this.Put(249, "ugrave");
            this.Put(95, "underscore");
            this.Put(118, "v");
            this.Put(119, "w");
            this.Put(120, "x");
            this.Put(121, "y");
            this.Put(253, "yacute");
            this.Put(255, "ydieresis");
            this.Put(165, "yen");
            this.Put(122, "z");
            this.Put(158, "zcaron");
            this.Put(48, "zero");
        }
    }
}
