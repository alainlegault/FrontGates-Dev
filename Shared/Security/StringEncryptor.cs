using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Security
{
	/// <summary>
	/// Utility class for symmetric encryption and decryption of string messages.
	/// </summary>
	public static class StringEncryptor
	{
		private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

		//Preconfigured Encryption Parameters
		private static readonly int BlockBitSize = 128;
		private static readonly int KeyBitSize = 256;

		//Preconfigured Password Key Derivation Parameters
		private static readonly int SaltBitSize = 64;
		private static readonly int Iterations = 10000;
		private static readonly int MinPasswordLength = 12;

		/// <summary>
		/// Default secret password that can be used for encryption and decryption.
		/// </summary>
		public const string DefaultPassword = "a381@G2&@r@S&cr&tK&y";

		internal static string Key = "";
		internal static string EncryptionKey
		{
			get
			{
				if (string.IsNullOrEmpty(Key))
					Key = DefaultPassword;

				return Key;
			}
		}
		public static string Encrypt(this string clearText)
		{
			var rand = new Random();
			var clearBytes = Encoding.Unicode.GetBytes(clearText);

			using (var encryptor = Aes.Create())
			{
				if (encryptor == null)
					return string.Empty;

				var iv = new byte[15];

				rand.NextBytes(iv);

				var pdb = new Rfc2898DeriveBytes(EncryptionKey, iv);

				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);

				using (var ms = new MemoryStream())
				{
					using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(clearBytes, 0, clearBytes.Length);
						cs.Close();
					}

					clearText = Convert.ToBase64String(iv) + Convert.ToBase64String(ms.ToArray());
				}
			}

			return clearText;
		}

		public static string Decrypt(this string cipherText)
		{
			var iv = Convert.FromBase64String(cipherText.Substring(0, 20));

			cipherText = cipherText.Substring(20).Replace(" ", "+");

			var cipherBytes = Convert.FromBase64String(cipherText);

			using (var encryptor = Aes.Create())
			{
				if (encryptor == null)
					return string.Empty;

				var pdb = new Rfc2898DeriveBytes(EncryptionKey, iv);

				encryptor.Key = pdb.GetBytes(32);
				encryptor.IV = pdb.GetBytes(16);

				using (var ms = new MemoryStream())
				{
					using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(cipherBytes, 0, cipherBytes.Length);
						cs.Close();
					}

					cipherText = Encoding.Unicode.GetString(ms.ToArray());
				}
			}

			return cipherText;
		}

		/// <summary>
		/// Simple encryption then authentication of a UTF8 message
		/// using Keys derived from the default password
		/// </summary>
		/// <param name="secretMesage">The secret mesage.</param>
		/// <returns>
		/// An encrypted message.
		/// </returns>
		public static string EncryptWithPassword(string secretMesage)
		{
			return EncryptWithPassword(secretMesage, DefaultPassword);
		}


		/// <summary>
		/// Simple encryption then authentication of a UTF8 message
		/// using Keys derived from a Password
		/// </summary>
		/// <param name="secretMesage">The secret mesage.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		/// An encrypted message.
		/// </returns>
		public static string EncryptWithPassword(string secretMesage, string password)
		{
			var nonSecretPayload = new byte[] { };

			//User Error Checks
			if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
				throw new ArgumentException($"Must have a password of at least {MinPasswordLength} characters!", "password");

			if (string.IsNullOrEmpty(secretMesage))
				throw new ArgumentException("Secret Message Required!", nameof(secretMesage));

			var payload = new byte[((SaltBitSize / 8) * 2) + nonSecretPayload.Length];

			Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
			var payloadIndex = nonSecretPayload.Length;

			byte[] cryptKey;
			byte[] authKey;

			//Use Random Salt to prevent pre-generated weak password attacks.
			using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
			{
				var salt = generator.Salt;

				//Generate Keys
				cryptKey = generator.GetBytes(KeyBitSize / 8);

				//Create Non Secret Payload
				Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
				payloadIndex += salt.Length;
			}

			//Deriving separate key, might be less efficient than using HKDF, 
			//but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
			using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
			{
				var salt = generator.Salt;

				//Generate Keys
				authKey = generator.GetBytes(KeyBitSize / 8);

				//Create Rest of Non Secret Payload
				Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
			}

			return Encrypt(secretMesage, cryptKey, authKey, payload);
		}

		/// <summary>
		/// Simple authentication (HMAC) and then decryption (AES) of a UTF8 Message
		/// using keys derived from a password.
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		/// The drecrypted message.
		/// </returns>
		/// <exception cref="System.ArgumentException">password</exception>
		/// <remarks>
		/// Significantly less secure than using random binary keys.
		/// </remarks>
		public static string DecryptWithPassword(string secretMesage)
		{
			return DecryptWithPassword(secretMesage, DefaultPassword);
		}

		/// <summary>
		/// Simple authentication (HMAC) and then decryption (AES) of a UTF8 Message
		/// using keys derived from a password.
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message.</param>
		/// <param name="password">The password.</param>
		/// <returns>
		/// The drecrypted message.
		/// </returns>
		/// <exception cref="System.ArgumentException">password</exception>
		/// <remarks>
		/// Significantly less secure than using random binary keys.
		/// </remarks>
		public static string DecryptWithPassword(string encryptedMessage, string password)
		{
			const int nonSecretPayloadLength = 0;

			//User Error Checks
			if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
				throw new ArgumentException($"Must have a password of at least {MinPasswordLength} characters!", nameof(password));

			if (string.IsNullOrWhiteSpace(encryptedMessage))
				throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

			var cryptSalt = new byte[SaltBitSize / 8];
			var authSalt = new byte[SaltBitSize / 8];

			var message = Convert.FromBase64String(encryptedMessage);

			//Grab Salt from Non-Secret Payload
			Array.Copy(message, nonSecretPayloadLength, cryptSalt, 0, cryptSalt.Length);
			Array.Copy(message, nonSecretPayloadLength + cryptSalt.Length, authSalt, 0, authSalt.Length);

			byte[] cryptKey;
			byte[] authKey;

			//Generate crypt key
			using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, Iterations))
				cryptKey = generator.GetBytes(KeyBitSize / 8);

			//Generate auth key
			using (var generator = new Rfc2898DeriveBytes(password, authSalt, Iterations))
				authKey = generator.GetBytes(KeyBitSize / 8);

			return Decrypt(encryptedMessage, cryptKey, authKey, cryptSalt.Length + authSalt.Length + nonSecretPayloadLength);
		}


		/// <summary>
		/// Helper that generates a random key on each call.
		/// </summary>
		/// <returns></returns>
		private static byte[] NewKey()
		{
			var key = new byte[KeyBitSize / 8];

			Random.GetBytes(key);

			return key;
		}

		/// <summary>
		/// Simple Encryption(AES) then Authentication (HMAC) for a UTF8 Message.
		/// </summary>
		/// <param name="secretMessage">The secret message.</param>
		/// <param name="cryptKey">The crypt key.</param>
		/// <param name="authKey">The auth key.</param>
		/// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
		/// <returns>Encrypted Message</returns>
		/// <remarks>
		/// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
		/// </remarks>
		private static string Encrypt(string secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
		{
			//User Error Checks
			if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
				throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(cryptKey));

			if (authKey == null || authKey.Length != KeyBitSize / 8)
				throw new ArgumentException($"Key needs to be {KeyBitSize} bit!", nameof(authKey));

			if (string.IsNullOrEmpty(secretMessage))
				throw new ArgumentException("Secret Message Required!", nameof(secretMessage));

			//non-secret payload optional
			nonSecretPayload = nonSecretPayload ?? new byte[] { };

			byte[] cipherText;
			byte[] iv;

			using (var aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
			{
				//Use random IV
				aes.GenerateIV();
				iv = aes.IV;

				using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
				using (var cipherStream = new MemoryStream())
				{
					using (var tCryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
					using (var tBinaryWriter = new BinaryWriter(tCryptoStream))
						//Encrypt Data
						tBinaryWriter.Write(Encoding.UTF8.GetBytes(secretMessage));

					cipherText = cipherStream.ToArray();
				}
			}

			//Assemble encrypted message and add authentication
			using (var hmac = new HMACSHA256(authKey))
			using (var encryptedStream = new MemoryStream())
			{
				using (var binaryWriter = new BinaryWriter(encryptedStream))
				{
					//Prepend non-secret payload if any
					binaryWriter.Write(nonSecretPayload);
					//Prepend IV
					binaryWriter.Write(iv);
					//Write Ciphertext
					binaryWriter.Write(cipherText);
					binaryWriter.Flush();

					//Authenticate all data
					var tag = hmac.ComputeHash(encryptedStream.ToArray());
					//Postpend tag
					binaryWriter.Write(tag);
				}

				return Convert.ToBase64String(encryptedStream.ToArray());
			}

		}

		/// <summary>
		/// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message.</param>
		/// <param name="cryptKey">The crypt key.</param>
		/// <param name="authKey">The auth key.</param>
		/// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
		/// <returns>Decrypted Message</returns>
		private static string Decrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
		{
			//Basic Usage Error Checks
			if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
				throw new ArgumentException($"CryptKey needs to be {KeyBitSize} bit!", nameof(cryptKey));

			if (authKey == null || authKey.Length != KeyBitSize / 8)
				throw new ArgumentException($"AuthKey needs to be {KeyBitSize} bit!", nameof(authKey));

			if (string.IsNullOrWhiteSpace(encryptedMessage))
				throw new ArgumentException("Encrypted Message Required!", nameof(encryptedMessage));

			var message = Convert.FromBase64String(encryptedMessage);

			using (var hmac = new HMACSHA256(authKey))
			{
				var sentTag = new byte[hmac.HashSize / 8];
				//Calculate Tag
				var calcTag = hmac.ComputeHash(message, 0, message.Length - sentTag.Length);
				var ivLength = (BlockBitSize / 8);

				//if message length is to small just return null
				if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
					return null;

				//Grab Sent Tag
				Array.Copy(message, message.Length - sentTag.Length, sentTag, 0, sentTag.Length);

				//Compare Tag with constant time comparison
				var compare = 0;

				for (var i = 0; i < sentTag.Length; i++)
					compare |= sentTag[i] ^ calcTag[i];

				//if message doesn't authenticate return null
				if (compare != 0)
					return null;

				using (var aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 })
				{
					//Grab IV from message
					var iv = new byte[ivLength];
					Array.Copy(message, nonSecretPayloadLength, iv, 0, iv.Length);

					using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
					using (var plainTextStream = new MemoryStream())
					{
						using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
						using (var binaryWriter = new BinaryWriter(decrypterStream))
							//Decrypt Cipher Text from Message
							binaryWriter.Write(message, nonSecretPayloadLength + iv.Length, message.Length - nonSecretPayloadLength - iv.Length - sentTag.Length);

						//Return Plain Text
						return Encoding.UTF8.GetString(plainTextStream.ToArray());
					}
				}
			}
		}
	}
}
