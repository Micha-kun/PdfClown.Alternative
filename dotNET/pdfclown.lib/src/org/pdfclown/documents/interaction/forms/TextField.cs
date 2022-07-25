/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

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
    using System;
    using org.pdfclown.documents.contents.composition;
    using org.pdfclown.documents.contents.objects;
    using org.pdfclown.documents.contents.tokens;
    using org.pdfclown.documents.contents.xObjects;
    using org.pdfclown.documents.interaction.annotations;
    using org.pdfclown.objects;

    using org.pdfclown.util;
    using bytes = org.pdfclown.bytes;
    using fonts = org.pdfclown.documents.contents.fonts;

    /**
      <summary>Text field [PDF:1.6:8.6.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class TextField
      : Field
    {

        internal TextField(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }
        /**
<summary>Creates a new text field within the given document context.</summary>
*/
        public TextField(
          string name,
          Widget widget,
          string value
          ) : base(
            PdfName.Tx,
            name,
            widget
            )
        { this.Value = value; }

        private void RefreshAppearance(
  )
        {
            var widget = this.Widgets[0];
            FormXObject normalAppearance;
            var normalAppearances = widget.Appearance.Normal;
            normalAppearance = normalAppearances[null];
            if (normalAppearance == null)
            { normalAppearances[null] = normalAppearance = new FormXObject(this.Document, widget.Box.Size); }
            PdfName fontName = null;
            double fontSize = 0;
            var defaultAppearanceState = this.DefaultAppearanceState;
            if (defaultAppearanceState == null)
            {
                // Retrieving the font to define the default appearance...
                fonts::Font defaultFont = null;
                PdfName defaultFontName = null;
                // Field fonts.
                var normalAppearanceFonts = normalAppearance.Resources.Fonts;
                foreach (var entry in normalAppearanceFonts)
                {
                    if (!entry.Value.Symbolic)
                    {
                        defaultFont = entry.Value;
                        defaultFontName = entry.Key;
                        break;
                    }
                }
                if (defaultFontName == null)
                {
                    // Common fonts.
                    var formFonts = this.Document.Form.Resources.Fonts;
                    foreach (var entry in formFonts)
                    {
                        if (!entry.Value.Symbolic)
                        {
                            defaultFont = entry.Value;
                            defaultFontName = entry.Key;
                            break;
                        }
                    }
                    if (defaultFontName == null)
                    {
                        //TODO:manage name collision!
                        formFonts[
                          defaultFontName = new PdfName("default")
                          ] = defaultFont = new fonts::StandardType1Font(
                            this.Document,
                            fonts::StandardType1Font.FamilyEnum.Helvetica,
                            false,
                            false
                            );
                    }
                    normalAppearanceFonts[defaultFontName] = defaultFont;
                }
                var buffer = new bytes::Buffer();
                new SetFont(defaultFontName, this.IsMultiline ? 10 : 0).WriteTo(buffer, this.Document);
                widget.BaseDataObject[PdfName.DA] = defaultAppearanceState = new PdfString(buffer.ToByteArray());
            }

            // Retrieving the font to use...
            var parser = new ContentParser(defaultAppearanceState.ToByteArray());
            foreach (var content in parser.ParseContentObjects())
            {
                if (content is SetFont)
                {
                    var setFontOperation = (SetFont)content;
                    fontName = setFontOperation.Name;
                    fontSize = setFontOperation.Size;
                    break;
                }
            }
            normalAppearance.Resources.Fonts[fontName] = this.Document.Form.Resources.Fonts[fontName];

            // Refreshing the field appearance...
            /*
             * TODO: resources MUST be resolved both through the apperance stream resource dictionary and
             * from the DR-entry acroform resource dictionary
             */
            var baseComposer = new PrimitiveComposer(normalAppearance);
            var composer = new BlockComposer(baseComposer);
            var currentLevel = composer.Scanner;
            var textShown = false;
            while (currentLevel != null)
            {
                if (!currentLevel.MoveNext())
                {
                    currentLevel = currentLevel.ParentLevel;
                    continue;
                }

                var content = currentLevel.Current;
                if (content is MarkedContent)
                {
                    var markedContent = (MarkedContent)content;
                    if (PdfName.Tx.Equals(((BeginMarkedContent)markedContent.Header).Tag))
                    {
                        // Remove old text representation!
                        markedContent.Objects.Clear();
                        // Add new text representation!
                        baseComposer.Scanner = currentLevel.ChildLevel; // Ensures the composer places new contents within the marked content block.
                        this.ShowText(composer, fontName, fontSize);
                        textShown = true;
                    }
                }
                else if (content is Text)
                { _ = currentLevel.Remove(); }
                else if (currentLevel.ChildLevel != null)
                { currentLevel = currentLevel.ChildLevel; }
            }
            if (!textShown)
            {
                _ = baseComposer.BeginMarkedContent(PdfName.Tx);
                this.ShowText(composer, fontName, fontSize);
                baseComposer.End();
            }
            baseComposer.Flush();
        }

        private void ShowText(
          BlockComposer composer,
          PdfName fontName,
          double fontSize
          )
        {
            var baseComposer = composer.BaseComposer;
            var scanner = baseComposer.Scanner;
            var textBox = scanner.ContentContext.Box;
            if (scanner.State.Font == null)
            {
                /*
                  NOTE: A zero value for size means that the font is to be auto-sized: its size is computed as
                  a function of the height of the annotation rectangle.
                */
                if (fontSize == 0)
                { fontSize = textBox.Height * 0.65; }
                baseComposer.SetFont(fontName, fontSize);
            }

            var text = (string)this.Value;

            var flags = this.Flags;
            if (((flags & FlagsEnum.Comb) == FlagsEnum.Comb)
              && ((flags & FlagsEnum.FileSelect) == 0)
              && ((flags & FlagsEnum.Multiline) == 0)
              && ((flags & FlagsEnum.Password) == 0))
            {
                var maxLength = this.MaxLength;
                if (maxLength > 0)
                {
                    textBox.Width /= maxLength;
                    for (int index = 0, length = text.Length; index < length; index++)
                    {
                        composer.Begin(
                          textBox,
                          XAlignmentEnum.Center,
                          YAlignmentEnum.Middle
                          );
                        _ = composer.ShowText(text[index].ToString());
                        composer.End();
                        textBox.X += textBox.Width;
                    }
                    return;
                }
            }

            textBox.X += 2;
            textBox.Width -= 4;
            YAlignmentEnum yAlignment;
            if ((flags & FlagsEnum.Multiline) == FlagsEnum.Multiline)
            {
                yAlignment = YAlignmentEnum.Top;
                textBox.Y += (float)(fontSize * .35);
                textBox.Height -= (float)(fontSize * .7);
            }
            else
            {
                yAlignment = YAlignmentEnum.Middle;
            }
            composer.Begin(
              textBox,
              this.Justification.ToXAlignment(),
              yAlignment
              );
            _ = composer.ShowText(text);
            composer.End();
        }

        /**
<summary>Gets/Sets whether the field can contain multiple lines of text.</summary>
*/
        public bool IsMultiline
        {
            get => (this.Flags & FlagsEnum.Multiline) == FlagsEnum.Multiline;
            set => this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.Multiline, value);
        }

        /**
          <summary>Gets/Sets whether the field is intended for entering a secure password.</summary>
        */
        public bool IsPassword
        {
            get => (this.Flags & FlagsEnum.Password) == FlagsEnum.Password;
            set => this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.Password, value);
        }

        /**
          <summary>Gets/Sets the justification to be used in displaying this field's text.</summary>
        */
        public JustificationEnum Justification
        {
            get => JustificationEnumExtension.Get((PdfInteger)this.BaseDataObject[PdfName.Q]);
            set => this.BaseDataObject[PdfName.Q] = value.GetCode();
        }

        /**
          <summary>Gets/Sets the maximum length of the field's text, in characters.</summary>
          <remarks>It corresponds to the maximum integer value in case no explicit limit is defined.</remarks>
        */
        public int MaxLength
        {
            get
            {
                var maxLengthObject = (PdfInteger)PdfObject.Resolve(this.GetInheritableAttribute(PdfName.MaxLen));
                return (maxLengthObject != null) ? maxLengthObject.IntValue : int.MaxValue;
            }
            set => this.BaseDataObject[PdfName.MaxLen] = (value != int.MaxValue) ? PdfInteger.Get(value) : null;
        }

        /**
          <summary>Gets/Sets whether text entered in the field is spell-checked.</summary>
        */
        public bool SpellChecked
        {
            get => (this.Flags & FlagsEnum.DoNotSpellCheck) != FlagsEnum.DoNotSpellCheck;
            set => this.Flags = EnumUtils.Mask(this.Flags, FlagsEnum.DoNotSpellCheck, !value);
        }

        /**
          <returns>Either a string or an <see cref="IBuffer"/>.</returns>
        */
        public override object Value
        {
            get
            {
                var valueObject = PdfObject.Resolve(this.GetInheritableAttribute(PdfName.V));
                if (valueObject is PdfString)
                {
                    return ((PdfString)valueObject).Value;
                }
                else if (valueObject is PdfStream)
                {
                    return ((PdfStream)valueObject).Body;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (!((value == null)
                    || (value is string)
                    || (value is bytes::IBuffer)))
                {
                    throw new ArgumentException("Value MUST be either a String or an IBuffer");
                }

                if (value != null)
                {
                    var oldValueObject = this.BaseDataObject.Resolve(PdfName.V);
                    bytes::IBuffer valueObjectBuffer = null;
                    if (oldValueObject is PdfStream)
                    {
                        valueObjectBuffer = ((PdfStream)oldValueObject).Body;
                        valueObjectBuffer.SetLength(0);
                    }
                    if (value is string)
                    {
                        if (valueObjectBuffer != null)
                        { _ = valueObjectBuffer.Append((string)value); }
                        else
                        { this.BaseDataObject[PdfName.V] = new PdfTextString((string)value); }
                    }
                    else // IBuffer.
                    {
                        if (valueObjectBuffer != null)
                        { _ = valueObjectBuffer.Append((bytes::IBuffer)value); }
                        else
                        { this.BaseDataObject[PdfName.V] = this.File.Register(new PdfStream((bytes::IBuffer)value)); }
                    }
                }
                else
                { this.BaseDataObject[PdfName.V] = null; }

                this.RefreshAppearance();
            }
        }
    }
}