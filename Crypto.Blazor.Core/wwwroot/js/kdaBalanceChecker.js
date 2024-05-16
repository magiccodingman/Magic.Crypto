// Load external scripts dynamically
function loadScript(src, integrity, crossOrigin) {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = src;
        if (integrity) script.integrity = integrity;
        if (crossOrigin) script.crossOrigin = crossOrigin;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`Script load error for ${src}`));
        document.head.appendChild(script);
    });
}

export async function loadDependencies() {
    try {
        await loadScript('https://code.jquery.com/jquery-3.7.1.min.js', null, 'anonymous');
        await loadScript('https://cdn.jsdelivr.net/npm/pact-lang-api@4.1.2/pact-lang-api-global.min.js');
    } catch (error) {
        console.error('Error loading external libraries:', error);
        throw error;
    }
}


export async function getVersion(server) {
    try {
        const res = await fetch(`https://${server}/info`);
        const resJSON = await res.json();
        const av = resJSON.nodeApiVersion;
        const nv = resJSON.nodeVersion;
        let chainIds = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
        if (resJSON.nodeChains.length !== 10) {
            const bh = resJSON.nodeGraphHistory[0][0];
            const len = resJSON.nodeGraphHistory[0][1].length;
            const cut = await fetch(`https://${server}/chainweb/${av}/${nv}/cut`);
            const cutJSON = await cut.json();
            const h = cutJSON.height;
            if (h > bh) {
                let cids = Array.from(Array(len).keys());
                chainIds = cids.map(x => x.toString());
            }
        }
        return {
            nv,
            chainIds
        };
    } catch (e) {
        console.log(e);
        throw new Error("Unable to fetch from " + JSON.stringify(server));
    }
}


export async function getChainBalanceResponse(server, nv, acctName, chainId) {
    const host = (chainId) => `https://${server}/chainweb/0.0/${nv}/chain/${chainId}/pact`;
    const creationTime = () => Math.round((new Date).getTime() / 1000) - 15;
    const token = 'coin';
    const dumMeta = (chainId) => Pact.lang.mkMeta("not-real", chainId, 0.00000001, 6000, creationTime(), 600);
    
    const response = await Pact.fetch.local({
        pactCode: `(${token}.details "${acctName}")`,
        meta: dumMeta(chainId)
    }, host(chainId))


    return response.result;
}
export async function getBalance(server, token, acctName) {

    const chainBal = {};
    const creationTime = () => Math.round((new Date).getTime() / 1000) - 15;
    const dumMeta = (chainId) => Pact.lang.mkMeta("not-real", chainId, 0.00000001, 6000, creationTime(), 600);

    async function getBalanceFromChain(host, chainId) {
        try {
            const response = await Pact.fetch.local({
                pactCode: `(${token}.details ${acctName})`,
                meta: dumMeta(chainId)
            }, host(chainId));
            console.log(host(chainId));
            console.log(dumMeta(chainId));
            console.log(response);
            const result = response.result;
            let bal = result.data
                ? (typeof result.data.balance === "number")
                    ? result.data.balance
                    : (result.data.balance.decimal ? result.data.balance.decimal : 0)
                : 0;
            return { ChainIdString: chainId.toString(), Balance: Number(bal) };
        } catch (e) {
            console.log(e);
            return { ChainIdString: chainId.toString(), Balance: 0 };
        }
    }

    try {
        const info = await getVersion(server);
        const host = (chainId) => `https://${server}/chainweb/0.0/${info.nv}/chain/${chainId}/pact`;
        const balancePromises = info.chainIds.map(id => getBalanceFromChain(host, id));
        const balances = await Promise.all(balancePromises);
        return balances;
    } catch (e) {
        console.log(e);
        return [];
    }
}
