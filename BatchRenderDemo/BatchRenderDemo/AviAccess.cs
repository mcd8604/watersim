using System;
using System.Runtime.InteropServices;

/*
 * References: 
 *  http://msdn.microsoft.com/en-us/library/ms706554(VS.85).aspx
 *  Vfw32.lib (Vfw.h), WinBase.h
 *  Microsoft Corporation
 * 
 * Author: Mike DeMauro
 */
namespace BatchRenderDemo
{
    /// <summary>
    /// Four-character code indicating the stream type. 
    /// The following constants have been defined for the data commonly found in AVI streams:
    /// </summary>
    public sealed class AviUtil
    {
        public static readonly int StreamType_Video = MakeFourCC('v', 'i', 'd', 's');
        public static readonly int StreamType_Audio = MakeFourCC('a', 'u', 'd', 's');
        public static readonly int StreamType_Midi = MakeFourCC('m', 'i', 'd', 's');
        public static readonly int StreamType_Text = MakeFourCC('t', 'x', 't', 's');

        /// <summary>
        /// The MakeFourCC macro converts four characters into a four-character code.
        /// </summary>
        /// <param name="ch0">First character of the four-character code.</param>
        /// <param name="ch1">Second character of the four-character code.</param>
        /// <param name="ch2">Third character of the four-character code.</param>
        /// <param name="ch3">Fourth character of the four-character code.</param>
        /// <returns>Returns the four-character code created from the given characters.</returns>
        /// <remarks>
        /// This macro does not check whether the four-character code it returns is valid.
        /// </remarks>
        public static Int32 MakeFourCC(char ch0, char ch1, char ch2, char ch3)
        {
            return ((Int32)(byte)(ch0) | ((byte)(ch1) << 8) | ((byte)(ch2) << 16) | ((byte)(ch3) << 24));
        }

        /// <summary>
        /// File Modes
        /// </summary>
        public const int OF_READ = 0x00000000;
        public const int OF_WRITE = 0x00000001;
        public const int OF_READWRITE = 0x00000002;
        public const int OF_SHARE_COMPAT = 0x00000000;
        public const int OF_SHARE_EXCLUSIVE = 0x00000010;
        public const int OF_SHARE_DENY_WRITE = 0x00000020;
        public const int OF_SHARE_DENY_READ = 0x00000030;
        public const int OF_SHARE_DENY_NONE = 0x00000040;
        public const int OF_PARSE = 0x00000100;
        public const int OF_DELETE = 0x00000200;
        public const int OF_VERIFY = 0x00000400;
        public const int OF_CANCEL = 0x00000800;
        public const int OF_CREATE = 0x00001000;
        public const int OF_PROMPT = 0x00002000;
        public const int OF_EXIST = 0x00004000;
        public const int OF_REOPEN = 0x00008000;

        /* Flags for AVI file index */
        public const int AVIIF_LIST = 0x00000001;
        public const int AVIIF_TWOCC = 0x00000002;
        public const int AVIIF_KEYFRAME = 0x00000010;
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/ms532290.aspx
    /// </summary>
    public struct BitMapInfoHeader 
    {
        public Int32 biSize; 
        public Int32 biWidth; 
        public Int32 biHeight; 
        public Int16 biPlanes; 
        public Int16 biBitCount; 
        public Int32 biCompression; 
        public Int32 biSizeImage; 
        public Int32 biXPelsPerMeter; 
        public Int32 biYPelsPerMeter; 
        public Int32 biClrUsed; 
        public Int32 biClrImportant;
    }

    public struct Rect
    {
        public Int32 left;
        public Int32 top;
        public Int32 right;
        public Int32 bottom;
    }

    public struct AviStreamInfo
    {
        public Int32 fccType;
        public Int32 fccHandler;
        public Int32 dwFlags;
        public Int32 dwCaps;
        public Int16 wPriority; 
        public Int16 wLanguage; 
        public Int32 dwScale; 
        public Int32 dwRate; 
        public Int32 dwStart; 
        public Int32 dwLength; 
        public Int32 dwInitialFrames; 
        public Int32 dwSuggestedBufferSize; 
        public Int32 dwQuality; 
        public Int32 dwSampleSize; 
        public Rect  rcFrame; 
        public Int32 dwEditCount; 
        public Int32 dwFormatChangeCount;
        public Int32[] szName; // length 64
    }

