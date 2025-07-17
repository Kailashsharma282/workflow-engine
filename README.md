
# Workflow Engine API (.NET 9)

A configurable state machine workflow engine built with .NET 9 Minimal APIs, implementing a complete solution for Infonetica's technical assignment.

## Features

- **State Machine Core**:
  - Define workflows with states and transitions
  - Start workflow instances
  - Execute validated state transitions
  - Track complete instance history

- **Technical Stack**:
  - .NET 9 Minimal APIs
  - Swagger/OpenAPI 3.0 documentation
  - In-memory data persistence
  - Nullable reference types
  - Built-in dependency injection

## Solution Structure (Visual Studio 2022)
```
WorkflowEngine/
├── Models/ # Domain models
│ ├── State.cs # State definition
│ ├── WorkflowAction.cs # Transition logic
│ └── WorkflowInstance.cs
├── Services/
│ └── WorkflowService.cs # Core business logic
├── Properties/
│ └── launchSettings.json
├── Program.cs # API configuration
└── WorkflowEngine.csproj # .NET 9 project
```


## Getting Started

### Prerequisites
- Visual Studio 2022 v17.8+
- .NET 9 SDK

### Installation
1. Clone the repository:
   ```
   git clone https://github.com/your-username/workflow-engine.git
Open WorkflowEngine.sln in Visual Studio

Restore NuGet packages (automatic in VS2022)

Set WorkflowEngine as startup project

Running the API
Debug Mode: Press F5 to launch with Swagger UI

Production Mode:

```
dotnet run --configuration Release
```
API Documentation
Access interactive docs at https://localhost:5001/index.html

Sample Workflow Definition
```
POST /definitions
{
  "name": "OrderProcessing",
  "states": [
    {"id": "created", "isInitial": true, "isFinal": false},
    {"id": "approved", "isFinal": false},
    {"id": "rejected", "isFinal": true}
  ],
  "actions": [
    {
      "id": "approve",
      "fromStates": ["created"],
      "toState": "approved"
    }
  ]
}
```
State Transition Flow
```
stateDiagram-v2
    [*] --> Created
    Created --> Approved: approve
    Created --> Rejected: reject
    Approved --> [*]
    Rejected --> [*]
```
Testing in Visual Studio

Set breakpoints in WorkflowService.cs

Use Test Explorer (Ctrl+E,T) for unit tests

Debug API requests with:

REST Client (Add *.http file)

Postman integration

Troubleshooting Issue	Solution
---
Swagger not loading	Check port in launchSettings.json
---
CS8618 warnings	Ensure all required fields are initialized
---
HTTPS errors	``Run dotnet dev-certs https --trust``
---
Roadmap
Add Entity Framework Core persistence

Implement JWT authentication

Add SignalR real-time updates

Docker containerization

Contributing
Create feature branch (git checkout -b feature/xyz)

Commit changes (git commit -am 'Add feature xyz')

Push to branch (git push origin feature/xyz)

