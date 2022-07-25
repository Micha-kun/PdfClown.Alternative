/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.interaction.forms
{
    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.objects;

    using org.pdfclown.util;

    /**
      <summary>Combo box [PDF:1.6:8.6.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class ComboBox
      : ChoiceField
    {

        internal ComboBox(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        /**
<summary>Creates a new combobox within the given document context.</summary>
*/
        public ComboBox(
          string name,
          Widget widget
          ) : base(name, widget)
        { this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.Combo, true); }

        /**
<summary>Gets/Sets whether the text is editable.</summary>
*/
        public bool Editable
        {
            get => (this.Flags & FlagsEnum.Edit) == FlagsEnum.Edit;
            set => this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.Edit, value);
        }

        /**
          <summary>Gets/Sets whether the edited text is spell checked.</summary>
        */
        public bool SpellChecked
        {
            get => (this.Flags & FlagsEnum.DoNotSpellCheck) != FlagsEnum.DoNotSpellCheck;
            set => this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.DoNotSpellCheck, !value);
        }
    }
}