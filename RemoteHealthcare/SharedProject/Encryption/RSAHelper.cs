using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SharedProject
{
    public class RsaHelper
    {
        /// <summary>
        /// It takes a message, an RSA key, and a boolean value that determines whether or not to use OAEP padding. It then
        /// returns the encrypted message
        /// </summary>
        /// <param name="message">The message to be encrypted.</param>
        /// <param name="RSAParameters">This is a struct that contains the RSA key.</param>
        /// <param name="doOAEPPadding">true to perform direct System.Security.Cryptography.RSA encryption using OAEP
        /// padding (only available on a computer running Microsoft Windows XP or later); otherwise, false to use PKCS#1
        /// v1.5 padding.</param>
        /// <returns>
        /// The encrypted message.
        /// </returns>
        public static byte[]? EncryptMessage(byte[] message, RSAParameters rsaKey, bool doOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaKey);
                    encryptedData = rsa.Encrypt(message, doOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine($"Error while trying to encrypt message: \n {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// It takes in a byte array of encrypted data, an RSA key, and a boolean value that determines whether or not to
        /// use OAEP padding. It then returns a byte array of decrypted data
        /// </summary>
        /// <param name="encryptedMessage">The encrypted message to decrypt.</param>
        /// <param name="RSAParameters">This is a struct that contains the RSA key.</param>
        /// <param name="doOAEPPadding">true if Optimal Asymmetric Encryption Padding (OAEP) should be used (only available
        /// on a computer running Microsoft Windows XP or later); otherwise, false to use PKCS#1 v1.5 padding.</param>
        /// <returns>
        /// The decrypted message.
        /// </returns>
        public static byte[]? DecryptMessage(byte[] encryptedMessage, RSAParameters rsaKey, bool doOAEPPadding)
        {
            try
            {
                byte[] decryptedMessage;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaKey);
                    decryptedMessage = rsa.Decrypt(encryptedMessage, doOAEPPadding);
                }
                
                return decryptedMessage;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine($"Error while trying to decrypt message: \n {e.Message}");
                Console.WriteLine($"\n {e.StackTrace}");
                return null;
            }
        }
    }
}