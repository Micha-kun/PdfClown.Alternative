/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

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



namespace org.pdfclown.objects
{
    using System;
    using org.pdfclown.bytes;
    using org.pdfclown.documents.files;

    using org.pdfclown.files;
    using org.pdfclown.tokens;

    /**
      <summary>PDF stream object [PDF:1.6:3.2.7].</summary>
    */
    public class PdfStream
      : PdfDataObject,
        IFileResource
    {
        private static readonly byte[] BeginStreamBodyChunk = Encoding.Pdf.Encode(Symbol.LineFeed + Keyword.BeginStream + Symbol.LineFeed);
        private static readonly byte[] EndStreamBodyChunk = Encoding.Pdf.Encode(Symbol.LineFeed + Keyword.EndStream);

        /**
          <summary>Indicates whether {@link #body} has already been resolved and therefore contains the
          actual stream data.</summary>
        */
        private bool bodyResolved;

        private PdfObject parent;
        private bool updateable = true;
        private bool updated;
        private bool virtual_;

        internal IBuffer body;
        internal PdfDictionary header;

        public PdfStream(
  ) : this(
    new PdfDictionary(),
    new bytes.Buffer()
    )
        { }

        public PdfStream(
          PdfDictionary header
          ) : this(
            header,
            new bytes.Buffer()
            )
        { }

        public PdfStream(
          IBuffer body
          ) : this(
            new PdfDictionary(),
            body
            )
        { }

        public PdfStream(
          PdfDictionary header,
          IBuffer body
          )
        {
            this.header = (PdfDictionary)this.Include(header);

            this.body = body;
            body.Dirty = false;
            body.OnChange += delegate (
              object sender,
              EventArgs args
              )
            { this.Update(); };
        }

        protected internal override bool Virtual
        {
            get => this.virtual_;
            set => this.virtual_ = value;
        }

        public override PdfObject Accept(
IVisitor visitor,
object data
)
        { return visitor.Visit(this, data); }

        /**
          <summary>Gets the stream body.</summary>
          <param name="decode">Defines whether the body has to be decoded.</param>
        */
        public IBuffer GetBody(
          bool decode
          )
        {
            if (!this.bodyResolved)
            {
                /*
                  NOTE: In case of stream data from external file, a copy to the local buffer has to be done.
                */
                var dataFile = this.DataFile;
                if (dataFile != null)
                {
                    this.Updateable = false;
                    this.body.Clear();
                    this.body.Write(dataFile.GetInputStream());
                    this.body.Dirty = false;
                    this.Updateable = true;
                }
                this.bodyResolved = true;
            }
            if (decode)
            {
                PdfDataObject filter = this.Filter;
                if (filter != null) // Stream encoded.
                {
                    this.header.Updateable = false;
                    PdfDataObject parameters = this.Parameters;
                    if (filter is PdfName) // Single filter.
                    {
                        this.body.Decode(
                          bytes.filters.Filter.Get((PdfName)filter),
                          (PdfDictionary)parameters
                          );
                    }
                    else // Multiple filters.
                    {
                        var filterIterator = ((PdfArray)filter).GetEnumerator();
                        var parametersIterator = (parameters != null) ? ((PdfArray)parameters).GetEnumerator() : null;
                        while (filterIterator.MoveNext())
                        {
                            PdfDictionary filterParameters;
                            if (parametersIterator == null)
                            { filterParameters = null; }
                            else
                            {
                                _ = parametersIterator.MoveNext();
                                filterParameters = (PdfDictionary)Resolve(parametersIterator.Current);
                            }
                            this.body.Decode(bytes.filters.Filter.Get((PdfName)Resolve(filterIterator.Current)), filterParameters);
                        }
                    }
                    // The stream is free from encodings.
                    this.Filter = null;
                    this.Parameters = null;
                    this.header.Updateable = true;
                }
            }
            return this.body;
        }

        /**
          <param name="preserve">Indicates whether the data from the old data source substitutes the
          new one. This way data can be imported to/exported from local or preserved in case of external
          file location changed.</param>
          <seealso cref="DataFile"/>
        */
        public void SetDataFile(
          FileSpecification value,
          bool preserve
          )
        {
            /*
              NOTE: If preserve argument is set to true, body's dirtiness MUST be forced in order to ensure
              data serialization to the new external location.

              Old data source | New data source | preserve | Action
              ----------------------------------------------------------------------------------------------
              local           | not null        | false     | A. Substitute local with new file.
              local           | not null        | true      | B. Export local to new file.
              external        | not null        | false     | C. Substitute old file with new file.
              external        | not null        | true      | D. Copy old file data to new file.
              local           | null            | (any)     | E. No action.
              external        | null            | false     | F. Empty local.
              external        | null            | true      | G. Import old file to local.
              ----------------------------------------------------------------------------------------------
            */
            var oldDataFile = this.DataFile;
            var dataFileObject = (value != null) ? value.BaseObject : null;
            if (value != null)
            {
                if (preserve)
                {
                    if (oldDataFile != null) // Case D (copy old file data to new file).
                    {
                        if (!this.bodyResolved)
                        {
                            // Transfer old file data to local!
                            _ = this.GetBody(false); // Ensures that external data is loaded as-is into the local buffer.
                        }
                    }
                    else // Case B (export local to new file).
                    {
                        // Transfer local settings to file!
                        this.header[PdfName.FFilter] = this.header[PdfName.Filter];
                        _ = this.header.Remove(PdfName.Filter);
                        this.header[PdfName.FDecodeParms] = this.header[PdfName.DecodeParms];
                        _ = this.header.Remove(PdfName.DecodeParms);

                        // Ensure local data represents actual data (otherwise it would be substituted by resolved file data)!
                        this.bodyResolved = true;
                    }
                    // Ensure local data has to be serialized to new file!
                    this.body.Dirty = true;
                }
                else // Case A/C (substitute local/old file with new file).
                {
                    // Dismiss local/old file data!
                    this.body.Clear();
                    // Dismiss local/old file settings!
                    this.Filter = null;
                    this.Parameters = null;
                    // Ensure local data has to be loaded from new file!
                    this.bodyResolved = false;
                }
            }
            else
            {
                if (oldDataFile != null)
                {
                    if (preserve) // Case G (import old file to local).
                    {
                        // Transfer old file data to local!
                        _ = this.GetBody(false); // Ensures that external data is loaded as-is into the local buffer.
                                                 // Transfer old file settings to local!
                        this.header[PdfName.Filter] = this.header[PdfName.FFilter];
                        _ = this.header.Remove(PdfName.FFilter);
                        this.header[PdfName.DecodeParms] = this.header[PdfName.FDecodeParms];
                        _ = this.header.Remove(PdfName.FDecodeParms);
                    }
                    else // Case F (empty local).
                    {
                        // Dismiss old file data!
                        this.body.Clear();
                        // Dismiss old file settings!
                        this.Filter = null;
                        this.Parameters = null;
                        // Ensure local data represents actual data (otherwise it would be substituted by resolved file data)!
                        this.bodyResolved = true;
                    }
                }
                else // E (no action).
                { /* NOOP */ }
            }
            this.header[PdfName.F] = dataFileObject;
        }

        public override PdfObject Swap(
          PdfObject other
          )
        {
            var otherStream = (PdfStream)other;
            var otherHeader = otherStream.header;
            var otherBody = otherStream.body;
            // Update the other!
            otherStream.header = this.header;
            otherStream.body = this.body;
            otherStream.Update();
            // Update this one!
            this.header = otherHeader;
            this.body = otherBody;
            this.Update();
            return this;
        }

        public override void WriteTo(
          IOutputStream stream,
          File context
          )
        {
            /*
              NOTE: The header is temporarily tweaked to accommodate serialization settings.
            */
            this.header.Updateable = false;

            byte[] bodyData = null;
            var filterApplied = false;
            /*
              NOTE: In case of external file, the body buffer has to be saved back only if the file was
              actually resolved (that is brought into the body buffer) and modified.
            */
            var dataFile = this.DataFile;
            if ((dataFile == null) || (this.bodyResolved && this.body.Dirty))
            {
                /*
                  NOTE: In order to keep the contents of metadata streams visible as plain text to tools
                  that are not PDF-aware, no filter is applied to them [PDF:1.7:10.2.2].
                */
                if ((this.Filter == null)
                   && context.Configuration.StreamFilterEnabled
                   && !PdfName.Metadata.Equals(this.header[PdfName.Type])) // Filter needed.
                {
                    // Apply the filter to the stream!
                    bodyData = this.body.Encode(bytes.filters.Filter.Get((PdfName)(this.Filter = PdfName.FlateDecode)), null);
                    filterApplied = true;
                }
                else // No filter needed.
                { bodyData = this.body.ToByteArray(); }

                if (dataFile != null)
                {
                    try
                    {
                        using (var dataFileOutputStream = dataFile.GetOutputStream())
                        { dataFileOutputStream.Write(bodyData); }
                    }
                    catch (Exception e)
                    { throw new Exception($"Data writing into {dataFile.Path} failed.", e); }
                }
            }
            if (dataFile != null)
            { bodyData = new byte[] { }; }

            // Set the encoded data length!
            this.header[PdfName.Length] = PdfInteger.Get(bodyData.Length);

            // 1. Header.
            this.header.WriteTo(stream, context);

            if (filterApplied)
            {
                // Restore actual header entries!
                this.header[PdfName.Length] = PdfInteger.Get((int)this.body.Length);
                this.Filter = null;
            }

            // 2. Body.
            stream.Write(BeginStreamBodyChunk);
            stream.Write(bodyData);
            stream.Write(EndStreamBodyChunk);

            this.header.Updateable = true;
        }

        /**
          <summary>Gets the decoded stream body.</summary>
        */
        public IBuffer Body =>
                /*
NOTE: Encoding filters are removed by default because they belong to a lower layer (token
layer), so that it's appropriate and consistent to transparently keep the object layer
unaware of such a facility.
*/
                this.GetBody(true);

        [PDF(VersionEnum.PDF12)]
        public FileSpecification DataFile
        {
            get => FileSpecification.Wrap(this.header[PdfName.F]);
            set => this.SetDataFile(value, false);
        }

        public PdfDirectObject Filter
        {
            get => (PdfDirectObject)((this.header[PdfName.F] == null)
                  ? this.header.Resolve(PdfName.Filter)
                  : this.header.Resolve(PdfName.FFilter));
            protected set => this.header[
                  (this.header[PdfName.F] == null)
                    ? PdfName.Filter
                    : PdfName.FFilter
                  ] = value;
        }

        /**
          <summary>Gets the stream header.</summary>
        */
        public PdfDictionary Header => this.header;

        public PdfDirectObject Parameters
        {
            get => (PdfDirectObject)((this.header[PdfName.F] == null)
                  ? this.header.Resolve(PdfName.DecodeParms)
                  : this.header.Resolve(PdfName.FDecodeParms));
            protected set => this.header[
                  (this.header[PdfName.F] == null)
                    ? PdfName.DecodeParms
                    : PdfName.FDecodeParms
                  ] = value;
        }

        public override PdfObject Parent
        {
            get => this.parent;
            internal set => this.parent = value;
        }

        public override bool Updateable
        {
            get => this.updateable;
            set => this.updateable = value;
        }

        public override bool Updated
        {
            get => this.updated;
            protected internal set => this.updated = value;
        }
    }
}