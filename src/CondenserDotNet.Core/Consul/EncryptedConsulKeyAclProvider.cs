using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace CondenserDotNet.Core.Consul
{
    public class EncryptedConsulKeyAclProvider : IConsulAclProvider
    {
        private readonly string _aclToken;

        public EncryptedConsulKeyAclProvider(string encryptedKeyName, X509Certificate2 encryptionCertifcate)
        {
            if (!encryptionCertifcate.HasPrivateKey) throw new ArgumentException("Certificate needs to have the private key to decrypt");
            if (encryptedKeyName.StartsWith("/")) encryptedKeyName = encryptedKeyName.Substring(1);
            using (var httpClient = HttpUtils.CreateClient(null))
            {
                var keyValues = httpClient.GetStringAsync($"/v1/key/{encryptedKeyName}").Result;
                var keys = JsonConvert.DeserializeObject<KeyValue[]>(keyValues);
                if (keys.Length != 1) throw new ArgumentException($"Should only be a single key returned from query but had {keys.Length}");
                var decryptedValue = encryptionCertifcate.GetRSAPrivateKey().DecryptValue(Convert.FromBase64String(keys[0].Value));
                _aclToken = Encoding.UTF8.GetString(decryptedValue);
            }
        }

        public string GetAclToken() => _aclToken;
    }
}
