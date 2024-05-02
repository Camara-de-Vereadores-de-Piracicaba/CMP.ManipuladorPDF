using System;
using System.Collections.Generic;

namespace CMP.ManipuladorPDF
{
    public static partial class ExtensionMethods
    {

        public static DocumentoPDF Protocolar(this DocumentoPDF documento, string protocolo, DateTime data, string hash = null, PosicaoCarimbo posicao = PosicaoCarimbo.DIREITA, bool qrcode = true)
        {
            string _data = data.ToString("G");
            List<string> linhas = new List<string>();

            string verificar = $"Para verificar a validade deste documento, use o QR Code abaixo ou acesse {documento.ValidadorURL}";
            string link = $"{documento.ValidadorURL}";

            if(hash!= null) 
            {
                verificar += $" e use o código {hash}";
                link += $"/{hash}";
            }

            verificar += ".";

            linhas.Add(verificar);
            linhas.Add($"Documento assinado digitalmente e protocolado na Câmara Municipal de Piracicaba em {_data}, sob o nº {protocolo}.");
            return Carimbo(documento, linhas, posicao, link);
        }

    }

}