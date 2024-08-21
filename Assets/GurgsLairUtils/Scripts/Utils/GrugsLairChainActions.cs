using Dojo.Starknet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class GrugsLairChainActions
{
    /// <summary>
    /// Fetch the average block time of N amount of blocks for Starknet Mainnet
    /// 
    /// TODO: See if its possbile to allow mora than Mainnet
    /// </summary>
    /// <param name="callback">Funciton that takes the float time in seconds</param>
    /// <param name="numberOfBlocksToAvarage">Amount of blocks to look back to</param>
    /// <returns></returns>
    public static IEnumerator FetchAverageBlockTimeCoroutine(Action<float> callback, int numberOfBlocksToAvarage = 5)
    {
        string url = "https://alpha-mainnet.starknet.io/feeder_gateway/get_block";

        // Fetch the latest block
        UnityWebRequest latestBlockRequest = UnityWebRequest.Get(url);
        yield return latestBlockRequest.SendWebRequest();

        if (latestBlockRequest.result != UnityWebRequest.Result.Success)
        {
            throw new Exception("Error fetching latest block: " + latestBlockRequest.error);
        }

        string latestBlockContent = latestBlockRequest.downloadHandler.text;
        JObject latestBlock = JObject.Parse(latestBlockContent);

        // Get the latest block number
        int latestBlockNumber = (int)latestBlock["block_number"];

        // Initialize variables to calculate the average block time
        int totalTime = 0;

        for (int i = 0; i < numberOfBlocksToAvarage; i++)
        {
            // Fetch the current block
            UnityWebRequest currentBlockRequest = UnityWebRequest.Get($"{url}?blockNumber={latestBlockNumber - i}");
            yield return currentBlockRequest.SendWebRequest();

            if (currentBlockRequest.result != UnityWebRequest.Result.Success)
            {
                throw new Exception("Error fetching current block: " + currentBlockRequest.error);
            }

            string currentBlockContent = currentBlockRequest.downloadHandler.text;
            JObject currentBlock = JObject.Parse(currentBlockContent);

            // Fetch the previous block
            UnityWebRequest previousBlockRequest = UnityWebRequest.Get($"{url}?blockNumber={latestBlockNumber - i - 1}");
            yield return previousBlockRequest.SendWebRequest();

            if (previousBlockRequest.result != UnityWebRequest.Result.Success)
            {
                throw new Exception("Error fetching previous block: " + previousBlockRequest.error);
            }

            string previousBlockContent = previousBlockRequest.downloadHandler.text;
            JObject previousBlock = JObject.Parse(previousBlockContent);

            // Calculate the block time
            int blockTime = (int)currentBlock["timestamp"] - (int)previousBlock["timestamp"];
            totalTime += blockTime;
        }

        // Calculate the average block time
        float averageBlockTime = (float)totalTime / numberOfBlocksToAvarage;

        // Return the result through the callback
        callback(averageBlockTime);
    }

    /// <summary>
    /// Helper to compute the poseidon hash of an array of FieldElements
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    unsafe public static FieldElement PoseidonHash(FieldElement[] array)
    {
        if (array == null || array.Length == 0)
        {
            throw new ArgumentException("Input array cannot be null or empty");
        }

        dojo_bindings.dojo.FieldElement[] dojoArray = new dojo_bindings.dojo.FieldElement[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            dojoArray[i] = array[i].Inner;
        }

        UIntPtr length = (UIntPtr)dojoArray.Length;

        fixed (dojo_bindings.dojo.FieldElement* arrayPtr = dojoArray)
        {
            dojo_bindings.dojo.FieldElement result = dojo_bindings.dojo.poseidon_hash(arrayPtr, length);

            return new FieldElement(result);
        }
    }

    /// <summary>
    /// Given an Address this function will return all the Stark Ids attached to that wallet
    /// </summary>
    /// <param name="address">address of the user to look up</param>
    /// <returns></returns>
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

    /// <summary>
    /// checks if the transaction given is a valid hash 
    /// 
    /// TODO: Pretty sure this doesnt work
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
}
