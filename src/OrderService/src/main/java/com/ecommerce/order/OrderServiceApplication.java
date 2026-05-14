package com.ecommerce.order;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.cache.annotation.EnableCaching;
import org.springframework.kafka.annotation.EnableKafka;

/**
 * Order Service Application (Spring Boot 3.2.0)
 * 
 * This is the main entry point for the Order Service in the E-Commerce Microservices Ecosystem.
 * It handles order management and shopping cart sessions using Redis for caching.
 * 
 * Main Features:
 * - Order CRUD operations with Spring Data JPA
 * - Shopping Cart session management using Redis with TTL
 * - Event-driven architecture with Apache Kafka
 * - JWT authentication with KeyCloak
 */
@SpringBootApplication
@EnableCaching
@EnableKafka
public class OrderServiceApplication {

    public static void main(String[] args) {
        SpringApplication.run(OrderServiceApplication.class, args);
    }
}
