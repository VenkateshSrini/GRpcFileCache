# Distributed In-Memory Cache with File-Based Persistence

This project is a distributed in-memory cache with file-based persistence designed to work in a Kubernetes environment. It uses a Least Recently Used (LRU) caching strategy and includes a file cleanup service that deletes old files based on their time to live (TTL). The cache stays updated across multiple nodes using a folder watcher to notify of changes in the file system.

![Architecture Diagram](./architecture-diagram.png)

## Features

- **In-Memory Caching**: Fast access to data stored in memory.
- **File-Based Persistence**: Data is persisted in a file system, providing durability.
- **Distributed Caching**: Designed to work in a Kubernetes environment with multiple nodes writing to the same file system.
- **File System Notifications**: A folder watcher notifies the cache of changes in the file system, keeping the cache updated across multiple nodes.
- **LRU Cache**: Uses a Least Recently Used (LRU) caching strategy to manage memory.
- **TTL-Based Cleanup**: Includes a file cleanup service that deletes old files based on their TTL.

## Comparison with Other Caching Solutions

Compared to other well-known open-source caching solutions, this project offers unique features:

- **Redis and Memcached**: While both Redis and Memcached support in-memory caching and can be deployed in a Kubernetes environment, neither provide file system notifications like this project does.
- **EHCache**: EHCache is a standards-based cache for Java that offers in-memory caching, disk-based caching, and off-heap caching. However, it does not inherently support Kubernetes or provide file system notifications.
- **Hazelcast and Apache Ignite**: Both Hazelcast and Apache Ignite are memory-centric distributed databases that can be deployed in a Kubernetes environment, but they do not provide file system notifications.

## Getting Started

To get started with this project, clone the repository and deploy it to your Kubernetes environment.

git clone https://github.com/your-repo-url.git cd your-repo kubectl apply -f file-cache-grpc-service.yaml

For more information on how to use this project, see the [documentation](./docs).

## Contributing

Contributions are welcome! Please read our [contributing guidelines](./CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for 
