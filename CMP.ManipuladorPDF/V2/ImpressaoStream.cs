using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;

namespace CMP.ManipuladorPDF.V2
{

    /// <summary>
    /// Fonte preguiçosa de um documento a ser impresso. O conteúdo só é aberto/lido
    /// quando a impressão chega nele, permitindo montar cadeias grandes sem manter
    /// todos os documentos em memória ao mesmo tempo.
    /// </summary>
    public class FonteDocumentoImpressao
    {

        /// <summary>
        /// Abre o conteúdo do PDF de origem sob demanda (ex.: um <see cref="FileStream"/> sobre o
        /// caminho em disco). O stream retornado é lido e descartado por documento.
        /// </summary>
        public Func<Stream> AbrirConteudo { get; set; }

        /// <summary>
        /// Hash do documento (código de verificação) usado no carimbo de protocolo.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Título lógico do documento, exibido na página de detalhes ao lado de cada assinatura.
        /// </summary>
        public string Titulo { get; set; }

    }

    /// <summary>
    /// Impressão de uma cadeia de documentos (protocolo + numeração + detalhes finais) escrevendo
    /// <b>incrementalmente</b> num <see cref="Stream"/> de destino, sem materializar a cadeia inteira
    /// em memória. Cada documento é lido, protocolado e numerado isoladamente e mesclado direto no
    /// documento de saída; ao final, a página de detalhes com os assinantes acumulados é anexada.
    ///
    /// <para>É a alternativa de baixo consumo de RAM à composição em <c>byte[]</c> (que re-serializa a
    /// cadeia inteira a cada junção). O destino pode ser qualquer <see cref="Stream"/> — inclusive um
    /// <c>Response.Body</c> não-seekável (escrita para frente; a xref é emitida no fechamento).</para>
    /// </summary>
    public static class ImpressaoStream
    {

        /// <summary>
        /// Monta a cadeia de documentos protocolando, numerando e anexando a página de detalhes,
        /// escrevendo o resultado incrementalmente em <paramref name="destino"/>.
        /// </summary>
        /// <param name="fontes">Documentos de origem, na ordem final, abertos sob demanda.</param>
        /// <param name="destino">Stream de saída (ex.: <c>Response.Body</c> ou um arquivo temporário).</param>
        /// <param name="hashFinal">Hash para o validador da página de detalhes. Nulo omite o código.</param>
        /// <param name="fecharStreamDestino">
        /// Se o iText deve fechar <paramref name="destino"/> ao final. Use <c>false</c> para
        /// <c>Response.Body</c> (o pipeline HTTP gerencia o fechamento) e <c>true</c> para arquivos.
        /// </param>
        public static void ImprimirParaStream(
            IEnumerable<FonteDocumentoImpressao> fontes,
            Stream destino,
            string hashFinal = null,
            bool fecharStreamDestino = true
        )
        {
            if (fontes == null)
                throw new ArgumentNullException(nameof(fontes));
            if (destino == null)
                throw new ArgumentNullException(nameof(destino));

            PdfWriter pdfWriter = new PdfWriter(destino);
            pdfWriter.SetCloseStream(fecharStreamDestino);
            PdfDocument documentoFinal = new PdfDocument(pdfWriter);
            PdfMerger merger = new PdfMerger(documentoFinal);

            List<AssinanteDocumento> todosAssinantes = new List<AssinanteDocumento>();
            int offset = 0;
            bool anexouAlgumDocumento = false;

            try
            {
                foreach (FonteDocumentoImpressao fonte in fontes)
                {
                    if (fonte == null || fonte.AbrirConteudo == null)
                        continue;

                    byte[] conteudo = LerConteudo(fonte.AbrirConteudo);

                    if (conteudo == null || conteudo.Length == 0)
                        continue;

                    // Título precisa ser definido ANTES de Assinantes(): quando o documento não está
                    // encriptado, cada AssinanteDocumento.Documento aponta para esta mesma instância,
                    // e é o Titulo dela que a página de detalhes exibe por assinatura.
                    DocumentoPDF documento = new DocumentoPDF(conteudo);
                    documento.Titulo = fonte.Titulo;

                    List<AssinanteDocumento> assinantes = documento.Assinantes();
                    todosAssinantes.AddRange(assinantes);

                    string data = assinantes.Count > 0 ? assinantes[0].Data : null;

                    DocumentoPDF protocolado = ProtocolarComReparo(documento, conteudo, fonte.Hash, data, assinantes);

                    // Numeração contínua por fonte (primeiroNumero = offset + 1) — equivalente a numerar
                    // a cadeia inteira de uma vez ao final, porém sem reabrir o documento todo.
                    DocumentoPDF numerado = protocolado.Numerar(primeiroNumero: offset + 1);

                    offset += MesclarNoDestino(merger, documentoFinal, numerado.ByteArray);
                    anexouAlgumDocumento = true;
                }

                if (anexouAlgumDocumento)
                {
                    DocumentoPDF paginaDetalhes = ConstruirPaginaDetalhes(hashFinal, todosAssinantes);
                    MesclarNoDestino(merger, documentoFinal, paginaDetalhes.ByteArray);
                }
            }
            finally
            {
                merger.Close();
                documentoFinal.Close();
            }
        }

