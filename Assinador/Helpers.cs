using iText.Kernel.Font;
using iText.Signatures;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace CMP.ManipuladorPDF
{

    internal static class EmbeddedResource
    {

        public static Stream GetStream(string fileName)
        {
            string name = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        public static byte[] GetByteArray(string fileName)
        {
            using Stream Stream = EmbeddedResource.GetStream(fileName);
            byte[] ByteArray = new byte[Stream.Length];
            Stream.Read(ByteArray, 0, ByteArray.Length);
            return ByteArray;
        }

    }

    internal static class PDFTrueTypeFont
    {
        private const string DefaultFont = "Roboto-Regular";

        public static PdfFont GetFont(string fontName = DefaultFont)
        {
            byte[] byteArray = EmbeddedResource.GetByteArray($"{fontName}.ttf");
            return PdfFontFactory.CreateFont(byteArray, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        }

    }

}