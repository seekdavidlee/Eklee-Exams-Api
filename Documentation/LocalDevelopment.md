# Introduction

This document will help you get started with running Eklee exams API locally.

Before we begin, let's recall that this is a multi-tenant SaaS application. We will be able to federate with one or more Azure Active Directory (AAD) tenants. Our application is also hosting an Admin API which will allow our internal users to access. Hence, we have our own AAD instance. If we have 2 customers, i.e. tenants, then we should have 3 AAD instances where we get our configuration settings from.

## Pre-requsites

Let's make sure you have the following already running.
* Local instance of Storage Emulator
* Local instance of CosmosDB Emulator

## Getting Started

To begin, we should go ahead and take the following local.settings.json file as an example. 

```
{
	"IsEncrypted": false,
	"Values": {
		"AzureWebJobsStorage": "UseDevelopmentStorage=true",
		"FUNCTIONS_WORKER_RUNTIME": "dotnet"
	},
	"GraphQl": {
		"EnableMetrics": "true",
		"ExposeExceptions": "true"
	},
	"Host": {
		"LocalHttpPort": 7071,
		"CORS": "*"
	},
	"ServiceAccounts": [
		{
			"AppId": "",
			"Issuer": ""
		}
	],
	"DocumentDb": {
		"Key": "",
		"Url": "https://localhost:8081",
		"RequestUnits": "400"
	},
	"Search": {
		"ServiceName": "",
		"ApiKey": ""
	},
	"Admin": {
		"DatabaseName":  "admindb",
		"Audience": "",
		"Issuer": "",
		"DocumentDb": {
			"Key": "",
			"Url": "https://localhost:8081",
			"RequestUnits": "400"
		}
	},
	"Security": {
		"Audience": "",
		"Issuers": ""
	},
	"AdminApi": {
		"Endpoint": "http://localhost:7071"
	},
	"AdminClient": {
		"ClientId": "",
		"ClientSecret": "",
		"Resource": "",
		"TenantId": ""
	}
}
```

## ServiceAccounts

There are 2 settings here. AppId refers to the application Id of the hosted Admin API. Issuer refers to our Admin AAD tenant issuer Url.