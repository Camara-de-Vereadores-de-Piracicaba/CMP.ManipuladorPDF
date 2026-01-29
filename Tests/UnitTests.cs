using CMP.Certificados;
using CMP.ManipuladorPDF;

namespace Tests
{
    [TestClass]
    public class PDFTests
    {

        public TestContext TestContext { get; set; }

        private void Debug(string text)
        {
            TestContext.WriteLine(text);
        }

        [TestMethod]
        public void HtmlToPdfTest()
        {
            string html = $@"<h1>Este é um PDF convertido para HTML</h1>";
            DocumentoPDF pdf = html.ConverterParaPdf();
        }

        [TestMethod]
        public void PasswordTest()
        {
            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_password.pdf"));
            pdf = pdf.Desencriptar("user");
            DocumentoPDF final = pdf.TornarSemEfeito();
        }

        [TestMethod]
        public void EncryptedTest()
        {
            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_encrypted.pdf"));
            DocumentoPDF final = pdf.TornarSemEfeito();
        }

        [TestMethod]
        public void ExternalSignaturesTest()
        {
            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_complex.pdf"));
            List<AssinanteDocumento> assinantes = pdf.Assinantes();

            if (
                assinantes[0].Nome != "Douglas Miranda da Silva" ||
                assinantes[1].Nome != "Aline Ribeiro de Campos Mello")
            {
                throw new Exception();
            }
        }

        [TestMethod]
        public void ExtractPageTest()
        {
            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_complex.pdf"));
            pdf.ExtrairPagina(1);
        }

        [TestMethod]
        public void LastPageTest()
        {
            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_problematic.pdf"));
            DocumentoPDF final = pdf
                .AdicionarDetalhesAoFinal("XXXXXXXX");

        }

        [TestMethod]
        public void MetadataTest()
        {
            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_problematic.pdf"));
            pdf = pdf.AdicionarMetadado(new Metadado("Nome", "Valor"));
            pdf = pdf.AdicionarMetadado(new Metadado("Nome2", "Valor2"));
            List<Metadado> metadata = pdf.ObtemMetadados();
            if (metadata.Count() != 6)
            {
                throw new Exception();
            }
        }

        [TestMethod]
        public async Task SignatureTest()
        {
            try
            {
                Certificado raiz = new(FileManager.GetFile("ca.pfx"), "ET1w4VGjsRlFuyfUd5kbNamD8oZiXLBp");
                Certificado cert = new(raiz, "TESTE", "teste@teste.com.br", "1234ab");
                await cert.AdicionarOCSP();

                DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_camara.pdf"));

                DocumentoPDF final = pdf.Assinar(cert, 1, 20, 770);
                List<AssinanteDocumento> assinantes = final.Assinantes();
            }
            catch (Exception ex)
            {
                Debug(ex.ToString());
                throw;
            }

        }

        [TestMethod]
        public async Task CompleteTest()
        {

            Certificado raiz = new Certificado(FileManager.GetFile("ca.pfx"), "ET1w4VGjsRlFuyfUd5kbNamD8oZiXLBp");
            Certificado cert = new Certificado(raiz, "TESTE", "teste@teste.com.br", "1234ab");

            await cert.AdicionarOCSP();

            DocumentoPDF pdf = new(FileManager.GetFile("test_pdf_problematic.pdf"));

            string secondString = $@"<h2>Este é outro documento convertido para PDF.</h2><img src=""https://picsum.photos/1024/1024"" />";
            DocumentoPDF pdf2 = secondString.ConverterParaPdf();

            DocumentoPDF final =
                pdf
                    .Juntar(pdf2)
                    .Numerar()
                    .AdicionarMetadado(new Metadado("Nome", "Valor"))
                    .TornarSemEfeito()
                    .Assinar(cert, 1, 20, 770)
                    .Protocolar("AAAAA")
                    .AdicionarDetalhesAoFinal("XXXXXXXX");

            List<AssinanteDocumento> assinantes = final.Assinantes();

            final.TestarValidadeDasAssinaturas(ValidacaoAssinatura.PARCIAL);

            final.OCR();

        }

    }
}