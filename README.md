# Introduction

The purpose of this project is to demonstrate the capabilities of [Eklee-Azure-Functions-GraphQl](https://github.com/seekdavidlee/Eklee-Azure-Functions-GraphQl). Having a good understanding of a particular use case is a good starting point for learning about the Eklee GraphQL implementation.

* [https://dev.azure.com/eklee/Eklee.Exams.Api/](https://dev.azure.com/eklee/Eklee.Exams.Api/)

## Topics
- [Local Development](Documentation/LocalDevelopment.md)

## Nuget

You can get the latest version of Eklee-Azure-Functions-GraphQl at [https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl](https://www.nuget.org/packages/Eklee.Azure.Functions.GraphQl)

## The Premise

Eklee Exams is a SaaS provider for providing testing services to Educational or Trade Organizations (tenants) who wishes to leverage a cloud platform for their students or contractors/employees/external product users (candidates) to take internal exams. Eklee Exams wishes to provides a common, configurable API from which the user interface (UI) can be customized for each tenant since each tenant has different UI experience for their specific exam(s). Eklee Exams engineers can provide additional services for developing and customizing the UI piece or the tenant's engineers/developers could do it themselves. The value proposition here is that each tenant can devote the majority of their resources towards building a UI with optimal and configurable UI experience for the exam which they wish for each candidate to take so the candidates can be certified using the most current topics and UI techniques. From a technical perpsective, tenants can be ensured that any backend concerns such as security, scaling, data management, cost can be taken care of with built-in cloud capabilities in Azure.

### Authentication and Authorization

An additional word about security. Each tenant already leverage existing identity provider such as Active Directory internally. Thus, it is necessary to federate with their identity provider. Initially, we will require each tenant to use their managed Azure Active Directory (AAD). Tenant will have the ability to leverage AAD connect to sync their on-premise AD users to the Cloud, onto AAD.

Eklee will leverage Role-based security and will have the following roles.

* Eklee.User.Reader
* Eklee.User.Writer
* Eklee.Admin.Writer
* Eklee.Admin.Reader

### Core Concepts

The following Models are created.

* Organization (Represents the customer)
* Exam (Represent the exam managed by the customer)
** Publication (Represents the exam itself, there's a connection from Exam to Publication via ExamPublication)
* TestResult (Represent the actual results from the exam)
** Candidate (Represent the employee who took the exam)
** TestResultPublication (Represents the connection from the TestResult to actual Exam)
* Employee (Represent the employee of the tenant company)

** More documentation/topics are coming. **