using Dojo.Starknet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class GrugsLairAddressUtils
{
    /// <summary>
    /// Returns a given address in the format 0x0000...999
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string ShortenAddress(string address)
    {
        return address.Substring(0, 6) + "..." + address.Substring(address.Length - 4);
    }

    /// <summary>
    /// Checks if the given hash is a valid transaction hash
    /// </summary>
    /// <param name="hash">hash to check the validity for</param>
    ///  <param name="hashesToAvoid">If the hash given is equal to any of the hashes in this list it will return false</param>
    /// <returns></returns>
    public static bool IsValidTransactionHash(string hash, List<string> hashesToAvoid)
    {
        if (hashesToAvoid.Contains(hash))
        {
            return false;
        }

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
    /// Add the necessary 0s to the hashes at the start due to queries sometimes taking them off and therefore not being able to find the correct hash
    /// </summary>
    /// <param name="inputHash"></param>
    /// <returns></returns>
    public static string Address0sFix(string inputHash)
    {
        const int requiredLength = 66;

        if (!inputHash.StartsWith("0x"))
        {
            inputHash = "0x" + inputHash;
        }

        int missingZeros = requiredLength - inputHash.Length;

        if (missingZeros > 0)
        {
            inputHash = "0x" + new string('0', missingZeros) + inputHash.Substring(2);
        }

        return inputHash;
    }

    /// <summary>
    /// Given an Address this function will return all the Stark Ids attached to that wallet
    /// </summary>
    /// <param name="address">address of the user to look up</param>
    /// <returns>List of all the Ids of the user</returns>
    public static async Task<List<string>> GetStarknetIds(string address)
    {
        var url = $"https://api.starknet.id/addr_to_full_ids?addr={address}";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        var asyncOperation = request.SendWebRequest();

        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new Exception($"Error: {request.responseCode}");
        }

        var responseBody = request.downloadHandler.text;

        var json = JObject.Parse(responseBody);
        var domains = new List<string>();

        foreach (var id in json["full_ids"])
        {
            if (id["domain"] != null)
            {
                var domain = id["domain"].ToString();
                domains.Add(domain);
            }
        }

        return domains;
    }

    //TODO: add comments and test properly HERE
    public static class ShortString
    {
        //returns felt252 with word
        public static FieldElement EncodeShortString(string input)
        {
            if (input.Length > 31)
                throw new ArgumentException("ShortString can only contain up to 31 ASCII characters.");

            byte[] bytes = Encoding.ASCII.GetBytes(input);
            BigInteger bigInt = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
            return new FieldElement(bigInt);
        }


        //give it a felt252 and it will return the string
        public static string DecodeShortString(FieldElement felt)
        {
            Span<byte> span = felt.Inner.data;

            string spanHex = BitConverter.ToString(span.ToArray()).Replace("-", " ");

            StringBuilder spanAscii = new StringBuilder(span.Length);
            for (int i = 0; i < span.Length; i++)
            {
                spanAscii.Append(span[i] >= 32 && span[i] <= 126 ? (char)span[i] : '.');
            }

            // Find the start and end of the actual string content
            int start = 0;
            int end = span.Length - 1;

            while (start < span.Length && span[start] == 0) start++;
            while (end >= 0 && span[end] == 0) end--;

            if (start > end)
            {
                return string.Empty;
            }

            string result = Encoding.ASCII.GetString(span.Slice(start, end - start + 1).ToArray()).TrimEnd('\0');

            return result;
        }
    }

}
