# Tickets Queue — Asynchronous Ticket Management System

## Overview

**Tickets Queue** is an academic software engineering project designed to explore the development of a ticket management system using **.NET 10, RabbitMQ, background workers, and SMTP email testing**.

The application allows users to create and retrieve tickets through a REST API. When a ticket is created, the API publishes an event to RabbitMQ. A separate Worker application consumes the event and processes an email notification using an SMTP server simulated by Mailpit.

The main goal is to understand how asynchronous messaging decouples application responsibilities and how background processing can be integrated into a .NET-based backend system.

> **Academic Project:** This repository was developed for learning and experimentation. It does not implement every recommended software engineering practice, production-grade reliability mechanism, or security control. The implementation prioritizes understanding the core concepts and their integration.

## Architecture

The solution is organized into independent applications with distinct responsibilities.

```text
TicketsQueue
├── TicketsQueue.API
├── TicketsQueue.Application
├── TicketsQueue.Domain
├── TicketsQueue.Infrastructure
└── TicketsQueue.Worker
```

The exact responsibilities of each project are:

| Component | Responsibility |
|---|---|
| `TicketsQueue.API` | Exposes HTTP endpoints for ticket operations and acts as the entry point for client requests. |
| `TicketsQueue.Application` | Contains application use cases and coordinates application workflows. |
| `TicketsQueue.Domain` | Contains core domain concepts and business rules. |
| `TicketsQueue.Infrastructure` | Provides infrastructure implementations, including persistence and RabbitMQ publishing. |
| `TicketsQueue.Worker` | Runs independently, consumes ticket events, and triggers email notifications. |

The API and Worker are separate executable processes. They communicate asynchronously through RabbitMQ rather than requiring the API to perform email notification work within the original HTTP request.

## Technology Stack

- **C# and .NET 10** — Backend development and application hosting.
- **ASP.NET Core Web API** — REST API implementation.
- **RabbitMQ** — Message broker for asynchronous event delivery.
- **RabbitMQ.Client** — .NET client library for publishing and consuming messages.
- **Background Worker / Hosted Service** — Continuous background processing.
- **MailKit** — SMTP client library used to send email messages.
- **Mailpit** — Local SMTP testing server that captures outgoing messages without delivering them to real recipients.
- **Docker** — Containerized local infrastructure.
- **Swagger / OpenAPI** — API exploration and testing, when enabled in the API project.
- **JSON** — Message serialization and communication between the API and Worker.
- **PostgreSQL** - Database to persist and retrieve the tickets data
The persistence technology and database configuration depend on the implementation included in the repository.

## Main Features

### 1. Ticket Management

The REST API provides the entry point for ticket operations, including ticket creation and retrieval.

The ticket workflow is responsible for receiving requests, executing the relevant application use cases, and returning HTTP responses to clients.

Ticket information includes identifiers and business attributes such as title and priority, according to the implemented request and response contracts.

### 2. Asynchronous Messaging with RabbitMQ

When a ticket is created, the API publishes a message describing the event.

RabbitMQ acts as the intermediary between the API and the Worker, allowing notification processing to occur independently of the original HTTP request.

The current implementation uses a **fanout exchange** and a queue named `tickets.created`. The queue must be bound to the same exchange used by the publisher so that published messages can reach the consumer.

The message is serialized as JSON and deserialized by the Worker into the corresponding message contract.

### 3. Background Processing

The Worker runs as an independent .NET application and continuously listens for messages on the configured RabbitMQ queue.

When a ticket-created event is received, the Worker:

1. Reads the message from RabbitMQ.
2. Deserializes the JSON payload into the ticket message contract.
3. Extracts the ticket information.
4. Builds an email notification containing the ticket details.
5. Sends the notification through the configured SMTP service.
6. Acknowledges the RabbitMQ message after the SMTP operation succeeds.

Manual message acknowledgment is used to demonstrate how message processing can be coordinated with broker delivery.

### 4. Email Notification Simulation

Mailpit provides a local environment for testing outgoing email messages.

Instead of delivering messages to real mailboxes, Mailpit captures them and makes their content available through a web interface.

This allows the complete notification workflow to be tested without requiring a real email delivery service or sending messages to actual recipients.

The SMTP implementation is separated behind an email sender interface, making it possible to replace the testing SMTP server with another provider in a future iteration.

## Message Flow

The end-to-end workflow is illustrated below.

```text
Client
  |
  | HTTP request
  v
TicketsQueue.API
  |
  | Create ticket
  | Publish ticket-created event
  v
RabbitMQ Exchange (fanout)
  |
  | Binding
  v
tickets.created Queue
  |
  | Asynchronous message delivery
  v
TicketsQueue.Worker
  |
  | Deserialize event
  | Build notification
  v
IEmailSender / MailKit
  |
  | SMTP
  v
Mailpit
  |
  v
Web Interface
```

This design separates ticket management from notification delivery. The API publishes an event, while the Worker independently handles the notification workflow.

## Running the Project Locally

### Prerequisites

Install or configure the following:

- .NET 10 SDK.
- Docker Desktop for Windows, or an equivalent Docker environment.
- Git.
- A REST client or browser for testing the API.
- The RabbitMQ and Mailpit containers.

### 1. Start RabbitMQ

Run the following command in PowerShell:

```powershell
docker run -d `
  --name tickets-rabbitmq `
  -p 5672:5672 `
  -p 15672:15672 `
  rabbitmq:4-management
```

