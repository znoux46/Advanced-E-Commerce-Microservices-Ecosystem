package com.ecommerce.order.model;

/**
 * Payment Status Enum
 * 
 * Represents the payment status of an order.
 */
public enum PaymentStatus {
    PENDING,        // Payment not yet processed
    COMPLETED,     // Payment successfully completed
    FAILED,       // Payment failed
    REFUNDED,     // Payment has been refunded
    PARTIAL_REFUND // Partial refund has been processed
}
