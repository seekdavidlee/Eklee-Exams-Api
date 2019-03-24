# Introduction

The purpose of this project is to demonstrate the capabilities of [Eklee-Azure-Functions-GraphQl](https://github.com/seekdavidlee/Eklee-Azure-Functions-GraphQl). Having a good understanding of a particular use case is a good starting point for learning about the Eklee GraphQL implementation.

* [https://dev.azure.com/eklee/Eklee.Exams.Api/](https://dev.azure.com/eklee/Eklee.Exams.Api/)

## Nuget

You can get the latest version of Eklee-Azure-Functions-GraphQl at [https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl](https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl)

## The Premise

Eklee Exams is a SaaS provider for providing testing services to Educational or Trade Organizations (tenants) who wishes to leverage a cloud platform for their students or contractors/employees/external product users (candidates) to take exams. Eklee Exams wishes to provides a common, configurable API from which the user interface (UI) can be customized for each tenant since each tenant has different UI experience for their specific exam(s). Eklee Exams engineers can provide additional services for developing and customizing the UI piece or the tenant's engineers/developers could do it themselves. The value proposition here is that each tenant can devote the majority of their resources towards building a UI with optimal and configurable UI experience for the exam which they wish for each candidate to take so the candidates can be certified using the most current topics and UI techniques. From a technical perpsective, tenants can be ensured that any backend concerns such as security, scaling, data management, cost can be taken care of with built-in cloud capabilities in Azure.

### Authentication and Authorization

An additional word about security. Each tenant already leverage existing identity provider such as Active Directory internally. Thus, it is necessary to federate with their identity provider. Initially, we will require each tenant to use their managed Azure Active Directory (AAD). Tenant will have the ability to leverage AAD connect to sync their on-premise AD users to the Cloud, onto AAD.

Eklee will leverage Role-based security and will have the following roles.

* System Administrator (Eklee Exam employees who are identified as Administrators who can perform read/write)
* System Support (Eklee Exam employees who can review organization)
* Organization Administrator (Tenant employees who are identity as Administrators and can perform read/write)
* Organization Read-only User (Tenant employees who can review the test results of the candidates)
* Candidate

### Core Concepts

The following entities are created.

* Internal Employee
* Organization
* Organization Employee
* Testing Center
* Exam
* Candidate
* Test Result

** More documentation/topics are coming. **