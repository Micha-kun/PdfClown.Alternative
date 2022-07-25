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
      <summary>ZapfDingbats encoding [PDF:1.7:D.5].</summary>
    */
    internal sealed class ZapfDingbatsEncoding
      : Encoding
    {
        public ZapfDingbatsEncoding(
          )
        {
            this.Put(32, '\u0020');
            this.Put(33, '\u2701');
            this.Put(34, '\u2702');
            this.Put(35, '\u2703');
            this.Put(36, '\u2704');
            this.Put(37, '\u260E');
            this.Put(38, '\u2706');
            this.Put(39, '\u2707');
            this.Put(40, '\u2708');
            this.Put(41, '\u2709');
            this.Put(42, '\u261B');
            this.Put(43, '\u261E');
            this.Put(44, '\u270C');
            this.Put(45, '\u270D');
            this.Put(46, '\u270E');
            this.Put(47, '\u270F');
            this.Put(48, '\u2710');
            this.Put(49, '\u2711');
            this.Put(50, '\u2712');
            this.Put(51, '\u2713');
            this.Put(52, '\u2714');
            this.Put(53, '\u2715');
            this.Put(54, '\u2716');
            this.Put(55, '\u2717');
            this.Put(56, '\u2718');
            this.Put(57, '\u2719');
            this.Put(58, '\u271A');
            this.Put(59, '\u271B');
            this.Put(60, '\u271C');
            this.Put(61, '\u271D');
            this.Put(62, '\u271E');
            this.Put(63, '\u271F');
            this.Put(64, '\u2720');
            this.Put(65, '\u2721');
            this.Put(66, '\u2722');
            this.Put(67, '\u2723');
            this.Put(68, '\u2724');
            this.Put(69, '\u2725');
            this.Put(70, '\u2726');
            this.Put(71, '\u2727');
            this.Put(72, '\u2605');
            this.Put(73, '\u2729');
            this.Put(74, '\u272A');
            this.Put(75, '\u272B');
            this.Put(76, '\u272C');
            this.Put(77, '\u272D');
            this.Put(78, '\u272E');
            this.Put(79, '\u272F');
            this.Put(80, '\u2730');
            this.Put(81, '\u2731');
            this.Put(82, '\u2732');
            this.Put(83, '\u2733');
            this.Put(84, '\u2734');
            this.Put(85, '\u2735');
            this.Put(86, '\u2736');
            this.Put(87, '\u2737');
            this.Put(88, '\u2738');
            this.Put(89, '\u2739');
            this.Put(90, '\u273A');
            this.Put(91, '\u273B');
            this.Put(92, '\u273C');
            this.Put(93, '\u273D');
            this.Put(94, '\u273E');
            this.Put(95, '\u273F');
            this.Put(96, '\u2740');
            this.Put(97, '\u2741');
            this.Put(98, '\u2742');
            this.Put(99, '\u2743');
            this.Put(100, '\u2744');
            this.Put(101, '\u2745');
            this.Put(102, '\u2746');
            this.Put(103, '\u2747');
            this.Put(104, '\u2748');
            this.Put(105, '\u2749');
            this.Put(106, '\u274A');
            this.Put(107, '\u274B');
            this.Put(108, '\u25CF');
            this.Put(109, '\u274D');
            this.Put(110, '\u25A0');
            this.Put(111, '\u274F');
            this.Put(112, '\u2750');
            this.Put(113, '\u2751');
            this.Put(114, '\u2752');
            this.Put(115, '\u25B2');
            this.Put(116, '\u25BC');
            this.Put(117, '\u25C6');
            this.Put(118, '\u2756');
            this.Put(119, '\u25D7');
            this.Put(120, '\u2759');
            this.Put(121, '\u2758');
            this.Put(122, '\u275A');
            this.Put(123, '\u275B');
            this.Put(124, '\u275C');
            this.Put(125, '\u275D');
            this.Put(126, '\u275E');

            // BEGIN: Undocumented range (parenthesis).
            this.Put(128, '\u2768');
            this.Put(129, '\u2769');
            this.Put(130, '\u276A');
            this.Put(131, '\u276B');
            this.Put(132, '\u276C');
            this.Put(133, '\u276D');
            this.Put(134, '\u276E');
            this.Put(135, '\u276F');
            this.Put(136, '\u2770');
            this.Put(137, '\u2771');
            this.Put(138, '\u2772');
            this.Put(139, '\u2773');
            this.Put(140, '\u2774');
            this.Put(141, '\u2775');
            // END: Undocumented range (parenthesis).

            this.Put(161, '\u2761');
            this.Put(162, '\u2762');
            this.Put(163, '\u2763');
            this.Put(164, '\u2764');
            this.Put(165, '\u2765');
            this.Put(166, '\u2766');
            this.Put(167, '\u2767');
            this.Put(168, '\u2663');
            this.Put(169, '\u2666');
            this.Put(170, '\u2665');
            this.Put(171, '\u2660');
            this.Put(172, '\u2460');
            this.Put(173, '\u2461');
            this.Put(174, '\u2462');
            this.Put(175, '\u2463');
            this.Put(176, '\u2464');
            this.Put(177, '\u2465');
            this.Put(178, '\u2466');
            this.Put(179, '\u2467');
            this.Put(180, '\u2468');
            this.Put(181, '\u2469');
            this.Put(182, '\u2776');
            this.Put(183, '\u2777');
            this.Put(184, '\u2778');
            this.Put(185, '\u2779');
            this.Put(186, '\u277A');
            this.Put(187, '\u277B');
            this.Put(188, '\u277C');
            this.Put(189, '\u277D');
            this.Put(190, '\u277E');
            this.Put(191, '\u277F');
            this.Put(192, '\u2780');
            this.Put(193, '\u2781');
            this.Put(194, '\u2782');
            this.Put(195, '\u2783');
            this.Put(196, '\u2784');
            this.Put(197, '\u2785');
            this.Put(198, '\u2786');
            this.Put(199, '\u2787');
            this.Put(200, '\u2788');
            this.Put(201, '\u2789');
            this.Put(202, '\u278A');
            this.Put(203, '\u278B');
            this.Put(204, '\u278C');
            this.Put(205, '\u278D');
            this.Put(206, '\u278E');
            this.Put(207, '\u278F');
            this.Put(208, '\u2790');
            this.Put(209, '\u2791');
            this.Put(210, '\u2792');
            this.Put(211, '\u2793');
            this.Put(212, '\u2794');
            this.Put(213, '\u2192');
            this.Put(214, '\u2194');
            this.Put(215, '\u2195');
            this.Put(216, '\u2798');
            this.Put(217, '\u2799');
            this.Put(218, '\u279A');
            this.Put(219, '\u279B');
            this.Put(220, '\u279C');
            this.Put(221, '\u279D');
            this.Put(222, '\u279E');
            this.Put(223, '\u279F');
            this.Put(224, '\u27A0');
            this.Put(225, '\u27A1');
            this.Put(226, '\u27A2');
            this.Put(227, '\u27A3');
            this.Put(228, '\u27A4');
            this.Put(229, '\u27A5');
            this.Put(230, '\u27A6');
            this.Put(231, '\u27A7');
            this.Put(232, '\u27A8');
            this.Put(233, '\u27A9');
            this.Put(234, '\u27AA');
            this.Put(235, '\u27AB');
            this.Put(236, '\u27AC');
            this.Put(237, '\u27AD');
            this.Put(238, '\u27AE');
            this.Put(239, '\u27AF');
            this.Put(241, '\u27B1');
            this.Put(242, '\u27B2');
            this.Put(243, '\u27B3');
            this.Put(244, '\u27B4');
            this.Put(245, '\u27B5');
            this.Put(246, '\u27B6');
            this.Put(247, '\u27B7');
            this.Put(248, '\u27B8');
            this.Put(249, '\u27B9');
            this.Put(250, '\u27BA');
            this.Put(251, '\u27BB');
            this.Put(252, '\u27BC');
            this.Put(253, '\u27BD');
            this.Put(254, '\u27BE');
        }
    }
}