The RabbitMQ management interface is available at:

http://localhost:15672

For a local development container using the default credentials, sign in with `guest` / `guest`.

### 2. Start Mailpit

Run:

```powershell
docker run -d `
  --name tickets-mailpit `
  -p 1025:1025 `
  -p 8025:8025 `
  axllent/mailpit
```

Mailpit's web interface is available at:

http://localhost:8025

The SMTP endpoint for applications running directly on the host machine is `localhost:1025`.

### 3. Configure the Applications

Configure the RabbitMQ connection settings in the API and Worker configuration files.

The Worker must use the same exchange name as the API publisher and consume from the queue bound to that exchange.

Example Worker configuration:

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "Exchange": "YOUR_EXCHANGE_NAME"
  },
  "Email": {
    "Host": "localhost",
    "Port": 1025,
    "FromName": "Tickets Queue",
    "FromEmail": "tickets@localhost",
    "ToEmail": "support@localhost",
    "Username": "",
    "Password": "",
    "UseStartTls": false
  }
}
```

Replace `YOUR_EXCHANGE_NAME` with the actual exchange configured in the API. Ensure that the queue name, exchange type, durability settings, and message contract are consistent between the applications.

These credentials and email addresses are intended only for local development. Do not use the default RabbitMQ credentials or unencrypted SMTP settings as a production configuration.

### 4. Run the Worker

Open a terminal in the Worker project directory:

```powershell
dotnet run --project .\TicketsQueue.Worker.csproj
```

Keep the process running. The Worker must remain active to consume messages and process notifications.

### 5. Run the API

Open a separate terminal in the API project directory and execute:

```powershell
dotnet run
```

Alternatively, run the API through your IDE.

Use the API's configured Swagger endpoint or a REST client to create a ticket.

### 6. Verify the Notification

After creating a ticket:

1. Confirm that the API successfully publishes the event.
2. Verify that the `tickets.created` queue receives the message in RabbitMQ.
3. Check the Worker terminal for message processing logs.
4. Open [Mailpit](http://localhost:8025).
5. Inspect the captured email and verify its subject and ticket details.

**Note:** The API, Worker, RabbitMQ, and Mailpit must all be running and configured correctly for the complete workflow to succeed.

## Reliability and Error Handling

The Worker uses manual acknowledgments to avoid confirming successful processing before the SMTP operation completes.

The current implementation also demonstrates basic handling of deserialization and processing errors. However, a requeued message may be delivered repeatedly when the underlying error persists.

This project does not claim to provide exactly-once processing or guaranteed email delivery. A successful SMTP operation only confirms acceptance by the SMTP server, not delivery to a real recipient's inbox.

## Academic Scope and Limitations

This project is intended for educational purposes and does not represent a complete production-ready ticketing platform.

The following areas are either simplified, not fully implemented, or require further development, depending on the final repository state:

- **Resilience:** No complete retry strategy with backoff, retry limits, and dead-letter queue management.
- **Duplicate processing:** No complete idempotency mechanism to prevent duplicate notification side effects.
- **Transactional publishing:** No transactional outbox implementation to coordinate database changes with event publication.
- **Observability:** Basic console logging rather than a comprehensive structured logging, metrics, and distributed tracing solution.
- **Security:** Local development credentials and SMTP settings are not intended for production. Production-grade secret management, authorization, and security hardening require additional work.
- **Email delivery:** Mailpit simulates email capture and does not validate real-world email deliverability.
- **Automated testing:** Comprehensive unit, integration, and end-to-end test coverage may not be present.
- **Deployment:** The local Docker setup is intended for development rather than a complete production deployment strategy.
- **Scalability:** Production capacity, broker topology, concurrency, and resource limits have not been comprehensively evaluated.
- **Operational recovery:** Broker failure handling, graceful recovery, poison-message handling, and delivery guarantees require further implementation.

These limitations are intentional boundaries of the learning scope, not claims that the listed practices are unnecessary. They identify opportunities for future improvements.

## Learning Objectives

The project provides hands-on experience with:

- Building a REST API using modern .NET.
- Separating application responsibilities across projects.
- Integrating a message broker into a backend system.
- Understanding exchanges, queues, bindings, and consumers in RabbitMQ.
- Processing asynchronous events through a hosted Worker.
- Sending SMTP messages using MailKit.
- Testing notification workflows without sending real emails.
- Running local infrastructure with Docker.
- Understanding the trade-offs between asynchronous processing, reliability, and operational complexity.

## Future Improvements

Potential next steps include:

- Implementing retry policies and dead-letter queues.
- Adding idempotent notification processing.
- Introducing the transactional outbox pattern.
- Improving structured logging, metrics, and distributed tracing.
- Adding automated tests for publishing, consumption, and notification delivery.
- Using environment-specific configuration and secure secret management.
- Creating a Docker Compose setup for the entire local environment.
- Evaluating concurrency, message throughput, and failure recovery.
- Integrating a real transactional email provider for controlled end-to-end delivery testing.

## Disclaimer

This repository is an academic project created to study backend development, asynchronous messaging, and email notification workflows.

It is not intended to be used as-is in a production environment. Not all recommended architectural patterns, security controls, reliability mechanisms, or operational practices have been implemented or validated.

The implementation should be considered a learning foundation that can be extended as more advanced software engineering concepts are studied and applied.