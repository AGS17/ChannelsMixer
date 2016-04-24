using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace TargaImage
{
    /// <summary>
    /// Reads and loads a Truevision TGA Format image file.
    /// </summary>
    public class TgaImage : IDisposable

    {
        private TargaHeader objTargaHeader = null;
        private TargaExtensionArea objTargaExtensionArea = null;
        private TargaFooter objTargaFooter = null;
        private Bitmap bitmapTargaImage = null;
        private BitmapSource bitmapSourceTargaImage = null;
        private TGAFormat eTgaFormat = TGAFormat.UNKNOWN;
        private FileInfo fileInfo = null;
        private int stride = 0;
        private int padding = 0;
        private GCHandle imageByteHandle;


        // Track whether Dispose has been called.
        private bool disposed = false;


        /// <summary>
        /// Creates a new instance of the TargaImage object.
        /// </summary>
        public TgaImage()
        {
            this.objTargaFooter = new TargaFooter();
            this.objTargaHeader = new TargaHeader();
            this.objTargaExtensionArea = new TargaExtensionArea();
            this.bitmapTargaImage = null;
            this.bitmapSourceTargaImage = null;
        }


        /// <summary>
        /// Creates a new instance of the TargaImage object with strFileName as the image loaded.
        /// </summary>
        public TgaImage(string fileName) : this()
        {
            var fileInfo = new FileInfo(fileName);

            // make sure we have a .tga file
            if (fileInfo.Extension == ".tga")
            {
                // make sure the file exists
                if (fileInfo.Exists)
                {
                    this.fileInfo = fileInfo;
                    byte[] filebytes = null;

                    // load the file as an array of bytes
                    filebytes = File.ReadAllBytes(fileInfo.FullName);
                    if (filebytes.Length > 0)
                    {
                        // create a seekable memory stream of the file bytes
                        using (var ms = new MemoryStream(filebytes))
                        {
                            if (ms.Length > 0 && ms.CanSeek)
                            {
                                // create a BinaryReader used to read the Targa file
                                using (var br = new BinaryReader(ms))
                                {
                                    this.LoadTgaFooterInfo(br);
                                    this.LoadTgaHeaderInfo(br);
                                    this.LoadTgaExtensionArea(br);
                                    this.LoadTgaImage(br);
                                }
                            }
                            else
                                throw new Exception(@"Error loading file, could not read file from disk.");
                        }
                    }
                    else
                        throw new Exception(@"Error loading file, could not read file from disk.");
                }
                else
                    throw new Exception($@"Error loading file, could not find file '{this.fileInfo.FullName}' on disk.");

            }
            else
                throw new Exception($@"Error loading file, file '{this.fileInfo.FullName}' must have an extension of '.tga'.");


        }


        /// <summary>
        /// Creates a new instance of the TargaImage object loading the image data from the provided stream.
        /// </summary>
        public TgaImage(Stream imageStream) : this()
        {
            if (imageStream != null && imageStream.Length > 0 && imageStream.CanSeek == true)
            {
                // create a BinaryReader used to read the Targa file
                using (BinaryReader binReader = new BinaryReader(imageStream))
                {
                    this.LoadTgaFooterInfo(binReader);
                    this.LoadTgaHeaderInfo(binReader);
                    this.LoadTgaExtensionArea(binReader);
                    this.LoadTgaImage(binReader);
                }
            }
            else
                throw new ArgumentException(@"Error loading image, Null, zero length or non-seekable stream provided.", nameof(imageStream));


        }


        /// <summary>
        /// Creates a new instance of the TargaImage object loading the image data from the BitmapSource.
        /// </summary>
        //public TgaImage(BitmapSource bitmapSource) : this()
        //{
        //    this.objTargaHeader = new TargaHeader
        //    {
        //        ColorMap = { Color.AliceBlue, Color.Black },
        //        ImageDescriptor =
        //    };

        //}


        /// <summary>
        /// Gets a TargaHeader object that holds the Targa Header information of the loaded file.
        /// </summary>
        public TargaHeader Header => this.objTargaHeader;


        /// <summary>
        /// Gets a TargaExtensionArea object that holds the Targa Extension Area information of the loaded file.
        /// </summary>
        public TargaExtensionArea ExtensionArea => this.objTargaExtensionArea;


        /// <summary>
        /// Gets a TargaExtensionArea object that holds the Targa Footer information of the loaded file.
        /// </summary>
        public TargaFooter Footer => this.objTargaFooter;


        /// <summary>
        /// Gets the Targa format of the loaded file.
        /// </summary>
        public TGAFormat Format => this.eTgaFormat;


        /// <summary>
        /// Gets a Bitmap representation of the loaded file.
        /// </summary>
        public Bitmap BitmapImage => this.bitmapTargaImage;

        /// <summary>
        /// Gets the thumbnail of the loaded file if there is one in the file.
        /// </summary>
        public BitmapSource BitmapSourceImage => this.bitmapSourceTargaImage;

        /// <summary>
        /// Gets the fileinfo of the loaded file.
        /// </summary>
        public FileInfo FileInfo => this.fileInfo;


        /// <summary>
        /// Gets the byte offset between the beginning of one scan line and the next. Used when loading the image into the Image Bitmap.
        /// </summary>
        /// <remarks>
        /// The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
        /// The stride refers to the number of bytes allocated for one scanline of the bitmap.
        /// </remarks>
        public int Stride => this.stride;


        /// <summary>
        /// Gets the number of bytes used to pad each scan line to meet the Stride value. Used when loading the image into the Image Bitmap.
        /// </summary>
        /// <remarks>
        /// The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
        /// The stride refers to the number of bytes allocated for one scanline of the bitmap.
        /// In your loop, you copy the pixels one scanline at a time and take into 
        /// consideration the amount of padding that occurs due to memory alignment.
        /// </remarks>
        public int Padding => this.padding;


        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        /// <summary>
        /// TargaImage deconstructor.
        /// </summary>
        ~TgaImage()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        
        /// <summary>
        /// Loads the Targa Footer information from the file.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTgaFooterInfo(BinaryReader binReader)
        {

            if (binReader?.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek)
            {
                try
                {
                    // set the cursor at the beginning of the signature string.
                    binReader.BaseStream.Seek((TargaConstants.FooterSignatureOffsetFromEnd * -1), SeekOrigin.End);

                    // read the signature bytes and convert to ascii string
                    string signature = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.FooterSignatureByteLength)).TrimEnd('\0');

                    // do we have a proper signature
                    if (string.CompareOrdinal(signature, TargaConstants.TargaFooterASCIISignature) == 0)
                    {
                        // this is a NEW targa file.
                        // create the footer
                        this.eTgaFormat = TGAFormat.NEW_TGA;

                        // set cursor to beginning of footer info
                        binReader.BaseStream.Seek((TargaConstants.FooterByteLength * -1), SeekOrigin.End);

                        // read the Extension Area Offset value
                        int extOffset = binReader.ReadInt32();

                        // read the Developer Directory Offset value
                        int devDirOff = binReader.ReadInt32();

                        // skip the signature we have already read it.
                        binReader.ReadBytes(TargaConstants.FooterSignatureByteLength);

                        // read the reserved character
                        string resChar = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.FooterReservedCharByteLength)).TrimEnd('\0');

                        // set all values to our TargaFooter class
                        this.objTargaFooter.SetExtensionAreaOffset(extOffset);
                        this.objTargaFooter.SetDeveloperDirectoryOffset(devDirOff);
                        this.objTargaFooter.SetSignature(signature);
                        this.objTargaFooter.SetReservedCharacter(resChar);
                    }
                    else
                    {
                        // this is not an ORIGINAL targa file.
                        this.eTgaFormat = TGAFormat.ORIGINAL_TGA;
                    }
                }
                catch (Exception)
                {
                    // clear all 
                    this.ClearAll();
                    throw;
                }
            }
            else
            {
                this.ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }
        }


        /// <summary>
        /// Loads the Targa Header information from the file.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTgaHeaderInfo(BinaryReader binReader)
        {

            if (binReader?.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek == true)
            {
                try
                {
                    // set the cursor at the beginning of the file.
                    binReader.BaseStream.Seek(0, SeekOrigin.Begin);

                    // read the header properties from the file
                    this.objTargaHeader.SetImageIDLength(binReader.ReadByte());
                    this.objTargaHeader.SetColorMapType((ColorMapType)binReader.ReadByte());
                    this.objTargaHeader.SetImageType((ImageType)binReader.ReadByte());

                    this.objTargaHeader.SetColorMapFirstEntryIndex(binReader.ReadInt16());
                    this.objTargaHeader.SetColorMapLength(binReader.ReadInt16());
                    this.objTargaHeader.SetColorMapEntrySize(binReader.ReadByte());

                    this.objTargaHeader.SetXOrigin(binReader.ReadInt16());
                    this.objTargaHeader.SetYOrigin(binReader.ReadInt16());
                    this.objTargaHeader.SetWidth(binReader.ReadInt16());
                    this.objTargaHeader.SetHeight(binReader.ReadInt16());

                    byte pixeldepth = binReader.ReadByte();
                    switch (pixeldepth)
                    {
                        case 8:
                        case 16:
                        case 24:
                        case 32:
                            this.objTargaHeader.SetPixelDepth(pixeldepth);
                            break;

                        default:
                            this.ClearAll();
                            throw new Exception("Targa Image only supports 8, 16, 24, or 32 bit pixel depths.");
                    }


                    byte imageDescriptor = binReader.ReadByte();
                    this.objTargaHeader.SetAttributeBits((byte)Utilities.GetBits(imageDescriptor, 0, 4));

                    this.objTargaHeader.SetVerticalTransferOrder((VerticalTransferOrder)Utilities.GetBits(imageDescriptor, 5, 1));
                    this.objTargaHeader.SetHorizontalTransferOrder((HorizontalTransferOrder)Utilities.GetBits(imageDescriptor, 4, 1));

                    // load ImageID value if any
                    if (this.objTargaHeader.ImageIDLength > 0)
                    {
                        byte[] imageIdValueBytes = binReader.ReadBytes(this.objTargaHeader.ImageIDLength);
                        this.objTargaHeader.SetImageIDValue(System.Text.Encoding.ASCII.GetString(imageIdValueBytes).TrimEnd('\0'));
                    }
                }
                catch (Exception)
                {
                    this.ClearAll();
                    throw;
                }


                // load color map if it's included and/or needed
                // Only needed for UNCOMPRESSED_COLOR_MAPPED and RUN_LENGTH_ENCODED_COLOR_MAPPED
                // image types. If color map is included for other file types we can ignore it.
                if (this.objTargaHeader.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
                {
                    if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
                        this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
                    {
                        if (this.objTargaHeader.ColorMapLength > 0)
                        {
                            try
                            {
                                for (int i = 0; i < this.objTargaHeader.ColorMapLength; i++)
                                {
                                    int a = 0;
                                    int r = 0;
                                    int g = 0;
                                    int b = 0;

                                    // load each color map entry based on the ColorMapEntrySize value
                                    switch (this.objTargaHeader.ColorMapEntrySize)
                                    {
                                        case 15:
                                            byte[] color15 = binReader.ReadBytes(2);
                                            // remember that the bytes are stored in reverse oreder
                                            this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color15[1], color15[0]));
                                            break;
                                        case 16:
                                            byte[] color16 = binReader.ReadBytes(2);
                                            // remember that the bytes are stored in reverse oreder
                                            this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color16[1], color16[0]));
                                            break;
                                        case 24:
                                            b = Convert.ToInt32(binReader.ReadByte());
                                            g = Convert.ToInt32(binReader.ReadByte());
                                            r = Convert.ToInt32(binReader.ReadByte());
                                            this.objTargaHeader.ColorMap.Add(System.Drawing.Color.FromArgb(r, g, b));
                                            break;
                                        case 32:
                                            a = Convert.ToInt32(binReader.ReadByte());
                                            b = Convert.ToInt32(binReader.ReadByte());
                                            g = Convert.ToInt32(binReader.ReadByte());
                                            r = Convert.ToInt32(binReader.ReadByte());
                                            this.objTargaHeader.ColorMap.Add(System.Drawing.Color.FromArgb(a, r, g, b));
                                            break;
                                        default:
                                            this.ClearAll();
                                            throw new Exception("TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");

                                    }


                                }
                            }
                            catch (Exception)
                            {
                                this.ClearAll();
                                throw;
                            }



                        }
                        else
                        {
                            this.ClearAll();
                            throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
                        }
                    }


                }
                else
                {
                    if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
                        this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
                    {
                        this.ClearAll();
                        throw new Exception("Image Type requires a Color Map and there was not a Color Map included in the file.");
                    }
                }


            }
            else
            {
                this.ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }
        }


        /// <summary>
        /// Loads the Targa Header information from the file.
        /// </summary>
        /// <param name="binWriter">A BinaryReader that points the loaded file byte stream.</param>
        //private void WriteTgaHeaderInfo(BinaryWriter binWriter)
        //{

        //    if (binWriter?.BaseStream != null && binWriter.BaseStream.Length > 0 && binWriter.BaseStream.CanWrite)
        //    {
        //        try
        //        {
        //            // set the cursor at the beginning of the file.
        //            binWriter.BaseStream.Seek(0, SeekOrigin.Begin);

        //            // read the header properties from the file
        //            binWriter.Write(this.objTargaHeader.ImageIDLength);
        //            binWriter.Write((byte)this.objTargaHeader.ColorMapType);
        //            binWriter.Write((byte)this.objTargaHeader.ImageType);

        //            binWriter.Write(this.objTargaHeader.ColorMapFirstEntryIndex);
        //            binWriter.Write(this.objTargaHeader.ColorMapLength);
        //            binWriter.Write(this.objTargaHeader.ColorMapEntrySize);

        //            binWriter.Write(this.objTargaHeader.XOrigin);
        //            binWriter.Write(this.objTargaHeader.YOrigin);
        //            binWriter.Write(this.objTargaHeader.Width);
        //            binWriter.Write(this.objTargaHeader.Height);

        //            binWriter.Write(this.objTargaHeader.PixelDepth);
                    
        //            byte imageDescriptor = binWriter.ReadByte();
        //            this.objTargaHeader.SetAttributeBits((byte)Utilities.GetBits(imageDescriptor, 0, 4));

        //            this.objTargaHeader.SetVerticalTransferOrder((VerticalTransferOrder)Utilities.GetBits(imageDescriptor, 5, 1));
        //            this.objTargaHeader.SetHorizontalTransferOrder((HorizontalTransferOrder)Utilities.GetBits(imageDescriptor, 4, 1));

        //            // load ImageID value if any
        //            if (this.objTargaHeader.ImageIDLength > 0)
        //            {
        //                byte[] imageIdValueBytes = binWriter.ReadBytes(this.objTargaHeader.ImageIDLength);
        //                this.objTargaHeader.SetImageIDValue(System.Text.Encoding.ASCII.GetString(imageIdValueBytes).TrimEnd('\0'));
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            this.ClearAll();
        //            throw;
        //        }


        //        // load color map if it's included and/or needed
        //        // Only needed for UNCOMPRESSED_COLOR_MAPPED and RUN_LENGTH_ENCODED_COLOR_MAPPED
        //        // image types. If color map is included for other file types we can ignore it.
        //        if (this.objTargaHeader.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
        //        {
        //            if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
        //                this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
        //            {
        //                if (this.objTargaHeader.ColorMapLength > 0)
        //                {
        //                    try
        //                    {
        //                        for (int i = 0; i < this.objTargaHeader.ColorMapLength; i++)
        //                        {
        //                            int a = 0;
        //                            int r = 0;
        //                            int g = 0;
        //                            int b = 0;

        //                            // load each color map entry based on the ColorMapEntrySize value
        //                            switch (this.objTargaHeader.ColorMapEntrySize)
        //                            {
        //                                case 15:
        //                                    byte[] color15 = binWriter.ReadBytes(2);
        //                                    // remember that the bytes are stored in reverse oreder
        //                                    this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color15[1], color15[0]));
        //                                    break;
        //                                case 16:
        //                                    byte[] color16 = binWriter.ReadBytes(2);
        //                                    // remember that the bytes are stored in reverse oreder
        //                                    this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color16[1], color16[0]));
        //                                    break;
        //                                case 24:
        //                                    b = Convert.ToInt32(binWriter.ReadByte());
        //                                    g = Convert.ToInt32(binWriter.ReadByte());
        //                                    r = Convert.ToInt32(binWriter.ReadByte());
        //                                    this.objTargaHeader.ColorMap.Add(Color.FromArgb(r, g, b));
        //                                    break;
        //                                case 32:
        //                                    a = Convert.ToInt32(binWriter.ReadByte());
        //                                    b = Convert.ToInt32(binWriter.ReadByte());
        //                                    g = Convert.ToInt32(binWriter.ReadByte());
        //                                    r = Convert.ToInt32(binWriter.ReadByte());
        //                                    this.objTargaHeader.ColorMap.Add(Color.FromArgb(a, r, g, b));
        //                                    break;
        //                                default:
        //                                    this.ClearAll();
        //                                    throw new Exception("TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");

        //                            }


        //                        }
        //                    }
        //                    catch (Exception)
        //                    {
        //                        this.ClearAll();
        //                        throw;
        //                    }



        //                }
        //                else
        //                {
        //                    this.ClearAll();
        //                    throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
        //                }
        //            }


        //        }
        //        else
        //        {
        //            if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
        //                this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
        //            {
        //                this.ClearAll();
        //                throw new Exception("Image Type requires a Color Map and there was not a Color Map included in the file.");
        //            }
        //        }


        //    }
        //    else
        //    {
        //        this.ClearAll();
        //        throw new Exception(@"Error loading file, could not read file from disk.");
        //    }
        //}


        /// <summary>
        /// Loads the Targa Extension Area from the file, if it exists.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTgaExtensionArea(BinaryReader binReader)
        {

            if (binReader != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek)
            {
                // is there an Extension Area in file
                if (this.objTargaFooter.ExtensionAreaOffset > 0)
                {
                    try
                    {
                        // set the cursor at the beginning of the Extension Area using ExtensionAreaOffset.
                        binReader.BaseStream.Seek(this.objTargaFooter.ExtensionAreaOffset, SeekOrigin.Begin);

                        // load the extension area fields from the file
                        this.objTargaExtensionArea.SetExtensionSize((int)(binReader.ReadInt16()));
                        this.objTargaExtensionArea.SetAuthorName(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorNameByteLength)).TrimEnd('\0'));
                        this.objTargaExtensionArea.SetAuthorComments(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorCommentsByteLength)).TrimEnd('\0'));


                        // get the date/time stamp of the file
                        Int16 iMonth = binReader.ReadInt16();
                        Int16 iDay = binReader.ReadInt16();
                        Int16 iYear = binReader.ReadInt16();
                        Int16 iHour = binReader.ReadInt16();
                        Int16 iMinute = binReader.ReadInt16();
                        Int16 iSecond = binReader.ReadInt16();
                        DateTime dtstamp;
                        string strStamp = $"{iMonth}/{iDay}/{iYear} {iHour}:{iMinute}:{iSecond}";
                        if (DateTime.TryParse(strStamp, out dtstamp))
                            this.objTargaExtensionArea.SetDateTimeStamp(dtstamp);


                        this.objTargaExtensionArea.SetJobName(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaJobNameByteLength)).TrimEnd('\0'));


                        // get the job time of the file
                        iHour = binReader.ReadInt16();
                        iMinute = binReader.ReadInt16();
                        iSecond = binReader.ReadInt16();
                        var ts = new TimeSpan(iHour, iMinute, iSecond);
                        this.objTargaExtensionArea.SetJobTime(ts);


                        this.objTargaExtensionArea.SetSoftwareID(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareIDByteLength)).TrimEnd('\0'));


                        // get the version number and letter from file
                        float iVersionNumber = (float)binReader.ReadInt16() / 100.0F;
                        string strVersionLetter = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareVersionLetterByteLength)).TrimEnd('\0');


                        this.objTargaExtensionArea.SetSoftwareID(iVersionNumber.ToString(@"F2") + strVersionLetter);


                        // get the color key of the file
                        int a = binReader.ReadByte();
                        int r = binReader.ReadByte();
                        int b = binReader.ReadByte();
                        int g = binReader.ReadByte();
                        this.objTargaExtensionArea.SetKeyColor(Color.FromArgb(a, r, g, b));


                        this.objTargaExtensionArea.SetPixelAspectRatioNumerator(binReader.ReadInt16());
                        this.objTargaExtensionArea.SetPixelAspectRatioDenominator(binReader.ReadInt16());
                        this.objTargaExtensionArea.SetGammaNumerator(binReader.ReadInt16());
                        this.objTargaExtensionArea.SetGammaDenominator(binReader.ReadInt16());
                        this.objTargaExtensionArea.SetColorCorrectionOffset(binReader.ReadInt32());
                        this.objTargaExtensionArea.SetPostageStampOffset(binReader.ReadInt32());
                        this.objTargaExtensionArea.SetScanLineOffset(binReader.ReadInt32());
                        this.objTargaExtensionArea.SetAttributesType(binReader.ReadByte());


                        // load Scan Line Table from file if any
                        if (this.objTargaExtensionArea.ScanLineOffset > 0)
                        {
                            binReader.BaseStream.Seek(this.objTargaExtensionArea.ScanLineOffset, SeekOrigin.Begin);
                            for (int i = 0; i < this.objTargaHeader.Height; i++)
                            {
                                this.objTargaExtensionArea.ScanLineTable.Add(binReader.ReadInt32());
                            }
                        }


                        // load Color Correction Table from file if any
                        if (this.objTargaExtensionArea.ColorCorrectionOffset > 0)
                        {
                            binReader.BaseStream.Seek(this.objTargaExtensionArea.ColorCorrectionOffset, SeekOrigin.Begin);
                            for (int i = 0; i < TargaConstants.ExtensionAreaColorCorrectionTableValueLength; i++)
                            {
                                a = binReader.ReadInt16();
                                r = binReader.ReadInt16();
                                b = binReader.ReadInt16();
                                g = binReader.ReadInt16();
                                this.objTargaExtensionArea.ColorCorrectionTable.Add(Color.FromArgb(a, r, g, b));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        this.ClearAll();
                        throw;
                    }
                }
            }
            else
            {
                this.ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }
        }

        /// <summary>
        /// Reads the image data bytes from the file. Handles Uncompressed and RLE Compressed image data. 
        /// Uses FirstPixelDestination to properly align the image.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        /// <returns>An array of bytes representing the image data in the proper alignment.</returns>
        private byte[] LoadImageBytes(BinaryReader binReader)
        {

            // read the image data into a byte array
            // take into account stride has to be a multiple of 4
            // use padding to make sure multiple of 4    

            byte[] data = null;
            if (binReader?.BaseStream != null && binReader.BaseStream.Length > 0 && binReader.BaseStream.CanSeek)
            {
                if (this.objTargaHeader.ImageDataOffset > 0)
                {
                    // padding bytes
                    byte[] padding = new byte[this.padding];
                    MemoryStream msData = null;
                    List<List<byte>> rows = null;
                    List<byte> row = null;
                    rows = new List<List<byte>>();
                    row = new List<byte>();


                    // seek to the beginning of the image data using the ImageDataOffset value
                    binReader.BaseStream.Seek(this.objTargaHeader.ImageDataOffset, SeekOrigin.Begin);


                    // get the size in bytes of each row in the image
                    int intImageRowByteSize = (int)this.objTargaHeader.Width * ((int)this.objTargaHeader.BytesPerPixel);

                    // get the size in bytes of the whole image
                    int intImageByteSize = intImageRowByteSize * (int)this.objTargaHeader.Height;

                    // is this a RLE compressed image type
                    if (this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE ||
                       this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED ||
                       this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_TRUE_COLOR)
                    {

                        #region COMPRESSED

                        // RLE Packet info
                        byte bRlePacket = 0;
                        int intRlePacketType = -1;
                        int intRlePixelCount = 0;
                        byte[] bRunLengthPixel = null;

                        // used to keep track of bytes read
                        int intImageBytesRead = 0;
                        int intImageRowBytesRead = 0;

                        // keep reading until we have the all image bytes
                        while (intImageBytesRead < intImageByteSize)
                        {
                            // get the RLE packet
                            bRlePacket = binReader.ReadByte();
                            intRlePacketType = Utilities.GetBits(bRlePacket, 7, 1);
                            intRlePixelCount = Utilities.GetBits(bRlePacket, 0, 7) + 1;

                            // check the RLE packet type
                            if ((RLEPacketType)intRlePacketType == RLEPacketType.RUN_LENGTH)
                            {
                                // get the pixel color data
                                bRunLengthPixel = binReader.ReadBytes((int)this.objTargaHeader.BytesPerPixel);

                                // add the number of pixels specified using the read pixel color
                                for (int i = 0; i < intRlePixelCount; i++)
                                {
                                    row.AddRange(bRunLengthPixel);

                                    // increment the byte counts
                                    intImageRowBytesRead += bRunLengthPixel.Length;
                                    intImageBytesRead += bRunLengthPixel.Length;

                                    // if we have read a full image row
                                    // add the row to the row list and clear it
                                    // restart row byte count
                                    if (intImageRowBytesRead == intImageRowByteSize)
                                    {
                                        rows.Add(row);
                                        row = new List<byte>();
                                        intImageRowBytesRead = 0;

                                    }
                                }

                            }

                            else if ((RLEPacketType)intRlePacketType == RLEPacketType.RAW)
                            {
                                // get the number of bytes to read based on the read pixel count
                                int intBytesToRead = intRlePixelCount * this.objTargaHeader.BytesPerPixel;

                                // read each byte
                                for (int i = 0; i < intBytesToRead; i++)
                                {
                                    row.Add(binReader.ReadByte());

                                    // increment the byte counts
                                    intImageBytesRead++;
                                    intImageRowBytesRead++;

                                    // if we have read a full image row
                                    // add the row to the row list and clear it
                                    // restart row byte count
                                    if (intImageRowBytesRead == intImageRowByteSize)
                                    {
                                        rows.Add(row);
                                        row = new List<byte>();
                                        intImageRowBytesRead = 0;
                                    }

                                }

                            }
                        }

                        #endregion

                    }

                    else
                    {
                        #region NON-COMPRESSED

                        // loop through each row in the image
                        for (int i = 0; i < this.objTargaHeader.Height; i++)
                        {
                            // loop through each byte in the row
                            for (int j = 0; j < intImageRowByteSize; j++)
                            {
                                // add the byte to the row
                                row.Add(binReader.ReadByte());
                            }

                            // add row to the list of rows
                            rows.Add(row);
                            row = new List<byte>();
                        }


                        #endregion
                    }

                    // flag that states whether or not to reverse the location of all rows.
                    bool blnRowsReverse = false;

                    // flag that states whether or not to reverse the bytes in each row.
                    bool blnEachRowReverse = false;

                    // use FirstPixelDestination to determine the alignment of the 
                    // image data byte
                    switch (this.objTargaHeader.FirstPixelDestination)
                    {
                        case FirstPixelDestination.TOP_LEFT:
                            blnRowsReverse = false;
                            blnEachRowReverse = true;
                            break;

                        case FirstPixelDestination.TOP_RIGHT:
                            blnRowsReverse = false;
                            blnEachRowReverse = false;
                            break;

                        case FirstPixelDestination.BOTTOM_LEFT:
                            blnRowsReverse = true;
                            blnEachRowReverse = true;
                            break;

                        case FirstPixelDestination.BOTTOM_RIGHT:
                        case FirstPixelDestination.UNKNOWN:
                            blnRowsReverse = true;
                            blnEachRowReverse = false;

                            break;
                    }

                    // write the bytes from each row into a memory stream and get the 
                    // resulting byte array
                    using (msData = new MemoryStream())
                    {

                        // do we reverse the rows in the row list.
                        if (blnRowsReverse == true)
                            rows.Reverse();

                        // go through each row
                        for (int i = 0; i < rows.Count; i++)
                        {
                            // do we reverse the bytes in the row
                            if (blnEachRowReverse == true)
                                rows[i].Reverse();

                            // get the byte array for the row
                            byte[] brow = rows[i].ToArray();

                            // write the row bytes and padding bytes to the memory streem
                            msData.Write(brow, 0, brow.Length);
                            msData.Write(padding, 0, padding.Length);
                        }
                        // get the image byte array
                        data = msData.ToArray();

                    }

                    // clear our row arrays
                    for (int i = 0; i < rows.Count; i++)
                    {
                        rows[i].Clear();
                        rows[i] = null;
                    }
                    rows.Clear();
                }
                else
                {
                    this.ClearAll();
                    throw new Exception(@"Error loading file, No image data in file.");
                }
            }
            else
            {
                this.ClearAll();
                throw new Exception(@"Error loading file, could not read file from disk.");
            }

            // return the image byte array
            return data;

        }

        /// <summary>
        /// Reads the image data bytes from the file and loads them into the Image Bitmap object.
        /// Also loads the color map, if any, into the Image Bitmap.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTgaImage(BinaryReader binReader)
        {

            // make sure we don't have a phantom Bitmap
            this.bitmapTargaImage?.Dispose();

            //**************  NOTE  *******************
            // The memory allocated for Microsoft Bitmaps must be aligned on a 32bit boundary.
            // The stride refers to the number of bytes allocated for one scanline of the bitmap.
            // In your loop, you copy the pixels one scanline at a time and take into
            // consideration the amount of padding that occurs due to memory alignment.
            // calculate the stride, in bytes, of the image (32bit aligned width of each image row)
            this.stride = ((this.objTargaHeader.Width * this.objTargaHeader.PixelDepth + 31) & ~31) >> 3; // width in bytes

            // calculate the padding, in bytes, of the image 
            // number of bytes to add to make each row a 32bit aligned row
            // padding in bytes
            this.padding = this.stride - (((this.objTargaHeader.Width * this.objTargaHeader.PixelDepth) + 7) / 8);


            // get the Pixel format to use with the Bitmap object
            var pf = this.GetPixelFormat();

            // get the image data bytes
            byte[] bimagedata = this.LoadImageBytes(binReader);

            // since the Bitmap constructor requires a poiter to an array of image bytes
            // we have to pin down the memory used by the byte array and use the pointer 
            // of this pinned memory to create the Bitmap.
            // This tells the Garbage Collector to leave the memory alone and DO NOT touch it.
            this.imageByteHandle = GCHandle.Alloc(bimagedata, GCHandleType.Pinned);



            // create a Bitmap object using the image Width, Height,
            // Stride, PixelFormat and the pointer to the pinned byte array.
            this.bitmapTargaImage = new Bitmap(
                this.objTargaHeader.Width, this.objTargaHeader.Height,
                this.stride, pf, this.imageByteHandle.AddrOfPinnedObject());
            
            // lets free the pinned bytes
            if (this.imageByteHandle != null && this.imageByteHandle.IsAllocated)
                this.imageByteHandle.Free();

            // load the color map into the Bitmap, if it exists
            if (this.objTargaHeader.ColorMap.Count > 0)
            {
                // get the Bitmap's current palette
                ColorPalette pal = this.bitmapTargaImage.Palette;

                // loop trough each color in the loaded file's color map
                for (int i = 0; i < this.objTargaHeader.ColorMap.Count; i++)
                {
                    // is the AttributesType 0 or 1 bit
                    bool forceopaque = false;

                    if (this.Format == TGAFormat.NEW_TGA && this.objTargaFooter.ExtensionAreaOffset > 0)
                    {
                        if (this.objTargaExtensionArea.AttributesType == 0 || this.objTargaExtensionArea.AttributesType == 1)
                            forceopaque = true;
                    }
                    else if (this.Header.AttributeBits == 0 || this.Header.AttributeBits == 1)
                        forceopaque = true;

                    if (forceopaque)
                        // use 255 for alpha ( 255 = opaque/visible ) so we can see the image
                        pal.Entries[i] = Color.FromArgb(255, this.objTargaHeader.ColorMap[i].R, this.objTargaHeader.ColorMap[i].G, this.objTargaHeader.ColorMap[i].B);

                    else
                        // use whatever value is there
                        pal.Entries[i] = this.objTargaHeader.ColorMap[i];

                }

                // set the new palette back to the Bitmap object
                this.bitmapTargaImage.Palette = pal;
            }
            else
            { // no color map


                // check to see if this is a Black and White (Greyscale)
                if (this.objTargaHeader.PixelDepth == 8 && (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_BLACK_AND_WHITE ||
                    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE))
                {
                    // get the current palette
                    ColorPalette pal = this.bitmapTargaImage.Palette;

                    // create the Greyscale palette
                    for (int i = 0; i < 256; i++)
                    {
                        pal.Entries[i] = Color.FromArgb(i, i, i);
                    }

                    // set the new palette back to the Bitmap object
                    this.bitmapTargaImage.Palette = pal;
                }
            }

            this.bitmapSourceTargaImage = BitmapSource.Create(
                this.bitmapTargaImage.Width, this.bitmapTargaImage.Height,
                96, 96, PixelFormats.Bgr32, null, bimagedata, this.stride);
        }

        public void WriteTo(Stream output)
        {
            var binaryWritter = new BinaryWriter(output, Encoding.Default, false);

        }

        public void WriteTo(string path)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                this.WriteTo(fs);
            }
        }

        /// <summary>
        /// Gets the PixelFormat to be used by the Image based on the Targa file's attributes
        /// </summary>
        /// <returns></returns>
        private PixelFormat GetPixelFormat()
        {

            PixelFormat pfTargaPixelFormat = PixelFormat.Undefined;

            // first off what is our Pixel Depth (bits per pixel)
            switch (this.objTargaHeader.PixelDepth)
            {
                case 8:
                    pfTargaPixelFormat = PixelFormat.Format8bppIndexed;
                    break;

                case 16:
                    // if this is a new tga file and we have an extension area, we'll determine the alpha based on 
                    // the extension area Attributes 
                    if (this.Format == TGAFormat.NEW_TGA && this.objTargaFooter.ExtensionAreaOffset > 0)
                    {
                        switch (this.objTargaExtensionArea.AttributesType)
                        {
                            case 0:
                            case 1:
                            case 2: // no alpha data
                                pfTargaPixelFormat = PixelFormat.Format16bppRgb555;
                                break;

                            case 3: // useful alpha data
                                pfTargaPixelFormat = PixelFormat.Format16bppArgb1555;
                                break;
                        }
                    }
                    else
                    {
                        // just a regular tga, determine the alpha based on the Header Attributes
                        if (this.Header.AttributeBits == 0)
                            pfTargaPixelFormat = PixelFormat.Format16bppRgb555;
                        if (this.Header.AttributeBits == 1)
                            pfTargaPixelFormat = PixelFormat.Format16bppArgb1555;
                    }

                    break;

                case 24:
                    pfTargaPixelFormat = PixelFormat.Format24bppRgb;
                    break;

                case 32:

                    // if this is a new tga file and we have an extension area, we'll determine the alpha based on 
                    // the extension area Attributes 
                    if (this.Format == TGAFormat.NEW_TGA && this.objTargaFooter.ExtensionAreaOffset > 0)
                    {
                        switch (this.objTargaExtensionArea.AttributesType)
                        {

                            case 0:
                            case 1:
                            case 2: // no alpha data
                                pfTargaPixelFormat = PixelFormat.Format32bppRgb;
                                break;

                            case 3: // useful alpha data
                                pfTargaPixelFormat = PixelFormat.Format32bppArgb;
                                break;

                            case 4: // premultiplied alpha data
                                pfTargaPixelFormat = PixelFormat.Format32bppPArgb;
                                break;

                        }
                    }
                    else
                    {
                        // just a regular tga, determine the alpha based on the Header Attributes
                        if (this.Header.AttributeBits == 0)
                            pfTargaPixelFormat = PixelFormat.Format32bppRgb;
                        if (this.Header.AttributeBits == 8)
                            pfTargaPixelFormat = PixelFormat.Format32bppArgb;

                        break;
                    }



                    break;

            }


            return pfTargaPixelFormat;
        }
        

        /// <summary>
        /// Clears out all objects and resources.
        /// </summary>
        private void ClearAll()
        {
            if (this.bitmapTargaImage != null)
            {
                this.bitmapTargaImage.Dispose();
                this.bitmapTargaImage = null;
            }
            
            if (this.imageByteHandle.IsAllocated)
                this.imageByteHandle.Free();
            
            this.objTargaHeader = new TargaHeader();
            this.objTargaExtensionArea = new TargaExtensionArea();
            this.objTargaFooter = new TargaFooter();
            this.eTgaFormat = TGAFormat.UNKNOWN;
            this.stride = 0;
            this.padding = 0;
            this.fileInfo = null;

        }

        /// <summary>
        /// Loads a Targa image file into a Bitmap object.
        /// </summary>
        /// <param name="fileName">The Targa image filename</param>
        /// <returns>A Bitmap object with the Targa image loaded into it.</returns>
        public static Bitmap LoadTargaImage(string fileName)
        {
            using (TgaImage ti = new TgaImage(fileName))
            {
                return CopyToBitmap(ti);
            }
        }

        /// <summary>
        /// Loads a stream with Targa image data into a Bitmap object.
        /// </summary>
        /// <param name="sFileName">The Targa image stream</param>
        /// <returns>A Bitmap object with the Targa image loaded into it.</returns>
        public static Bitmap LoadTargaImage(Stream imageStream)
        {
            using (TgaImage ti = new TgaImage(imageStream))
            {
                return CopyToBitmap(ti);
            }
        }

        private static Bitmap CopyToBitmap(TgaImage ti)
        {
            Bitmap b = null;
            if (ti.BitmapImage.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                b = (Bitmap)ti.BitmapImage.Clone();
            }
            else
            {
                b = new Bitmap(ti.BitmapImage.Width, ti.BitmapImage.Height, ti.BitmapImage.PixelFormat);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.DrawImage(ti.BitmapImage, 0, 0, new Rectangle(0, 0, b.Width, b.Height), GraphicsUnit.Pixel);
                }
            }
            return b;
        }


        #region IDisposable Members

        /// <summary>
        /// Disposes all resources used by this instance of the TargaImage class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);

        }


        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">If true dispose all resources, else dispose only release unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.bitmapTargaImage != null)
                    {
                        this.bitmapTargaImage.Dispose();
                    }

                    if (this.imageByteHandle != null)
                    {
                        if (this.imageByteHandle.IsAllocated)
                        {
                            this.imageByteHandle.Free();
                        }

                    }

                    if (this.objTargaHeader != null)
                    {
                        this.objTargaHeader.ColorMap.Clear();
                        this.objTargaHeader = null;
                    }
                    if (this.objTargaExtensionArea != null)
                    {
                        this.objTargaExtensionArea.ColorCorrectionTable.Clear();
                        this.objTargaExtensionArea.ScanLineTable.Clear();
                        this.objTargaExtensionArea = null;
                    }

                    this.objTargaFooter = null;

                }
                // Release unmanaged resources. If disposing is false, 
                // only the following code is executed.
                // ** release unmanged resources here **

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.

            }
            this.disposed = true;
        }


        #endregion
    }

}