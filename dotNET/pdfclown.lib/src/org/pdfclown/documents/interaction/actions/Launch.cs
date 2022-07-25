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

namespace org.pdfclown.documents.interaction.actions
{
    using System;
    using System.Collections.Generic;

    using org.pdfclown.documents.files;
    using org.pdfclown.objects;

    /**
      <summary>'Launch an application' action [PDF:1.6:8.5.3].</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public sealed class Launch
      : Action
    {

        internal Launch(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        /**
<summary>Creates a launcher.</summary>
<param name="context">Document context.</param>
<param name="target">Either a <see cref="FileSpecification"/> or a <see cref="WinTarget"/>
representing either an application or a document.</param>
*/
        public Launch(
          Document context,
          PdfObjectWrapper target
          ) : base(context, PdfName.Launch)
        { this.Target = target; }

        /**
<summary>Gets/Sets the action options.</summary>
*/
        public OptionsEnum Options
        {
            get
            {
                OptionsEnum options = 0;
                var optionsObject = this.BaseDataObject[PdfName.NewWindow];
                if ((optionsObject != null)
                  && ((PdfBoolean)optionsObject).BooleanValue)
                { options |= OptionsEnum.NewWindow; }
                return options;
            }
            set
            {
                if ((value & OptionsEnum.NewWindow) == OptionsEnum.NewWindow)
                { this.BaseDataObject[PdfName.NewWindow] = PdfBoolean.True; }
                else if ((value & OptionsEnum.SameWindow) == OptionsEnum.SameWindow)
                { this.BaseDataObject[PdfName.NewWindow] = PdfBoolean.False; }
                else
                { _ = this.BaseDataObject.Remove(PdfName.NewWindow); } // NOTE: Forcing the absence of this entry ensures that the viewer application should behave in accordance with the current user preference.
            }
        }

        /**
          <summary>Gets/Sets the application to be launched or the document to be opened or printed.
          </summary>
        */
        public PdfObjectWrapper Target
        {
            get
            {
                PdfDirectObject targetObject;
                if ((targetObject = this.BaseDataObject[PdfName.F]) != null)
                {
                    return FileSpecification.Wrap(targetObject);
                }
                else if ((targetObject = this.BaseDataObject[PdfName.Win]) != null)
                {
                    return new WinTarget(targetObject);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value is FileSpecification)
                { this.BaseDataObject[PdfName.F] = ((FileSpecification)value).BaseObject; }
                else if (value is WinTarget)
                { this.BaseDataObject[PdfName.Win] = ((WinTarget)value).BaseObject; }
                else
                {
                    throw new ArgumentException("MUST be either FileSpecification or WinTarget");
                }
            }
        }
        /**
  <summary>Windows-specific launch parameters [PDF:1.6:8.5.3].</summary>
*/
        public class WinTarget
          : PdfObjectWrapper<PdfDictionary>
        {

            private static readonly Dictionary<OperationEnum, PdfString> OperationEnumCodes;

            static WinTarget()
            {
                OperationEnumCodes = new Dictionary<OperationEnum, PdfString>();
                OperationEnumCodes[OperationEnum.Open] = new PdfString("open");
                OperationEnumCodes[OperationEnum.Print] = new PdfString("print");
            }

            internal WinTarget(
              PdfDirectObject baseObject
              ) : base(baseObject)
            { }

            public WinTarget(
Document context,
string fileName
) : base(context, new PdfDictionary())
            { this.FileName = fileName; }

            public WinTarget(
              Document context,
              string fileName,
              OperationEnum operation
              ) : this(context, fileName)
            { this.Operation = operation; }

            public WinTarget(
              Document context,
              string fileName,
              string parameterString
              ) : this(context, fileName)
            { this.ParameterString = parameterString; }

            /**
<summary>Gets the code corresponding to the given value.</summary>
*/
            private static PdfString ToCode(
              OperationEnum value
              )
            { return OperationEnumCodes[value]; }

            /**
              <summary>Gets the operation corresponding to the given value.</summary>
            */
            private static OperationEnum ToOperationEnum(
              PdfString value
              )
            {
                foreach (var operation in OperationEnumCodes)
                {
                    if (operation.Value.Equals(value))
                    {
                        return operation.Key;
                    }
                }
                return OperationEnum.Open;
            }

            public override object Clone(
Document context
)
            { throw new NotImplementedException(); }

            /**
              <summary>Gets/Sets the default directory.</summary>
            */
            public string DefaultDirectory
            {
                get
                {
                    var defaultDirectoryObject = (PdfString)this.BaseDataObject[PdfName.D];
                    return (defaultDirectoryObject != null) ? ((string)defaultDirectoryObject.Value) : null;
                }
                set => this.BaseDataObject[PdfName.D] = new PdfString(value);
            }

            /**
              <summary>Gets/Sets the file name of the application to be launched
              or the document to be opened or printed.</summary>
            */
            public string FileName
            {
                get => (string)((PdfString)this.BaseDataObject[PdfName.F]).Value;
                set => this.BaseDataObject[PdfName.F] = new PdfString(value);
            }

            /**
              <summary>Gets/Sets the operation to perform.</summary>
            */
            public OperationEnum Operation
            {
                get => ToOperationEnum((PdfString)this.BaseDataObject[PdfName.O]);
                set => this.BaseDataObject[PdfName.O] = ToCode(value);
            }

            /**
              <summary>Gets/Sets the parameter string to be passed to the application.</summary>
            */
            public string ParameterString
            {
                get
                {
                    var parameterStringObject = (PdfString)this.BaseDataObject[PdfName.P];
                    return (parameterStringObject != null) ? ((string)parameterStringObject.Value) : null;
                }
                set => this.BaseDataObject[PdfName.P] = new PdfString(value);
            }
            /**
  <summary>Operation [PDF:1.6:8.5.3].</summary>
*/
            public enum OperationEnum
            {
                /**
                  <summary>Open.</summary>
                */
                Open,
                /**
                  <summary>Print.</summary>
                */
                Print
            };
        }
    }
}