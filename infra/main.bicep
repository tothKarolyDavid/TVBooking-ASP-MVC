param environmentName string
param location string = resourceGroup().location

var containerAppName = '${environmentName}-app'
var containerAppsEnvName = '${environmentName}-env'
var containerRegistryName = replace(replace('${environmentName}acr', '-', ''), '.', '')
var storageAccountName = replace(replace('${environmentName}data', '-', ''), '.', '')
var fileShareName = 'tvbooking-data'

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2026-01-01' = {
  name: containerAppsEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: null
    }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-08-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: { name: 'Standard_LRS' }
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2025-08-01' = {
  name: '${storageAccountName}/default/${fileShareName}'
  dependsOn: [
    storageAccount
  ]
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: containerRegistryName
  location: location
  sku: { name: 'Basic' }
  properties: {
    adminUserEnabled: true
  }
}

resource containerApp 'Microsoft.App/containerApps@2026-01-01' = {
  name: containerAppName
  location: location
  tags: {
    'azd-service-name': 'web'
  }
  identity: { type: 'SystemAssigned' }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      secrets: [
        {
          name: 'acr-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
      ]
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'acr-password'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'tvbooking'
          image: '${containerRegistry.properties.loginServer}/tvbooking/web-tvbooking:latest'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, containerApp.id, 'AcrPull')
  properties: {
    principalId: containerApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  }
  scope: containerRegistry
}

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.properties.loginServer
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = containerAppsEnvName
output APPLICATION_URL string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output STORAGE_ACCOUNT_NAME string = storageAccount.name
output STORAGE_ACCOUNT_KEY string = storageAccount.listKeys().keys[0].value
output FILE_SHARE_NAME string = fileShareName
output CONTAINER_APP_NAME string = containerAppName
output RESOURCE_GROUP_NAME string = resourceGroup().name
