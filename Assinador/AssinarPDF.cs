using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.IO;
using System.Threading.Tasks;

namespace Assinador
{
    public static class AssinarPDF
    {
        public static async Task<MemoryStream> Sign(string caminhoCertificado, string senha, MemoryStream sourceFile)
        {
            string fileName = "";
            int bufferSize;
            FileStream fs = null;

            try
            {
                fileName = System.IO.Path.GetTempFileName() + ".pdf";
                bufferSize = sourceFile.ToArray().Length;
                await File.WriteAllBytesAsync(fileName, sourceFile.ToArray());
                fs = File.OpenRead(fileName);

                char[] PASSWORD = senha.ToCharArray();

                Pkcs12Store pk12 = new Pkcs12Store(new FileStream(caminhoCertificado, FileMode.Open, FileAccess.Read), PASSWORD);
                string alias = null;
                foreach (object a in pk12.Aliases)
                {
                    alias = ((string)a);
                    if (pk12.IsKeyEntry(alias))
                    {
                        break;
                    }
                }
                ICipherParameters pk = pk12.GetKey(alias).Key;

                X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
                X509Certificate[] chain = new X509Certificate[ce.Length];
                for (int k = 0; k < ce.Length; ++k)
                {
                    chain[k] = ce[k].Certificate;
                }

                PdfReader reader = new PdfReader(fs);

                MemoryStream outputStream = new MemoryStream();

                PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties());

                PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
                appearance.SetReason("Apenas teste")
                    .SetLocation("Brazil")
                    .SetPageRect(new Rectangle(36, 648, 200, 100))
                    .SetPageNumber(1);
                signer.SetFieldName("Goiabada");

                IExternalSignature pks = new PrivateKeySignature(pk, DigestAlgorithms.SHA256);

                signer.SignDetached(pks, chain, null, null, null, 0,
                PdfSigner.CryptoStandard.CMS);

                return outputStream;
            }
            catch
            {
                fs.Close();
                File.Delete(fileName);
            }
            finally
            {
                fs.Close();
                File.Delete(fileName);
            }

            return sourceFile;
        }
    }
}