        /// <summary>
        /// Mescla um PDF (em bytes) no documento de saída e devolve a quantidade de páginas anexadas.
        /// Após a mesclagem, tenta liberar os objetos copiados para reduzir o pico de memória — falha
        /// nessa liberação é apenas uma perda de otimização e não afeta a corretude da saída.
        /// </summary>
        private static int MesclarNoDestino(PdfMerger merger, PdfDocument documentoFinal, byte[] conteudo)
        {
            using (PdfReader leitor = new PdfReader(new MemoryStream(conteudo)))
            using (PdfDocument origem = new PdfDocument(leitor))
            {
                int paginas = origem.GetNumberOfPages();
                merger.Merge(origem, 1, paginas);

                try
                {
                    documentoFinal.FlushCopiedObjects(origem);
                }
                catch
                {
                    // Liberação antecipada é best-effort; ignora e segue.
                }

                return paginas;
            }
        }

        /// <summary>
        /// Lê todo o conteúdo do stream de origem, descartando-o em seguida.
        /// </summary>
        private static byte[] LerConteudo(Func<Stream> abrir)
        {
            using (Stream stream = abrir())
            {
                if (stream == null)
                    return null;

                if (stream is MemoryStream memoryStream)
                    return memoryStream.ToArray();

                using (MemoryStream buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    return buffer.ToArray();
                }
            }
        }

        /// <summary>
        /// Protocola o documento; se falhar (PDF corrompido), tenta reparar a partir do conteúdo
        /// original e protocolar novamente — mesma estratégia da impressão síncrona atual.
        /// </summary>
        private static DocumentoPDF ProtocolarComReparo(
            DocumentoPDF documento,
            byte[] conteudoOriginal,
            string hash,
            string data,
            List<AssinanteDocumento> assinantes)
        {
            try
            {
                return documento.Protocolar(hash, data, assinantes);
            }
            catch
            {
                return new DocumentoPDF(conteudoOriginal).RepararDocumento().Protocolar(hash, data, assinantes);
            }
        }

