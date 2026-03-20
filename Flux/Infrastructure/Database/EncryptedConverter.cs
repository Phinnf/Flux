using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Flux.Infrastructure.Security;

namespace Flux.Infrastructure.Database
{
    public class EncryptedConverter : ValueConverter<string, string>
    {
        public EncryptedConverter(string encryptionKey, ConverterMappingHints? mappingHints = null)
            : base(
                v => AesGcmEncryptor.Encrypt(v, encryptionKey),
                v => AesGcmEncryptor.Decrypt(v, encryptionKey),
                mappingHints)
        {
        }
    }
}
