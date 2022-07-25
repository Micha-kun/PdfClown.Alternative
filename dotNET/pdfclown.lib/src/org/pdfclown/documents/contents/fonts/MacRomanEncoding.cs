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
      <summary>Mac OS standard latin encoding [PDF:1.6:D].</summary>
    */
    internal sealed class MacRomanEncoding
      : Encoding
    {
        public MacRomanEncoding(
          )
        {
            this.Put(65, "A");
            this.Put(174, "AE");
            this.Put(231, "Aacute");
            this.Put(229, "Acircumflex");
            this.Put(128, "Adieresis");
            this.Put(203, "Agrave");
            this.Put(129, "Aring");
            this.Put(204, "Atilde");
            this.Put(66, "B");
            this.Put(67, "C");
            this.Put(130, "Ccedilla");
            this.Put(68, "D");
            this.Put(69, "E");
            this.Put(131, "Eacute");
            this.Put(230, "Ecircumflex");
            this.Put(232, "Edieresis");
            this.Put(233, "Egrave");
            this.Put(70, "F");
            this.Put(71, "G");
            this.Put(72, "H");
            this.Put(73, "I");
            this.Put(234, "Iacute");
            this.Put(235, "Icircumflex");
            this.Put(236, "Idieresis");
            this.Put(237, "Igrave");
            this.Put(74, "J");
            this.Put(75, "K");
            this.Put(76, "L");
            this.Put(77, "M");
            this.Put(78, "N");
            this.Put(132, "Ntilde");
            this.Put(79, "O");
            this.Put(206, "OE");
            this.Put(238, "Oacute");
            this.Put(239, "Ocircumflex");
            this.Put(133, "Odieresis");
            this.Put(241, "Ograve");
            this.Put(175, "Oslash");
            this.Put(205, "Otilde");
            this.Put(80, "P");
            this.Put(81, "Q");
            this.Put(82, "R");
            this.Put(83, "S");
            this.Put(84, "T");
            this.Put(85, "U");
            this.Put(242, "Uacute");
            this.Put(243, "Ucircumflex");
            this.Put(134, "Udieresis");
            this.Put(244, "Ugrave");
            this.Put(86, "V");
            this.Put(87, "W");
            this.Put(88, "X");
            this.Put(89, "Y");
            this.Put(217, "Ydieresis");
            this.Put(90, "Z");
            this.Put(97, "a");
            this.Put(135, "aacute");
            this.Put(137, "acircumflex");
            this.Put(171, "acute");
            this.Put(138, "adieresis");
            this.Put(190, "ae");
            this.Put(136, "agrave");
            this.Put(38, "ampersand");
            this.Put(140, "aring");
            this.Put(94, "asciicircum");
            this.Put(126, "asciitilde");
            this.Put(42, "asterisk");
            this.Put(64, "at");
            this.Put(139, "atilde");
            this.Put(98, "b");
            this.Put(92, "backslash");
            this.Put(124, "bar");
            this.Put(123, "braceleft");
            this.Put(125, "braceright");
            this.Put(91, "bracketleft");
            this.Put(93, "bracketright");
            this.Put(249, "breve");
            this.Put(165, "bullet");
            this.Put(99, "c");
            this.Put(255, "caron");
            this.Put(141, "ccedilla");
            this.Put(252, "cedilla");
            this.Put(162, "cent");
            this.Put(246, "circumflex");
            this.Put(58, "colon");
            this.Put(44, "comma");
            this.Put(169, "copyright");
            this.Put(219, "currency");
            this.Put(100, "d");
            this.Put(160, "dagger");
            this.Put(224, "daggerdbl");
            this.Put(161, "degree");
            this.Put(172, "dieresis");
            this.Put(214, "divide");
            this.Put(36, "dollar");
            this.Put(250, "dotaccent");
            this.Put(245, "dotlessi");
            this.Put(101, "e");
            this.Put(142, "eacute");
            this.Put(144, "ecircumflex");
            this.Put(145, "edieresis");
            this.Put(143, "egrave");
            this.Put(56, "eight");
            this.Put(201, "ellipsis");
            this.Put(209, "emdash");
            this.Put(208, "endash");
            this.Put(61, "equal");
            this.Put(33, "exclam");
            this.Put(193, "exclamdown");
            this.Put(102, "f");
            this.Put(222, "fi");
            this.Put(53, "five");
            this.Put(223, "fl");
            this.Put(196, "florin");
            this.Put(52, "four");
            this.Put(218, "fraction");
            this.Put(103, "g");
            this.Put(167, "germandbls");
            this.Put(96, "grave");
            this.Put(62, "greater");
            this.Put(199, "guillemotleft");
            this.Put(200, "guillemotright");
            this.Put(220, "guilsinglleft");
            this.Put(221, "guilsinglright");
            this.Put(104, "h");
            this.Put(253, "hungarumlaut");
            this.Put(45, "hyphen");
            this.Put(105, "i");
            this.Put(146, "iacute");
            this.Put(148, "icircumflex");
            this.Put(149, "idieresis");
            this.Put(147, "igrave");
            this.Put(106, "j");
            this.Put(107, "k");
            this.Put(108, "l");
            this.Put(60, "less");
            this.Put(194, "logicalnot");
            this.Put(109, "m");
            this.Put(248, "macron");
            this.Put(181, "mu");
            this.Put(110, "n");
            this.Put(57, "nine");
            this.Put(150, "ntilde");
            this.Put(35, "numbersign");
            this.Put(111, "o");
            this.Put(151, "oacute");
            this.Put(153, "ocircumflex");
            this.Put(154, "odieresis");
            this.Put(207, "oe");
            this.Put(254, "ogonek");
            this.Put(152, "ograve");
            this.Put(49, "one");
            this.Put(187, "ordfeminine");
            this.Put(188, "ordmasculine");
            this.Put(191, "oslash");
            this.Put(155, "otilde");
            this.Put(112, "p");
            this.Put(166, "paragraph");
            this.Put(40, "parenleft");
            this.Put(41, "parenright");
            this.Put(37, "percent");
            this.Put(46, "period");
            this.Put(225, "periodcentered");
            this.Put(228, "perthousand");
            this.Put(43, "plus");
            this.Put(177, "plusminus");
            this.Put(113, "q");
            this.Put(63, "question");
            this.Put(192, "questiondown");
            this.Put(34, "quotedbl");
            this.Put(227, "quotedblbase");
            this.Put(210, "quotedblleft");
            this.Put(211, "quotedblright");
            this.Put(212, "quoteleft");
            this.Put(213, "quoteright");
            this.Put(226, "quotesinglbase");
            this.Put(39, "quotesingle");
            this.Put(114, "r");
            this.Put(168, "registered");
            this.Put(251, "ring");
            this.Put(115, "s");
            this.Put(164, "section");
            this.Put(59, "semicolon");
            this.Put(55, "seven");
            this.Put(54, "six");
            this.Put(47, "slash");
            this.Put(32, "space");
            this.Put(163, "sterling");
            this.Put(116, "t");
            this.Put(51, "three");
            this.Put(247, "tilde");
            this.Put(170, "trademark");
            this.Put(50, "two");
            this.Put(117, "u");
            this.Put(156, "uacute");
            this.Put(158, "ucircumflex");
            this.Put(159, "udieresis");
            this.Put(157, "ugrave");
            this.Put(95, "underscore");
            this.Put(118, "v");
            this.Put(119, "w");
            this.Put(120, "x");
            this.Put(121, "y");
            this.Put(216, "ydieresis");
            this.Put(180, "yen");
            this.Put(122, "z");
            this.Put(48, "zero");
        }
    }
}
