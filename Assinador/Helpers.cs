using iText.Kernel.Font;
using iText.Signatures;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Org.BouncyCastle.X509;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace CMP.ManipuladorPDF
{

    internal static class Conversion
    {
        public static double ToRadians(int degrees)
        {
            return Math.PI / 180 * degrees;
        }
    }

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

    internal static class ImageExtensions
    {
        private static int DetectCropHeight(this Image<Rgba32> image)
        {
            int cropHeight = 0;
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    int transparent = 0;
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        ref Rgba32 pixel = ref pixelRow[x];
                        if (pixel.A == 0)
                        {
                            transparent++;
                        }
                    }

                    if (transparent < pixelRow.Length)
                    {
                        cropHeight = y;
                        break;
                    }
                }
            });
            return cropHeight;
        }

        private static Image AutoCrop(this Image<Rgba32> image)
        {
            int cropHeight=DetectCropHeight(image);
            image.Mutate(x => x
                .Crop(
                    new Rectangle(0,cropHeight,image.Width,image.Height-cropHeight)
                )
                .Rotate(270, KnownResamplers.Lanczos3)
            );

            cropHeight = DetectCropHeight(image);
            image.Mutate(x => x
                .Crop(
                    new Rectangle(0, cropHeight, image.Width, image.Height - cropHeight)
                )
                .Rotate(90, KnownResamplers.Lanczos3)
            );
            return image;
        }

    }

    class X509Certificate2RSASignature : IExternalSignature
    {
        public X509Certificate2RSASignature(X509Certificate2 certificate)
        {
            this.certificate = certificate;
        }

        public Org.BouncyCastle.X509.X509Certificate[] GetChain()
        {
            var bcCertificate = new Org.BouncyCastle.X509.X509Certificate(X509CertificateStructure.GetInstance(certificate.RawData));
            return new Org.BouncyCastle.X509.X509Certificate[] { bcCertificate };
        }

        public string GetSignatureAlgorithmName()
        {
            return "RSA";
        }

        public ISignatureMechanismParams GetSignatureMechanismParameters()
        {
            return null;
        }

        public string GetDigestAlgorithmName()
        {
            return "SHA512";
        }

        public byte[] Sign(byte[] message)
        {
            using RSA rsa = certificate.GetRSAPrivateKey();
            return rsa.SignData(message, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }

        X509Certificate2 certificate;
    }
}