    public struct AviCompressOptions
    {
        public Int32 fccType;		              /* stream type, for consistency */
        public Int32 fccHandler;                 /* compressor */
        public Int32 dwKeyFrameEvery;            /* keyframe rate */
        public Int32 dwQuality;                  /* compress quality 0-10,000 */
        public Int32 dwBytesPerSecond;           /* bytes per second */
        public Int32 dwFlags;                    /* flags... see below */
        public IntPtr lpFormat;                  /* save format */
        public Int32 cbFormat;
        public IntPtr lpParms;                   /* compressor options */
        public Int32 cbParms;
        public Int32 dwInterleaveEvery;          /* for non-video streams only */
    }

    /// <summary>
    /// Defines for the dwFlags field of the AVICOMPRESSOPTIONS struct
    /// Each of these flags determines if the appropriate field in the structure
    /// (dwInterleaveEvery, dwBytesPerSecond, and dwKeyFrameEvery) is payed
    /// attention to.  See the autodoc in avisave.c for details.
    /// </summary>
    public static class DWFLAGS
    {
        public static short AVICOMPRESSF_INTERLEAVE = 0x00000001;   // interleave
        public static short AVICOMPRESSF_DATARATE = 0x00000002;   // use a data rate
        public static short AVICOMPRESSF_KEYFRAMES = 0x00000004;   // use keyframes
        public static short AVICOMPRESSF_VALID = 0x00000008;    // has valid data?
    }

    class AviAccess
    {
        #region AVIFile Library Operations

        /// <summary>
        /// Initializes the AVIFile library.
        /// </summary>
        [DllImport("avifil32.dll")]
        public static extern void AVIFileInit();

        /// <summary>
        /// Exits the AVIFile library and decrements the reference count for the library.
        /// </summary>
        [DllImport("avifil32.dll")]
        public static extern void AVIFileExit();

        #endregion

        #region Opening and Closing AVI Files

        /// <summary>
        /// Opens an AVI file and returns the address of a file interface used to access it.
        /// </summary>
        /// <param name="ppfile">Pointer to a buffer that receives the new IAVIFile interface pointer.</param>
        /// <param name="szFile">Null-terminated string containing the name of the file to open.</param>
        /// <param name="uMode">Access mode to use when opening the file. The default access mode is OF_READ.</param>
        /// <param name="pclsidHandler">Pointer to a class identifier of the standard or custom handler you want to use. If the value is NULL,
        /// the system chooses a handler from the registry based on the file extension or the RIFF type specified in the file.</param>
        /// <returns>Returns zero if successful or an error otherwise.</returns>
        [DllImport("avifil32.dll")]
        public static extern int AVIFileOpen(
            out IntPtr ppfile,
            string szFile,
            int uMode,
            int pclsidHandler);

        /// <summary>
        /// The AVIFileAddRef function increments the reference count of an AVI file.
        /// </summary>
        /// <param name="pfile">Handle to an open AVI file.</param>
        /// <returns>Returns the updated reference count for the file interface.</returns>
        /// <remarks>The argument pfile is a pointer to an IAVIFile interface.</remarks>
        [DllImport("avifil32.dll")]
        public static extern int AVIFileAddRef(
            IntPtr pfile
        );

        /// <summary>
        /// The AVIFileRelease function decrements the reference count of an AVI file interface handle and closes the file if the count reaches zero.
        /// </summary>
        /// <param name="pfile">Handle to an open AVI file.</param>
        /// <returns>Returns the reference count of the file. This return value should be used only for debugging purposes.</returns>
        /// <remarks>The argument pfile is a pointer to an IAVIFile interface.</remarks>
        [DllImport("avifil32.dll")]
        public static extern int AVIFileRelease(
            IntPtr pfile
        );

        #endregion

        #region Opening and Closing Streams

        /// <summary>
        /// Increments the reference count of an AVI stream.
        /// </summary>
        /// <param name="pavi">Handle to an open stream.</param>
        /// <returns>Returns the current reference count of the stream. This value should be used only for debugging purposes.</returns>
        /// <remarks>The argument pavi is a pointer to an IAVIStream interface.</remarks>
        [DllImport("avifil32.dll")]
        public static extern int AVIStreamAddRef(
            IntPtr pavi
        );

        /// <summary>
        /// Decrements the reference count of an AVI stream interface handle, and closes the stream if the count reaches zero.
        /// </summary>
        /// <param name="pavi">Handle to an open stream.</param>
        /// <returns>Returns the current reference count of the stream. This value should be used only for debugging purposes.</returns>
        /// <remarks>The argument pavi is a pointer to an IAVIStream interface.</remarks>
        [DllImport("avifil32.dll")]
        public static extern int AVIStreamRelease(
            IntPtr pavi
        );

        #endregion

        #region Writing Individual Streams

