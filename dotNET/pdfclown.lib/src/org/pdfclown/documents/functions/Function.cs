/*
  Copyright 2010-2012 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.documents.functions
{
    using System;
    using System.Collections.Generic;

    using org.pdfclown.objects;
    using org.pdfclown.util.math;

    /**
      <summary>Function [PDF:1.6:3.9].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public abstract class Function
      : PdfObjectWrapper<PdfDataObject>
    {

        private const int FunctionType0 = 0;
        private const int FunctionType2 = 2;
        private const int FunctionType3 = 3;
        private const int FunctionType4 = 4;

        protected Function(
          PdfDirectObject baseObject
          ) : base(baseObject)
        { }

        protected Function(
Document context,
PdfDataObject baseDataObject
) : base(context, baseDataObject)
        { }
        /**
  <summary>Default intervals callback.</summary>
*/
        protected delegate IList<Interval<T>> DefaultIntervalsCallback<T>(
          IList<Interval<T>> intervals
          ) where T : IComparable<T>;

        /**
  <summary>Gets a function's dictionary.</summary>
  <param name="functionDataObject">Function data object.</param>
*/
        private static PdfDictionary GetDictionary(
          PdfDataObject functionDataObject
          )
        {
            if (functionDataObject is PdfDictionary)
            {
                return (PdfDictionary)functionDataObject;
            }
            else // MUST be PdfStream.
            {
                return ((PdfStream)functionDataObject).Header;
            }
        }

        /**
          <summary>Gets the intervals corresponding to the specified key.</summary>
        */
        protected IList<Interval<T>> GetIntervals<T>(
          PdfName key,
          DefaultIntervalsCallback<T> defaultIntervalsCallback
          ) where T : IComparable<T>
        {
            IList<Interval<T>> intervals;
            var intervalsObject = (PdfArray)this.Dictionary[key];
            if (intervalsObject == null)
            {
                intervals = (defaultIntervalsCallback == null)
                  ? null
                  : defaultIntervalsCallback(new List<Interval<T>>());
            }
            else
            {
                intervals = new List<Interval<T>>();
                for (
                  int index = 0,
                    length = intervalsObject.Count;
                  index < length;
                  index += 2
                  )
                {
                    intervals.Add(
                      new Interval<T>(
                        (T)((IPdfNumber)intervalsObject[index]).Value,
                        (T)((IPdfNumber)intervalsObject[index + 1]).Value
                        )
                      );
                }
            }
            return intervals;
        }

        /**
  <summary>Gets this function's dictionary.</summary>
*/
        protected PdfDictionary Dictionary => GetDictionary(this.BaseDataObject);

        /**
<summary>Gets the result of the calculation applied by this function
to the specified input values.</summary>
<param name="inputs">Input values.</param>
*/
        public abstract double[] Calculate(
          double[] inputs
          );

        /**
          <summary>Gets the result of the calculation applied by this function
          to the specified input values.</summary>
          <param name="inputs">Input values.</param>
         */
        public IList<PdfDirectObject> Calculate(
          IList<PdfDirectObject> inputs
          )
        {
            IList<PdfDirectObject> outputs = new List<PdfDirectObject>();
            var inputValues = new double[inputs.Count];
            for (
              int index = 0,
                length = inputValues.Length;
              index < length;
              index++
              )
            { inputValues[index] = ((IPdfNumber)inputs[index]).RawValue; }
            var outputValues = this.Calculate(inputValues);
            for (
              int index = 0,
                length = outputValues.Length;
              index < length;
              index++
              )
            { outputs.Add(PdfReal.Get(outputValues[index])); }
            return outputs;
        }

        /**
<summary>Wraps a function base object into a function object.</summary>
<param name="baseObject">Function base object.</param>
<returns>Function object associated to the base object.</returns>
*/
        public static Function Wrap(
          PdfDirectObject baseObject
          )
        {
            if (baseObject == null)
            {
                return null;
            }

            var dataObject = baseObject.Resolve();
            var dictionary = GetDictionary(dataObject);
            var functionType = ((PdfInteger)dictionary[PdfName.FunctionType]).RawValue;
            switch (functionType)
            {
                case FunctionType0:
                    return new Type0Function(baseObject);
                case FunctionType2:
                    return new Type2Function(baseObject);
                case FunctionType3:
                    return new Type3Function(baseObject);
                case FunctionType4:
                    return new Type4Function(baseObject);
                default:
                    throw new NotSupportedException($"Function type {functionType} unknown.");
            }
        }

        /**
          <summary>Gets the (inclusive) domains of the input values.</summary>
          <remarks>Input values outside the declared domains are clipped to the nearest boundary value.</remarks>
        */
        public IList<Interval<double>> Domains => this.GetIntervals<double>(PdfName.Domain, null);

        /**
          <summary>Gets the number of input values (parameters) of this function.</summary>
        */
        public int InputCount => ((PdfArray)this.Dictionary[PdfName.Domain]).Count / 2;

        /**
          <summary>Gets the number of output values (results) of this function.</summary>
        */
        public int OutputCount
        {
            get
            {
                var rangesObject = (PdfArray)this.Dictionary[PdfName.Range];
                return (rangesObject == null) ? 1 : (rangesObject.Count / 2);
            }
        }

        /**
          <summary>Gets the (inclusive) ranges of the output values.</summary>
          <remarks>Output values outside the declared ranges are clipped to the nearest boundary value;
          if this entry is absent, no clipping is done.</remarks>
          <returns><code>null</code> in case of unbounded ranges.</returns>
        */
        public IList<Interval<double>> Ranges => this.GetIntervals<double>(PdfName.Range, null);
    }
}