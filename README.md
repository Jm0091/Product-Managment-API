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

## Caching Strategy

This project utilizes **IMemoryCache** to optimize performance. The following cache keys are used:

1. **ProductCacheKey**: Caches individual product details based on product ID.
2. **ProductsPagedCacheKey**: Caches paginated product lists for specific page numbers and page sizes.

Both **absolute** and **sliding expiration policies** are applied to ensure data freshness and cache efficiency. Absolute expiration defines how long the cache should store an item, while sliding expiration resets the timer whenever the item is accessed.

### Cache Invalidation

The cache is automatically invalidated in the following cases:

- **When a product is updated or deleted**, the corresponding cache key is removed to ensure data consistency.
- **Paged product lists** are also cleared when changes occur, ensuring that new data is fetched from the database.

---

## Future Improvements

1. **Redis Integration**: Planning to integrate Redis for distributed caching, which will allow for scaling across multiple instances in cloud environments or clustered setups.
   
2. **Cache Tagging**: Exploring cache tagging techniques to manage related cache entries in groups, enabling more efficient invalidation when multiple related keys need to be cleared.

3. **Improved Concurrency Handling**: Enhancing cache consistency with more advanced locking mechanisms to avoid race conditions during simultaneous cache access and modification.

4. **Enhanced Cache Policies**: Introducing more granular cache policies, such as prioritizing frequently accessed or critical data for longer cache durations.

---


## Setup and Installation

### Prerequisites

- .NET SDK 8.0
- SQL Server or any supported database
- IDE like Visual Studio or Rider

### Steps to Run the Project

1. **Clone the repository:**

    ```bash
    git clone https://github.com/your-username/caching-demo.git
    cd caching-demo
    ```

2. **Setup the database:**

    - Update the connection string in `appsettings.json` to point to your local SQL Server.
    - Apply migrations to set up the database:

      ```bash
      dotnet ef database update
      ```

3. **Run the application:**

    - From the project directory, use the following command to start the API:

      ```bash
      dotnet run
      ```

    - The API will be available at `https://localhost:5001` or `http://localhost:5000`.

4. **Testing the API:**

    - Use tools like Postman or Curl to interact with the API endpoints (e.g., `GET /api/products`, `POST /api/products`, etc.).

5. **Caching:**

    - Cached data can be retrieved and verified by making repeated GET requests to the API.
    - Update or delete operations will automatically invalidate the cache, ensuring fresh data.

