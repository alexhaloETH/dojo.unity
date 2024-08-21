using System;
using System.Linq;
using System.Numerics;

public static class GrugsLairMathsUtils
{
    //TODO: felt operations are missing, a felt is just a 252 instead of a 256 bit number, to look into

    public readonly static string EmptyFieldElement = "0x0000000000000000000000000000000000000000000000000000000000000000";

    public static string BigIntToHex(BigInteger bigInt)
    {
        return "0x" + bigInt.ToString("X").ToLower();
    }

    /// <summary>
    /// Multiplies a given number by 10 raised to the specified decimal length and returns it as a BigInteger.
    /// </summary>
    /// <param name="number">The number to be multiplied.</param>
    /// <param name="decimalLength">The power of 10 to multiply the number by, default is 18.</param>
    /// <returns>A BigInteger representing the multiplied value.</returns>
    public static BigInteger NumberToBigint(int number, int decimalLength = 18)
    {
        BigInteger result = new BigInteger(number) * BigInteger.Pow(10, decimalLength);
        return result;
    }

    /// <summary>
    /// Converts a BigInteger to a decimal and rounds the result to a specified number of decimal places.
    /// </summary>
    /// <param name="bigInt">The BigInteger to be converted.</param>
    /// <param name="decimalPlaces">The number of decimal places to round the result to.</param>
    /// <returns>A decimal representing the rounded value.</returns>
    public static decimal BigIntToDecimal(BigInteger bigInt, int decimalPlaces = 2)
    {
        try
        {
            decimal result = (decimal)(bigInt / BigInteger.Pow(10, 18));
            result = Math.Round(result, decimalPlaces);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Splits and returns two u128 from a u256
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static (string firstHalf, string lastHalf) GetFirstAndLastU128(string hex)
    {
        hex = hex.PadLeft(64, '0');

        string firstHalf = "0x" + hex.Substring(0, 32).TrimStart('0');

        string lastHalf = "0x" + hex.Substring(hex.Length - 32).TrimStart('0');

        return (firstHalf, lastHalf);
    }

    /// <summary>
    /// Given a hex less than u256 it will return a hex that is u256
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ConvertTo256BitHex(string input)
    {
        if (input.StartsWith("0x"))
        {
            input = input.Substring(2);
        }

        byte[] bytes = Enumerable.Range(0, input.Length / 2)
                                 .Select(x => Convert.ToByte(input.Substring(x * 2, 2), 16))
                                 .ToArray();

        byte[] paddedBytes = new byte[32];
        Buffer.BlockCopy(bytes, 0, paddedBytes, 32 - bytes.Length, bytes.Length);

        string result = "0x" + BitConverter.ToString(paddedBytes).Replace("-", "").ToLower();

        return result;
    }

    /// <summary>
    /// Converts a hexadecimal string to a BigInteger.
    /// </summary>
    /// <param name="hexString">The hexadecimal string to be converted.</param>
    /// <returns>A BigInteger representing the converted value.</returns>
    public static BigInteger HexToBigInteger(string hexString)
    {
        if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hexString = hexString.Substring(2);
        }

        hexString = hexString.TrimStart('0');
        byte[] bytes = new byte[hexString.Length / 2 + hexString.Length % 2];
        for (int index = 0; index < bytes.Length; index++)
        {
            string byteValue = hexString.Length % 2 != 0 && index == 0
                ? hexString.Substring(0, 1)
                : hexString.Substring(index * 2 - (hexString.Length % 2 == 0 ? 0 : 1), 2);

            bytes[bytes.Length - index - 1] = Convert.ToByte(byteValue, 16);
        }

        return new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
    }
}
