/*
  Copyright 2015 Stefano Chizzolini. http://www.pdfclown.org

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
      <summary>Symbol encoding [PDF:1.7:D.4].</summary>
    */
    internal sealed class SymbolEncoding
      : Encoding
    {
        public SymbolEncoding(
          )
        {
            this.Put(32, "space");
            this.Put(33, "exclam");
            this.Put(34, "universal");
            this.Put(35, "numbersign");
            this.Put(36, "existential");
            this.Put(37, "percent");
            this.Put(38, "ampersand");
            this.Put(39, "suchthat");
            this.Put(40, "parenleft");
            this.Put(41, "parenright");
            this.Put(42, "asteriskmath");
            this.Put(43, "plus");
            this.Put(44, "comma");
            this.Put(45, "minus");
            this.Put(46, "period");
            this.Put(47, "slash");
            this.Put(48, "zero");
            this.Put(49, "one");
            this.Put(50, "two");
            this.Put(51, "three");
            this.Put(52, "four");
            this.Put(53, "five");
            this.Put(54, "six");
            this.Put(55, "seven");
            this.Put(56, "eight");
            this.Put(57, "nine");
            this.Put(58, "colon");
            this.Put(59, "semicolon");
            this.Put(60, "less");
            this.Put(61, "equal");
            this.Put(62, "greater");
            this.Put(63, "question");
            this.Put(64, "congruent");
            this.Put(65, "Alpha");
            this.Put(66, "Beta");
            this.Put(67, "Chi");
            this.Put(68, "Delta");
            this.Put(69, "Epsilon");
            this.Put(70, "Phi");
            this.Put(71, "Gamma");
            this.Put(72, "Eta");
            this.Put(73, "Iota");
            this.Put(74, "theta1");
            this.Put(75, "Kappa");
            this.Put(76, "Lambda");
            this.Put(77, "Mu");
            this.Put(78, "Nu");
            this.Put(79, "Omicron");
            this.Put(80, "Pi");
            this.Put(81, "Theta");
            this.Put(82, "Rho");
            this.Put(83, "Sigma");
            this.Put(84, "Tau");
            this.Put(85, "Upsilon");
            this.Put(86, "sigma1");
            this.Put(87, "Omega");
            this.Put(88, "Xi");
            this.Put(89, "Psi");
            this.Put(90, "Zeta");
            this.Put(91, "bracketleft");
            this.Put(92, "therefore");
            this.Put(93, "bracketright");
            this.Put(94, "perpendicular");
            this.Put(95, "underscore");
            this.Put(96, "radicalex");
            this.Put(97, "alpha");
            this.Put(98, "beta");
            this.Put(99, "chi");
            this.Put(100, "delta");
            this.Put(101, "epsilon");
            this.Put(102, "phi");
            this.Put(103, "gamma");
            this.Put(104, "eta");
            this.Put(105, "iota");
            this.Put(106, "phi1");
            this.Put(107, "kappa");
            this.Put(108, "lambda");
            this.Put(109, "mu");
            this.Put(110, "nu");
            this.Put(111, "omicron");
            this.Put(112, "pi");
            this.Put(113, "theta");
            this.Put(114, "rho");
            this.Put(115, "sigma");
            this.Put(116, "tau");
            this.Put(117, "upsilon");
            this.Put(118, "omega1");
            this.Put(119, "omega");
            this.Put(120, "xi");
            this.Put(121, "psi");
            this.Put(122, "zeta");
            this.Put(123, "braceleft");
            this.Put(124, "bar");
            this.Put(125, "braceright");
            this.Put(126, "similar");
            this.Put(160, "Euro");
            this.Put(161, "Upsilon1");
            this.Put(162, "minute");
            this.Put(163, "lessequal");
            this.Put(164, "fraction");
            this.Put(165, "infinity");
            this.Put(166, "florin");
            this.Put(167, "club");
            this.Put(168, "diamond");
            this.Put(169, "heart");
            this.Put(170, "spade");
            this.Put(171, "arrowboth");
            this.Put(172, "arrowleft");
            this.Put(173, "arrowup");
            this.Put(174, "arrowright");
            this.Put(175, "arrowdown");
            this.Put(176, "degree");
            this.Put(177, "plusminus");
            this.Put(178, "second");
            this.Put(179, "greaterequal");
            this.Put(180, "multiply");
            this.Put(181, "proportional");
            this.Put(182, "partialdiff");
            this.Put(183, "bullet");
            this.Put(184, "divide");
            this.Put(185, "notequal");
            this.Put(186, "equivalence");
            this.Put(187, "approxequal");
            this.Put(188, "ellipsis");
            this.Put(189, "arrowvertex");
            this.Put(190, "arrowhorizex");
            this.Put(191, "carriagereturn");
            this.Put(192, "aleph");
            this.Put(193, "Ifraktur");
            this.Put(194, "Rfraktur");
            this.Put(195, "weierstrass");
            this.Put(196, "circlemultiply");
            this.Put(197, "circleplus");
            this.Put(198, "emptyset");
            this.Put(199, "intersection");
            this.Put(200, "union");
            this.Put(201, "propersuperset");
            this.Put(202, "reflexsuperset");
            this.Put(203, "notsubset");
            this.Put(204, "propersubset");
            this.Put(205, "reflexsubset");
            this.Put(206, "element");
            this.Put(207, "notelement");
            this.Put(208, "angle");
            this.Put(209, "gradient");
            this.Put(210, "registerserif");
            this.Put(211, "copyrightserif");
            this.Put(212, "trademarkserif");
            this.Put(213, "product");
            this.Put(214, "radical");
            this.Put(215, "dotmath");
            this.Put(216, "logicalnot");
            this.Put(217, "logicaland");
            this.Put(218, "logicalor");
            this.Put(219, "arrowdblboth");
            this.Put(220, "arrowdblleft");
            this.Put(221, "arrowdblup");
            this.Put(222, "arrowdblright");
            this.Put(223, "arrowdbldown");
            this.Put(224, "lozenge");
            this.Put(225, "angleleft");
            this.Put(226, "registersans");
            this.Put(227, "copyrightsans");
            this.Put(228, "trademarksans");
            this.Put(229, "summation");
            this.Put(230, "parenlefttp");
            this.Put(231, "parenleftex");
            this.Put(232, "parenleftbt");
            this.Put(233, "bracketlefttp");
            this.Put(234, "bracketleftex");
            this.Put(235, "bracketleftbt");
            this.Put(236, "bracelefttp");
            this.Put(237, "braceleftmid");
            this.Put(238, "braceleftbt");
            this.Put(239, "braceex");
            this.Put(241, "angleright");
            this.Put(242, "integral");
            this.Put(243, "integraltp");
            this.Put(244, "integralex");
            this.Put(245, "integralbt");
            this.Put(246, "parenrighttp");
            this.Put(247, "parenrightex");
            this.Put(248, "parenrightbt");
            this.Put(249, "bracketrighttp");
            this.Put(250, "bracketrightex");
            this.Put(251, "bracketrightbt");
            this.Put(252, "bracerighttp");
            this.Put(253, "bracerightmid");
            this.Put(254, "bracerightbt");
        }
    }
}
