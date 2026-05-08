param environmentName string
param location string = resourceGroup().location

var appServicePlanName = '${environmentName}-asp'
var webAppName = '${environmentName}-app'
var dataDirectory = 'D:\\home\\data'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'app'
  properties: {
    reserved: false
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  tags: {
    'azd-service-name': 'web'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v10.0'
      alwaysOn: false
      http20Enabled: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ConnectionStrings__ApplicationDbContextConnection'
          value: 'Data Source=${dataDirectory}\\TVBookingMVC.db'
        }
      ]
    }
  }
}

output APPLICATION_URL string = 'https://${webApp.properties.defaultHostName}'
output APP_SERVICE_PLAN_NAME string = appServicePlanName
output WEBAPP_NAME string = webAppName
output RESOURCE_GROUP_NAME string = resourceGroup().name
