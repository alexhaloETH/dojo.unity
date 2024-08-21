using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class UtilsGrugsLair 
{
    #region Address Actions

    /// <summary>
    /// Returns a given address in the format 0x0000...999
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string ShortenAddress(string address)
    {
        if (address == null || address.Length < 9)
        {
            throw new ArgumentException("Address must be at least 9 characters long.");
        }

        return address.Substring(0, 6) + "..." + address.Substring(address.Length - 4);
    }

    /// <summary>
    /// checks i the transaction given is a valid hash 
    /// </summary>
    /// <param name="inputHash"></param>
    /// <returns></returns>
    public static bool IsValidTransactionHash(string hash)
    {
        // Check if the string length is exactly 64 characters
        if (hash.Length != 64)
        {
            return false;
        }

        // Regular expression to check if the string contains only hexadecimal characters (0-9, a-f, A-f)
        Regex hexRegex = new Regex("^[0-9a-fA-F]+$");

        // Return true if the string matches the regex pattern, otherwise false
        return hexRegex.IsMatch(hash);
    }

    /// <summary>
    /// Given an Address this function will return all the Stark Ids attached to that wallet
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    static async Task<List<string>> GetStarknetIds(string address)
    {
        var url = $"https://api.starknet.id/addr_to_full_ids?addr={address}";

        using var client = new HttpClient();
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error: {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseBody);
        var domains = new List<string>();

        foreach (var id in json["full_ids"])
            if (id["domain"] != null)
                domains.Add(id["domain"].ToString());
        return domains;
    }

    #endregion

    #region Mathematical Operations
    /// <summary>
    /// What FieldElement("0"); would equal to
    /// </summary>
    public static string emptyFieldElement = "0x0000000000000000000000000000000000000000000000000000000000000000";

    /// <summary>
    /// Multiplies a given number by 10 raised to the specified decimal length and returns it as a BigInteger.
    /// </summary>
    /// <param name="number">The number to be multiplied.</param>
    /// <param name="decimalLength">The power of 10 to multiply the number by, default is 18.</param>
    /// <returns>A BigInteger representing the multiplied value.</returns>
    public static BigInteger NumberToBigint(long number, int decimalLength = 18)
    {
        BigInteger result = new BigInteger(number) * BigInteger.Pow(10, decimalLength);
        return result;
    }

    /// <summary>
    /// Divides a BigInteger by 10^18 and returns the result as a decimal.
    /// </summary>
    /// <param name="bigInt">The BigInteger to be divided.</param>
    /// <returns>A decimal representing the divided value.</returns>
    public static decimal DivideByTenPowerEighteen(BigInteger bigInt)
    {
        try
        {
            decimal result = (decimal)(bigInt / BigInteger.Pow(10, 18));
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
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

    #endregion


}
