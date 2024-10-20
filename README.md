# Product Management API - .NET 8.0 with Caching

## Overview

Welcome to the Product Management API! 🚀 This project is a demonstration of building a high-performance, scalable, and maintainable .NET 8.0 application with **robust caching mechanisms** using `IMemoryCache`.

The API manages product data, integrates efficient paging and filtering, and optimizes data retrieval through in-memory caching. With clean architectural patterns, validations, and caching strategies, this application is designed to handle real-world performance needs.

---

## Key Features

- **In-Memory Caching**: Boosted performance by caching frequently accessed data, significantly reducing database load.
- **Efficient Cache Invalidation**: Implemented smart cache invalidation strategies to keep the cache in sync with the database updates.
- **Paging and Filtering Support**: Seamless pagination and filtering logic integrated with the cache to enhance performance for large datasets.
- **DTO & Validation**: Ensured robust data transfer and input validation using Data Transfer Objects (DTOs) and FluentValidation.
- **Repository Pattern**: Abstracted database access using the repository pattern, promoting clean separation of concerns.
- **.NET 8.0 Enhancements**: Leveraged the latest .NET 8.0 performance improvements for low-latency API responses.
- **Best Practices**: Followed SOLID principles and implemented dependency injection for maintainability and scalability.

---

## Technology Stack

- **Framework**: .NET 8.0
- **Caching**: IMemoryCache
- **Database**: SQL Server
- **Validation**: FluentValidation
- **Data Access**: Repository Pattern with Entity Framework Core
- **Testing**: xUnit for unit and integration testing

---

## Setup and Installation

### Prerequisites

- .NET SDK 8.0
- SQL Server or any supported database
- IDE like Visual Studio or Rider

### Steps to Run

1. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/product-management-api.git
