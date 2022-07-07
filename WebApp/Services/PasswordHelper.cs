﻿using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace WebApp.Services
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Returns the provided <paramref name="bitsOfEntropy"/> that are base64url encoded.
        /// </summary>
        /// <param name="bitsOfEntropy">Must be at least 128.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Base64Url(int bitsOfEntropy = 128)
        {
            if (bitsOfEntropy < 128)
                throw new ArgumentOutOfRangeException(nameof(bitsOfEntropy), bitsOfEntropy, $"The {nameof(bitsOfEntropy)} must be greater than or equal 128.");

            int numberOfBytes = (int)Math.Ceiling(bitsOfEntropy / 8f);
            byte[] bytes = RandomNumberGenerator.GetBytes(numberOfBytes);
            return WebEncoders.Base64UrlEncode(bytes);
        }
    }
}