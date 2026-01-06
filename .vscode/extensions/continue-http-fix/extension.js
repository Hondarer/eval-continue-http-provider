const vscode = require('vscode');

/**
 * @param {vscode.ExtensionContext} context
 */
function activate(context) {
  console.log('Continue HTTP Providers Fix: Activating...');

  // Continue 拡張機能が有効になるまで待つ
  setTimeout(() => {
    registerHttpProviders();
  }, 2000);
}

async function registerHttpProviders() {
  const continueExt = vscode.extensions.getExtension('continue.continue');

  if (!continueExt) {
    console.error('Continue HTTP Fix: Continue extension not found');
    return;
  }

  let continueApi;
  try {
    continueApi = continueExt.isActive ? continueExt.exports : await continueExt.activate();
  } catch (error) {
    console.error('Continue HTTP Fix: Failed to activate Continue', error);
    return;
  }

  if (!continueApi?.registerCustomContextProvider) {
    console.error('Continue HTTP Fix: registerCustomContextProvider API not available');
    return;
  }

  // HTTP プロバイダーの設定
  const httpConfigs = [
    { url: 'http://localhost:5000/context', title: 'http-provider-5000', displayTitle: 'HTTP Provider 5000' },
    { url: 'http://localhost:5001/context', title: 'http-provider-5001', displayTitle: 'HTTP Provider 5001' },
    { url: 'http://localhost:5002/context', title: 'http-provider-5002', displayTitle: 'HTTP Provider 5002' }
  ];

  // 各 HTTP プロバイダーを登録
  for (const config of httpConfigs) {
    const provider = createHttpProvider(config);
    try {
      await continueApi.registerCustomContextProvider(provider);
      console.log(`Continue HTTP Fix: Registered ${config.displayTitle}`);
    } catch (error) {
      console.error(`Continue HTTP Fix: Failed to register ${config.displayTitle}`, error);
    }
  }
}

function createHttpProvider({ url, title, displayTitle }) {
  return {
    get description() {
      return {
        title,
        displayTitle,
        description: `Retrieve context from ${url}`,
        type: 'normal'
      };
    },

    async getContextItems(query, extras) {

      try {
        const response = await fetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            query: query || '',
            // extras から追加情報を渡すことも可能
            fullInput: extras?.fullInput || ''
          })
        });

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();

        // レスポンスを Continue が期待する形式に変換
        if (Array.isArray(data)) {
          return data;
        } else if (data.name && data.description && data.content) {
          return [data];
        } else {
          return [{
            name: displayTitle,
            description: `Response from ${displayTitle}`,
            content: JSON.stringify(data, null, 2)
          }];
        }
      } catch (error) {
        console.error(`Continue HTTP Fix: Error fetching from ${displayTitle}:`, error);
        return [{
          name: displayTitle,
          description: 'Error',
          content: `Failed to fetch from ${url}: ${error.message}`
        }];
      }
    },

    // submenu タイプの場合に使用
    async loadSubmenuItems(args) {
      return [];
    }
  };
}

function deactivate() {
  console.log('Continue HTTP Fix: Deactivating...');
}

module.exports = {
  activate,
  deactivate
};
