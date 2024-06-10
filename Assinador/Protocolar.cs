using System;
using System.Collections.Generic;
using System.Linq;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        /// <summary>
        /// Protocola um documento PDF, carimbando a lateral da página
        /// </summary>
        /// <param name="documento">Documento para ser assinado.</param>
        /// <param name="hash">Hash do documento, para verificação</param>
        /// <param name="numero">Número do documento</param>
        /// <param name="posicao">Posição do carimbo. Tipo PosicaoCarimbo.</param>
        /// <param name="qrcode">Link para o QRCode. O QRCode não é apresentado caso setado como 'null'.</param>

        public static DocumentoPDF Protocolar(
            this DocumentoPDF documento, 
            string hash = null, 
            string data = null, 
            List<AssinanteDocumento> assinantes = null, 
            string numero = null, 
            PosicaoCarimbo posicao = PosicaoCarimbo.DIREITA, 
            bool qrcode = true
        )
        {
            data ??= DateTime.Now.ToString("G");
            
            List<string> linhas = new List<string>();

            string verificar = $"Para verificar a validade deste documento, use o QR Code abaixo ou acesse {documento.ValidadorURL}";
            string link = $"{documento.ValidadorURL}";

            if(hash != null) 
            {
                verificar += $" e use o código {hash}";
                link += $"/{hash}";
            }

            verificar += ".";
            linhas.Add(verificar);

            string protocolo = $"Este documento é uma cópia digital e foi assinado digitalmente.";

            if (assinantes != null && assinantes.Any() && assinantes?[0].Nome!="")
            {
                protocolo += $" por";

                AssinanteDocumento ultimo = assinantes.LastOrDefault();

                foreach(AssinanteDocumento assinante in assinantes)
                {
                    if (assinante.Equals(ultimo))
                    {
                        if(assinantes.Count > 1)
                        {
                            protocolo += " e";
                        }

                        protocolo += $" {assinante.Nome}";
                    }
                    else
                    {
                        protocolo += $" {assinante.Nome}, ";
                    }
                }


            }

            protocolo += $" Protocolado na Câmara Municipal de Piracicaba";

            if (data != null)
            {
                protocolo += $" em {data}";
            }

            if(numero != null)
            {
                protocolo += $", sob o nº {numero}";
            }

            protocolo += ".";

            linhas.Add(protocolo);

            return Carimbo(documento, linhas, posicao, link);
        }

    }

}