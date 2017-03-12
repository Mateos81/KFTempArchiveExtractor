using System.IO;
using System.Text;

namespace TempArchiveExtractor
{
    /// <summary>
    /// Represents a Steam Workshop "temporary" archive file.
    /// </summary>
    public class TempArchiveReader
    {
        #region Fields

        /// <summary>
        /// Size of a block.
        /// </summary>
        const int BLOCKSIZE = 0x10000;

        /// <summary>
        /// Archive stream.
        /// </summary>
        FileStream _archiveStream;

        /// <summary>
        /// Archive file encoding.
        /// </summary>
        static Encoding ANSIEncoding = Encoding.GetEncoding(1252);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="archiveStream">Archive to read.</param>
        public TempArchiveReader(FileStream archiveStream)
        {
            _archiveStream = archiveStream;
        }

        #endregion Constructors

        #region Methods

        public int ReadInt32()
        {
            int result = _archiveStream.ReadByte();
            result += _archiveStream.ReadByte() << 8;
            result += _archiveStream.ReadByte() << 16;
            result += _archiveStream.ReadByte() << 32;
            return result;
        }

        public int ReadFCompactIndex()
        {
            int result = 0;
            int B0 = _archiveStream.ReadByte();
            //String msg = String.Format("FCompactIndex: 0x{0:X}", B0);
            if ((B0 & 0x40) != 0)
            {
                int B1 = _archiveStream.ReadByte();
                //msg += String.Format("{0:X}", B1);
                if ((B1 & 0x80) != 0)
                {
                    int B2 = _archiveStream.ReadByte();
                    //msg += String.Format("{0:X}", B2);
                    if ((B2 & 0x80) != 0)
                    {
                        int B3 = _archiveStream.ReadByte();
                        //msg += String.Format("{0:X}", B3);
                        if ((B3 & 0x80) != 0)
                        {
                            int B4 = _archiveStream.ReadByte();
                            //msg += String.Format("{0:X}", B4);
                            result = B4;
                        }
                        result = (result << 7) + (B3 & 0x7F);
                    }
                    result = (result << 7) + (B2 & 0x7F);
                }
                result = (result << 7) + (B1 & 0x7F);
            }
            result = (result << 6) + (B0 & 0x3F);
            if ((B0 & 0x80) != 0) result *= -1;

            //msg += String.Format(" => 0x{0:X} {0}", result);
            //Console.Out.WriteLine(msg);

            return result;
        }

        public string ReadFString()
        {
            int strLen = ReadFCompactIndex();
            byte[] strBytes = new byte[strLen];
            _archiveStream.Read(strBytes, 0, strLen);

            //String msg = String.Format("FString: length 0x{0:X} {0} : {1:X}..{2:X}"
            //    , strLen, strBytes[0], strBytes[strLen-1]);
            //Console.Out.WriteLine(msg);

            string str = ANSIEncoding.GetString(strBytes, 0, strLen - 1);
            return str;
        }

        public void Skip(int count)
        {
            _archiveStream.Seek(count, SeekOrigin.Current);
        }

        public void ReadIntoStream(int count, Stream outStream)
        {
            byte[] buffer = new byte[BLOCKSIZE];
            while (count > BLOCKSIZE)
            {
                _archiveStream.Read(buffer, 0, BLOCKSIZE);
                outStream.Write(buffer, 0, BLOCKSIZE);
                count -= BLOCKSIZE;
            }
            _archiveStream.Read(buffer, 0, count);
            outStream.Write(buffer, 0, count);
        }

        #endregion Methods
    }
}