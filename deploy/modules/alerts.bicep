param environmentName string
param location string
param tags object
param alertEmail string
param applicationInsightsId string
param serviceBusNamespaceName string
param queueName string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'ag-oddscollector-${environmentName}'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: take('odds-${environmentName}', 12)
    enabled: true
    emailReceivers: [
      {
        name: 'email'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

resource leagueFailures 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: 'alert-oddscollector-${environmentName}-league-failures'
  location: location
  tags: tags
  kind: 'LogAlert'
  properties: {
    displayName: 'OddsCollector (${environmentName}): league collection failed'
    description: 'The Odds API data of a league could not be collected. The run kept the other leagues; see the "Failed to get ... for {League}" logs.'
    severity: 2
    enabled: true
    scopes: [
      applicationInsightsId
    ]
    evaluationFrequency: 'PT6H'
    windowSize: 'PT6H'
    criteria: {
      allOf: [
        {
          query: '''
            customMetrics
            | where name == "oddscollector.oddsapi.league.failures"
            | extend Operation = tostring(customDimensions.operation), League = tostring(customDimensions.league)
            | summarize Failures = sum(valueSum) by Operation, League
            '''
          timeAggregation: 'Total'
          metricMeasureColumn: 'Failures'
          dimensions: [
            {
              name: 'Operation'
              operator: 'Include'
              values: [
                '*'
              ]
            }
            {
              name: 'League'
              operator: 'Include'
              values: [
                '*'
              ]
            }
          ]
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    autoMitigate: true
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
  }
}

// Fires on every message that is dead-lettered: DeadLetterFunction counts each one as it takes it out of the
// dead-letter queue. Resolves by itself once an hour passes without new ones.
resource deadLetteredMessages 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: 'alert-oddscollector-${environmentName}-dead-lettered-messages'
  location: location
  tags: tags
  kind: 'LogAlert'
  properties: {
    displayName: 'OddsCollector (${environmentName}): messages dead-lettered'
    description: 'Messages of ${queueName} were dead-lettered: PredictionFunction could not read them, or they expired before it processed them. DeadLetterFunction logged each one ("Removed dead-lettered message ...", with the reason and the body) and removed it.'
    severity: 2
    enabled: true
    scopes: [
      applicationInsightsId
    ]
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    criteria: {
      allOf: [
        {
          query: '''
            customMetrics
            | where name == "oddscollector.servicebus.deadlettered"
            | extend Reason = tostring(customDimensions.reason)
            | summarize DeadLettered = sum(valueSum) by Reason
            '''
          timeAggregation: 'Total'
          metricMeasureColumn: 'DeadLettered'
          dimensions: [
            {
              name: 'Reason'
              operator: 'Include'
              values: [
                '*'
              ]
            }
          ]
          operator: 'GreaterThan'
          threshold: 0
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    autoMitigate: true
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
  }
}

// DeadLetterFunction empties the dead-letter queue within seconds. If the queue stays non-empty for a whole hour,
// the function is not running or keeps failing, and the alert above no longer sees new dead-lettered messages.
// The name is the one the earlier alert on the dead-letter count had, so that deploying updates it in place.
resource deadLetterQueueNotDrained 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'alert-oddscollector-${environmentName}-dead-lettered'
  location: 'global'
  tags: tags
  properties: {
    description: 'The dead-letter queue of ${queueName} has not been empty for an hour: DeadLetterFunction is not removing messages from it. Check the failures of DeadLetterFunction.'
    severity: 2
    enabled: true
    scopes: [
      serviceBusNamespace.id
    ]
    evaluationFrequency: 'PT1H'
    windowSize: 'PT1H'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'DeadLetteredMessages'
          criterionType: 'StaticThresholdCriterion'
          metricNamespace: 'Microsoft.ServiceBus/namespaces'
          metricName: 'DeadletteredMessages'
          dimensions: [
            {
              name: 'EntityName'
              operator: 'Include'
              values: [
                queueName
              ]
            }
          ]
          operator: 'GreaterThan'
          threshold: 0
          timeAggregation: 'Minimum'
        }
      ]
    }
    autoMitigate: true
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}
