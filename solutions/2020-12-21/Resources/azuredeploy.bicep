// Resource name
param name string
param index int

// Resource location
param location string = resourceGroup().location

// Resource location code
param locationCode string = 'krc'

// Storage CosmosAccount
param storageAccountSku string = 'Standard_LRS'

// Function App
param functionAppTimezone string = 'Korea Standard Time'
param functionAppWorkerRuntime string {
  allowed: [
    'dotnet'
    'node'
    'python'
  ]
}
param functionAppWorkerVersion string

var metadata = {
  longName: '{0}-${name}-step-${index}-${locationCode}'
  shortName: '{0}${name}${index}${locationCode}'
}

var storage = {
  name: format(metadata.shortName, 'st')
  location: location
  sku: storageAccountSku
}

resource st 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: storage.name
  location: storage.location
  kind: 'StorageV2'
  sku: {
    name: storage.sku
  }
  properties: {
    supportsHttpsTrafficOnly: true
  }
}

var workspace = {
  name: replace(metadata.longName, '{0}', 'wrkspc')
  location: location
}

resource wrkspc 'Microsoft.OperationalInsights/workspaces@2020-08-01' = {
  name: workspace.name
  location: workspace.location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    workspaceCapping: {
      dailyQuotaGb: -1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

var appInsights = {
  name: replace(metadata.longName, '{0}', 'appins')
  location: location
}

resource appins 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: appInsights.name
  location: appInsights.location
  kind: 'web'
  properties: {
    Flow_Type: 'Bluefield'
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: wrkspc.id
  }
}

var servicePlan = {
  name: replace(metadata.longName, '{0}', 'csplan')
  location: location
}

resource csplan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: servicePlan.name
  location: servicePlan.location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionApp'
  properties: {
    reserved: true
  }
}

var functionApp = {
  name: replace(metadata.longName, '{0}', 'fncapp')
  location: location
  timezone: functionAppTimezone
  workerRuntime: functionAppWorkerRuntime
  workerVersion: functionAppWorkerVersion
}

resource fncapp 'Microsoft.Web/sites@2020-06-01' = {
  name: functionApp.name
  location: functionApp.location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: csplan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: '${functionApp.workerRuntime}|${functionApp.workerVersion}'
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: '${reference(appins.id, '2020-02-02-preview', 'Full').properties.InstrumentationKey}'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '${reference(appins.id, '2020-02-02-preview', 'Full').properties.connectionString}'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${st.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(st.id, '2019-06-01').keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionApp.workerRuntime
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${st.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(st.id, '2019-06-01').keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: functionApp.name
        }
        {
          name: 'WEBSITE_TIME_ZONE'
          value: functionApp.timezone
        }
        {
          name: 'NO_PEPPER_CONTAINER'
          value: 'https://${st.name}.blob.${environment().suffixes.storage}/no-pepper/'
        }
        {
          name: 'PEPPER_CONTAINER'
          value: 'https://${st.name}.blob.${environment().suffixes.storage}/pepper/'
        }
      ]
    }
  }
}