        /// <summary>
        /// Creates a new stream in an existing file and creates an interface to the new stream.
        /// </summary>
        /// <param name="pfile">Handle to an open AVI file</param>
        /// <param name="ppavi">Pointer to the new stream interface.</param>
        /// <param name="psi">Pointer to a structure containing information about the new stream, including the stream type and its sample rate.</param>
        /// <returns>Returns zero if successful or an error otherwise. Unless the file has been opened with write permission, this function returns AVIERR_READONLY.</returns>
        /// <remarks>
        /// This function starts a reference count for the new stream.
        /// The argument pfile is a pointer to an IAVIFile interface. 
        /// The argument ppavi is a pointer to an IAVIStream interface.
        /// </remarks>
		[DllImport("avifil32.dll")]
		public static extern int AVIFileCreateStream(
			IntPtr pfile,
			out IntPtr ppavi, 
			ref AviStreamInfo psi);
        
        /// <summary>
        /// Sets the format of a stream at the specified position.
        /// </summary>
        /// <param name="pavi">Handle to an open stream.</param>
        /// <param name="lPos">Position in the stream to receive the format.</param>
        /// <param name="lpFormat">Pointer to a structure containing the new format.</param>
        /// <param name="cbFormat">Size, in bytes, of the block of memory referenced by lpFormat.</param>
        /// <returns>Returns zero if successful or an error otherwise.</returns>
        /// <remarks>
        /// The handler for writing AVI files does not accept format changes. 
        /// Besides setting the initial format for a stream, 
        /// only changes in the palette of a video stream are allowed in an AVI file. 
        /// The palette change must occur after any frames already written to the AVI file. 
        /// Other handlers might impose different restrictions.
        /// 
        /// The argument pavi is a pointer to an IAVIStream interface.
        /// </remarks>
		[DllImport("avifil32.dll")]
		public static extern int AVIStreamSetFormat(
            IntPtr pavi,  
            int lPos,        
            ref BitMapInfoHeader lpFormat,  
            int cbFormat );

        /// <summary>
        /// Writes data to a stream.
        /// </summary>
        /// <param name="pavi">Handle to an open stream.</param>
        /// <param name="lStart">First sample to write.</param>
        /// <param name="lSamples">Number of samples to write.</param>
        /// <param name="lpBuffer">Pointer to a buffer containing the data to write.</param>
        /// <param name="cbBuffer">Size of the buffer referenced by lpBuffer.</param>
        /// <param name="dwFlags">Flag associated with this data.</param>
        /// <param name="plSampWritten">Pointer to a buffer that receives the number of samples written. This can be set to NULL.</param>
        /// <param name="plBytesWritten">Pointer to a buffer that receives the number of bytes written. This can be set to NULL.</param>
        /// <returns>Returns zero if successful or an error otherwise.</returns>
        /// <remarks>
        /// The default AVI file handler supports writing only at the end of a stream. The "WAVE" file handler supports writing anywhere.
        /// This function overwrites existing data, rather than inserting new data.
        /// The argument pavi is a pointer to an IAVIStream interface.
        /// </remarks>
        [DllImport("avifil32.dll")]
        public static extern int AVIStreamWrite(
            IntPtr pavi,
            int lStart,
            int lSamples,
            IntPtr lpBuffer,
            int cbBuffer,
            int dwFlags,
            IntPtr plSampWritten,
            IntPtr plBytesWritten
        );

        #endregion

        
        /// <summary>
        /// Creates a compressed stream from an uncompressed stream and a compression filter, 
        /// and returns the address of a pointer to the compressed stream. 
        /// This function supports audio and video compression.
        /// </summary>
        /// <param name="ppsCompressed">Pointer to a buffer that receives the compressed stream pointer.</param>
        /// <param name="psSource">Pointer to the stream to be compressed.</param>
        /// <param name="lpOptions">Pointer to a structure that identifies the type of compression to use and the options to apply. 
        /// You can specify video compression by identifying an appropriate handler in the AVICOMPRESSOPTIONS structure. 
        /// For audio compression, specify the compressed data format.</param>
        /// <param name="pclsidHandler">Pointer to a class identifier used to create the stream.</param>
        /// <returns>Returns AVIERR_OK if successful or an error otherwise.</returns>
        /// <remarks>
        /// Applications can read from or write to the compressed stream.
        /// A PAVISTREAM is a pointer to an IAVIStream interface.
        /// </remarks>
        //[DllImport("avifil32.dll")]
        //public static extern int AVIMakeCompressedStream(
        //    IntPtr ppsCompressed,
        //    IntPtr psSource,
        //    ref AviCompressOptions lpOptions,
        //    int pclsidHandler // just send 0 since unneeded
        //);
    }
}
