// Alerts that cover failures the functions survive: a league that keeps failing (the run still succeeds with the
// other leagues and only counts oddscollector.oddsapi.league.failures) and messages dead-lettered by
// PredictionFunction.

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

// Collection runs every 6 hours (upcoming events) and once a day (results), so a 6-hour window sees each run once.
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

resource deadLetteredMessages 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'alert-oddscollector-${environmentName}-dead-lettered'
  location: 'global'
  tags: tags
  properties: {
    description: 'Messages in the dead-letter queue of ${queueName}: PredictionFunction could not read or process them.'
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
          timeAggregation: 'Maximum'
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
