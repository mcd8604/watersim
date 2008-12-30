using System;
using System.Runtime.InteropServices;

/*
 * References: 
 *  http://msdn.microsoft.com/en-us/library/ms706554(VS.85).aspx
 *  Vfw32.lib (vfw.h) - http://doc.ddart.net/msdn/header/include/vfw.h.html
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
    public class FccType
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
    }

    public struct Rect
    { 
	    public Int32 x; 
	    public Int32 y; 
	    public Int32 width; 
	    public Int32 height; 
    } 	

    public struct AviStreamInfo
    {
        public Int32 fccType;
        public Int32 fccHandler;
        public Int32 dwFlags;
        public Int32 dwCaps;
        public Int32 wPriority; 
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

    //public struct AVICOMPRESSOPTIONS
    //{
    //    public Int32 fccType;		              /* stream type, for consistency */
    //    public Int32 fccHandler;                 /* compressor */
    //    public Int32 dwKeyFrameEvery;            /* keyframe rate */
    //    public Int32 dwQuality;                  /* compress quality 0-10,000 */
    //    public Int32 dwBytesPerSecond;           /* bytes per second */
    //    public Int32 dwFlags;                    /* flags... see below */
    //    public IntPtr lpFormat;                  /* save format */
    //    public Int32 cbFormat;
    //    public IntPtr lpParms;                   /* compressor options */
    //    public Int32 cbParms;
    //    public Int32 dwInterleaveEvery;          /* for non-video streams only */
    //}

    ///// <summary>
    ///// Defines for the dwFlags field of the AVICOMPRESSOPTIONS struct
    ///// Each of these flags determines if the appropriate field in the structure
    ///// (dwInterleaveEvery, dwBytesPerSecond, and dwKeyFrameEvery) is payed
    ///// attention to.  See the autodoc in avisave.c for details.
    ///// </summary>
    //public static class DWFLAGS {
    //    public static short AVICOMPRESSF_INTERLEAVE	= 0x00000001;   // interleave
    //    public static short AVICOMPRESSF_DATARATE = 0x00000002;   // use a data rate
    //    public static short AVICOMPRESSF_KEYFRAMES = 0x00000004;   // use keyframes
    //    public static short AVICOMPRESSF_VALID = 0x00000008;    // has valid data?
    //}

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
          ref int plSampWritten,
          ref int plBytesWritten
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
    }
}