        /// <summary>
        /// Gera <b>apenas</b> a página de detalhes (assinantes + QR Code de validação), para ser
        /// anexada como última página da cadeia. Reproduz o layout de <c>PaginaFinal</c> de forma
        /// isolada, sem reabrir/mesclar o documento inteiro — por isso não reaproveita o
        /// <c>AdicionarDetalhesAoFinal</c> existente (que exigiria a cadeia completa em memória).
        /// </summary>
        private static DocumentoPDF ConstruirPaginaDetalhes(string hash, List<AssinanteDocumento> assinantes)
        {
            string validadorBase = new DocumentoPDF().ValidadorURL;

            string title = "Estes documentos foram assinados digitalmente pelos seguintes signatários:";
            string signatures = "";
            string lsv = "";

            List<AssinanteDocumento> _signatures = assinantes ?? new List<AssinanteDocumento>();

            foreach (AssinanteDocumento signature in _signatures)
            {
                string docTitle = "";
                if (signature.Documento != null && signature.Documento.Titulo != null)
                {
                    docTitle = $"<h2>{signature.Documento.Titulo}</h2>";
                }

                string date = signature.Data;
                signatures += $@"
                    <div class=""signature"">
                    <div class=""signature-icon"">
                        <svg xmlns=""http://www.w3.org/2000/svg"" height=""24"" viewBox=""0 -960 960 960"" width=""24""><path d=""M563-491q73-54 114-118.5T718-738q0-32-10.5-47T679-800q-47 0-83 79.5T560-541q0 14 .5 26.5T563-491ZM120-120v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80Zm160 0v-80h80v80h-80ZM136-280l-56-56 64-64-64-64 56-56 64 64 64-64 56 56-64 64 64 64-56 56-64-64-64 64Zm482-40q-30 0-55-11.5T520-369q-25 14-51.5 25T414-322l-28-75q28-10 53.5-21.5T489-443q-5-22-7.5-48t-2.5-56q0-144 57-238.5T679-880q52 0 85 38.5T797-734q0 86-54.5 170T591-413q7 7 14.5 10.5T621-399q26 0 60.5-33t62.5-87l73 34q-7 17-11 41t1 42q10-5 23.5-17t27.5-30l63 49q-26 36-60 58t-63 22q-21 0-37.5-12.5T733-371q-28 25-57 38t-58 13Z""/></svg>
                    </div>
                    <div class=""signature-info"">
                        {docTitle}
                        <h1>{signature.Nome}</h1>
                        <h2>{signature.Email}</h2>
                        <h4>Assinado no dia {date}</h4>
                    </div>
                </div>";
            }

            string validadorUrl = validadorBase;
            string hasHash = ".";

            if (hash != null)
            {
                validadorUrl = $"{validadorBase}/{hash}";
                hasHash = $" e informe o código <b>{hash}</b>.";
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(validadorUrl, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeString = qrCode.GetGraphic(4);

            string html = $@"
                <style>
                    *{{
                        font-family:'Aptos';
                        margin:0;
                        padding:0;
                        box-sizing:border-box;
                    }}
                    @page{{
                        margin-left:2cm;
                        margin-right:2cm;
                        margin-top:1.5cm;
                        margin-bottom:2cm;
                    }}
                    html,body{{
                        height:100%;
                    }}
                    body{{
                        display:flex;
                        flex-direction:column;
                    }}
                    #header{{
                        display:block;
                    }}
                    #header img{{
                        width:8cm;
                    }}
                    #main{{
                        display:block;
                        flex:1;
                    }}
                    #footer{{
                        position:absolute;
                        width:17cm;
                        left:-1cm;
                        bottom:-1.5cm;
                        display:flex;
                        align-items:flex-end;
                    }}
                    #qrcode{{
                        height:2cm;
                        margin-right:0.1cm;
                    }}
                    #info{{
                        display:flex;
                        align-items:flex-end;
                        font-size:0.3cm;
                        padding-bottom:0.15cm;
                    }}
                    .title{{
                        font-size:0.45cm;
                        text-transform:uppercase;
                        margin-bottom:1cm;
                        margin-top:0.5cm;
                    }}
                    .signatures{{
                        width:17cm;
                        display:flex;
                        flex-wrap:wrap;
                        xflex-direction:column;
                    }}
                    .signature{{
                        width:8cm;
                        display:flex;
                        margin-bottom:0.35cm;
                    }}
                    .signature-icon{{
                        margin-right:0.3cm;
                    }}
                    .signature-info{{
                        flex:1;
                    }}
                    .signature h1{{
                        font-size:0.4cm;
                    }}
                    .signature h2{{
                        font-size:0.3cm;
                        font-weight:400;
                        padding-bottom:0.1cm;
                    }}
                    .signature h3{{
                        font-size:0.25cm;
                        font-weight:400;
                    }}
                    .signature h4{{
                        font-size:0.28cm;
                    }}
                    a{{
                        text-decoration:none;
                        color:#000;
                    }}
                </style>
                <div id=""header"">
                    <img src=""https://sistemas.camarapiracicaba.sp.gov.br/arquivos/imagens/horizontal.png"" />
                </div>
                <div id=""main"">
                    <h1 class=""title"">{title}</h1>
                    <div class=""signatures"">{signatures}</div>
                    <div class=""signatures"">{lsv}</div>
                </div>
                <div id=""footer"">
                    <img id=""qrcode"" src=""data:image/png;base64,{qrCodeString}"" />
                    <div id=""info"">
                        <p>
                            Se você deseja verificar a autenticidade deste documento, use o QR Code ao lado,<br />
                            ou acesse <b><a href=""{validadorBase}"">{validadorBase}</a></b>{hasHash}
                        </p>
                    </div>
                </div>
            ";

            return html.ConverterParaPdf();
        }

    }

}
