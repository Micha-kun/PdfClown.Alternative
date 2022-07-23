/*
  Copyright 2006-2013 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it):
      - porting and adaptation (extension to any bit depth other than 8) of [JT]
        predictor-decoding implementation.
    * Joshua Tauberer (code contributor, http://razor.occams.info):
      - predictor-decoding contributor on .NET implementation.
    * Jean-Claude Truy (bugfix contributor): [FIX:0.0.8:JCT].

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

namespace org.pdfclown.bytes.filters
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using org.pdfclown.objects;

    /**
      <summary>zlib/deflate [RFC:1950,1951] filter [PDF:1.6:3.3.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public sealed class FlateFilter
    : Filter
    {
        internal FlateFilter(
)
        { }

        private byte[] DecodePredictor(
  byte[] data,
  PdfDictionary parameters
  )
        {
            if (parameters == null)
            {
                return data;
            }

            var predictor = parameters.ContainsKey(PdfName.Predictor) ? ((PdfInteger)parameters[PdfName.Predictor]).RawValue : 1;
            if (predictor == 1) // No predictor was applied during data encoding.
            {
                return data;
            }

            var sampleComponentBitsCount = parameters.ContainsKey(PdfName.BitsPerComponent) ? ((PdfInteger)parameters[PdfName.BitsPerComponent]).RawValue : 8;
            var sampleComponentsCount = parameters.ContainsKey(PdfName.Colors) ? ((PdfInteger)parameters[PdfName.Colors]).RawValue : 1;
            var rowSamplesCount = parameters.ContainsKey(PdfName.Columns) ? ((PdfInteger)parameters[PdfName.Columns]).RawValue : 1;

            var input = new MemoryStream(data);
            var output = new MemoryStream();
            switch (predictor)
            {
                case 2: // TIFF Predictor 2 (component-based).
                    var sampleComponentPredictions = new int[sampleComponentsCount];
                    var sampleComponentIndex = 0;
                    int sampleComponentDelta;
                    while ((sampleComponentDelta = input.ReadByte()) != -1)
                    {
                        var sampleComponent = sampleComponentDelta + sampleComponentPredictions[sampleComponentIndex];
                        output.WriteByte((byte)sampleComponent);

                        sampleComponentPredictions[sampleComponentIndex] = sampleComponent;

                        sampleComponentIndex = (++sampleComponentIndex) % sampleComponentsCount;
                    }
                    break;
                default: // PNG Predictors [RFC 2083] (byte-based).
                    var sampleBytesCount = (int)Math.Ceiling((sampleComponentBitsCount * sampleComponentsCount) / 8d); // Number of bytes per pixel (bpp).
                    var rowSampleBytesCount = ((int)Math.Ceiling((sampleComponentBitsCount * sampleComponentsCount * rowSamplesCount) / 8d)) + sampleBytesCount; // Number of bytes per row (comprising a leading upper-left sample (see Paeth method)).
                    var previousRowBytePredictions = new int[rowSampleBytesCount];
                    var currentRowBytePredictions = new int[rowSampleBytesCount];
                    var leftBytePredictions = new int[sampleBytesCount];
                    int predictionMethod;
                    while ((predictionMethod = input.ReadByte()) != -1)
                    {
                        Array.Copy(currentRowBytePredictions, 0, previousRowBytePredictions, 0, currentRowBytePredictions.Length);
                        Array.Clear(leftBytePredictions, 0, leftBytePredictions.Length);
                        for (
                          var rowSampleByteIndex = sampleBytesCount; // Starts after the leading upper-left sample (see Paeth method).
                          rowSampleByteIndex < rowSampleBytesCount;
                          rowSampleByteIndex++
                          )
                        {
                            var byteDelta = input.ReadByte();

                            var sampleByteIndex = rowSampleByteIndex % sampleBytesCount;

                            int sampleByte;
                            switch (predictionMethod)
                            {
                                case 0: // None (no prediction).
                                    sampleByte = byteDelta;
                                    break;
                                case 1: // Sub (predicts the same as the sample to the left).
                                    sampleByte = byteDelta + leftBytePredictions[sampleByteIndex];
                                    break;
                                case 2: // Up (predicts the same as the sample above).
                                    sampleByte = byteDelta + previousRowBytePredictions[rowSampleByteIndex];
                                    break;
                                case 3: // Average (predicts the average of the sample to the left and the sample above).
                                    sampleByte = byteDelta + ((int)Math.Floor((leftBytePredictions[sampleByteIndex] + previousRowBytePredictions[rowSampleByteIndex]) / 2d));
                                    break;
                                case 4: // Paeth (a nonlinear function of the sample above, the sample to the left, and the sample to the upper left).
                                    int paethPrediction;
                                    var leftBytePrediction = leftBytePredictions[sampleByteIndex];
                                    var topBytePrediction = previousRowBytePredictions[rowSampleByteIndex];
                                    var topLeftBytePrediction = previousRowBytePredictions[rowSampleByteIndex - sampleBytesCount];
                                    var initialPrediction = (leftBytePrediction + topBytePrediction) - topLeftBytePrediction;
                                    var leftPrediction = Math.Abs(initialPrediction - leftBytePrediction);
                                    var topPrediction = Math.Abs(initialPrediction - topBytePrediction);
                                    var topLeftPrediction = Math.Abs(initialPrediction - topLeftBytePrediction);
                                    if ((leftPrediction <= topPrediction)
                                      && (leftPrediction <= topLeftPrediction))
                                    { paethPrediction = leftBytePrediction; }
                                    else if (topPrediction <= topLeftPrediction)
                                    { paethPrediction = topBytePrediction; }
                                    else
                                    { paethPrediction = topLeftBytePrediction; }
                                    sampleByte = byteDelta + paethPrediction;
                                    break;
                                default:
                                    throw new NotSupportedException($"Prediction method {predictionMethod} unknown.");
                            }
                            output.WriteByte((byte)sampleByte);

                            leftBytePredictions[sampleByteIndex] = currentRowBytePredictions[rowSampleByteIndex] = (byte)sampleByte;
                        }
                    }
                    break;
            }
            return output.ToArray();
        }

        private void Transform(
          Stream input,
          Stream output
          )
        {
            var buffer = new byte[8192];
            int bufferLength;
            while ((bufferLength = input.Read(buffer, 0, buffer.Length)) != 0)
            { output.Write(buffer, 0, bufferLength); }

            input.Close();
            output.Close();
        }

        public override byte[] Decode(
byte[] data,
int offset,
int length,
PdfDictionary parameters
)
        {
            var outputStream = new MemoryStream();
            var inputStream = new MemoryStream(data, offset, length);
            var inputFilter = new DeflateStream(inputStream, CompressionMode.Decompress);
            inputStream.Position = 2; // Skips zlib's 2-byte header [RFC 1950] [FIX:0.0.8:JCT].
            this.Transform(inputFilter, outputStream);
            return this.DecodePredictor(outputStream.ToArray(), parameters);
        }

        public override byte[] Encode(
          byte[] data,
          int offset,
          int length,
          PdfDictionary parameters
          )
        {
            var inputStream = new MemoryStream(data, offset, length);
            var outputStream = new MemoryStream();
            var outputFilter = new DeflateStream(outputStream, CompressionMode.Compress, true);
            // Add zlib's 2-byte header [RFC 1950] [FIX:0.0.8:JCT]!
            outputStream.WriteByte(0x78); // CMF = {CINFO (bits 7-4) = 7; CM (bits 3-0) = 8} = 0x78.
            outputStream.WriteByte(0xDA); // FLG = {FLEVEL (bits 7-6) = 3; FDICT (bit 5) = 0; FCHECK (bits 4-0) = {31 - ((CMF * 256 + FLG - FCHECK) Mod 31)} = 26} = 0xDA.
            this.Transform(inputStream, outputFilter);
            return outputStream.ToArray();
        }
    }
}
