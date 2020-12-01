// Resource name
param name string

// Resource location
param location string = resourceGroup().location

// Resource location code
param locationCode string = 'krc'

// Endpoints
param step01Endpoint string
param step02Endpoint string
param step03Endpoint string
param step04Endpoint string
param step05Endpoint string
param step06Endpoint string
param step07Endpoint string
param step08Endpoint string
param step09Endpoint string

var metadata = {
  longName: '{0}-${name}-{1}-${locationCode}'
  shortName: '{0}${name}${locationCode}'
}

var connection = {
  name: replace(metadata.longName, '{0}', 'apicon')
  location: location
  connectionId: '${resourceGroup().id}/providers/Microsoft.Web/connections/${replace(metadata.longName, '{0}', 'apicon')}'
  id: '${subscription().id}/providers/Microsoft.Web/locations/${location}/managedApis/office365'
}

resource apicon 'Microsoft.Web/connections@2016-06-01' = {
  name: replace(connection.name, '{1}', 'office365')
  location: connection.location
  kind: 'V1'
  properties: {
    displayName: replace(connection.name, '{1}', 'office365')
    api: {
      id: connection.id
    }
  }
}

var logicApp = {
  name: replace(metadata.longName, '{0}', 'logapp')
  location: location
}

var endpoints = {
  step01: step01Endpoint
  step02: step02Endpoint
  step03: step03Endpoint
  step04: step04Endpoint
  step05: step05Endpoint
  step06: step06Endpoint
  step07: step07Endpoint
  step08: step08Endpoint
  step09: step09Endpoint
}

