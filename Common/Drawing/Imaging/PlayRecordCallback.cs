﻿using System;

namespace Common.Drawing.Imaging
{
    /// <summary>This delegate is not used. For an example of enumerating the records of a metafile, see <see cref="M:Common.Drawing.Graphics.EnumerateMetafile(Common.Drawing.Imaging.Metafile,Common.Drawing.Point,Common.Drawing.Graphics.EnumerateMetafileProc)" />.</summary>
    /// <param name="recordType">Not used. </param>
    /// <param name="flags">Not used. </param>
    /// <param name="dataSize">Not used. </param>
    /// <param name="recordData">Not used. </param>
    public delegate void PlayRecordCallback(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr recordData);
}
