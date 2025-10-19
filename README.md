# Clean Architecture Example (.NET 9, Modular Monolith)

A complete example project demonstrating **Clean Architecture** principles implemented as a **modular monolith** in C# (.NET 9).  
The goal of this repository is to provide a clear and educational foundation for building maintainable, scalable, and testable applications following clean software architecture patterns.

---

## Overview

This project illustrates how to structure a .NET application by separating business logic, infrastructure concerns, and user interface into distinct layers.  
It follows **Clean Architecture** and **SOLID** principles to ensure high maintainability, clear boundaries between layers, and easy testability.

The solution demonstrates how each layer interacts through well-defined contracts (interfaces), how dependencies flow only inward, and how implementation details (like databases or UI frameworks) can be swapped without touching the core domain logic.

The **modular monolith** approach used here keeps everything within one solution — organized by feature and responsibility — while maintaining boundaries that would later allow an easy evolution into a microservice-based system if desired.

---

## Architecture Layers

### **Domain Layer**
- Contains the *core business logic* and rules of the system.
- Includes **Entities**, **Value Objects**, **Aggregates**, and **Domain Events**.
- Has **no dependencies** on any external frameworks or technologies.
- Represents the *heart* of the application — pure C# classes with business meaning.

### **Application Layer**
- Defines **Use Cases** that orchestrate domain logic to fulfill specific operations.
- Depends **only on the Domain layer**.
- Uses **interfaces (ports)** to communicate with the outer layers (e.g., persistence, UI).
- Contains DTOs, service interfaces, and business workflows.

### **Infrastructure Layer**
- Implements the technical details (adapters) required by the Application layer.
- Includes **InMemory persistence** that can later be replaced with a real database (e.g., EF Core, Dapper, or SQL).
- Contains implementations for repositories, logging, and configuration.
- Fully replaceable without changing business logic.

### **Presentation Layer**
- Provides the **user interface**, currently implemented as a **WPF application**.
- Communicates with the Application layer through use case interfaces.
- Designed to be replaceable with other UI technologies like **Blazor** or **Web API**.

---

## Features

- Clean separation between **Domain**, **Application**, **Infrastructure**, and **Presentation** layers  
- **In-memory data persistence** for fast testing and demonstration  
- **WPF** UI demonstrating interaction with Application use cases  
- **xUnit** test suite for core logic and use case validation  
- Strict dependency flow — outer layers depend on inner layers only  
- Modular and extensible structure ready for domain expansion  

---

## Architecture Diagram

```

┌──────────────────────────────────────┐
│           Presentation (WPF)         │
│   → Calls Application Use Cases      │
└──────────────────┬───────────────────┘
│
┌──────────────────┴───────────────────┐
│           Application Layer          │
│  → Coordinates business operations   │
│  → Depends only on Domain Layer      │
└──────────────────┬───────────────────┘
│
┌──────────────────┴───────────────────┐
│            Domain Layer              │
│  → Core entities, value objects      │
│  → Contains pure business rules      │
└──────────────────┬───────────────────┘
│
┌──────────────────┴───────────────────┐
│         Infrastructure Layer         │
│  → Database, file, and external APIs │
│  → Adapters implementing interfaces  │
└──────────────────────────────────────┘

```

---

## Technologies

- **.NET 9 / C# 13**
- **WPF** (Presentation)
- **xUnit** (Testing)
- **InMemory Repositories** (Infrastructure)
- **Dependency Injection** for decoupling layers
- **Clean Architecture** & **SOLID** principles

---

## Purpose

This repository serves as a **reference implementation** for learning and experimenting with Clean Architecture in .NET.  
It demonstrates how to:
- Design a modular, decoupled architecture
- Structure code around **business capabilities**
- Separate core logic from framework dependencies
- Write testable and maintainable C# applications

Use this as a starting point for your own projects or as a template to explore advanced topics like DDD, CQRS, or Event-Driven architectures.

---