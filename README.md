# Magic Crypto Blazor

Welcome to **Magic Crypto Blazor**! This repository contains two projects within the solution:

- **Crypto.Blazor.Core**
- **Crypto.MauiBlazor.Wallet**

These projects aim to make blockchain API calls easier to utilize within C# and Blazor applications. While most blockchain APIs and libraries use JavaScript, this project integrates these JavaScript libraries to be more accessible in C# Blazor environments. The **Crypto.MauiBlazor.Wallet** project is a secure way for users to track their crypto through open-source, easily changeable code.

## Projects Overview

### 1. Crypto.Blazor.Core

**Crypto.Blazor.Core** is a C# Razor library for Blazor projects, currently supporting Kadena with limited functionality. The primary goal is to utilize the [pact-lang-api](https://www.npmjs.com/package/pact-lang-api) JavaScript library, the official JS library created by the Kadena development team, without rewriting it in C#. This approach ensures easy updates and maintenance by leveraging the official Kadena library.

[Nuget Package](https://www.nuget.org/packages/Crypto.Blazor.Core)

#### Key Features

- **Kadena Support**: Integration with the `pact-lang-api` library for seamless interaction with Kadena's blockchain.
- **Encapsulated JS Interoperability**: Uses service initiation to interact with the `pact-lang-api` library, minimizing overhead.
- **Wallet and Chain Balance Checks**: Check the balance of a KDA wallet across all chains, multiple wallets at once, and individual chain balances.

#### Current Methods

- **GetKdaChainBalance**
    ```cs
    public async Task<KdaChainBalance> GetKdaChainBalance(string walletAddr, int chainId)
    public async Task<KdaChainBalance> GetKdaChainBalance(string walletAddr, string chainIdString)
    ```

- **UpdateKdaVersion**
    ```cs
    public async Task UpdateKdaVersion()
    ```

- **GetKdaWalletAsync**
    ```cs
    public async Task<List<KdaWallet>> GetKdaWalletAsync(IEnumerable<string> walletAddr)
    public async Task<KdaWallet> GetKdaWalletAsync(string walletAddr)
    ```

#### Example Usage

To initiate and use the `CryptoService` in your Blazor application:

1. In `Program.cs`, add the service configuration:
    ```cs
    MagicCryptoServiceConfig.Configure(builder.Services);
    ```

2. Inject the `CryptoService` into your component:
    ```razor
    @inject CryptoService _CryptoService
    ```

3. Use the service methods:
    ```cs
    // Example with multiple addresses
    string[] addresses = ["address1","address2"];
    List<KdaWallet> Wallets = await _CryptoService.GetKdaWalletAsync(addresses);

    // Example with a single address
    KdaWallet Wallet = await _CryptoService.GetKdaWalletAsync("address1");

    // Example with chain balance
    KdaChainBalance WalletBalance = await _CryptoService.GetKdaChainBalance("address1", 2);

    // Example with all chain balances
    List<KdaChainBalance> AllChainBalances = await _CryptoService.GetKdaAllChainBalancesAsync("address1");
    ```

#### Rate Limiting

The Kadena Chainweb API has a rate limit of 50 calls per second. Within the `CryptoService`, a hardcoded rate limit of 40 calls per 1.3 seconds is set to prevent abuse and accidental IP bans. This default setting provides a 38.5% reduction in the rate limit, ensuring smooth operation and compliance with Kadena's rate limits.

### 2. Crypto.MauiBlazor.Wallet

*Coming Soon*: Detailed documentation and features for **Crypto.MauiBlazor.Wallet** will be added later.

## Future Plans

- Expanding support for additional blockchain APIs.
- Enhancing functionality and adding more features to both projects.
- Ensuring robust security and ease of use for developers and end-users.
