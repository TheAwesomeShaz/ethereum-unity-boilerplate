using Moralis.Platform.Objects;
using Moralis.Web3Api.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
using UnityEngine.Networking;

public class TokenListController : MonoBehaviour
{

    /// <summary>
    /// Username text
    /// </summary>
    public TMPro.TMP_Text nameText;

    /// <summary>
    /// Prefab of the item to draw the token to and show in the list.
    /// </summary>
    public GameObject ListItemPrefab;

    /// <summary>
    /// Grid layout to hold the NFT Images Button
    /// </summary>
    public Transform TokenListTransform;

    /// <summary>
    /// Chain ID to fetch tokens from. Might be better to make this
    /// a drop down that is selectable at run time.
    /// </summary>
    public int ChainId;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BuildTokenList());
    }

        IEnumerator BuildTokenList()
        {
        // Get user object and display user name
        MoralisUser user = MoralisInterface.GetUser();

        if (user != null)
        {
            nameText.text = user.username;

            string addr = user.authData["moralisEth"]["id"].ToString();

            List<Erc20TokenBalance> tokens =
                MoralisInterface.GetClient().Web3Api.Account.GetTokenBalances(addr.ToLower(),
                                          (ChainList)ChainId);

            foreach (Erc20TokenBalance token in tokens)
            {
                // Ignor entry without symbol
                if (string.IsNullOrWhiteSpace(token.Symbol))
                {
                    continue;
                }

                // Create and add an Token button to the display list. 
                var tokenObj = Instantiate(ListItemPrefab, TokenListTransform);
                var tokenSymbol = tokenObj.GetFirstChildComponentByName<Text>("TokenSymbolText", false);
                var tokenBalanace = tokenObj.GetFirstChildComponentByName<Text>("TokenCountText", false);
                var tokenImage = tokenObj.GetFirstChildComponentByName<Image>("TokenThumbNail", false);
                var tokenButton = tokenObj.GetComponent<Button>();
                var rectTransform = tokenObj.GetComponent<RectTransform>();

                var parentTransform = TokenListTransform.GetComponent<RectTransform>();


                float parentWidth = parentTransform.rect.width;
                
                rectTransform.sizeDelta = new Vector2(parentWidth, 64);

                if (parentWidth > 800.0f)
                {
                    tokenSymbol.fontSize = 36;
                    tokenBalanace.fontSize = 24;

                    var symRectTran = tokenSymbol.GetComponent<RectTransform>();
                    var balRectTran = tokenBalanace.GetComponent<RectTransform>();

                    symRectTran.sizeDelta = new Vector2(105, symRectTran.rect.height);
                    balRectTran.sizeDelta = new Vector2(350, balRectTran.rect.height);

                    symRectTran.position = new Vector3(symRectTran.position.x + 30, symRectTran.position.y);
                    balRectTran.position = new Vector3(balRectTran.position.x - 100, balRectTran.position.y);
                }
                else if (parentWidth > 600.0f)
                {
                    tokenSymbol.fontSize = 28;
                    tokenBalanace.fontSize = 18;

                    var symRectTran = tokenSymbol.GetComponent<RectTransform>();
                    var balRectTran = tokenBalanace.GetComponent<RectTransform>();

                    symRectTran.sizeDelta = new Vector2(75, symRectTran.rect.height);
                    balRectTran.sizeDelta = new Vector2(275, balRectTran.rect.height);

                    symRectTran.position = new Vector3(symRectTran.position.x + 15, symRectTran.position.y);
                    balRectTran.position = new Vector3(balRectTran.position.x - 50, balRectTran.position.y);
                }

                tokenSymbol.text = token.Symbol;
                tokenBalanace.text = token.Balance;

                // When button clicked display theCoingecko page for that token.
                tokenButton.onClick.AddListener(delegate
                {
                    // Display token CoinGecko page on click.
                    Application.OpenURL($"https://coinmarketcap.com/currencies/{token.Name}");
                });

                using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(token.Thumbnail))
                {
                    yield return imageRequest.SendWebRequest();

                    if (imageRequest.isNetworkError)
                    {
                        Debug.Log("Error Getting Nft Image: " + imageRequest.error);
                    }
                    else
                    {
                        Texture2D tokenTexture = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;

                        var sprite = Sprite.Create(tokenTexture,
                                    new Rect(0.0f, 0.0f, tokenTexture.width, tokenTexture.height),
                                    new Vector2(0.75f, 0.75f), 100.0f);

                        tokenImage.sprite = sprite;
                    }
                }

                
            }
        }
    }

}