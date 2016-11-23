using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net;
using kbsrserver.Models;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Modes;
using System.Text;

namespace kbsrserver.Helpers
{
    public static class RequestHelper
    {
        public static HttpResponseMessage CreateCustomErrorResponse(this HttpRequestMessage request, HttpStatusCode code, string error)
        {
            return request.CreateResponse(code, new ErrorResponse { Code = code, Message = error });
        }

        public static string GetImei(this HttpRequestMessage request)
        {
            IEnumerable<string> values;
            return request.Headers.TryGetValues("Imei", out values) ? values.FirstOrDefault() : null;
        }

        public static string GetSignature(this HttpRequestMessage request)
        {
            IEnumerable<string> values;
            return request.Headers.TryGetValues("Signature", out values) ? values.FirstOrDefault() : null;
        }
    }

    public static class BouncyCastleHelper
    {
        public static string ToHexString(this byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        public static byte[] FromHexString(this string hex)
        {
            if (hex.Length % 2 == 1)
                hex = '0' + hex;
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static RsaKeyParameters GetPublicKey(string publicKey)
        {
            var inputStream = new Asn1InputStream(publicKey.FromHexString());
            var obj = inputStream.ReadObject();
            var seq = (Asn1Sequence)obj;
            var pkInfo = SubjectPublicKeyInfo.GetInstance(seq);
            var pubk = RsaPublicKeyStructure.GetInstance(pkInfo.GetPublicKey());
            return new RsaKeyParameters(false, pubk.Modulus, pubk.PublicExponent);
        }

        public static bool Verify(string data, string expectedSignature, string publicKey)
        {
            var key = GetPublicKey(publicKey);
            var signer = SignerUtilities.GetSigner("SHA1withRSA");
            signer.Init(false, key);
            var dataInBytes = Encoding.UTF8.GetBytes(data);
            var expectedSignatureInBytes = expectedSignature.FromHexString();
            signer.BlockUpdate(dataInBytes, 0, dataInBytes.Length);
            return signer.VerifySignature(expectedSignatureInBytes);
        }

        public static string GenerateSerpentKey()
        {
            var ckg = new CipherKeyGenerator();
            ckg.Init(new KeyGenerationParameters(new SecureRandom(), 256));
            return ckg.GenerateKey().ToHexString();
        }

        public static string EncryptSessionKey(string sessionKey, string publicKey)
        {
            var c = new Pkcs1Encoding(new RsaEngine());
            c.Init(true, GetPublicKey(publicKey));
            var text = sessionKey.FromHexString();
            var l = c.ProcessBlock(text, 0, text.Length);
            return l.ToHexString();
        }

        public static string EncryptNote(string note, string sessionKey)
        {
            var c = new PaddedBufferedBlockCipher(new CfbBlockCipher(new SerpentEngine(), 128), new Pkcs7Padding());

            var text = Encoding.UTF8.GetBytes(note);
            var key = sessionKey.FromHexString();
            var iv = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            c.Init(true, new ParametersWithIV(new KeyParameter(key), iv));

            var ct = new byte[c.GetOutputSize(text.Length)];
            int l = c.ProcessBytes(text, 0, text.Length, ct, 0);
            c.DoFinal(ct, l);
            return ct.ToHexString();
        }
    }
}