package com.ecommerce.order.model;

/**
 * Order Status Enum
 * 
 * Represents the various states an order can be in.
 */
public enum OrderStatus {
    PENDING,        // Order created, waiting for payment confirmation
    CONFIRMED,       // Order confirmed, payment received
    PROCESSING,     // Order is being processed
    SHIPPED,       // Order has been shipped
    DELIVERED,     // Order has been delivered
    CANCELLED,      // Order has been cancelled
    REFUNDED       // Order has been refunded
}
