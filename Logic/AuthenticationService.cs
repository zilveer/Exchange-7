using DataAccess.UnitOfWork;
using Entity;
using Infrastructure;
using OtpNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Util;

namespace Logic
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IReadOnlyContext _readOnlyContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProvider _timeProvider;
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly IRedisCache _redisCache;
        public AuthenticationService(IUnitOfWork unitOfWork, IReadOnlyContext readOnlyContext, IRedisCache redisCache,ITimeProvider timeProvider, byte[] key, byte[] iv)
        {
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _readOnlyContext = readOnlyContext;
            _redisCache = redisCache;
            _key = key;
            _iv = iv;
        }

        public BusinessOperationResult<UserCredentials> Authenticate(string emailOrMobile, string password)
        {
            var userCredential = _unitOfWork.UserCredentialRepository.GetByEmailOrPhone(emailOrMobile);
            if (userCredential != null)
            {
                if (userCredential.LoginUnblockOn <= _timeProvider.GetUtcDateTime()) //unblock after 
                {
                    userCredential.LoginUnblockOn = null;
                    userCredential.LoginFailedAttempt = 0;
                }

                if (userCredential.LoginUnblockOn == null)
                {
                    if (userCredential.PasswordHash == ComputeHash(password, userCredential.Salt))
                    {
                        if (userCredential.LoginFailedStartOn != null || userCredential.LoginUnblockOn != null)
                        {
                            userCredential.LoginFailedStartOn = null;
                            userCredential.LoginFailedAttempt = 0;
                            userCredential.LoginUnblockOn = null;
                        }
                        _unitOfWork.SaveChanges();
                        return new BusinessOperationResult<UserCredentials>()
                        {
                            ErrorCode = 0,
                            ErrorMessage = "Login Successful.",
                            Entity = userCredential
                        };
                    }
                    else
                    {
                        if (userCredential.LoginFailedStartOn == null || userCredential.LoginFailedStartOn < _timeProvider.GetUtcDateTime().AddHours(-24))
                        {
                            userCredential.LoginFailedStartOn = _timeProvider.GetUtcDateTime();
                            userCredential.LoginFailedAttempt = 0;
                        }

                        userCredential.LoginFailedAttempt++;

                        if (userCredential.LoginFailedAttempt == 3)
                        {
                            userCredential.LoginUnblockOn = _timeProvider.GetUtcDateTime().AddHours(24);
                        }
                        _unitOfWork.SaveChanges();
                        return new BusinessOperationResult<UserCredentials>()
                        {
                            ErrorCode = 1003,
                            ErrorMessage = "Invalid email or password."
                        };
                    }
                }
                else
                {
                    return new BusinessOperationResult<UserCredentials>()
                    {
                        ErrorCode = 1002,
                        ErrorMessage = "Your Login is blocked for 24 Hours. You can login after ",
                        Entity = userCredential
                    };
                }
            }
            else
            {
                return new BusinessOperationResult<UserCredentials>()
                {
                    ErrorCode = 1001,
                    ErrorMessage = "Invalid email or password."
                };
            }
        }

        public BusinessOperationResult<UserCredentials> VerifyTotp(int userId, string totp)
        {
            var userCredential = _readOnlyContext.UserCredentialReadOnlyRepository.GetByUserId(userId);
            if (userCredential != null)
            {
                if (userCredential.TwoFactorEnabled && !string.IsNullOrWhiteSpace(userCredential.TwoFactorKey))
                {
                    var encryptedKey = Convert.FromBase64String(userCredential.TwoFactorKey);
                    var key = Decrypt(encryptedKey);
                    Totp totpAlgo = new Totp(key, 30, OtpHashMode.Sha1, 6);
                    if (totpAlgo.VerifyTotp(totp, out long timeStepMatched, new VerificationWindow(2, 2)))
                    {
                        var redisKey = $"TOTP-{userId}-{timeStepMatched}";
                        if (!_redisCache.Exists(redisKey))
                        {
                            _redisCache.StringSet(redisKey, string.Empty, TimeSpan.FromSeconds(30));
                            return new BusinessOperationResult<UserCredentials>()
                            {
                                ErrorCode = 0,
                                Entity = userCredential
                            };
                        }
                        else
                        {
                            return new BusinessOperationResult<UserCredentials>()
                            {
                                ErrorCode = 1004,
                                ErrorMessage = "TOTP already used once. retry after some time."
                            };
                        }
                    }
                    else
                    {
                        return new BusinessOperationResult<UserCredentials>()
                        {
                            ErrorCode = 1003,
                            ErrorMessage = "Invalid TOTP."
                        };
                    }
                }
                else
                {
                    return new BusinessOperationResult<UserCredentials>()
                    {
                        ErrorCode = 1002,
                        ErrorMessage = "Two factor authentication is not enabled."
                    };
                }
            }
            else
            {
                return new BusinessOperationResult<UserCredentials>()
                {
                    ErrorCode = 1001,
                    ErrorMessage = "Invalid UserId."
                };
            }
        }

        private byte[] Decrypt(byte[] cipherText)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            List<byte> bytes = new List<byte>();
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = _key;
                rijAlg.IV = _iv;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] buffer = new byte[1024];
                        var bytesRead = 0;
                        do
                        {
                            bytesRead = csDecrypt.Read(buffer, 0, 1024);
                            bytes.AddRange(buffer.Take(bytesRead));
                        } while (bytesRead > 0);
                    }
                }
            }

            return bytes.ToArray();
        }

        private byte[] Encrypt(byte[] plain)
        {
            if (plain == null || plain.Length <= 0)
                throw new ArgumentNullException("plain");

            byte[] encrypted;
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = _key;
                rijAlg.IV = _iv;

                using (ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV))
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plain, 0, plain.Length);
                            csEncrypt.FlushFinalBlock();
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
            }
            return encrypted;
        }

        private string ComputeHash(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var saltBytes = Convert.FromBase64String(salt);
                var bytes = new byte[passwordBytes.Length + saltBytes.Length];
                Array.Copy(passwordBytes, 0, bytes, 0, passwordBytes.Length);
                Array.Copy(saltBytes, 0, bytes, passwordBytes.Length, saltBytes.Length);
                return Convert.ToBase64String(sha256.ComputeHash(bytes));
            }
        }

        private string GenerateSalt()
        {
            var bytes = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(bytes, 0, 32);
            }
            return Convert.ToBase64String(bytes);
        }

        public bool IsTwoFactorEnabled(int userId)
        {
            return _readOnlyContext.UserCredentialReadOnlyRepository.GetByUserId(userId).TwoFactorEnabled;
        }
    }
}