resource logapp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: replace(logicApp.name, '{1}', 'recipe')
  location: logicApp.location
  properties: {
    state: 'Enabled'
    parameters: {
      '$connections': {
        value: {
          office365: {
            connectionId: replace(connection.connectionId, '{1}', 'office365')
            connectionName: apicon.name
            id: connection.id
          }
        }
      }
    }
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {
        '$connections': {
          type: 'object'
          defaultValue: {}
        }
        step01Endpoint: {
          type: 'string'
          defaultValue: endpoints.step01
        }
        step02Endpoint: {
          type: 'string'
          defaultValue: endpoints.step02
        }
        step03Endpoint: {
          type: 'string'
          defaultValue: endpoints.step03
        }
        step04Endpoint: {
          type: 'string'
          defaultValue: endpoints.step04
        }
        step05Endpoint: {
          type: 'string'
          defaultValue: endpoints.step05
        }
        step06Endpoint: {
          type: 'string'
          defaultValue: endpoints.step06
        }
        step07Endpoint: {
          type: 'string'
          defaultValue: endpoints.step07
        }
        step08Endpoint: {
          type: 'string'
          defaultValue: endpoints.step08
        }
        step09Endpoint: {
          type: 'string'
          defaultValue: endpoints.step09
        }
      }
      triggers: {
        manual: {
          type: 'Request'
          kind: 'Http'
          inputs: {
            schema: {
              type: 'object'
              properties: {
                boughtSlicedGaraetteok: {
                  type: 'boolean'
                }
                timeToSoakInMinutes: {
                  type: 'integer'
                }
                timeToSliceInMinutes: {
                  type: 'integer'
                }
                timeToStirFryInMinutes: {
                  type: 'integer'
                }
                timeToBoilInMinutes: {
                  type: 'integer'
                }
                pepper: {
                  type: 'boolean'
                }
                email: {
                  type: 'string'
                }
              }
            }
          }
        }
      }
      actions: {
        HTTP_Webhook_Step_1: {
          type: 'HttpWebhook'
          runAfter: {}
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step01Endpoint\')'
              body: {
                boughtSlicedGaraetteok: '@triggerBody()?[\'boughtSlicedGaraetteok\']'
                timeToSoakInMinutes: '@triggerBody()?[\'timeToSoakInMinutes\']'
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        HTTP_Webhook_Step_2: {
          type: 'HttpWebhook'
          runAfter: {}
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step02Endpoint\')'
              body: {
                timeToSliceInMinutes: '@triggerBody()?[\'timeToSliceInMinutes\']'
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        HTTP_Webhook_Step_3: {
          type: 'HttpWebhook'
          runAfter: {}
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step03Endpoint\')'
              body: {
                timeToStirFryInMinutes: '@triggerBody()?[\'timeToStirFryInMinutes\']'
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        HTTP_Webhook_Step_6: {
          type: 'HttpWebhook'
          runAfter: {}
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step06Endpoint\')'
              body: {
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        Check_Steps: {
          type: 'Compose'
          runAfter: {
            HTTP_Webhook_Step_1: [
              'Succeeded'
            ]
            HTTP_Webhook_Step_2: [
              'Succeeded'
            ]
            HTTP_Webhook_Step_3: [
              'Succeeded'
            ]
            HTTP_Webhook_Step_6: [
              'Succeeded'
            ]
          }
          inputs: {
            step1Completed: '@body(\'HTTP_Webhook_Step_1\')?[\'completed\']'
            step2Completed: '@body(\'HTTP_Webhook_Step_2\')?[\'completed\']'
            step3Completed: '@body(\'HTTP_Webhook_Step_3\')?[\'completed\']'
            step6Completed: '@json(body(\'HTTP_Webhook_Step_6\'))?[\'completed\']'
          }
        }
        HTTP_Webhook_Step_4: {
          type: 'HttpWebhook'
          runAfter: {
            Check_Steps: [
              'Succeeded'
            ]
          }
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step04Endpoint\')'
              body: {
                timeToBoilInMinutes: '@triggerBody()?[\'timeToBoilInMinutes\']'
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        Is_Bubble_Removed: {
          type: 'InitializeVariable'
          runAfter: {
            Check_Steps: [
              'Succeeded'
            ]
          }
          inputs: {
            variables: [
              {
                type: 'boolean'
                name: 'BubbleRemoved'
                value: false
              }
            ]
          }
        }
        Until: {
          type: 'Until'
          runAfter: {
            Is_Bubble_Removed: [
              'Succeeded'
            ]
          }
          expression: '@equals(variables(\'BubbleRemoved\'), true)'
          limit: {
            count: 60
            timeout: 'PT1H'
          }
          actions: {
            BubbleAppeared: {
              type: 'Compose'
              runAfter: {}
              inputs: {
                bubbleAppeared: '@if(greater(rand(0, 11), 5), true, false)'
              }
            }
            HTTP_Step_5: {
              type: 'Http'
              runAfter: {
                BubbleAppeared: [
                  'Succeeded'
                ]
              }
              inputs: {
                method: 'POST'
                uri: '@parameters(\'step05Endpoint\')'
                body: '@outputs(\'BubbleAppeared\')'
              }
            }
            Set_variable: {
              type: 'SetVariable'
              runAfter: {
                HTTP_Step_5: [
                  'Succeeded'
                ]
              }
              inputs: {
                name: 'BubbleRemoved'
                value: '@body(\'HTTP_Step_5\')?[\'removed\']'
              }
            }
          }
        }
        HTTP_Webhook_Step_7: {
          type: 'HttpWebhook'
          runAfter: {
            HTTP_Webhook_Step_4: [
              'Succeeded'
            ]
            Until: [
              'Succeeded'
            ]
          }
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step07Endpoint\')'
              body: {
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        HTTP_Webhook_Step_8: {
          type: 'HttpWebhook'
          runAfter: {
            HTTP_Webhook_Step_7: [
              'Succeeded'
            ]
          }
          inputs: {
            subscribe: {
              method: 'POST'
              uri: '@parameters(\'step08Endpoint\')'
              body: {
                callbackUrl: '@listCallbackUrl()'
              }
            }
            unsubscribe: {}
          }
        }
        HTTP_Step_9: {
          type: 'Http'
          runAfter: {
            HTTP_Webhook_Step_8: [
              'Succeeded'
            ]
          }
          inputs: {
            method: 'POST'
            uri: '@parameters(\'step09Endpoint\')'
            body: {
              pepper: '@triggerBody()?[\'pepper\']'
            }
          }
        }
        Send_an_email: {
          type: 'ApiConnection'
          runAfter: {
            HTTP_Step_9: [
              'Succeeded'
            ]
          }
          inputs: {
            method: 'POST'
            host: {
              connection: {
                name: '@parameters(\'$connections\')[\'office365\'][\'connectionId\']'
              }
            }
            path: '/v2/Mail'
            body: {
              To: '@triggerBody()?[\'email\']'
              Subject: 'Your Tteokguk Is Ready!'
              Body: '<p><img src="@{json(body(\'HTTP_Step_9\'))?[\'tteokgukImageUrl\']}"></p>'
            }
          }
        }
      }
      outputs: {}
    }
  }
}
