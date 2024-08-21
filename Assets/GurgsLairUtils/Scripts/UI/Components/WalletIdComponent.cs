using Dojo.Starknet;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class WalletIdComponent : MonoBehaviour
{
    private TMP_Text walletAddressText;

    private string shortendWalletAddress;
    private List<string> starknetIds;

    private int currentStarknetIdIndex = 0;
    private bool showingAddress = true;
    private Coroutine noStarkIdCoroutine;

    public async Task InitiateComponent(Account account)
    {
        walletAddressText = GetComponent<TMP_Text>();

        if (walletAddressText == null)
        {
            Debug.LogError("TMP_Text component not found!");
            return;
        }

        if (account == null)
        {
            walletAddressText.text = "Connect";
            return;
        }

        shortendWalletAddress = GrugsLairAddressUtils.ShortenAddress(account.Address.Hex());
        walletAddressText.text = shortendWalletAddress;
            
        starknetIds = await GrugsLairAddressUtils.GetStarknetIds(account.Address.Hex());

        if (starknetIds.Count > 0)
        {
            walletAddressText.text = starknetIds[0];
        }
    }

    public void CycleThroughId()
    {
        if (starknetIds != null && starknetIds.Count > 0)
        {
            currentStarknetIdIndex = (currentStarknetIdIndex + 1) % starknetIds.Count;
            walletAddressText.text = starknetIds[currentStarknetIdIndex];
        }
    }

    public void CycleBetweenAddressAndID()
    {
        if (starknetIds == null || starknetIds.Count == 0)
        {
            if (noStarkIdCoroutine == null)
            {
                noStarkIdCoroutine = StartCoroutine(ShowNoStarkIdMessage());
            }
        }
        else
        {
            if (noStarkIdCoroutine != null)
            {
                StopCoroutine(noStarkIdCoroutine);
                noStarkIdCoroutine = null;
            }

            showingAddress = !showingAddress;
            walletAddressText.text = showingAddress ? shortendWalletAddress : starknetIds[currentStarknetIdIndex];
        }
    }

    private IEnumerator ShowNoStarkIdMessage()
    {
        walletAddressText.text = "No Stark Id";
        yield return new WaitForSeconds(3);
        walletAddressText.text = shortendWalletAddress;
        noStarkIdCoroutine = null;
    }
}
