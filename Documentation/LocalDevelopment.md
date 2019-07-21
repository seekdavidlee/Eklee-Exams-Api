# Introduction

This document will help you get started with running Eklee exams API locally.

Before we begin, let's recall that this is a multi-tenant SaaS application. We will be able to federate with one or more Azure Active Directory (AAD) tenants. Our application is also hosting an Admin API which will allow our internal users manage new tenants which we call organizations. If we have 2 customers, i.e. tenants, then we should have 3 AAD instances in which the third AAD represents our own AAD Admin tenant.

## Pre-requsites

Let's make sure you have the following already running.
* Local instance of [Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)
* Local instance of [CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator)

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

### ServiceAccounts

AppId refers to the application Id of the hosted Admin API. Issuer refers to our Admin AAD tenant issuer Url.

### DocumentDb

The DocumentDb Key and Url represents the CosmosDb database instance where our tenant's collections will be created.

### Search

The Search ServiceName and ApiKey is the Azure Search instance where our tenant's search indexes will be created.

### Admin

The Audience represents the Application Id of the Admin API. Issuer refers to our Admin AAD tenant issuer Url. The DocumentDb Key and Url represents the CosmosDb database instance where our Admin specific collections will be created.

### Security

The Audience represents the Application Id of the Exam API. The Issuers refers to our Admin AAD tenant issuer Url and other Admin AAD tenants' issuer Url we trust.

### AdminApi

The endpoint represents the Url from where the Admin API is hosted. Note that for this example, we are hosting the Admin Url in the same Azure Function host.

### AdminClient

When the Exam API is first started, we will need to review which AAD tenants have federated with us. The AdminClient is used to connect to the Admin API. In security terms, we are using the Client Credentials flow to connect to the Admin API via GraphQL.

## Azure Active Directory

This section is to provide additional details to each of the AAD specific settings. For the purpose of this example, it is recommanded for you to create 3 Free AAD instances. The first AAD can represent you as the Admin and provider of the Exam API. The other 2 AAD instances would represent the 2 tenants that are your customers.

In the Admin AAD instance, you can create 2 Applications, one to represent the Exam API and the other to represent the Admin API. 

You can create the following roles for the Exam API. The following is part of the Application Manifest in AAD application. Be sure to update the id for each role which is just a unique GUID.

Under the section Supported account types, be sure to choose "
Accounts in any organizational directory".

```
"appRoles": [
	{
		"allowedMemberTypes": [
			"User"
		],
		"description": "User can read data.",
		"displayName": "UserReadOnlyRole",
		"id": "<GUID>",
		"isEnabled": true,
		"lang": null,
		"origin": "Application",
		"value": "Eklee.User.Reader"
	},
	{
		"allowedMemberTypes": [
			"User"
		],
		"description": "User can write data.",
		"displayName": "UserWriteOnlyRole",
		"id": "<GUID>",
		"isEnabled": true,
		"lang": null,
		"origin": "Application",
		"value": "Eklee.User.Writer"
	}
],
```

You can create the following roles for the Admin API.

```
"appRoles": [
	{
		"allowedMemberTypes": [
			"User"
		],
		"description": "User can write data.",
		"displayName": "UserWriteOnlyRole",
		"id": "<GUID>",
		"isEnabled": true,
		"lang": null,
		"origin": "Application",
		"value": "Eklee.Admin.Writer"
	},
	{
		"allowedMemberTypes": [
			"User"
		],
		"description": "User can read data.",
		"displayName": "UserReadOnlyRole",
		"id": "<GUID>",
		"isEnabled": true,
		"lang": null,
		"origin": "Application",
		"value": "Eklee.Admin.Reader"
	}
]
```

Under the section Supported account types, be sure to choose "
Accounts in this organizational directory only".

Now, we can start creating users in the Admin AAD and start assigning users specific roles. We can also do the same for the other 2 tenants. 