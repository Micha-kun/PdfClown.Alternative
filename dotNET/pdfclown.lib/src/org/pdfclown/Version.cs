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


namespace org.pdfclown
{
    using System;
    using System.Collections.Generic;

    using System.Text.RegularExpressions;
    using org.pdfclown.objects;
    using org.pdfclown.util.metadata;

    /**
      <summary>Generic PDF version number [PDF:1.6:H.1].</summary>
      <seealso cref="VersionEnum"/>
    */
    public sealed class Version : IVersion
    {
        private static readonly Regex versionPattern = new Regex("^(\\d+)\\.(\\d+)$");
        private static readonly IDictionary<string, Version> versions = new Dictionary<string, Version>();

        private readonly int major;
        private readonly int minor;

        private Version(int major, int minor)
        {
            this.major = major;
            this.minor = minor;
        }

        public int CompareTo(IVersion value) { return VersionUtils.CompareTo(this, value); }

        public static Version Get(
            PdfName version
)
        { return Get(version.RawValue); }

        public static Version Get(string version)
        {
            if (!versions.ContainsKey(version))
            {
                var versionMatch = versionPattern.Match(version);
                if (!versionMatch.Success)
                {
                    throw new Exception($"Invalid PDF version format: '{versionPattern}' pattern expected.");
                }

                var versionObject = new Version(
                    int.Parse(versionMatch.Groups[1].Value),
                    int.Parse(versionMatch.Groups[2].Value));
                versions[version] = versionObject;
            }
            return versions[version];
        }

        public override string ToString() { return VersionUtils.ToString(this); }

        public int Major => this.major;

        public int Minor => this.minor;

        public IList<int> Numbers => new List<int> { this.major, this.minor };
    }
}
